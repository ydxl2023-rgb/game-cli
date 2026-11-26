"""Generate formal UGUI inputs from recovered individual assets and authored hospital sections."""
from pathlib import Path
import json
import shutil
import hashlib

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
records = json.loads((OUT/'resource-index.json').read_text(encoding='utf-8'))['resources']
sources = {r['name']:r for r in records if r['png']}


def node(name, kind, x, y, w, h, children=None):
    return dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])


def label(name,x,y,w,h,size=32):
    n = node(name,'Text',x,y,w,h)
    static = {'CloseLabel':30210,'ClaimLabel':30124,'MaterialTitle':30221,'ProductTitle':30222,'RepairLabel':30203,
              'ForgeTitle':30224,'HotelTitle':30225,'AuctionTitle':30226}
    common = json.loads((ROOT/'client/Assets/Data/Jsons/Excels/CommonTextConfig.bytes').read_text())['data']
    language = json.loads((ROOT/'client/Assets/Data/Jsons/Excels/LanguageConfig.bytes').read_text())['data']
    content = language[common[str(static[name])]['Context']] if name in static else ''
    n['text'] = dict(content=content,fontSize=size,alignment='center',color='#F7E7C4' if name in ['Gold','Treatment','Title','Revenue'] else '#392B20')
    return n


def button(name,title,x,y,w,h):
    return node(name,'Button',x,y,w,h,[label(title,x,y,w,h)])


def write(name,children,bindings):
    design = dict(schemaVersion='1',uiName=name,referenceResolution=dict(width=1080,height=1920),
                  nodes=[node(name,'Container',0,0,1080,1920,children)])
    assets = []
    for resource,targets in bindings.items():
        source = sources[resource]
        shared = 'Common/Icons' if 'CommonIcon' in source['png'] else 'Home/Hospital'
        assets.append(dict(id=resource,file='../../'+source['png'].replace('\\','/'),
                           path=f'Assets/Data/Widgets/{shared}/{resource}.png',usedBy=targets))
    (OUT/(name+'.json')).write_text(json.dumps(design,ensure_ascii=False,indent=2),encoding='utf-8')
    (OUT/(name+'.materials.json')).write_text(json.dumps(dict(schemaVersion='1',uiName=name,assets=assets),indent=2),encoding='utf-8')


write('UIHomeHospital',[
    node('PageBackground','Image',0,0,1080,1920),
    node('Room','Container',0,0,1080,650,[node('RoomSpine','Container',0,0,1080,650),
        node('Patient','Container',680,470,170,190),node('ProductionTarget','Container',535,395,10,10)]),
    label('Gold',30,40,500,70),label('Treatment',30,545,630,90,28),
    node('Header','Image',0,650,1080,90,[label('Title',20,650,480,90,42),label('Revenue',520,650,530,90,28)]),
    node('BonusPanel','Image',30,770,1020,140,[label('Bonus',40,785,1000,110,32)]),
    node('ProductPanel','Image',30,940,1020,320,[label('ProductTitle',60,950,220,55),
        node('ProductIcon','Image',80,1015,180,180),label('ProductName',310,985,680,70,36),
        label('ProductStock',310,1060,680,55),label('Detail',300,1120,710,120,29)]),
    node('MaterialPanel','Image',0,1330,1080,250,[label('MaterialTitle',40,1290,230,65),
        node('CraftClick','Button',75,1360,200,190,[node('MaterialIcon','Image',90,1365,170,165)]),
        label('MaterialStock',320,1360,700,75),label('Feedback',300,1440,720,110,29)]),
    node('FlyingMaterial','Image',90,1365,100,100),
    label('TaskText',40,1610,1000,95,31),
    node('ClaimVisible','Container',680,1720,340,80,[button('ClaimClick','ClaimLabel',680,1720,340,80)]),
    node('Footer','Image',0,1820,1080,100,[button('CloseClick','CloseLabel',30,1830,320,75)])
], {'common_bg_18':['PageBackground'],'shop_bg_1':['Header'],'common_bg_8_0':['BonusPanel'],
    'common_bg_11':['ProductPanel'],'cons_bg_6':['MaterialPanel'],'hub_bg_7':['Footer'],
    'cons_bg_7':['CraftClick','ClaimClick','CloseClick'],'HpMaterial_1':['MaterialIcon','FlyingMaterial'],
    'HpItem_1':['ProductIcon']})
write('UIHomeHospitalEntry',[
    node('EntryVisible','Container',410,700,260,180,[
        node('RepairVisible','Container',410,700,260,180,[node('RepairClick','Button',410,700,260,180,[
            label('RepairLabel',440,707,200,65,38),node('Currency','Image',450,780,60,60),
            label('Cost',515,780,105,60,34)])]),
        node('BuiltVisible','Container',492,688,96,102,[node('EnterClick','Button',492,688,96,102)]),
        label('Feedback',240,900,600,100,30)]),node('Coin','Image',500,900,65,65),
    label('BuildingName',455,835,170,38,26),
    label('ForgeTitle',455,835,170,38,26),
    label('HotelTitle',455,835,170,38,26),
    label('AuctionTitle',455,835,170,38,26),
    node('MerchantEntry','Container',492,688,96,102,[node('MerchantClick','Button',492,688,96,102)]),
    node('ForgeEntry','Container',492,688,96,102,[node('ForgeClick','Button',492,688,96,102)]),
    node('HotelEntry','Container',492,688,96,102,[node('HotelClick','Button',492,688,96,102)])
], {'hub_bg_4_1':['RepairClick'],'hub_button_15_0':['EnterClick','MerchantClick','ForgeClick','HotelClick'],
    'big_gold':['Currency','Coin']})

imports = []
selection = 'extracted-resources/assets/data/effects/textures/e_npc_xuanze_1.png'
selection_target = 'Assets/Data/Scenes/Home/Opening/e_npc_xuanze_1.png'
shutil.copyfile(ROOT/selection,ROOT/'client'/selection_target)
imports.append(dict(source=selection,destination=selection_target,uses=['Hospital, forge, hotel and auction entrance ground selection']))
for source in sources.values():
    if 'CommonIcon' not in source['png']:
        continue
    src = ROOT/source['png']
    dst = ROOT/'client/Assets/Data/Widgets/Common/Icons'/src.name
    dst.parent.mkdir(parents=True,exist_ok=True)
    if not dst.exists():
        shutil.copyfile(src,dst)
    imports.append(dict(sourceAssetID=source['sourceAssetID'],source=source['png'],
                        destination=str(dst.relative_to(ROOT/'client')),sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
name = 'e_ui_bg_priest_1'
destination = ROOT/'client/Assets/Data/Widgets/Home/Hospital/Spine'
destination.mkdir(parents=True,exist_ok=True)
for suffix in ['.atlas.txt','.skel.bytes','.png']:
    src = ROOT/f'extracted-resources/assets/data/spines/{name}/{name}{suffix}'
    dst = destination/src.name
    shutil.copyfile(src,dst)
    imports.append(dict(source=str(src.relative_to(ROOT)),destination=str(dst.relative_to(ROOT/'client')),
                        sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf-8')
print('Prepared hospital page, world entry and original Spine dependencies.')
