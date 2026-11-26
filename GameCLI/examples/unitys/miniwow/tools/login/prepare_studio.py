"""Package the approved local entry design and source; never infer layout from verification output."""
from pathlib import Path
import hashlib,json
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).parent
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
assets=[]
materials=json.loads((OUT/'materials.json').read_text('utf8'))['assets']
for item in materials:
 source=(OUT/item['file']).resolve().relative_to(ROOT).as_posix()
 destination=item['path']
 assets.append(dict(purpose=item['id'],source=source,destination=destination,sourceSha256=digest(ROOT/source),destinationSha256=digest(ROOT/'client'/destination)))
assert all(a['sourceSha256']==a['destinationSha256'] for a in assets)
(OUT/'resources.json').write_text(json.dumps(dict(schemaVersion=2,assets=assets,layoutEvidence='Original login screenshot, excluding the WeChat host bar; background logical size 720x1560, centered cover.'),ensure_ascii=False,indent=2),'utf8')
def package(system):
 paths=set()
 for directory in [f'Assets/Scripts/Systems/{system}']+(['Assets/Data/Widgets/Login'] if system=='Login' else []):
  paths.add(ROOT/'client'/(directory+'.meta'))
  paths.update(p for p in (ROOT/'client'/directory).rglob('*') if p.is_file() and p.suffix in ['.cs','.meta','.prefab'])
 if system=='Login':
  paths.update(OUT/n for n in ['README.md','resources.json','materials.json','login.json','install.ps1','verify-session.ps1','prepare_studio.py','restore_design.py'])
 files=[]
 for p in sorted(paths):
  content=p.read_bytes().decode('utf8');base=ROOT/'client' if p.is_relative_to(ROOT/'client') else ROOT
  files.append(dict(path=p.relative_to(base).as_posix(),content=content,sha256=hashlib.sha256(content.encode()).hexdigest()))
 assert len(files)<=128 and sum(len(f['content'].encode()) for f in files)<4*1024*1024
 return dict(schemaVersion=1,files=files)
def vector(x,y):return dict(x=x,y=y)
def rect(anchor,pivot,pos,size,maximum=None):
 return dict(anchorMin=vector(*anchor),anchorMax=vector(*(maximum or anchor)),pivot=vector(*pivot),anchoredPosition=vector(*pos),sizeDelta=vector(*size))
nodes=[]
def visit(source,parent=None,parent_rect=None):
 name=source['name'];r=source['rect'];ident='UILogin' if parent is None else 'Login'+name
 n=dict(id=ident,name=name,parent=parent,type=source['element'],x=r['x'],y=r['y'],width=r['width'],height=r['height'],color='#FFFFFF',text='',fontSize=28,surface='transparent',active=name not in ['LoadingVisible','FailedVisible'])
 if name in ['UILogin','EntryPanel','Background']:
  n['rectTransform']=rect((0,0),(.5,.5),(0,0),(0,0),(1,1))
 elif parent=='LoginEntryPanel':
  top=name in ['Logo','AgeRating','Notice']
  n['rectTransform']=rect((.5,1 if top else 0),(.5,.5),(r['x']+r['width']/2-540,(0 if top else 1920)-r['y']-r['height']/2),(r['width'],r['height']))
 else:
  n['rectTransform']=rect((.5,.5),(.5,.5),(r['x']+r['width']/2-parent_rect['x']-parent_rect['width']/2,parent_rect['y']+parent_rect['height']/2-r['y']-r['height']/2),(r['width'],r['height']))
 asset=next((a for a in assets if a['purpose']==name),None)
 if asset:
  n['image']=asset['destinationSha256'];n['imageFit']='100% 100%'
 if source.get('text'):
  t=source['text'];n.update(text=t['content'],fontSize=t['fontSize'],color=t['color'],textAlign='center',verticalAlign='center')
 nodes.append(n)
 for child in source.get('children',[]):visit(child,ident,r)
visit(json.loads((OUT/'login.json').read_text('utf8'))['nodes'][0])
nodes[0].update(templateSourceId='SvLogin',templateSourceVersion=11,scripts=package('Login'))
master=dict(id='MasterSystem',name='MasterSystem',parent=None,type='Container',systemKind='code',x=0,y=0,width=1080,height=1920,color='#FFFFFF',text='',fontSize=28,surface='transparent',scripts=package('Master'),templateSourceId='MasterSystem',templateSourceVersion=9)
(OUT/'studio-package.json').write_text(json.dumps(dict(nodes=nodes+[master]),ensure_ascii=True),'utf8')
print('Prepared Login and Master source packages and resource manifest.')
