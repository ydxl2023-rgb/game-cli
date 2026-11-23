"""Extract mall cards and compose shared ItemSlot through UnityCLI only."""
import json, pathlib, re, subprocess
project=pathlib.Path('G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
report=pathlib.Path('reports/shop-item-slot'); report.mkdir(parents=True,exist_ok=True)
def run(args):
 p=subprocess.run([cli,'--project',str(project),'--format','json',*args],capture_output=True,encoding='utf-8-sig')
 try:v=json.loads(p.stdout)
 except Exception: raise RuntimeError(p.stdout+p.stderr)
 with (report/'operations.jsonl').open('a',encoding='utf-8') as f:f.write(json.dumps({'args':args,'result':v})+'\n')
 assert v['ok'],v
 return v['data']
def field(asset,path,component,name,value):return run(['modify','--asset',asset,'--path',path,'--component',component,'--property',name,'--value',str(value)])
def attach(parent,path,child,name):
 args=['prefab','attach','--parent-prefab',parent,'--parent-path',path,'--child-prefab',child,'--child-name',name,'--replace','--no-scene-replace']
 run(args+['--dry-run']);run(args)
def bind(asset,root,fields):
 field(asset,root,'Metadata','metadatas.Array.size',len(fields))
 for i,(name,sync,path,comp) in enumerate(fields):
  prefix=f'metadatas.Array.data[{i}]'
  for k,v in [('name',name),('type',sync),('targets.Array.size',1),('components.Array.size',1),('targets.Array.data[0]','@'+path),('components.Array.data[0]','@'+path+'#'+comp)]:field(asset,root,'Metadata',prefix+'.'+k,v)
if all((project/f'Assets/Data/Widgets/Common/Micros/Shop{k}Item.prefab').exists() for k in ['Recharge','Gift']):
 raise SystemExit('Both cards already exist. Review the current composition instead of rerunning this one-time migration.')
source=json.loads(pathlib.Path('.data/imports/sv-shop/bindings.json').read_text())
contracts=[]
for kind,cell in [('Recharge','ScrollCell'),('Gift','GridCell')]:
 page=f'Assets/Data/Widgets/Shop/Shop{kind}/Shop{kind}.prefab'
 src=f'Shop{kind}/Animator/ScrollView/Viewport/Grid/{cell}'
 card=f'Assets/Data/Widgets/Common/Micros/Shop{kind}Item.prefab'
 # Preserve exact pre-change sources for review, without touching Unity assets.
 for p in [page,page+'.meta']:(report/(kind+'-'+pathlib.Path(p).name)).write_bytes((project/p).read_bytes())
 args=['prefab','extract','--prefab',page,'--path',src,'--dst',card]
 if not (project/card).exists():
  run(args+['--dry-run']);run(args)
 inspection=run(['prefab','inspect','--prefab',card,'--recursive'])
 root=inspection['nodes'][0]['path']
 field(card,root,'RectTransform','m_AnchorMin','0.5,0.5');field(card,root,'RectTransform','m_AnchorMax','0.5,0.5');field(card,root,'RectTransform','m_AnchoredPosition','0,0')
 child='GoodsIcon' if kind=='Recharge' else 'ItemInfo'
 attach(card,root+'/RechargeField','Assets/Data/Widgets/Common/Micros/ItemSlot/ItemSlot.prefab',child)
 slot=root+'/RechargeField/'+child
 field(card,slot,'RectTransform','m_AnchoredPosition','-18.97,46.06' if kind=='Recharge' else '3.759,27')
 field(card,slot,'Image','m_Enabled','false')
 prefix=slot+'/BagIcon1/IconVisible'
 for node in ['ItemInfo/ItemHui','ItemInfo/ItemRare']:
  field(card,prefix+'/'+node,'Image','m_Enabled','false')
 if kind=='Recharge':
  field(card,slot,'RectTransform','m_SizeDelta','300,180')
  field(card,prefix+'/ItemInfo/ItemIcon','RectTransform','m_SizeDelta','300,180')
  field(card,slot+'/BagIcon1','Image','m_Enabled','false')
  field(card,prefix+'/NumVisible/ItemNumber','Text','m_Enabled','false')
 if kind=='Gift':
  field(card,prefix+'/NumVisible/ItemNumber','Text','m_Color','0.2,0.2,0.2,1')
 # A card owns the only click target, including nested item art.
 nested=run(['prefab','inspect','--prefab',card,'--recursive'])
 for n in nested['nodes']:
  if not n['path'].startswith(slot+'/'):continue
  for c in n['components']:
   if c['type']=='UnityEngine.UI.Button':run(['components','remove','--asset',card,'--path',n['path'],'--component','Button'])
   if c['type'] in ['UnityEngine.UI.Image','UnityEngine.UI.Text']:field(card,n['path'],c['type'],'m_RaycastTarget','false')
 spec=next(s for s in source['specs'] if s['name']=='Shop'+kind+'Item')
 fields=[]
 for f in spec['fields']:
  if f['name']=='count':continue
  if f['name']=='icon':fields.append(('item','SyncMetadata',slot,'Oathx.Widget.UIItemSlot'))
  else:fields.append((f['name'],f['sync'],f['targets'][0].replace(src,root),f['component']))
 bind(card,root,fields)
 attach(page,src.rsplit('/',1)[0],card,cell)
 view=f'Shop{kind}/Animator/ScrollView/Viewport'
 field(page,view,'FancyGridScrollView','cellPrefab','@'+src+'#FancyGridScrollViewCell')
 field(page,f'Shop{kind}','Metadata','metadatas.Array.data[2].targets.Array.data[0]','@'+src)
 field(page,f'Shop{kind}','Metadata','metadatas.Array.data[2].components.Array.data[0]','@'+src+'#RectTransform')
 contracts.append({'kind':kind,'page':page,'card':card,'root':root,'cell':src,'slot':slot,'fields':fields})
 (report/'bindings.json').write_text(json.dumps(contracts,indent=2),encoding='utf-8')
 print(kind+' card extracted and rebound.',flush=True)
run(['save'])

