using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Oathx.Core;
using Oathx.Widget;

/// <summary>Editor-only binding of the frozen practice page hierarchy.</summary>
[InitializeOnLoad]
public static class PracticeBindLegacy
{
    static PracticeBindLegacy()
    {
        EditorApplication.update += () => {
            const string request = "Library/practice-bind.request";
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !System.IO.File.Exists(request)) return;
            System.IO.File.Delete(request);
            try { Build(); System.IO.File.WriteAllText("Library/practice-bind.result", "success"); }
            catch (Exception error) { System.IO.File.WriteAllText("Library/practice-bind.result", error.ToString()); Debug.LogException(error); }
        };
    }

    /// <summary>Bind four generated prefabs and replace the startup scene instance.</summary>
    public static void Build()
    {
        foreach (string name in new[] { "Launch", "Login", "Loading", "Notice" })
        {
            string path = "Assets/Data/Widgets/" + name + "/" + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (name == "Launch") BindLaunch(root);
                else BindWidget(root, name);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        EditorSceneManager.OpenScene("Assets/Scenes/App.unity");
        XApp app = UnityEngine.Object.FindObjectOfType<XApp>();
        if (!app) throw new InvalidOperationException("Scene XApp missing.");
        var data = new SerializedObject(app);
        var property = data.FindProperty("launcher");
        var old = property.objectReferenceValue as XLauncher;
        if (old) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Widgets/Launch/Launch.prefab");
        var launch = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var canvas = launch.GetComponent<Canvas>();
        if (!canvas) canvas = launch.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;
        var scaler = launch.GetComponent<CanvasScaler>();
        if (!scaler) scaler = launch.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080);
        scaler.matchWidthOrHeight = 0.5f;
        property.objectReferenceValue = launch.GetComponent<XLauncher>();
        data.ApplyModifiedPropertiesWithoutUndo();
        foreach (var layerScaler in UnityEngine.Object.FindObjectsOfType<CanvasScaler>(true))
        {
            layerScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            layerScaler.referenceResolution = new Vector2(1920, 1080);
            layerScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
        EditorSceneManager.SaveScene(app.gameObject.scene);
        foreach (var module in new[] { "Login", "Loading", "Notice" })
        {
            string folder = "Assets/Scripts/Systems/" + module;
            if (!AssetDatabase.IsValidFolder(folder + "/VM")) AssetDatabase.CreateFolder(folder, "VM");
            string oldPath = folder + "/UI/VM" + module + ".cs";
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(oldPath)) AssetDatabase.MoveAsset(oldPath, folder + "/VM/VM" + module + ".cs");
        }
        foreach (string component in new[] { "NoticeLayout", "NoticeText" })
        {
            string oldPath = "Assets/Scripts/Systems/Notice/Components/" + component + ".cs";
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(oldPath)) AssetDatabase.MoveAsset(oldPath, "Assets/Scripts/Systems/Notice/" + component + ".cs");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Practice four-page binding complete.");
    }

    private static Transform Node(GameObject root, string path)
    {
        var result = root.transform.Find(path);
        if (!result) throw new InvalidOperationException(root.name + "/" + path + " missing.");
        return result;
    }

    private static void Assign(Component component, string field, UnityEngine.Object value)
    {
        var data = new SerializedObject(component);
        var property = data.FindProperty(field);
        if (property == null) throw new InvalidOperationException(component.GetType().Name + "." + field + " missing.");
        property.objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BindLaunch(GameObject root)
    {
        var view = root.GetComponent<XLauncher>() ?? root.AddComponent<XLauncher>();
        Assign(view,"loading",Node(root,"Animator/Loading").gameObject);
        Assign(view,"update",Node(root,"Animator/Update").gameObject);
        Assign(view,"loadingText",Node(root,"Animator/Loading/DownloadTips").GetComponent<Text>());
        Assign(view,"tip",Node(root,"Animator/Loading/Tip").GetComponent<Text>());
        Assign(view,"downloadText",Node(root,"Animator/Update/TipsGather/DownloadTips").GetComponent<Text>());
        Assign(view,"fill",Node(root,"Animator/Update/Slider/FillArea/Fill"));
        Assign(view,"handle",Node(root,"Animator/Update/Slider/HandleSlideArea/Handle"));
    }

    private static void BindWidget(GameObject root, string name)
    {
        string panel="Animator/Panel", item=panel+"/ScrollView/Viewport/TextCells/Item";
        string body=item+"/ExpandContent/Content/Item/MainText";
        var paths=new Dictionary<string,string>();
        if(name=="Login") paths=new Dictionary<string,string>{
            {"announcementClick","Animator/ButtonGroup/LayoutGroup/Announcement"},{"enterClick","Animator/Content/Enter"},
            {"canEnter","Animator/Content/Enter"},{"serverName","Animator/Content/Server/Name"},
            {"serverStatus","Animator/Content/Server/Status/Status"},{"versionText","Animator/ButtonGroup/Version"},
            {"username","Animator/Account/Username"},{"friends","Animator/Content/Friends"},{"changeClick","Animator/Content/Server/Change"}};
        if(name=="Loading") paths=new Dictionary<string,string>{{"rate","Animator/RateBg/Rate"},{"tipText","Animator/RateBg/TipText"}};
        if(name=="Notice")
        {
            var old=Node(root,body).GetComponent<Text>();
            if(old.GetType()==typeof(Text))
            {
                string json=EditorJsonUtility.ToJson(old); var target=old.gameObject;
                UnityEngine.Object.DestroyImmediate(old);
                var replacement=target.AddComponent<NoticeText>();
                EditorJsonUtility.FromJsonOverwrite(json,replacement);
            }
            var empty=Node(root,panel).Find("Empty");
            if(!empty)
            {
                var obj=new GameObject("Empty",typeof(RectTransform),typeof(Text));
                obj.transform.SetParent(Node(root,panel),false); empty=obj.transform;
                var text=obj.GetComponent<Text>(); var source=Node(root,body).GetComponent<Text>();
                text.font=source.font; text.fontSize=source.fontSize; text.color=source.color; text.alignment=TextAnchor.MiddleCenter;
                var rect=obj.GetComponent<RectTransform>(); rect.anchorMin=new Vector2(.15f,.2f);rect.anchorMax=new Vector2(.85f,.7f);rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            }
            var scrollNode=Node(root,panel+"/ScrollView");
            var scroll=scrollNode.GetComponent<ScrollRect>()??scrollNode.gameObject.AddComponent<ScrollRect>();
            scroll.content=(RectTransform)Node(root,panel+"/ScrollView/Viewport/TextCells");
            scroll.viewport=(RectTransform)Node(root,panel+"/ScrollView/Viewport");scroll.horizontal=false;scroll.vertical=true;
            if(!scroll.viewport.GetComponent<RectMask2D>())scroll.viewport.gameObject.AddComponent<RectMask2D>();
            var layout=root.GetComponent<NoticeLayout>()??root.AddComponent<NoticeLayout>();
            Assign(layout,"body",Node(root,body).GetComponent<Text>());Assign(layout,"expanded",Node(root,item+"/ExpandContent"));
            Assign(layout,"entry",Node(root,item));Assign(layout,"entries",scroll.content);
            paths=new Dictionary<string,string>{{"closeClick",panel+"/TopBg/Close"},{"expandClick",item+"/NoticeTitleList/TitleGreen"},
                {"arrowClick",item+"/NoticeTitleList/TitleArrow"},{"weekClick",panel+"/WeekNomoreShow/ImgFrame"},
                {"weekAreaClick",panel+"/WeekNomoreShow/WeekNomoreBtn"},{"noticeTitleName",item+"/NoticeTitleList/NoticeTitleName"},
                {"body",body},{"entry",item},{"empty",panel+"/Empty"},{"expanded",item+"/ExpandContent"},
                {"selected",panel+"/WeekNomoreShow/ImgFrame/Selected"},{"emptyText",panel+"/Empty"}};
        }
        MonoScript script=null;
        foreach(string guid in AssetDatabase.FindAssets("VM"+name+" t:MonoScript"))
        {var candidate=AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));if(candidate.GetClass()?.Name=="VM"+name){script=candidate;break;}}
        if(!script)throw new InvalidOperationException("Missing VM"+name);
        var metadata=root.GetComponent<Metadata>()??root.AddComponent<Metadata>();
        metadata.className=script.GetClass().FullName;metadata.classGuid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(script));metadata.metadatas.Clear();
        foreach(var field in script.GetClass().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).OrderBy(f=>f.MetadataToken))
        {
            if(!field.GetCustomAttributes(false).Any(a=>a.GetType().Name=="Binding"||a.GetType().Name=="BindingAttribute"))continue;
            if(!paths.ContainsKey(field.Name))throw new InvalidOperationException("Unmapped "+field.Name);
            var node=Node(root,paths[field.Name]);string sync=field.Name=="canEnter"?"SyncInteractable":field.FieldType==typeof(string)?"SyncText":field.FieldType==typeof(bool)?"SyncVisible":"SyncClick";
            Component component=sync=="SyncText"?(Component)node.GetComponent<Text>():sync=="SyncVisible"?node:node.GetComponent<Button>();
            if(!component)throw new InvalidOperationException("Missing component for "+field.Name);
            metadata.metadatas.Add(new MetadataField{name=field.Name,type=sync,targets=new List<GameObject>{node.gameObject},components=new List<Component>{component}});
        }
        EditorUtility.SetDirty(metadata);
    }
}
