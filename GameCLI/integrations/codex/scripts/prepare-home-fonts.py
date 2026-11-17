"""Derive browser fonts from the exact source faces; retain native originals."""
import hashlib
import json
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path('.data/home-font-tools').resolve()))
from fontTools.ttLib import TTFont

folder = pathlib.Path('.data/imports/sv-home/fidelity-candidate')
contract = json.loads((folder / 'presentation.json').read_text(encoding='utf-8'))
payload = json.loads((folder / 'template.json').read_text(encoding='utf-8'))
fonts = {}
previous = json.loads((folder / 'fonts.json').read_text(encoding='utf-8')) if (folder / 'fonts.json').exists() else {}
(folder / 'fonts').mkdir(exist_ok=True)
for style in contract['textStyles']:
    source = style['font']['path']
    if source not in fonts:
        path = pathlib.Path('G:/Works/SV/Development/Project') / source
        output = folder / 'fonts' / (path.stem + '.woff2')
        source_hash = hashlib.sha256(path.read_bytes()).hexdigest()
        if not output.exists() or previous.get(source, {}).get('sourceSha256') != source_hash:
            font = TTFont(path, fontNumber=0)
            font.flavor = 'woff2'
            font.save(output)
        fonts[source] = dict(assetId=hashlib.sha256(output.read_bytes()).hexdigest(), fileName=output.name,
                             sourceSha256=hashlib.sha256(path.read_bytes()).hexdigest())
    node = next(node for node in payload['design']['nodes'] if node['id'] == style['node'])
    node['fontAsset'] = fonts[source]['assetId']
    if style['outline']:
        outline = json.loads(style['outline']['json'])['MonoBehaviour']
        color = outline['OutlineColor']
        node['textOutline'] = dict(width=outline['OutlineWidth'], color='#' + ''.join(f'{round(color[k]*255):02x}' for k in 'rgba'))
(folder / 'fonts.json').write_text(json.dumps(fonts, indent=2), encoding='utf-8')
(folder / 'template.json').write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding='utf-8')
print('Prepared source font faces:', len(fonts))
