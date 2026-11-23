using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Widget;
using Newtonsoft.Json;

// Independent, temporary runtime observation. Never saved into product resources.
public static class PracticeIndependentProbe
{
    private static string Path(Transform t) { return t.parent ? Path(t.parent)+"/"+t.name : t.name; }
    public static string Snapshot()
    {
        var buttons=UnityEngine.Object.FindObjectsOfType<Button>().Select(b=>new {path=Path(b.transform),interactable=b.IsInteractable(),texts=b.GetComponentsInChildren<Text>().Select(t=>t.text).ToArray()}).ToArray();
        var texts=UnityEngine.Object.FindObjectsOfType<Text>().Select(t=>new {path=Path(t.transform),text=t.text}).ToArray();
        var animations=UnityEngine.Object.FindObjectsOfType<Animator>().Where(a=>a.runtimeAnimatorController).Select(a=>new {path=Path(a.transform),state=a.GetCurrentAnimatorStateInfo(0).fullPathHash,time=a.GetCurrentAnimatorStateInfo(0).normalizedTime}).ToArray();
        return JsonConvert.SerializeObject(new {frame=Time.frameCount,buttons,texts,animations,metadata=UnityEngine.Object.FindObjectsOfType<Metadata>().Select(m=>Path(m.transform)).ToArray()});
    }
    public static string Click(string suffix)
    {
        var targets=UnityEngine.Object.FindObjectsOfType<Button>().Where(b=>suffix.StartsWith("text:")?b.GetComponentsInChildren<Text>().Any(t=>t.text==suffix.Substring(5)):Path(b.transform).EndsWith(suffix,StringComparison.Ordinal)).ToArray();
        if(targets.Length!=1)return JsonConvert.SerializeObject(new {error="target count",count=targets.Length,suffix});
        var b=targets[0];var r=(RectTransform)b.transform;
        var canvas=b.GetComponentInParent<Canvas>();var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        var position=RectTransformUtility.WorldToScreenPoint(camera,r.TransformPoint(r.rect.center));
        var pointer=new PointerEventData(EventSystem.current){position=position,button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        var first=hits.Count>0?hits[0].gameObject:null;
        var actual=first?ExecuteEvents.GetEventHandler<IPointerClickHandler>(first):null;
        bool match=actual==b.gameObject&&b.IsInteractable();
        if(match)ExecuteEvents.Execute(actual,pointer,ExecuteEvents.pointerClickHandler);
        return JsonConvert.SerializeObject(new {target=Path(b.transform),position=new {x=position.x,y=position.y},first=first?Path(first.transform):null,actual=actual?Path(actual.transform):null,match});
    }
    public static bool Request(string name,bool open)
    {
        var type=typeof(PracticePlugin).Assembly.GetType(name);
        return open?XPluginUtility.PostEvent(new SystemActive{type=type}):XPluginUtility.PostEvent(new SystemDetive{type=type});
    }
    public static bool OpenLoading()
    {
        return UISystem.GetSingleton().OpenWidget<UILoading>(w=>{if(w){w.UpdateProgress(.375f,"Independent");w.SetTip("Independent acceptance");}});
    }
    public static string LoadingBoundary()
    {
        if(!UISystem.GetSingleton().TryGetWidget<UILoading>(out var w))return "missing";
        var rows=new List<object>();
        foreach(var value in new[]{-1f,0f,.375f,1f,2f,float.NaN,float.NegativeInfinity,float.PositiveInfinity})
        {
            bool updated=w.UpdateProgress(value,"Independent");
            rows.Add(new {input=value.ToString(),updated,vm=w.vm.Rate,texts=w.GetComponentsInChildren<Text>().Select(t=>t.text).ToArray()});
        }
        w.SetTip(null);
        return JsonConvert.SerializeObject(new {rows,cleared=w.vm.TipText});
    }
    public static bool CloseLoading()
    {
        System.Type[] widgets=new System.Type[]{typeof(UILoading)};
        UISystem.GetSingleton().CloseWidget(widgets,immediate:true);
        return true;
    }
    public static string ShopLogic()
    {
        var e=new ShopEntity();var p=new ShopProduct{id=1,category=ShopCategory.Recharge,price=10,reward=5,remaining=1};
        bool init=e.InitializeDemo(new[]{p},20);p.price=999;
        e.SelectProduct(1);e.CancelPurchase();var cancelled=e.ConfirmPurchase();
        e.SelectProduct(1);var bought=e.ConfirmPurchase();var duplicate=e.ConfirmPurchase();
        bool sold=e.SelectProduct(1);var copy=e.GetProducts();copy[0].remaining=99;
        return JsonConvert.SerializeObject(new {init,cancelled,bought,duplicate,sold,balance=e.demoBalance,tokens=e.demoTokens,count=e.purchaseCount,remaining=e.GetProducts()[0].remaining});
    }
    public static string MailLogic()
    {
        var e=new MailEntity();
        var m=new MailMessage{id=901,title="Independent",body="Body",sentAt=1,expiresAt=1000,rewards=new List<MailReward>{new MailReward{itemId=1,count=5,icon="test"}}};
        bool init=e.ReplaceInbox(new[]{m});m.title="Mutated";m.rewards[0].count=999;
        bool isolated=e.GetMessages(10)[0].title=="Independent"&&e.GetMessages(10)[0].rewards[0].count==5;
        var unread=e.Delete(901);e.Select(901,10);var pending=e.Delete(901);var claim=e.Claim(901,10);var duplicate=e.Claim(901,10);var deleted=e.Delete(901);
        e.ReplaceInbox(new[]{m});var reimport=e.Claim(901,10);
        bool invalid=e.ReplaceInbox(new[]{m,m});bool preserved=e.GetMessages(10).Count==1;
        var copy=e.GetMessages(10);copy[0].body="escape";bool output=e.GetMessages(10)[0].body=="Body";
        bool empty=e.ReplaceInbox(new MailMessage[0]);
        return JsonConvert.SerializeObject(new {init,isolated,unread,pending,claim,duplicate,deleted,reimport,invalid,preserved,output,empty,count=e.GetMessages(10).Count,receipts=e.GetReceipts().Count});
    }
    public static string SettingsLogic()
    {
        string key=StorageKey.For<SettingsSystem,SettingsEntity>(x=>x.values);bool exists=PlayerPrefs.HasKey(key);string prior=PlayerPrefs.GetString(key);
        try
        {
            PlayerPrefs.DeleteKey(key);var e=new SettingsEntity();
            bool mid=e.SelectQuality(SettingsOption.Model,SettingsQuality.Medium);
            e.SelectPreset(SettingsQuality.Medium);var model=e.ReadQuality(SettingsOption.Model);
            bool bad=e.ChangeVolume(SettingsOption.MusicVolume,float.NaN);bool unknown=e.TogglePreference((SettingsOption)999);
            e.ChangeVolume(SettingsOption.MusicVolume,.37f);e.TogglePreference(SettingsOption.MusicEnabled);e.TogglePreference(SettingsOption.MusicEnabled);
            var restored=new SettingsEntity();float volume=restored.ReadVolume(SettingsOption.MusicVolume);
            var snapshot=e.values;e.ChangeVolume(SettingsOption.MusicVolume,.8f);bool isolated=Mathf.Approximately(snapshot[SettingsOption.MusicVolume],.37f);
            PlayerPrefs.SetString(key,"{broken");var corrupt=new SettingsEntity();
            return JsonConvert.SerializeObject(new {count=e.values.Count,mid,model,bad,unknown,volume,isolated,corruptCount=corrupt.values.Count});
        }
        finally {if(exists)PlayerPrefs.SetString(key,prior);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
    }
    public static string RedDotLogic()
    {
        var e=new RedDotEntity();e.Initialize();var ids=e.GetIds();var before=e.GetCounts();
        var rows=new List<object>();
        foreach(var id in ids)
        {
            if(!e.UpdateSignal(id,3))continue;
            var changes=e.Refresh(ids);var count=e.GetCount(id);var unchanged=e.Refresh(ids).Count;
            var snapshot=e.GetSignals();snapshot[id]=999;bool isolated=e.GetSignals()[id]==3;
            bool negative=e.UpdateSignal(id,-1);e.UpdateSignal(id,0);e.Refresh(ids);
            rows.Add(new {id,count,changes=changes.Count,unchanged,isolated,negative,zero=e.GetCount(id)});
        }
        return JsonConvert.SerializeObject(new {ids,rows,unknown=e.UpdateSignal(int.MaxValue,1)});
    }
    public static string Sample()
    {
        return JsonConvert.SerializeObject(UnityEngine.Object.FindObjectsOfType<Animator>().Where(a=>a.runtimeAnimatorController).Select(a=>new{path=Path(a.transform),time=a.GetCurrentAnimatorStateInfo(0).normalizedTime,children=a.GetComponentsInChildren<Transform>().Where(t=>t.name.IndexOf("Ball",StringComparison.OrdinalIgnoreCase)>=0).Select(t=>new{path=Path(t),z=t.localEulerAngles.z})}));
    }
    public static string ConfigCounts()
    {
        var rows=new List<object>();
        foreach(var f in typeof(Config.Excel).GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static))
        {
            var dictionary=f.GetValue(null) as System.Collections.IDictionary;
            if(dictionary!=null)rows.Add(new{name=f.Name,count=dictionary.Count});
        }
        return JsonConvert.SerializeObject(rows);
    }
    public static string ProfileBinding()
    {
        if(!UISystem.GetSingleton().TryGetWidget<UIHome>(out var home))return "missing";
        var vm=home.vm;var prior=vm.Profile.PlayerName;var level=vm.Profile.PlayerLevel;
        vm.Profile.PlayerName="Independent Profile";vm.Profile.PlayerLevel="37";
        var texts=home.GetComponentsInChildren<Text>(true).Where(t=>t.text=="Independent Profile"||t.text=="37").Select(t=>new{path=Path(t.transform),text=t.text,id=t.GetInstanceID()}).ToArray();
        var refs=home.GetComponentsInChildren<Metadata>(true).Where(m=>m.name=="ProfileView"||m.name=="ProfileShortcuts").Select(m=>new{path=Path(m.transform),fields=m.metadatas.Select(f=>new{name=f.name,targets=f.targets.Select(t=>new{path=t?Path(t.transform):null,inside=t&&t.transform.IsChildOf(home.transform),id=t?t.GetInstanceID():0}),components=f.components.Select(c=>new{path=c?Path(c.transform):null,inside=c&&c.transform.IsChildOf(home.transform),id=c?c.GetInstanceID():0})})}).ToArray();
        vm.Profile.PlayerName=prior;vm.Profile.PlayerLevel=level;
        return JsonConvert.SerializeObject(new{texts,refs});
    }
    public static bool StopMaster()
    {
        XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Unload<MasterSystem>();
        return true;
    }
    public static bool RestartMaster()
    {
        var p=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>();
        var master=p.Load<MasterSystem>();if(!master.IsActive())master.Active();XPluginUtility.PostEvent(new SystemActive{type=typeof(LoginSystem)});
        return true;
    }
    public static bool HomeData()
    {
        var e=XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        e.playerName="Independent Host";e.currencyOneAmount=123;e.currencyTwoAmount=456;e.currencyThreeAmount=789;e.chatMessage="Independent Chat";
        XPluginUtility.PostEvent(new FunctionLevelChanged{level=37});
        return XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<HomeSystem>().Refresh();
    }
    public static bool SwitchMail(bool enabled)
    {
        return XPluginUtility.PostEvent(new FunctionSwitchChanged{id=Config.Excel.SYS_MAIL,enabled=enabled});
    }
    public static string LaunchBoundary()
    {
        var l=UnityEngine.Object.FindObjectOfType<XLauncher>(true);var rows=new List<object>();
        foreach(bool download in new[]{false,true})
        {
            if(download)l.ShowUpdate();else l.ShowLoading();
            foreach(float v in new[]{-1f,0f,.375f,1f,2f,float.NaN,float.NegativeInfinity,float.PositiveInfinity})
            {
                l.UpdateProgress(v,"Independent");
                var fill=(RectTransform)typeof(XLauncher).GetField("fill",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(l);
                var handle=(RectTransform)typeof(XLauncher).GetField("handle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(l);
                rows.Add(new{download,input=v.ToString(),texts=l.GetComponentsInChildren<Text>().Select(t=>t.text).ToArray(),fill=new{x=fill.anchorMax.x,y=fill.anchorMax.y},handle=new{x=handle.anchorMin.x,y=handle.anchorMin.y}});
            }
        }
        l.ShowBackground();int active=l.GetComponentsInChildren<Text>().Length;
        return JsonConvert.SerializeObject(new{rows,backgroundActiveTexts=active});
    }
    public static string NoticeContent(bool empty)
    {
        if(!UISystem.GetSingleton().TryGetWidget<UINotice>(out var w))return "missing";
        string body=empty?string.Empty:string.Join("\n",Enumerable.Repeat("Independent line <a href=\"https://example.com\">LINK</a>",80));
        bool set=w.SetContent("Independent",body);
        var urls=new[]{"https://example.com","http://example.com","javascript:alert(1)","file:///x","relative",""}.Select(x=>new{input=x,valid=NoticeText.TryNormalizeUrl(x,out var normalized),normalized});
        return JsonConvert.SerializeObject(new{set,entry=w.vm.Entry,empty=w.vm.Empty,urls});
    }
    public static string ScrollState()
    {
        Canvas.ForceUpdateCanvases();
        return JsonConvert.SerializeObject(UnityEngine.Object.FindObjectsOfType<ScrollRect>().Select(s=>new{path=Path(s.transform),contentHeight=s.content?s.content.rect.height:0,viewportHeight=s.viewport?s.viewport.rect.height:0,position=s.verticalNormalizedPosition}));
    }
    public static bool InvalidLogin() { return XPluginUtility.PostEvent(new LoginSucceed { pid=0 }); }
    public static bool InvalidMaster()
    {
        XPluginUtility.PostEvent<SystemActive>(null);
        return true;
    }
    public static string DeepState()
    {
        var m=XEntityManager.GetSingleton().QueryEntity<MailEntity>();
        var s=XEntityManager.GetSingleton().QueryEntity<ShopEntity>();
        return JsonConvert.SerializeObject(new {
            mail=new{selected=m.selectedId,messages=m.GetMessages(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),receipts=m.GetReceipts()},
            shop=new{s.demoBalance,s.demoTokens,s.purchaseCount,s.category,products=s.GetProducts()},
            entries=UnityEngine.Object.FindObjectsOfType<UIActivityEntry>(true).Select(x=>new{path=Path(x.transform),id=x.GetSystemId(),active=x.gameObject.activeInHierarchy,bound=x.vm!=null,badges=x.GetComponentsInChildren<UIActivityBadge>(true).Length}),
            scopes=RedDotScope.Snapshot().Select(x=>Path(x.transform)),
            sliders=UnityEngine.Object.FindObjectsOfType<Slider>().Select(x=>new{path=Path(x.transform),x.value,x.minValue,x.maxValue}),
            rewards=UnityEngine.Object.FindObjectsOfType<UIRewardEntry>().Select(x=>new{path=Path(x.transform),bound=x.vm!=null,texts=x.GetComponentsInChildren<Text>().Select(t=>new{path=Path(t.transform),t.text})})
        });
    }
    public static string RedSignals(int value)
    {
        var e=XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();int sent=0;
        foreach(var id in e.GetIds()) if(XPluginUtility.PostEvent(new RedDotSignalChanged{id=id,count=value}))sent++;
        return JsonConvert.SerializeObject(new{sent,scopes=RedDotScope.Snapshot().Length});
    }
    public static string HomeBadges(int mode)
    {
        var e=XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var entries=UnityEngine.Object.FindObjectsOfType<UIActivityEntry>(true);
        foreach(var x in entries)e.SetBadges(x.GetSystemId(),(ActivityBadge)mode);
        bool refresh=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<HomeSystem>().Refresh();
        return JsonConvert.SerializeObject(new{refresh,entries=entries.Length});
    }
    public static string MailFixture()
    {
        var e=XEntityManager.GetSingleton().QueryEntity<MailEntity>();long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var ok=e.ReplaceInbox(new[]{new MailMessage{id=9101,title="Independent reward",body="Reward fixture",sentAt=now,expiresAt=now+86400,rewards=new List<MailReward>{new MailReward{itemId=1,count=123,icon=""}}},new MailMessage{id=9102,title="Independent plain",body="Plain fixture",sentAt=now,expiresAt=now+86400,rewards=new List<MailReward>()}});
        bool refresh=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<MailSystem>().Refresh();
        return JsonConvert.SerializeObject(new{ok,refresh});
    }
    public static string NoticeShort()
    {
        if(!UISystem.GetSingleton().TryGetWidget<UINotice>(out var w))return "missing";
        return w.SetContent("Independent short","One <a href=\"https://example.com\">LINK</a> line").ToString();
    }
    public static string SliderInput(string suffix,float ratio)
    {
        var s=UnityEngine.Object.FindObjectsOfType<Slider>().Single(x=>Path(x.transform).EndsWith(suffix,StringComparison.Ordinal));
        var r=(RectTransform)s.transform;var canvas=s.GetComponentInParent<Canvas>();var cam=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        var local=new Vector2(Mathf.Lerp(r.rect.xMin,r.rect.xMax,ratio),r.rect.center.y);var pos=RectTransformUtility.WorldToScreenPoint(cam,r.TransformPoint(local));
        var p=new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
        var first=hits.Count>0?hits[0].gameObject:null;var actual=first?ExecuteEvents.GetEventHandler<IPointerDownHandler>(first):null;
        bool match=actual==s.gameObject;if(match){ExecuteEvents.Execute(actual,p,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(actual,p,ExecuteEvents.pointerUpHandler);}
        return JsonConvert.SerializeObject(new{path=Path(s.transform),match,s.value,first=first?Path(first.transform):null});
    }    public static string Gates(bool enabled)
    {
        foreach(var id in Config.Excel.functionSystemConfig.Keys)XPluginUtility.PostEvent(new FunctionSwitchChanged{id=id,enabled=enabled});
        XPluginUtility.PostEvent(new FunctionLevelChanged{level=99});
        return Config.Excel.functionSystemConfig.Count.ToString();
    }
    public static string NoticeReminder()
    {
        var e=XEntityManager.GetSingleton().QueryEntity<NoticeEntity>();
        return JsonConvert.SerializeObject(new{until=e.suppressedUntil,suppressed=UINotice.IsReminderSuppressed()});
    }
    public static string RedState()
    {
        var e=XEntityManager.GetSingleton().QueryEntity<RedDotEntity>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        return JsonConvert.SerializeObject(new{counts=e.GetCounts(),scopes=RedDotScope.Snapshot().Select(s=>new{path=Path(s.transform),bindings=((RedDotBinding[])typeof(RedDotScope).GetField("bindings",flags).GetValue(s)).Select(b=>{var t=(GameObject)typeof(RedDotBinding).GetField("target",flags).GetValue(b);var label=(Text)typeof(RedDotBinding).GetField("countText",flags).GetValue(b);return new{id=(int)typeof(RedDotBinding).GetField("id",flags).GetValue(b),target=t?Path(t.transform):null,active=t&&t.activeSelf,text=label?label.text:null};})})});
    }    public static string LoginLogic()
    {
        string a=StorageKey.For<LoginSystem,LoginEntity>(x=>x.selectedServer),b=StorageKey.For<LoginSystem,LoginEntity>(x=>x.recentServer);
        bool ha=PlayerPrefs.HasKey(a),hb=PlayerPrefs.HasKey(b);int pa=PlayerPrefs.GetInt(a),pb=PlayerPrefs.GetInt(b);
        try {
            var e=new LoginEntity();var list=new[]{new ServerEntry{id=801,name="normal",state=ServerState.Normal},new ServerEntry{id=802,name="maintenance",state=ServerState.Maintenance}};
            bool init=e.SetServers(list);list[0].name="mutated";bool isolated=e.GetServers()[0].name=="normal";
            bool selected=e.SelectServer(802),blocked=!e.CanEnter(),recordBlocked=!e.RecordLoginServer();
            bool invalid=!e.SetServers(new[]{list[0],list[0]})&&!e.SetServers(new ServerEntry[0])&&!e.SetServers(null);
            bool preserved=e.serverCount==2;bool normal=e.SelectServer(801)&&e.CanEnter()&&e.RecordLoginServer();var restore=new LoginEntity();
            var copy=e.GetServers();copy.Clear();
            return JsonConvert.SerializeObject(new{init,isolated,selected,blocked,recordBlocked,invalid,preserved,normal,restoredSelection=restore.selectedServer,restoredRecent=restore.recentServer,outputIsolated=e.serverCount==2});
        }finally{if(ha)PlayerPrefs.SetInt(a,pa);else PlayerPrefs.DeleteKey(a);if(hb)PlayerPrefs.SetInt(b,pb);else PlayerPrefs.DeleteKey(b);PlayerPrefs.Save();}
    }    public static string NoticeLinks()
    {
        Canvas.ForceUpdateCanvases();var t=UnityEngine.Object.FindObjectOfType<NoticeText>();
        var links=(List<(string url,Color32 marker,List<Rect> bounds)>)typeof(NoticeText).GetField("links",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(t);
        var canvas=t.canvas;var cam=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        return JsonConvert.SerializeObject(links.Select(l=>new{url=l.url,glyphs=l.bounds.Count,hits=l.bounds.Select(r=>{var p=RectTransformUtility.WorldToScreenPoint(cam,t.rectTransform.TransformPoint(r.center));bool hit=t.TryGetLink(p,cam,out var url);return new{x=p.x,y=p.y,hit,url};})}));
    }
    public static string NoticeScroll(float delta)
    {
        var s=UnityEngine.Object.FindObjectOfType<UINotice>().GetComponentInChildren<ScrollRect>();var r=s.viewport;var canvas=s.GetComponentInParent<Canvas>();var cam=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        var pos=RectTransformUtility.WorldToScreenPoint(cam,r.TransformPoint(r.rect.center)); var noticeText=s.GetComponentInChildren<NoticeText>(); if(noticeText){var ls=(List<(string url,Color32 marker,List<Rect> bounds)>)typeof(NoticeText).GetField("links",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(noticeText);foreach(var l in ls){foreach(var box in l.bounds){var q=RectTransformUtility.WorldToScreenPoint(cam,noticeText.rectTransform.TransformPoint(box.center));if(noticeText.TryGetLink(q,cam,out var ignored)){pos=q;goto located;}}} } located: var p=new PointerEventData(EventSystem.current){position=pos,scrollDelta=new Vector2(0,delta)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);var first=hits.Count>0?hits[0].gameObject:null;var actual=first?ExecuteEvents.GetEventHandler<IScrollHandler>(first):null;bool match=actual==s.gameObject;if(match)ExecuteEvents.Execute(actual,p,ExecuteEvents.scrollHandler);
        return JsonConvert.SerializeObject(new{match,first=first?Path(first.transform):null,position=s.verticalNormalizedPosition});
    }    private static int completions;
    public static string MasterRace()
    {
        completions=0;
        foreach(var t in new[]{typeof(MailSystem),typeof(SettingsSystem),typeof(ShopSystem)})XPluginUtility.FireEvent(new SystemActive{type=t,complete=()=>completions++});
        var m=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<MasterSystem>();m.Detive();m.Active();return "deferred requested then deactivated/reactivated same instance";
    }
    public static string RaceState(){return JsonConvert.SerializeObject(new{completions,widgets=UnityEngine.Object.FindObjectsOfType<Metadata>().Select(m=>Path(m.transform)).Where(x=>x.Contains("UIMail")||x.Contains("UIShop")||x.Contains("UISettings")).ToArray()});}
    public static string InvalidTypes()
    {
        completions=0;
        foreach(var t in new[]{null,typeof(string),typeof(MasterSystem),typeof(RuntimeSystem),typeof(System.Collections.Generic.List<>)}){
            XPluginUtility.FireEvent(new SystemActive{type=t,complete=()=>completions++});XPluginUtility.FireEvent(new SystemDetive{type=t,complete=()=>completions++});
        }
        return "sent 10 invalid type operations";
    }
    public static string WidgetRace()
    {
        UISystem.GetSingleton().OpenWidget<UILoading>(w=>{if(w)completions++;});
        UISystem.GetSingleton().CloseWidget(new[]{typeof(UILoading)},immediate:true);
        return "loading open-close in same frame";
    }
    public static string ScreenGeometry()
    {
        return JsonConvert.SerializeObject(new{Screen.width,Screen.height,canvases=UnityEngine.Object.FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).Select(c=>new{path=Path(c.transform),rect=new{x=c.pixelRect.x,y=c.pixelRect.y,width=c.pixelRect.width,height=c.pixelRect.height},c.scaleFactor,scaler=c.GetComponent<CanvasScaler>()?c.GetComponent<CanvasScaler>().screenMatchMode.ToString():"none"}),roots=UnityEngine.Object.FindObjectsOfType<Metadata>().Where(h=>h.transform.parent&&h.transform.parent.GetComponent<Canvas>()&&h.transform.Find("Animator")).Select(h=>new{path=Path(h.transform),corners=Corners((RectTransform)h.transform),animator=Corners((RectTransform)h.transform.Find("Animator"))}),cameras=UnityEngine.Object.FindObjectsOfType<Camera>().Select(c=>new{c.name,c.pixelWidth,c.pixelHeight,target=c.targetTexture?c.targetTexture.width+"x"+c.targetTexture.height:"none"})});
    }
    private static object[] Corners(RectTransform r)
    {
        var c=r.GetComponentInParent<Canvas>();var cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var corners=new Vector3[4];r.GetWorldCorners(corners);return corners.Select(v=>{var q=RectTransformUtility.WorldToScreenPoint(cam,v);return (object)new{x=q.x,y=q.y};}).ToArray();
    }
    public static string HomeActivityGates(bool enabled)
    {
        if(!UISystem.GetSingleton().TryGetWidget<UIHome>(out var w))return "missing";
        foreach(var id in w.GetActivityIds())XPluginUtility.PostEvent(new FunctionSwitchChanged{id=id,enabled=enabled});
        return w.GetActivityIds().Length.ToString();
    }    public static string RedService(bool enabled)
    {
        var p=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>();
        if(enabled){var m=p.Load<RedDotSystem>();if(!m.IsActive())m.Active();}else p.Unload<RedDotSystem>();
        return enabled.ToString();
    }
    public static string ExtendedLogic()
    {
        var s=new ShopEntity();var a=new ShopProduct{id=1,category=ShopCategory.Recharge,price=10,reward=1,remaining=1};var b=new ShopProduct{id=2,category=ShopCategory.Gift,price=0,reward=2,remaining=1};
        s.InitializeDemo(new[]{a,b},5);s.SelectProduct(1);var insufficient=s.ConfirmPurchase();bool unchanged=s.demoBalance==5&&s.purchaseCount==0;
        s.SelectProduct(1);s.SelectCategory(ShopCategory.Gift);var stale=s.ConfirmPurchase();bool invalid=!s.InitializeDemo(new[]{a,a},100)&&s.demoBalance==5;
        var e=new MailEntity();var r=new MailReward{itemId=1,count=1,icon="test"};e.ReplaceInbox(new[]{new MailMessage{id=1,title="expired",body="x",sentAt=1,expiresAt=10,rewards=new List<MailReward>{r}},new MailMessage{id=2,title="plain",body="x",sentAt=1,expiresAt=100,rewards=new List<MailReward>()}});
        var expired=e.Claim(1,10);var plain=e.Claim(2,10);var batch=e.ClaimAll(10);bool missing=e.GetMessages(10).Count==1;
        return JsonConvert.SerializeObject(new{insufficient,unchanged,stale,invalid,expired,plain,batch,missing});
    }
    public static string FunctionLogic()
    {
        var e=new FunctionManagerEntity();e.UpdateLevel(10);var now=new DateTimeOffset(2026,11,20,0,0,0,TimeSpan.Zero);
        string prefix="{\"ID\":1,\"Condtions\":[\"Level:10\",\"Switch:1\"],\"StartTime\":\"2026-11-20T00:00:00Z\",\"StopTime\":\"2026-11-21T00:00:00Z\"}";
        var row=JsonConvert.DeserializeObject<Config.FunctionSystem>(prefix);
        bool start=e.Evaluate(row,now),before=e.Evaluate(row,now.AddSeconds(-1)),stop=e.Evaluate(row,now.AddDays(1));e.UpdateLevel(9);bool low=e.Evaluate(row,now);
        var malformed=JsonConvert.DeserializeObject<Config.FunctionSystem>(prefix.Replace("Level:10","Unknown:10"));bool bad=e.Evaluate(malformed,now);
        return JsonConvert.SerializeObject(new{start,before,stop,low,bad,negative=e.UpdateLevel(-1),unknown=e.UpdateSwitch(int.MaxValue,true)});
    }
    public static string RedValidation()
    {
        var field=typeof(Config.Excel).GetField("redDotConfig");var prior=field.GetValue(null);var results=new List<object>();
        try{
            foreach(var spec in new[]{"mismatch","missing-parent","cycle","invalid-condition","invalid-flag"}){
                var rows=new Dictionary<int,Config.RedDot>();
                var j=Newtonsoft.Json.Linq.JObject.Parse("{\"ID\":1,\"NumID\":1,\"ParentID\":0,\"RedDotCondition\":1,\"RedDotDisplay\":0,\"Enabled\":1,\"RefreshOnOpen\":1,\"RefreshOnClose\":1}");
                if(spec=="mismatch")j["NumID"]=2;if(spec=="missing-parent")j["ParentID"]=999;if(spec=="cycle")j["ParentID"]=1;if(spec=="invalid-condition")j["RedDotCondition"]=999;if(spec=="invalid-flag")j["Enabled"]=2;
                rows.Add(1,j.ToObject<Config.RedDot>());field.SetValue(null,rows);bool rejected=false;try{new RedDotEntity().Initialize();}catch(InvalidOperationException){rejected=true;}results.Add(new{spec,rejected});
            }
        }finally{field.SetValue(null,prior);}
        return JsonConvert.SerializeObject(results);
    }    public static string NoticeInput()
    {
        var w=UnityEngine.Object.FindObjectOfType<UINotice>();var s=w.GetComponentInChildren<ScrollRect>();var c=s.GetComponentInParent<Canvas>();var cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var r=s.viewport;
        var local=new Vector2(r.rect.xMax-30,r.rect.center.y);var pos=RectTransformUtility.WorldToScreenPoint(cam,r.TransformPoint(local));
        var p=new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left,scrollDelta=new Vector2(0,-1000)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);var first=hits.Count>0?hits[0].gameObject:null;var route=first?ExecuteEvents.GetEventHandler<IScrollHandler>(first):null;
        float before=s.verticalNormalizedPosition;if(route)ExecuteEvents.Execute(route,p,ExecuteEvents.scrollHandler);float afterScroll=s.verticalNormalizedPosition;
        var drag=first?ExecuteEvents.GetEventHandler<IDragHandler>(first):null;
        if(drag){ExecuteEvents.Execute(drag,p,ExecuteEvents.initializePotentialDrag);ExecuteEvents.Execute(drag,p,ExecuteEvents.beginDragHandler);p.position+=new Vector2(0,60);p.delta=new Vector2(0,60);p.dragging=true;ExecuteEvents.Execute(drag,p,ExecuteEvents.dragHandler);ExecuteEvents.Execute(drag,p,ExecuteEvents.endDragHandler);}
        return JsonConvert.SerializeObject(new{first=first?Path(first.transform):null,scrollMatch=route==s.gameObject,dragMatch=drag==s.gameObject,before,afterScroll,afterDrag=s.verticalNormalizedPosition});
    }    public static string SettingsInventory()
    {
        var w=UnityEngine.Object.FindObjectOfType<UISettings>();var m=w.GetComponent<Metadata>();
        return JsonConvert.SerializeObject(new{values=XEntityManager.GetSingleton().QueryEntity<SettingsEntity>().values,fields=m.metadatas.Select(f=>new{name=f.name,targets=f.targets.Select(t=>new{path=t?Path(t.transform):null,active=t&&t.gameObject.activeInHierarchy,hidden=t?Hidden(t.transform):null})})});
    }
    private static string Hidden(Transform t){while(t){if(!t.gameObject.activeSelf)return Path(t);t=t.parent;}return null;}
    public static string SettingPointer(string field)
    {
        var w=UnityEngine.Object.FindObjectOfType<UISettings>();var f=w.GetComponent<Metadata>().metadatas.Single(x=>string.Equals(x.name,field,StringComparison.OrdinalIgnoreCase));var target=f.targets.First();var before=XEntityManager.GetSingleton().QueryEntity<SettingsEntity>().values;
        string result=Click(Path(target.transform));var after=XEntityManager.GetSingleton().QueryEntity<SettingsEntity>().values;
        return JsonConvert.SerializeObject(new{field,path=Path(target.transform),active=target.gameObject.activeInHierarchy,hidden=Hidden(target.transform),result,changes=after.Where(kv=>before[kv.Key]!=kv.Value).Select(kv=>new{option=kv.Key.ToString(),before=before[kv.Key],after=kv.Value})});
    }
    public static string ScrollSurface(string suffix,float delta)
    {
        var t=UnityEngine.Object.FindObjectsOfType<RectTransform>().Single(x=>Path(x).EndsWith(suffix,StringComparison.Ordinal));var c=t.GetComponentInParent<Canvas>();var cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var pos=RectTransformUtility.WorldToScreenPoint(cam,t.TransformPoint(t.rect.center));var p=new PointerEventData(EventSystem.current){position=pos,scrollDelta=new Vector2(0,delta)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);var first=hits.Count>0?hits[0].gameObject:null;var route=first?ExecuteEvents.GetEventHandler<IScrollHandler>(first):null;if(route)ExecuteEvents.Execute(route,p,ExecuteEvents.scrollHandler);
        return JsonConvert.SerializeObject(new{first=first?Path(first.transform):null,route=route?Path(route.transform):null,delta});
    }
    public static string LongMail()
    {
        var e=XEntityManager.GetSingleton().QueryEntity<MailEntity>();long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var list=new List<MailMessage>();
        for(int i=0;i<120;i++)list.Add(new MailMessage{id=20000+i,title="Stress "+i.ToString("D3"),body=string.Join("\n",Enumerable.Repeat("Long mail body "+i,30)),sentAt=now-i,expiresAt=now+86400,rewards=new List<MailReward>{new MailReward{itemId=1001,count=i+1,icon="Assets/Data/Widgets/Common/Micros/ItemSlot/Resources/ui_system_information_14.png"}}});
        bool accepted=e.ReplaceInbox(list);bool refreshed=XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<MailSystem>().Refresh();return JsonConvert.SerializeObject(new{accepted,refreshed,count=e.GetMessages(now).Count});
    }
    public static string MailCells()
    {
        return JsonConvert.SerializeObject(new{selected=XEntityManager.GetSingleton().QueryEntity<MailEntity>().selectedId,cells=UnityEngine.Object.FindObjectsOfType<UIMailEntry>(true).Select(x=>new{instance=x.GetInstanceID(),active=x.gameObject.activeInHierarchy,bound=x.vm!=null,id=x.vm!=null?x.vm.mailId:0,texts=x.GetComponentsInChildren<Text>(true).Select(t=>t.text).ToArray()}),details=UnityEngine.Object.FindObjectOfType<UIMail>().GetComponentsInChildren<Text>().Select(t=>new{path=Path(t.transform),text=t.text})});
    }
    public static string AnimationSnapshot()
    {
        return JsonConvert.SerializeObject(new{frame=Time.frameCount,time=Time.realtimeSinceStartup,animators=UnityEngine.Object.FindObjectsOfType<Animator>().Where(a=>a.runtimeAnimatorController).Select(a=>new{path=Path(a.transform),state=a.GetCurrentAnimatorStateInfo(0).fullPathHash,time=a.GetCurrentAnimatorStateInfo(0).normalizedTime,loop=a.GetCurrentAnimatorStateInfo(0).loop,clips=a.GetCurrentAnimatorClipInfo(0).Select(c=>new{name=c.clip.name,c.clip.length,c.weight}),pose=a.GetComponentsInChildren<Transform>().Select(t=>new{path=Path(t),position=new{x=t.localPosition.x,y=t.localPosition.y},scale=new{x=t.localScale.x,y=t.localScale.y},z=t.localEulerAngles.z}).ToArray()}),legacy=UnityEngine.Object.FindObjectsOfType<Animation>().Select(a=>new{path=Path(a.transform),a.isPlaying})});
    }    public static string InstallRecorder(){XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Load<PracticeEventRecorder>();return "installed";}
    public static string Recorded(){return JsonConvert.SerializeObject(new{home=PracticeEventRecorder.home,red=PracticeEventRecorder.red});}
    public static string RedCoalesce()
    {
        PracticeEventRecorder.red.Clear();
        XPluginUtility.FireEvent(new RedDotSignalChanged{id=Config.Excel.RED_DOT_HOME_MAILTIPS,count=7});
        XPluginUtility.FireEvent(new RedDotSignalChanged{id=Config.Excel.RED_DOT_HOME_MAILTIPS,count=9});
        return "two signals same frame";
    }
    public static string AllHomeClicks()
    {
        if(!UISystem.GetSingleton().TryGetWidget<UIHome>(out var w))return "missing";
        var f=XEntityManager.GetSingleton().QueryEntity<FunctionManagerEntity>();var ids=w.GetActivityIds();f.UpdateLevel(99);
        var entries=w.GetComponentsInChildren<UIActivityEntry>(true);var rows=new List<object>();PracticeEventRecorder.home.Clear();
        foreach(var e in entries){
            foreach(var id in ids)f.UpdateSwitch(id,id==e.GetSystemId());
            XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<HomeSystem>().Refresh();Canvas.ForceUpdateCanvases();
            var b=e.GetComponent<Button>();if(!b)b=e.GetComponentInChildren<Button>();
            var path=b?Path(b.transform):null;var before=PracticeEventRecorder.home.Count;
            var click=b?Click(path):"no-button";
            rows.Add(new{id=e.GetSystemId(),path,click,emitted=PracticeEventRecorder.home.Skip(before).ToArray()});
        }
        foreach(var id in ids)f.UpdateSwitch(id,true);XPluginManager.GetSingleton().QueryPlugin<PracticePlugin>().Query<HomeSystem>().Refresh();
        return JsonConvert.SerializeObject(rows);
    }}
public class PracticeEventRecorder : XEventModule
{
    public static readonly List<string> home=new List<string>();
    public static readonly List<string> red=new List<string>();
    public override bool OnActive(){Subscribe<HomeEntryRequested>(e=>home.Add(e.entry.ToString()));Subscribe<RedDotChanged>(e=>red.Add(e.id+":"+e.count));return true;}
    public override void OnDetive(){UnsubscribeAll();}
}



