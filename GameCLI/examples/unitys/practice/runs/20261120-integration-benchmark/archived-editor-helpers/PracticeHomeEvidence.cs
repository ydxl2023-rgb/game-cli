using UnityEditor;
using UnityEngine;
using Oathx.Widget;
using System.Collections.Generic;
using Newtonsoft.Json;
public static class PracticeHomeEvidence
{
 public static string Inspect()
 {
  var root=PrefabUtility.LoadPrefabContents("Assets/Data/Widgets/Home/Home.prefab");
  try {
   var rows=new List<object>();
   foreach(var metadata in root.GetComponentsInChildren<Metadata>(true)) {
    if(metadata.className != "VMProfile" && metadata.className != "VMProfileShortcuts") continue;
    foreach(var field in metadata.metadatas) {
     for(int i=0;i<field.components.Count;i++) {
      var c=field.components[i]; var t=field.targets[i];
      rows.Add(new { owner=metadata.name, vm=metadata.className, field=field.name, componentId=c?c.GetInstanceID():0,targetId=t?t.GetInstanceID():0,componentPath=c?AnimationUtility.CalculateTransformPath(c.transform,root.transform):null,targetPath=t?AnimationUtility.CalculateTransformPath(t.transform,root.transform):null,inside=c && c.transform.IsChildOf(root.transform),sameTarget=c && t && c.gameObject==t });
     }
    }
   }
   return JsonConvert.SerializeObject(rows);
  } finally { PrefabUtility.UnloadPrefabContents(root); }
 }
}
