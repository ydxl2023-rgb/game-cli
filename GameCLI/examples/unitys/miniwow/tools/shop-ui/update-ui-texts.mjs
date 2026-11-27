import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';

const directory=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(directory,'../..');
const require=createRequire(path.join(root,'client/Library/TaskConfigAuthor/author.cjs'));
const {Workbook,SpreadsheetFile,FileBlob}=await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const manifest=JSON.parse(await fs.readFile(path.join(directory,'configuration-source.json'),'utf8'));
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
console.log('Updated original trading UI labels.');
