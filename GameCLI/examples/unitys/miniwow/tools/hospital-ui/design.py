"""Generate formal UGUI inputs from recovered individual assets and authored hospital sections."""
from pathlib import Path
import json
import shutil
import hashlib

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
records = json.loads((OUT/'resource-index.json').read_text(encoding='utf-8'))['resources']
sources = {r['name']:r for r in records if r['png']}

for name in ['small_zuanshi','small_time']:
    path=f'extracted-resources/fairygui/CommonIcon/{name}.png'
    meta=json.loads((ROOT/path).with_suffix('.json').read_text(encoding='utf8'))
    sources[name]=dict(name=name,png=path,sourceAssetID=meta['id'])


def node(name, kind, x, y, w, h, children=None):
    return dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])


def label(name,x,y,w,h,size=32):
    n = node(name,'Text',x,y,w,h)
    static = {'CloseLabel':30210,'ClaimLabel':30124,'MaterialTitle':30406,'ProductTitle':30407,'RepairLabel':30203,
              'ForgeTitle':30224,'HotelTitle':30225,'AuctionTitle':30226,'NpcTitle':30201,
              'UpgradeHint':30401,'AutoLabel':30402,'EmptyText':30403,'CraftHint':30404}
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
        shared = 'Common/Icons' if 'CommonIcon' in source['png'] else 'Hospital/Resources'
        assets.append(dict(id=resource,file='../../'+source['png'].replace('\\','/'),
                           path=f'Assets/Data/Widgets/{shared}/{resource}.png',usedBy=targets))
    (OUT/(name+'.json')).write_text(json.dumps(design,ensure_ascii=False,indent=2),encoding='utf-8')
    (OUT/(name+'.materials.json')).write_text(json.dumps(dict(schemaVersion='1',uiName=name,assets=assets),indent=2),encoding='utf-8')


def text(name,x,y,w,h,size=28,color='#725939'):
    n=label(name,x,y,w,h,size)
    n['text'].update(color=color)
    return n

write('UIHomeHospital',[
    node('PageBackground','Image',0,0,1080,1920),
    node('Room','Container',0,0,1080,650,[node('RoomSpine','Container',0,0,1080,650),
        node('Priest','Container',440,260,200,220),node('CounterSpine','Container',0,0,1080,650),
        node('Patient','Container',680,470,170,190),node('ProductionTarget','Container',535,395,10,10),
        text('NpcTitle',450,245,180,35,25,'#41B335')]),
    node('GoldIcon','Image',25,145,45,45),label('Gold',72,142,130,48,27),
    node('GemIcon','Image',235,145,42,42),label('Gem',280,142,130,48,27),
    label('Treatment',250,535,580,50,24),
    node('Header','Image',0,650,1080,80,[label('Title',15,660,300,55,36),
        node('RevenueIcon','Image',690,667,48,48),label('Revenue',742,664,145,50,28),
        node('UpgradeClick','Button',900,658,155,65)]),
    node('BonusPanel','Image',125,780,830,175,[
        node('PriceIcon','Image',145,790,55,55),label('PriceBonus',205,792,150,50,27),
        node('TimeIcon','Image',430,790,55,55),label('TimeBonus',490,792,140,50,27),
        node('CapacityIcon','Image',715,790,55,55),label('CapacityBonus',775,792,120,50,27),
        text('UpgradeHint',155,880,735,45,25)]),
    node('ProductPanel','Image',120,960,835,290,[
        node('ProductTab','Image',115,950,115,55),text('ProductTitle',120,957,105,40,25,'#FFF2D5'),
        node('AutoCraftClick','Button',770,975,42,42),text('AutoLabel',815,975,135,42,22),
        node('EmptyVisible','Container',220,1075,635,75,[text('EmptyText',230,1085,615,55,27)]),
        node('ProductsVisible','Container',150,1030,765,190,[node('ProductIcon','Image',170,1040,125,140),
            label('ProductName',325,1020,560,55,29),label('ProductStock',325,1080,560,45,25),
            label('Detail',315,1125,585,80,22)])]),
    node('MaterialPanel','Image',0,1535,1080,125,[
        node('MaterialTab','Image',120,1510,125,55),text('MaterialTitle',125,1517,115,40,23,'#FFF2D5'),
        node('CraftClick','Button',465,1545,150,115,[node('MaterialIcon','Image',482,1545,110,90),
            label('MaterialStock',492,1620,95,35,23)])]),
    node('FlyingMaterial','Image',482,1545,90,90),
    text('CraftHint',200,1685,680,45,25),
    label('Feedback',170,1715,740,40,23),
    node('Footer','Image',0,1755,1080,165,[node('CloseClick','Button',20,1780,110,95),
        label('TaskText',160,1775,730,50,24),
        node('ClaimVisible','Container',725,1840,300,65,[button('ClaimClick','ClaimLabel',725,1840,300,65)])])
], {'common_bg_18':['PageBackground'],'shop_bg_1':['Header'],'common_bg_8_0':['BonusPanel'],
    'common_bg_11':['ProductPanel'],'cons_bg_6':['MaterialPanel'],'hub_bg_7':['Footer'],
    'cons_bg_7':['ClaimClick'],'HpMaterial_1':['MaterialIcon','FlyingMaterial'],'HpItem_1':['ProductIcon'],
    'common_button_7':['CloseClick'],'shop_button_8':['UpgradeClick'],'common_title_16':['ProductTab','MaterialTab'],
    'big_gold':['GoldIcon','RevenueIcon','PriceIcon'],'small_zuanshi':['GemIcon'],'small_time':['TimeIcon']})
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
destination = ROOT/'client/Assets/Data/Widgets/Hospital/Resources/Spine'
destination.mkdir(parents=True,exist_ok=True)
for suffix in ['.atlas.txt','.skel.bytes','.png']:
    src = ROOT/f'extracted-resources/assets/data/spines/{name}/{name}{suffix}'
    dst = destination/src.name
    shutil.copyfile(src,dst)
    imports.append(dict(source=str(src.relative_to(ROOT)),destination=str(dst.relative_to(ROOT/'client')),
                        sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf-8')
print('Prepared hospital page, world entry and original Spine dependencies.')

priest = ROOT/'extracted-resources/assets/data/spines/npc_priest_2'
target = ROOT/'client/Assets/Data/Widgets/Hospital/Resources/Priest'
target.mkdir(parents=True,exist_ok=True)
for name in ['npc_priest_2.skel.bytes','npc_priest.atlas.txt','npc_priest.png']:
    shutil.copyfile(priest/name,target/name)

atlas = target/'npc_priest.atlas.txt'
atlas.write_text(atlas.read_text(encoding='utf8').replace('NPC_priest.png','npc_priest.png'),encoding='utf8')

for entry in json.loads((OUT/'UIHomeHospital.materials.json').read_text())['assets']:
    original=(OUT/entry['file']).resolve()
    imports.append(dict(source=str(original.relative_to(ROOT)),destination=entry['path'],
        sourceAssetID=sources[entry['id']]['sourceAssetID'],uses=entry['usedBy'],
        sha256=hashlib.sha256(original.read_bytes()).hexdigest()))
for name in ['npc_priest_2.skel.bytes','npc_priest.atlas.txt','npc_priest.png']:
    original=priest/name
    imported=target/name
    imports.append(dict(source=str(original.relative_to(ROOT)),destination=str(imported.relative_to(ROOT/'client')),
        sha256=hashlib.sha256(original.read_bytes()).hexdigest(),
        importedSha256=hashlib.sha256(imported.read_bytes()).hexdigest(),
        conversion='Atlas page NPC_priest.png normalized to imported texture name npc_priest.png' if name.endswith('.txt') else 'Original bytes',
        uses=['Hospital independent priest Spine']))
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf8')
