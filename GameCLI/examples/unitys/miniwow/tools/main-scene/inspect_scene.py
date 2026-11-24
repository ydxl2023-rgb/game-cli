from pathlib import Path
import sys,json,collections
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/python-libs'))
import UnityPy
source=ROOT/'wxecf0e106809a477d/game-cache/StreamingAssets'
env=UnityPy.Environment()
for p in source.glob('*.bundle'):env.load_file(str(p))
files={f.name.lower():f for f in env.assets}
scene=files['buildplayer-building_sdc']
trees={o.path_id:o.read_typetree() for o in scene.objects.values() if o.type.name in ['GameObject','Transform','Camera','MeshFilter','MeshRenderer']}
gos={o.path_id:trees[o.path_id] for o in scene.objects.values() if o.type.name=='GameObject'}
trs={o.path_id:trees[o.path_id] for o in scene.objects.values() if o.type.name=='Transform'}
parents={d['m_GameObject']['m_PathID']:d['m_Father']['m_PathID'] for d in trs.values()}
bygo={d['m_GameObject']['m_PathID']:d for d in trs.values()}
def walk(tid,depth=0):
 t=trs[tid];g=gos[t['m_GameObject']['m_PathID']]
 if depth<3:print(' '*depth,g['m_Name'],'active',g['m_IsActive'],'pos',t['m_LocalPosition'])
 for p in t['m_Children']:walk(p['m_PathID'],depth+1)
for tid,t in trs.items():
 if t['m_Father']['m_PathID']==0:walk(tid)
for obj in scene.objects.values():
 if obj.type.name=='Camera':
  d=trees[obj.path_id];gid=d['m_GameObject']['m_PathID'];print('CAM',gid,gos[gid]['m_Name'],bygo[gid])
print('asset files',len(files))
for name in ['cab-4c52fff4e3c5327da00f07c701929dd8','cab-7c8d3f5d35aaf5ef0944161f30ceba91','cab-3d4478614785c4361a9782fb7104421c','cab-18a7513b19298cd3eacd40e5f54d22f8']:
 f=files.get(name);print('DEPENDENCY',name,collections.Counter(o.type.name for o in f.objects.values()) if f else 'MISSING')
 if f:
  for o in list(f.objects.values()):
   if o.type.name=='Material':
    d=o.read_typetree();print('MAT',d['m_Name'],d['m_Shader'],str(d['m_SavedProperties'])[:550])
