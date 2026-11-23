using UnityEditor;
using System.IO;
[InitializeOnLoad]
public static class PracticeFinalCleanup
{
 static PracticeFinalCleanup()
 {
  EditorApplication.delayCall += () => {
   if(EditorApplication.isPlayingOrWillChangePlaymode) return;
   string[] names={"PracticeBindLegacy","PracticeHomeEvidence","PracticeIndependentProbe","PracticeMaterialImport","PracticeNoticeInputRepair","PracticeSelfCheck","PracticeFinalCleanup"};
   var results=new System.Collections.Generic.List<string>();
   foreach(var name in names) {
    string path="Assets/Editor/"+name+".cs";
    results.Add(path+":"+AssetDatabase.DeleteAsset(path));
   }
   AssetDatabase.SaveAssets();
   File.WriteAllLines("Library/practice-final-cleanup.result",results);
  };
 }
}
