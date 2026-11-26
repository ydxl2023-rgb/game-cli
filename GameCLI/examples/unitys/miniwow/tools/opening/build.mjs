import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath, pathToFileURL} from 'node:url';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const require = createRequire(path.join(root, 'client/Library/TaskConfigAuthor/author.cjs'));
const {Workbook, SpreadsheetFile, FileBlob} = await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const workbook = Workbook.create();
const columns = [['ID','int'],['TaskID','int'],['NextTaskID','int'],['WaterItemID','int'],['WaterAmount','int'],['WaterX','float'],['WaterY','float'],['WaterZ','float'],['WaterSizeX','float'],['WaterSizeY','float'],['WaterSizeZ','float'],['InteractionRadius','float'],['IntroDialogueID','int'],['VideoPath','string']];
const sheet = workbook.worksheets.add('HomeOpening');
sheet.getRange('A1:N4').values = [columns.map(c=>c[0]),columns.map(c=>c[0]),columns.map(c=>c[1]),[1,60001,60002,83011,10,-17.23,35.3,14.68,7,1,6,1.2,3042,'Assets/Data/Widgets/Home/Opening/startVideo.mp4']];
sheet.getRange('A1:N3').format.fill='#DCE6F1';
sheet.getRange('A:N').format.columnWidth=20;
const dialogue = workbook.worksheets.add('OpeningDialogue');
dialogue.getRange('A1:C6').values = [
 ['ID','SpeakerTextID','ContentTextID'], ['ID','SpeakerTextID','ContentTextID'],
 ['int','int','int'], [3042,30014,30016], [3043,30015,30017], [3053,30019,30020]
];
dialogue.getRange('A1:C3').format.fill='#DCE6F1';
const rescue = workbook.worksheets.add('OpeningRescue');
const rescueColumns = [['ID','int'],['TaskID','int'],['BuildingID','int'],['WaterCost','int'],['TargetX','float'],['TargetY','float'],['TargetZ','float'],['WaterTargetX','float'],['WaterTargetY','float'],['WaterTargetZ','float'],['DialogueID','int'],['ObjectiveTextID','int'],['DurationSeconds','float'],['ActionTextID','int'],['AutoClaim','int']];
rescue.getRange('A1:O8').values = [rescueColumns.map(c=>c[0]),rescueColumns.map(c=>c[0]),rescueColumns.map(c=>c[1]),
 [1,60001,60019,10,-9.38,35.3,9.21,-20.41,35.3,14.46,3043,30006,1.5,30011,0],
 [2,60002,60017,5,-20.12,35.71,-3.64,-14.34,35.3,12.65,3053,30021,1.5,30011,0],
 [3,60002,60018,5,-26.65,35.71,0.01,-14.34,35.3,12.65,0,30022,1.5,30011,0],
 [4,60003,60018,0,-26.65,35.71,0.01,-14.34,35.3,12.65,0,30026,3,30024,0],
 [5,60003,60018,0,-26.65,35.71,0.01,-14.34,35.3,12.65,0,30027,1,30025,1]];
rescue.getRange('A1:O3').format.fill='#DCE6F1';
rescue.getRange('A:O').format.columnWidth=22;
await (await SpreadsheetFile.exportXlsx(workbook)).save(path.join(root,'config/propOpening.xlsx'));
const preview = await workbook.render({sheetName:'OpeningRescue',range:'A1:O8',scale:1.5});
await fs.writeFile(path.join(root,'tools/opening/third-config.png'), new Uint8Array(await preview.arrayBuffer()));
const textFile=path.join(root,'config/propCommonText.xlsx');
const texts=await SpreadsheetFile.importXlsx(await FileBlob.load(textFile));
const textSheet=texts.worksheets.getItem('CommonText');
const rows=textSheet.getRange('A4:C100').values;
const additions=[
 [30001,'TEXT_OPENING_SKIP','跳过剧情'],[30002,'TEXT_OPENING_SKIP_CONFIRM','是否确认跳过剧情？'],
 [30003,'TEXT_OPENING_CONTINUE','继续'],[30004,'TEXT_OPENING_CANCEL','继续观看'],
 [30005,'TEXT_OPENING_WATER','前往水井取水，救出被困村民'],
 [30006,'TEXT_OPENING_FIRE','前往交易行，使用 {0} 份水救火'],
 [30007,'TEXT_OPENING_EXTINGUISH','正在救火…'],[30008,'TEXT_OPENING_CLAIM','任务完成 · 点击领取'],
 [30009,'TEXT_OPENING_NEXT','任务 {0} · {1}'],[30010,'TEXT_OPENING_WATER_COUNT','救火的水 {0}/{1}'],
 [30011,'TEXT_OPENING_RESCUE','救火'],[30012,'TEXT_OPENING_VIDEO_ERROR','剧情视频加载失败，请重试或跳过'],
 [30013,'TEXT_OPENING_RETRY','重试'],[30014,'TEXT_OPENING_PRIEST','牧师'],[30015,'TEXT_OPENING_MERCHANT','商人'],
 [30016,'TEXT_OPENING_INTRO','火还没灭！村子到处都有人被困。先去水井打水救他们出来！'],
 [30017,'TEXT_OPENING_THANKS','货架还在，账册也没烧完。等我好好整理一下，英雄们带回来的东西就有地方卖了。'],
 [30018,'TEXT_OPENING_TASK','任务 {0} · {1} ({2}/{3})'],
 [30019,'TEXT_OPENING_HOTEL_OWNER','旅店老板'],
 [30020,'TEXT_OPENING_HOTEL_THANKS','十分感谢你，这边剩下的就交给我吧，修整一下旅店就能恢复给大家做饭了。\n你先去隔壁看看，那边也需要帮忙。'],
 [30021,'TEXT_OPENING_HOTEL_FIRE','前往旅店，使用 {0} 份水救火'],
 [30022,'TEXT_OPENING_HOSPITAL_FIRE','前往医院，使用 {0} 份水救火'],
 [30023,'TEXT_OPENING_FETCH','取水'],
 [30024,'TEXT_OPENING_CLEAR','清理'],[30025,'TEXT_OPENING_SAVE_PERSON','救人'],
 [30026,'TEXT_OPENING_CLEAR_HINT','点击清理倒塌的横梁'],
 [30027,'TEXT_OPENING_SAVE_HINT','点击救出困住的人'],
 [30028,'TEXT_OPENING_COUNTDOWN','剩余 {0:0.0} 秒'],
 [30029,'TEXT_OPENING_AUTOMATIC_CLAIM','救援完成，正在领取任务']
];
for(const [id,alias,content] of additions){
 let i=rows.findIndex(r=>r[1]===id);
 if(i<0)i=rows.findIndex(r=>!r[0]);
 rows[i]=[alias,id,content];
 textSheet.getRange(`A${i+4}:C${i+4}`).values=[rows[i]];
}
await (await SpreadsheetFile.exportXlsx(texts)).save(textFile);
await fs.writeFile(path.join(root,'tools/opening/config-evidence.json'),JSON.stringify({source:'GuideStoryController.Story1/Story2/Story3; Building_sdc guide and collider anchors; tbconversation 3042/3043/3053',tasks:[60001,60002,60003],next:60004,water:10,adaptations:['InteractionRadius=1.2 is local arrival tolerance','Native fire presentation reuses the recovered orange sprite sheet'],columns},null,2));
console.log('Authored HomeOpening and opening language entries.');
