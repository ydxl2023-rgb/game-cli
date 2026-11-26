import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
import {fileURLToPath, pathToFileURL} from 'node:url';
import {createHash} from 'node:crypto';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const temporary = path.join(root, 'client/Library/TaskConfigAuthor');
const require = createRequire(path.join(temporary, 'author.cjs'));
const {Workbook, SpreadsheetFile, FileBlob} = await import(pathToFileURL(require.resolve('@oai/artifact-tool')));

if (process.argv.includes('--install-task-system')) {
    const functionPath = path.join(root, 'config/propFunctionSystem.xlsx');
    const functions = await SpreadsheetFile.importXlsx(await FileBlob.load(functionPath));
    const functionSheet = functions.worksheets.getItem('FunctionSystem');
    const rows = functionSheet.getRange('A1:E30').values;
    let row = rows.findIndex(values => values[2] === 'TaskSystem');
    if (row < 0) row = rows.findIndex((values, index) => index >= 3 && !values[0]);
    if (row < 3) throw new Error('No function row available');
    functionSheet.getRange(`A${row + 1}:E${row + 1}`).values = [[10004, 10004, 'TaskSystem', 0, 1]];
    await (await SpreadsheetFile.exportXlsx(functions)).save(functionPath);
    const file = path.join(root, 'config/propTask.xlsx');
    const current = await SpreadsheetFile.importXlsx(await FileBlob.load(file));
    let setting;
    try { setting = current.worksheets.getItem('TaskSetting'); } catch {}
    if (!setting) {
        setting = current.worksheets.add('TaskSetting');
        setting.getRange('A1:B4').values = [
            ['ID', 'InitialChapterID'], ['ID', 'InitialChapterID'], ['int', 'int'], [1, 1]
        ];
        setting.getRange('A:B').format.columnWidth = 25;
        setting.getRange('A1:B3').format.fill = '#DCE6F1';
    }
    await (await SpreadsheetFile.exportXlsx(current)).save(file);
    const manifestPath = path.join(root, 'tools/task-config/manifest.json');
    const manifest = JSON.parse(await fs.readFile(manifestPath, 'utf8'));
    if (!manifest.sheets.some(sheet => sheet.name === 'TaskSetting'))
        manifest.sheets.push({name:'TaskSetting', columns:[['ID','int'],['InitialChapterID','int']],rows:1});
    await fs.writeFile(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    console.log('Installed TaskSystem configuration and explicit initial chapter setting.');
    process.exit(0);
}

const navigationTargets = {};
const commandName = name => ({Navigation: 'Navigate', Conversation: 'ShowDialogue'}[name] ?? name);
function callExpression(name, args) {
    if (name === 'Navigation') {
        const match = /^([^/]+)_(\d+)\/Collider$/.exec(args[0]);
        if (!match) throw new Error('Unsupported navigation target: ' + args[0]);
        const id = Number(match[2]);
        if (navigationTargets[id] && navigationTargets[id] !== args[0]) throw new Error('Ambiguous building ID');
        navigationTargets[id] = args[0];
        args = [id];
    }
    return commandName(name) + '(' + args.map(value => JSON.stringify(value)).join(', ') + ')';
}
if (['--numeric-ids', '--numeric-chapter-ids', '--numeric-task-ids'].some(flag => process.argv.includes(flag))) {
    const all = process.argv.includes('--numeric-ids');
    const names = all ? ['Task', 'TaskChapter', 'TaskUnlock', 'TaskGuide']
        : [process.argv.includes('--numeric-task-ids') ? 'Task' : 'TaskChapter'];
    const prefixes = {Task:'TASK_', TaskChapter:'TASK_CHAPTER_', TaskUnlock:'TASK_UNLOCK_', TaskGuide:'TASK_GUIDE_'};
    const file = path.join(root, 'config/propTask.xlsx');
    const manifestPath = path.join(root, 'tools/task-config/manifest.json');
    const manifest = JSON.parse(await fs.readFile(manifestPath, 'utf8'));
    const current = await SpreadsheetFile.importXlsx(await FileBlob.load(file));
    for (const sheetName of names) {
        const definition = manifest.sheets.find(sheet => sheet.name === sheetName);
        const sheet = current.worksheets.getItem(sheetName);
        const values = sheet.getRange(`A4:B${definition.rows + 3}`).values;
        const ids = values.map(([alias, id]) => {
            if (!Number.isInteger(id) || (typeof alias === 'string' ? alias !== prefixes[sheetName] + id : alias !== id))
                throw new Error('Unexpected ID in ' + sheetName + ': ' + alias);
            return [id];
        });
        sheet.getRange(`A4:A${definition.rows + 3}`).values = ids;
        console.log('Numeric IDs: ' + sheetName + ', ' + ids.length + ' rows.');
    }
    if (all) manifest.idPolicy = 'All task sheets use numeric IDs; string IDs require explicit authorization except language tables.';
    await (await SpreadsheetFile.exportXlsx(current)).save(file);
    await fs.writeFile(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    for (const sheetName of names) {
        const preview = await current.render({sheetName,range:'A1:C8',scale:1,format:'png'});
        await fs.writeFile(path.join(temporary, sheetName + '-ids.png'), new Uint8Array(await preview.arrayBuffer()));
    }
    process.exit(0);
}
if (process.argv.includes('--function-calls')) {
    const file = path.join(root, 'config/propTask.xlsx');
    const manifestPath = path.join(root, 'tools/task-config/manifest.json');
    const manifest = JSON.parse(await fs.readFile(manifestPath, 'utf8'));
    Object.assign(navigationTargets, manifest.navigationTargets ?? {});
    const definition = manifest.sheets.find(sheet => sheet.name === 'TaskGuide');
    const current = await SpreadsheetFile.importXlsx(await FileBlob.load(file));
    const sheet = current.worksheets.getItem('TaskGuide');
    const values = sheet.getRange(`A1:J${definition.rows + 3}`).values;
    if (values[1][6] !== 'ConditionExpression' || values[1][7] !== 'ActionExpression')
        throw new Error('Unexpected TaskGuide headers');
    for (const row of values.slice(3)) {
        if (!row[7].startsWith('{')) continue;
        if (!row[7].endsWith('}')) throw new Error('Invalid command tuple: ' + row[7]);
        const [name, ...args] = JSON.parse('[' + row[7].slice(1, -1) + ']');
        if (typeof name !== 'string' || !/^[A-Za-z_]\w*$/.test(name))
            throw new Error('Invalid command name: ' + name);
        row[7] = callExpression(name, args);
    }
    sheet.getRange('A1').write(values);
    manifest.signatures = Object.fromEntries(Object.entries(manifest.signatures)
        .map(([name, args]) => [commandName(name), name === 'Navigation' ? ['Number'] : args]));
    manifest.actionFormat = 'Command(argument, ...)';
    manifest.navigationTargets = navigationTargets;
    await (await SpreadsheetFile.exportXlsx(current)).save(file);
    await fs.writeFile(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    const preview = await current.render({sheetName:'TaskGuide',range:'G1:J8',scale:1,format:'png'});
    await fs.writeFile(path.join(temporary, 'task-preview.png'), new Uint8Array(await preview.arrayBuffer()));
    console.log('Migrated ' + definition.rows + ' actions; preserved conditions and other sheets.');
    process.exit(0);
}

if (process.argv.includes('--add-conditions')) {
    const file = path.join(root, 'config/propTask.xlsx');
    const manifestPath = path.join(root, 'tools/task-config/manifest.json');
    const manifest = JSON.parse(await fs.readFile(manifestPath, 'utf8'));
    const definition = manifest.sheets.find(sheet => sheet.name === 'TaskGuide');
    const current = await SpreadsheetFile.importXlsx(await FileBlob.load(file));
    const sheet = current.worksheets.getItem('TaskGuide');
    if (!definition.columns.some(column => column[0] === 'ConditionExpression')) {
        const values = sheet.getRange(`A1:I${definition.rows + 3}`).values;
        if (values[1][6] !== 'ActionExpression') throw new Error('Unexpected TaskGuide header; migration stopped');
        values.forEach((row, index) => row.splice(6, 0,
            index < 2 ? 'ConditionExpression' : index === 2 ? 'string' : 'true'));
        sheet.getRange('A1').write(values);
        definition.columns.splice(6, 0, ['ConditionExpression', 'string']);
    }
    sheet.getRange('G:G').format.columnWidth = 38;
    sheet.getRange('H:H').format.columnWidth = 95;
    sheet.getRange('A1:J3').format.fill = '#DCE6F1';
    await (await SpreadsheetFile.exportXlsx(current)).save(file);
    manifest.conditionPolicy = 'Explicit boolean Evaluate expression; false skips this action attempt, not failure.';
    await fs.writeFile(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
    const preview = await current.render({sheetName:'TaskGuide',range:'G1:J8',scale:1,format:'png'});
    await fs.writeFile(path.join(temporary, 'task-preview.png'), new Uint8Array(await preview.arrayBuffer()));
    console.log('Migrated TaskGuide in place; existing rows and other sheets retained.');
    process.exit(0);
}
const workbook = Workbook.create();
const sources = {};
const sourceNames = ['tbtask', 'tbchaptertask', 'tbtaskunlock', 'tbguidetask'];
const hashes = {};
for (const name of sourceNames) {
    const raw = await fs.readFile(path.join(root, 'research/config-study/tables', `${name}.json`));
    sources[name] = JSON.parse(raw);
    hashes[name] = createHash('sha256').update(raw).digest('hex');
}
const list = values => `{${values.join(',')}}`;
const pairs = values => values.map(value => `{${value.Id},${value.Num}}`).join(',');
const fields = (names, types) => names.split(' ').map((name, i) => [name, types[i]]);
const sheets = [];
function add(name, columns, rows) {
    const sheet = workbook.worksheets.add(name);
    const header = [columns.map(c => c[0]), columns.map(c => c[0]), columns.map(c => c[1])];
    const values = [...header, ...rows];
    sheet.getRange('A1').write(values);
    sheet.getRange(`A1:AF${values.length}`).format.rowHeight = 24;
    sheet.getRange('A1:AF3').format.fill = '#DCE6F1';
    sheet.getRange('A:AF').format.columnWidth = 22;
    sheet.getRange('A:A').format.columnWidth = 34;
    sheets.push({name, columns, rows: rows.length});
    return sheet;
}

const taskColumns = fields(
    'ID NumID Name Description Icon TaskConditionType SourceConditionType TaskConditionShape Param1 Param2 Param3 ConditionItems FinishValue ShowTips GuideID Rewards QuestValue TaskBuildingType SourceBuildingType NPCTalkID Priority TipsDelayTime GuideConversationDelayTime GuideConversationID',
    ['int','int','string','string','string','enum','int','enum','int','int','int','List<int:Id,long:Num>',
     'int','bool','int','List<int:Id,long:Num>','int','enum','int','int','int','int','int','int']);
const taskSheet = add('Task', taskColumns, sources.tbtask.map(row => {
    const condition = row.FinishConditionValues;
    return [row.TaskID, row.TaskID, row.TaskName, row.TaskDescription, row.Icon,
        row.FinishConditionType.name, row.FinishConditionType.value, condition.$type.split('.').at(-1),
        condition.Param1 ?? 0, condition.Param2 ?? 0, condition.Param3 ?? 0, pairs(condition.List ?? []),
        row.FinishValue, row.ShowTips, row.JumpType, pairs(row.Reward), row.QuestValue,
        row.BuildingType.name, row.BuildingType.value, row.NPCTalkID, row.Priority,
        row.TipsDelayTime, row.GuideConversationDelayTime, row.GuideConversationID];
}));
taskSheet.getRange('D:D').format.columnWidth = 52;
taskSheet.getRange('E:E').format.columnWidth = 42;
add('TaskChapter', fields('ID NumID TaskIDs', ['int','int','List<int>']),
    sources.tbchaptertask.map(row => [row.ID, row.ID, list(row.TaskId)]));
add('TaskUnlock', fields('ID NumID TaskID TaskCategory SourceTaskType Conditions TriggerRewards',
    ['int','int','int','enum','int','List<int:Type,int:Value>','List<int:Id,long:Num>']),
    sources.tbtaskunlock.map(row => [row.ID, row.ID, row.TaskId, row.TaskType.name,
        row.TaskType.value, row.UnlockCondition.map(c => `{${c.Type.value},${c.Value}}`).join(','), pairs(row.TriggerReward)]));
const signatures = {
    Navigation: ['String'], CloseUI: ['String'], OpenUI: ['String'], Wait: ['Number'],
    GuideFinger: ['String'], ShowMain: [], Tips: ['Number'], SelectExploreArea: ['Number'], Conversation: ['Number']
};
const tipId = 20001;
const tipAlias = 'TEXT_TASK_GUIDE_ASSIST';
const tips = sources.tbguidetask.filter(row => row.Action === 'Tips');
if (tips.length !== 1 || tips[0].ActionParam.length !== 1) throw new Error('Review new tip rows before conversion');
const commonPath = path.join(root, 'config/propCommonText.xlsx');
const common = await SpreadsheetFile.importXlsx(await FileBlob.load(commonPath));
const commonSheet = common.worksheets.getItem('CommonText');
const commonRows = commonSheet.getRange('A1:C1000').values;
let tipRow = commonRows.findIndex(row => row[0] === tipAlias);
if (tipRow < 0) {
    if (commonRows.some(row => row[1] === tipId)) throw new Error('Common text ID collision');
    tipRow = commonRows.findIndex((row, index) => index >= 3 && !row[0] && !row[1]);
}
if (tipRow < 3) throw new Error('No valid common text insertion row');
commonSheet.getRange(`A${tipRow + 1}:C${tipRow + 1}`).values = [[tipAlias, tipId, tips[0].ActionParam[0]]];
await (await SpreadsheetFile.exportXlsx(common)).save(commonPath);

function expression(row) {
    const signature = signatures[row.Action];
    if (!signature || signature.length !== row.ActionParam.length) throw new Error(`Unknown signature ${row.ID}`);
    const argumentsText = row.ActionParam.map((value, index) => {
        if (row.Action === 'Tips') return String(tipId);
        if (signature[index] === 'String') return JSON.stringify(value);
        if (!/^-?(?:\d+(?:\.\d+)?|\.\d+)$/.test(value)) throw new Error(`Invalid numeric parameter ${row.ID}`);
        return String(Number(value));
    });
    return callExpression(row.Action, argumentsText.map(value => JSON.parse(value)));
}
add('TaskSetting', fields('ID InitialChapterID', ['int','int']), [[1, 1]]);
const guideSheet = add('TaskGuide', fields('ID NumID GroupID TaskGuideType SourceGuideType NeedSave ConditionExpression ActionExpression NextID FailureNextID',
    ['int','int','int','enum','int','bool','string','string','int','int']),
    sources.tbguidetask.map(row => [row.ID, row.ID, row.GroupID, row.GuideType.name,
        row.GuideType.value, row.NeedSave, 'true', expression(row), row.NextID, row.FNextID]));
guideSheet.getRange('G:G').format.columnWidth = 38;
guideSheet.getRange('H:H').format.columnWidth = 95;

await fs.mkdir(temporary, {recursive: true});
const output = await SpreadsheetFile.exportXlsx(workbook);
await output.save(path.join(root, 'config/propTask.xlsx'));
const manifest = {sources: hashes, sheets,
    idPolicy: 'All task sheets use numeric IDs; string IDs require explicit authorization except language tables.',
    signatures: Object.fromEntries(Object.entries(signatures).map(([name, args]) =>
        [commandName(name), name === 'Navigation' ? ['Number'] : args])), tipId, tipAlias,
    actionFormat: 'Command(argument, ...)', navigationTargets,
    taskIdPolicy: 'Numeric ID; no TASK_ constants. Existing NumID column retained.',
    chapterIdPolicy: 'Numeric ID; no TASK_CHAPTER constants. Existing NumID column retained.',
    conditionPolicy: 'Explicit boolean Evaluate expression; false skips this action attempt, not failure.',
    note: 'Source enum numbers preserved separately; action expressions are checked using com.oathx.expression.',
    unlockConditionTypes: [...new Map(sources.tbtaskunlock.flatMap(r => r.UnlockCondition.map(c => [c.Type.value,c.Type]))).values()]};
await fs.writeFile(path.join(root, 'tools/task-config/manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
const preview = await workbook.render({sheetName:'TaskGuide', range:'G1:J8', scale:1, format:'png'});
await fs.writeFile(path.join(temporary, 'task-preview.png'), new Uint8Array(await preview.arrayBuffer()));
console.log(JSON.stringify(sheets.map(s => ({name:s.name,rows:s.rows}))));
