using System;
using System.IO;
using System.Reflection;
using Oathx.Core;
using UnityEngine;
using UnityEngine.UI;
/// <summary>Temporary acceptance of the active serialized launcher.</summary>
public static class LaunchAcceptanceProbe
{
    public static void Run()
    {
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../runs/20261112-fullflow/acceptance/launch-probe.log"));
        File.WriteAllText(output,DateTime.UtcNow.ToString("O")+"\n");
        var launcher=(XLauncher)typeof(XApp).GetField("launcher",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(XApp.GetSingleton());
        Action<string,bool,string> record=(id,ok,detail)=>File.AppendAllText(output,id+" "+(ok?"passed":"failed")+" "+detail+"\n");
        Func<string,object> field=name=>typeof(XLauncher).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(launcher);
        var loading=(GameObject)field("loading");var update=(GameObject)field("update");
        var loadingText=(Text)field("loadingText");var downloadText=(Text)field("downloadText");var tip=(Text)field("tip");
        var fill=(RectTransform)field("fill");var handle=(RectTransform)field("handle");
        record("LCH-serialized-target",launcher.enabled&&launcher.name=="TemplateLaunch",launcher.name);
        launcher.ShowLoading("Acceptance loading");
        record("LCH-show-loading",loading.activeSelf&&!update.activeSelf,"loading="+loading.activeSelf+" update="+update.activeSelf);
        float[] values={0,.375f,1};int[] percents={0,38,100};
        for(int i=0;i<values.Length;i++)
        {
            launcher.UpdateProgress(values[i],"Acceptance loading");
            record("LCH-loading-progress",loadingText.text=="Acceptance loading ("+percents[i]+"%)",loadingText.text);
        }
        launcher.SetTip("Acceptance tip");record("LCH-tip",tip.text=="Acceptance tip",tip.text);
        launcher.ShowUpdate("Acceptance update");
        record("LCH-show-update",!loading.activeSelf&&update.activeSelf,"loading="+loading.activeSelf+" update="+update.activeSelf);
        launcher.UpdateProgress(.375f,"Acceptance update");
        record("LCH-download-text",downloadText.text=="Acceptance update 38%",downloadText.text);
        record("LCH-fill",Mathf.Approximately(fill.anchorMax.x,.375f),fill.anchorMax.ToString("F4"));
        record("LCH-handle",Mathf.Approximately(handle.anchorMin.x,.375f)&&Mathf.Approximately(handle.anchorMax.x,.375f),handle.anchorMin.ToString("F4")+" / "+handle.anchorMax.ToString("F4"));
        launcher.ShowBackground();
        record("LCH-background",launcher.gameObject.activeInHierarchy&&!loading.activeSelf&&!update.activeSelf,"root="+launcher.gameObject.activeInHierarchy+" loading="+loading.activeSelf+" update="+update.activeSelf);
        File.AppendAllText(output,"LAUNCH_PROBE_COMPLETED\n");
    }
}
