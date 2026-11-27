"""Trace first-tier trading goods and the authored market UI to recovered client data."""
from pathlib import Path
import json, hashlib, sys
ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).parent
TABLES = ROOT/'research/config-study/tables'
def read(name):
    return json.loads((TABLES/(name+'.json')).read_text(encoding='utf8'))
items = {r['Id']:r for r in read('tbitem')}
goods = [r for r in read('tbshop') if r['ShopType']['name']=='AuctionHouse' and items[r['Goods']['Id']]['Tier']==1]
feature = next(r for r in read('tbfeatureunlock') if r['ID']['value']==1025)
assert feature['UnlockNeedCondition'][0]['Type']['name']=='ChapterTask'
rows=[]
languages=[]
for good in goods:
    item=items[good['Goods']['Id']]
    text_id=310000+good['Id']*2
    rows.append([good['Id'],item['Id'],good['Goods']['Num'],good['Price'][0]['Id'],good['Price'][0]['Num'],
                 good['Sort'],good['Limit'],item['Tier'],text_id,text_id+1,
                 'Assets/Data/Widgets/Common/Icons/'+item['Icon'].split('/')[-1]+'.png'])
    languages.extend([[text_id,f'TEXT_TRADE_ITEM_{item["Id"]}',item['Name']],
                      [text_id+1,f'TEXT_TRADE_DESC_{item["Id"]}',item['Desc']]])
labels=['交易行','材料','装备交易尚未开放','金币 {0}','剩余 {0}','已拥有 {0}','购买','取消','返回村庄',
        '购买数量','总价 {0} 金币','金币不足','商品余量不足','请先完成医院修复任务','购买成功','请选择商品',
        '数量超出可购买范围','任务：{0}','领取奖励','−','+','确认购买','10级材料','1级交易行','待售材料','材料拍卖','装备拍卖','交易日报尚未接入','10级']
aliases=['TITLE','MATERIALS','EQUIPMENT_PENDING','GOLD','REMAINING','OWNED','BUY','CANCEL','BACK',
         'QUANTITY','TOTAL','NO_GOLD','SOLD_OUT','LOCKED','SUCCESS','NO_SELECTION','INVALID_QUANTITY',
         'TASK','CLAIM','MINUS','PLUS','CONFIRM','TIER','LEVEL_TITLE','SALE_TITLE','MATERIAL_TAB','EQUIPMENT_TAB','LOG_PENDING','TIER_SHORT']
languages += [[30301+i,'TEXT_TRADE_'+aliases[i],t] for i,t in enumerate(labels)]
sheets=[dict(name='ShopGoods',columns=[['ID','int','原商品ID'],['ItemID','int','物品ID'],['Amount','int','每份数量'],
    ['CurrencyID','int','货币ID'],['Price','int','每份价格'],['Order','int','原排序'],['Limit','int','原配置限购上限'],
    ['Tier','int','原物品档位'],['NameTextID','int','名称语言ID'],['DescriptionTextID','int','说明语言ID'],
    ['IconPath','string','共享图标']],rows=rows),
    dict(name='ShopSetting',columns=[['ID','int','交易行建筑ID'],['RequiredTaskID','int','原开放任务'],
         ['MaxQuantity','int','本地单次购买上限'],['Tier','int','首版开放档位']],
         rows=[[60019,feature['UnlockNeedCondition'][0]['Value'],100,1]])]
sources=[TABLES/(n+'.json') for n in ['tbshop','tbitem','tbfeatureunlock']]
manifest=dict(sheets=sheets,languages=languages,sources=[dict(path=p.relative_to(ROOT).as_posix(),
    sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in sources],
    offlineAdaptation='First-tier offers use client config limits, no server restock or hero sales; per-order cap 100. Existing herb purchases migrate into offer 6001 usage.')
(OUT/'configuration-source.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
sys.path.insert(0,str(ROOT/'tools/opening'))
from audit_original_ui import package
layout=package(ROOT/'extracted-resources/assets/data/ui/atlas/shop/shop_fui.bytes',
               ['TradeMarketPage','TradeMarketScene','TradeMarketItem','ShopBuyPage'])
(OUT/'original-ui-audit.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf8')
print('Recorded',len(rows),'first-tier original offers and',len(layout['components']),'UI components.')
