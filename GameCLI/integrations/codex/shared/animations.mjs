// V1 playback supports unweighted Euler Z and RawImage UV X curves.
// Zero X/Y Euler curves are retained for faithful Unity export.
export function validateAnimations(value, nodes) {
  if(value===undefined)return undefined;
  const fail=()=>{throw new Error('动画数据无效或包含尚未支持的曲线。');};
  if(!Array.isArray(value)||value.length>64)fail();
  const map=new Map(nodes.map(n=>[n.id,n])),ids=new Set(),bindings=new Set();
  for(const a of value) {
    if(!a || typeof a.id!=='string'||!a.id||ids.has(a.id)||!map.get(a.hostNodeId)?.animationHost)fail();ids.add(a.id);
    const c=a.clip,s=a.state;
    if(!c||!s||!Number.isFinite(c.startTime)||!Number.isFinite(c.stopTime)||c.stopTime<=c.startTime||!Number.isFinite(c.frameRate)||c.frameRate<=0||typeof c.loopTime!=='boolean'||!Number.isFinite(s.speed)||s.speed<=0||!Number.isFinite(s.cycleOffset)||s.default!==true)fail();
    if(!Array.isArray(c.curves)||!c.curves.length||c.curves.length>128)fail();
    for(const curve of c.curves) {
      let n=map.get(curve.targetNodeId);if(!n)fail();
      if(curve.component==='UnityEngine.UI.RawImage'&&(!n.image||!n.uvRect))fail();
      let count=0;while(n&&n.id!==a.hostNodeId&&count++<=nodes.length)n=map.get(n.parent);
      if(n?.id!==a.hostNodeId)fail();
      const prop=curve.property, euler=curve.component==='UnityEngine.Transform'&&/^localEulerAnglesRaw\.[xyz]$/.test(prop);
      if(!euler&&!(curve.component==='UnityEngine.UI.RawImage'&&prop==='m_UVRect.x'))fail();
      const binding=curve.targetNodeId+':'+prop;if(bindings.has(binding))fail();bindings.add(binding);
      if(!Array.isArray(curve.keys)||!curve.keys.length||curve.keys.length>4096)fail();
      let last=-Infinity;
      for(const k of curve.keys) {
        if(!['time','value','inTangent','outTangent','inWeight','outWeight'].every(f=>Number.isFinite(k[f]))||k.weightedMode!==0||k.time<=last||k.time<c.startTime||k.time>c.stopTime)fail();
        if(euler&&!prop.endsWith('.z')&&(k.value!==0||k.inTangent!==0||k.outTangent!==0))fail();last=k.time;
      }
      if(curve.keys[0].time!==c.startTime||last!==c.stopTime)fail();
    }
  }
  return structuredClone(value);
}
export function sampleCurve(keys,time) {
  if(time<=keys[0].time)return keys[0].value;
  for(let i=1;i<keys.length;i++)if(time<=keys[i].time){
    const a=keys[i-1],b=keys[i],d=b.time-a.time,t=(time-a.time)/d;
    return (2*t**3-3*t*t+1)*a.value+(t**3-2*t*t+t)*d*a.outTangent+(-2*t**3+3*t*t)*b.value+(t**3-t*t)*d*b.inTangent;
  }
  return keys.at(-1).value;
}
export function animationTime(animation,seconds) {
  const {clip,state}=animation,d=clip.stopTime-clip.startTime;
  const elapsed=seconds*state.speed+state.cycleOffset*d;
  return clip.startTime+(clip.loopTime?((elapsed%d)+d)%d:Math.max(0,Math.min(d,elapsed)));
}
export function remapAnimations(animations,remap,prefix) {
  return (animations??[]).map(a=>({...structuredClone(a),id:prefix+a.id,hostNodeId:remap.get(a.hostNodeId),clip:{...structuredClone(a.clip),curves:a.clip.curves.map(c=>({...structuredClone(c),targetNodeId:remap.get(c.targetNodeId)}))}}));
}
