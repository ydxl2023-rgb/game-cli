import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const require=createRequire(path.join(root,'client/Library/TaskConfigAuthor/author.cjs'));
const {Workbook,SpreadsheetFile,FileBlob}=await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const book=Workbook.create();
function sheet(name,columns,rows){
 const s=book.worksheets.add(name);
 const end=columns.length<=26?String.fromCharCode(64+columns.length):'A'+String.fromCharCode(64+columns.length-26);
 s.getRange(`A1:${end}${rows.length+3}`).values=[columns.map(c=>c[0]),columns.map(c=>c[0]),columns.map(c=>c[1]),...rows];
 s.getRange(`A1:${end}3`).format.fill='#DCE6F1';
 s.getRange(`A:${end}`).format.columnWidth=20;
}
// Source: tbhero 125; tbbuildinglevel 60018001; tbshop 6001; tbitem 77101;
// tbglobal ResponseEffect/GetGoodUseValue. HP percent and craft ratio are explicit offline adaptations.
sheet('VillageSetting',[
 ['ID','int'],['HeroID','int'],['ActorID','int'],['BuildingID','int'],['GoldID','int'],
 ['MaterialID','int'],['PurchasePrice','int'],
 ['MaterialCost','int'],['OutputCount','int'],
 ['InitialHealthPercent','int'],['HeroGold','int'],
 ['HospitalX','float'],['HospitalY','float'],['HospitalZ','float']
],[[1,125,1,60018,70001,77001,30,1,1,20,500000,-26.63,35.2,-2.43]]);
sheet('VillageTask',[
 ['ID','int'],['Action','int'],['RequiredTaskID','int'],['TitleTextID','int'],
 ['DetailTextID','int'],['ActionTextID','int']
],[
 [60004,1,60003,30101,30102,30103],
 [60101,2,60004,30104,30105,30106],
 [60102,3,60101,30107,30108,30109],
 [60103,4,60102,30110,30111,30112],
 [60104,5,60103,30113,30114,30115]
]);
await(await SpreadsheetFile.exportXlsx(book)).save(path.join(root,'config/propVillage.xlsx'));
const texts=await SpreadsheetFile.importXlsx(await FileBlob.load(path.join(root,'config/propCommonText.xlsx')));
const s=texts.worksheets.getItem('CommonText');
const rows=s.getRange('A4:C180').values;
const additions=[
 [30101,'TEXT_VILLAGE_RECRUIT_TITLE','石盾加伦（战士）'],
 [30102,'TEXT_VILLAGE_RECRUIT_DETAIL','咳……我还活着。这条命是你从火里抢回来的。给我一把剑，我会为斯坦恩村而战。'],
 [30103,'TEXT_VILLAGE_RECRUIT','招募加伦'],
 [30104,'TEXT_VILLAGE_REPAIR_TITLE','修复医院'],
 [30105,'TEXT_VILLAGE_REPAIR_DETAIL','火是灭了……可病床、药柜和治疗台都毁了。如果不马上修好医院，这些伤员撑不过今晚....\n修复费用：{0} 金币'],
 [30106,'TEXT_VILLAGE_REPAIR','修复医院'],
 [30107,'TEXT_VILLAGE_BUY_TITLE','交易行 · 宁神花'],
 [30108,'TEXT_VILLAGE_BUY_DETAIL','购买 1 份宁神花：{0} 金币\n用于在医院生产微效药剂。'],
 [30109,'TEXT_VILLAGE_BUY','购买宁神花'],
 [30110,'TEXT_VILLAGE_CRAFT_TITLE','医院 · 药剂生产'],
 [30111,'TEXT_VILLAGE_CRAFT_DETAIL','消耗 {0} 份宁神花，生产 {1} 份微效药剂。'],
 [30112,'TEXT_VILLAGE_CRAFT','生产微效药剂'],
 [30113,'TEXT_VILLAGE_HEAL_TITLE','加伦疗伤'],
 [30114,'TEXT_VILLAGE_HEAL_DETAIL','加伦正在前往医院。抵达后使用 1 份微效药剂疗伤。'],
 [30115,'TEXT_VILLAGE_HEAL','前往医院'],
 [30116,'TEXT_VILLAGE_STOCK','金币 {0}　宁神花 {1}　微效药剂 {2}'],
 [30117,'TEXT_VILLAGE_RECEIVED','已招募石盾加伦\n加伦已加入村庄，接下来修复医院为他疗伤。'],
 [30118,'TEXT_VILLAGE_HEALTH','加伦生命 {0}%　治疗剩余 {1:0.0} 秒'],
 [30119,'TEXT_VILLAGE_REJECTED','条件不足，请检查金币、材料和前置任务。'],
 [30120,'TEXT_VILLAGE_FINISHED','加伦已完成治疗，任务完成后点击领取。'],
 [30121,'TEXT_VILLAGE_TASK_DONE','操作完成 · 点击领取任务奖励'],
 [30122,'TEXT_VILLAGE_CLOSE','确认获得'],
 [30123,'TEXT_VILLAGE_NEXT','后续任务：{0}'],
 [30124,'TEXT_VILLAGE_CLAIM','领取奖励'],
 [30125,'TEXT_VILLAGE_HEALING','正在疗伤'],
 [30126,'TEXT_VILLAGE_TRAVELING','前往医院中']
];
for(const[id,alias,text] of additions){let i=rows.findIndex(r=>r[1]===id);if(i<0)i=rows.findIndex(r=>!r[0]);rows[i]=[alias,id,text];s.getRange(`A${i+4}:C${i+4}`).values=[rows[i]];}
await(await SpreadsheetFile.exportXlsx(texts)).save(path.join(root,'config/propCommonText.xlsx'));
await fs.writeFile(path.join(root,'tools/opening/village-config.png'),new Uint8Array(await(await book.render({sheetName:'VillageTask',range:'A1:F8',scale:1.5})).arrayBuffer()));
console.log('Authored five village tasks and explicit offline economy configuration.');

