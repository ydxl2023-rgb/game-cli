import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath, pathToFileURL} from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const dependencyRoot = process.env.CODEX_WORKSPACE_NODE_MODULES;
if (!dependencyRoot) throw new Error('Set CODEX_WORKSPACE_NODE_MODULES to the bundled workspace dependencies.');
const require = createRequire(path.join(dependencyRoot, '../author.cjs'));
const {Workbook, SpreadsheetFile, FileBlob} = await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const scene = JSON.parse(await fs.readFile(path.join(root, 'tools/main-scene/scene.json'), 'utf8'));
const point = scene.nodes.find(node => node.name === 'ExplorePoint').position;
const events = JSON.parse(await fs.readFile(path.join(root, 'research/config-study/tables/tbadventureevent.json'), 'utf8'));
const globals = JSON.parse(await fs.readFile(path.join(root, 'research/config-study/tables/tbglobal.json'), 'utf8'));
const global = Array.isArray(globals) ? globals[0] : globals;
const event = events.find(row => row.ID === 2);
const book = Workbook.create();
const sheet = book.worksheets.add('ExploreJourney');
const columns = [['ID','int'],['HeroID','int'],['ActorID','int'],['RequiredTaskID','int'],
    ['EventID','int'],['AreaID','int'],['EventType','int'],['ExecuteSeconds','float'],
    ['TravelSeconds','float'],['ExitX','float'],['ExitY','float'],['ExitZ','float']];
sheet.getRange('A1:L4').values = [columns.map(c => c[0]), columns.map(c => c[0]), columns.map(c => c[1]),
    [1,125,1,60104,event.ID,event.AreaId,event.EventType.value,
        global.AdventureEventExecuteTime[event.EventType.name] / 1000,8,point.x,point.y,point.z]];
sheet.getRange('A1:L3').format.fill = '#DCE6F1';
sheet.getRange('A:L').format.columnWidth = 20;
await (await SpreadsheetFile.exportXlsx(book)).save(path.join(root, 'config/propExplore.xlsx'));
const systemsPath = path.join(root, 'config/propFunctionSystem.xlsx');
const systems = await SpreadsheetFile.importXlsx(await FileBlob.load(systemsPath));
const systemsSheet = systems.worksheets.items[0];
const rows = systemsSheet.getRange('A1:E30').values;
let index = rows.findIndex((row, i) => i > 2 && row[1] === 10009);
if (index < 0) index = rows.findIndex((row, i) => i > 2 && !row[0]);
systemsSheet.getRange(`A${index+1}:E${index+1}`).values = [[10009,10009,'ExploreSystem',0,1]];
await (await SpreadsheetFile.exportXlsx(systems)).save(systemsPath);
console.log('Authored ExploreJourney and registered ExploreSystem 10009.');
