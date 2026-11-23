from pathlib import Path
import json,re,hashlib
root=Path(__file__).parent;project=root.parent.parent/'client';old=root.parent/'20261120-texture-refresh'
items=json.loads((old/'inventory.json').read_text());paths={p:e['index'] for e in items for p in e['paths']};guid={}
for p in (project/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(errors='ignore'),re.M)
 if m:guid[m[1]]=p.with_suffix('').relative_to(project).as_posix()
reports=[]
for p in (project/'Assets/Data/Widgets').rglob('*.prefab'):
 refs=set(re.findall(r'guid: ([0-9a-f]{32})',p.read_text()))
 images=sorted({guid[g] for g in refs if g in guid and guid[g] in paths})
 reports.append({'prefab':p.relative_to(project).as_posix(),'images':images,'indices':sorted({paths[x] for x in images})})
locked={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file() and p.relative_to(project).as_posix() not in paths}
(root/'prefab-references.json').write_text(json.dumps(reports,indent=2));(root/'locked.json').write_text(json.dumps(locked,indent=2))
print(json.dumps({'prefabs':len(reports),'referencedUnique':len({i for r in reports for i in r['indices']}),'outsideWidgetsImages':[v for v in guid.values() if v.endswith('.png') and v not in paths][:30]}))
