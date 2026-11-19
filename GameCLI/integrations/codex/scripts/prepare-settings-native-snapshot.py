"""Adapt the CLI-exported snapshot using source UGUI values for one atomic CLI restore."""
import json
import re
import hashlib
from pathlib import Path
from PIL import Image

base=Path(__file__).resolve().parents[1]
folder=base/'.data/imports/sv-settings'
manifest=json.loads((folder/'native-snapshot/manifest.json').read_text(encoding='utf8'))
snapshot=json.loads((folder/'native-snapshot'/manifest['entries'][0]['file']).read_text(encoding='utf8'))
source=json.loads((folder/'source/main/unity-prefab.json').read_text(encoding='utf8'))
design=json.loads((folder/'main/template.json').read_text(encoding='utf8'))['design']
contract=json.loads((folder/'bindings.json').read_text(encoding='utf8'))
nodes={n['id']:n for n in design['nodes']}
paths={}
for n in snapshot['prefab']['nodes']:
    paths[n['id']]=paths[n['parentId']]+'/'+n['name'] if n['parentId'] else n['name']
nativeNodes={paths[n['id']]:n for n in snapshot['prefab']['nodes']}

def native(path):
    n=nodes['Settings'+path.replace('/','_')]
    return native(n['parent'][8:].replace('_','/'))+'/'+n['name'] if n['parent'] else n['name']

def designPath(n):
    return designPath(nodes[n['parent']])+'/'+n['name'] if n['parent'] else n['name']

allowed={designPath(n) for n in design['nodes']}
snapshot['prefab']['nodes']=[n for n in snapshot['prefab']['nodes'] if paths[n['id']] in allowed]

def flatten(props):
    result={}
    for key,value in props.items():
        if isinstance(value,list) and value and all(isinstance(v,(int,float)) for v in value):
            axes='rgba' if 'Color' in key else 'xyzw'
            result.update({key+'.'+axis:v for axis,v in zip(axes,value)})
        elif isinstance(value,dict) and not any(k in value for k in ['nodeId','assetPath','deferredSlot']):
            result.update(flatten({key+'.'+k:v for k,v in value.items()}))
        else:result[key]=value
    return result

def attach(node,typ,props):
    fullname='UnityEngine.UI.'+typ if typ not in ['CanvasRenderer','CanvasGroup'] else 'UnityEngine.'+typ
    node['components']=[c for c in node['components'] if c['type']!=fullname]
    node['components'].append(dict(id='Settings'+typ,type=fullname,properties=flatten(props)))

for n in snapshot['prefab']['nodes']:
    n['components']=[c for c in n['components'] if c['type'].startswith('UnityEngine.')]
    for c in n['components']:
        c['properties']=flatten(c['properties'])
        c['properties']={k:v for k,v in c['properties'].items() if 'm_PersistentCalls' not in k}

for original in source['nodes']:
    identifier='Settings'+original['path'].replace('/','_')
    if identifier not in nodes:continue
    n=nativeNodes[native(original['path'])];view=nodes[identifier]
    n['active']=view.get('active',True)
    # Preserve native anchors; flattened web bounds are insufficient for driven sliders.
    if original['parent']:
        n['rect']={key:[original['rectTransform'][key][axis] for axis in 'xy'] for key in ['anchorMin','anchorMax','pivot','anchoredPosition','sizeDelta']}
    for c in original['components']:
        data=next(iter(json.loads(c['json']).values())) if c.get('json') else {}
        name=c['type'].split(',')[0].split('.')[-1]
        if 'm_FontData' in data:
            props={k:data[k] for k in ['m_Enabled','m_Color','m_Maskable'] if k in data}
            props.update({'m_Text':view.get('text',''),'m_RaycastTarget':False})
            props['m_FontData']={k:v for k,v in data['m_FontData'].items() if isinstance(v,(int,float,bool,str))}
            font=next(r for r in c['references'] if r['property']=='m_FontData.m_Font')
            props['m_FontData']['m_Font']={'assetPath':'Assets/Data/Widgets/Common/Resources/'+(Path(font['path']).name if font['path'].startswith('Assets/') else 'msyh.ttc')}
            attach(n,'Text',props)
        if name in ['Image','CustomImage','RawImage']:
            graphic=next((c for c in n['components'] if c['type']=='UnityEngine.UI.Image'),None)
            if graphic:
                graphic['properties'].update(flatten({k:data[k] for k in ['m_Color','m_Enabled','m_Maskable','m_RaycastTarget'] if k in data}))
            if graphic and view.get('image'):
                graphic['properties']['m_Sprite']={'assetPath':'Assets/Data/Widgets/Settings/Images/'+view['image']+'.png'}
                graphic['properties']['m_Type']=1 if view.get('imageType')=='sliced' else 0
        if name in ['HorizontalLayoutGroup','VerticalLayoutGroup','GridLayoutGroup','LayoutElement','ContentSizeFitter']:
            props={k:v for k,v in data.items() if k.startswith('m_') and k not in ['m_EditorHideFlags','m_Name','m_EditorClassIdentifier']}
            attach(n,name,props)
        if name=='OutlineEx':
            attach(n,'Outline',{'m_EffectColor':data['OutlineColor'],'m_EffectDistance':{'x':data['OutlineWidth'],'y':-data['OutlineWidth']},'m_UseGraphicAlpha':True})
        if name in ['Slider','ScrollRect']:
            keys=['m_MinValue','m_MaxValue','m_WholeNumbers','m_Direction','m_Value','m_Interactable'] if name=='Slider' else ['m_Horizontal','m_Vertical','m_MovementType','m_Inertia','m_ScrollSensitivity','m_DecelerationRate','m_Elasticity']
            props={k:data[k] for k in keys if k in data}
            for r in c['references']:
                if r['property'] in ['m_FillRect','m_HandleRect','m_Viewport','m_Content'] and r.get('hierarchy'):
                    props[r['property']]={'nodeId':nativeNodes[native(r['hierarchy'])]['id'],'componentType':'UnityEngine.RectTransform'}
            attach(n,name,props)
        if name=='Mask':attach(n,'Mask',{'m_ShowMaskGraphic':data.get('m_ShowMaskGraphic',False)})
        if name=='RectMask2D':attach(n,'RectMask2D',{k:data[k] for k in ['m_Padding','m_Softness'] if k in data})

for field in contract['specs'][0]['fields']:
    if field['component']!='Button':continue
    for path in field['paths']:
        n=nativeNodes[native(path)]
        if not any(c['type']=='UnityEngine.UI.Image' for c in n['components']):
            attach(n,'Image',{'m_Color':dict(r=0,g=0,b=0,a=0),'m_RaycastTarget':True})
        attach(n,'Button',{'m_Interactable':True,'m_Transition':0,'m_TargetGraphic':{'nodeId':n['id'],'componentType':'UnityEngine.UI.Image'}})

snapshot.pop('widgetBindings',None)
snapshot['diagnostics']=[]
snapshot['visualTextures']=[]
assetNames=json.loads((folder/'assets/asset-names.json').read_text(encoding='utf8'))
for identifier,sourcePath in assetNames.items():
    if not any(n.get('image')==identifier for n in design['nodes']):continue
    body=(folder/'assets'/(identifier+'.png')).read_bytes()
    width,height=Image.open(folder/'assets'/(identifier+'.png')).size
    meta=Path('G:/Works/SV/Development/Project',sourcePath+'.meta').read_text(encoding='utf8')
    border=re.search(r'spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}',meta)
    pixels=re.search(r'spritePixelsToUnits: ([\d.]+)',meta)
    snapshot['visualTextures'].append(dict(path='Assets/Data/Widgets/Settings/Images/'+identifier+'.png',deliverySha256=hashlib.sha256(body).hexdigest(),deliveryWidth=width,deliveryHeight=height,settings=dict(spriteMode=1,spritePixelsToUnits=float(pixels[1]) if pixels else 100,spriteBorder=dict(zip('xyzw',map(float,border.groups()))) if border else dict.fromkeys('xyzw',0),wrapMode=1)))
(folder/'settings-native.json').write_text(json.dumps(snapshot,ensure_ascii=False,indent=2),encoding='utf8')
print('Prepared',len(snapshot['prefab']['nodes']),'native nodes with explicit source UGUI properties')
