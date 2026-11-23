using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Reconnect extracted native animation graphs to the converted mall hierarchy.</summary>
public static class ShopPageAnimations
{
    private const string folder = "Assets/Data/Widgets/Shop/Animations/Adapted";

    /// <summary>Restore the Animator at every retained source host, including native nested list items.</summary>
    public static string Restore(string directory, string name)
    {
        var plan = JObject.Parse(File.ReadAllText(Path.Combine(directory,name+"-plan.json")));
        string path = (string)plan["asset"];
        var root = PrefabUtility.LoadPrefabContents(path);
        var report = new JObject();
        var missing = new HashSet<string>();
        int bound = 0;
        try
        {
            var map = plan["nodes"].GroupBy(n=>(string)n["original"]).ToDictionary(g=>g.Key,g=>(string)g.First()["target"]);
            RestoreEffects(root.transform,plan["effects"]);
            foreach (var host in plan["hosts"])
            {
                string original = (string)host["original"];
                string target = (string)host["target"];
                var transform = target.Length==0 ? root.transform.Find("Animator") ?? root.transform : root.transform.Find(target);
                if (!transform) { missing.Add("host "+target); continue; }
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath((string)host["guid"]));
                if (!controller) throw new InvalidDataException("Missing controller for "+name+"/"+target);
                var animator = transform.GetComponent<Animator>();
                if (!animator) animator=transform.gameObject.AddComponent<Animator>();
                var replacements = new List<KeyValuePair<AnimationClip,AnimationClip>>();
                foreach (var clip in controller.animationClips.Distinct())
                {
                    var copy = UnityEngine.Object.Instantiate(clip);
                    bool changed=false;
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
                    {
                        string source = (original+"/"+binding.path).Trim('/');
                        Transform child=null;
                        if (binding.path.Length==0) child=transform;
                        else if (map.TryGetValue(source,out var dest)) child=root.transform.Find(dest);
                        var next=binding;
                        bool valid=child && (child==transform || child.IsChildOf(transform));
                        if (valid)
                        {
                            next.path=AnimationUtility.CalculateTransformPath(child,transform);
                            if (next.type==typeof(CanvasGroup) && !child.GetComponent<CanvasGroup>()) child.gameObject.AddComponent<CanvasGroup>();
                            if ((next.type==typeof(MonoBehaviour) || next.type==typeof(RawImage)) && next.propertyName.StartsWith("m_Color") && child.GetComponent<Graphic>())
                                next.type=child.GetComponent<Graphic>().GetType();
                            if (next.type==typeof(RawImage) && next.propertyName=="m_Texture" && !child.GetComponent<RawImage>())
                            {
                                var img=child.GetComponent<Image>();
                                var color=img ? img.color : Color.white;
                                var texture=img && img.sprite ? img.sprite.texture : null;
                                if(img) UnityEngine.Object.DestroyImmediate(img);
                                var raw=child.gameObject.AddComponent<RawImage>();
                                raw.color=color;
                                raw.texture=texture;
                                raw.raycastTarget=false;
                            }
                            valid=next.type!=null && (next.type==typeof(GameObject) || child.GetComponent(next.type));
                        }
                        if (!valid) missing.Add(clip.name+" / "+binding.path+" / "+binding.propertyName);
                        if (valid && next.path==binding.path && next.type==binding.type) continue;
                        changed=true;
                        if (binding.isPPtrCurve)
                        {
                            var keys=AnimationUtility.GetObjectReferenceCurve(clip,binding);
                            AnimationUtility.SetObjectReferenceCurve(copy,binding,null);
                            if(valid) AnimationUtility.SetObjectReferenceCurve(copy,next,keys);
                        }
                        else
                        {
                            var keys=AnimationUtility.GetEditorCurve(clip,binding);
                            AnimationUtility.SetEditorCurve(copy,binding,null);
                            if(valid) AnimationUtility.SetEditorCurve(copy,next,keys);
                        }
                    }
                    if (!changed) {UnityEngine.Object.DestroyImmediate(copy);continue;}
                    var clipPath=folder+"/"+name+"-"+Hash128.Compute(target+AssetDatabase.GetAssetPath(clip))+".anim";
                    copy.name=clip.name;
                    replacements.Add(new KeyValuePair<AnimationClip,AnimationClip>(clip,Store(copy,clipPath)));
                }
                if(replacements.Count==0) animator.runtimeAnimatorController=controller;
                else
                {
                    var adapted=new AnimatorOverrideController(controller);
                    adapted.ApplyOverrides(replacements);
                    animator.runtimeAnimatorController=Store(adapted,folder+"/"+name+"-"+Hash128.Compute(target)+".overrideController");
                }
                bound++;
            }

            if (name=="NewPeopleGiftView")
            {
                var viewport=root.transform.Find("Animator/Center/GiftScrollView/MaskViewport");
                var list=viewport.Find("Content");
                var previous=list.Find("Entry");
                if (previous) UnityEngine.Object.DestroyImmediate(previous.gameObject);
                var entry=UnityEngine.Object.Instantiate(viewport.Find("ScrollCell"),list,false);
                entry.name="Entry";
                ((RectTransform)entry).anchoredPosition=Vector2.zero;
                entry.gameObject.SetActive(true);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);
            AssetDatabase.SaveAssets();
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        report["hosts"]=bound;
        report["skipped"]=new JArray(missing.OrderBy(v=>v));
        File.WriteAllText(Path.Combine(directory,name+"-result.json"),report.ToString());

        return name+" "+bound+" hosts, "+missing.Count+" unavailable bindings";
    }

    /// <summary>Exercise each native default state on a disposable play-mode page instance.</summary>
    public static string Verify(string path)
    {
        var instance=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        int controllers=0,playing=0;
        try
        {
            instance.SetActive(true);
            foreach(var animator in instance.GetComponentsInChildren<Animator>(true))
            {
                if(!animator.runtimeAnimatorController) throw new InvalidDataException("Unbound animator on "+animator.name);
                controllers++;
                if(!animator.isActiveAndEnabled) continue;
                animator.Rebind();
                animator.Update(0);
                var before=animator.GetCurrentAnimatorStateInfo(0);
                animator.Update(.12f);
                var after=animator.GetCurrentAnimatorStateInfo(0);
                if(after.fullPathHash!=0 && (after.normalizedTime>before.normalizedTime || after.fullPathHash!=before.fullPathHash)) playing++;
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }

        return controllers+","+playing;
    }

    /// <summary>Restore source render components and resolve their serialized external references.</summary>
    private static void RestoreEffects(Transform root, JToken effects)
    {
        foreach (var spec in effects)
        {
            string path=(string)spec["path"];
            if (root.Find(path)) continue;
            int slash=path.LastIndexOf('/');
            var parent=slash<0 ? root : root.Find(path.Substring(0,slash));
            if (!parent) throw new InvalidDataException("Missing effect parent "+path);
            bool rect=spec["components"].Any(c=>(string)c["type"]=="UnityEngine.RectTransform");
            var obj=rect ? new GameObject(path.Substring(slash+1),typeof(RectTransform)) : new GameObject(path.Substring(slash+1));
            obj.SetActive(false);
            obj.layer=parent.gameObject.layer;
            obj.transform.SetParent(parent,false);
            foreach (var data in spec["components"])
            {
                var type=typeof(Transform).Assembly.GetType((string)data["type"]) ?? typeof(ParticleSystem).Assembly.GetType((string)data["type"]);
                if(type==null) throw new InvalidDataException((string)data["type"]);
                var component=obj.GetComponent(type);
                if (!component) component=obj.AddComponent(type);
                EditorJsonUtility.FromJsonOverwrite(data["values"].ToString(),component);
                var serialized=new SerializedObject(component);
                foreach(var reference in data["refs"])
                {
                    var prop=serialized.FindProperty((string)reference["property"]);
                    if(prop==null || prop.propertyType!=SerializedPropertyType.ObjectReference) continue;
                    var asset=AssetDatabase.GUIDToAssetPath((string)reference["guid"]);
                    var matched=AssetDatabase.LoadAllAssetsAtPath(asset).FirstOrDefault(x=>x && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(x,out string _,out long id) && id==(long)reference["fileId"]);
                    if(matched) prop.objectReferenceValue=matched;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            obj.SetActive((bool)spec["active"]);
        }
    }

    /// <summary>Preserve asset GUIDs on repeated production runs.</summary>
    private static T Store<T>(T value,string path) where T : UnityEngine.Object
    {
        var prior=AssetDatabase.LoadAssetAtPath<T>(path);
        if (!prior) {AssetDatabase.CreateAsset(value,path);return value;}
        EditorUtility.CopySerialized(value,prior);
        UnityEngine.Object.DestroyImmediate(value);
        EditorUtility.SetDirty(prior);

        return prior;
    }
}






