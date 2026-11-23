from pathlib import Path
import json,hashlib,shutil
import numpy as np
from PIL import Image,ImageDraw
p=Path('G:/Works/GameCLI/GameCLI/examples/unitys/practice/client');r=p.parent/'runs/20261123-mail-button-alpha';r.mkdir(exist_ok=True)
locked={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets').rglob('*') if f.is_file() and f.suffix!='.png'}
records=[]
for name in ['ui_system_mail_01.png','ui_system_mail_02.png']:
 f=p/'Assets/Data/Widgets/Mail/MailAssets'/name;b=r/name
 if not b.exists():shutil.copy2(f,b)
 im=Image.open(b).convert('RGBA');a=np.array(im);h,w=a.shape[:2]
 # Follow the existing outer white arc. Supersample only its alpha boundary.
 scale=16;yy,xx=np.mgrid[:h*scale,:w*scale];x=(xx+.5)/scale;y=(yy+.5)/scale
 cx,cy,radius=35.67,46.80,35.15
 corner=(x<cx)&(y>cy);inside=(x-cx)**2+(y-cy)**2<=radius**2
 mask=(~corner|inside).reshape(h,scale,w,scale).mean((1,3))
 oldalpha=a[:,:,3].copy();a[:,:,3]=np.rint(oldalpha*mask).astype('uint8')
 out=Image.fromarray(a);out.save(f)
 assert out.size==im.size;assert np.array_equal(a[:,:,:3],np.array(im)[:,:,:3]);assert a[75,5,3]==0
 records.append({'file':str(f),'size':list(im.size),'alphaPixelsChanged':int(np.sum(a[:,:,3]!=oldalpha)),'rgbUnchanged':True})
assert all(hashlib.sha256((p/k).read_bytes()).hexdigest()==v for k,v in locked.items())
(r/'verification.json').write_text(json.dumps({'textures':records,'nonTextureFilesUnchanged':len(locked)},indent=2))
sheet=Image.new('RGB',(1080,440));draw=ImageDraw.Draw(sheet)
for row,name in enumerate(['ui_system_mail_01.png','ui_system_mail_02.png']):
 for col,folder in enumerate([r,p/'Assets/Data/Widgets/Mail/MailAssets']):
  for y0 in range(row*220,(row+1)*220,20):
   for x0 in range(col*540,(col+1)*540,20):draw.rectangle((x0,y0,x0+19,y0+19),fill=(155,155,155) if (x0//20+y0//20)%2 else (215,215,215))
  icon=Image.open(folder/name).resize((520,184),Image.Resampling.NEAREST);sheet.paste(icon,(col*540+10,row*220+15),icon)
  draw.text((col*540+12,row*220+202),'BEFORE' if col==0 else 'AFTER',fill='black')
sheet.save(r/'alpha-comparison.png');print(json.dumps(records))
