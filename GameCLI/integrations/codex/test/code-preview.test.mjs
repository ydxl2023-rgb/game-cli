import test from 'node:test';
import assert from 'node:assert/strict';
import {highlightSource} from '../ui/code-preview.mjs';
test('source preview highlights C# and metadata without interpreting source as HTML',()=>{
 const cs=highlightSource('XLauncher.cs','// comment\r\npublic class XLauncher { string tip = "<script>alert(1)</script>"; }');
 assert.equal(cs.language,'csharp');assert.equal(cs.lines,2);
 assert.match(cs.html,/hljs-keyword/);assert.match(cs.html,/hljs-comment/);assert.match(cs.html,/hljs-string/);
 assert.ok(!cs.html.includes('<script>'));assert.match(cs.html,/&lt;script&gt;/);
 const meta=highlightSource('XLauncher.cs.meta','fileFormatVersion: 2\nguid: abc');
 assert.equal(meta.language,'yaml');assert.match(meta.html,/hljs-attr/);
 assert.equal(highlightSource('notes.txt','<img src=x onerror=alert(1)>').html,'&lt;img src=x onerror=alert(1)&gt;');
});
