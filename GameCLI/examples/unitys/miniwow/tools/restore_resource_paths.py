"""Materialize exports using declared container/FGUI paths, with explicit fallbacks."""
from pathlib import Path, PurePosixPath
import collections, hashlib, json, re, shutil, sqlite3, sys

ROOT=Path(__file__).resolve().parents[1]
CAT=ROOT/'research/resource-catalog'
DEST=ROOT/'extracted-resources'
sys.path.insert(0,str(ROOT/'research/python-libs'))
import UnityPy

def sha(data): return hashlib.sha256(data).hexdigest()

def segment(value):
    value=re.sub(r'[<>:"/\\|?*\x00-\x1f]','_',str(value)).rstrip(' .')[:130] or 'unnamed'
    if value.upper().split('.')[0] in {'CON','PRN','AUX','NUL',*[f'COM{i}' for i in range(1,10)],*[f'LPT{i}' for i in range(1,10)]}: value='_'+value
    return value

def safe_path(value):
    parts=PurePosixPath(value.replace('\\','/').lstrip('/')).parts
    if '..' in parts: raise ValueError('Traversal in resource path')
    return Path(*(segment(p) for p in parts if p not in ('','.')))

data=json.loads((CAT/'catalog.json').read_text(encoding='utf-8'))
sources={s['id']:s for s in data['sources']}
assets={a['id']:a for a in data['assets']}
lookup={(a['source_id'],a.get('serialized_file'),a.get('path_id')):a for a in assets.values() if a.get('path_id')}
bundle_dirs={}; scan_errors=[]
for i,source in enumerate(sources.values()):
    path=ROOT/'wxecf0e106809a477d'/source['path']
    if path.suffix!='.bundle': continue
    try:
        env=UnityPy.load(str(path)); found=[]
        for original,ptr in env.container.items():
            obj=ptr.deref()
            asset=lookup.get((source['id'],obj.assets_file.name,str(obj.path_id)))
            if asset is None: continue
            asset.setdefault('container_paths',[])
            if original not in asset['container_paths']: asset['container_paths'].append(original)
            found.append(original)
        if found:
            # Common directory of declared assets, not a reconstruction of subasset source files.
            dirs=[PurePosixPath(p).parent.parts for p in found]
            prefix=[]
            for values in zip(*dirs):
                if len(set(values))!=1: break
                prefix.append(values[0])
            if prefix: bundle_dirs[source['id']]=Path(*prefix)
    except Exception as error: scan_errors.append({'source':source['path'],'error':str(error)})
    if i%30==0: print('Path scan',i,flush=True)

DEST.mkdir(exist_ok=True)
manifest=[]; occupied={}

def place(src,relative,expected,record):
    relative=safe_path(relative.as_posix()); key=relative.as_posix().casefold()
    if key in occupied and occupied[key]!=expected:
        relative=relative.with_name(relative.stem+'__'+record['asset_id'][:12]+relative.suffix)
        key=relative.as_posix().casefold()
    target=DEST/relative
    if not target.resolve().is_relative_to(DEST.resolve()): raise ValueError('Unsafe output')
    if target.exists() and sha(target.read_bytes())!=expected:
        raise ValueError('Refusing to overwrite different existing file: '+str(target))
    target.parent.mkdir(parents=True,exist_ok=True)
    if not target.exists(): shutil.copyfile(src,target)
    if sha(target.read_bytes())!=expected: raise ValueError('Copied hash mismatch: '+str(target))
    occupied[key]=expected
    record['restored_path']=target.relative_to(ROOT).as_posix()
    record['sha256']=expected;manifest.append(record)
    return record['restored_path']

for i,export in enumerate(data['exports']):
    a=assets[export['asset_id']]; original=None; method='unmapped'
    suffix=export['format']; name=segment(a['name'])
    if a.get('container_paths'):
        original=sorted(a['container_paths'])[0]; base=safe_path(original);method='unity_container'
        if export['role']=='raw_unity_object': relative=Path('_serialized')/Path(str(base)+'.bin')
        elif export['role']=='type_tree': relative=Path(str(base)+'.json')
        elif export['role']=='text_asset_payload': relative=base
        else: relative=base.with_suffix(suffix)
    elif a['type']=='FairyGUIItem':
        original=(a.get('metadata',{}).get('source_path') or '/')+'/'+a['name']
        base=Path('fairygui')/segment(a['package_name'])/safe_path(original)
        relative=Path(str(base)+suffix);method='fairygui_declared_path'
    elif a['type']=='ConfigTable':
        relative=Path('config-tables')/(name+'.bytes');method='config_table_name'
    elif a['type']=='WebDataMember':
        original=a['name'];relative=Path('webdata')/safe_path(original);method='webdata_member'
    elif a['source_id'] in bundle_dirs:
        base=bundle_dirs[a['source_id']]/'_subassets'/segment(a['type'])/(name+'__'+a['id'][:12])
        relative=Path(str(base)+suffix);method='bundle_directory_subasset'
    else:
        relative=Path('_unmapped')/segment(Path(sources[a['source_id']]['path']).stem)/segment(a['type'])/(name+'__'+a['id'][:12]+suffix)
    record=dict(export_id=export['id'],asset_id=a['id'],name=a['name'],type=a['type'],
        source=sources[a['source_id']]['path'],previous_path=export['path'],original_path=original,
        path_basis=method,all_container_paths=a.get('container_paths',[]))
    restored=place(ROOT/export['path'],relative,export['sha256'],record)
    export['original_layout_path']=restored
    export['original_layout_basis']=method
    if i%10000==0: print('Restored',i,'/',len(data['exports']),flush=True)

for a in assets.values():
    if a.get('status')=='source_available' and a.get('source_path'):
        s=sources[a['source_id']]
        place(ROOT/a['source_path'],safe_path(s['path']),s['sha256'],dict(asset_id=a['id'],export_id=None,
            name=a['name'],type=a['type'],source=s['path'],previous_path=a['source_path'],original_path=s['path'],path_basis='original_file'))

summary=dict(records=len(manifest),unique_files=len(occupied),path_basis=dict(collections.Counter(r['path_basis'] for r in manifest)),
    assets_with_container_paths=sum(bool(a.get('container_paths')) for a in assets.values()),scan_errors=scan_errors,
    hashes_verified=len(manifest))
(DEST/'path-manifest.json').write_text(json.dumps({'summary':summary,'files':manifest},ensure_ascii=False,indent=2),encoding='utf-8')
(DEST/'README.md').write_text('''# 按原始路径归档的提取资源

- `assets/`：Unity AssetBundle中声明的原始路径；非根对象放在相应包目录下的 `_subassets`，不冒称子资源有独立原始文件。
- `fairygui/包名/`：FairyGUI包内目录和资源名；PNG保持已提取的图块，JSON是元数据。
- `config-tables/`：按ConfigHead声明的表名存放二进制配置，不代表原作磁盘目录。
- `webdata/`：WebData内部文件原路径。
- `_serialized/`：有原始路径对象的原始Unity序列化数据。
- `_unmapped/`：没有原始路径证据的对象，按来源与类型保留。
- `videos/` 等：原始独立文件的相对路径。

JSON与serialized.bin不是可直接导入的prefab/材质/动画资产。相同目录下同名不同内容会追加资源ID，原始名字仍可在path-manifest.json查询。
`path-manifest.json`完整记录每个文件原始路径、映射依据、旧导出位置、新位置及SHA256；全部文件复制后再次校验。
原有resource-catalog与数据库台账仍保留，exports.payload.original_layout_path指向这里。
''',encoding='utf-8')
(CAT/'catalog.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
db=sqlite3.connect(CAT/'catalog.sqlite')
for table in ('assets','exports'):
    db.executemany(f'INSERT OR REPLACE INTO {table}(id,payload) VALUES (?,?)',[(r['id'],json.dumps(r,ensure_ascii=False)) for r in data[table]])
db.commit();db.close()
print(json.dumps(summary,ensure_ascii=True),flush=True)
