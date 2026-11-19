"""Finalize reviewed VM fields after CLI snapshot restore and temporary named-binding bootstrap."""
import json
import re
import subprocess
from pathlib import Path

base = Path(__file__).resolve().parents[1]
folder = base / '.data/imports/sv-settings'
project = base.parents[1] / 'examples/unitys/development/client'
cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
asset = 'Assets/Data/Widgets/Settings/Settings.prefab'
source = json.loads((folder/'source/main/unity-prefab.json').read_text(encoding='utf8'))
design = json.loads((folder/'main/template.json').read_text(encoding='utf8'))['design']
nodes = {n['id']: n for n in design['nodes']}
contract = json.loads((folder/'bindings.json').read_text(encoding='utf8'))
log = []

def invoke(*args):
    result = subprocess.run([cli,'--project',str(project),'--format','json',*map(str,args)],capture_output=True,encoding='utf-8-sig')
    assert result.stdout, result.stderr
    response = json.loads(result.stdout)
    assert response['ok'], (args,response)
    log.append({'args':args,'status':response.get('status')})
    return response['data']

inspection = invoke('prefab','inspect','--prefab',asset,'--recursive')
components = {n['path']:set(c['type'].split('.')[-1] for c in n['components']) for n in inspection['nodes']}
def native(path):
    n = nodes['Settings'+path.replace('/','_')]
    if not n['parent']: return n['name']
    return native(n['parent'][8:].replace('_','/'))+'/'+n['name']

def add(path, component):
    if component not in components[path]:
        invoke('components','add','--asset',asset,'--path',path,'--component',component)
        components[path].add(component)

def setfield(path, component, field, value):
    if isinstance(value,bool):value=str(value).lower()
    invoke('modify','--asset',asset,'--path',path,'--component',component,'--property',field,'--value',str(value))

spec=contract['specs'][0]
add('Settings','UISettings')
add('Settings','Metadata')
guid=re.search(r'^guid: (.+)$',(project/'Assets/Scripts/Systems/Settings/VM/VMSettings.cs.meta').read_text(),re.M)[1]
setfield('Settings','Metadata','className','VMSettings')
setfield('Settings','Metadata','classGuid',guid)
setfield('Settings','Metadata','metadatas.Array.size',len(spec['fields']))
for i, field in enumerate(spec['fields']):
    prefix=f'metadatas.Array.data[{i}]'
    for key,value in [('name',field['name'])]:
        setfield('Settings','Metadata',prefix+'.'+key,value)
    field['targets']=[native(p) for p in field['paths']]
    # Generated metadata already owns each unique target. Repair only multi-target or ambiguous source names.
    if len(field['paths']) == 1 and not field['name'].endswith('Unselected'):
        if i%20==0:print('Bound',i,flush=True)
        continue
    setfield('Settings','Metadata',prefix+'.targets.Array.size',len(field['paths']))
    setfield('Settings','Metadata',prefix+'.components.Array.size',len(field['paths']))
    for j,target in enumerate(field['targets']):
        setfield('Settings','Metadata',prefix+f'.targets.Array.data[{j}]','@'+target)
        setfield('Settings','Metadata',prefix+f'.components.Array.data[{j}]','@'+target+'#'+field['component'])
    if i%20==0:print('Bound',i,flush=True)
(folder/'bindings.json').write_text(json.dumps(contract,ensure_ascii=False,indent=2),encoding='utf8')
(base/'reports/settings-template/native-binding-log.json').write_text(json.dumps(log,indent=2),encoding='utf8')
print('Completed',len(log),'UnityCLI mutations',flush=True)
