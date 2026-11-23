from pathlib import Path
import json,re,hashlib
from PIL import Image,ImageDraw
r=Path('GameCLI/examples/unitys/practice/runs/20261120-streetball-v4');r.mkdir(exist_ok=True);p=r.parent.parent/'client';old=r.parent/'20261120-texture-refresh';items=json.loads((old/'inventory.json').read_text());byguid={}
for e in items:
 for rel in e['paths']:
  m=re.search(r'^guid: (\w+)',Path(str(p/rel)+'.meta').read_text(),re.M)
  if m:byguid[m[1]]=e['index']
rows=[]
for f in (p/'Assets/Data/Widgets').rglob('*.prefab'):
 for block in re.split(r'--- !u!',f.read_text()):
  if 'm_SpriteState:' in block:
   refs=re.findall(r'm_(Highlighted|Pressed|Selected|Disabled)Sprite:.*?guid: (\w+)',block)
   if refs:rows.append({'prefab':f.relative_to(p).as_posix(),'states':[(s,byguid.get(g,g)) for s,g in refs]})
(r/'state-audit.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows))
sel=[53,64,65,66,69,70,116,118,129,130,135,143,144,145,146,147,180,181,187,198,199,220,224,237,238,61,62,63]
sheet=Image.new('RGB',(900,len(sel)*100),(45,48,54));d=ImageDraw.Draw(sheet)
for j,i in enumerate(sel):
 e=items[i]
 for k,base in enumerate([old/'originals',p]):
  im=Image.open(base/e['paths'][0]).convert('RGBA');im.thumbnail((390,90));sheet.paste(im,(k*450+45,j*100),im)
 d.text((0,j*100),str(i));d.text((450,j*100),str(i))
sheet.save(r/'compare.png')
(r/'locked.json').write_text(json.dumps({f.relative_to(p).as_posix():hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets').rglob('*') if f.is_file() and f.suffix!='.png'}))
