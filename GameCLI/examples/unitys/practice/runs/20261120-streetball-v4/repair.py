from pathlib import Path
import json,hashlib,shutil
import numpy as np
from PIL import Image,ImageDraw
r=Path(__file__).parent;p=r.parent.parent/'client';old=r.parent/'20261120-texture-refresh';v2=r.parent/'20261120-streetball-v2';v3=r.parent/'20261120-streetball-v3';items=json.loads((old/'inventory.json').read_text());sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
base=json.loads((v2/'result.json').read_text())['records'];indices={e['index'] for e in base if e['donor'] in [116,129,167]};indices.update([138,180,181,187,198,199,220,222,224,237,238,282]);indices.add(54)
materials={}
for k in [116,129,167]:
 im=Image.open(v2/'generated'/f'{k}.png').convert('RGB');w,h=im.size;materials[k]=im.crop((int(w*.28),int(h*.32),int(w*.72),int(h*.68)))
records=[]
for i in sorted(indices):
 e=items[i];src=Image.open(old/'originals'/e['paths'][0]).convert('RGBA');before=Image.open(p/e['paths'][0]).convert('RGBA')
 for rel in e['paths']:
  backup=r/'before'/rel;backup.parent.mkdir(parents=True,exist_ok=True)
  if not backup.exists():shutil.copy2(p/rel,backup)
 if i==54:
  before=Image.open(r/'before'/e['paths'][0]).convert('RGBA')
  # Rects overlap by 16.8 units. Keep transforms unchanged; reserve 22 source pixels on the left.
  out=Image.new('RGBA',src.size);fit=before.resize((240,161),Image.Resampling.LANCZOS);out.paste(fit,(22,0));method='22px transparent left inset; unchanged 262x161 canvas'
 else:
  a=np.array(src);rgb=a[:,:,:3].astype(float);mx=rgb.max(2);mn=rgb.min(2);sat=mx-mn;l=rgb.mean(2)/255
  cyan=np.array(materials[116].resize(src.size,Image.Resampling.LANCZOS)).astype(float)
  orange=np.array(materials[129].resize(src.size,Image.Resampling.LANCZOS)).astype(float)
  neutral=np.repeat((l*235+10)[:,:,None],3,axis=2)
  neutral[:,:,0]*=1.0;neutral[:,:,1]*=.99;neutral[:,:,2]*=.97
  warm=(rgb[:,:,0]>rgb[:,:,2]*1.1)&(sat>30)
  cool=(rgb[:,:,2]>rgb[:,:,0]*1.06)&(sat>30)
  # Preserve source highlight/ink structure; use only the generated interior, never its rectangular frame.
  new=neutral.copy()
  for mask,mat in [(warm,orange),(cool,cyan)]:
   target=mat*(.42+.58*l[:,:,None]);new[mask]=target[mask]
  # Near-white borders and dark glyphs are inherited from their source coverage.
  white=(mn>195)&(sat<55);dark=(mx<80)
  new[white]=rgb[white]*.98+2
  new[dark]=rgb[dark]*.8+np.array([5,7,10])
  # The OFF / unselected states must stay neutral, not share the ON cyan material.
  if i in [138,198,220,238]:new=neutral
  a[:,:,:3]=np.clip(new,0,255).astype('uint8');out=Image.fromarray(a);method='generated interior with source border, symbol luminance, and exact alpha; neutral inactive state'
  assert np.array_equal(np.array(out)[:,:,3],np.array(src)[:,:,3])
 dest=r/'textures'/f'{i:03}.png';dest.parent.mkdir(exist_ok=True);out.save(dest)
 for rel in e['paths']:shutil.copy2(dest,p/rel)
 records.append({'index':i,'paths':e['paths'],'method':method,'size':list(out.size)})
locked=json.loads((r/'locked.json').read_text());assert all(sha(p/k)==h for k,h in locked.items()),'Non-texture change'
(r/'result.json').write_text(json.dumps({'unique':len(records),'files':sum(len(e['paths']) for e in records),'records':records},indent=2))
sel=[54,53,64,66,69,70,116,118,129,130,138,143,144,145,146,147,180,181,187,198,199,220,222,224,237,238,282]
sheet=Image.new('RGB',(900,len(sel)*100),(45,48,54));d=ImageDraw.Draw(sheet)
for j,i in enumerate(sel):
 e=items[i]
 for k,basepath in enumerate([r/'before',p]):
  im=Image.open(basepath/e['paths'][0]).convert('RGBA');im.thumbnail((390,90));sheet.paste(im,(k*450+45,j*100),im)
 d.text((0,j*100),str(i));d.text((450,j*100),str(i))
sheet.save(r/'repairs.png');print('updated',len(records),sum(len(e['paths']) for e in records))

