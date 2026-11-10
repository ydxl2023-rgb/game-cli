import {animationTime,sampleCurve} from '../shared/animations.mjs';
export function playAnimations(design,canvas) {
  const nodes=new Map(design.nodes.map(n=>[n.id,n]));
  const elements=new Map([...canvas.querySelectorAll('[data-node]')].map(e=>[e.dataset.node,e]));
  const originals=new Map(),start=performance.now();let frame;
  for(const a of design.animations??[])for(const c of a.clip.curves){const e=elements.get(c.targetNodeId);if(e&&!originals.has(e))originals.set(e,{transform:e.style.transform,backgroundPosition:e.style.backgroundPosition});}
  function tick(now){
    for(const a of design.animations??[]){const time=animationTime(a,(now-start)/1000);
      for(const c of a.clip.curves){const el=elements.get(c.targetNodeId),n=nodes.get(c.targetNodeId);if(!el||!n)continue;
        const value=sampleCurve(c.keys,time);
        if(c.property==='localEulerAnglesRaw.z'){const s=n.localScale??{x:1,y:1};el.style.transform=`rotate(${-value}deg) scale(${s.x},${s.y})`;}
        if(c.property==='m_UVRect.x'){const u=n.uvRect;el.style.backgroundPosition=`${-value*n.width/u.width}px ${(u.y+u.height-1)*n.height/u.height}px`;}
      }
    }
    frame=requestAnimationFrame(tick);
  }
  if(originals.size)tick(start);
  return ()=>{cancelAnimationFrame(frame);for(const [el,style] of originals)Object.assign(el.style,style);};
}
