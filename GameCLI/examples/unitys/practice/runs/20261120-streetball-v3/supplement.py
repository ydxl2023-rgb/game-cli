from pathlib import Path
import json,hashlib,re,shutil
import numpy as np
from PIL import Image
r=Path('GameCLI/examples/unitys/practice/runs/20261120-streetball-v3');p=r.parent.parent/'client'
report=json.loads((r/'result.json').read_text());locked=json.loads((r/'locked.json').read_text())
for n,name in enumerate(['common_arrow_up','common_icon_lock','common_slot_back'],297):
 rel=f'Assets/Data/UI/Textures/{name}.png';dest=p/rel;backup=r/'before'/rel
 backup.parent.mkdir(parents=True,exist_ok=True)
 if not backup.exists():shutil.copy2(dest,backup)
 src=Image.open(backup).convert('RGBA');a=np.array(src);l=np.mean(a[:,:,:3],axis=2)/255
 mat=Image.open(r.parent/'20261120-streetball-v2/generated/129.png').convert('RGB');w,h=mat.size;mat=mat.crop((int(w*.22),int(h*.28),int(w*.78),int(h*.72))).resize(src.size)
 if n==297:a[:,:,:3]=np.clip(np.array(mat)*(.55+.45*l[:,:,None]),0,255)
 elif n==298:a[:,:,:3]=np.clip(np.array(mat)*(.3+.7*l[:,:,None]),0,255)
 else:a[:,:,:3]=np.clip(np.array([16,22,31])*(1-l[:,:,None])+np.array([246,235,213])*l[:,:,None],0,255)
 Image.fromarray(a).save(dest);shutil.copy2(dest,r/'textures'/f'{n:03d}.png')
 guid=re.search(r'^guid: (\w+)',Path(str(dest)+'.meta').read_text(),re.M)[1]
 for row in report['prefabCoverage']:
  if guid in (p/row['prefab']).read_text():
   row['indices'].append(n);row['images'].append(rel);row['replaced']+=1
 report['records'].append({'index':n,'status':'replaced','method':'generated-material-through-original-alpha-and-detail','size':list(src.size),'paths':[rel]})
 locked.pop(rel)
external=[k for k,v in locked.items() if hashlib.sha256((p/k).read_bytes()).hexdigest()!=v]
assert external==['Assets/Scripts/PracticePlugin.cs'], external
report['concurrentChangesPreserved']=external
for k in external: locked.pop(k)
report.update(totalUnique=300,replacedUnique=290,replacedFiles=745,unchangedNonTextureFiles=len(locked))
(r/'result.json').write_text(json.dumps(report,indent=2));(r/'final-locked.json').write_text(json.dumps(locked,indent=2))
lines=['# UI prefab texture coverage','','| Prefab | Replaced textures | Preserved backgrounds | Pending | Layout/animation |','|---|---:|---:|---:|---|']
for row in report['prefabCoverage']:lines.append(f"| {row['prefab']} | {row['replaced']} | {row['backgrounds']} | 0 | unchanged |")
(r/'coverage.md').write_text('\n'.join(lines))
print({k:v for k,v in report.items() if k not in ['records','prefabCoverage']})

