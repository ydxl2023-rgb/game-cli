from pathlib import Path
import json,re,collections
r=Path('GameCLI/examples/unitys/practice/runs/20261120-streetball-v4');p=r.parent.parent/'client';report=json.loads((r.parent/'20261120-streetball-v3/result.json').read_text());indices={rel:e['index'] for e in report['records'] for rel in e['paths']};guid={}
for f in (p/'Assets').rglob('*.png.meta'):
 g=re.search(r'^guid: (\w+)',f.read_text(),re.M)[1];guid[g]=f.with_suffix('').relative_to(p).as_posix()
rows=[];transition=collections.Counter();missing=[]
for f in (p/'Assets/Data/Widgets').rglob('*.prefab'):
 s=f.read_text();transition.update(re.findall(r'm_Transition: (\d+)',s));refs=[]
 for g in set(re.findall(r'guid: (\w+)',s)):
  if g in guid:
   rel=guid[g];refs.append({'path':rel,'index':indices.get(rel)})
   if rel not in indices:missing.append(rel)
 rows.append({'prefab':f.relative_to(p).as_posix(),'textures':refs,'stateSprites':re.findall(r'm_(Highlighted|Pressed|Selected|Disabled)Sprite:.*?guid: (\w+)',s)})
a={'prefabs':len(rows),'transitions':dict(transition),'uncoveredPngReferences':missing,'spriteSwapReferences':sum(len(x['stateSprites']) for x in rows),'statePairs':{'settingsToggle':[198,199],'settingsSelection':[238,237],'disabledAndEnabled':[138,116],'orangeAndCyanRounded':[129,130]},'rows':rows}
assert not missing
(r/'state-audit.json').write_text(json.dumps(a,indent=2));print({k:v for k,v in a.items() if k!='rows'})
