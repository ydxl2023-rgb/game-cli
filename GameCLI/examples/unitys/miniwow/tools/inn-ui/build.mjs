import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../..');
const require=createRequire(path.join(root,'client/Library/TaskConfigAuthor/author.cjs'));
const {Workbook,SpreadsheetFile,FileBlob}=await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const systemPath=path.join(root,'config/propFunctionSystem.xlsx');
const systems=await SpreadsheetFile.importXlsx(await FileBlob.load(systemPath));
const systemSheet=systems.worksheets.items[0];
const systemRows=systemSheet.getRange('A1:H20').values;
for(const [id,type] of [[10007,'InnSystem']]) {
    let index=systemRows.findIndex((row,i)=>i>2 && row[1]===id);
    if(index<0)index=systemRows.findIndex((row,i)=>i>2 && !row[0]);
    const row=[id,id,type,0,1];
    systemRows[index]=row;
    systemSheet.getRange(`A${index+1}:E${index+1}`).values=[row];
}
await(await SpreadsheetFile.exportXlsx(systems)).save(systemPath);
