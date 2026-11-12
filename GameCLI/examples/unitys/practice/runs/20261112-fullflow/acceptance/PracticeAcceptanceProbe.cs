using System;
using System.Collections;
using System.IO;
using System.Linq;
using Oathx.Core;
using Oathx.Widget;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Temporary independent acceptance driver; never saves product assets.</summary>
public static class PracticeAcceptanceProbe
{
    private static string evidence;

    /// <summary>Run inside the authorized isolated Play session.</summary>
    public static void Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Play required");
        evidence = Path.GetFullPath(Path.Combine(Application.dataPath, "../../runs/20261112-fullflow/acceptance/runtime-probe.log"));
        File.WriteAllText(evidence, DateTime.UtcNow.ToString("O") + "\n");
        XCoroutine.Run(Exercise());
    }

    private static void Record(string id, bool passed, string actual)
    {
        File.AppendAllText(evidence, DateTime.UtcNow.ToString("O") + " " + id + " " + (passed ? "passed" : "failed") + " " + actual + "\n");
    }

    private static Component Target(Component page, string field)
    {
        return page.GetComponent<Metadata>().metadatas.Single(x => x.name == field).components.First();
    }

    private static void Click(Component page, string field)
    {
        ((Button)Target(page, field)).onClick.Invoke();
    }

    private static IEnumerator Exercise()
    {
        var ui = UISystem.GetSingleton();
        bool found = ui.TryGetWidget<UILogin>(out var login);
        Record("LGN-start", found, "Login loaded");
        if (!found) yield break;
        login.vm.ServerName = "Acceptance local label";
        yield return null;
        Record("LGN-vm", ((Text)Target(login,"serverName")).text == "Acceptance local label", ((Text)Target(login,"serverName")).text);
        Click(login,"enterClick");
        yield return null;
        yield return null;
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        Record("MST-enter", master.pid == 1, "pid=" + master.pid);
        XPluginUtility.PostEvent(new LoginSucceed { pid = -1 });
        yield return null;
        Record("MST-invalid-pid", master.pid == 1, "pid=" + master.pid);
        XPluginUtility.PostEvent(new SystemActive { type = typeof(string) });
        XPluginUtility.PostEvent(new SystemActive { type = null });
        XPluginUtility.PostEvent(new SystemActive { type = typeof(MasterSystem) });
        yield return null;
        Record("MST-invalid-types", ui.TryGetWidget<UILogin>(out login), "Login remains open");
        for (int cycle = 0; cycle < 3; cycle++)
        {
            Click(login,"announcementClick");
            yield return new WaitForSecondsRealtime(1f);
            bool opened = ui.TryGetWidget<UINotice>(out var notice);
            Record("NTC-open-" + cycle, opened, "via Login button and Master");
            if (!opened) yield break;
            notice.SetContent("Acceptance", "Test announcement body");
            yield return null;
            Record("NTC-content-" + cycle, ((Text)Target(notice,"body")).text == "Test announcement body", ((Text)Target(notice,"body")).text);
            Click(notice,"expandClick");
            yield return null;
            Record("NTC-collapse-" + cycle, !notice.vm.Expanded, "expanded=" + notice.vm.Expanded);
            notice.SetContent(null," ");
            yield return null;
            Record("NTC-empty-" + cycle, notice.vm.Empty && !notice.vm.Entry, "empty=" + notice.vm.Empty);
            Click(notice,"closeClick");
            yield return new WaitForSecondsRealtime(.5f);
            Record("NTC-close-" + cycle, !ui.TryGetWidget<UINotice>(out var closed), "via close Button and Master");
        }
        foreach (string value in new [] { "https://example.com", "http://example.org", "javascript:alert(1)", "file:///tmp/a", "/relative", "" })
        {
            bool ok = NoticeText.TryNormalizeUrl(value,out var url);
            Record("NTC-url", ok == value.StartsWith("http",StringComparison.Ordinal), value + " -> " + ok);
        }
        UILoading loading = null;
        ui.OpenWidget<UILoading>(widget => { loading = widget; });
        yield return new WaitForSecondsRealtime(1f);
        Record("LDG-open", loading != null,"explicit callback");
        if (!loading) yield break;
        float[] values = { -1f, 0f, .375f, 1f, 2f, float.NaN, float.NegativeInfinity, float.PositiveInfinity };
        int[] expected = { 0,0,38,100,100,0,0,100 };
        for (int index = 0; index < values.Length; index++)
        {
            loading.UpdateProgress(values[index], "Probe");
            yield return null;
            string actual = ((Text)Target(loading,"rate")).text;
            Record("LDG-progress", actual == "Probe(" + expected[index] + "%)", values[index] + " -> " + actual);
        }
        loading.SetTip(null);
        yield return null;
        Record("LDG-null-tip", ((Text)Target(loading,"tipText")).text == "", "cleared");
        Type[] widgets = { typeof(UILoading) };
        ui.CloseWidget(widgets, immediate:true);
        yield return null;
        Record("LDG-close", !ui.TryGetWidget<UILoading>(out var afterLoading), "closed");
        var launcher = UnityEngine.Object.FindObjectOfType<XLauncher>();
        Record("LCH-background", launcher && launcher.gameObject.activeInHierarchy, "Launch retained behind Login");
        File.AppendAllText(evidence,"PROBE_COMPLETED\n");
    }
    public static void Visual()
    {
        evidence = Path.GetFullPath(Path.Combine(Application.dataPath, "../../runs/20261112-fullflow/acceptance/visual-probe.log"));
        File.WriteAllText(evidence, "Visual probe\n");
        XCoroutine.Run(VisualSteps());
    }

    private static IEnumerator VisualSteps()
    {
        var ui = UISystem.GetSingleton();
        ui.TryGetWidget<UILogin>(out var login);
        var button = (Button)Target(login,"announcementClick");
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        var pointer = new UnityEngine.EventSystems.PointerEventData(eventSystem);
        pointer.position = RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        eventSystem.RaycastAll(pointer,hits);
        bool hit = hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button;
        Record("LGN-announcement-raycast",hit,string.Join(",",hits.Select(x => x.gameObject.name)));
        if (hit) UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        else Click(login,"announcementClick");
        for(int frame=0;frame<45;frame++)
        {
            yield return null;
            if(ui.TryGetWidget<UINotice>(out var notice))
            {
                string scales = string.Join(";",notice.GetComponentsInChildren<RectTransform>(true).Where(x=>x.GetComponent<Animator>()||x.name=="Mask"||x.name=="Content"||x.name=="Panel").Select(x=>x.name+"="+x.localScale.ToString("F4")));
                File.AppendAllText(evidence,"frame="+frame+" "+scales+"\n");
            }
        }
        if(ui.TryGetWidget<UINotice>(out var page))
        {
            var text=(NoticeText)Target(page,"body");
            Canvas.ForceUpdateCanvases();
            var links = (System.Collections.IEnumerable)typeof(NoticeText).GetField("links",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(text);
            int count=0;
            foreach(var link in links)
            {
                var bounds=(System.Collections.Generic.List<Rect>)link.GetType().GetField("Item3").GetValue(link);
                foreach(var rect in bounds)
                {
                    var screen=RectTransformUtility.WorldToScreenPoint(null,text.transform.TransformPoint(rect.center));
                    if(text.TryGetLink(screen,null,out var url)) { count++; break; }
                }
            }
            Record("NTC-link-visible-glyph",count>0,"links hit="+count+"; no browser opened");
            button=(Button)Target(page,"closeClick");
            pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
            hits.Clear(); eventSystem.RaycastAll(pointer,hits);
            Record("NTC-close-raycast",hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,string.Join(",",hits.Select(x=>x.gameObject.name)));
        }
        File.AppendAllText(evidence,"VISUAL_PROBE_COMPLETED\n");
    }    public static void Reopen()
    {
        evidence=Path.GetFullPath(Path.Combine(Application.dataPath,"../../runs/20261112-fullflow/acceptance/reopen-probe.log"));
        File.WriteAllText(evidence,"Reopen probe\n"); XCoroutine.Run(ReopenSteps());
    }
    private static IEnumerator ReopenSteps()
    {
        var ui=UISystem.GetSingleton();
        XPluginUtility.PostEvent(new SystemDetive { type=typeof(LoginSystem) });
        yield return new WaitForSecondsRealtime(.5f);
        Record("LGN-detive",!ui.TryGetWidget<UILogin>(out var old),"closed");
        XPluginUtility.PostEvent(new SystemActive { type=typeof(LoginSystem) });
        yield return new WaitForSecondsRealtime(1f);
        bool open=ui.TryGetWidget<UILogin>(out var login); Record("LGN-reopen",open,"loaded");
        if(!open) yield break;
        var entity=XEntityManager.GetSingleton().QueryEntity<MasterEntity>(); entity.pid=42;
        Click(login,"enterClick"); yield return null; yield return null;
        Record("LGN-reopen-enter",entity.pid==1,"pid="+entity.pid);
        Click(login,"announcementClick"); yield return new WaitForSecondsRealtime(.5f);
        Record("LGN-reopen-notice",ui.TryGetWidget<UINotice>(out var notice),"opened");
        if(notice) Click(notice,"closeClick");
        for(int cycle=0;cycle<2;cycle++)
        {
            UILoading loading=null;ui.OpenWidget<UILoading>(widget=>{loading=widget;});
            yield return new WaitForSecondsRealtime(.5f);
            Record("LDG-reopen-"+cycle,loading!=null,"ready");
            if(loading){loading.UpdateProgress(.5f,"Reopen");yield return null;Record("LDG-reopen-text-"+cycle,((Text)Target(loading,"rate")).text=="Reopen(50%)",((Text)Target(loading,"rate")).text);}
            Type[] widgets={typeof(UILoading)};ui.CloseWidget(widgets,immediate:true);yield return null;
            Record("LDG-reclose-"+cycle,!ui.TryGetWidget<UILoading>(out var closed),"closed");
        }
        File.AppendAllText(evidence,"REOPEN_COMPLETED\n");
    }}



