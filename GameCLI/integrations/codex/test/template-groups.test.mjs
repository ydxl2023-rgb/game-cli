import test from 'node:test';import assert from 'node:assert/strict';
import {groupedTemplates,templateGroups} from '../shared/template-groups.mjs';
import {builtInTemplates} from '../shared/templates.mjs';
test('Loading variants share a group and stay separate from prefab groups',()=>{const templates=[{id:'A',category:'system',group:'loading'},{id:'B',category:'system',group:'loading'},{id:'C',category:'prefab',group:'item'}];const result=groupedTemplates('system',templateGroups,templates);assert.equal(result.length,1);assert.equal(result[0].name,'Loading');assert.deepEqual(result[0].templates.map(t=>t.id),['A','B']);});
test('legacy and unknown groups remain accessible under uncategorized',()=>{const result=groupedTemplates('system',[],[{id:'A',category:'system'},{id:'B',category:'system',group:'unknown'}]);assert.equal(result[0].id,'general');assert.equal(result[0].templates.length,2);});
test('all built-in templates have a category-scoped group',()=>{for(const t of builtInTemplates())assert.ok(templateGroups.some(g=>g.id===t.group&&g.category===t.category));});
