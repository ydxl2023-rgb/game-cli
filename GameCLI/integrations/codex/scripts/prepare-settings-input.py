"""Extract reviewed source layout into UnityCLI input; never author native YAML."""
import json
from pathlib import Path
base=Path(__file__).resolve().parents[1]
f=base/'.data/imports/sv-settings'
payload=json.loads((f/'main/template.json').read_text(encoding='utf8'))
d=payload['design']; source=json.loads((f/'source/main/unity-prefab.json').read_text(encoding='utf8'))
nodes={n['id']:n for n in d['nodes']}
# Source opens the default tab in logic. Mirror that initial state in the template.
for n in d['nodes']:
 if n['id']=='Settings0_3':n['active']=True
for i in range(5):
 for suffix,active in [('_0',i!=0),('_1',i==0),('_1_0',True)]:nodes[f'Settings0_1_{i}'+suffix]['active']=active
# Account service modal remains dormant until a product provides its implementation.
for n in d['nodes']:
 if n['name']=='ServerName':n['text']=''
 # The Chinese reference uses the original CN service groups, not both regional variants.
 if n['id']=='Settings0_6_1':n['active']=False
# Absolute preview bounds plus authoritative source anchors.
def convert(n,x=0,y=0):
 x+=n['x'];y+=n['y']
 return dict(id=n['id'],name=n['name'],element=n['type'],active=n.get('active',True),rect=dict(x=x,y=y,width=n['width'],height=n['height']),rectTransform=n['rectTransform'],style=dict(color='#ffffff00' if n.get('surface')=='transparent' and not n.get('image') else n['color'],opacity=n.get('opacity',1),sprite=n.get('image',''),imageType=n.get('imageType','simple')),text=dict(content=n.get('text',''),fontSize=n.get('fontSize',20),color=n['color'],alignment={'left':'Left','center':'Center','right':'Right'}[n.get('textAlign','left')]) if n['type']=='Text' else None,children=[convert(c,x,y) for c in d['nodes'] if c['parent']==n['id']])
assets=[dict(id=h,file='../assets/'+h+'.png',path='Assets/Data/Widgets/Settings/Images/'+h+'.png',usedBy=[n['id'] for n in d['nodes'] if n.get('image')==h]) for h in sorted(set(n['image'] for n in d['nodes'] if n.get('image')))]
for asset in assets:
 borders=[n['imageBorder'] for n in d['nodes'] if n.get('image')==asset['id'] and n.get('imageBorder')]
 if borders:asset['spriteBorder']=[borders[0][k] for k in 'xyzw']
for name,data in [('Settings.visual-ui.json',dict(schemaVersion='1.0',uiName='UISettings',referenceResolution=dict(width=1920,height=1080),nodes=[convert(nodes[payload['root']])])),('asset-manifest.json',dict(schemaVersion='1.0',uiName='UISettings',referenceResolution=dict(width=1920,height=1080),assets=assets)),('template.json',payload)]:
 (f/'main'/name).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
print(len(nodes),len(assets))

