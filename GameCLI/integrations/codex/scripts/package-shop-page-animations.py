"""Collect source controller graphs and their dependencies for import by the Unity editor."""
import json
import os
import re
import tarfile
from io import BytesIO
from pathlib import Path

base = Path(__file__).resolve().parents[1]
source = Path(os.getenv('SV_PROJECT', 'G:/Works/SV/Development/Project'))
assets = base.parents[1] / 'examples/unitys/development/client/Assets/Data/Widgets'
archive = base / '.data/imports/sv-shop'
out = archive / 'native-page-animation'
out.mkdir(exist_ok=True)
cards = ['GoodField','LongStripField','GiftBagField','RechargeGifField','UIShopWallPaperItem']
targets = {p.stem:p for p in (assets/'Shop/Pages').glob('*.prefab')}
targets.update({name:assets/f'Common/Micros/{name}.prefab' for name in cards})
deps, required, plans = {}, set(), {}
for name,path in targets.items():
    raw=json.loads((archive/f'full-export/{name}/unity-prefab.json').read_text(encoding='utf8'))
    deps.update({d['guid']:d['path'] for d in raw['dependencies']})
    design=json.loads((archive/f'full-designs/{name}/template.json').read_text(encoding='utf8'))['design']
    native={}
    for n in design['nodes']:
        native[n['id']]=(native[n['parent']]+'/'+n['name']) if n['parent'] else ''
    all_nodes={n['path']:n for n in raw['nodes']}
    fx=set()
    for n in raw['nodes']:
        if any(c['type'].split(',')[0] in ('UnityEngine.MeshRenderer','UnityEngine.ParticleSystem','UnityEngine.Animator') for c in n['components']):
            ix=n['path']
            while ix and name+ix.replace('/','_') not in native:
                fx.add(ix)
                ix=all_nodes[ix]['parent']
    names, nodes, hosts, effects = {}, [], [], []
    for n in raw['nodes']:
        index=n['path']
        names[index]=(names[n['parent']]+'/'+n['name']).strip('/') if n['parent'] else ''
        key=name+index.replace('/','_')
        if key not in native and index in fx:
            parent=native[name+n['parent'].replace('/','_')]
            siblings=[v['path'] for v in raw['nodes'] if v['parent']==n['parent'] and v['name']==n['name']]
            suffix='' if siblings[0]==index else str(siblings.index(index)+1)
            native[key]=parent+'/'+n['name']+suffix
            components=[]
            for c in n['components']:
                typ=c['type'].split(',')[0]
                if typ not in ('UnityEngine.Transform','UnityEngine.RectTransform','UnityEngine.MeshFilter','UnityEngine.MeshRenderer','UnityEngine.ParticleSystem','UnityEngine.ParticleSystemRenderer'):continue
                refs=[r for r in c['references'] if r['path'].startswith('Assets/') and not r['property'].startswith('m_CorrespondingSourceObject') and not r['path'].endswith('.prefab')]
                required.update(r['guid'] for r in refs)
                components.append({'type':typ,'values':next(iter(json.loads(c['json']).values())) if c.get('json') else {},'refs':refs})
            effects.append({'path':native[key].strip('/'),'active':n['activeSelf'],'components':components})
        if key not in native: continue
        dest=native[key].strip('/')
        nodes.append({'original':names[index],'target':dest,'components':[c['type'].split(',')[0] for c in n['components']]})
        for c in n['components']:
            if not c['type'].startswith('UnityEngine.Animator,'): continue
            ref=next((r for r in c['references'] if r['property']=='m_Controller'),None)
            if ref:
                required.add(ref['guid'])
                hosts.append({'original':names[index],'target':dest,'guid':ref['guid']})
    plans[name]={'asset':'Assets/'+path.relative_to(assets.parents[1]).as_posix(),'nodes':nodes,'hosts':hosts,'effects':effects}
# The independent recommendation cell has the same source animation subtree.
plan=plans['ShopEverydaySaleView']
prefix='ScrollView/ScrollCell'
def subtree(entries):
    return [dict(e,original=e['original'][len(prefix):].strip('/'),target=e['target'][len('Animator/'+prefix):].strip('/')) for e in entries if e['original']==prefix or e['original'].startswith(prefix+'/')]
plans['ShopRecommendedItem']={'asset':'Assets/Data/Widgets/Common/Micros/ShopRecommendedItem.prefab','nodes':subtree(plan['nodes']),'hosts':subtree(plan['hosts'])}
plans['ShopRecommendedItem']['effects']=[dict(e,path=e['path'][len('Animator/'+prefix):].strip('/')) for e in plan['effects'] if e['path'].startswith('Animator/'+prefix+'/')]
pending=list(required)
while pending:
    guid=pending.pop()
    p=source/deps[guid]
    if p.suffix not in ('.controller','.anim','.mat','.asset','.shadergraph'): continue
    for g in re.findall(r'guid: ([0-9a-f]{32})',p.read_text(encoding='utf8')):
        if g in required or g not in deps or deps[g].startswith('Packages/'): continue
        required.add(g)
        pending.append(g)
with tarfile.open(out/'animations.unitypackage','w:gz') as tar:
    for guid in sorted(required):
        p=source/deps[guid]
        kind={'.controller':'Controllers','.anim':'Clips','.png':'Sprites'}.get(p.suffix,'Effects')
        label=p.name if sum(Path(deps[g]).name==p.name for g in required)==1 else guid[:8]+'-'+p.name
        dest='Assets/Data/Widgets/Shop/Animations/'+kind+'/'+label
        for key,body in [('pathname',dest.encode()),('asset',p.read_bytes()),('asset.meta',Path(str(p)+'.meta').read_bytes())]:
            info=tarfile.TarInfo(guid+'/'+key);info.size=len(body);tar.addfile(info,BytesIO(body))
wall=plans['WallpaperMallView']
prefix=next(n['target'] for n in wall['nodes'] if n['original'].endswith('/UIShopWallPaperItem'))
for group,field in [('nodes','target'),('hosts','target'),('effects','path')]:
    for n in wall[group]:
        if n[field].startswith(prefix+'/'):
            n[field]=prefix+'/Animator'+n[field][len(prefix):]
for name,plan in plans.items():
    (out/(name+'-plan.json')).write_text(json.dumps(plan,ensure_ascii=False),encoding='utf8')
(out/'plan.json').write_text(json.dumps(plans,ensure_ascii=False),encoding='utf8')
(out/'guids.json').write_text(json.dumps(sorted(required)),encoding='utf8')
print(len(required),'assets;',sum(len(p['hosts']) for p in plans.values()),'retained animator hosts')



# Install only byte-identical source assets. Refuse collisions with an existing destination.
if '--install' in __import__('sys').argv:
    project=assets.parents[2]
    with tarfile.open(out/'animations.unitypackage') as tar:
        for m in tar:
            if not m.name.endswith('/pathname'): continue
            dest=project/tar.extractfile(m).read().decode()
            assert dest.resolve().is_relative_to(assets/'Shop/Animations')
            for suffix,entry in [('.meta','asset.meta'),('','asset')]:
                f=Path(str(dest)+suffix)
                body=tar.extractfile(m.name.rsplit('/',1)[0]+'/'+entry).read()
                if f.exists():
                    if suffix=='.meta': assert re.search(rb'^guid: (\w+)',f.read_bytes(),re.M)[1]==re.search(rb'^guid: (\w+)',body,re.M)[1]
                    else: assert f.read_bytes()==body,f
                else:
                    f.parent.mkdir(parents=True,exist_ok=True)
                    f.write_bytes(body)


