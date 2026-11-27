"""Author portrait trading UI using recovered textures, item icons and trading Spine."""
from pathlib import Path
import json, shutil, hashlib
ROOT=Path(__file__).resolve().parents[2]; OUT=Path(__file__).parent
records={}
for f in (ROOT/'extracted-resources/fairygui').rglob('*.json'):
    row=json.loads(f.read_text(encoding='utf8'))
    if row.get('name') and f.with_suffix('.png').exists():
        records[(row.get('package_name'),row['name'])]=(f.with_suffix('.png'),row)
manifest=json.loads((OUT/'configuration-source.json').read_text(encoding='utf8'))
common=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/CommonTextConfig.bytes').read_text(encoding='utf8'))['data']
language=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/LanguageConfig.bytes').read_text(encoding='utf8'))['data']
imports=[]
def node(name,kind,x,y,w,h,children=None):
    return dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])
def text(name,x,y,w,h,size=30,id=None):
    n=node(name,'Text',x,y,w,h)
    context=common[str(id)]['Context'] if id else ''
    content=language[context] if context.isdecimal() else context
    n['text']=dict(content=content,fontSize=size,alignment='center',color='#3E2C1E')
    return n
def button(name,label,x,y,w,h,id):
    return node(name,'Button',x,y,w,h,[text(label,x,y,w,h,30,id)])
def write(name,children,bindings,w=1080,h=1920):
    data=dict(schemaVersion='1',uiName=name,referenceResolution=dict(width=w,height=h),
              nodes=[node(name,'Container',0,0,w,h,children)])
    assets=[]
    for resource,targets in bindings.items():
        source,row=next((v for (package,n),v in records.items() if n==resource))
        target=f'Assets/Data/Widgets/Shop/Images/{resource}.png'
        assets.append(dict(id=resource,file='../../'+source.relative_to(ROOT).as_posix(),path=target,usedBy=targets))
        imports.append(dict(source=source.relative_to(ROOT).as_posix(),sourceAssetID=row['id'],destination=target,
            sha256=hashlib.sha256(source.read_bytes()).hexdigest(),uses=targets))
    (OUT/(name+'.json')).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
    (OUT/(name+'.materials.json')).write_text(json.dumps(dict(schemaVersion='1',uiName=name,assets=assets),indent=2))
write('UIShop',[
    node('Background','Image',0,0,1080,1920),
    node('Room','Container',0,0,1080,650,[node('RoomSpine','Container',0,0,1080,650)]),
    text('Gold',25,50,550,70,34),
    node('Header','Image',0,650,1080,95,[text('Title',20,650,1040,95,42,30301)]),
    text('TierLabel',30,765,500,70,32,30323),text('EquipmentLabel',530,765,520,70,28,30303),
    node('Offers','Container',30,855,1020,710,[node('Viewport','Container',30,855,1020,710,
        [node('Content','Container',30,855,1020,710)])]),
    text('Feedback',30,1575,1020,65,30),text('TaskText',30,1650,1020,90,30),
    node('ClaimVisible','Container',700,1750,330,75,[button('ClaimClick','ClaimLabel',700,1750,330,75,30319)]),
    node('Footer','Image',0,1820,1080,100,[button('CloseClick','CloseLabel',25,1830,330,75,30309)])
],{'common_bg_18':['Background'],'shop_bg_1':['Header'],'hub_bg_7':['Footer'],
   'cons_bg_7':['CloseClick','ClaimClick']})
write('UIShopOffer',[
    node('Card','Image',0,0,1020,220),node('Icon','Image',30,35,150,150),
    text('Name',200,10,450,65,36),text('Stock',200,85,450,50,28),text('Owned',200,145,450,50,28),
    node('SelectClick','Button',690,65,290,100,[node('Currency','Image',710,85,60,60),
        text('Price',775,75,190,80,32)])
],{'common_bg_11':['Card'],'cons_bg_7':['SelectClick'],'big_gold':['Currency']},1020,220)
write('UIShopPurchase',[
    node('Shade','Image',0,0,1080,1920),node('Panel','Image',75,490,930,980),
    node('Icon','Image',120,565,170,170),text('Name',315,535,600,80,42),
    text('Description',315,625,600,130,28),text('Stock',120,780,400,60),text('Owned',550,780,400,60),
    text('QuantityLabel',240,860,600,65,32,30310),
    button('MinusClick','MinusLabel',210,950,130,90,30320),
    text('Quantity',350,950,380,90,40),button('PlusClick','PlusLabel',740,950,130,90,30321),
    text('Total',170,1070,740,80,36),text('Feedback',120,1160,840,90,30),
    button('ConfirmClick','ConfirmLabel',340,1270,400,90,30322),
    button('CancelClick','CancelLabel',360,1400,360,70,30308)
],{'common_bg_18':['Shade'],'common_bg_11':['Panel'],'cons_bg_7':['MinusClick','PlusClick','ConfirmClick','CancelClick']})
for row in manifest['sheets'][0]['rows']:
    filename=Path(row[-1]).stem
    source,meta=records[('CommonIcon',filename)]
    destination=ROOT/'client'/row[-1]; destination.parent.mkdir(parents=True,exist_ok=True)
    if not destination.exists(): shutil.copyfile(source,destination)
    imports.append(dict(source=source.relative_to(ROOT).as_posix(),sourceAssetID=meta['id'],destination=row[-1],
        sha256=hashlib.sha256(source.read_bytes()).hexdigest(),uses=[f'ShopGoods/{row[0]}/IconPath']))
name='e_ui_bg_trading_1'; folder=ROOT/'client/Assets/Data/Widgets/Shop/Spine';folder.mkdir(parents=True,exist_ok=True)
for suffix in ['.atlas.txt','.skel.bytes','.png']:
    source=ROOT/f'extracted-resources/assets/data/spines/{name}/{name}{suffix}';destination=folder/source.name
    shutil.copyfile(source,destination)
    imports.append(dict(source=source.relative_to(ROOT).as_posix(),destination=destination.relative_to(ROOT/'client').as_posix(),
        sha256=hashlib.sha256(source.read_bytes()).hexdigest(),uses=['Trading room animated interior']))
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf8')
print('Prepared trading catalog, reusable offer cell, confirmation and original interior.')
