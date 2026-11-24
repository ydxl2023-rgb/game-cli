"""Recover source scene transforms and referenced render data without running game code."""
from pathlib import Path
import sys,json,hashlib,collections,re,sqlite3
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/python-libs'))
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
OUT=Path(__file__).parent
DATA=OUT/'data'; DATA.mkdir(exist_ok=True)
source=ROOT/'wxecf0e106809a477d/game-cache/StreamingAssets'
env=UnityPy.Environment();origins={}
for p in source.glob('*.bundle'):
 bundle=env.load_file(str(p))
 def register(f):
  if hasattr(f,'objects'):
   if f.name:origins[f.name.lower()]=p
   return
  if hasattr(f,'files'):
   for child in f.files.values():register(child)
 register(bundle)
files={f.name.lower():f for f in env.assets}
scene=files['buildplayer-building_sdc']
cache={};meshes={};materials={};textures={};issues=[];receipts=[]
catalog=sqlite3.connect(ROOT/'research/resource-catalog/catalog.sqlite')
catalog_assets={}
catalog_sources={r['id']:r for (payload,) in catalog.execute('SELECT payload FROM sources') for r in [json.loads(payload)]}
for (payload,) in catalog.execute('SELECT payload FROM assets'):
 r=json.loads(payload)
 catalog_assets[(r.get('serialized_file'),r.get('path_id'))]=r
catalog.close()
def tree(o):
 k=(o.assets_file.name,o.path_id)
 if k not in cache:cache[k]=o.read_typetree()
 return cache[k]
def key(o):return hashlib.sha256((o.assets_file.name+':'+str(o.path_id)).encode()).hexdigest()[:20]
def resolve(f,p):
 if p['m_PathID']==0:return None
 if p['m_FileID']:
  path=f.externals[p['m_FileID']-1].path
  name=path.rsplit('/',1)[-1].lower()
  if name not in files:
   issues.append(dict(kind='external-missing',file=f.name,reference=path,pathId=str(p['m_PathID'])))
   return None
  f=files[name]
 return f.objects.get(p['m_PathID'])
def receipt(o,kind,path):
 p=origins.get(o.assets_file.name.lower());srcsha=hashlib.sha256(p.read_bytes()).hexdigest() if p else None
 original=catalog_assets[(o.assets_file.name,str(o.path_id))]
 assert catalog_sources[original['source_id']]['sha256']==srcsha
 aid=original['id']
 receipts.append(dict(id=key(o),sourceAssetId=aid,type=kind,name=tree(o).get('m_Name',''),source=p.relative_to(ROOT).as_posix() if p else None,sourceSha256=srcsha,serializedFile=o.assets_file.name,pathId=str(o.path_id),output=path,sha256=hashlib.sha256((OUT/path).read_bytes()).hexdigest()))
def texture(o):
 if not o:return None
 k=key(o)
 if k not in textures:
  d=tree(o);path=f'data/{k}.png';o.read().image.save(OUT/path)
  textures[k]=dict(id=k,name=d['m_Name'],file=path)
  receipt(o,'Texture2D',path)
 return k
def material(o):
 if not o:return None
 k=key(o)
 if k not in materials:
  d=tree(o);props=d['m_SavedProperties'];tex=dict(props['m_TexEnvs']);colors=dict(props['m_Colors']);floats=dict(props['m_Floats'])
  main=next((tex[name] for name in ['_BaseMap','_MainTex'] if name in tex and tex[name]['m_Texture']['m_PathID']),None)
  shader=resolve(o.assets_file,d['m_Shader']);sd=tree(shader) if shader else {}
  shadername=sd.get('m_ParsedForm',{}).get('m_Name',sd.get('m_Name','unknown'))
  val=dict(id=k,name=d['m_Name'],shader=shadername,texture=texture(resolve(o.assets_file,main['m_Texture'])) if main else None,scale=main['m_Scale'] if main else dict(x=1,y=1),offset=main['m_Offset'] if main else dict(x=0,y=0),colors=colors,floats=floats,queue=d.get('m_CustomRenderQueue',-1),keywords=d.get('m_ShaderKeywords',''))
  materials[k]=val;path=f'data/{k}.material.json';(OUT/path).write_text(json.dumps(val,ensure_ascii=False),'utf8');receipt(o,'Material',path)
 return k
def mesh(o):
 if not o:return None
 k=key(o)
 if k not in meshes:
  h=MeshHandler(o.read());h.process()
  def flat(values,n):return [float(x) for v in values for x in v[:n]] if values is not None else []
  val=dict(id=k,name=tree(o)['m_Name'],vertices=flat(h.m_Vertices,3),normals=flat(h.m_Normals,3),uv=flat(h.m_UV0,2),uv2=flat(h.m_UV1,2),colors=flat(h.m_Colors,4),submeshes=[[int(x) for tri in sub for x in tri] for sub in h.get_triangles()])
  path=f'data/{k}.mesh.json';(OUT/path).write_text(json.dumps(val,separators=(',',':')),'utf8');meshes[k]=dict(id=k,name=val['name'],file=path,vertices=len(val['vertices'])//3,submeshes=len(val['submeshes']));receipt(o,'Mesh',path)
 return k
nodes=[]
trs={o.path_id:o for o in scene.objects.values() if o.type.name=='Transform'}
for tid,o in trs.items():
 d=tree(o);go=resolve(scene,d['m_GameObject']);g=tree(go)
 node=dict(id=str(tid),sourceGameObjectId=str(go.path_id),name=g['m_Name'],active=g['m_IsActive'],layer=g['m_Layer'],parent=str(d['m_Father']['m_PathID']),position=d['m_LocalPosition'],rotation=d['m_LocalRotation'],scale=d['m_LocalScale'])
 components={}
 for c in g['m_Component']:
  co=resolve(scene,c['component'])
  if co:components[co.type.name]=co
 if 'MeshFilter' in components:
  filt=tree(components['MeshFilter']);p=filt['m_Mesh']
  if p['m_FileID'] and scene.externals[p['m_FileID']-1].path=='Library/unity default resources' and p['m_PathID'] in [10202,10206]:
   node['builtinMesh']={10202:'Cube',10206:'Cylinder'}[p['m_PathID']]
  else:node['mesh']=mesh(resolve(scene,p))
 if 'MeshRenderer' in components:
  renderer=tree(components['MeshRenderer']);node['materials']=[material(resolve(scene,p)) for p in renderer['m_Materials']];node['rendererEnabled']=renderer['m_Enabled']
 if 'Camera' in components:node['camera']=tree(components['Camera'])
 if 'Light' in components:node['light']=tree(components['Light'])
 node['omittedComponents']=[kind for kind in components if kind not in ['Transform','MeshFilter','MeshRenderer','Camera','Light']]
 nodes.append(node)
# Original runtime attaches InitialModelPath to each empty Model anchor before unlock.
building_configs=json.loads((OUT/'building-config.json').read_text('utf8'))
container={}
for f in list(files.values()):
 for o in f.objects.values():
  if o.type.name=='AssetBundle':
   for path,entry in tree(o)['m_Container']:
    container[path.lower()]=(f,entry['asset'])
dynamic=[]
for config in building_configs:
 if not config['initialModelPath']:continue
 building_root=next(n for n in nodes if n['name']=='BuildingRoot')
 building=next(n for n in nodes if n['parent']==building_root['id'] and n['name'].endswith('_'+str(config['id'])))
 anchor=next(n for n in nodes if n['parent']==building['id'] and n['name']=='Model')
 path='assets/data/models/building/'+config['initialModelPath'].lower()+'.prefab'
 pointer=container.get(path)
 if pointer is None:raise ValueError('Missing initial building '+path)
 root=resolve(*pointer);g=tree(root)
 transform=next(resolve(root.assets_file,c['component']) for c in g['m_Component'] if resolve(root.assets_file,c['component']).type.name=='Transform')
 prefix='building-'+str(config['id'])+'-'
 def append_transform(o,parent,isroot=False):
  d=tree(o);go=resolve(o.assets_file,d['m_GameObject']);g=tree(go)
  node=dict(id=prefix+key(o),sourceGameObjectId=prefix+str(go.path_id),name=g['m_Name'],active=g['m_IsActive'],layer=g['m_Layer'],parent=parent,position=d['m_LocalPosition'],rotation=d['m_LocalRotation'],scale=d['m_LocalScale'])
  components={}
  for c in g['m_Component']:
   co=resolve(go.assets_file,c['component'])
   if co:components[co.type.name]=co
  if 'MeshFilter' in components:
   co=components['MeshFilter'];node['mesh']=mesh(resolve(co.assets_file,tree(co)['m_Mesh']))
  if 'MeshRenderer' in components:
   co=components['MeshRenderer'];r=tree(co);node['materials']=[material(resolve(co.assets_file,p)) for p in r['m_Materials']];node['rendererEnabled']=r['m_Enabled']
  node['omittedComponents']=[kind for kind in components if kind not in ['Transform','MeshFilter','MeshRenderer']]
  nodes.append(node)
  for child in d['m_Children']:append_transform(resolve(o.assets_file,child),node['id'])
 append_transform(transform,anchor['id'],True)
 dynamic.append(dict(buildingId=config['id'],name=config['name'],sourcePrefab=path,parent=anchor['id'],state='initial / locked appearance; no gameplay state implied'))
payload=dict(schemaVersion=1,sourceScene='Building_sdc',nodes=nodes,dynamicBuildings=dynamic,meshes=list(meshes.values()),materials=list(materials.values()),textures=list(textures.values()),issues=issues)
(OUT/'scene.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2),'utf8')
(OUT/'asset-manifest.json').write_text(json.dumps(receipts,ensure_ascii=False,indent=2),'utf8')
summary=dict(nodes=len(nodes),renderers=sum('materials' in n for n in nodes),meshes=len(meshes),materials=len(materials),textures=len(textures),issues=len(issues),shaders=dict(collections.Counter(m['shader'] for m in materials.values())))
(OUT/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),'utf8');print(json.dumps(summary,ensure_ascii=False))
