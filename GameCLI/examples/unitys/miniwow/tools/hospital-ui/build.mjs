import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../..');
const require=createRequire(path.join(root,'client/Library/TaskConfigAuthor/author.cjs'));
const {Workbook,SpreadsheetFile,FileBlob}=await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const manifest=JSON.parse(await fs.readFile(path.join(directory,'configuration-source.json'),'utf8'));
const book=Workbook.create();
for(const table of manifest.sheets){
    const sheet=book.worksheets.add(table.name);
    const last=String.fromCharCode(64+table.columns.length);
    sheet.getRange(`A1:${last}${table.rows.length+3}`).values=[
        table.columns.map(c=>c[2]),table.columns.map(c=>c[0]),table.columns.map(c=>c[1]),...table.rows
    ];
    sheet.getRange(`A1:${last}3`).format.fill='#DCE6F1';
    sheet.getRange(`A:${last}`).format.columnWidth=24;
}
await(await SpreadsheetFile.exportXlsx(book)).save(path.join(root,'config/propHomeBuilding.xlsx'));
const languagesPath=path.join(root,'config/propCommonText.xlsx');
const languages=await SpreadsheetFile.importXlsx(await FileBlob.load(languagesPath));
const sheet=languages.worksheets.getItem('CommonText');
const rows=sheet.getRange('A4:C600').values;
for(const [id,alias,text] of manifest.languages){
    let index=rows.findIndex(row=>row[1]===id);
    if(index>=0 && rows[index][0]!==alias) throw Error(`Language ID already owned: ${id}`);
    if(index<0) index=rows.findIndex(row=>!row[0]);
    if(index<0) throw Error('CommonText range exhausted');
    rows[index]=[alias,id,text];
    sheet.getRange(`A${index+4}:C${index+4}`).values=[rows[index]];
}
await(await SpreadsheetFile.exportXlsx(languages)).save(languagesPath);
await fs.writeFile(path.join(directory,'configuration-preview.png'),new Uint8Array(
    await(await book.render({sheetName:'HomeBuildingLevel',range:'A1:I8',scale:1.5})).arrayBuffer()));
console.log('Authored original hospital building configuration and localized item texts.');
