"""Trace hospital configuration to recovered client tables without inventing server rules."""
from pathlib import Path
import hashlib
import json

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
TABLES = ROOT / 'research/config-study/tables'


def read(name):
    return json.loads((TABLES / (name + '.json')).read_text(encoding='utf-8'))


def collect():
    building = next(row for row in read('tbbuilding') if row['ID'] == 60018)
    levels = [row for row in read('tbbuildinglevel') if row['ID'] // 1000 == building['ID']]
    feature = next(row for row in read('tbfeatureunlock') if row['ID']['value'] == building['Feature']['value'])
    response = read('tbglobal')[0]['ResponseEffect']
    item_ids = {cost['Id'] for level in levels for cost in level['BuildingUpCost']}
    item_ids.update(good[field] for level in levels for good in level['Effect']['Goods']
                    for field in ('Craft', 'Cost'))
    items = [row for row in read('tbitem') if row['Id'] in item_ids]
    assert len(items) == len(item_ids)
    sheets = []

    def sheet(name, columns, rows):
        assert len({row[0] for row in rows}) == len(rows), name
        sheets.append(dict(name=name, columns=columns, rows=rows))

    sheet('HomeBuilding', [
        ['ID','int','原建筑ID'], ['FeatureID','int','原功能ID'], ['NameTextID','int','语言表ID'],
        ['NpcNameTextID','int','NPC语言表ID'], ['InitialModel','string','原始未修复模型路径'],
        ['EntryGuideID','int','原入口引导ID'], ['ConversationID','int','医院页面对话ID']
    ], [[60018, feature['ID']['value'], 30201, 30202, building['InitialModelPath'],
         building['JumpType'], read('tbglobal')[0]['HospitalGuideConversation']]])
    sheet('HomeBuildingLevel', [
        ['ID','int','原等级ID'], ['BuildingID','int','建筑ID'], ['Level','int','等级'],
        ['Power','int','战力'], ['Model','string','原始建成模型路径'], ['BuildSeconds','int','建造时间'],
        ['PricePercent','int','服务价格加成百分比'], ['CutSeconds','int','使用时间减少秒数'],
        ['Capacity','int','同时消费人数']
    ], [[row['ID'],60018,row['Level'],row['BuildingPower'],row['ModelPath'],row['BuildingUpNeedTime'],
         row['Effect']['PricePct'],row['Effect']['CutCdSec'],row['Effect']['PeopleNum']] for row in levels])
    sheet('HomeBuildingCost', [
        ['ID','int','等级ID乘10加费用序号'], ['LevelID','int','目标等级ID'],
        ['ItemID','int','消耗物品'], ['Amount','int','数量']
    ], [[row['ID']*10+i,row['ID'],cost['Id'],cost['Num']]
        for row in levels for i,cost in enumerate(row['BuildingUpCost'],1)])
    # OwnerKind: 1 = feature ID, 2 = target level ID. Preserve original condition types.
    conditions = [[feature['ID']['value']*10+i,1,feature['ID']['value'],c['Type']['value'],c['Value']]
                  for i,c in enumerate(feature['UnlockNeedCondition'],1)]
    conditions += [[row['ID']*10+i,2,row['ID'],c['Type']['value'],c['Value']]
                   for row in levels for i,c in enumerate(row['BuildingUpCondition'],1)]
    sheet('HomeBuildingCondition', [
        ['ID','int','数字条件行ID'], ['OwnerKind','int','1功能开放2目标等级'], ['OwnerID','int','所属配置ID'],
        ['Type','int','原UnLockType枚举值'], ['Value','int','条件参数']
    ], conditions)
    goods = []
    unresolved = []
    for row in levels:
        entries = row['Effect']['Goods']
        for i,good in enumerate(entries):
            index = len(response)-len(entries)+i
            valid = 0 <= index < len(response)
            key = row['ID']*10+i+1
            goods.append([key,row['ID'],i,good['Cost'],good['Craft'],good['CraftPrice'],
                          response[index] if valid else -1])
            if not valid:
                unresolved.append(dict(goodsID=key,levelID=row['ID'],responseIndex=index,
                                       reason='GetGoodUseValue index outside recovered ResponseEffect'))
    sheet('HomeBuildingGoods', [
        ['ID','int','等级ID乘10加商品序号'], ['LevelID','int','等级ID'], ['Order','int','原商品零基序号'],
        ['MaterialID','int','制作原料ID'], ['ProductID','int','商品ID'],
        ['ServiceBasePrice','int','英雄消费基础价格不是制作费用'],
        ['RecoveryPercent','int','按原算法推导负1表示证据不完整禁止执行']
    ], goods)
    sheet('HomeBuildingItem', [
        ['ID','int','原物品ID'], ['NameTextID','int','名称语言表ID'], ['DescriptionTextID','int','说明语言表ID'],
        ['IconSource','string','原FairyGUI资源标识不是Unity加载路径'], ['IconPath','string','工程共享图标路径'], ['UseSeconds','int','原基础使用时间']
    ], [[item['Id'],100000+item['Id']*2,100001+item['Id']*2,item['Icon'],
         'Assets/Data/Widgets/Common/Icons/'+item['Icon'].split('/')[-1]+'.png',item['UseEffect'].get('Time',0)] for item in items])
    sheet('HomeHospitalSetting', [['ID','int','医院ID'],['CraftSeconds','float','原作生产表现秒数'],
        ['FlightSeconds','float','原作物品飞行秒数']], [[60018,3,0.5]])
    languages = [[30201,'TEXT_HOSPITAL_NAME',building['Name']], [30202,'TEXT_HOSPITAL_NPC',building['NPCName']]]
    languages += [
        [30203,'TEXT_HOSPITAL_REPAIR','修复'], [30204,'TEXT_HOSPITAL_ENTER','进入'],
        [30205,'TEXT_HOSPITAL_LEVEL','Lv.{0} 医院'], [30206,'TEXT_HOSPITAL_BONUS','收益 +{0}%    耗时 −{1}秒    容量 {2}人'],
        [30207,'TEXT_HOSPITAL_DETAIL','恢复生命 {0}%    使用时间 {1}秒\n英雄消费价格 {2} 金币'],
        [30208,'TEXT_HOSPITAL_STOCK','库存 {0}'], [30209,'TEXT_HOSPITAL_MATERIAL','原料 {0} · 点击生产'],
        [30210,'TEXT_HOSPITAL_BACK','返回村庄'], [30211,'TEXT_HOSPITAL_IDLE','暂无英雄疗伤'],
        [30212,'TEXT_HOSPITAL_TREATING','加伦  生命 {0}%  治疗剩余 {1:0.0}秒'],
        [30213,'TEXT_HOSPITAL_TRAVELING','加伦正在前往医院'], [30214,'TEXT_HOSPITAL_FINISHED','加伦已恢复生命'],
        [30215,'TEXT_HOSPITAL_NO_MATERIAL','宁神花不足，请前往交易行购买'],
        [30216,'TEXT_HOSPITAL_NO_GOLD','金币不足，无法修复医院'],
        [30217,'TEXT_HOSPITAL_CRAFTING','生产中 {0:0.0}秒'],
        [30218,'TEXT_HOSPITAL_NEXT_LEVEL','下一等级需公会等级 {0}'],
        [30219,'TEXT_HOSPITAL_TASK','任务：{0}'], [30220,'TEXT_HOSPITAL_REVENUE','医院收入 {0}'],
        [30221,'TEXT_HOSPITAL_MATERIAL_TITLE','材料'], [30222,'TEXT_HOSPITAL_PRODUCT_TITLE','药剂'],
        [30223,'TEXT_HOSPITAL_GOLD','金币 {0}'],
        [30401,'TEXT_HOSPITAL_UPGRADE_HINT','提升等级可增加经营收益'],
        [30402,'TEXT_HOSPITAL_AUTO_CRAFT','自动生产'],
        [30403,'TEXT_HOSPITAL_EMPTY','没有商品了，请尽快制作'],
        [30404,'TEXT_HOSPITAL_CRAFT_HINT','点击材料即可开始生产'],
        [30405,'TEXT_HOSPITAL_LEVEL_LABEL','{0}级 医院'],
        [30406,'TEXT_HOSPITAL_RAW_MATERIAL','原材料'],
        [30407,'TEXT_HOSPITAL_FINISHED_PRODUCT','成品']
    ]
    for item in items:
        name = '金币' if item['Id']==70001 else item['Name']
        languages += [[100000+item['Id']*2,f'TEXT_BUILDING_ITEM_{item["Id"]}',name],
                      [100001+item['Id']*2,f'TEXT_BUILDING_ITEM_DESC_{item["Id"]}',item['Desc']]]
    for id, alias, building_id in [(30224, 'TEXT_FORGE_NPC', 60002),
                                   (30225, 'TEXT_HOTEL_NPC', 60017),
                                   (30226, 'TEXT_AUCTION_NPC', 60019)]:
        resident = next(row for row in read('tbbuilding') if row['ID'] == building_id)
        languages.append([id, alias, resident['NPCName']])
    files = [TABLES/(name+'.json') for name in
             ['tbbuilding','tbbuildinglevel','tbfeatureunlock','tbitem','tbglobal']]
    code = ROOT/'research/code-study/managed/decompiled/HotUpdate/Zombie'
    files += [code/(name+'.cs') for name in ['BuildingData','BuildingInteractiveModule','UIBuildingOpBubble',
              'UIBuildingPanel','UIConsumableShopView','ConsumableProxy','BuildingProxy','PlayerProxy',
              'TaskProxy','BaseBuildingController']]
    manifest = dict(schemaVersion=1,scope='Hospital 60018 original configuration; not runtime feature activation',
                    sheets=sheets,languages=languages,unresolved=unresolved,
                    sources=[dict(path=str(f.relative_to(ROOT)).replace('\\','/'),
                                  sha256=hashlib.sha256(f.read_bytes()).hexdigest()) for f in files],
                    sourceUI=dict(page='Consumable/ConsumableShopPage',repairBubble='Building/BuildingOpBubble',
                                  spine='e_ui_bg_priest_1/e_ui_bg_priest_1',
                                  craftPresentationSeconds=3,materialFlightSeconds=0.5),
                    offlineRules=dict(materialCount=1,outputCount=1,instantCommit=True,
                                      evidence='Existing DEMO adaptation, not recovered server behavior'))
    (OUT/'configuration-source.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(dict(tables={s['name']:len(s['rows']) for s in sheets},unresolved=len(unresolved))))


if __name__ == '__main__':
    collect()
