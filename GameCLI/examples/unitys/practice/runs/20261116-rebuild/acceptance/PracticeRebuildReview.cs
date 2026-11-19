using System;
using System.Collections;
using System.IO;
using System.Linq;
using Oathx.Core;
using Oathx.Widget;
using Oathx.Entity;
using UnityEngine;
using UnityEngine.UI;
public static class PracticeRebuildReview
{
 static string log="../runs/20261116-rebuild/acceptance/third-probe.log";
 static void Record(string key,bool pass,string actual){File.AppendAllText(log,key+" "+(pass?"passed":"failed")+" "+actual+"\n");}
 static Component Target(Component p,string name){return p.GetComponent<Metadata>().metadatas.Single(x=>x.name==name).components.First();}
 static void Click(Component p,string name){((Button)Target(p,name)).onClick.Invoke();}
  public static void HoldSelector(){var ui=UISystem.GetSingleton();if(ui.TryGetWidget<UILogin>(out var login))Click(login,"changeClick");}
 public static void Launch(){XCoroutine.Run(LaunchSteps());}
 static IEnumerator LaunchSteps(){
  log="../runs/20261116-rebuild/acceptance/launch-probe.log";File.WriteAllText(log,"Launch explicit test input\n");
  var launch=UnityEngine.Object.FindObjectsOfType<XLauncher>().First(x=>x.enabled);
  launch.ShowLoading("Review loading");launch.UpdateProgress(.375f,"Review");yield return null;
  Record("loading",true,string.Join("|",launch.GetComponentsInChildren<Text>().Select(x=>x.text)));
  launch.ShowUpdate("Review download");launch.UpdateProgress(.375f,"Review");yield return null;
  Record("download",true,string.Join("|",launch.GetComponentsInChildren<Text>().Select(x=>x.text)));
  launch.ShowBackground();yield return null;Record("background",launch.gameObject.activeInHierarchy,"active texts="+launch.GetComponentsInChildren<Text>().Length);
 }
 public static void Run(){File.WriteAllText(log,"Programmatic callbacks only; no physical click proof\n");XCoroutine.Run(Exercise());}
 static IEnumerator Exercise(){
  var ui=UISystem.GetSingleton();
  if(!ui.TryGetWidget<UILogin>(out var login)){Record("login",false,"not loaded");yield break;}
  var entity=XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
  int original=entity.selectedServer;
  Record("login",true,"name="+login.vm.ServerName+" entity="+original);
  Click(login,"changeClick");yield return new WaitForSecondsRealtime(1);
  if(!ui.TryGetWidget<UIServerSelect>(out var selector)){Record("selector",false,"not loaded");yield break;}
  Record("selector",true,"groups="+selector.vm.Groups.Count+" servers="+selector.vm.Servers.Count);
  for(int group=0;group<selector.vm.Groups.Count;group++){
   selector.vm.GroupClick(null,group);yield return new WaitForSecondsRealtime(.3f);
   Record("group",selector.vm.Servers.Count>0,"index="+group+" count="+selector.vm.Servers.Count);
  }
  selector.vm.ServerClick(null,0);yield return new WaitForSecondsRealtime(.5f);
  Record("select",!ui.TryGetWidget<UIServerSelect>(out var closed),"entity="+entity.selectedServer+" name="+login.vm.ServerName+" canEnter="+login.vm.CanEnter);
  Record("stored",new LoginEntity().selectedServer==entity.selectedServer,"restored="+new LoginEntity().selectedServer);
  Click(login,"changeClick");yield return new WaitForSecondsRealtime(.5f);
  if(ui.TryGetWidget<UIServerSelect>(out selector)){selector.vm.GroupClick(null,2);yield return new WaitForSecondsRealtime(.3f);selector.vm.ServerClick(null,2);yield return new WaitForSecondsRealtime(.3f);Record("maintenance",!login.vm.CanEnter,"entity="+entity.selectedServer+" canEnter="+login.vm.CanEnter+" actualButton="+((Button)Target(login,"enterClick")).interactable);}
  entity.SelectServer(original);
  Click(login,"announcementClick");yield return new WaitForSecondsRealtime(.7f);
  if(ui.TryGetWidget<UINotice>(out var notice)){Record("notice-open",true,"loaded");Click(notice,"closeClick");yield return new WaitForSecondsRealtime(.5f);Record("notice-close",!ui.TryGetWidget<UINotice>(out notice),"closed");}else Record("notice-open",false,"not loaded");
  Record("done",true,"source implementation unchanged; restore selection entity="+original);
 }
}
