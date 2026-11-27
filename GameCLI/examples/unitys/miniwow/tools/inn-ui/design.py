"""Build the original consumable-page structure with inn-owned assets and explicit local meal controls."""
from pathlib import Path
import json,shutil,hashlib
ROOT=Path(__file__).resolve().parents[2]; OUT=Path(__file__).parent
layout=json.loads((ROOT/'tools/hospital-ui/UIHomeHospital.json').read_text(encoding='utf8'))
layout['uiName']='UIInn'; root=layout['nodes'][0];root['id']=root['name']='UIInn'
common=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/CommonTextConfig.bytes').read_text())['data']
lang=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/LanguageConfig.bytes').read_text())['data']
def text(id): return lang[common[str(id)]['Context']]
def node(name,kind,x,y,w,h,children=None):
 return dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])
def label(name,id,x,y,w,h):
 n=node(name,'Text',x,y,w,h);n['text']=dict(content=text(id) if id else '',fontSize=28,alignment='center',color='#F7E7C4');return n
def traverse(n):
 yield n
 for c in n.get('children',[]): yield from traverse(c)
for n in traverse(root):
 if n['name']=='Priest': n['id']=n['name']='Innkeeper'
 if n['name']=='NpcTitle': n['text']['content']=text(30501)
 if n['name']=='Treatment':
  n['id']=n['name']='Energy';n['rect'].update(x=250,y=1440,width=580,height=45);n['text']['fontSize']=25;n['text']['color']='#392B20'
 if n['name']=='FlyingMaterial': n['rect']['y']=1545
# Actual material controls remain in the same original consumption page regions.
business=[]; retained=[]
for n in root['children']:
 if n['name'] in ['BonusPanel','ProductPanel','MaterialPanel','FlyingMaterial','CraftHint']:
  business.append(n)
 else: retained.append(n)
retained.append(node('BusinessVisible','Container',0,0,1080,1920,business))
retained.append(node('RepairVisible','Container',100,950,880,400,[label('RepairHint',30512,130,970,820,100),node('RepairClick','Button',240,1130,600,110,[label('RepairCost',0,240,1130,600,110)])]))
retained.append(node('RestVisible','Container',230,1300,620,100,[node('RestClick','Button',230,1300,620,100,[label('RestLabel',30507,230,1300,620,100)])]))
# Keep foreground controls after the business panel in draw order.
root['children']=retained
for n in traverse(root):
 if n['name'] in ['Gem','TaskText']: n['text']['color']='#F7E7C4'
 if n['name'] in ['RestLabel','RepairCost','ClaimLabel']: n['text']['color']='#392B20'
manifest=json.loads((ROOT/'tools/hospital-ui/UIHomeHospital.materials.json').read_text())
manifest['uiName']='UIInn'
imports=[]
for a in manifest['assets']:
 if 'Hospital/Resources/' in a['path']: a['path']=a['path'].replace('Hospital/Resources/','Inn/Resources/')
 if a['id']=='HpMaterial_1':
  a['id']='ApMaterial_1';a['file']='../../extracted-resources/fairygui/CommonIcon/ApMaterial_1.png';a['path']='Assets/Data/Widgets/Common/Icons/ApMaterial_1.png'
 if a['id']=='HpItem_1':
  a['id']='ApItem_1';a['file']='../../extracted-resources/fairygui/CommonIcon/ApItem_1.png';a['path']='Assets/Data/Widgets/Common/Icons/ApItem_1.png'
 if a['id']=='common_bg_11': a['usedBy']+=['RepairClick','RestClick','ClaimClick']
manifest['assets']=[a for a in manifest['assets'] if a['id']!='cons_bg_7']
manifest['assets'].append(dict(id='small_person',file='../../extracted-resources/fairygui/CommonIcon/small_person.png',path='Assets/Data/Widgets/Common/Icons/small_person.png',usedBy=['CapacityIcon']))
for name in ['e_ui_bg_hotel_1','npc_hotel']:
 folder=ROOT/'extracted-resources/assets/data/spines'/name
 dst=ROOT/'client/Assets/Data/Widgets/Inn/Resources'/name;dst.mkdir(parents=True,exist_ok=True)
 for suffix in ['.png','.atlas.txt','.skel.bytes']:
  src=folder/(name+suffix);target=dst/src.name;shutil.copyfile(src,target)
  if suffix=='.atlas.txt':target.write_text(target.read_text(encoding='utf8').replace('NPC_hotel.png','npc_hotel.png'),encoding='utf8')
  imports.append(dict(source=str(src.relative_to(ROOT)),destination=str(target.relative_to(ROOT/'client')),sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
for a in manifest['assets']:
 src=(OUT/a['file']).resolve();imports.append(dict(source=str(src.relative_to(ROOT)),destination=a['path'],sha256=hashlib.sha256(src.read_bytes()).hexdigest(),uses=a['usedBy']))
(OUT/'UIInn.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf8')
(OUT/'UIInn.materials.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf8')
