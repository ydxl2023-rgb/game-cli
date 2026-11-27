import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../..');
const require=createRequire(path.join(root,'client/Library/BlacksmithConfigAuthor/author.cjs'));
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
    for(let i=0;i<table.columns.length;i++) {
        const column=String.fromCharCode(65+i);
        sheet.getRange(`${column}:${column}`).format.columnWidth=table.columns[i][1]==='string'?90:20;
    }
}
await(await SpreadsheetFile.exportXlsx(book)).save(path.join(root,'config/propBlacksmith.xlsx'));
const languagesPath=path.join(root,'config/propCommonText.xlsx');
const languages=await SpreadsheetFile.importXlsx(await FileBlob.load(languagesPath));
const sheet=languages.worksheets.getItem('CommonText');
const rows=sheet.getRange('A4:C900').values;
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
    await(await book.render({sheetName:'BlacksmithRecipe',range:'A1:F8',scale:1.5})).arrayBuffer()));
console.log('Authored forge configuration and localized item texts.');

const systemPath=path.join(root,'config/propFunctionSystem.xlsx');
const systems=await SpreadsheetFile.importXlsx(await FileBlob.load(systemPath));
const systemSheet=systems.worksheets.items[0];
const systemRows=systemSheet.getRange('A1:H20').values;
for(const [id,type] of [[10008,'BlacksmithSystem']]) {
    let index=systemRows.findIndex((row,i)=>i>2 && row[1]===id);
    if(index<0)index=systemRows.findIndex((row,i)=>i>2 && !row[0]);
    const row=[id,id,type,0,1];
    systemRows[index]=row;
    systemSheet.getRange(`A${index+1}:E${index+1}`).values=[row];
}
await(await SpreadsheetFile.exportXlsx(systems)).save(systemPath);

