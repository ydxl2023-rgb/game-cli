"""Author forge UGUI from recovered page sections and original image identities."""
from pathlib import Path
import json, hashlib, shutil
ROOT=Path(__file__).resolve().parents[2]; OUT=Path(__file__).parent
records={}
for file in (ROOT/'extracted-resources/fairygui').rglob('*.json'):
    row=json.loads(file.read_text(encoding='utf8'))
    if row.get('name') and file.with_suffix('.png').exists():
        records[(row.get('package_name'),row['name'])]=(file.with_suffix('.png'),row)
common=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/CommonTextConfig.bytes').read_text())['data']
language=json.loads((ROOT/'client/Assets/Data/Jsons/Excels/LanguageConfig.bytes').read_text(encoding='utf8'))['data']
manifest=json.loads((OUT/'configuration-source.json').read_text(encoding='utf8'))
ids={r[1]:r[0] for r in manifest['languages']}
imports=[]
def node(name,kind,x,y,w,h,children=None):
    return dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])
def text(name,x,y,w,h,size=32,label=None):
    n=node(name,'Text',x,y,w,h)
    context=common[str(ids['TEXT_BLACKSMITH_'+label])]['Context'] if label else ''
    content=language[context] if context.isdecimal() else context
    n['text']=dict(content=content,fontSize=size,alignment='center',color='#3E2C1E')
    return n
def button(name,label,x,y,w,h,key=None):
    return node(name,'Button',x,y,w,h,[text(label,x,y,w,h,30,key)])
def write(name,children,bindings,w=1080,h=1920):
    data=dict(schemaVersion='1',uiName=name,referenceResolution=dict(width=w,height=h),
        nodes=[node(name,'Container',0,0,w,h,children)])
    assets=[]
    for resource,targets in bindings.items():
        source,row=next(v for (_,n),v in records.items() if n==resource)
        destination=f'Assets/Data/Widgets/Blacksmith/Images/{resource}.png'
        assets.append(dict(id=resource,file='../../'+source.relative_to(ROOT).as_posix(),path=destination,usedBy=targets))
        imports.append(dict(source=source.relative_to(ROOT).as_posix(),destination=destination,
            sourceAssetID=row['id'],sha256=hashlib.sha256(source.read_bytes()).hexdigest(),uses=targets))
    (OUT/(name+'.json')).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
    (OUT/(name+'.materials.json')).write_text(json.dumps(dict(schemaVersion='1',uiName=name,assets=assets),indent=2))
write('UIBlacksmith',[
    node('InputBlock','Image',0,0,1080,1920),
    text('Gold',30,60,500,70),
    node('Background','Image',0,570,1080,1350),
    node('Header','Image',0,570,1080,95,[text('Title',30,570,1020,95,42,'TITLE')]),
    text('LocalRule',30,680,1020,55,28,'LOCAL_RULE'),
    text('Revenue',30,750,620,65),button('CollectClick','CollectLabel',700,745,330,75,'CLAIM_REVENUE'),
    node('RepairVisible','Container',80,900,920,240,[button('RepairClick','RepairText',110,950,860,100)]),
    node('CatalogVisible','Container',30,860,1020,750,[node('Entries','Container',30,860,1020,750,
        [node('Viewport','Container',30,860,1020,750,[node('Content','Container',30,860,1020,750)])])]),
    node('HeroVisible','Container',30,860,1020,750,[text('HeroText',60,940,960,180,40),
        text('EquipmentText',60,1170,960,160,36)]),
    text('Feedback',30,1630,1020,90,30),
    button('RecipesClick','RecipesLabel',40,1740,310,80,'RECIPES'),
    button('ShelfClick','ShelfLabel',385,1740,310,80,'SHELF'),
    button('HeroClick','HeroLabel',730,1740,310,80,'HERO'),
    node('Footer','Image',0,1840,1080,80,[button('CloseClick','CloseLabel',25,1845,330,70,'BACK')])
],{'common_bg_18':['Background'],'shop_bg_1':['Header'],'hub_bg_7':['Footer'],
   'common_bg_11':['RepairClick','CollectClick','RecipesClick','ShelfClick','HeroClick','CloseClick']})
write('UIBlacksmithEntry',[
    node('Card','Image',0,0,1020,195),node('Icon','Image',30,20,155,155),
    text('Title',205,15,470,65,36),text('Detail',205,95,470,65,28),
    button('SelectClick','ActionText',710,55,270,85)
],{'common_bg_11':['Card','SelectClick']},1020,195)
write('UIBlacksmithConfirm',[
    node('Shade','Image',0,0,1080,1920),node('Panel','Image',70,400,940,1120),
    node('Icon','Image',130,500,200,200),text('Title',350,500,580,120,40),
    text('Cost',130,740,820,240,34),
    node('QuantityVisible','Container',120,1020,840,90,[button('MinusClick','MinusLabel',130,1020,120,80,'MINUS'),
        text('Quantity',270,1020,540,80,34),button('PlusClick','PlusLabel',830,1020,120,80,'PLUS')]),
    text('Feedback',120,1130,840,90,30),
    button('ConfirmClick','ConfirmText',230,1260,620,100),
    button('CancelClick','CancelLabel',330,1400,420,75,'CANCEL')
],{'common_bg_11':['Panel','MinusClick','PlusClick','ConfirmClick','CancelClick']})
for sheet in manifest['sheets']:
    if sheet['name'] not in ['BlacksmithRecipe','BlacksmithMaterial']:continue
    for r in sheet['rows']:
        destination=ROOT/'client'/r[-1]
        source,meta=next(v for (_,n),v in records.items() if n==destination.stem)
        destination.parent.mkdir(parents=True,exist_ok=True)
        if not destination.exists():shutil.copyfile(source,destination)
        imports.append(dict(source=source.relative_to(ROOT).as_posix(),destination=r[-1],
            sourceAssetID=meta['id'],sha256=hashlib.sha256(source.read_bytes()).hexdigest(),uses=[sheet['name']+'/'+str(r[0])]))
(OUT/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf8')
