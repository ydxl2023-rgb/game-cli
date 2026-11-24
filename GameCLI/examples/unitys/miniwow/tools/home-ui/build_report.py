"""Produce a searchable evidence browser, resource index and SQLite UI inventory."""
from pathlib import Path
import json,hashlib,sqlite3,html,shutil,collections
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2];OUT=Path(__file__).parent
data=json.loads((OUT/'inventory.json').read_text('utf8'));records=data['resources'];byurl={r['url']:r for r in records}
db=sqlite3.connect(ROOT/'research/resource-catalog/catalog.sqlite')
db.execute('CREATE TABLE IF NOT EXISTS home_ui_resources (ui_url TEXT PRIMARY KEY,source_asset_id TEXT NOT NULL,name TEXT NOT NULL,package TEXT NOT NULL,payload TEXT NOT NULL)')
for r in records:
 r['dynamicUses']=[dict(table=d['table'],id=d['id'],name=d['name']) for d in data['dynamic'] if d['icon'] in [r['url'],'ui://'+r['package']+'/'+r['name']]]
 if r['imagePath']:assert hashlib.sha256((ROOT/r['imagePath']).read_bytes()).hexdigest()==r['sha256']
 assert db.execute('SELECT 1 FROM assets WHERE id=?',(r['sourceAssetId'],)).fetchone()
 db.execute('INSERT OR REPLACE INTO home_ui_resources VALUES(?,?,?,?,?)',(r['url'],r['sourceAssetId'],r['name'],r['package'],json.dumps(r,ensure_ascii=False)))
db.commit();db.close()
(OUT/'resource-index.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),'utf8')
def esc(s):return html.escape(str(s))
cards=[]
for r in records:
 path='../../'+(r['imagePath'] or r['metadataPath'])
 img=f'<img loading="lazy" src="{esc(path)}">' if r['imagePath'] else '<span>组件</span>'
 uses='; '.join(e['owner']+' / '+e['role'] for e in r['usedBy'])
 dynamic='; '.join(e['name'] for e in r['dynamicUses'])
 cards.append(f'<article><a href="{esc(path)}">{img}</a><b>{esc(r["package"]+"/"+r["name"])}</b><code>{esc(r["url"])}</code><small>{esc(uses)}</small><small>{esc(dynamic)}</small><a href="{esc(path)}">打开本地资源</a></article>')
rows=[]
root=next(c for c in data['components'] if c['url']==data['root'])
for n in root['children']:
 source=byurl.get(n.get('sourceUrl'),{})
 rows.append(f'<tr><td>{esc(n["name"])}</td><td>{esc(n["type"])}</td><td>{esc(n["position"])}</td><td>{esc(n.get("size",source.get("size","")))}</td><td>{esc(source.get("package","")+"/"+source.get("name",""))}</td></tr>')
page='''<!doctype html><html lang="zh"><meta charset="utf-8"><title>Miniwow 主界面资源核对</title><style>body{font:15px system-ui;background:#202020;color:#eee;margin:24px}a{color:#eac48a}h1{font-size:26px}input{padding:12px;width:70%;position:sticky;top:0}section{display:grid;grid-template-columns:repeat(auto-fill,minmax(210px,1fr));gap:12px}article{background:#3d3831;padding:12px;display:flex;flex-direction:column;gap:6px;overflow-wrap:anywhere}[hidden]{display:none!important}article img{width:100%;height:110px;object-fit:contain}small{color:#ccc}code{font-size:11px}td,th{padding:6px;border-bottom:1px solid #555;text-align:left}.reference{width:280px;float:right;margin:0 0 16px 24px}</style><h1>主界面预制与资源核对</h1><img class="reference" src="reference.png"><p>依据：用户截图、Main/MainPage 包内结构、反编译调用点、tbmainfunctions / tbmainactivity。此页是资源审阅页，不是游戏界面。</p><p>源尺寸720×1560；目标1080×1920。不能将整个源页面拉伸。动态图片和世界空间UI参见 README.md 的待办项。</p><p>已解析范围：46组件、135图片；包含隐藏状态和非截图活动，不表示全部应同时显示。</p><details><summary>主页面36个直接子节点</summary><table><tr><th>节点</th><th>类型</th><th>源位置</th><th>源尺寸</th><th>引用</th></tr>'''+''.join(rows)+'''</table></details><div style="clear:both"></div><h2>资源索引</h2><input id="search" placeholder="搜索节点名、资源名、ui:// 或用途，例如 FunctionBtns、商城、RedDot"><section>'''+''.join(cards)+'''</section><script>document.querySelector('#search').addEventListener('input',e=>{const q=e.target.value.toLowerCase();document.querySelectorAll('article').forEach(a=>a.hidden=!a.textContent.toLowerCase().includes(q))})</script></html>'''
(OUT/'index.html').write_text(page,'utf8')
lines=['# 主页面组件结构（包内证据）','','源坐标720×1560；visible只是序列化值，最终可见性还受控制器与运行时逻辑影响。','']
for c in data['components']:
 lines.extend([f'## {c["name"]}',f'`{c["url"]}`，源尺寸 `{c["size"]}`。','控制器：'+', '.join(x['name'] for x in c['controllers'])+'。','过渡：'+', '.join(c['transitions'])+'。','','| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |','|---|---|---|---|'])
 for n in c['children']:
  refs=[n.get(k) for k in ['sourceUrl','loaderUrl','defaultItem','text','title','icon'] if n.get(k)]
  lines.append(f'| {n["name"]} | {n["type"]} | {n["position"]} / {n.get("size", "继承资源")} | '+ '<br>'.join(str(x).replace('|','\\|') for x in refs)+' |')
 lines.append('')
(OUT/'COMPONENTS.md').write_text('\n'.join(lines),'utf8')
items=[r for r in records if r['imagePath']];font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',12)
for page in range((len(items)+39)//40):
 sheet=Image.new('RGB',(900,650),'#4b4540');draw=ImageDraw.Draw(sheet)
 for i,r in enumerate(items[page*40:(page+1)*40]):
  x=(i%8)*112;y=(i//8)*130;im=Image.open(ROOT/r['imagePath']).convert('RGBA');im.thumbnail((100,95));sheet.paste(im,(x+(112-im.width)//2,y),im)
  draw.text((x+3,y+97),r['name'][:17],font=font,fill='white');draw.text((x+3,y+112),r['package'],font=font,fill='#bbbbbb')
 sheet.save(OUT/f'resources-{page+1}.jpg')
print(json.dumps(dict(resources=len(records),images=len(items),sourceIdsAndImageHashesValidated=True)))
