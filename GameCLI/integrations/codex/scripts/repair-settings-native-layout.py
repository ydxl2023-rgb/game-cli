"""Restore source image colors and slider anchors through the official UnityCLI."""
import json
import subprocess
from pathlib import Path

base = Path(__file__).resolve().parents[1]
folder = base / '.data/imports/sv-settings'
source = json.loads((folder/'source/main/unity-prefab.json').read_text(encoding='utf8'))
design = json.loads((folder/'main/template.json').read_text(encoding='utf8'))['design']
nodes = {n['id']: n for n in design['nodes']}
cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
project = base.parents[1]/'examples/unitys/development/client'
asset = 'Assets/Data/Widgets/Settings/Settings.prefab'
log = []

def native(path):
    n = nodes['Settings'+path.replace('/', '_')]
    return native(n['parent'][8:].replace('_', '/'))+'/'+n['name'] if n['parent'] else n['name']

def modify(path, component, prop, value):
    if isinstance(value, dict): value = ','.join(str(v) for v in value.values())
    if isinstance(value, bool): value = str(value).lower()
    args = ['modify','--asset',asset,'--path',native(path),'--component',component,'--property',prop,'--value',str(value)]
    result = subprocess.run([cli,'--project',str(project),'--format','json',*args],capture_output=True,encoding='utf-8-sig')
    response = json.loads(result.stdout)
    assert response['ok'], (args,response)
    log.append({'args':args,'status':response['status']})
    if len(log)%20 == 0: print('Repaired',len(log),flush=True)

sliderPaths = [n['path'] for n in source['nodes'] if any(c['type'].split(',')[0].endswith('.Slider') for c in n['components'])]
for n in source['nodes']:
    if 'Settings'+n['path'].replace('/','_') not in nodes: continue
    for c in n['components']:
        name = c['type'].split(',')[0].split('.')[-1]
        if name not in ['Image','CustomImage','RawImage']: continue
        data = next(iter(json.loads(c['json']).values()))
        if 'm_Color' in data and data['m_Color'] != dict(r=1,g=1,b=1,a=1):
            modify(n['path'],'Image','m_Color',data['m_Color'])
    if any(n['path']==p or n['path'].startswith(p+'/') for p in sliderPaths):
        for key in ['anchorMin','anchorMax','pivot','sizeDelta','anchoredPosition']:
            modify(n['path'],'RectTransform','m_'+key[0].upper()+key[1:],n['rectTransform'][key])

(base/'reports/settings-template/native-layout-repair.json').write_text(json.dumps(log,indent=2),encoding='utf8')
print('Completed',len(log),'official mutations',flush=True)
