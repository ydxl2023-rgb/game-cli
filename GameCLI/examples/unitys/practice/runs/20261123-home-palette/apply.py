from pathlib import Path
import json,hashlib,shutil
import numpy as np
from PIL import Image,ImageDraw
r=Path(__file__).parent;p=r.parent.parent/'client';items=json.loads((r.parent/'20261120-texture-refresh/inventory.json').read_text());manifest=json.loads((r/'generation.json').read_text());sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
assert len(manifest)==13
locked=json.loads((r/'locked.json').read_text());assert all(sha(p/k)==v for k,v in locked.items())
records=[];preview=Image.new('RGB',(900,((len(manifest)+3)//4)*190),(42,45,51));draw=ImageDraw.Draw(preview)
def warmcount(im):
 hsv=np.array(im.convert('HSV'));alpha=np.array(im.getchannel('A'));return int(np.sum((hsv[:,:,0]>8)&(hsv[:,:,0]<45)&(hsv[:,:,1]>100)&(hsv[:,:,2]>120)&(alpha>128)))
for j,m in enumerate(sorted(manifest,key=lambda m:m['index'])):
 i=m['index'];e=items[i];rel=e['paths'][0];backup=r/'before'/rel;backup.parent.mkdir(parents=True,exist_ok=True)
 if not backup.exists():shutil.copy2(p/rel,backup)
 src=Image.open(backup).convert('RGBA');genpath=r/'generated'/f'{i}.png';genpath.parent.mkdir(exist_ok=True);shutil.copy2(m['source'],genpath);gen=Image.open(genpath).convert('RGBA')
 box=gen.getchannel('A').point(lambda a:255 if a>128 else 0).getbbox();target=src.getchannel('A').point(lambda a:255 if a>32 else 0).getbbox();x,y,x2,y2=target
 out=Image.new('RGBA',src.size);fitted=gen.crop(box).resize((x2-x,y2-y),Image.Resampling.LANCZOS);out.paste(fitted,(x,y))
 dest=r/'textures'/f'{i:03}.png';dest.parent.mkdir(exist_ok=True);out.save(dest);shutil.copy2(dest,p/rel)
 assert out.size==src.size
 records.append({'index':i,'path':rel,'size':list(out.size),'warmPixelsBefore':warmcount(src),'warmPixelsAfter':warmcount(out)})
 icon=out.copy();icon.thumbnail((210,145));px=(j%4)*225;py=(j//4)*190;preview.paste(icon,(px+(225-icon.width)//2,py+8),icon);draw.text((px+8,py+160),str(i),fill='white')
assert all(sha(p/k)==v for k,v in locked.items())
preview.save(r/'palette-preview.png');(r/'verification.json').write_text(json.dumps({'textures':records,'nonTextureFilesUnchanged':len(locked),'warmPixelsBefore':sum(x['warmPixelsBefore'] for x in records),'warmPixelsAfter':sum(x['warmPixelsAfter'] for x in records)},indent=2));print('Updated',len(records),'home textures')
