#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Export-only. Does not save prefabs, instantiate into user scenes, or change source assets.
[InitializeOnLoad]
public static class GameCliPrefabExporter
{
    [Serializable] public class Request { public string prefab; public string output; public int width; public int height; public List<TextOverride> texts; }
    /// <summary>Explicit localized preview text keyed by exported hierarchy path.</summary>
    [Serializable] public class TextOverride { public string path; public string text; }
    [Serializable] public class Export {
        public string schema = "gamecli.unity-prefab.v1";
        public string unityVersion, sourcePath, sourceGuid;
        public List<Node> nodes = new List<Node>();
        public List<Dependency> dependencies = new List<Dependency>();
    }
    [Serializable] public class Dependency { public string path, guid; }
    [Serializable] public class ComponentData { public string type, json; public List<Reference> references = new List<Reference>(); }
    [Serializable] public class Reference { public string property, guid, path, hierarchy; public long fileId; }
    [Serializable] public class RectData {
        public Vector2 anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, offsetMin, offsetMax;
        public Vector3 anchoredPosition3D, localPosition, localScale, localEulerAngles;
        public Quaternion localRotation;
        public Rect rect;
    }
    [Serializable] public class Node {
        public string path, parent, name, sourceGuid;
        public long sourceFileId;
        public int siblingIndex, layer;
        public bool activeSelf;
        public RectData rectTransform;
        public List<ComponentData> components = new List<ComponentData>();
    }
    static double next;
    static GameCliPrefabExporter() { EditorApplication.update += Poll; }
    static void Poll() {
        if (EditorApplication.timeSinceStartup < next || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        next = EditorApplication.timeSinceStartup + 2;
        string requestPath = Path.GetFullPath("Library/GameCLIExport/request.json");
        if (!File.Exists(requestPath)) return;
        string text = File.ReadAllText(requestPath);
        File.Move(requestPath, requestPath + ".processing");
        try { var request = JsonUtility.FromJson<Request>(text); Run(request); File.Delete(requestPath + ".processing"); }
        catch (Exception e) { File.WriteAllText(Path.Combine(Path.GetDirectoryName(requestPath), "error.txt"), e.ToString()); Debug.LogException(e); }
    }
    public static void Run(Request request) {
        if (string.IsNullOrEmpty(request.prefab) || !request.prefab.StartsWith("Assets/") || !request.prefab.EndsWith(".prefab")) throw new ArgumentException("Expected project prefab path");
        string output = Path.GetFullPath(request.output);
        Directory.CreateDirectory(output);
        var export = new Export { unityVersion = Application.unityVersion, sourcePath = request.prefab, sourceGuid = AssetDatabase.AssetPathToGUID(request.prefab) };
        // LoadPrefabContents resolves nested prefabs and variants into a separate preview scene.
        GameObject root = PrefabUtility.LoadPrefabContents(request.prefab);
        try {
            foreach (var text in root.GetComponentsInChildren<Text>(true)) {
                string path = Hierarchy(text.transform, root.transform);
                var replacement = request.texts == null ? null : request.texts.Find(item => item.path == path);
                if (replacement != null) text.text = replacement.text;
            }
            var rect = root.transform as RectTransform;
            if (rect != null && request.width > 0 && request.height > 0) {
                // Resolve layout in the disposable prefab preview, without saving the asset.
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, request.width);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, request.height);
                var groups = root.GetComponentsInChildren<LayoutGroup>(true);
                for (int pass = 0; pass < 3; pass++) {
                    for (int index = groups.Length - 1; index >= 0; index--)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(groups[index].transform as RectTransform);
                }
            }
            Visit(root.transform, "0", null, root.transform, export);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        foreach (string dependency in AssetDatabase.GetDependencies(request.prefab, true)) export.dependencies.Add(new Dependency { path = dependency, guid = AssetDatabase.AssetPathToGUID(dependency) });
        File.WriteAllText(Path.Combine(output, "unity-prefab.json"), JsonUtility.ToJson(export, true));
        File.WriteAllText(Path.Combine(output, "complete.json"), "{\"nodes\":" + export.nodes.Count + ",\"dependencies\":" + export.dependencies.Count + "}");
        Debug.Log("GameCLI exported " + request.prefab + " to " + output);
    }
    static string Hierarchy(Transform t, Transform root) {
        if (t == root) return "0";
        if (!t.IsChildOf(root)) return null;
        var parts = new List<string>();
        while (t != root) { parts.Insert(0, t.GetSiblingIndex().ToString()); t = t.parent; }
        return "0/" + string.Join("/", parts);
    }
    static void Visit(Transform t, string path, string parent, Transform root, Export export) {
        var n = new Node { path = path, parent = parent, name = t.name, activeSelf = t.gameObject.activeSelf, layer = t.gameObject.layer, siblingIndex = t.GetSiblingIndex() };
        var source = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
        if (source != null) AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out n.sourceGuid, out n.sourceFileId);
        var r = t as RectTransform;
        if (r != null) n.rectTransform = new RectData { anchorMin=r.anchorMin, anchorMax=r.anchorMax, pivot=r.pivot, anchoredPosition=r.anchoredPosition, anchoredPosition3D=r.anchoredPosition3D, sizeDelta=r.sizeDelta, offsetMin=r.offsetMin, offsetMax=r.offsetMax, localPosition=r.localPosition, localScale=r.localScale, localRotation=r.localRotation, localEulerAngles=r.localEulerAngles, rect=r.rect };
        foreach (var component in t.GetComponents<Component>()) {
            if (component == null) { n.components.Add(new ComponentData { type="MissingScript" }); continue; }
            var data = new ComponentData { type=component.GetType().AssemblyQualifiedName, json=EditorJsonUtility.ToJson(component,true) };
            using (var serialized = new SerializedObject(component)) {
                var property=serialized.GetIterator();
                while(property.Next(true)) {
                    if(property.propertyType!=SerializedPropertyType.ObjectReference || property.objectReferenceValue==null)continue;
                    var value=property.objectReferenceValue;
                    var reference=new Reference { property=property.propertyPath, path=AssetDatabase.GetAssetPath(value) };
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value,out reference.guid,out reference.fileId);
                    Transform target=value is GameObject ? ((GameObject)value).transform : value is Component ? ((Component)value).transform : null;
                    if(target!=null)reference.hierarchy=Hierarchy(target,root);
                    data.references.Add(reference);
                }
            }
            n.components.Add(data);
        }
        export.nodes.Add(n);
        for(int i=0;i<t.childCount;i++) Visit(t.GetChild(i),path+"/"+i,path,root,export);
    }
}
#endif
