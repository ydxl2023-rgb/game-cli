"""Merge derived inventories, verify export hashes, and produce a searchable local report."""
from pathlib import Path
import collections, hashlib, json, sqlite3

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'research/resource-catalog'
catalog=json.loads((OUT/'catalog.json').read_text(encoding='utf-8'))
for supplement in sorted(OUT.glob('supplemental-*.json')):
    data=json.loads(supplement.read_text(encoding='utf-8'))
    for name in ('assets','exports'):
        merged={r['id']:r for r in catalog[name]}
        merged.update({r['id']:r for r in data.get(name,[])})
        catalog[name]=list(merged.values())

assets={r['id']:r for r in catalog['assets']}
ui_roles={0:'UI图片/图标；按图集映射导出，具体业务用途待核实',1:'UI影片剪辑定义；帧动画尚未重建',2:'UI声音资源引用',3:'UI组件定义；组件层级和交互尚未重建',4:'UI图集定义',5:'UI字体定义',6:'Spine资源定义',7:'DragonBones资源定义'}
for asset in assets.values():
    if asset['type']=='FairyGUIItem':
        asset['purpose_guess']=ui_roles.get(asset.get('metadata',{}).get('type'),'UI资源定义；具体业务用途待核实')
exports=collections.defaultdict(list)
for row in catalog['exports']: exports[row['asset_id']].append(row)
configs=next((a for a in assets.values() if a['name']=='Configs' and a['type']=='TextAsset'),None)
header=next((a for a in assets.values() if a['name']=='ConfigHead' and a['type']=='TextAsset'),None)
if configs and header:
    def payload(a): return ROOT/next(e['path'] for e in exports[a['id']] if e['role']=='text_asset_payload')
    raw=payload(configs).read_bytes()
    for line in payload(header).read_text(encoding='utf-8-sig').splitlines():
        name,start,size=line.split(',');start=int(start);size=int(size)
        assert start>=0 and start+size<=len(raw)
        aid=hashlib.sha256((configs['id']+'|'+name).encode()).hexdigest()
        if aid in assets: continue
        from extract_resources import label
        record=dict(id=aid,source_id=configs['source_id'],parent_asset_id=configs['id'],name=name,
                    type='ConfigTable',status='payload_exported',offset=start,size=size,errors=[],
                    **label(name,'ConfigTable'))
        record['purpose_guess']='Luban二进制配置表；字段结构尚需代码验证'
        record['evidence']=['ConfigHead明确给出表名、偏移与长度；未解码表字段']
        catalog['assets'].append(record);assets[aid]=record
        dest=OUT/'exports/ConfigTable'/f'{name}.bytes';dest.parent.mkdir(exist_ok=True)
        data=raw[start:start+size];dest.write_bytes(data)
        catalog['exports'].append(dict(id=hashlib.sha256((aid+'|table').encode()).hexdigest(),asset_id=aid,
            path=dest.relative_to(ROOT).as_posix(),format='.bytes',role='config_table_payload',size=size,sha256=hashlib.sha256(data).hexdigest()))

sources={r['id']:r for r in catalog['sources']}
failures=[]
for row in catalog['exports']:
    path=(ROOT/row['path']).resolve()
    if not path.is_relative_to(OUT.resolve()): raise ValueError('Export escapes catalog directory')
    if row['asset_id'] not in assets: raise ValueError('Dangling export')
    if not path.exists() or hashlib.sha256(path.read_bytes()).hexdigest()!=row['sha256']: failures.append(row['path'])
assert not failures, failures[:10]
assert all(a['source_id'] in sources for a in catalog['assets'])
assert len(set(r['id'] for r in catalog['exports']))==len(catalog['exports'])
summary=catalog['summary']
summary.update(asset_count=len(catalog['assets']),export_count=len(catalog['exports']),
    asset_status=dict(collections.Counter(a['status'] for a in catalog['assets'])),
    types=dict(collections.Counter(a['type'] for a in catalog['assets'])),
    export_hashes_verified=len(catalog['exports']),reference_count=sum(len(a.get('references',[])) for a in catalog['assets']))
(OUT/'catalog.json').write_text(json.dumps(catalog,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
db=sqlite3.connect(OUT/'catalog.sqlite')
for table in ('sources','assets','exports'):
    db.executemany(f'INSERT OR REPLACE INTO {table}(id,payload) VALUES (?,?)',[(a['id'],json.dumps(a,ensure_ascii=False)) for a in catalog[table]])
db.execute("CREATE INDEX IF NOT EXISTS exports_asset_idx ON exports(json_extract(payload,'$.asset_id'))")
db.execute("CREATE INDEX IF NOT EXISTS assets_type_idx ON assets(json_extract(payload,'$.type'))")
db.commit();db.close()

exports=collections.defaultdict(list)
for e in catalog['exports']: exports[e['asset_id']].append(e)
rows=[]
for a in catalog['assets']:
    files=exports[a['id']]
    image=next((e for e in files if e['format']=='.png'),None)
    def link(e): return Path(e['path']).relative_to('research/resource-catalog').as_posix()
    rows.append(dict(id=a['id'],name=a['name'],type=a['type'],system=a.get('system_guess','待分类'),
        purpose=a.get('purpose_guess','待核实'),confidence=a.get('confidence','unknown'),status=a['status'],
        source=sources[a['source_id']]['path'],container=a.get('container_paths',[])+([a['ui_url'],a.get('metadata',{}).get('source_path') or ''] if a.get('ui_url') else []),errors=a.get('errors',[]),
        image=link(image) if image else None,files=[dict(path=link(e),role=e['role']) for e in files]))
html=r'''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>小骑士冲冲冲 · 资源台账</title>
<style>body{font:15px system-ui;margin:32px;background:#f4f6f7;color:#203040}h1{margin-bottom:8px}input,select,button{padding:10px;border:1px solid #abc;border-radius:6px;margin:4px}input{width:36%}table{width:100%;border-collapse:collapse;background:white}td,th{padding:12px;border-bottom:1px solid #ddd;text-align:left;vertical-align:top}img{max-width:120px;max-height:100px;background:#ddd}small{display:block;color:#657080;overflow-wrap:anywhere}a{color:#176958}summary{cursor:pointer}#count{margin:12px}p{line-height:1.6}</style>
<h1>小骑士冲冲冲 · 资源台账</h1><p>所有用途均带证据等级。关键词标签是推测，不能替代代码或运行验证。PNG/OBJ/音频为导出文件；JSON/serialized.bin 为结构与原始对象，不能直接视为Unity prefab。</p>
<input id="q" placeholder="搜索名称、用途、系统、源包或ID"><select id="type"><option value="">全部类型</option></select><select id="system"><option value="">全部系统</option></select><button id="prev">上一页</button><button id="next">下一页</button><div id="count"></div><table><thead><tr><th>预览</th><th>名称与类型</th><th>用途及可信度</th><th>来源 / 导出</th></tr></thead><tbody id="body"></tbody></table>
<script>const rows=__ROWS__;let page=0;const q=document.querySelector('#q'),type=document.querySelector('#type'),system=document.querySelector('#system');
for(const [el,key] of [[type,'type'],[system,'system']])for(const s of [...new Set(rows.map(r=>r[key]))].sort()){const o=document.createElement('option');o.value=s;o.textContent=s;el.append(o)}
function text(tag,value,parent){const e=document.createElement(tag);e.textContent=value;parent.append(e);return e}
function render(){const needle=q.value.toLowerCase();const found=rows.filter(r=>(!type.value||r.type===type.value)&&(!system.value||r.system===system.value)&&(!needle||JSON.stringify(r).toLowerCase().includes(needle)));page=Math.min(page,Math.max(0,Math.ceil(found.length/60)-1));document.querySelector('#count').textContent=`共 ${rows.length} 项 / 匹配 ${found.length} 项 / 第 ${page+1} 页`;const body=document.querySelector('#body');body.replaceChildren();for(const r of found.slice(page*60,page*60+60)){const tr=document.createElement('tr');body.append(tr);const cells=[0,1,2,3].map(()=>{const c=document.createElement('td');tr.append(c);return c});if(r.image){const im=document.createElement('img');im.loading='lazy';im.src=r.image;cells[0].append(im)}text('strong',r.name,cells[1]);text('small',r.type+' · '+r.status,cells[1]);text('small',r.id,cells[1]);text('div',r.system,cells[2]);text('div',r.purpose,cells[2]);text('small',r.confidence,cells[2]);text('small',r.errors.join('\n'),cells[2]);text('small',r.source,cells[3]);text('small',r.container.join('\n'),cells[3]);for(const f of r.files){const a=text('a',f.role,cells[3]);a.href=f.path;a.target='_blank';cells[3].append(document.createElement('br'))}}}
for(const el of [q,type,system])el.addEventListener('input',()=>{page=0;render()});document.querySelector('#next').onclick=()=>{page++;render()};document.querySelector('#prev').onclick=()=>{page=Math.max(0,page-1);render()};render();</script></html>'''
(OUT/'index.html').write_text(html.replace('__ROWS__',json.dumps(rows,ensure_ascii=False).replace('<','\\u003c')),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=True))

