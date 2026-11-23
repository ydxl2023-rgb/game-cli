from pathlib import Path
import json,shutil
r=Path('GameCLI/examples/unitys/practice/runs/20261123-home-palette');p=r.parent.parent/'client';a=json.loads((r.parent/'20261120-texture-refresh/inventory.json').read_text());m=json.loads((r/'generation.json').read_text());updated=[]
for e in m:
 i=e['index']
 for rel in a[i]['paths'][1:]:
  if not rel.startswith('Assets/Data/Widgets/Home/'):continue
  b=r/'before'/rel;b.parent.mkdir(parents=True,exist_ok=True)
  if not b.exists():shutil.copy2(p/rel,b)
  shutil.copy2(r/'textures'/f'{i:03}.png',p/rel);updated.append(rel)
v=json.loads((r/'verification.json').read_text());v['additionalHomeCopiesUpdated']=updated;(r/'verification.json').write_text(json.dumps(v,indent=2));print(updated)
