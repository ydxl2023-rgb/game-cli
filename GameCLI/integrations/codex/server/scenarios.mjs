import {createHash} from 'node:crypto';
import {createServer} from 'node:net';
import {join, relative} from 'node:path';
import {readFile, writeFile} from 'node:fs/promises';
import {setTimeout as delay} from 'node:timers/promises';
import {connectUnityCli} from './unity-cli.mjs';

const tolerance = 1; // screen pixels
const operations = {
  call: ['type', 'member', 'args', 'equals'],
  wait: ['type', 'member', 'args', 'equals', 'stableMs', 'timeoutMs'],
  layout: ['path', 'name', 'target', 'count', 'columns', 'fullyVisible', 'separate', 'inside', 'uniformScale'],
  screenshot: []
};
const text = value => typeof value === 'string' && value.trim().length > 0;
const positive = value => Number.isSafeInteger(value) && value > 0;

export function validateSuite(suite, templateId) {
  if (suite?.schema !== 'gamecli-scenarios/1' || suite.templateId !== templateId ||
      !Array.isArray(suite.steps) || !suite.steps.length) throw Error('Invalid scenario suite or template identity.');
  const ids = new Set();
  for (const step of suite.steps) {
    if (!step || !Object.hasOwn(operations, step.op) || !text(step.id) || !/^[a-z][a-z0-9-]{0,63}$/.test(step.id ?? '') ||
        ids.has(step.id)) throw Error('Unknown operation or invalid/duplicate step ID.');
    const allowed = ['id', 'op', ...operations[step.op]];
    if (Object.keys(step).some(key => !allowed.includes(key))) throw Error('Unknown scenario field at ' + step.id);
    if (['call', 'wait'].includes(step.op)) {
      if (!text(step.type) || !text(step.member) || !text(step.equals) ||
          (step.args !== undefined && (!Array.isArray(step.args) || !step.args.every(v => ['boolean','number','string'].includes(typeof v))))) {
        throw Error('Invalid method or expected return at ' + step.id);
      }
      if (step.op === 'wait' && (!positive(step.timeoutMs ?? 20000) || (step.timeoutMs ?? 0) > 120000 ||
          !Number.isSafeInteger(step.stableMs ?? 0) || (step.stableMs ?? 0) < 0 || (step.stableMs ?? 0) >= (step.timeoutMs ?? 20000))) throw Error('Invalid wait window.');
    }
    if (step.op === 'layout') {
      if (!text(step.path) || !positive(step.count) || (step.columns !== undefined && (!positive(step.columns) || step.columns > step.count)) ||
          ['name','target'].some(k => step[k] !== undefined && !text(step[k])) ||
          ['separate','fullyVisible','uniformScale'].some(k => step[k] !== undefined && typeof step[k] !== 'boolean') ||
          (step.inside !== undefined && !suite.steps.slice(0, ids.size).some(s => s.id === step.inside && s.op === 'layout'))) throw Error('Invalid layout assertion at ' + step.id);
    }
    ids.add(step.id);
  }
  if (!suite.steps.some(s => s.op !== 'screenshot')) throw Error('A scenario needs at least one assertion.');
}

function contains(a, b) {
  return b.x >= a.x-tolerance && b.y >= a.y-tolerance &&
    b.x+b.width <= a.x+a.width+tolerance && b.y+b.height <= a.y+a.height+tolerance;
}

export function checkLayout(step, data, parentData) {
  const items = data.items;
  const problems = [];
  if (items.length !== step.count) problems.push('Expected ' + step.count + ' items, found ' + items.length);
  for (const [i, item] of items.entries()) {
    const b = item.bounds;
    if (![b, item.visible].every(r => r && ['x','y','width','height'].every(k => Number.isFinite(r[k]))) ||
        b.width <= 0 || b.height <= 0) throw Error('Invalid geometry for item ' + i);
    if (step.fullyVisible && !contains(item.visible, b)) problems.push('Clipped item ' + i);
    if (step.uniformScale && Math.abs(Math.abs(item.scale.x)-Math.abs(item.scale.y)) > Math.max(Math.abs(item.scale.x),Math.abs(item.scale.y)) * .01) problems.push('Nonuniform scale at ' + i);
    if (step.inside) {
      const parent = parentData?.items.find(p => p.instanceId === item.instanceId);
      if (!parent || !contains(parent.bounds,b)) problems.push('Outside matching parent at ' + i);
    }
    if (step.separate) {
      for (let j = 0; j < i; j++) {
        const a = items[j].bounds;
        const overlapX = Math.min(a.x+a.width,b.x+b.width)-Math.max(a.x,b.x);
        const overlapY = Math.min(a.y+a.height,b.y+b.height)-Math.max(a.y,b.y);
        if (overlapX > tolerance && overlapY > tolerance) problems.push('Overlapping items ' + j + ', ' + i);
      }
    }
  }
  if (step.columns) {
    const sorted = items.map(i => i.bounds.y+i.bounds.height/2).sort((a,b) => b-a);
    const rows = [];
    for (const y of sorted) {
      if (!rows.length || rows.at(-1).y-y > tolerance) rows.push({y,count:0});
      rows.at(-1).count++;
    }
    if (rows.some((row,i) => row.count !== Math.min(step.columns,step.count-i*step.columns))) problems.push('Unexpected row lengths: ' + rows.map(r=>r.count).join(', '));
  }
  return problems;
}

async function executeStep(step, cli, directory, project, layouts) {
  if (step.op === 'screenshot') {
    const path = join(directory, step.id+'.png');
    const actual = await cli(['screenshot','--dst',relative(project,path)]);
    const png = await readFile(path);
    if (!png.subarray(0,8).equals(Buffer.from('89504e470d0a1a0a','hex'))) throw Error('Screenshot PNG missing.');
    return {actual:{...actual,sha256:createHash('sha256').update(png).digest('hex')}, problems:[]};
  }
  if (step.op === 'layout') {
    const actual = await cli(['measure','--path',step.path,...(step.name?['--name',step.name]:[]),...(step.target?['--child-name',step.target]:[])]);
    const problems = checkLayout(step,actual,layouts.get(step.inside));
    layouts.set(step.id,actual);
    return {actual,problems};
  }
  const args = ['call','--invoke-type',step.type,'--invoke-member',step.member,'--args-json',JSON.stringify(step.args ?? [])];
  const deadline = Date.now() + (step.timeoutMs ?? 20000);
  let since, actual;
  do {
    actual = await cli(args);
    if (actual.returnSummary === step.equals) {
      since ??= Date.now();
      if (Date.now()-since >= (step.stableMs ?? 0)) return {actual,problems:[]};
    } else since = undefined;
    if (step.op === 'call') break;
    await delay(200);
  } while (Date.now() < deadline);
  return {actual,problems:['Expected '+step.equals+' at '+step.member]};
}

async function waitForPlayState(cli, playing) {
  const deadline = Date.now()+30000;
  do {
    try {
      const state = await cli(['run','status']);
      if (state.isPlaying === playing && state.isPlayingOrWillChangePlaymode === playing) return;
    } catch (error) {
      if (Date.now() >= deadline) throw error;
    }
    await delay(300);
  } while (Date.now() < deadline);
  throw Error('Timed out waiting for Play Mode transition.');
}

export async function runScenarios(suite, binding, directory, cliPath, fingerprint) {
  validateSuite(suite,suite.templateId);
  const steps = suite.steps.map(({id,op}) => ({id,op,status:'not-verified'}));
  const report = {schema:'gamecli-scenario-result/1',projectId:binding.projectId,unityProjectId:binding.unityProjectId,fingerprint,status:'not-verified',steps};
  const lock = createServer();
  let cli, started = false;
  try {
    const key = createHash('sha256').update(binding.path).digest('hex').slice(0,24);
    const socket = process.platform === 'win32' ? '\\\\.\\pipe\\gamecli-scenarios-'+key : '/tmp/gamecli-scenarios-'+key+'.sock';
    await new Promise((ok,fail) => { lock.once('error',fail); lock.listen(socket,ok); });
    cli = await connectUnityCli(binding.path,cliPath);
    const state = await cli(['run','status']);
    if (state.isPlaying || state.isPlayingOrWillChangePlaymode) throw Error('Scenario requires a fresh Play session; current session was preserved.');
    await cli(['run','play']);
    started = true;
    await waitForPlayState(cli,true);
    const layouts = new Map();
    for (const [index,step] of suite.steps.entries()) {
      if (report.status === 'failed' && step.op !== 'screenshot') continue;
      let result;
      try {
        result = await executeStep(step,cli,directory,binding.path,layouts);
      } catch (error) {
        steps[index].error = error.message;
        throw error;
      }
      Object.assign(steps[index],result,{status:result.problems.length?'failed':'passed'});
      if (result.problems.length) report.status = 'failed';
    }
    if (steps.every(s=>s.status==='passed')) report.status='passed';
  } catch (error) {
    report.error = error.message;
  } finally {
    if (started) {
      try { await cli(['run','stop']); await waitForPlayState(cli,false); }
      catch (error) { report.cleanupError=error.message; if(report.status==='passed')report.status='not-verified'; }
    }
    if (lock.listening) await new Promise(ok => lock.close(ok));
  }
  await writeFile(join(directory,'scenarios.json'),JSON.stringify({suite,...report},null,2));
  return report;
}
