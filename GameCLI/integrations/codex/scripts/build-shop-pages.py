"""Build the extracted SV shop page and product-card prefabs through UnityCLI."""
import json
import os
import subprocess
from pathlib import Path
from shared_icon_path import exported_image_path

base = Path(__file__).resolve().parents[1]
project = base.parents[1] / 'examples/unitys/development/client'
cli = os.getenv('UNITYCLI', 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe')
source = base / '.data/imports/sv-shop/full-designs'
cards = ['GoodField','LongStripField','GiftBagField','RechargeGifField','UIShopWallPaperItem']
pages = ['ShopEverydaySaleView','SetterGiftView','NewPeopleGiftView','MonthCardView','Daily','Gift','ShopGrowSaleView','WallpaperMallView','GrowthFindsView','GrowthFundDetailView','SetterGiftListView']
for name in cards + pages:
    folder = source / name
    d = json.loads((folder/'template.json').read_text(encoding='utf8'))['design']
    by_parent = {}
    images = {}
    for n in d['nodes']:
        by_parent.setdefault(n['parent'], []).append(n)
        if n.get('image'):
            images.setdefault(n['image'], []).append(n)
    def convert(n, x=0, y=0):
        x += n['x']
        y += n['y']
        view = dict(id=n['id'],name=n['name'],element=n['type'],active=n.get('active',True),
                    rect=dict(x=x,y=y,width=n['width'],height=n['height']),rectTransform=n['rectTransform'],
                    style=dict(color='#ffffff00' if n.get('surface')=='transparent' and not n.get('image') else n['color'],
                               opacity=n.get('opacity',1),sprite=n.get('image',''),imageType=n.get('imageType','simple')),
                    children=[convert(c,x,y) for c in by_parent.get(n['id'],[])])
        if n['type']=='Text':
            view['text']=dict(content=n['text'],fontSize=n['fontSize'],color=n['color'],alignment=n.get('textAlign','left').title())
        return view
    assets = []
    for key, nodes in images.items():
        asset=dict(id=key,file='../assets/'+key+'.png',path=exported_image_path('Shop',key),usedBy=[n['id'] for n in nodes])
        border=next((n['imageBorder'] for n in nodes if n.get('imageBorder')),None)
        if border: asset['spriteBorder']=[border[k] for k in 'xyzw']
        assets.append(asset)
    header=dict(schemaVersion='1.0',uiName='UI'+name.removesuffix('View'),referenceResolution=dict(width=1920,height=1080))
    for filename,data in [('visual-ui.json',dict(nodes=[convert(by_parent[None][0])])),('asset-manifest.json',dict(assets=assets))]:
        (folder/filename).write_text(json.dumps(header|data,ensure_ascii=False),encoding='utf8')
    target='Assets/Data/Widgets/'+('Common/Micros/' if name in cards else 'Shop/Pages/')+name+'.prefab'
    args=[cli,'--project',str(project),'--format','json','ugui','UI'+name.removesuffix('View'),'--json',str(folder/'visual-ui.json'),'--manifest',str(folder/'asset-manifest.json'),'--prefab',target,'--no-scene-replace']
    for flags in [['--dry-run'],[]]:
        reply=json.loads(subprocess.run(args+flags,capture_output=True,encoding='utf8').stdout)
        if not reply['ok']: raise RuntimeError(reply['message'])
    print(name,flush=True)



