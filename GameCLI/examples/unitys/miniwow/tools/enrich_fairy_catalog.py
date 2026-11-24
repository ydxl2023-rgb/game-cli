"""Export FGUI-declared atlas rectangles and supplemental catalog records.
Rotated atlas crops remain packed: no guessed orientation restoration.
"""
import json, hashlib, re
from pathlib import Path
from PIL import Image
from fairy_catalog import parse

root=Path(__file__).resolve().parents[1]
catalog=json.loads((root/'research/resource-catalog/catalog.json').read_text(encoding='utf-8'))
assets=[]; exports=[]; packages=[]; failures=[]
by_export={}
for e in catalog['exports']: by_export.setdefault(e['asset_id'],{})[e['role']]=e
textures={}
image_cache={}
for a in catalog['assets']:
    if a['type']=='Texture2D': textures.setdefault((a['source_id'],a['name']),[]).append(a)
out=root/'research/resource-catalog/exports/FairyGUI';out.mkdir(parents=True,exist_ok=True)
def digest(s): return hashlib.sha256(s.encode()).hexdigest()
def register(a,path,role):
    raw=path.read_bytes();exports.append(dict(id=digest(a['id']+role),asset_id=a['id'],path=path.relative_to(root).as_posix(),format=path.suffix,role=role,size=len(raw),sha256=hashlib.sha256(raw).hexdigest()))
for parent in catalog['assets']:
    if parent['type']!='TextAsset' or not parent['name'].endswith('_fui'): continue
    try:
        payload=by_export[parent['id']]['text_asset_payload']; pkg=parse(root/payload['path'])
        items={x['id']:x for x in pkg['items']};sprites={x['item_id']:x for x in pkg['sprites']}
        packages.append(dict(name=pkg['name'],version=pkg['version'],items=len(items),sprites=len(sprites)))
        for key in sorted(items.keys()|sprites.keys()):
            item=items.get(key,{'id':key,'name':None,'type':'sprite_frame'}); sprite=sprites.get(key)
            aid=digest(parent['id']+':fairy:'+key)
            a=dict(id=aid,source_id=parent['source_id'],parent_asset_id=parent['id'],type='FairyGUIItem',name=item.get('name') or key,
                   status='exported',errors=[],references=[{'asset_id':parent['id']}],system_guess=pkg['name'],
                   purpose_guess='FairyGUI declared package item; business use needs review',confidence='declared_ui_metadata',
                   evidence=['FGUI string/item/sprite tables', 'https://github.com/fairygui/FairyGUI-unity/blob/master/Assets/Scripts/UI/UIPackage.cs'],
                   package_name=pkg['name'],package_id=pkg['package_id'],ui_url='ui://'+pkg['package_id']+key,metadata=item,sprite=sprite,
                   limitations=pkg['limitations'])
            if sprite:
                atlas=items.get(sprite['atlas_id'],{}); texture_name=pkg['name']+'_'+Path(atlas.get('file') or '').stem
                candidates=textures.get((parent['source_id'],texture_name),[])
                try:
                    if len(candidates)!=1: raise ValueError('Atlas texture match count: '+str(len(candidates)))
                    texture=candidates[0];image_export=by_export[texture['id']]['image']
                    if texture['id'] not in image_cache:
                        image_cache[texture['id']]=Image.open(root/image_export['path']).copy()
                    im=image_cache[texture['id']]
                    if im:
                        x,y,w,h=sprite['rect']
                        if min(x,y)<0 or min(w,h)<=0 or x+w>im.width or y+h>im.height: raise ValueError('Invalid atlas bounds')
                        target=out/(aid[:20]+'.png');im.crop((x,y,x+w,y+h)).save(target)
                    a['references'].append({'asset_id':texture['id']});a['atlas_texture_asset_id']=texture['id']
                    a['crop_orientation']='packed_rotated' if sprite['rotated'] else 'upright'
                    a['limitations']=a['limitations']+['PNG is the exact packed atlas rectangle; trimming is not padded and rotated rectangles are not unrotated.']
                    register(a,target,'fairygui_atlas_crop')
                except Exception as ex:
                    a['errors'].append(str(ex));a['status']='metadata_only';failures.append({'item':key,'package':pkg['name'],'error':str(ex)})
            target=out/(aid[:20]+'.json');target.write_text(json.dumps(a,ensure_ascii=False,indent=2),encoding='utf-8');register(a,target,'fairygui_item_metadata');assets.append(a)
    except Exception as ex: failures.append({'package':parent['name'],'error':str(ex)})
result={'assets':assets,'exports':exports,'summary':{'packages':packages,'items':len(assets),'png_crops':sum(e['role']=='fairygui_atlas_crop' for e in exports),'failures':failures}}
(root/'research/resource-catalog/supplemental-fairy.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(result['summary'],ensure_ascii=True))
