"""Build a local, searchable inventory of recovered code, not a Unity project."""
import hashlib
import html
import json
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'research/code-study'


def main():
    rows = []
    for folder in ['managed/decompiled', 'packages/members']:
        for path in sorted((ROOT / folder).rglob('*')):
            if path.suffix not in ['.cs', '.js']:
                continue
            payload = path.read_bytes()
            relative = path.relative_to(ROOT).as_posix()
            parts = Path(relative).parts
            assembly = parts[2] if parts[0] == 'managed' else 'JavaScript'
            group = ('Configuration' if 'Zombie.Table' in relative else
                     'Battle' if '/Battle/' in relative or '/Dungeon/' in relative else
                     'UI' if path.stem.startswith('UI') else
                     'Protocol' if '/zfoocs/' in relative else
                     'Application' if assembly == 'HotUpdate' else 'Dependency')
            rows.append({'path': relative, 'name': path.name, 'assembly': assembly,
                         'group': group, 'bytes': len(payload), 'lines': len(payload.splitlines()),
                         'sha256': hashlib.sha256(payload).hexdigest()})
    (ROOT / 'code-index.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2), 'utf-8')
    encoded = json.dumps(rows, ensure_ascii=False).replace('<', '\\u003c')
    page = '''<!doctype html><html lang="zh"><meta charset="utf-8"><title>小骑士冲冲冲 · 代码学习索引</title>
<style>body{font:16px/1.6 system-ui;margin:36px auto;max-width:1200px;color:#263442;background:#f7f8fa}input,select{padding:10px;font:inherit}input{width:50%}table{width:100%;border-collapse:collapse;background:white}td,th{text-align:left;padding:8px;border-bottom:1px solid #ddd}a{color:#2457a7}small{color:#586575}</style>
<h1>小骑士冲冲冲 · 代码学习索引</h1><p>本地静态恢复资料。C# 是反编译结果，JS 是包内提取文件；不等同于原始开发工程，未验证可编译。</p>
<p><a href="README.md">阅读指南</a> · <a href="GAMEPLAY-STUDY.md">机制学习</a> · <a href="ARCHITECTURE-STUDY.md">架构学习</a> · <a href="code-index.json">完整机器清单</a></p>
<input id="query" placeholder="输入类名、路径或关键词，如 Explore、Buff、Proxy"><select id="assembly"><option value="">全部程序集</option></select><p id="count"></p>
<table><thead><tr><th>文件</th><th>分类提示</th><th>行数</th></tr></thead><tbody id="body"></tbody></table>
<script>const rows=__ROWS__;const query=document.getElementById('query'),select=document.getElementById('assembly'),body=document.getElementById('body');
for(const name of [...new Set(rows.map(x=>x.assembly))]){const option=document.createElement('option');option.value=name;option.textContent=name;select.append(option)}
function render(){const q=query.value.toLowerCase();const found=rows.filter(x=>(!select.value||x.assembly===select.value)&&x.path.toLowerCase().includes(q));document.getElementById('count').textContent=`匹配 ${found.length} / ${rows.length} 个文件；显示前 300 条。分类是路径提示，并非语义分析结论。`;body.replaceChildren();for(const row of found.slice(0,300)){const tr=document.createElement('tr'),td=document.createElement('td'),a=document.createElement('a');a.href=row.path;a.textContent=row.path;td.append(a);tr.append(td);for(const value of [row.group,row.lines]){const cell=document.createElement('td');cell.textContent=value;tr.append(cell)}body.append(tr)}}query.oninput=render;select.onchange=render;render();</script></html>'''
    (ROOT / 'index.html').write_text(page.replace('__ROWS__', encoded), 'utf-8')
    print(json.dumps({'files': len(rows), 'assemblies': dict(Counter(r['assembly'] for r in rows))}))


if __name__ == '__main__':
    main()
