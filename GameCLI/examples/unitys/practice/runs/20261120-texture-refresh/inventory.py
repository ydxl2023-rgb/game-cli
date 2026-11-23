from pathlib import Path
import hashlib,json,shutil
from PIL import Image,ImageDraw,ImageFont
project=Path('G:/Works/GameCLI/GameCLI/examples/unitys/practice/client')
out=Path(__file__).parent
out.mkdir(parents=True,exist_ok=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
groups={}; locked={}
for p in (project/'Assets').rglob('*'):
 if not p.is_file():continue
 rel=p.relative_to(project).as_posix()
 if p.suffix.lower() in ['.png','.jpg','.jpeg','.tga','.psd'] and '/Data/Widgets/' in '/'+rel:
  h=sha(p)
  if h not in groups:
   im=Image.open(p).convert('RGBA');a=im.getchannel('A'); hist=a.histogram()
   groups[h]={'sha256':h,'paths':[],'size':list(im.size),'alphaRange':list(a.getextrema()),'semiPixels':sum(hist[1:255]),'opaquePixels':hist[255],'transparentPixels':hist[0]}
  groups[h]['paths'].append(rel)
  dst=out/'originals'/rel;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dst)
 else:locked[rel]=sha(p)
items=sorted(groups.values(),key=lambda x:-(x['size'][0]*x['size'][1]))
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
for i,item in enumerate(items):item['index']=i
for start in range(0,len(items),48):
 sheet=Image.new('RGB',(1200,1200),(45,48,55));d=ImageDraw.Draw(sheet)
 for j,item in enumerate(items[start:start+48]):
  x=(j%6)*200;y=(j//6)*150
  im=Image.open(project/item['paths'][0]).convert('RGBA');im.thumbnail((186,114))
  sheet.paste(im,(x+(200-im.width)//2,y+8+(114-im.height)//2),im)
  d.text((x+4,y+124),f"{item['index']:03d} {item['size'][0]}x{item['size'][1]}",font=font,fill='white')
 sheet.save(out/f'contact-{start//48:02d}.png')
(out/'inventory.json').write_text(json.dumps(items,indent=2),encoding='utf8')
(out/'locked-files.json').write_text(json.dumps(locked,indent=2),encoding='utf8')
print(json.dumps({'files':sum(len(x['paths']) for x in items),'unique':len(items),'locked':len(locked),'first':items[:3]},indent=2))
