using UnityEditor;
using System.IO;
[InitializeOnLoad]
public static class PracticeSupplementCleanup
{
 static PracticeSupplementCleanup()
 {
  EditorApplication.delayCall += () => {
   if(EditorApplication.isPlayingOrWillChangePlaymode) return;
   bool probe=AssetDatabase.DeleteAsset("Assets/Editor/PracticeIndependentProbe.cs");
   bool helper=AssetDatabase.DeleteAsset("Assets/Editor/PracticeSupplementCleanup.cs");
   AssetDatabase.SaveAssets();
   File.WriteAllText("Library/practice-supplement-cleanup.result", "probe="+probe+";helper="+helper);
  };
 }
}
