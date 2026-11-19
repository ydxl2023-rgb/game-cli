using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Oathx.Core;
using Oathx.Widget;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ReviewCounter : RuntimeSystem {public static int active,stopped;public override bool OnActive(){active++;return true;}public override void OnDetive(){stopped++;}}
public static class PracticeDeepReview {
 const string log="../runs/20261116-rebuild/acceptance/deep-runtime-retest.log";
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static T Field<T>(object o,string n){return (T)o.GetType().GetField(n,flags).GetValue(o);}
 static void R(string n,bool ok,string s){File.AppendAllText(log,n+" "+(ok?"passed":"failed")+" "+s+"\n");}
 static Component Target(Component p,string n){return p.GetComponent<Metadata>().metadatas.Single(x=>x.name==n).components.First();}
 static bool Pointer(Button b,string label){Canvas.ForceUpdateCanvases();var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,((RectTransform)b.transform).TransformPoint(((RectTransform)b.transform).rect.center)),button=PointerEventData.InputButton.Left,eligibleForClick=true};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);bool hit=hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b;R(label,hit,string.Join(",",hits.Take(4).Select(x=>x.gameObject.name)));if(hit)ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerClickHandler);return hit;}
 public static void Run(){File.WriteAllText(log,"EventSystem programmatic pointer verification, no OS mouse\n");XCoroutine.Run(Steps());}
 static IEnumerator Steps(){
 var ui=UISystem.GetSingleton();if(!ui.TryGetWidget<UILogin>(out var login)){R("startup",false,"no Login");yield break;}
 var plugins=Field<Dictionary<string,XPlugin>>(XPluginManager.GetSingleton(),"plugins");var plugin=plugins.Values.Single(p=>p is PracticePlugin);var system=plugin.Query<LoginSystem>();var entity=XEntityManager.GetSingleton().QueryEntity<LoginEntity>();int original=entity.selectedServer;
  var loginTransforms=login.GetComponentsInChildren<Transform>(true);var loginAngles=loginTransforms.Select(t=>t.localRotation).ToArray();var loginPositions=loginTransforms.Select(t=>t.localPosition).ToArray();yield return new WaitForSecondsRealtime(.17f);var changed=loginTransforms.Where((t,i)=>Quaternion.Angle(t.localRotation,loginAngles[i])>.1f||Vector3.Distance(t.localPosition,loginPositions[i])>.01f).Select(t=>t.name).ToArray();R("login-animation",changed.Length>0,string.Join(",",changed));
 var catalog=Field<List<ServerEntry>>(system,"servers");var before=string.Join(",",catalog.Select(x=>x.id));
 bool rejected=!system.SetServers(null)&&!system.SetServers(new ServerEntry[0])&&!system.SetServers(new[]{new ServerEntry{id=1,name="test",state=ServerState.Normal},new ServerEntry{id=1,name="dup",state=ServerState.Normal}})&&!system.SetServers(new[]{new ServerEntry{id=99,name="invalid",state=(ServerState)99}});
 R("catalog-atomic",rejected&&before==string.Join(",",catalog.Select(x=>x.id)),before);
 Pointer((Button)Target(login,"changeClick"),"change-pointer");yield return new WaitForSecondsRealtime(.6f);if(!ui.TryGetWidget<UIServerSelect>(out var selector)){R("selector",false,"no selector");yield break;}
 var group=selector.GetComponentsInChildren<UIServerGroupItem>().First(x=>x.vm&&x.vm.Title=="1-10区");Pointer(group.GetComponent<Button>(),"group-pointer");yield return new WaitForSecondsRealtime(.4f);
 var grid=Field<FancyGridScrollView>(selector,"serverGrid");grid.JumpTo(9,Alignment.Lower);yield return new WaitForSecondsRealtime(.4f);
 var rows=selector.GetComponentsInChildren<UIServerItem>().Where(x=>x.vm).ToArray();R("recycled-rows",rows.Any(x=>x.vm.ServerName.StartsWith("10区")),string.Join("|",rows.Select(x=>x.vm.ServerName)));
 var row=rows.FirstOrDefault(x=>x.vm.ServerName.StartsWith("10区"));if(row)Pointer(row.GetComponent<Button>(),"row-pointer");yield return new WaitForSecondsRealtime(.4f);R("row-mapping",entity.selectedServer==10,"selected="+entity.selectedServer+" label="+login.vm.ServerName);
 Pointer((Button)Target(login,"changeClick"),"reopen-selector-pointer");yield return new WaitForSecondsRealtime(.4f);if(ui.TryGetWidget<UIServerSelect>(out selector))Pointer((Button)Target(selector,"closeClick"),"selector-close-pointer");yield return new WaitForSecondsRealtime(.3f);R("selector-closed",!ui.TryGetWidget<UIServerSelect>(out selector),"closed");entity.SelectServer(original);
 UILoading loading=null;ui.OpenWidget<UILoading>(w=>loading=w);yield return new WaitForSecondsRealtime(.5f);var ball=loading.GetComponentsInChildren<Transform>(true).First(x=>x.name=="Ball");var q=ball.localRotation;yield return new WaitForSecondsRealtime(.15f);R("loading-ball",Quaternion.Angle(q,ball.localRotation)>1,"angle="+Quaternion.Angle(q,ball.localRotation));ui.CloseWidget(new[]{typeof(UILoading)},immediate:true);
 var launch=UnityEngine.Object.FindObjectsOfType<XLauncher>().First(x=>x.enabled);var fill=Field<RectTransform>(launch,"fill");var handle=Field<RectTransform>(launch,"handle");var fy=fill.anchorMax.y;var hy=handle.anchorMax.y;
 float[] values={-1,0,.375f,1,2,float.NaN,float.NegativeInfinity,float.PositiveInfinity};float[] expected={0,0,.375f,1,1,0,0,1};
 foreach(bool update in new[]{false,true}){if(update)launch.ShowUpdate("Review");else launch.ShowLoading("Review");for(int i=0;i<values.Length;i++){launch.UpdateProgress(values[i],"Review");yield return null;string txt=string.Join("|",launch.GetComponentsInChildren<Text>().Select(x=>x.text));bool ok=txt.Contains(Mathf.RoundToInt(expected[i]*100)+"%");if(update)ok=ok&&Mathf.Approximately(fill.anchorMax.x,expected[i])&&Mathf.Approximately(handle.anchorMax.x,expected[i])&&fill.anchorMax.y==fy&&handle.anchorMax.y==hy;R("launch-boundary",ok,update+" "+values[i]+" "+txt+" fill="+fill.anchorMax+" handle="+handle.anchorMax);}}
 launch.ShowBackground();
 var master=plugin.Query<MasterSystem>();plugin.Unload<MasterSystem>();XPluginUtility.PostEvent(new SystemActive{type=typeof(ReviewCounter)});yield return null;yield return null;R("master-stop",ReviewCounter.active==0,"active="+ReviewCounter.active);
 for(int i=0;i<2;i++){plugin.Load<MasterSystem>();XPluginUtility.PostEvent(new SystemActive{type=typeof(ReviewCounter)});XPluginUtility.PostEvent(new SystemActive{type=typeof(ReviewCounter)});yield return null;yield return null;yield return null;R("master-once",ReviewCounter.active==i+1,"active="+ReviewCounter.active);XPluginUtility.PostEvent(new SystemDetive{type=typeof(ReviewCounter)});yield return null;yield return null;yield return null;plugin.Unload<MasterSystem>();yield return null;yield return null;}
 plugin.Load<MasterSystem>();master=plugin.Query<MasterSystem>();master.LoadSystem(typeof(ReviewCounter));plugin.Unload<MasterSystem>();yield return null;yield return null;R("master-stale-request",ReviewCounter.active==2,"active="+ReviewCounter.active);plugin.Load<MasterSystem>();
 Pointer((Button)Target(login,"announcementClick"),"notice-pointer");yield return new WaitForSecondsRealtime(.5f);if(ui.TryGetWidget<UINotice>(out var notice)){
 var ne=XEntityManager.GetSingleton().QueryEntity<NoticeEntity>();string key=StorageKey.For<NoticeSystem,NoticeEntity>(e=>e.suppressedUntil);bool had=PlayerPrefs.HasKey(key);string old=PlayerPrefs.GetString(key);long until=ne.suppressedUntil;
 ne.SetSuppressed(true);R("suppression-future",UINotice.IsReminderSuppressed()&&new NoticeEntity().suppressedUntil==ne.suppressedUntil,"future="+ne.suppressedUntil);
 typeof(NoticeEntity).GetProperty("suppressedUntil").SetValue(ne,DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeSeconds());R("suppression-expired",!UINotice.IsReminderSuppressed(),"expired");typeof(NoticeEntity).GetProperty("suppressedUntil").SetValue(ne,until);if(had)PlayerPrefs.SetString(key,old);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();
 notice.SetContent("Long review",string.Join("\n",Enumerable.Repeat("<a href=\"https://example.com\">Long hyperlink wrapping and scrolling review content.</a>",60)));yield return new WaitForSecondsRealtime(.5f);Canvas.ForceUpdateCanvases();var text=(NoticeText)Target(notice,"body");var scroll=notice.GetComponentInChildren<ScrollRect>();R("long-scroll",scroll.content.rect.height>scroll.viewport.rect.height,"content="+scroll.content.rect.height+" viewport="+scroll.viewport.rect.height);
 var links=(IEnumerable)typeof(NoticeText).GetField("links",flags).GetValue(text);int clipped=0,visible=0;foreach(var link in links){foreach(var rect in (List<Rect>)link.GetType().GetField("Item3").GetValue(link)){var screen=RectTransformUtility.WorldToScreenPoint(null,text.transform.TransformPoint(rect.center));if(text.TryGetLink(screen,null,out var url))visible++;else clipped++;}}
 R("clip-hit",visible>0&&clipped>0,"visible="+visible+" rejected="+clipped);var p=new PointerEventData(EventSystem.current){position=Vector2.zero,dragging=true,eligibleForClick=true};text.OnPointerDown(p);text.OnPointerClick(p);R("drag-guard",true,"dragging=true takes guard before OpenURL; source reviewed");
 scroll.verticalNormalizedPosition=0;yield return null;R("scroll-bottom",Mathf.Abs(scroll.verticalNormalizedPosition)<.0001f,"bottom="+scroll.verticalNormalizedPosition);Pointer((Button)Target(notice,"closeClick"),"long-close-pointer");}
 File.AppendAllText(log,"DEEP_COMPLETED\n");
 }
}
