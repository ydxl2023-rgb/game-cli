"""Recover first-level forge rules and record source evidence before implementation."""
from pathlib import Path
import json, hashlib, sys

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).parent
TABLES = ROOT/'research/config-study/tables'
def read(name):
    return json.loads((TABLES/(name+'.json')).read_text(encoding='utf8'))
def table(name, columns, rows):
    return dict(name=name, columns=[[n,t,n] for n,t in columns], rows=rows)

level = next(r for r in read('tbbuildinglevel') if r['ID']==60002001)
feature = next(r for r in read('tbfeatureunlock') if r['ID']['value']==1005)
recipes = [r for r in read('tbheroequip') if r['ID'] in level['Effect']['NormalFormula']]
heroes = read('tbhero')
items = {r['Id']:r for r in read('tbitem')}
source_language = {r['ID']:r['SimpleChinese'] for r in read('tblanguage')}
languages=[]
rows=[]
costs=[]
for r in recipes:
    text_id=600000+r['ID']
    rows.append([r['ID'],1,text_id,r['Trade']['value'],r['Price'],
        'Assets/Data/Widgets/Common/Icons/'+r['Icon'].split('/')[-1]+'.png'])
    languages.append([text_id,'TEXT_FORGE_RECIPE_'+str(r['ID']),r['EquipmentName']])
    for i,c in enumerate(r['FormulaCost'],1):
        costs.append([r['ID']*10+i,r['ID'],c['Id'],c['Num']])
item_rows=[]
for id in sorted({r[2] for r in costs}):
    item=items[id]; text_id=800000+id
    item_rows.append([id,text_id,'Assets/Data/Widgets/Common/Icons/'+item['Icon'].split('/')[-1]+'.png'])
    languages.append([text_id,'TEXT_FORGE_MATERIAL_'+str(id),source_language.get(item['Name'],item['Name'])])
labels={
    'TITLE':'铁匠铺','LOCKED':'完成并领取任务 60329 后开放铁匠铺',
    'REPAIR':'修复铁匠铺 · {0}金币','REPAIRED':'铁匠铺修复完成',
    'RECIPES':'配方','SHELF':'货架','HERO':'英雄','FORGE':'打造',
    'QUANTITY':'打造数量 {0}','COST':'{0}：{1} / {2}',
    'SUCCESS':'打造完成，装备已放入货架','NO_MATERIAL':'材料或金币不足',
    'NO_SELECTION':'请先选择配方或装备','SELL':'出售给英雄',
    'SOLD':'交易完成，英雄已装备武器','INCOMPATIBLE':'当前英雄职业不适用该武器',
    'NO_HERO':'尚未招募英雄','NO_GOLD':'英雄金币不足','NO_STOCK':'该装备已不在货架上',
    'REPAIR_FIRST':'请先修复铁匠铺','BACK':'返回村庄','PRICE':'售价 {0} 金币',
    'OWNED':'货架 {0} 件','EQUIPPED':'已装备：{0}','EMPTY':'暂无装备',
    'HERO_GOLD':'英雄金币 {0}','LOCAL_RULE':'首级武器 · 打造并出售给英雄',
    'FORGING':'打造中…','MINUS':'−','PLUS':'+','CONFIRM':'确认打造','CANCEL':'取消',
    'GOLD':'金币 {0}','NO_REPAIR_GOLD':'金币不足，无法修复铁匠铺',
    'TRADE':'职业 {0}','SELECT':'选择','DETAIL':'选择配方后确认材料和数量',
    'HERO_NAME':'{0}','SELL_CONFIRM':'出售并装备 · {0}金币',
    'REVENUE':'铁匠收入 {0}金币','CLAIM_REVENUE':'收取收入'}
languages += [[30601+i,'TEXT_BLACKSMITH_'+key,value] for i,(key,value) in enumerate(labels.items())]
hero_rows=[]
for h in heroes:
    text_id=900000+h['ID']
    hero_rows.append([h['ID'],h['TradeType']['value'],text_id])
    languages.append([text_id,'TEXT_FORGE_HERO_'+str(h['ID']),h['Name']])
sheets=[table('BlacksmithSetting',[('ID','int'),('RequiredTaskID','int'),('RepairCurrencyID','int'),
    ('RepairCost','int'),('MaxQuantity','int'),('ForgeSeconds','float')],
    [[60002,feature['UnlockNeedCondition'][0]['Value'],level['BuildingUpCost'][0]['Id'],
      level['BuildingUpCost'][0]['Num'],10,1]]),
    table('BlacksmithRecipe',[('ID','int'),('Level','int'),('NameTextID','int'),('Trade','int'),('Price','int'),('IconPath','string')],rows),
    table('BlacksmithCost',[('ID','int'),('RecipeID','int'),('ItemID','int'),('Amount','int')],costs),
    table('BlacksmithMaterial',[('ID','int'),('NameTextID','int'),('IconPath','string')],item_rows),
    table('BlacksmithHero',[('ID','int'),('Trade','int'),('NameTextID','int')],hero_rows)]
sources=[TABLES/(n+'.json') for n in ['tbbuildinglevel','tbfeatureunlock','tbheroequip','tbhero','tbitem','tblanguage']]
sources += [ROOT/'research/code-study/managed/decompiled/HotUpdate/Zombie'/f'{n}.cs'
            for n in ['ForgeProxy','UIEquipmentShopView','UIForgeView','UIForgingView']]
manifest=dict(sheets=sheets,languages=languages,sources=[dict(path=p.relative_to(ROOT).as_posix(),
    sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in sources],
    offlineRules='First level only; deterministic base equipment without affixes; immediate local hero trade; replaced equipment returns to shelf; income collected separately. No recovered server roll or combat power formula claimed.')
(OUT/'configuration-source.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
sys.path.insert(0,str(ROOT/'tools/opening'))
from audit_original_ui import package
layout=package(ROOT/'extracted-resources/assets/data/ui/atlas/forge/forge_fui.bytes',
    ['EquipmentShopPage','ForgePage','ForgingPage','ShopPaperCom','ShopShelfCom','ShopHeroCom'])
(OUT/'original-ui-audit.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf8')
print('Recovered',len(recipes),'recipes and',len(hero_rows),'hero professions.')
