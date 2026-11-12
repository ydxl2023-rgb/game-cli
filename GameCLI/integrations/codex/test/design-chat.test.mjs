import test from 'node:test';
import assert from 'node:assert/strict';
import {validateAttachments,validateProposal} from '../server/design-chat.mjs';
import {designPrompt} from '../server/design-agent.mjs';
import {starter} from '../shared/design.mjs';
test('design discussion requires valid proposals, limited attachments and real buttons',()=>{
 const design=starter();
 assert.deepEqual(validateProposal({reply:'需要确认系统',operations:[]},design,[]).operations,[]);
 assert.throws(()=>validateProposal({reply:'bad',operations:[{type:'connect',sourceId:'missing',targetId:'x',action:'show'}]},design,[]),/真实按钮/);
 assert.throws(()=>validateProposal({reply:'bad',operations:[{type:'add',key:'invented',templateId:'missing',version:1}]},design,[]),/不存在/);
 assert.throws(()=>validateAttachments(Array(5).fill({name:'x',content:'a'})),/最多/);
 assert.throws(()=>validateAttachments([{name:'x',content:'a'.repeat(100001)}]),/超过/);
 assert.deepEqual(validateAttachments([{name:'requirements.md',content:'需求'}]),[{name:'requirements.md',content:'需求'}]);
 assert.match(designPrompt({project:{id:'A'}}),/不执行修改/);
});
