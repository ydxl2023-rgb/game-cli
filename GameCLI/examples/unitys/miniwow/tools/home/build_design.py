"""Compose independently extracted artwork into the portrait camp and Home widget."""
from pathlib import Path
import hashlib,json,sys,shutil
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/python-libs'))
from PIL import Image
OUT=Path(__file__).parent
ASSETS=OUT/'assets'; ASSETS.mkdir(exist_ok=True)
source={
 'camp':'assets/data/ui/background/hero_bg_2.png',
 'hero':'fairygui/Main/images/hub_bg_33.png',
 'map':'assets/data/ui/background/map_1_0.png',
 'button':'fairygui/Main/images/hub_button_40.png',
 'panel':'fairygui/Main/images/hub_bg_7.png',
 'ribbon':'fairygui/Main/images/hub_bg_23.png',
 'compass':'fairygui/Main/images/hub_button_22.png',
}
ids={};manifest=[];names={}
for name,relative in source.items():
 p=ROOT/'extracted-resources'/relative; data=p.read_bytes(); h=hashlib.sha256(data).hexdigest()
 (ASSETS/(h+'.png')).write_bytes(data);ids[name]=h;names[h]=name+'.png'
 sidecar=p.with_suffix('.json')
 origin=json.loads(sidecar.read_text('utf8')) if sidecar.exists() else {}
 manifest.append(dict(name=name,source=p.relative_to(ROOT).as_posix(),sourceAssetId=origin.get('id'),sourceSha256=h,assetId=h,file=h+'.png',size=list(Image.open(p).size),purpose={'camp':'Home scene static environment','hero':'Home static character illustration; not an animated model','map':'Map preview; no stage simulation','button':'Independent button background','panel':'Panel background','ribbon':'Section title background (collected, unused in final layout)','compass':'Map navigation icon'}[name]))
font=ROOT/'extracted-resources/assets/data/fonts/fangzhenglanting.ttf'
fontHash=hashlib.sha256(font.read_bytes()).hexdigest()
shutil.copyfile(font,ASSETS/(fontHash+'.ttf'));names[fontHash]='fangzhenglanting.ttf'
(ASSETS/'asset-names.json').write_text(json.dumps(names,indent=2),'utf8')
(OUT/'asset-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),'utf8')
stretch=dict(anchorMin=dict(x=0,y=0),anchorMax=dict(x=1,y=1),pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=0),sizeDelta=dict(x=0,y=0))
def make_nodes():
 nodes=[]
 def n(id,parent,type,x,y,w,h,**kw):
  node=dict(id=id,name=id,parent=parent,type=type,x=x,y=y,width=w,height=h,color='#FFFFFF',active=True,surface='transparent',fontSize=32,text='')
  node.update(kw)
  if type=='Text':node.update(fontAsset=fontHash,verticalAlign='center',textOutline=dict(color='#2C1A16FF',width=1.5))
  if type=='Button':node['action']=dict(type='none')
  nodes.append(node);return node
 return nodes,n
nodes,n=make_nodes()
n('HomeEnvironment',None,'Container',0,0,1080,1920,rectTransform=stretch)
n('CampBackground','HomeEnvironment','Image',0,0,1080,1920,image=ids['camp'],imageFit='cover',uvRect=dict(x=0,y=0,width=1,height=1))
n('CampHero','CampBackground','Image',210,540,660,629,image=ids['hero'],imageFit='contain',rectTransform=dict(anchorMin=dict(x=.5,y=.45),anchorMax=dict(x=.5,y=.45),pivot=dict(x=.5,y=0),anchoredPosition=dict(x=0,y=0),sizeDelta=dict(x=660,y=629)))
environment=dict(root='HomeEnvironment',design=dict(schemaVersion=1,rootId='Canvas',name='MiniwowPortraitUI',width=1080,height=1920,nodes=nodes,animations=[]))
(OUT/'environment-payload.json').write_text(json.dumps(environment,ensure_ascii=False,indent=2),'utf8')
nodes,n=make_nodes()
n('MiniwowHome',None,'Container',0,0,1080,1920,rectTransform=stretch,templateSourceId='SvHome',templateSourceVersion=31)['name']='Home'
n('SafeArea','MiniwowHome','Container',0,0,1080,1920,rectTransform=stretch)
n('CampVisible','SafeArea','Container',0,0,1080,1920,rectTransform=stretch)
n('Header','CampVisible','Container',0,0,1080,190)
n('Heading','Header','Text',48,72,700,65,text='冒险营地',fontSize=44,textAlign='left',color='#FFE4A8')
n('Footer','CampVisible','Container',60,1680,960,160,rectTransform=dict(anchorMin=dict(x=.5,y=0),anchorMax=dict(x=.5,y=0),pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=140),sizeDelta=dict(x=960,y=160)))
n('MapClick','Footer','Button',240,0,480,130,image=ids['button'],imageFit='100% 100%')
n('MapIcon','MapClick','Image',30,25,65,80,image=ids['compass'],imageFit='contain')
n('MapLabel','MapClick','Text',110,20,330,90,text='冒险地图',fontSize=38,textAlign='center')
n('MapVisible','SafeArea','Container',0,0,1080,1920,rectTransform=stretch,active=False)
n('MapBackground','MapVisible','Image',0,0,1080,1920,image=ids['map'],imageFit='cover',uvRect=dict(x=0,y=0,width=1,height=1))
n('MapTitlePanel','MapVisible','Image',60,100,960,150,image=ids['panel'],imageFit='100% 100%')
n('MapTitle','MapVisible','Text',100,135,880,70,text='冒险地图',fontSize=52,textAlign='center',color='#FFE4A8')
n('MapInfo','MapVisible','Image',60,1330,960,440,image=ids['panel'],imageFit='100% 100%',rectTransform=dict(anchorMin=dict(x=.5,y=0),anchorMax=dict(x=.5,y=0),pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=370),sizeDelta=dict(x=960,y=440)))
n('MapStatus','MapInfo','Text',60,40,840,60,text='远方的旅途，等待启程',fontSize=38,textAlign='center',color='#FFE4A8')
n('MapHint','MapInfo','Text',60,125,840,75,text='战斗关卡尚未开放',fontSize=29,textAlign='center')
n('ReturnClick','MapInfo','Button',120,250,720,135,image=ids['button'],imageFit='100% 100%')
n('ReturnLabel','ReturnClick','Text',50,20,620,95,text='返回营地',fontSize=42,textAlign='center',rectTransform=dict(anchorMin=dict(x=0,y=1),anchorMax=dict(x=0,y=1),pivot=dict(x=0,y=1),anchoredPosition=dict(x=50,y=-20),sizeDelta=dict(x=620,y=95)))
animations=[dict(id='MiniwowHomeEntrance',clip=dict(name='HomeOpen',frameRate=30,startTime=0,stopTime=.6,loopTime=False,curves=[dict(targetNodeId='MiniwowHome',component='UnityEngine.CanvasGroup',property='m_Alpha',keys=[dict(time=0,value=0,inTangent=0,outTangent=0),dict(time=.6,value=1,inTangent=0,outTangent=0)])]),state=dict(name='Open',speed=1,default=True),source=dict(adaptation='Portrait Home entrance fade; custom product timing'))]
payload=dict(root='MiniwowHome',design=dict(schemaVersion=1,rootId='Canvas',name='MiniwowPortraitUI',width=1080,height=1920,nodes=nodes,animations=animations))
nodes[0]['animationHost']=True
animations[0]['hostNodeId']='MiniwowHome'
animations[0]['state']['cycleOffset']=0
for curve in animations[0]['clip']['curves']:
 curve.update(preWrapMode=8,postWrapMode=8)
 for key in curve['keys']:key.update(inWeight=1/3,outWeight=1/3,weightedMode=0)
(OUT/'home-payload.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2),'utf8')
print(json.dumps(dict(homeNodes=len(nodes),assets=len(manifest),font=fontHash)))
