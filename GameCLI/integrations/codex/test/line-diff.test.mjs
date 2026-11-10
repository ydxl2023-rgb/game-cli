import test from 'node:test';
import assert from 'node:assert/strict';
import {alignedDiff,changedSpan} from '../shared/line-diff.mjs';
test('align inserted lines without marking following unchanged lines modified',()=>{
 const rows=alignedDiff('a\nb\nc','a\nx\nb\nc');assert.deepEqual(rows.map(r=>r.kind),['same','added','same','same']);assert.equal(rows[2].left,2);assert.equal(rows[2].right,3);
});
test('paired replacements highlight only changed characters',()=>{const rows=alignedDiff('size: 128\nend','size: 240\nend');assert.equal(rows[0].kind,'changed');assert.deepEqual(changedSpan(rows[0].before,rows[0].after),{start:6,leftEnd:9,rightEnd:9});});
test('large and repeated inputs preserve every source line with bounded alignment',()=>{for(const [a,b]of [[Array.from({length:4000},(_,i)=>'node'+i),Array.from({length:4000},(_,i)=>i===2000?'new':'node'+i)],[Array(400).fill('a'),Array(500).fill('b')]]){const rows=alignedDiff(a.join('\n'),b.join('\n'));assert.deepEqual(rows.filter(r=>r.left!==null).map(r=>r.before),a);assert.deepEqual(rows.filter(r=>r.right!==null).map(r=>r.after),b);}});
