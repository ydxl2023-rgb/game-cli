import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const require = createRequire(path.join(process.env.CODEX_WORKSPACE_NODE_MODULES, '../author.cjs'));
const {Workbook,SpreadsheetFile,FileBlob} = await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const source = JSON.parse(await fs.readFile(path.join(root,'tools/recruit/configuration-source.json'),'utf8'));
const book = Workbook.create();
for (const table of source.sheets) {
    const sheet = book.worksheets.add(table.name);
    const end = String.fromCharCode(64 + table.columns.length);
    sheet.getRange(`A1:${end}${table.rows.length+3}`).values = [table.columns.map(c=>c[0]),
        table.columns.map(c=>c[0]),table.columns.map(c=>c[1]),...table.rows];
    sheet.getRange(`A1:${end}3`).format.fill = '#DCE6F1';
    sheet.getRange(`A:${end}`).format.columnWidth = 20;
}
await (await SpreadsheetFile.exportXlsx(book)).save(path.join(root,'config/propRecruit.xlsx'));
const textPath = path.join(root,'config/propCommonText.xlsx');
const textBook = await SpreadsheetFile.importXlsx(await FileBlob.load(textPath));
const textSheet = textBook.worksheets.getItem('CommonText');
const textRows = textSheet.getRange('A1:C1600').values;
for (const [id,alias,content] of source.languages) {
    let index = textRows.findIndex((row,i)=>i>2 && row[1]===id);
    if (index<0) index=textRows.findIndex((row,i)=>i>2 && !row[0]);
    textRows[index]=[alias,id,content];
    textSheet.getRange(`A${index+1}:C${index+1}`).values=[textRows[index]];
}
await (await SpreadsheetFile.exportXlsx(textBook)).save(textPath);
const systemsPath = path.join(root,'config/propFunctionSystem.xlsx');
const systems = await SpreadsheetFile.importXlsx(await FileBlob.load(systemsPath));
const sheet = systems.worksheets.items[0];
const rows = sheet.getRange('A1:E30').values;
for (const [id,type] of [[10010,'RecruitSystem'],[10011,'BattleEntrySystem']]) {
    let index=rows.findIndex((row,i)=>i>2 && row[1]===id);
    if(index<0) index=rows.findIndex((row,i)=>i>2 && !row[0]);
    rows[index]=[id,id,type,0,1];
    sheet.getRange(`A${index+1}:E${index+1}`).values=[rows[index]];
}
await (await SpreadsheetFile.exportXlsx(systems)).save(systemsPath);
console.log('Recruitment, dialogue, hero and battle entry source tables authored.');
