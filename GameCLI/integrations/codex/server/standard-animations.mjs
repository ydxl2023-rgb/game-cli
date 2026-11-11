import {createHash} from 'node:crypto';
import {z} from 'zod';

export const standardAnimationsSchema = `
CREATE TABLE IF NOT EXISTS gamecli_ui.standard_animations (
 id text NOT NULL, version integer NOT NULL CHECK(version > 0),
 name text NOT NULL, category text NOT NULL,
 status text NOT NULL CHECK(status IN ('draft','published')),
 definition jsonb NOT NULL, sha256 text NOT NULL CHECK(length(sha256)=64),
 created_by text NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 PRIMARY KEY(id,version)
);`;

const key = z.object({time:z.number().finite().nonnegative(),value:z.number().finite(),
 inSlope:z.number().finite(),outSlope:z.number().finite(),inWeight:z.number().min(0).max(1),
 outWeight:z.number().min(0).max(1),weightedMode:z.number().int().min(0).max(3),tangentMode:z.number().int()}).strict();
const curve = z.object({target:z.string().regex(/^[a-zA-Z][a-zA-Z0-9]*$/),
 component:z.literal('UnityEngine.Transform'),property:z.enum(['m_LocalScale.x','m_LocalScale.y','m_LocalScale.z']),
 keys:z.array(key).min(2).max(256),preWrapMode:z.literal(8),postWrapMode:z.literal(8)}).strict();
const definitionSchema = z.object({schemaVersion:z.literal('standard-animation/1'),
 id:z.string().regex(/^[a-z][a-z0-9.-]{0,95}$/),version:z.number().int().positive(),
 name:z.string().min(1),category:z.literal('dialog.open'),description:z.string().min(1),
 targets:z.array(z.object({id:z.string().regex(/^[a-zA-Z][a-zA-Z0-9]*$/),description:z.string().min(1),multiple:z.boolean()}).strict()).min(1),
 spec:z.object({duration:z.number().positive().max(10),scaleFrom:z.number().positive(),scaleTo:z.number().positive(),
 easing:z.literal('easeOutCubic'),loop:z.literal(false),timeMode:z.literal('unscaled'),
 guidance:z.array(z.string()).min(1)}).strict(),
 clip:z.object({schemaVersion:z.literal('animation-clip/1'),frameRate:z.number().positive(),loop:z.literal(false),curves:z.array(curve).min(1).max(64)}).strict(),
 controller:z.object({defaultState:z.literal('Open'),states:z.array(z.object({name:z.literal('Open'),clip:z.literal('open'),speed:z.literal(1)}).strict()).length(1),transitions:z.array(z.never()).length(0)}).strict(),
 provenance:z.object({source:z.string(),tool:z.string(),notes:z.string()}).strict(),
 preview:z.null()
}).strict();

// This first schema describes scale entrances; extend its version for other motion kinds.
export function validateStandardAnimation(input) {
 const value=definitionSchema.parse(input),targets=new Set(value.targets.map(t=>t.id));
 if(targets.size!==value.targets.length)throw Error('Duplicate animation target.');
 const bindings=new Set();
 for(const c of value.clip.curves){
  const binding=c.target+':'+c.property;
  if(!targets.has(c.target)||bindings.has(binding))throw Error('Unknown or duplicate curve target.');
  bindings.add(binding);
  if(c.keys[0].time!==0||Math.abs(c.keys.at(-1).time-value.spec.duration)>1e-6)throw Error('Curve duration differs from specification.');
  if(c.keys.some((k,i)=>i>0&&k.time<=c.keys[i-1].time))throw Error('Keyframe times must increase.');
  const from=c.property.endsWith('.z')?1:value.spec.scaleFrom,to=c.property.endsWith('.z')?1:value.spec.scaleTo;
  if(c.keys[0].value!==from||c.keys.at(-1).value!==to)throw Error('Curve endpoints differ from specification.');
 }
 for(const target of targets)for(const axis of ['x','y','z'])if(!bindings.has(target+':m_LocalScale.'+axis))throw Error('Missing scale axis.');
 return value;
}

function canonical(value){
 if(Array.isArray(value))return value.map(canonical);
 if(value&&typeof value==='object')return Object.fromEntries(Object.keys(value).sort().map(k=>[k,canonical(value[k])]));
 return value;
}
export function animationHash(value){return createHash('sha256').update(JSON.stringify(canonical(validateStandardAnimation(value)))).digest('hex');}

// Concrete paths belong to the consuming window, never to the standard record.
export function bindStandardAnimation(input,bindings){
 const value=validateStandardAnimation(input),curves=[];
 if(!bindings||Object.keys(bindings).some(k=>!value.targets.some(t=>t.id===k)))throw Error('Unknown animation binding.');
 const paths=new Set();
 for(const target of value.targets){
  const targets=bindings[target.id];
  if(!Array.isArray(targets)||!targets.length||(!target.multiple&&targets.length>1))throw Error('Missing target binding.');
  for(const path of targets){
   if(typeof path!=='string'||path.length>512||path.includes('\\')||path.includes(':')||/[\x00-\x1f]/.test(path)||path.split('/').some(p=>p==='..'||p==='.')||path.startsWith('/')||path.endsWith('/')&&path!=='')throw Error('Invalid relative animation path.');
   if(paths.has(path))throw Error('Duplicate target path.');
   paths.add(path);
   for(const c of value.clip.curves.filter(c=>c.target===target.id)){
    const {target:unused,...curve}=c;curves.push({path,...curve});
   }
  }
 }
 return {...value.clip,curves};
}
