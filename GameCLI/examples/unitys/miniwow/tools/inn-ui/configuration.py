"""Append original inn rows to the shared building configuration before spreadsheet export."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[2]
def extend(sheets, languages, unresolved):
    def read(name):
        return json.loads((ROOT/'research/config-study/tables'/f'{name}.json').read_text(encoding='utf8'))
    sheets.append(dict(name='HomeInnSetting',columns=[['ID','int','旅店建筑ID'],['MaterialCount','int','本地每批原料数量'],['OutputCount','int','本地每批成品数量'],['InitialEnergyPercent','int','本地旧存档初次接入行动力'],['CraftSeconds','float','原作制作表现秒数']],rows=[[60017,1,1,0,3]]))
    tables={s['name']:s['rows'] for s in sheets}
    b=next(r for r in read('tbbuilding') if r['ID']==60017)
    feature=next(r for r in read('tbfeatureunlock') if r['ID']==b['Feature'])
    levels=[r for r in read('tbbuildinglevel') if r['ID']//1000==60017]
    response=read('tbglobal')[0]['ResponseEffect']
    tables['HomeBuilding'].append([60017,feature['ID']['value'],30501,30225,b['InitialModelPath'],b['JumpType'],read('tbglobal')[0]['HotelGuideConversation']])
    item_ids=set()
    for r in levels:
        e=r['Effect']
        tables['HomeBuildingLevel'].append([r['ID'],60017,r['Level'],r['BuildingPower'],r['ModelPath'],r['BuildingUpNeedTime'],e['PricePct'],e['CutCdSec'],e['PeopleNum']])
        for i,c in enumerate(r['BuildingUpCost'],1):
            tables['HomeBuildingCost'].append([r['ID']*10+i,r['ID'],c['Id'],c['Num']]); item_ids.add(c['Id'])
        for i,c in enumerate(r['BuildingUpCondition'],1):
            tables['HomeBuildingCondition'].append([r['ID']*10+i,2,r['ID'],c['Type']['value'],c['Value']])
        for i,g in enumerate(e['Goods']):
            index=len(response)-len(e['Goods'])+i
            tables['HomeBuildingGoods'].append([r['ID']*10+i+1,r['ID'],i,g['Cost'],g['Craft'],g['CraftPrice'],response[index] if index>=0 else -1])
            item_ids.update([g['Cost'],g['Craft']])
            if index < 0:
                unresolved.append(dict(goodsID=r['ID']*10+i+1,levelID=r['ID'],responseIndex=index,reason='Original response index outside recovered table'))
    for i,c in enumerate(feature['UnlockNeedCondition'],1):
        tables['HomeBuildingCondition'].append([feature['ID']['value']*10+i,1,feature['ID']['value'],c['Type']['value'],c['Value']])
    existing={r[0] for r in tables['HomeBuildingItem']}
    for item in read('tbitem'):
        id=item['Id']
        if id not in item_ids or id in existing: continue
        tables['HomeBuildingItem'].append([id,100000+id*2,100001+id*2,item['Icon'],'Assets/Data/Widgets/Common/Icons/'+item['Icon'].split('/')[-1]+'.png',item['UseEffect'].get('Time',0)])
        languages.extend([[100000+id*2,f'TEXT_BUILDING_ITEM_{id}',item['Name']],[100001+id*2,f'TEXT_BUILDING_ITEM_DESC_{id}',item['Desc']]])
    languages.extend([
        [30501,'TEXT_INN_NAME','旅店'],[30502,'TEXT_INN_LEVEL','{0}级 旅店'],
        [30503,'TEXT_INN_DETAIL','恢复行动力 {0}% · 用餐 {1}秒 · 英雄支付 {2}金币'],
        [30504,'TEXT_INN_REPAIR','修复旅店：{0}金币'],[30505,'TEXT_INN_LOCKED','完成并领取医院修复任务后开放旅店'],
        [30506,'TEXT_INN_NO_MATERIAL','原料不足，请到交易行购买鱼'],[30507,'TEXT_INN_REST','安排英雄用餐'],
        [30508,'TEXT_INN_RESTING','用餐中，剩余 {0:0.0}秒'],[30509,'TEXT_INN_ENERGY','英雄行动力 {0}%'],
        [30510,'TEXT_INN_REST_UNAVAILABLE','需要已招募英雄、食物和英雄金币，且行动力未满'],
        [30511,'TEXT_INN_NO_GOLD','金币不足，无法修复旅店'],[30512,'TEXT_INN_REPAIR_HINT','修复后可以制作食物，供英雄恢复行动力'],
        [30513,'TEXT_INN_CRAFTED','食物制作完成'],[30514,'TEXT_INN_REPAIRED','旅店修复完成']])
