"""Compose MainPage artwork as independent UGUI nodes; business data comes from Excel."""
from pathlib import Path
import json,hashlib,sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/python-libs'))
from PIL import Image
from export_config import export
OUT=Path(__file__).parent
ASSETS=OUT/'assets';ASSETS.mkdir(exist_ok=True)
cfg=export();texts=cfg['texts'];index=json.loads((OUT/'resource-index.json').read_text('utf8'))
lookup={r['package']+'/'+r['name']:r for r in index};names={};manifest=[];ids={}
def art(key,source=None,crop=None,shared=True):
 if key in ids:return ids[key]
 if source:
  p=ROOT/'extracted-resources'/source;m=json.loads(p.with_suffix('.json').read_text('utf8')) if p.with_suffix('.json').exists() else {};sp=m.get('sprite',{});aid=m.get('id')
 else:
  m=lookup.get(key) or next(r for r in index if r['name']==key.split('/')[-1]);p=ROOT/m['imagePath'];sp=m.get('sprite',{});aid=m['sourceAssetId']
 img=Image.open(p).convert('RGBA');packed=img.size
 original=sp.get('original_size',img.size);offset=sp.get('offset',[0,0])
 if original!=list(img.size):
  restored=Image.new('RGBA',original);restored.paste(img,offset);img=restored
 if crop:img=img.crop(crop)
 temp=ASSETS/'current.png';img.save(temp);data=temp.read_bytes();temp.unlink();sha=hashlib.sha256(data).hexdigest();(ASSETS/(sha+'.png')).write_bytes(data)
 filename=key.replace('/','_')+('_icon' if crop else '')+'.png';names[sha]=filename
 dst='Assets/Data/Widgets/'+('Common/Icons/' if shared else 'Home/UIHomeAssets/')+(filename if shared else filename.lower())
 manifest.append(dict(key=key,source=str(p.relative_to(ROOT)).replace('\\','/'),sourceAssetId=aid,sourceSha256=hashlib.sha256(p.read_bytes()).hexdigest(),sha256=sha,assetId=sha,destination=dst,packedSize=packed,originalSize=original,offset=offset,crop=crop,purpose=key))
 ids[key]=sha;return sha
stretch=dict(anchorMin=dict(x=0,y=0),anchorMax=dict(x=1,y=1),pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=0),sizeDelta=dict(x=0,y=0))
def rect(x,y,w,h,anchor='top'):
 return dict(anchorMin=dict(x=0,y=1 if anchor=='top' else 0),anchorMax=dict(x=0,y=1 if anchor=='top' else 0),pivot=dict(x=0,y=1),anchoredPosition=dict(x=x,y=-y if anchor=='top' else y),sizeDelta=dict(x=w,y=h))
font_hashes={}
for font in ['fangzhenglanting','hanyijingkai']:
 data=(ROOT/'extracted-resources/assets/data/fonts'/f'{font}.ttf').read_bytes();sha=hashlib.sha256(data).hexdigest();font_hashes[font]=sha;(ASSETS/(sha+'.ttf')).write_bytes(data);names[sha]=font+'.ttf'
nodes=[]
def n(id,parent,typ,x,y,w,h,**kw):
 d=dict(id=id,name=id,parent=parent,type=typ,x=x,y=y,width=w,height=h,color='#FFFFFF',active=True,surface='transparent',fontSize=28,text='',rectTransform=rect(x,y,w,h));d.update(kw)
 if d.get('image','present') is None:d.pop('image')
 if typ=='Text':d.update(fontAsset=font_hashes['hanyijingkai' if id in ['GuildTitle','Power'] or (id.startswith('Entry40') and id.endswith('Label')) else 'fangzhenglanting'],verticalAlign='center',textOutline=dict(color='#231C13FF',width=1.3))
 if typ=='Button':d['action']=dict(type='none')
 nodes.append(d);return d
n('UIHome',None,'Container',0,0,1080,1920,rectTransform=stretch,templateSourceId='SvHome',templateSourceVersion=31)
n('SafeArea','UIHome','Container',0,0,1080,1920,rectTransform=stretch)
n('CampVisible','SafeArea','Container',0,0,1080,1920,rectTransform=stretch)
n('Header','CampVisible','Image',0,0,1080,250,image=art('Common/common_bg_16_0',shared=False),imageFit='100% 100%')
for i,(name,key,source) in enumerate([('Gold','gold','small_gold'),('Gem','gem','small_zuanshi'),('Energy','energy','small_tili')]):
 x=25+i*198
 n(name+'Rail','CampVisible','Image',x,110,178,42,image=art('Common/hub_bg_6',shared=False),imageFit='100% 100%')
 n(name+'Icon',name+'Rail','Image',-1,-1,44,44,image=art('CommonIcon/'+source,'fairygui/CommonIcon/'+source+'.png'),imageFit='contain')
 n(name,name+'Rail','Text',44,0,132,42,textAlign='center',fontSize=27,color='#F6E9D4')
n('LeaderPlate','CampVisible','Image',100,228,470,61,image=art('Main/hub_bg_1_0',shared=False),imageFit='100% 100%')
n('SettingsClick','CampVisible','Button',22,175,121,121,image=art('Common/player_bg_0'),imageFit='contain')
n('Portrait','SettingsClick','Image',13,12,95,95,image=art('CharacterIcon/hero_head_20032'),imageFit='contain')
n('PlayerName','CampVisible','Text',157,176,400,42,fontSize=28,textAlign='left',color='#EADAC1')
n('GuildTitle','CampVisible','Text',152,232,242,46,fontSize=33,textAlign='left',color='#FFCF43')
n('PowerIcon','CampVisible','Image',397,236,38,38,image=art('Common/common_icon_19'),imageFit='contain')
n('Power','CampVisible','Text',438,231,145,46,fontSize=33,textAlign='left',color='#FFCF43')
n('DemoLabel','CampVisible','Text',855,42,195,32,text=texts['home.demo'],fontSize=23,textAlign='right',color='#C4B69C')
n('RightRail','CampVisible','Image',976,330,90,431,image=art('Main/hub_bg_3',shared=False),imageFit='100% 100%')
n('BottomPlate','CampVisible','Image',0,0,1080,171,image=art('Main/hub_bg_7',shared=False),imageFit='100% 100%',rectTransform=rect(0,171,1080,171,'bottom'))
n('BottomDecoration','BottomPlate','Image',760,0,320,171,image=art('Main/hub_bg_25',shared=False),imageFit='contain')
n('Entries','CampVisible','Container',0,0,1080,1920,rectTransform=stretch)
activity_index=0;function_index=0
for e in cfg['entries']:
 group=e['group'];id='Entry'+str(e['id']);bottom=False
 if group=='Activity':x,y,w,h=25,342,120,135
 elif group=='Function':x,y,w,h=978,354,90,103
 elif group=='Quick':x,y,w,h=790+(e['id']-301)*92,225,82,82
 elif group=='Navigation':x,y,w,h=[6,280,420,560,700,917][e['id']-401],168,138,155;bottom=True
 else:x,y,w,h=966,337,103,106;bottom=True
 n(id,'Entries','Container',x,y,w,h,rectTransform=rect(x,y,w,h,'bottom' if bottom else 'top'))
 px=py=0
 if group=='Activity':
  px=(activity_index%3)*132;py=(activity_index//3)*142
  if e['visible']:activity_index+=1
 elif group=='Function':
  py=function_index*112
  if e['visible']:function_index+=1
 n(id+'Position',id,'Container',px,py,w,h,name='Position')
 n(id+'Visible',id+'Position','Container',0,0,w,h,name='Visible',active=e['visible'])
 n(id+'Click',id+'Visible','Button',0,0,w,h,name='OpenClick')
 iconkey='Main/'+e['icon'];crop=None
 if group=='Function':
  # Labels are separately authored Text nodes; crop only the lower baked label strip.
  raw=lookup[iconkey];size=raw.get('sprite',{}).get('original_size',raw['size']);crop=[0,0,size[0],int(size[1]*.61)]
 if group=='Recruit':
  raw=lookup[iconkey];size=raw.get('sprite',{}).get('original_size',raw['size']);crop=[0,0,size[0],int(size[1]*.68)]
 icon=art(iconkey,crop=crop) if e['icon'] else None
 if group=='Quick':ix,iy,iw,ih=0,0,w,h
 elif group=='Navigation':ix,iy,iw,ih=(3,5,128,120) if e['id'] in [401,406] else (27,22,84,85)
 else:ix,iy,iw,ih=4,0,w-8,h-33
 n(id+'Icon',id+'Click','Image',ix,iy,iw,ih,image=icon,imageFit='contain')
 if group!='Quick':n(id+'Label',id+'Click','Text',-6,h-41,w+12,38,text=texts[e['labelId']],fontSize=29 if group=='Navigation' else 26,textAlign='center',color='#FFF7E7')
 n(id+'Locked',id+'Click','Image',0,0,w,h,name='Locked',color='#16130D',surface='solid',active=not e['unlocked'])
 # The lock overlay never captures clicks, so locked destinations explain their condition.
 n(id+'LockIcon',id+'Locked','Image',w/2-13,20,26,32,image=art('Common/common_icon_16'),imageFit='contain')
n('TaskClick','CampVisible','Button',0,350,715,97,rectTransform=rect(0,350,715,97,'bottom'))
n('TaskBackground','TaskClick','Image',0,0,715,97,image=art('Main/hub_bg_5',shared=False),imageFit='100% 100%')
n('TaskIcon','TaskClick','Image',22,5,90,90,image=art('Main/hub_button_14'),imageFit='contain')
n('TaskProgress','TaskClick','Text',130,21,570,55,fontSize=28,textAlign='left',color='#FFF2DC')
n('ChatClick','CampVisible','Button',0,247,850,64,rectTransform=rect(0,247,850,64,'bottom'))
n('ChatBackground','ChatClick','Image',0,0,850,64,image=art('Main/hub_bg_5'),imageFit='100% 100%')
n('Channel','ChatClick','Text',32,3,100,56,text=texts['home.chat'],fontSize=27,textAlign='center',color='#E0CAA4')
n('Chat','ChatClick','Text',153,3,680,56,fontSize=26,textAlign='left',color='#B7A378')
# Keep the previously implemented map preview as its own full-screen layer.
old=json.loads((ROOT/'tools/home/home-payload.json').read_text('utf8'))
oldids={i['name']:i['assetId'] for i in json.loads((ROOT/'tools/home/asset-manifest.json').read_text('utf8'))}
for key in ['map','button','panel']:
 src=next(i['source'] for i in json.loads((ROOT/'tools/home/asset-manifest.json').read_text('utf8')) if i['name']==key)
 art('Preview/'+key,src.removeprefix('extracted-resources/'),shared=False)
n('MapVisible','SafeArea','Container',0,0,1080,1920,rectTransform=stretch,active=False)
n('MapBackground','MapVisible','Image',0,0,1080,1920,image=ids['Preview/map'],imageFit='cover',rectTransform=stretch)
n('MapTitle','MapVisible','Text',100,95,880,80,text=texts['home.map'],fontSize=48,textAlign='center',color='#FFE4A8')
n('MapHint','MapVisible','Text',100,350,880,80,text=texts['home.maphint'],fontSize=30,textAlign='center',rectTransform=rect(100,350,880,80,'bottom'))
n('ReturnClick','MapVisible','Button',200,230,680,130,image=ids['Preview/button'],imageFit='100% 100%',rectTransform=rect(200,230,680,130,'bottom'))
n('ReturnLabel','ReturnClick','Text',0,15,680,100,text=texts['home.return'],fontSize=39,textAlign='center')
# Unused legacy intent binding remains a transparent inactive map shortcut, preserving older probe/API callers.
n('MapClick','CampVisible','Button',0,0,1,1,active=False)
n('FeedbackVisible','SafeArea','Image',0,0,1080,1920,rectTransform=stretch,color='#000000',surface='solid',active=False)
n('FeedbackPanel','FeedbackVisible','Image',140,735,800,390,image=art('Common/common_bg_8_2',shared=False),imageFit='100% 100%')
n('FeedbackText','FeedbackPanel','Text',60,65,680,100,fontSize=38,textAlign='center',color='#4B3022')
n('CloseFeedbackClick','FeedbackPanel','Button',220,230,360,100,image=ids['Preview/button'],imageFit='100% 100%')
n('CloseFeedbackLabel','CloseFeedbackClick','Text',0,0,360,100,text=texts['home.close'],fontSize=36,textAlign='center')
animations=old['design']['animations']
for a in animations:
 a['hostNodeId']='UIHome'
 for c in a['clip']['curves']:c['targetNodeId']='UIHome'
nodes[0]['animationHost']=True
payload=dict(root='UIHome',design=dict(schemaVersion=1,rootId='Canvas',name='Miniwow MainPage adaptation',width=1080,height=1920,nodes=nodes,animations=animations))
(OUT/'uihome-payload.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2),'utf8')
(ASSETS/'asset-names.json').write_text(json.dumps(names,indent=2),'utf8')
for item in manifest:
 if '/Home/UIHomeAssets/' in item['destination']:item['destination']='Assets/Data/Widgets/Home/UIHomeAssets/'+names[item['assetId']].lower()
(OUT/'production-resources.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),'utf8')
print('Prepared',len(nodes),'nodes and',len(manifest),'independent images')
