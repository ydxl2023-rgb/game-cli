using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
[InitializeOnLoad]
public static class PracticeNoticeInputRepair
{
 static PracticeNoticeInputRepair()
 {
  EditorApplication.update += () => {
   const string request="Library/practice-notice-input.request";
   if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !System.IO.File.Exists(request)) return;
   System.IO.File.Delete(request);
   const string path="Assets/Data/Widgets/Notice/Notice.prefab";
   var root=PrefabUtility.LoadPrefabContents(path);
   try {
    var viewport=root.transform.Find("Animator/Panel/ScrollView/Viewport");
    var graphic=viewport.GetComponent<Image>();
    if(!graphic) graphic=viewport.gameObject.AddComponent<Image>();
    graphic.color=Color.clear;
    graphic.raycastTarget=true;
    foreach(var text in root.GetComponentsInChildren<NoticeText>(true)) text.raycastTarget=true;
    PrefabUtility.SaveAsPrefabAsset(root,path);
    System.IO.File.WriteAllText("Library/practice-notice-input.result","success");
   } finally { PrefabUtility.UnloadPrefabContents(root); }
  };
 }
}
