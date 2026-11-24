import {existsSync} from 'node:fs';
if(existsSync(new URL('../../config/Home.xlsx',import.meta.url)))throw Error('Home.xlsx already exists; edit with UnityCLI excel.');
import {Workbook,SpreadsheetFile} from '@oai/artifact-tool';
const wb=Workbook.create();
const sheets={
 Entries:[['id','group','labelId','icon','visible','unlocked','route'],
 [101,'Activity','home.shop','hub_button_18',true,true,'Shop'],[102,'Activity','home.sign','hub_button_29',true,true,'SevenDayLogin'],[103,'Activity','home.sprint','hub_button_52',true,true,'OpeningSprint'],[104,'Activity','home.first','',false,true,'FirstPurchase'],[105,'Activity','home.trial','hub_button_31',true,true,'GrowthTrial'],
 [201,'Function','home.market','hub_button_12',true,true,'Market'],[202,'Function','home.tasks','hub_button_20',true,true,'Task'],[203,'Function','home.bag','hub_button_21',true,true,'Backpack'],[204,'Function','home.mail','hub_button_16',true,true,'Mail'],[205,'Function','home.rank','hub_button_11',false,true,'Rank'],
 [301,'Quick','home.community','hub_icon_18',true,true,'Community'],[302,'Quick','home.notice','hub_icon_19',true,true,'Notice'],[303,'Quick','home.survey','hub_icon_9',true,true,'Survey'],
 [401,'Navigation','home.explore','hub_button_22',true,true,'MapPreview'],[402,'Navigation','home.heroes','hub_button_24',true,true,'Heroes'],[403,'Navigation','home.mansion','hub_button_25',true,false,'Mansion'],[404,'Navigation','home.playmode','hub_button_26',true,false,'PlayMode'],[405,'Navigation','home.guild','hub_button_27',true,false,'Guild'],[406,'Navigation','home.world','hub_button_32_1',true,false,'World'],
 [501,'Recruit','home.recruit','hub_icon_13',true,true,'Recruit']],
 Profile:[['id','playerName','guildTitle','power','gold','gem','energy','energyLimit','taskProgress','chat'],[1,'营地见习生','初级村庄 II',37000,19000,4262,48,20,'装备蓝色品质的人数（1/3）','欢迎回来，领主大人！']],
 propCommonText:[['id','text'],['home.shop','商城'],['home.sign','七日签到'],['home.sprint','开服冲刺'],['home.first','首充'],['home.trial','成长试炼'],['home.market','集市'],['home.tasks','任务'],['home.bag','背包'],['home.mail','邮件'],['home.rank','排行'],['home.community','社区'],['home.notice','公告'],['home.survey','问卷'],['home.explore','探索'],['home.heroes','英雄'],['home.mansion','府邸'],['home.playmode','试炼'],['home.guild','公会'],['home.world','领地'],['home.recruit','招募'],['home.close','关闭'],['home.locked','该入口尚未解锁'],['home.pending','该系统尚未接入'],['home.settings','设置'],['home.chat','村庄'],['home.return','返回营地'],['home.map','冒险地图'],['home.maphint','地图预览 · 战斗关卡尚未开放'],['home.demo','本地演示'],['home.invalid','入口配置无效']],
 Evidence:[['topic','basis','limitation'],['Entries','tbmainfunctions / tbmainactivity / UIMainTabsCom','Local visibility and unlock states; not original account or server state'],['Profile','User screenshot; presentation example','Amounts and progression are local demonstration values'],['FirstPurchase','Original uses e_ui_shouchong_icon_1','Hidden until effect is recovered; no substituted art'],['RedDot','Original uses business module data','No invented notifications; ownership reserved for RedDotSystem']]
};
for(const [name,rows] of Object.entries(sheets)){const s=wb.worksheets.add(name);s.getRangeByIndexes(0,0,rows.length,rows[0].length).values=rows;}
await (await SpreadsheetFile.exportXlsx(wb)).save(new URL('../../config/Home.xlsx',import.meta.url).pathname.replace(/^\/([A-Za-z]:)/,'$1'));
console.log('Home.xlsx created');
