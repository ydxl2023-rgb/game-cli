from pathlib import Path
import sys,json
R=Path(__file__).resolve().parents[2];O=Path(__file__).parent;sys.path.insert(0,str(R/'tools'))
from fairy_catalog import Reader
used={}
for p in O.glob('*.materials.json'):
 for asset in json.loads(p.read_text(encoding='utf8'))['assets']:used.setdefault(Path(asset['file']).stem,[]).append(asset['path'])
rows=[]
for pkg in ['common','guide','building','level']:
 r=Reader((R/f'extracted-resources/assets/data/ui/atlas/{pkg}/{pkg}_fui.bytes').read_bytes());r.read('I');r.read('I');r.read('B');r.string();r.string();r.pos+=20;start=r.pos;r.seek(start,4);r.strings=[r.string() for _ in range(r.read('I'))]
 if r.seek(start,5):
  for _ in range(r.read('I')):
   i=r.read('H');length=r.read('I');r.strings[i]=r.string(length)
 r.seek(start,1)
 for _ in range(r.read('H')):
  length=r.read('I');end=r.pos+length;kind=r.read('B');id=r.s();name=r.s();r.s();r.s();r.read('B');w=r.read('i');h=r.read('i')
  if kind==0:
   option=r.read('B')
   if option==1:
    x,y,cw,ch=[r.read('i') for _ in range(4)]
    for path in used.get(name,[]): rows.append(dict(path=path,border=[x,h-y-ch,w-x-cw,y]))
  r.pos=end
(O/'slice-borders.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf8');print(rows)
