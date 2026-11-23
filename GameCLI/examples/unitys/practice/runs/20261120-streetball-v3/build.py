from pathlib import Path
import json,hashlib,shutil,re
import numpy as np
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).parent;old=root.parent/'20261120-texture-refresh';v2=root.parent/'20261120-streetball-v2';project=root.parent.parent/'client'
items=json.loads((old/'inventory.json').read_text());locked=json.loads((root/'locked.json').read_text());sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
background={0,1,2,3,4,5,6,8,9,15}
art=[12,20,21,37,41,42,43,44,45,46,54,57,61,62,63,*range(71,114),127,134,137,153,156,159,163,164,172,179,183,186,188,7,16,19,33,212,55,65,192,193,194,195,196,239,140,148,177,178,206,207]
missing=[i for i in art if not (root/'generated'/f'{i}.png').exists()]
if missing:raise SystemExit('Pending generated assets: '+str(missing))
assert all(sha(project/p)==h for p,h in locked.items()),'Non-texture edit detected'
def original(e):return Image.open(old/'originals'/e['paths'][0]).convert('RGBA')
def fit_icon(src,gen):
 box=gen.getchannel('A').point(lambda v:255 if v>100 else 0).getbbox();target=src.getchannel('A').getbbox();x,y,r,b=target
 image=gen.crop(box).resize((r-x,b-y),Image.Resampling.LANCZOS)
 out=Image.new('RGBA',src.size);out.paste(image,(x,y));return out
materials={i:Image.open(v2/'generated'/f'{i}.png').convert('RGB') for i in [116,129,167]}
for key,im in list(materials.items()):
 w,h=im.size;materials[key]=im.crop((int(w*.22),int(h*.28),int(w*.78),int(h*.72)))
v2records={x['index']:x for x in json.loads((v2/'result.json').read_text())['records']}
records=[]
for e in items:
 i=e['index'];src=original(e)
 for rel in e['paths']:
  dst=root/'before'/rel
  if not dst.exists():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(project/rel,dst)
 if i in background:
  for rel in e['paths']:shutil.copy2(old/'originals'/rel,project/rel)
  records.append({'index':i,'status':'background-preserved','paths':e['paths']});continue
 if i in art:
  out=fit_icon(src,Image.open(root/'generated'/f'{i}.png').convert('RGBA'));method='individual-imagegen'
 elif i in [49,219]:
  out=fit_icon(src,Image.open(root/'generated/212.png').convert('RGBA'))
  if i==49:out.putalpha(src.getchannel('A'))
  method='generated-basketball-variant'
 elif i in v2records and i not in [167]:
  out=Image.open(v2/'textures'/f'{i:03d}.png').convert('RGBA');method='v2-generated-material'
 else:
  # Composite generated color/material through the exact source shape and preserve
  # local luminance structure so typography and symbols never become blank blocks.
  a=np.asarray(src).astype(np.float32);rgb=a[:,:,:3];mx=rgb.max(2);mn=rgb.min(2);sat=mx-mn
  l=(rgb[:,:,0]*.299+rgb[:,:,1]*.587+rgb[:,:,2]*.114)/255
  warm=(rgb[:,:,0]>rgb[:,:,2]*1.12)&(sat>25)
  cool=(rgb[:,:,2]>rgb[:,:,0]*1.05)&(sat>25)
  green=(rgb[:,:,1]>rgb[:,:,0]*1.35)&(rgb[:,:,1]>rgb[:,:,2]*1.25)&(sat>45)
  orange=np.asarray(materials[129].resize(src.size)).astype(float)
  cyan=np.asarray(materials[116].resize(src.size)).astype(float)
  dark=np.asarray(materials[167].resize(src.size)).astype(float)
  # Source dark/light contrast retained; warm paper stays legible under dark text.
  ink=np.clip(dark*.30+[9,12,18],0,255)
  paper=np.clip([250,241,224]+(orange-orange.mean(2,keepdims=True))*.025,0,255)
  new=ink*(1-l[:,:,None])+paper*l[:,:,None]
  new[warm]=orange[warm]*(.45+.55*l[warm,None])
  new[cool]=cyan[cool]*(.4+.6*l[cool,None])
  new[green]=rgb[green]*.92+np.array([5,14,6])
  # Text/line anti-aliasing, shadows and all semi-transparent fades are exact.
  arr=a.astype(np.uint8);arr[:,:,:3]=np.clip(new,0,255).astype(np.uint8)
  out=Image.fromarray(arr);method='generated-material-through-original-alpha-and-detail'
 dst=root/'textures'/f'{i:03d}.png';dst.parent.mkdir(exist_ok=True);out.save(dst)
 assert out.size==src.size
 for rel in e['paths']:shutil.copy2(dst,project/rel)
 assert sha(dst)!=e['sha256']
 records.append({'index':i,'status':'replaced','method':method,'size':e['size'],'paths':e['paths']})
assert all(sha(project/p)==h for p,h in locked.items())
assert all(sha(project/p)==e['sha256'] for e in items if e['index'] in background for p in e['paths'])
refs=json.loads((root/'prefab-references.json').read_text());byindex={x['index']:x for x in records}
for r in refs:
 r['replaced']=len([i for i in r['indices'] if byindex[i]['status']=='replaced']);r['backgrounds']=len([i for i in r['indices'] if i in background]);r['pending']=[]
 r['prefabUnchanged']=sha(project/r['prefab'])==locked[r['prefab']]
report={'prefabs':len(refs),'totalUnique':len(items),'replacedUnique':sum(x['status']=='replaced' for x in records),'replacedFiles':sum(len(x['paths']) for x in records if x['status']=='replaced'),'preservedBackgrounds':len(background),'unchangedNonTextureFiles':len(locked),'records':records,'prefabCoverage':refs}
(root/'result.json').write_text(json.dumps(report,indent=2),encoding='utf8')
lines=['# UI prefab texture coverage','','| Prefab | Replaced textures | Preserved backgrounds | Pending | Layout/animation |','|---|---:|---:|---:|---|']
for r in refs:lines.append(f"| {r['prefab']} | {r['replaced']} | {r['backgrounds']} | 0 | unchanged |")
(root/'coverage.md').write_text('\n'.join(lines),encoding='utf8')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
for start in range(0,len(items),48):
 sheet=Image.new('RGB',(1200,1200),(40,43,49));d=ImageDraw.Draw(sheet)
 for j,e in enumerate(items[start:start+48]):
  im=Image.open(project/e['paths'][0]).convert('RGBA');im.thumbnail((186,114));x=j%6*200;y=j//6*150;sheet.paste(im,(x+(200-im.width)//2,y+8+(114-im.height)//2),im);d.text((x+4,y+124),f"{e['index']:03d} {e['size'][0]}x{e['size'][1]}",font=font,fill='white')
 sheet.save(root/f'contact-{start//48:02d}.png')
print(json.dumps({k:v for k,v in report.items() if k not in ('records','prefabCoverage')}))
