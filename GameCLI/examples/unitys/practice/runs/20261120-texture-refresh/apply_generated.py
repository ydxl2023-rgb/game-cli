from pathlib import Path
import json,hashlib
import numpy as np
from PIL import Image

root=Path(__file__).parent
project=root.parent.parent/'client'
items=json.loads((root/'inventory.json').read_text())
locked=json.loads((root/'locked-files.json').read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert all((project/p).is_file() and sha(project/p)==h for p,h in locked.items()),'Non-texture baseline changed'
report=[]
for meta in sorted((root/'generated').glob('*.json')):
 entry=json.loads(meta.read_text());i=entry['index'];item=items[i]
 src=Image.open(root/'originals'/item['paths'][0]).convert('RGBA')
 gen=Image.open(meta.with_suffix('.png')).convert('RGBA').resize(src.size,Image.Resampling.LANCZOS)
 if i in (2,3,4):
  result=gen.copy();result.putalpha(src.getchannel('A'));method='generated RGB, original alpha, exact dimensions'
 else:
  # Geometry-preserving palette transfer: keep source luminance details and alpha.
  a=np.asarray(src).astype(np.float32);g=np.asarray(gen).astype(np.float32)
  mask=(a[:,:,3]>128)&(g[:,:,3]>128)
  s=a[:,:,:3];t=g[:,:,:3]
  sm=np.median(s[mask],axis=0);gm=np.median(t[mask],axis=0)
  shift=np.clip(gm-sm,-24,24)*0.55
  lum=s.mean(axis=2,keepdims=True)
  rgb=np.clip(lum+(s-lum)*1.08+shift,0,255)
  # Preserve neutral lettering and near-black ink exactly.
  protect=(s.max(axis=2)-s.min(axis=2)<12)|(s.max(axis=2)<35)
  rgb[protect]=s[protect]
  out=a.astype(np.uint8);out[:,:,:3]=rgb.astype(np.uint8)
  result=Image.fromarray(out);method='generated palette transfer; source geometry, text and alpha preserved'
 target=root/'processed'/f'{i:03d}.png';target.parent.mkdir(exist_ok=True);result.save(target)
 for rel in item['paths']:
  dest=project/rel
  assert sha(dest)==item['sha256'],'Concurrent texture edit: '+rel
  dest.write_bytes(target.read_bytes())
  check=Image.open(dest).convert('RGBA')
  assert check.size==src.size and check.getchannel('A').tobytes()==src.getchannel('A').tobytes()
 report.append({'index':i,'files':len(item['paths']),'method':method})
assert all(sha(project/p)==h for p,h in locked.items())
summary={'status':'partial','processedUnique':len(report),'processedFiles':sum(x['files'] for x in report),'totalUnique':len(items),'totalFiles':sum(len(x['paths']) for x in items),'nonTextureHashesUnchanged':len(locked),'results':report,'blockedIndices':[0,1],'pendingIndices':[x['index'] for x in items if x['index'] not in [r['index'] for r in report] and x['index'] not in [0,1]]}
(root/'result.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
print(json.dumps({k:v for k,v in summary.items() if k not in ('results','pendingIndices')}))
