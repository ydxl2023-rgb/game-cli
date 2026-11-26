"""Restore separate original Login artwork at the product's 1080 x 1920 resolution."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
document = json.loads((OUT / 'login.json').read_text('utf8'))
panel = document['nodes'][0]['children'][1]
children = panel['children']
children[:] = [node for node in children if node['name'] != 'ModeLabel']

def node(name, kind, x, y, width, height, children=None):
    return dict(id=name, name=name, element=kind,
                rect=dict(x=x, y=y, width=width, height=height), children=children or [])

entry = next(n for n in children if n['name'] == 'EnterVisible')
entry['rect'] = dict(x=180, y=1604, width=720, height=108)
button = entry['children'][0]
button['rect'] = entry['rect'].copy()
button['children'] = []
for item in children:
    if item['name'] in ('LoadingVisible', 'FailedVisible'):
        item['rect']['y'] = 1590 if item['name'] == 'LoadingVisible' else 1510
        for label in item['children']:
            label['rect'] = item['rect'].copy()
children[:] = [n for n in children if n['name'] not in ('Logo', 'AgeRating', 'Notice', 'Legal')]
notice_caption = node('NoticeCaption', 'Text', 954, 290, 94, 28)
notice_caption['text'] = dict(content='公告', fontSize=20, alignment='center', color='#E9D9AF')
children.extend([
    node('Logo', 'Image', 270, 130, 540, 313),
    node('AgeRating', 'Image', 20, 114, 86, 113),
    node('Notice', 'Image', 954, 206, 94, 92, [notice_caption]),
    node('Legal', 'Image', 180, 1718, 720, 192),
])
assets = []
for name, source, filename in [
    ('Background', 'assets/data/ui/background/003.png', 'background.png'),
    ('EnterClick', 'fairygui/Login/images/Login_button_1.png', 'enter.png'),
    ('Logo', 'fairygui/Login/images/20260522-101849.png', 'logo.png'),
    ('AgeRating', 'fairygui/Login/images/Login_bg_6.png', 'age.png'),
    ('Notice', 'fairygui/Login/images/Login_icon_3.png', 'notice.png'),
    ('Legal', 'fairygui/Login/images/Login_bg_5.png', 'legal.png'),
]:
    assets.append(dict(id=name, file='../../extracted-resources/' + source,
                       path='Assets/Data/Widgets/Login/LoginAssets/' + filename, usedBy=[name]))
(OUT / 'login.json').write_text(json.dumps(document, ensure_ascii=False, indent=2), 'utf8')
(OUT / 'materials.json').write_text(json.dumps(dict(schemaVersion='1', uiName='UILogin', assets=assets),
                                             ensure_ascii=False, indent=2), 'utf8')
