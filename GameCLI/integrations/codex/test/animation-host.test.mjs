import test from 'node:test';import assert from 'node:assert/strict';
import {ensureAnimationHost} from '../shared/animation-host.mjs';
import {validateTemplate,builtInTemplates} from '../shared/templates.mjs';
import {designHtml} from '../shared/design.mjs';
test('system Animator wrapper preserves bounds references and remains idempotent',()=>{
 const template=builtInTemplates().find(t=>t.category==='system'),d=template.payload.design;const root=template.payload.root;const host=d.nodes.find(n=>n.animationHost);assert.ok(host);assert.equal(host.parent,root);assert.deepEqual(d.nodes.filter(n=>n.parent===root).map(n=>n.id),[host.id]);const before=structuredClone(d);assert.equal(ensureAnimationHost(d,root),host.id);assert.deepEqual(d,before);assert.match(designHtml(d),/data-ui-animation-host="true"/);assert.deepEqual(validateTemplate({root,design:d},'system').design,d);
 d.width=1200;d.height=1800;ensureAnimationHost(d,root);const owner=d.nodes.find(n=>n.id===root);assert.equal(host.width,owner.width);assert.equal(host.height,owner.height);
});
test('ordinary prefab stays unwrapped',()=>{for(const t of builtInTemplates().filter(t=>t.category==='prefab'))assert.equal(t.payload.design.nodes.filter(n=>n.animationHost).length,0);});
