"""Extract existing badge visuals and build a resumable official UnityCLI migration plan.
Reads Unity YAML only; all Unity asset writes go through UnityCLI.
"""
import json,re,copy,hashlib,subprocess,sys
from pathlib import Path
import yaml
from PIL import Image
BASE=Path(__file__).resolve().parents[1]
PROJECT=BASE.parents[1]/'examples/unitys/development/client'
OUT=BASE/'.data/home-badges'; OUT.mkdir(exist_ok=True)
ASSET='Assets/Data/Widgets/Home/Home.prefab'
CLI='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
def read(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def save(p,v): p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def build():
 if (OUT/'progress.json').exists():
  raise RuntimeError('This one-time baseline migration has started. Preserve its plan and resume with --run; new projects restore the packaged prefab instead.')
 snap=read(BASE/'.data/home-entry-export/prefabs/9cb5e039e72efa54b834c094bf738a09.json')
 design=read(BASE/'.data/imports/sv-home/fidelity-candidate/template.json')['design']; paths={}
 for n in design['nodes']: paths[n['id']]=(paths[n['parent']]+'/' if n.get('parent') else '')+n['name']
 nodes=snap['prefab']['nodes']; sp={}; bypath={}
 for n in nodes:
  sp[n['id']]=(sp[n['parentId']]+'/' if n.get('parentId') else '')+n['name']; bypath[sp[n['id']]]=n
 # Resolve original external references, without modifying serialized asset text.
 docs={}
 for fid,body in re.findall(r'--- !u!\d+ &(-?\d+)\s*\n(.*?)(?=\n--- !u!|\Z)',(PROJECT/ASSET).read_text(encoding='utf-8'),re.S):
  docs[int(fid)]=yaml.safe_load(body)
 transforms={k:next(iter(v.values())) for k,v in docs.items() if 'RectTransform' in v or 'Transform' in v}
 def path(t):
  tr=transforms[t]; go=docs[tr['m_GameObject']['fileID']]['GameObject']; parent=tr['m_Father']['fileID']
  return (path(parent)+'/' if parent in transforms else '')+go['m_Name']
 original={path(k):docs[t['m_GameObject']['fileID']]['GameObject'] for k,t in transforms.items()}
 assets={}
 for m in (PROJECT/'Assets').rglob('*.meta'):
  match=re.search(r'^guid: (\w+)',m.read_text(encoding='utf-8-sig'),re.M)
  if match: assets[match[1]]=m.relative_to(PROJECT).as_posix()[:-5]
 canonical={}
 for guid,asset in list(assets.items()):
  if asset.endswith('.png'):
   digest=hashlib.sha256((PROJECT/asset).read_bytes()).hexdigest()
   canonical.setdefault(digest,asset);assets[guid]=canonical[digest]
 for p,n in bypath.items():
  comp=[next(iter(docs[x['component']['fileID']].values())) for x in original[p]['m_Component'] if x['component']['fileID'] in docs]
  for c in n['components']:
   source=next((v for v in comp if ('m_Controller' in v if c['type'].endswith('.Animator') else 'm_Sprite' in v if c['type'].endswith('.Image') else False)),None)
   if source:
    for k,v in source.items():
     if isinstance(v,dict) and v.get('guid') in assets: c['properties'][k]={'assetPath':assets[v['guid']]}
 states=read(BASE/'.data/imports/sv-home/fidelity-candidate/presentation.json')['states']
 ops=[]
 def command(*args):
  args=list(map(str,args))
  if '--asset' in args and '/Badges/' in args[args.index('--asset')+1]:
   name=Path(args[args.index('--asset')+1]).stem
   args=[name if a=='Badge' else a.replace('@Badge#','@'+name+'#') if a.startswith('@Badge#') else '@'+name if a=='@Badge' else a for a in args]
  ops.append(args)
 def field(asset,path,typ,key,value): command('modify','--asset',asset,'--path',path,'--component',typ,'--property',key,'--value',value)
 def add(asset,path,typ): command('components','add','--asset',asset,'--path',path,'--component',typ)
 def metadata(asset,path,idx,name,typ,targets):
  prefix=f'metadatas.Array.data[{idx}]'
  for k,v in [('name',name),('type',typ),('targets.Array.size',len(targets)),('components.Array.size',len(targets))]:field(asset,path,'Metadata',prefix+'.'+k,v)
  for j,(p,c) in enumerate(targets):
   field(asset,path,'Metadata',prefix+f'.targets.Array.data[{j}]','@'+p)
   field(asset,path,'Metadata',prefix+f'.components.Array.data[{j}]','@'+p+'#'+c)
 variants={}; mounts=[]
 for st in states:
  if st['name']=='TaskEffectTips': continue
  p=paths[st['node']]; root=bypath[p]; children=[copy.deepcopy(n) for n in nodes if n['id']==root['id'] or n['id'].startswith(root['id']+'/')]
  for n in children:
   n['id']='root'+n['id'][len(root['id']):]
   n['parentId']='root'+n['parentId'][len(root['id']):] if n['parentId'] and n['parentId'].startswith(root['id']) else None
   for c in n['components']:
    if 'm_RaycastTarget' in c['properties']:c['properties']['m_RaycastTarget']=False
    for prop,value in list(c['properties'].items()):
     if isinstance(value,list) and value and all(isinstance(v,(float,int)) for v in value):
      del c['properties'][prop]
      for axis,v in zip('rgba' if 'Color' in prop else 'xyzw',value):c['properties'][prop+'.'+axis]=v
  r=children[0]; r.update(name='Badge',active=True,parentId=None,siblingIndex=0,localPosition=[0,0,0],localScale=[1,1,1],rect={'anchorMin':[0,0],'anchorMax':[1,1],'pivot':[.5,.5],'anchoredPosition':[0,0],'sizeDelta':[0,0]})
  key=hashlib.sha256(json.dumps(children,sort_keys=True).encode()).hexdigest()[:10]
  kind=2 if st['name']=='AdsNode' else 1
  badge='Assets/Data/Widgets/Home/Badges/'+('Advertisement' if kind==2 else 'New')+key+'.prefab'
  if key not in variants:
   variants[key]=badge; data={'schemaVersion':'ui-template/1','resources':{},'prefab':{'name':'Badge','rootNodeId':'root','nodes':children}}
   textures=set(re.findall(r'"assetPath": "([^"]+\.png)"',json.dumps(data)))
   data['visualTextures']=[]
   for texture in sorted(textures):
    w,h=Image.open(PROJECT/texture).size
    settings=yaml.safe_load(Path(str(PROJECT/texture)+'.meta').read_text(encoding='utf-8'))['TextureImporter']
    data['visualTextures'].append({'path':texture,'deliveryWidth':w,'deliveryHeight':h,'deliverySha256':hashlib.sha256((PROJECT/texture).read_bytes()).hexdigest(),'settings':{'spriteMode':1,'spritePixelsToUnits':settings['spritePixelsToUnits'],'spriteBorder':settings['spriteBorder'],'wrapMode':1}})
   file=OUT/(key+'.json');save(file,data)
   command('prefab','restore','--json',str(file),'--prefab',badge,'--dry-run')
   command('prefab','restore','--json',str(file),'--prefab',badge)
   add(badge,'Badge','UIActivityBadge')
   guid=re.search(r'guid: (\w+)',(PROJECT/'Assets/Scripts/Systems/Home/VM/VMActivityBadge.cs.meta').read_text())[1]
   field(badge,'Badge','Metadata','className','VMActivityBadge');field(badge,'Badge','Metadata','classGuid',guid)
   field(badge,'Badge','Metadata','metadatas.Array.size',1);metadata(badge,'Badge',0,'visible','SyncVisible',[('Badge','RectTransform')])
  mounts.append({'path':p,'kind':kind,'prefab':badge,'node':st['node']})
 # Create default placements for entries without an authored mount.
 defaults={}
 for kind in [1,2]:
  mount=next(m for m in mounts if m['kind']==kind and ('Duiyuan' in m['path'] if kind==1 else True))
  data=read(OUT/(mount['prefab'].split('/')[-1].removesuffix('.prefab').removeprefix('Advertisement').removeprefix('New')+'.json'))
  root=data['prefab']['nodes'][0];root['rect'].update(anchorMin=[1,1] if kind==1 else [.5,0],anchorMax=[1,1] if kind==1 else [.5,0],anchoredPosition=[-15,-2] if kind==1 else [0,12],sizeDelta=[75,30] if kind==1 else [70,26])
  badge='Assets/Data/Widgets/Home/Badges/'+('NewDefault' if kind==1 else 'AdvertisementDefault')+'.prefab';file=OUT/('default'+str(kind)+'.json');save(file,data)
  command('prefab','restore','--json',str(file),'--prefab',badge,'--dry-run');command('prefab','restore','--json',str(file),'--prefab',badge);add(badge,'Badge','UIActivityBadge')
  field(badge,'Badge','Metadata','className','VMActivityBadge');field(badge,'Badge','Metadata','classGuid',guid);field(badge,'Badge','Metadata','metadatas.Array.size',1);metadata(badge,'Badge',0,'visible','SyncVisible',[('Badge','RectTransform')]);defaults[kind]=badge
 enum=(PROJECT/'Assets/Scripts/Systems/Home/HomeEvents.cs').read_text(encoding='utf-8').split('public enum HomeEntry')[1].split('}')[0]
 names=re.findall(r'^\s*(\w+),?\s*$',enum,re.M);ids={n:i for i,n in enumerate(names)}
 entries=[]
 for index,f in enumerate(snap['widgetBindings']['widgets'][0]['fields']):
  if f['syncType']!='SyncClick':continue
  name=f['name'][:-5]; dest=name[0].upper()+name[1:];p=sp[f['targets'][0]['nodeId']];p=p.rsplit('/',1)[0] if name=='shop' else p
  entries.append({'path':p,'id':ids[dest],'buttons':[(sp[t['nodeId']],'Button') for t in f['targets']],'rootIndex':index,'name':name+'Entry','existing':False})
 for item in read(BASE/'config/home-functions.json'):
  entries.append({'path':paths[item['node']],'id':item['id'],'buttons':[(paths[t['node']],'Button') for t in item['buttons']],'existing':True})
 for m in mounts:
  owners=[e for e in entries if m['path'].startswith(e['path']+'/')];assert owners, m;owner=max(owners,key=lambda e:len(e['path']));m['entryId']=owner['id']
  for n in nodes:
   p=sp[n['id']]
   if p==m['path'] or p.startswith(m['path']+'/'):
    for c in reversed(n['components']):
     if c['type']!='UnityEngine.CanvasRenderer':command('components','remove','--asset',ASSET,'--path',p,'--component',c['type'])
    if any(c['type']=='UnityEngine.CanvasRenderer' for c in n['components']):command('components','remove','--asset',ASSET,'--path',p,'--component','UnityEngine.CanvasRenderer')
 guid=re.search(r'guid: (\w+)',(PROJECT/'Assets/Scripts/Systems/Home/VM/VMActivityEntry.cs.meta').read_text())[1]
 for e in entries:
  p=e['path']
  if not e['existing']:add(ASSET,p,'UIActivityEntry')
  field(ASSET,p,'UIActivityEntry','id',e['id']);field(ASSET,p,'Metadata','className','VMActivityEntry');field(ASSET,p,'Metadata','classGuid',guid);field(ASSET,p,'Metadata','metadatas.Array.size',4)
  if not e['existing']:
   metadata(ASSET,p,0,'openClick','SyncClick',e['buttons']);metadata(ASSET,p,1,'visible','SyncVisible',[(p,'RectTransform')])
  metadata(ASSET,p,2,'interactable','SyncInteractable',e['buttons']);metadata(ASSET,p,3,'badges','SyncActivityBadges',[(p,'UIActivityEntry')])
  field(ASSET,p,'UIActivityEntry','badgeSlots.Array.size',2)
  for j,kind in enumerate([1,2]):
   m=next((m for m in mounts if m['entryId']==e['id'] and m['kind']==kind),None)
   for k,v in [('kind',kind),('anchor','@'+(m['path'] if m else p)+'#RectTransform'),('prefab',m['prefab'] if m else defaults[kind])]:field(ASSET,p,'UIActivityEntry',f'badgeSlots.Array.data[{j}].'+k,v)
  if not e['existing']:metadata(ASSET,'Home',e['rootIndex'],e['name'],'SyncMetadata',[(p,'Metadata')])
 save(OUT/'plan.json',ops);save(BASE/'config/home-entry-badges.json',{'entries':entries,'mounts':mounts,'defaults':defaults,'variants':list(variants.values())})
 print(f'Plan: {len(entries)} entries, {len(mounts)} mounts, {len(variants)} variants, {len(ops)} commands',flush=True)
def already_applied(args):
 if args[0]!='modify':return False
 def option(k):return args[args.index(k)+1]
 try:
  asset=option('--asset'); target=option('--path'); typ=option('--component').split('.')[-1]
  docs={int(fid):yaml.load(body,Loader=yaml.CSafeLoader) for fid,body in re.findall(r'--- !u!\d+ &(-?\d+)\s*\n(.*?)(?=\n--- !u!|\Z)',(PROJECT/asset).read_text(encoding='utf-8'),re.S)}
  transforms={k:next(iter(v.values())) for k,v in docs.items() if 'RectTransform' in v or 'Transform' in v}
  paths={}
  def path(t):
   if t not in paths:
    tr=transforms[t];go=docs[tr['m_GameObject']['fileID']]['GameObject'];parent=tr['m_Father']['fileID']
    paths[t]=(path(parent)+'/' if parent in transforms else '')+go['m_Name']
   return paths[t]
  gos={path(k):t['m_GameObject']['fileID'] for k,t in transforms.items()}
  def component(p,t):
   for item in docs[gos[p]]['GameObject']['m_Component']:
    fid=item['component']['fileID'];doc=docs[fid];name=next(iter(doc));value=doc[name]
    if name==t or name=='MonoBehaviour' and scriptnames.get(value.get('m_Script',{}).get('guid'))==t:return fid,value
   raise KeyError(t)
  _,value=component(target,typ)
  prop=option('--property').replace('.Array.data[','[').replace('.Array.size','.length')
  for token in re.findall(r'[^.\[\]]+',prop):
   value=len(value) if token=='length' else value[int(token)] if token.isdigit() else value[token]
  raw=option('--value')
  if raw.startswith('@'):
   p,_,t=raw[1:].partition('#');expected=component(p,t)[0] if t else gos[p]
   return value.get('fileID')==expected
  if raw.startswith('Assets/'):
   guid=re.search(r'^guid: (\w+)',Path(str(PROJECT/raw)+'.meta').read_text(encoding='utf-8'),re.M)[1]
   return isinstance(value,dict) and value.get('guid')==guid
  return str(value).lower()==raw.lower()
 except (KeyError,IndexError,TypeError,AttributeError,FileNotFoundError):return False
scriptnames={}
def run():
 for meta in list((PROJECT/'Assets/Scripts').rglob('*.cs.meta'))+list((PROJECT/'LocalPackages/com.oathx.widget').rglob('*.cs.meta')):
  guid=re.search(r'^guid: (\w+)',meta.read_text(encoding='utf-8-sig'),re.M)
  if guid:scriptnames[guid[1]]=meta.name[:-8]
 ops=read(OUT/'plan.json');progress=OUT/'progress.json';start=read(progress)['next'] if progress.exists() else 0
 skipped=0
 for i in range(start,len(ops)):
  if already_applied(ops[i]):skipped+=1
  else:
   result=subprocess.run([CLI,'--project',str(PROJECT),'--format','json',*ops[i]],capture_output=True,encoding='utf-8-sig')
   try:r=json.loads(result.stdout)
   except:raise RuntimeError(f'{i}: {result.stdout} {result.stderr}')
   if not r.get('ok'):raise RuntimeError(f'{i}: {ops[i]} {r}')
  save(progress,{'next':i+1})
  if i%25==0:print(f'{i+1}/{len(ops)}, skipped {skipped}',flush=True)
if __name__=='__main__':
 if '--run' in sys.argv:run()
 else:build()
