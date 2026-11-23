from pathlib import Path
import json,hashlib,shutil
from PIL import Image,ImageDraw
import numpy as np
root=Path(__file__).parent;old=root.parent/'20261120-texture-refresh';project=root.parent.parent/'client'
items=json.loads((old/'inventory.json').read_text());locked=json.loads((old/'locked-files.json').read_text());sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert all(sha(project/p)==h for p,h in locked.items())
# Back up precisely the current version before any write.
for e in items:
 for rel in e['paths']:
  dst=root/'before'/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(project/rel,dst)
backgrounds=[0,1,2,3,4,5,6,8,9,15]
for i in backgrounds:
 for rel in items[i]['paths']:shutil.copy2(old/'originals'/rel,project/rel)
cyan=[28,53,70,116,118,130,144,146,147,180,199,224,237,263,266,285]
orange=[31,64,66,69,114,129,135,143,145,149,154,170,187,221,253,255,257]
dark=[26,27,32,39,50,131,136,142,150,162,171,191,210,215,217,218,226,230,236,250,265,268,271,278,284,286]
mapping={**{i:116 for i in cyan},**{i:129 for i in orange},**{i:167 for i in dark},56:56,58:56,59:59,60:60}
records=[]
for i,donor in mapping.items():
 e=items[i];src=Image.open(old/'originals'/e['paths'][0]).convert('RGBA');gen=Image.open(root/'generated'/f'{donor}.png').convert('RGBA')
 box=gen.getchannel('A').point(lambda v:255 if v>128 else 0).getbbox();gen=gen.crop(box)
 result=Image.new('RGBA',src.size);bbox=src.getchannel('A').getbbox();x,y,r,b=bbox
 if donor in (56,59,60):
  gen=gen.resize((r-x,b-y),Image.Resampling.LANCZOS);result.paste(gen,(x,y));method='generated icon fitted to original visible bounds'
 else:
  # Reuse generated material, original alpha supplies exact silhouette and fading.
  gen=gen.resize(src.size,Image.Resampling.LANCZOS)
  result=gen;result.putalpha(src.getchannel('A'));method='generated UI material composited with original alpha'
  # Keep original symbol/text geometry for glyph-like small assets.
  if min(src.size)<45 or i in (253,255,257,263,266):
   material=Image.open(root/'generated'/f'{donor}.png').convert('RGB');w,h=material.size
   rgb=material.crop((w//4,h//4,3*w//4,3*h//4)).resize(src.size,Image.Resampling.LANCZOS)
   result=rgb.convert('RGBA');result.putalpha(src.getchannel('A'))
 out=root/'textures'/f'{i:03d}.png';out.parent.mkdir(exist_ok=True);result.save(out)
 for rel in e['paths']:shutil.copy2(out,project/rel)
 records.append({'index':i,'donor':donor,'files':len(e['paths']),'method':method})
assert all(sha(project/p)==h for p,h in locked.items())
assert all(sha(project/rel)==items[i]['sha256'] for i in backgrounds for rel in items[i]['paths'])
for e in items:
 for rel in e['paths']:assert Image.open(project/rel).size==tuple(e['size'])
summary={'generatedMasters':6,'replacedUnique':len(records),'replacedFiles':sum(r['files'] for r in records),'backgroundIndicesRestoredAndLocked':backgrounds,'unchangedNonTextureFiles':len(locked),'records':records}
(root/'result.json').write_text(json.dumps(summary,indent=2),encoding='utf8')
sheet=Image.new('RGB',(1200,660),(38,42,49));d=ImageDraw.Draw(sheet)
for j,i in enumerate([56,59,60,116,129,26,28,31,70,130,143,144]):
 im=Image.open(root/'textures'/f'{i:03d}.png');im.thumbnail((270,170));x=(j%4)*300;y=(j//4)*220;sheet.paste(im,(x+(300-im.width)//2,y+12),im);d.text((x+14,y+192),f'Asset {i}',fill='white')
sheet.save(root/'preview.png')
print(json.dumps({k:v for k,v in summary.items() if k!='records'}))
