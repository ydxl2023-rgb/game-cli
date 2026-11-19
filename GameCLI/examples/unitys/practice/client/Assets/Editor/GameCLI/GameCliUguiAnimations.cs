#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// Implements the persisted design.animations contract; source metadata is never executed.
public static class GameCliUguiAnimations
{
    public static int Apply(JToken value, IDictionary<string, GameObject> objects, string folder, bool dryRun)
    {
        if (value == null) return 0;
        var animations = value as JArray;
        if (animations == null || animations.Count > 64) throw new Exception("animations must be an array of at most 64 items.");
        var ids = new HashSet<string>(); var hosts = new HashSet<string>();
        foreach (JObject animation in animations)
        {
            var id = (string)animation["id"];
            var hostId = (string)animation["hostNodeId"];
            GameObject host;
            if (string.IsNullOrWhiteSpace(id) || !ids.Add(id) || !hosts.Add(hostId) || !objects.TryGetValue(hostId, out host) || host.GetComponent<Animator>() == null)
                throw new Exception("Animation requires a unique ID and an animationHost with one default state.");
            var data = animation["clip"]; var state = animation["state"];
            if (data == null || state == null || (bool?)state["default"] != true) throw new Exception("Animation clip and default state required.");
            float start = Number(data,"startTime"), stop = Number(data,"stopTime"), speed = Number(state,"speed"), fps = Number(data,"frameRate");
            if (start != 0 || stop <= start || speed <= 0 || fps <= 0) throw new Exception("Expected zero-based clip time, positive duration/frameRate/speed.");
            float offset = Number(state,"cycleOffset");
            if (data["loopTime"]?.Type != JTokenType.Boolean || string.IsNullOrWhiteSpace((string)data["name"]) || string.IsNullOrWhiteSpace((string)state["name"])) throw new Exception("Invalid animation names or loopTime.");
            var curves = data["curves"] as JArray;
            if (curves == null || curves.Count == 0 || curves.Count > 128) throw new Exception("Expected 1..128 animation curves.");
            var clip = new AnimationClip { name = (string)data["name"], frameRate = fps, wrapMode = Wrap(data["wrapMode"]) };
            try
            {
                var bindings = new HashSet<string>();
                foreach (JObject curve in curves)
                {
                    GameObject target;
                    if (!objects.TryGetValue((string)curve["targetNodeId"], out target) || (target != host && !target.transform.IsChildOf(host.transform))) throw new Exception("Curve target must be under its animation host.");
                    string component = (string)curve["component"], property = (string)curve["property"];
                    bool euler = component == "UnityEngine.Transform" && new[] {"localEulerAnglesRaw.x","localEulerAnglesRaw.y","localEulerAnglesRaw.z"}.Contains(property);
                    bool scale = component == "UnityEngine.Transform" && new[] {"m_LocalScale.x","m_LocalScale.y","m_LocalScale.z"}.Contains(property);
                    bool uv = component == "UnityEngine.UI.RawImage" && property == "m_UVRect.x" && target.GetComponent<RawImage>() != null;
                    if (!euler && !scale && !uv) throw new Exception("Unsupported animation binding: " + component + "." + property);
                    var path = AnimationUtility.CalculateTransformPath(target.transform, host.transform);
                    if (path.Split('/').Where(p => p.Length > 0).Any(p => p.Contains(':'))) throw new Exception("Invalid animation target name.");
                    if (!bindings.Add(path + ":" + property)) throw new Exception("Duplicate animation binding.");
                    // A path must resolve to this exact node; duplicate sibling names are ambiguous.
                    if (path.Length > 0 && host.transform.Find(path) != target.transform) throw new Exception("Ambiguous animation hierarchy path: " + path);
                    var sourceKeys = curve["keys"] as JArray;
                    if (sourceKeys == null || sourceKeys.Count == 0 || sourceKeys.Count > 4096) throw new Exception("Invalid animation keys.");
                    var keys = new List<Keyframe>(); float previous = float.NegativeInfinity;
                    foreach (JObject key in sourceKeys)
                    {
                        float time = Number(key,"time"), v = Number(key,"value"), input = Number(key,"inTangent"), output = Number(key,"outTangent");
                        float iw = Number(key,"inWeight"), ow = Number(key,"outWeight");
                        if ((int?)key["weightedMode"] != 0 || time <= previous || time < start || time > stop) throw new Exception("Invalid key order/time or unsupported weighted curve.");
                        if (euler && !property.EndsWith(".z") && (v != 0 || input != 0 || output != 0)) throw new Exception("V1 supports zero X/Y Euler curves only.");
                        keys.Add(new Keyframe(time,v,input,output,iw,ow) { weightedMode = WeightedMode.None }); previous = time;
                    }
                    if (keys[0].time != start || previous != stop) throw new Exception("Curve must span the full clip interval.");
                    var result = new AnimationCurve(keys.ToArray()) { preWrapMode = Wrap(curve["preWrapMode"]), postWrapMode = Wrap(curve["postWrapMode"]) };
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path,(euler || scale) ? typeof(Transform) : typeof(RawImage),property),result);
                }
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.startTime = start; settings.stopTime = stop; settings.loopTime = (bool)data["loopTime"];
                AnimationUtility.SetAnimationClipSettings(clip,settings);
                Verify(clip,host,curves,objects,start,stop);
                if (dryRun) continue;
                Directory.CreateDirectory(folder);
                // Stable filenames based on the host ID; display names never become filesystem paths.
                using (var hash = System.Security.Cryptography.SHA256.Create())
                {
                    string key = BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hostId))).Replace("-", "").Substring(0,16);
                    string clipPath = folder + "/Animation_" + key + ".anim", controllerPath = folder + "/Animation_" + key + ".controller";
                    var saved = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (saved == null) { saved = UnityEngine.Object.Instantiate(clip); saved.name = clip.name; AssetDatabase.CreateAsset(saved,clipPath); }
                    else EditorUtility.CopySerialized(clip,saved);
                    saved.name = clip.name;
                    EditorUtility.SetDirty(saved);
                    var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) ?? AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                    if (controller.layers.Length != 1) throw new Exception("Generated controller must contain exactly one layer.");
                    var machine = controller.layers[0].stateMachine;
                    foreach (var old in machine.states) machine.RemoveState(old.state);
                    var created = machine.AddState((string)state["name"]); created.motion = saved; created.speed = speed; created.cycleOffset = offset;
                    created.writeDefaultValues = (bool?)state["writeDefaultValues"] ?? true; machine.defaultState = created;
                    EditorUtility.SetDirty(machine); EditorUtility.SetDirty(controller);
                    host.GetComponent<Animator>().runtimeAnimatorController = controller;
                    AssetDatabase.SaveAssets();
                    Verify(saved,host,curves,objects,start,stop);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(clip); }
        }
        return animations.Count;
    }

    static float Number(JToken token, string key)
    {
        var value = token[key];
        if (value == null || (value.Type != JTokenType.Float && value.Type != JTokenType.Integer)) throw new Exception("Missing numeric animation field: " + key);
        float number = (float)value;
        if (float.IsNaN(number) || float.IsInfinity(number)) throw new Exception("Invalid animation number.");
        return number;
    }
    static WrapMode Wrap(JToken token)
    {
        int mode = (int?)token ?? 0;
        if (!Enum.IsDefined(typeof(WrapMode),mode)) throw new Exception("Invalid wrap mode.");
        return (WrapMode)mode;
    }
    static void Verify(AnimationClip clip, GameObject host, JArray curves, IDictionary<string,GameObject> objects, float start, float stop)
    {
        var clone = UnityEngine.Object.Instantiate(host);
        try
        {
            foreach (float time in new[] {start, start+(stop-start)*.25f, start+(stop-start)*.5f, start+(stop-start)*.75f})
            {
                clip.SampleAnimation(clone,time);
                foreach (JObject source in curves)
                {
                    string path = AnimationUtility.CalculateTransformPath(objects[(string)source["targetNodeId"]].transform,host.transform);
                    var target = path.Length == 0 ? clone.transform : clone.transform.Find(path);
                    var keys = (JArray)source["keys"];
                    float expected = Sample(keys,time);
                    string property = (string)source["property"];
                    bool rotation = property.StartsWith("localEulerAnglesRaw.");
                    bool scale = property.StartsWith("m_LocalScale.");
                    var vector = scale ? target.localScale : target.localEulerAngles;
                    float actual = (rotation || scale) ? (property.EndsWith(".x") ? vector.x : property.EndsWith(".y") ? vector.y : vector.z) : target.GetComponent<RawImage>().uvRect.x;
                    float error = rotation ? Mathf.Abs(Mathf.DeltaAngle(expected,actual)) : Mathf.Abs(expected-actual);
                    if (error > .01f) throw new Exception("Animation sample mismatch: " + path + "/" + property + " at " + time + " expected " + expected + " actual " + actual);
                }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(clone); }
    }
    static float Sample(JArray keys, float time)
    {
        if (time <= (float)keys[0]["time"]) return (float)keys[0]["value"];
        for(int i=1;i<keys.Count;i++) if(time <= (float)keys[i]["time"])
        {
            var a=keys[i-1];var b=keys[i];float d=(float)b["time"]-(float)a["time"],t=(time-(float)a["time"])/d;
            return (2*t*t*t-3*t*t+1)*(float)a["value"]+(t*t*t-2*t*t+t)*d*(float)a["outTangent"]+(-2*t*t*t+3*t*t)*(float)b["value"]+(t*t*t-t*t)*d*(float)b["inTangent"];
        }
        return (float)keys[keys.Count-1]["value"];
    }
}
#endif
