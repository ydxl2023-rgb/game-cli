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
    if name == 'UIShop':
        children = [n for n in children if n['name'] not in ['TaskText','ClaimVisible']] + [n for n in children if n['name'] in ['TaskText','ClaimVisible']]
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
    node('Room','Container',0,0,1080,730,[node('RoomSpine','Container',0,0,1080,730),
        node('Player','Container',330,380,150,210),text('NpcTitle',450,230,180,38,25,30301)]),
    text('Gold',0,0,1,1,1),
    node('Header','Image',0,650,1080,80,[text('Title',20,657,500,60,40,30324)]),
    node('MarketPanel','Image',0,730,1080,1025),
    text('SaleTitle',405,765,270,55,36,30325),
    node('LeftLine','Image',345,783,90,12),node('RightLine','Image',645,783,90,12),
    node('TierClick','Button',465,823,150,50,[text('TierLabel',475,826,105,42,25,30329)]),
    node('LogClick','Button',855,760,100,110),
    node('Offers','Container',140,1000,800,660,[node('Viewport','Container',140,1000,800,660,
        [node('Content','Container',140,1000,800,660)])]),
    text('Feedback',160,1670,760,45,27),text('TaskText',400,1815,640,50,24),
    node('ClaimVisible','Container',735,1855,280,55,[button('ClaimClick','ClaimLabel',735,1855,280,55,30319)]),
    node('Footer','Image',0,1755,1080,165,[node('CloseClick','Button',20,1780,110,95),
        node('MaterialClick','Button',150,1760,145,145,[node('MaterialIcon','Image',180,1770,75,75),
            text('MaterialLabel',150,1850,145,40,27,30326)]),
        node('EquipmentClick','Button',295,1760,145,145,[node('EquipmentIcon','Image',325,1770,75,75),
            node('Lock','Image',347,1790,35,40),text('EquipmentLabel',295,1850,145,40,27,30327)])]),
    node('TierVisible','Container',440,875,200,65,[node('TierOptionClick','Button',440,875,200,65,
        [text('TierOptionLabel',450,880,180,50,28,30329)])])
],{'hub_bg_7':['Background','Footer'],'shop_bg_1':['Header'],'goldshop_bg_10':['MarketPanel'],
   'common_line_2':['RightLine'],'common_line_3':['LeftLine'],
   'shop_button_2_1':['TierClick'],'shop_button_3_1':['TierOptionClick'],
   'cons_button_1':['LogClick'],'common_button_7':['CloseClick'],
   'Function_7':['MaterialIcon'],'Function_21':['EquipmentIcon'],'common_icon_16':['Lock'],
   'common_button_3':['ClaimClick']})
write('UIShopOffer',[
    node('Card','Image',0,0,260,550),
    text('Name',10,90,240,65,36),
    node('ItemFrame','Image',55,190,150,150),node('Icon','Image',65,200,130,130),
    node('TierRibbon','Image',50,185,63,30),text('ItemTier',50,184,63,32,23,30329),
    node('StockBack','Image',25,375,210,50),text('Stock',10,375,240,50,30),text('Owned',0,0,1,1,1),
    node('SelectClick','Button',25,525,210,100,[node('Currency','Image',55,553,44,44),
        text('Price',100,542,110,65,42)])
],{'goldshop_bg_11':['Card'],'goldshop_bg_12':['StockBack'],'common_bg_21':['ItemFrame'],'shop_title_1':['TierRibbon'],
   'common_button_3':['SelectClick'],'big_gold':['Currency']},260,650)
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
