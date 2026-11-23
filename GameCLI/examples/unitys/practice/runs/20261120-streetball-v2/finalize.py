from pathlib import Path
import json,shutil,hashlib
r=Path(__file__).parent;o=r.parent/'20261120-texture-refresh';p=r.parent.parent/'client'
items=json.loads((o/'inventory.json').read_text());report=json.loads((r/'result.json').read_text())
# These sprites include functional symbols within a filled shape. Keep their original readability.
keep=[142,150,226,250]
for i in keep:
 for rel in items[i]['paths']:shutil.copy2(o/'originals'/rel,p/rel)
report['records']=[x for x in report['records'] if x['index'] not in keep]
report['replacedUnique']=len(report['records']);report['replacedFiles']=sum(x['files'] for x in report['records'])
report['functionalSymbolsPreserved']=keep
locked=json.loads((o/'locked-files.json').read_text());assert all(hashlib.sha256((p/f).read_bytes()).hexdigest()==h for f,h in locked.items())
(r/'result.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(report['replacedUnique'],report['replacedFiles'])
