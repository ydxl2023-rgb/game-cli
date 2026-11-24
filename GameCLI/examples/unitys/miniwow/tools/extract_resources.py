"""Extract supplied Unity resources and build a reproducible, evidence-labelled catalog."""
from pathlib import Path
import argparse, base64, collections, csv, hashlib, json, re, sqlite3, struct, sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'research/python-libs'))
import UnityPy

RULES = [
    ('building|hotel|priest|tailor|weaponstore|forge|craft', '村庄经营与补给'),
    ('explor|adventure|worldmap|navigation', '探索与世界地图'),
    ('hero|character', '英雄'), ('equipment', '装备'), ('pet', '宠物'),
    ('relic', '藏品'), ('book', '典籍'), ('battle|monster|skill|buff', '战斗'),
    ('login|loading|startvideo', '启动与登录'), ('shop|mall|pay', '商店'),
    ('activity|fund', '活动'), ('common', '通用UI'), ('hotupdate', '逻辑程序集'),
    ('config', '配置'), ('manor', '庄园')]

def digest(data):
    return hashlib.sha256(data).hexdigest()

def safe(name):
    return re.sub(r'[^\w.\-]', '_', str(name))[:90] or 'unnamed'

def json_default(value):
    if isinstance(value, bytes):
        return {'encoding': 'base64', 'data': base64.b64encode(value).decode()}
    return str(value)

def label(name, kind):
    matches = [system for pattern, system in RULES if re.search(pattern, name, re.I)]
    purposes = {'Texture2D':'纹理/图集，具体图块需UI映射确认', 'Sprite':'二维精灵',
        'TextAsset':'文本或二进制数据，按内容进一步识别', 'Mesh':'网格模型（OBJ不含完整骨骼/材质）',
        'AudioClip':'音乐或音效', 'Material':'材质参数与纹理引用', 'GameObject':'对象/预制结构',
        'MonoBehaviour':'组件序列化数据', 'AnimationClip':'动画数据', 'Font':'字体',
        'VideoClip':'视频引用', 'Shader':'着色器数据'}
    return dict(system_guess=' / '.join(matches) or '待分类',
                purpose_guess=purposes.get(kind, kind + ' 序列化资源'),
                confidence='name_inference' if matches else 'unknown',
                evidence=['原始文件/容器路径/对象名称关键词分类，未经运行验证'])

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, default=ROOT/'wxecf0e106809a477d')
    args = parser.parse_args()
    source = args.source.resolve()
    out = ROOT/'research/resource-catalog'
    out.mkdir(parents=True, exist_ok=True)
    catalog = dict(schema_version=1, sources=[], assets=[], exports=[])
    expected = {r['path']:r for r in csv.DictReader((source/'resources.csv').open(encoding='utf-8-sig'))}

    def write_export(asset, data, suffix, role):
        if isinstance(data, str): data = data.encode('utf-8')
        relative = Path('exports')/asset['type']/f"{asset['id'][:20]}_{safe(asset['name'])}{suffix}"
        dest = out/relative
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_bytes(data)
        item = dict(id=digest((asset['id']+role+str(relative)).encode()), asset_id=asset['id'],
                    path=str(dest.relative_to(ROOT)).replace('\\','/'), format=suffix,
                    role=role, size=len(data), sha256=digest(data))
        catalog['exports'].append(item)
        return item

    def unity_file(path, source_row):
        env = UnityPy.load(str(path))
        containers = collections.defaultdict(list)
        for key, ptr in env.container.items():
            try:
                obj = ptr.deref()
                containers[(obj.assets_file.name,obj.path_id)].append(key)
            except Exception:
                pass
        objects = list(env.objects)
        source_row['object_count'] = len(objects)
        for obj in objects:
            kind = obj.type.name
            serialized_file = obj.assets_file.name
            aid = digest(f"{source_row['id']}|{serialized_file}|{obj.path_id}".encode())
            try: name = obj.peek_name() or str(obj.path_id)
            except Exception: name = str(obj.path_id)
            paths = containers.get((serialized_file,obj.path_id), [])
            row = dict(id=aid, source_id=source_row['id'], serialized_file=serialized_file,
                       path_id=str(obj.path_id), type=kind, name=name, container_paths=paths,
                       status='registered', errors=[], references=[])
            row.update(label(' '.join([source_row['path'],name]+paths),kind))
            catalog['assets'].append(row)
            try:
                raw = obj.get_raw_data()
                write_export(row, raw, '.serialized.bin', 'raw_unity_object')
                row['status']='raw_exported'
            except Exception as error:
                row['errors'].append('raw: '+str(error)[:400])
            try:
                if kind in ('Texture2D','Sprite'):
                    import io
                    data=obj.read(); im=data.image
                    buf=io.BytesIO(); im.save(buf,format='PNG')
                    write_export(row,buf.getvalue(),'.png','image')
                    row['width'],row['height']=im.size
                elif kind == 'TextAsset':
                    data=obj.read().m_Script
                    if isinstance(data,str): data=data.encode('utf-8',errors='surrogateescape')
                    ext=Path(name).suffix or '.bytes'
                    write_export(row,data,ext,'text_asset_payload')
                elif kind == 'Mesh':
                    write_export(row,obj.read().export(),'.obj','mesh_geometry')
                elif kind == 'Font':
                    data=bytes(obj.read().m_FontData)
                    if not data: raise ValueError('No embedded font data')
                    write_export(row,data,'.otf' if data[:4]==b'OTTO' else '.ttf','font')
                elif kind == 'AudioClip':
                    samples=obj.read().samples
                    if not samples: raise ValueError('No decoded audio samples')
                    for index,(audio_name,data) in enumerate(samples.items()):
                        write_export(row,data,f'.{index}'+(Path(audio_name).suffix or '.audio'),'audio_'+str(index))
                else:
                    tree=obj.read_typetree()
                    def refs(value, trail=''):
                        if isinstance(value,dict):
                            if 'm_PathID' in value and 'm_FileID' in value and value['m_PathID']:
                                fid=int(value['m_FileID'])
                                external=None
                                if fid>0 and fid<=len(obj.assets_file.externals):
                                    external=str(obj.assets_file.externals[fid-1].path)
                                row['references'].append(dict(field=trail,file_id=fid,path_id=str(value['m_PathID']),external=external))
                            for k,v in value.items(): refs(v,trail+'.'+k)
                        elif isinstance(value,list):
                            for i,v in enumerate(value): refs(v,trail+f'[{i}]')
                    refs(tree)
                    write_export(row,json.dumps(tree,ensure_ascii=False,default=json_default),'.json','type_tree')
                row['status']='exported'
            except Exception as error:
                row['errors'].append('decoded: '+str(error)[:400])
        source_row['status']='parsed'

    files=sorted(p for p in source.rglob('*') if p.is_file())
    for index,path in enumerate(files):
        relative=path.relative_to(source).as_posix(); raw=path.read_bytes(); sha=digest(raw)
        row=dict(id=digest((relative+'|'+sha).encode()),path=relative,sha256=sha,size=len(raw),
                 status='inventoried',errors=[],expected_sha256=expected.get(relative,{}).get('sha256'))
        row['integrity']='verified' if row['expected_sha256']==sha else ('mismatch' if row['expected_sha256'] else 'not_in_manifest')
        catalog['sources'].append(row)
        try:
            if row['integrity']=='mismatch': raise ValueError('Source digest differs from supplied manifest')
            if path.suffix=='.bundle': unity_file(path,row)
            elif raw.startswith(b'UnityWebData1.0\0'):
                end=struct.unpack_from('<I',raw,16)[0]; pos=20
                while pos<end:
                    offset,size,length=struct.unpack_from('<III',raw,pos);pos+=12
                    name=raw[pos:pos+length].decode();pos+=length
                    if offset+size>len(raw): raise ValueError('Invalid webdata member bounds')
                    asset=dict(id=digest((row['id']+'|'+name).encode()),source_id=row['id'],name=name,type='WebDataMember',status='exported',errors=[])
                    asset.update(label(name,'WebDataMember'));catalog['assets'].append(asset)
                    exported=write_export(asset,raw[offset:offset+size],Path(name).suffix or '.bin','webdata_member')
                    if name in ('data.unity3d','Resources/unity_default_resources'):
                        unity_file(ROOT/exported['path'],row)
                row['status']='parsed'
            elif path.suffix=='.wxapkg':
                row['status']='encrypted_unparsed' if raw.startswith(b'V1MMWX') else 'unsupported_package'
                row['errors'].append('Package payload not decrypted; no inferred member inventory')
            elif path.suffix=='.br' and len(raw)<8:
                row['status']='incomplete';row['errors'].append('Placeholder-sized wasm file; not executable payload')
            else:
                asset=dict(id=digest((row['id']+'|file').encode()),source_id=row['id'],name=path.name,
                           type='Video' if path.suffix=='.mp4' else 'File',status='source_available',errors=[],
                           source_path=path.relative_to(ROOT).as_posix())
                asset.update(label(relative,asset['type']));catalog['assets'].append(asset)
        except Exception as error:
            row['status']='parse_failed';row['errors'].append(str(error)[:600])
        if index%15==0: print(f'{index+1}/{len(files)} sources; {len(catalog["assets"])} assets',flush=True)

    catalog['summary']=dict(source_count=len(catalog['sources']),asset_count=len(catalog['assets']),export_count=len(catalog['exports']),
        source_status=dict(collections.Counter(r['status'] for r in catalog['sources'])),
        asset_status=dict(collections.Counter(r['status'] for r in catalog['assets'])),
        types=dict(collections.Counter(r['type'] for r in catalog['assets'])),tool='UnityPy '+UnityPy.__version__)
    (out/'catalog.json').write_text(json.dumps(catalog,ensure_ascii=False,indent=2),encoding='utf-8')
    db=sqlite3.connect(out/'catalog.sqlite')
    for table in ('sources','assets','exports'):
        db.execute(f'CREATE TABLE IF NOT EXISTS {table} (id TEXT PRIMARY KEY, payload TEXT NOT NULL)')
        db.executemany(f'INSERT OR REPLACE INTO {table} VALUES (?,?)',[(r['id'],json.dumps(r,ensure_ascii=False)) for r in catalog[table]])
    db.execute('CREATE TABLE IF NOT EXISTS annotations (asset_id TEXT PRIMARY KEY, system_name TEXT, purpose TEXT, status TEXT, evidence TEXT)')
    db.execute('CREATE VIEW IF NOT EXISTS resource_search AS SELECT id, json_extract(payload,\'$.name\') AS name, json_extract(payload,\'$.type\') AS type, json_extract(payload,\'$.system_guess\') AS system_guess, json_extract(payload,\'$.purpose_guess\') AS purpose_guess, json_extract(payload,\'$.status\') AS status FROM assets')
    db.commit(); db.close()
    print(json.dumps(catalog['summary'],ensure_ascii=True),flush=True)

if __name__=='__main__': main()
