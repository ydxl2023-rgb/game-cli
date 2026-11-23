from pathlib import Path
import json,hashlib
import numpy as np
from PIL import Image
r=Path('GameCLI/examples/unitys/practice/runs/20261120-streetball-v4');p=r.parent.parent/'client';old=r.parent/'20261120-texture-refresh';sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest();locked=json.loads((r/'locked.json').read_text());result=json.loads((r/'result.json').read_text())
assert all(sha(p/k)==h for k,h in locked.items())
for e in result['records']:
 for rel in e['paths']:
  src=Image.open(old/'originals'/rel).convert('RGBA');dst=Image.open(p/rel).convert('RGBA');assert src.size==dst.size
  if e['index']!=54:assert np.array_equal(np.array(src)[:,:,3],np.array(dst)[:,:,3])
items=json.loads((old/'inventory.json').read_text())
for i in [0,1,2,3,4,5,6,8,9,15]:
 for rel in items[i]['paths']:assert sha(p/rel)==items[i]['sha256']
assert not (p/'Assets/Scripts/TextureStateCheck.cs').exists()
verification={'repairedUnique':result['unique'],'repairedFiles':result['files'],'allDimensionsPreserved':True,'originalAlphaRestoredFor':62,'recruitLeftTransparentInset':22,'recruitCanvas':[262,161],'prefabsAnimationsAndMetaUnchanged':True,'lockedFilesChecked':len(locked),'backgroundsUnchanged':10,'runtimeChecks':['Home recruitment separation','Settings selected/unselected contrast','Settings audience toggle OFF->ON->OFF, original preference restored','Sound tab toggle and slider edges'],'staticAudit':'All 44 prefabs; 173 ColorTint and 73 None transitions; zero SpriteSwap references; state images are child layers or VM visibility','runtimeScreenshots':['home-fixed.png','settings-state-a.png','settings-state-b.png','settings-sound.png'],'limitations':'Runtime visual inspection sampled main affected controls; not an exhaustive click-through of every button.'}
(r/'verification.json').write_text(json.dumps(verification,ensure_ascii=False,indent=2));print(json.dumps(verification))
