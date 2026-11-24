"""Adapt extracted launch artwork into a portrait GameCLI project document."""
from pathlib import Path
import json
import hashlib
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/python-libs'))
from PIL import Image

OUT = Path(__file__).resolve().parent
ASSETS = OUT / 'assets'
ASSETS.mkdir(exist_ok=True)
source = ROOT / 'research/code-study/packages/members/__WITHOUT_MULTI_PLUGINCODE__/images/background.jpg'
sources = {'background': source,
           'track': ROOT / 'extracted-resources/fairygui/FirstPackage/images/Login_bg_3.png',
           'fill': ROOT / 'extracted-resources/fairygui/FirstPackage/images/Login_bg_4.png'}
assets = {}
names = {}
manifest = []
for name, path in sources.items():
    # Format conversion only; artwork is retained without repainting.
    import io
    buffer = io.BytesIO()
    Image.open(path).save(buffer, format='PNG')
    data = buffer.getvalue()
    digest = hashlib.sha256(data).hexdigest()
    (ASSETS / (digest + '.png')).write_bytes(data)
    assets[name] = digest
    names[digest] = name + '.png'
    manifest.append({'name': name, 'source': path.relative_to(ROOT).as_posix(),
                     'sourceSha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                     'assetId': digest, 'file': digest + '.png'})
(ASSETS / 'asset-names.json').write_text(json.dumps(names, indent=2), 'utf-8')
(OUT / 'asset-manifest.json').write_text(json.dumps(manifest, indent=2), 'utf-8')
nodes = []
def node(id, name, parent, type, x, y, width, height, **extra):
    value = dict(id=id, name=name, parent=parent, type=type, x=x, y=y,
                 width=width, height=height, color='#FFFFFF', active=True,
                 surface='transparent', **extra)
    nodes.append(value)
    value.setdefault('fontSize', 20)
    value.setdefault('text', '')
    return value

root = node('MiniwowLaunch','Launch',None,'Container',0,0,1080,1920,
            templateSourceId='SvStart', templateSourceVersion=7)
root['rectTransform'] = dict(anchorMin=dict(x=0,y=0),anchorMax=dict(x=1,y=1),
    pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=0),sizeDelta=dict(x=0,y=0))
node('LaunchArtwork','Artwork','MiniwowLaunch','Image',0,0,1080,1920,
     image=assets['background'],imageFit='cover',uvRect=dict(x=0,y=280/1560,width=1,height=1280/1560))
for mode in ['Loading','Update']:
    panel=node(mode,mode,'MiniwowLaunch','Container',0,0,1080,1920)
    panel['active']=mode=='Loading'
    # Keep bottom feedback away from the hero group and viewport edges.
    node(mode+'Text',mode+'Text',mode,'Text',140,1590,800,54,
         text='正在准备冒险' if mode=='Loading' else '正在加载资源',fontSize=32,textAlign='center')
    node(mode+'Track','Track',mode,'Image',130,1660,820,54,image=assets['track'],imageFit='100% 100%')
    node(mode+'Area','FillArea',mode+'Track','Container',31,9,758,34)
    fill=node(mode+'Fill','Fill',mode+'Area','Image',0,0,758,34,image=assets['fill'],
              imageFit='100% 100%',uvRect=dict(x=0,y=0,width=1,height=1))
    fill['rectTransform']=dict(anchorMin=dict(x=0,y=0),anchorMax=dict(x=0,y=1),
        pivot=dict(x=.5,y=.5),anchoredPosition=dict(x=0,y=0),sizeDelta=dict(x=0,y=0))
    node(mode+'Handle','Handle',mode+'Area','Container',0,0,1,34)
    node(mode+'Percent',mode+'Percent',mode,'Text',140,1735,800,44,
         text='0%',fontSize=28,textAlign='center')
node('LaunchTip','Tip','Loading','Text',120,1805,840,42,
     text='每一次出发，都是新的冒险',fontSize=25,textAlign='center')
design=dict(schemaVersion=1,rootId='Canvas',name='MiniwowPortraitUI',width=1080,height=1920,nodes=nodes,animations=[])
payload=dict(root='MiniwowLaunch',design=design,source={'templateId':'SvStart','templateVersion':7,'adaptation':'1080x1920 portrait; original extracted artwork and separate progress graphics'})
(OUT/'launch-payload.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2),'utf-8')
print(json.dumps({'nodes':len(nodes),'assets':len(assets),'resolution':[1080,1920]}))


