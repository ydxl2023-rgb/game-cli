import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { starter, validateDesign, designHtml, pointInDesign, appendControl, phoneDesign, textTarget, resizeNode, resizedRect } from '../shared/design.mjs';
import { createStore } from '../server/store.mjs';
import { createHttpServer } from '../server/http.mjs';
import { createMcpServer, resourceUri } from '../server/mcp.mjs';
import { Client } from '@modelcontextprotocol/sdk/client/index.js';
import { InMemoryTransport } from '@modelcontextprotocol/sdk/inMemory.js';

test('prefab node names use short PascalCase while stable IDs retain action targets',()=>{
  const d=starter(), id=appendControl(d,'InventorySystem',null,{x:0,y:0});
  const clean=validateDesign(d), get=suffix=>clean.nodes.find(n=>n.id===id+suffix);
  assert.equal(get('').name,'Inventory'); assert.equal(get('Background').name,'Background');
  assert.equal(get('UseLabel').name,'Label');
  const target=get('Use').action.target;
  clean.nodes.find(n=>n.id===target).name='confirmation';
  const renamed=validateDesign(clean);
  assert.equal(renamed.nodes.find(n=>n.id===id+'Use').action.target,target);
  assert.match(designHtml(renamed),/data-ui-name="Confirmation"/);
  get('Use').name='Use_Button'; assert.throws(()=>validateDesign(clean),/大驼峰/);
});

test('Canvas is a fixed root independent of design filename and preserves system parenting',()=>{
  const d=starter(), id=appendControl(d,'InventorySystem',null,{x:0,y:0});
  const clean=validateDesign(d); assert.equal(clean.rootId,'Canvas');
  assert.equal(clean.nodes.find(n=>n.id===id).parent,null);
  clean.name='RenamedDesign';
  assert.match(designHtml(clean),/data-ui-id="Canvas" data-ui-type="Container"/);
  clean.nodes[0].id='Canvas'; assert.throws(()=>validateDesign(clean));
});

test('inventory system is an isolated prefab group with wired actions and unique instances', async t => {
  const d=starter(), originalCount=d.nodes.length;
  const id=appendControl(d,'InventorySystem',null,{x:0,y:0});
  const group=d.nodes.slice(originalCount), map=new Map(d.nodes.map(n=>[n.id,n]));
  assert.equal(group.filter(n=>n.parent===null).length,1);
  const background=group.find(n=>n.parent===id);
  assert.equal(background.role,'background'); assert.equal(background.type,'Image');
  assert.equal(background.width,d.width); assert.equal(background.height,d.height);
  const old=structuredClone(d); old.nodes=old.nodes.filter(n=>n.id!==background.id);
  const upgraded=phoneDesign(old); assert.equal(upgraded.nodes.find(n=>n.parent===id).role,'background');
  assert.deepEqual(phoneDesign(upgraded),upgraded);
  for(const n of group) { let root=n; while(root.parent)root=map.get(root.parent); assert.equal(root.id,id); }
  const dialog=group.find(n=>n.template==='ConfirmDialog');
  assert.deepEqual(map.get(id+'Use').action,{type:'show',target:dialog.id});
  assert.equal(group.filter(n=>n.template==='ItemSlot').length,12);
  assert.ok(group.filter(n=>n.template==='ItemSlot').every(n=>n.width===128 && n.height===128));
  const second=appendControl(d,'InventorySystem',null,{x:0,y:0});
  assert.notEqual(second,id); assert.notEqual(d.nodes.find(n=>n.id===second+'Use').action.target,dialog.id);
  while(d.nodes.length+93<=300) appendControl(d,'InventorySystem',null,{x:0,y:0});
  const before=JSON.stringify(d); assert.throws(()=>appendControl(d,'InventorySystem',null,{x:0,y:0})); assert.equal(JSON.stringify(d),before);
  const dir=await mkdtemp(join(tmpdir(),'gamecli-system-'));t.after(()=>rm(dir,{recursive:true,force:true}));
  const store=createStore(dir);await store.save(d,0);
  assert.equal((await store.snapshot()).design.nodes.find(n=>n.id===id).template,'InventorySystem');
});

test('confirmation dialog has full-screen mask, centered panel and editable button labels', async t => {
  const d=starter(), id=appendControl(d,'ConfirmDialog','InventoryPanel',{x:0,y:0});
  const get=suffix=>d.nodes.find(n=>n.id===id+suffix);
  assert.equal(get('').parent,null); assert.equal(get('Mask').width,d.width);
  assert.equal(get('Mask').opacity,.6);
  assert.equal(get('Panel').x,(d.width-get('Panel').width)/2);
  textTarget(d,get('Confirm')).text='确定购买';
  assert.match(designHtml(d),/确定购买/);
  resizeNode(d,get(''),600,1000);
  assert.equal(get('Mask').width,600);
  assert.equal(get('ConfirmLabel').width,get('Confirm').width);
  assert.notEqual(appendControl(d,'ConfirmDialog',null,{x:0,y:0}),id);
  const dir=await mkdtemp(join(tmpdir(),'gamecli-dialog-')); t.after(()=>rm(dir,{recursive:true,force:true}));
  const store=createStore(dir); await store.save(d,0);
  assert.equal((await store.snapshot()).design.nodes.find(n=>n.id===id).template,'ConfirmDialog');
});

test('scroll panel reuses 128px item slots and preserves scrolling in export and storage', async t => {
  const d = starter(), id = appendControl(d,'ScrollPanel',null,{x:500,y:800});
  const panel=d.nodes.find(n=>n.id===id), content=d.nodes.find(n=>n.parent===id);
  const slots=d.nodes.filter(n=>n.parent===content.id);
  assert.equal(panel.template,'ScrollPanel'); assert.equal(slots.length,12);
  assert.ok(content.height>panel.height);
  assert.ok(slots.every(n=>n.template==='ItemSlot' && n.width===128 && n.height===128));
  assert.equal(slots[3].x,16); assert.equal(slots[3].y,160);
  assert.ok(slots.every(n=>d.nodes.filter(c=>c.parent===n.id).length===5));
  assert.match(designHtml(d),/overflow-y:auto/);
  resizeNode(d,panel,608,320);
  assert.equal(slots[3].y,16); assert.equal(slots[4].y,160);
  assert.equal(content.height,448);
  resizeNode(d,panel,320,1000);
  assert.equal(slots[2].x,16); assert.equal(slots[2].y,160);
  assert.equal(slots[1].x,160);
  const positions = slots.map(n=>[n.x,n.y]);
  resizeNode(d,panel,320,1500);
  assert.deepEqual(slots.map(n=>[n.x,n.y]),positions);
  resizeNode(d,panel,320,1000);
  assert.equal(content.height,1000);
  assert.ok(slots.every(n=>n.width===128 && n.height===128));
  assert.equal(d.nodes.find(n=>n.parent===slots[0].id && n.role==='icon').width,128);
  const dir=await mkdtemp(join(tmpdir(),'gamecli-scroll-')); t.after(()=>rm(dir,{recursive:true,force:true}));
  const store=createStore(dir); await store.save(d,0);
  assert.equal((await store.snapshot()).design.nodes.find(n=>n.id===id).template,'ScrollPanel');
  appendControl(d,'ScrollPanel',null,{x:500,y:800}); assert.doesNotThrow(()=>validateDesign(d));
});

test('item slot has five ordered editable layers, survives serialization and resizes as a group', async t => {
  const d = starter(), id = appendControl(d,'ItemSlot',null,{x:500,y:800});
  const root = d.nodes.find(n=>n.id===id), children=d.nodes.filter(n=>n.parent===id);
  assert.equal(root.width,128); assert.equal(root.height,128);
  assert.deepEqual([children[1].x,children[1].y,children[1].width,children[1].height],[0,0,128,128]);
  assert.deepEqual(children.map(n=>n.role),['background','icon','mask','count','frame']);
  assert.equal(children[3].text,'99'); assert.equal(children[2].opacity,.25);
  resizeNode(d,root,480,360);
  assert.equal(children[0].width,480); assert.equal(children[0].height,360);
  assert.equal(children[1].x,0); assert.equal(children[1].y,0);
  assert.equal(children[1].width,480); assert.equal(children[1].height,360);
  assert.equal(children[3].width,384);
  const clean=validateDesign(d); assert.equal(clean.nodes.find(n=>n.id===id).template,'ItemSlot');
  assert.match(designHtml(clean),/data-studio-role="frame"/);
  assert.match(designHtml(clean),/background:transparent;border:6px solid/);
  const next=appendControl(d,'ItemSlot',null,{x:500,y:800}); assert.notEqual(next,id);
  const dir=await mkdtemp(join(tmpdir(),'gamecli-item-slot-')); t.after(()=>rm(dir,{recursive:true,force:true}));
  const store=createStore(dir); await store.save(clean,0);
  assert.equal((await store.snapshot()).design.nodes.find(n=>n.id===id+'Mask').opacity,.25);
    resizeNode(d,root,1,1); assert.doesNotThrow(()=>validateDesign(d));
});

test('resize keeps button labels fitted and text edits target exported child', () => {
  const d = starter(), button = d.nodes.find(n => n.id === 'EquipButton');
  const label = textTarget(d, button);
  resizeNode(d, button, 600, 180);
  assert.equal(label.width, 600); assert.equal(label.height, 180);
  label.text = '确认装备\n下一步'; label.fontSize = 48;
  validateDesign(d); assert.match(designHtml(d), /确认装备\n下一步/);
  assert.equal(button.text, '');
  assert.deepEqual(resizedRect({x:20,y:30,width:100,height:80},'nw',30,20),{x:50,y:50,width:70,height:60});
  assert.equal(resizedRect({x:20,y:30,width:100,height:80},'e',-200,0).width,1);
  assert.throws(() => resizeNode(d, button, 0, 180), /宽高/);
  label.x = 10; label.width = 300;
  resizeNode(d, button, 700, 200); assert.equal(label.width, 300);
});

test('phone canvas, scaled drop positions and explicit button text nodes', () => {
  const d = starter();
  assert.equal(d.width / d.height, 9 / 16);
  assert.deepEqual(pointInDesign(154, 296, { left: 100, top: 200, width: 270, height: 480 }, 1080, 1920), { x: 216, y: 384 });
  const id = appendControl(d, 'Button', 'InventoryPanel', { x: 500, y: 900 });
  const n = d.nodes.find(n => n.id === id), label = d.nodes.find(n => n.parent === id);
  assert.equal(n.x, 320); assert.equal(n.y, 850); assert.equal(n.text, '');
  assert.equal(label.type, 'Text'); assert.equal(label.text, '按钮');
  validateDesign(d);
  const html = designHtml(d); assert.ok(html.includes(`data-ui-id="${label.id}"`));
  const outside = appendControl(d, 'Image', null, { x: -100, y: 9999 });
  const clamped = d.nodes.find(n => n.id === outside);
  assert.equal(clamped.x, 0); assert.equal(clamped.y + clamped.height, d.height);
  assert.deepEqual(phoneDesign(d), d);
});

test('invalid IDs, cycles, unsupported controls and executable text are handled', () => {
  const d = starter(); validateDesign(d);
  d.nodes[0].parent = 'DetailPanel'; assert.throws(() => validateDesign(d), /循环/);
  const duplicate = starter(); duplicate.nodes[1].id = duplicate.nodes[0].id; assert.throws(() => validateDesign(duplicate), /唯一/);
  const malformed = starter(); malformed.nodes[0].width = -1; assert.throws(() => validateDesign(malformed), /尺寸/);
  const unsafe = starter(); unsafe.nodes[1].text = '<script>alert(1)</script>';
  const html = designHtml(unsafe); assert.ok(!html.includes('<script>')); assert.ok(html.includes('&lt;script&gt;'));
  assert.ok(html.includes('data-ui-type="Button"')); assert.ok(html.includes('fui-design-resolution'));
});

test('draft survives new store instances, rejects stale save, exports separate source files', async t => {
  const root = await mkdtemp(join(tmpdir(), 'gamecli-studio-')); t.after(() => rm(root, { recursive: true, force: true }));
  const a = createStore(root), b = createStore(root);
  assert.equal((await a.snapshot()).revision, 0);
  const d = starter(); d.nodes[1].text = 'Updated';
  await a.save(d, 0); assert.equal((await b.snapshot()).design.nodes[1].text, 'Updated');
  await assert.rejects(b.save(starter(), 0), /其他窗口/);
  const output = await a.exportDesign(d);
  assert.match(await readFile(join(output.directory, 'UIInventory.html'), 'utf8'), /Updated/);
  assert.equal(JSON.parse(await readFile(join(output.directory, 'UIInventory.studio.json'), 'utf8')).name, d.name);
});

test('HTTP serves UI, preserves draft and rejects foreign origin and invalid inputs', async t => {
  const root = await mkdtemp(join(tmpdir(), 'gamecli-http-'));
  const server = createHttpServer(createStore(root),{list:async()=>({source:'postgresql',categories:[{id:'prefab',name:'预制控件'}],templates:[]}),close:async()=>{}}); await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(async () => { await new Promise(resolve => server.close(resolve)); await rm(root, { recursive: true, force: true }); });
  const base = `http://127.0.0.1:${server.address().port}`;
  assert.equal((await fetch(base + '/studio')).status, 200);
  assert.equal((await (await fetch(base+'/api/studio/templates')).json()).source,'postgresql');
  const body = JSON.stringify({ design: starter(), revision: 0 });
  const headers = { 'Content-Type': 'application/json' };
  assert.equal((await fetch(base + '/api/studio/save', { method: 'POST', headers: { ...headers, Origin: 'https://example.com' }, body })).status, 403);
  assert.equal((await fetch(base + '/api/studio/save', { method: 'POST', headers, body })).status, 200);
  assert.equal((await (await fetch(base + '/api/studio')).json()).revision, 1);
  assert.equal((await fetch(base + '/api/studio/save', { method: 'POST', headers, body })).status, 400);
});

test('MCP exposes UI resource and real save/export tools', async t => {
  const root = await mkdtemp(join(tmpdir(), 'gamecli-mcp-'));
  const server = createMcpServer(createStore(root),{list:async()=>({source:'postgresql',categories:[],templates:[]}),close:async()=>{}});
  const client = new Client({ name: 'studio-test', version: '1.0.0' });
  const [a,b] = InMemoryTransport.createLinkedPair();
  await Promise.all([server.connect(a), client.connect(b)]);
  t.after(async () => { await client.close(); await server.close(); await rm(root, { recursive: true, force: true }); });
  const tools = (await client.listTools()).tools;
  const templates=await client.callTool({name:'gamecli_ui_templates',arguments:{}});assert.equal(templates.structuredContent.source,'postgresql');
  assert.equal(tools.find(t => t.name === 'gamecli_ui_open')._meta.ui.resourceUri, resourceUri);
  const resource = await client.readResource({ uri: resourceUri }); assert.match(resource.contents[0].text, /UI Studio/);
  const snapshot = await client.callTool({ name: 'gamecli_ui_open', arguments: {} }); assert.equal(snapshot.structuredContent.revision, 0);
  const saved = await client.callTool({ name: 'gamecli_ui_save', arguments: { design: starter(), revision: 0 } }); assert.equal(saved.structuredContent.revision, 1);
  const invalid = await client.callTool({ name: 'gamecli_ui_save', arguments: { design: {}, revision: 1 } }); assert.equal(invalid.isError, true);
  const exported = await client.callTool({ name: 'gamecli_ui_export', arguments: { design: starter() } }); assert.ok(exported.structuredContent.html.includes('data-ui-id'));
});
