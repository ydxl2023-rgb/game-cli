"""Audit MainPage component dependencies; never create or modify Unity assets.
Binary layout follows official FairyGUI UIPackage/GComponent/GObject/GList readers.
Controllers and transitions are inventoried, not evaluated as runtime state.
"""
from pathlib import Path
import sys,json,sqlite3,hashlib,collections,struct,re,html
ROOT=Path(__file__).resolve().parents[2];OUT=Path(__file__).parent
sys.path.insert(0,str(ROOT/'tools'))
from fairy_catalog import Reader
db=sqlite3.connect(f'file:{(ROOT/"research/resource-catalog/catalog.sqlite").as_posix()}?mode=ro',uri=True)
assets=[json.loads(x[0]) for x in db.execute('select payload from assets')]
exports=collections.defaultdict(list)
for (raw,) in db.execute('select payload from exports'):
 e=json.loads(raw);exports[e['asset_id']].append(e)
db.close()
byurl={a['ui_url']:a for a in assets if a['type']=='FairyGUIItem'}
aliases={'ui://'+a['package_name']+'/'+a['name']:u for u,a in byurl.items()}
def canonical(url):return aliases.get(url,url)
packages={};rawitems={};sources=[]
for a in assets:
 if a['type']!='TextAsset' or not a['name'].endswith('_fui'):continue
 ex=next(e for e in exports[a['id']] if e['role']=='text_asset_payload');path=ROOT/ex['path'];r=Reader(path.read_bytes())
 assert r.read('I')==0x46475549
 version=r.read('I');assert r.read('B')==0
 pid=r.string();name=r.string();r.pos+=20;start=r.pos
 r.seek(start,4);r.strings=[r.string() for _ in range(r.read('I'))]
 if r.seek(start,5):
  for _ in range(r.read('I')):
   idx=r.read('H');n=r.read('I');r.strings[idx]=r.string(n)
 packages[pid]=name;sources.append(dict(package=name,packageId=pid,path=ex['path'],sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
 r.seek(start,1)
 for _ in range(r.read('H')):
  length=r.read('I');end=r.pos+length
  kind=r.read('B');iid=r.s();iname=r.s();r.s();r.s();r.read('B');w=r.read('i');h=r.read('i')
  if kind==3:
   extension=r.read('B');length=r.read('I');buffer=Reader(r.data[r.pos:r.pos+length]);buffer.strings=r.strings
   rawitems['ui://'+pid+iid]=(buffer,extension)
  r.pos=end

types=['Image','MovieClip','Swf','Graph','Loader','Group','Text','RichText','InputText','Component','List','Label','Button','ComboBox','ProgressBar','Slider','ScrollBar','Tree','Loader3D']
parsed={};errors=[]
def component(url):
 if url in parsed:return parsed[url]
 r,extension=rawitems[url];a=byurl[url];pid=a['package_id']
 result=dict(url=url,name=a['package_name']+'/'+a['name'],sourceAssetId=a['id'],size=[a['metadata']['width'],a['metadata']['height']],extension=extension,children=[],controllers=[],transitions=[],references=[])
 parsed[url]=result
 def ref(v,owner,role):
  if v and v.startswith('ui://'):result['references'].append(dict(url=v,owner=owner,role=role))
 if r.seek(0,1):
  for _ in range(r.read('h')):
   length=r.read('H');end=r.pos+length;begin=r.pos
   r.seek(begin,0);c=dict(name=r.s());r.read('B');r.seek(begin,1);c['pages']=[dict(id=r.s(),name=r.s()) for j in range(r.read('h'))];result['controllers'].append(c);r.pos=end
 if r.seek(0,2):
  for _ in range(r.read('h')):
   length=r.read('H');end=r.pos+length;begin=r.pos
   r.seek(begin,0);t=r.read('B');src=r.s();pkg=r.s();node=dict(type=types[t],id=r.s(),name=r.s(),position=[r.read('i'),r.read('i')])
   if src:node['sourceUrl']='ui://'+(pkg or pid)+src;ref(node['sourceUrl'],node['name'],'component/image')
   if r.read('B'):node['size']=[r.read('i'),r.read('i')]
   if r.read('B'):node['sizeLimits']=[r.read('i') for k in range(4)]
   if r.read('B'):node['scale']=[r.read('f'),r.read('f')]
   if r.read('B'):node['skew']=[r.read('f'),r.read('f')]
   if r.read('B'):node['pivot']=[r.read('f'),r.read('f'),bool(r.read('B'))]
   node.update(alpha=r.read('f'),rotation=r.read('f'),visible=bool(r.read('B')),touchable=bool(r.read('B')),grayed=bool(r.read('B')))
   if t in [6,7,8] and r.seek(begin,5):
    node['font']=r.s();node['fontSize']=r.read('h');node['color']=[r.read('B') for k in range(4)];ref(node['font'],node['name'],'font')
    if r.seek(begin,6):node['text']=r.s()
   if t==4 and r.seek(begin,5):
    node['loaderUrl']=r.s();ref(node['loaderUrl'],node['name'],'loader')
   if t==10 and r.seek(begin,8):
    node['defaultItem']=r.s();ref(node['defaultItem'],node['name'],'list template');node['items']=[]
    for j in range(r.read('h')):
     size=r.read('H');itemend=r.pos+size
     item=dict(url=r.s() or node['defaultItem'],title=r.s(),selectedTitle=r.s(),icon=r.s(),selectedIcon=r.s(),name=r.s())
     for key in ['url','icon','selectedIcon']:ref(item[key],node['name']+f'[{j}]',key)
     node['items'].append(item);r.pos=itemend
   ext=rawitems.get(node.get('sourceUrl'),(None,0))[1]
   if ext==12 and r.seek(begin,6) and r.read('B')==12:
    for key in ['title','selectedTitle','icon','selectedIcon']:
     node[key]=r.s();ref(node[key],node['name'],key)
   node['gears']=[]
   if r.seek(begin,2):
    for j in range(r.read('h')):
     size=r.read('H');gearend=r.pos+size;kind=r.read('B');controller=r.read('h');gear=dict(type=kind,controller=controller)
     if kind in [6,7]:
      gear['values']=[]
      for k in range(r.read('h')):
       page=r.s()
       if page is not None:
        value=r.s();gear['values'].append(dict(page=page,value=value))
        if kind==7:ref(value,node['name'],'gear icon')
      if r.read('B'):
       gear['default']=r.s()
       if kind==7:ref(gear['default'],node['name'],'gear default icon')
     node['gears'].append(gear);r.pos=gearend
   result['children'].append(node);r.pos=end
 if r.seek(0,5):
  for _ in range(r.read('h')):
   length=r.read('H');end=r.pos+length;result['transitions'].append(r.s());r.pos=end
 return result

class Luban:
 def __init__(self,path):self.data=path.read_bytes();self.pos=0
 def i(self):
  b=self.data[self.pos];self.pos+=1
  if b<128:return b
  n,mask=(1,63) if b<192 else (2,31) if b<224 else (3,15) if b<240 else (4,0)
  v=b&mask
  for _ in range(n):v=(v<<8)|self.data[self.pos];self.pos+=1
  return v if v<2147483648 else v-4294967296
 def s(self):
  n=self.i();v=self.data[self.pos:self.pos+n].decode();self.pos+=n;return v
tables={}
for name in ['tbmainfunctions','tbmainactivity']:
 reader=Luban(ROOT/f'extracted-resources/config-tables/{name}.bytes');rows=[]
 for _ in range(reader.i()):
  row=dict(id=reader.i(),name=reader.s())
  if name=='tbmainactivity':row.update(type=reader.i(),icon=reader.s(),effect=reader.s())
  else:row.update(icon=reader.s(),feature=reader.i())
  rows.append(row)
 assert reader.pos==len(reader.data)
 tables[name]=rows

root='ui://1pyb9685q2my4z';pending=[root];dynamic=[]
world_roots=['Building/BuildingTitleBubble','Building/BuildingPeopleBubble','Building/BuildingResTipsBubble','Building/FactoryBubble','Building/BuildingEntranceBubble','Building/BuildingOpBubble']
pending.extend('ui://'+name for name in world_roots)
for name,rows in tables.items():
 for row in rows:
  if row['icon'].startswith('ui://'):pending.append(row['icon']);dynamic.append(dict(table=name,**row))
seen=set();edges=[]
while pending:
 url=canonical(pending.pop())
 if url in seen:continue
 seen.add(url)
 if url not in byurl:errors.append(dict(kind='unresolved-url',url=url));continue
 if url not in rawitems:continue
 c=component(url)
 for ref in c['references']:
  ref['url']=canonical(ref['url']);edges.append(dict(component=url,**ref));pending.append(ref['url'])
records=[]
for url in sorted(seen):
 if url not in byurl:continue
 a=byurl[url];files=exports[a['id']]
 png=next((e for e in files if e['role']=='fairygui_atlas_crop'),None)
 records.append(dict(url=url,sourceAssetId=a['id'],package=a['package_name'],name=a['name'],type=a['metadata']['type'],size=[a['metadata'].get('width'),a['metadata'].get('height')],metadataPath=f"extracted-resources/fairygui/{a['package_name']}{a['metadata'].get('source_path') or '/'}{a['name']}.json",imagePath=(png.get('original_layout_path') or png['path']) if png else None,sha256=png['sha256'] if png else None,sprite=a.get('sprite'),usedBy=[e for e in edges if e['url']==url]))
result=dict(root=root,rootComponent=parsed[root]['name'],rootSize=parsed[root]['size'],worldRoots=world_roots,components=list(parsed.values()),resources=records,dynamic=dynamic,tables=tables,errors=errors,sources=sources,limitations=['Controller page names and icon/text gears are inventoried; relations, transition curves and runtime conditions are not evaluated.','Dynamic loader resources outside the two decoded tables need separate code/config evidence.','Dependency closure includes hidden variants, not only screenshot-visible items.'])
OUT.mkdir(exist_ok=True)
(OUT/'inventory.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),'utf8')
print(json.dumps(dict(rootSize=result['rootSize'],rootChildren=len(parsed[root]['children']),components=len(parsed),resources=len(records),images=sum(bool(r['imagePath']) for r in records),errors=errors),ensure_ascii=True))
