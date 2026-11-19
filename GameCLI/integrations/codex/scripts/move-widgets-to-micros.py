"""Move reusable widget modules with UnityCLI, preserving every asset and GUID."""
import json,pathlib,hashlib,subprocess,re
project=pathlib.Path('G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
report=pathlib.Path('reports/micros-directory');report.mkdir(parents=True,exist_ok=True)
moves={f'Assets/Data/Widgets/{name}':f'Assets/Data/Widgets/Micros/{name}' for name in ['Currency','Profile','ItemSlot','Reward','ShopGiftItem','ShopRechargeItem']}
moves.update({'Assets/Data/Widgets/Common/Badges':'Assets/Data/Widgets/Micros/Badges/Common','Assets/Data/Widgets/Home/Badges':'Assets/Data/Widgets/Micros/Badges/Home'})
def run(args):
 p=subprocess.run([cli,'--project',str(project),'--format','json',*args],capture_output=True,encoding='utf-8-sig');v=json.loads(p.stdout)
 with (report/'operations.jsonl').open('a',encoding='utf-8') as f:f.write(json.dumps({'args':args,'result':v})+'\n')
 assert v['ok'],v
 return v
identity=json.loads((project/'ProjectSettings/GameCLIProject.json').read_text())
assert identity['unityProjectId']=='b99aae81-c4b3-44ea-a032-a8df17831096'
assert all((project/src).is_dir() and not (project/dst).exists() for src,dst in moves.items()),'Review missing source or conflicting destination.'
files=[]
for src,dst in moves.items():
 for p in [pathlib.Path(str(project/src)+'.meta'),*(project/src).rglob('*')]:
  if p.is_file():
   path=p.relative_to(project).as_posix();body=p.read_bytes();entry={'from':path,'to':dst+path[len(src):],'sha256':hashlib.sha256(body).hexdigest()}
   if p.suffix=='.meta':
    match=re.search(r'^guid: (\w+)',body.decode('utf-8-sig'),re.M)
    if match:entry['guid']=match[1]
   files.append(entry)
legacy=project/'Assets/Data/Widgets/Micros/ItemSlot.prefab';legacyHash=hashlib.sha256(legacy.read_bytes()).hexdigest()
(report/'migration.json').write_text(json.dumps({'schemaVersion':1,'directories':moves,'files':files,'legacyItemSlotSha256':legacyHash},indent=2),encoding='utf-8')
run(['asset','create-folder','--dst','Assets/Data/Widgets/Micros/Badges'])
for src,dst in moves.items():
 run(['asset','move','--src',src,'--dst',dst]);assert not (project/src).exists();print(src+' -> '+dst,flush=True)
for f in files:assert hashlib.sha256((project/f['to']).read_bytes()).hexdigest()==f['sha256'],f['to']
assert hashlib.sha256(legacy.read_bytes()).hexdigest()==legacyHash
(report/'move-verification.json').write_text(json.dumps({'verified':True,'files':len(files),'guids':sum('guid'in f for f in files),'legacyUnchanged':True},indent=2),encoding='utf-8')
print('All moved bytes and GUIDs preserved.',flush=True)
