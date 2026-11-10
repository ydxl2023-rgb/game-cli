import test from 'node:test';
import assert from 'node:assert/strict';
import {createRelations} from '../ui/relations.mjs';

test('middle-button panning moves the view without editing cards and ends on release or blur',()=>{
 const elements=new Map();
 const target=()=>({style:{},listeners:{},scrollLeft:0,scrollTop:0,clientHeight:400,
  classList:{add(){},remove(){}},setAttribute(){},append(){},querySelector(){return {};},
  addEventListener(type,fn){(this.listeners[type]??=[]).push(fn);},
  setPointerCapture(id){this.capture=id;},hasPointerCapture(id){return this.capture===id;},releasePointerCapture(){this.capture=null;},
  getBoundingClientRect(){return {left:0,top:0};}});
 const oldDocument=globalThis.document,oldWindow=globalThis.window;
 globalThis.document={getElementById(id){if(!elements.has(id))elements.set(id,target());return elements.get(id);},createElement:target};
 globalThis.window=target();
 let edits=0;
 try{
  createRelations({getDesign:()=>({nodes:[]}),edit:()=>edits++});
  const view=elements.get('relations-view'),plane=elements.get('relations-plane'),win=globalThis.window;
  const emit=(object,type,extra={})=>{const event={button:1,pointerId:7,clientX:100,clientY:100,preventDefault(){},stopPropagation(){},...extra};for(const fn of object.listeners[type]??[])fn(event);};
  emit(view,'pointerdown');emit(view,'pointermove',{clientX:160,clientY:75});
  assert.equal(plane.style.transform,'translate(60px, -25px)');assert.equal(view.capture,7);
  emit(win,'pointerup');emit(view,'pointermove',{clientX:200});
  assert.equal(plane.style.transform,'translate(60px, -25px)');assert.equal(view.capture,null);
  emit(view,'wheel',{deltaY:200,deltaMode:0});
  emit(view,'pointerdown');emit(view,'pointermove',{clientX:130,clientY:120});
  const zoom=Number(plane.style.zoom),translation=plane.style.transform.match(/translate\(([^p]+)px, ([^p]+)px\)/);
  assert.ok(Math.abs(Number(translation[1])*zoom-90)<.001);
  assert.ok(Math.abs(Number(translation[2])*zoom+5)<.001);
  emit(win,'blur');assert.equal(view.capture,null);assert.equal(edits,0);
 }finally{globalThis.document=oldDocument;globalThis.window=oldWindow;}
});
