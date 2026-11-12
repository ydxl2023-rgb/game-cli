import {animationTime,sampleCurve} from '../shared/animations.mjs';
export function playAnimations(design,canvas) {
  const nodes=new Map(design.nodes.map(n=>[n.id,n]));
  const elements=new Map([...canvas.querySelectorAll('[data-node]')].map(e=>[e.dataset.node,e]));
  const originals=new Map();let start=performance.now(),frame,pausedAt=null,stopped=false;
  for(const a of design.animations??[])for(const c of a.clip.curves){const e=elements.get(c.targetNodeId);if(e&&!originals.has(e))originals.set(e,{transform:e.style.transform,backgroundPosition:e.style.backgroundPosition});}
  function tick(now){
    if(stopped||pausedAt!==null)return;
    const transforms=new Map();
    for(const a of design.animations??[]){const time=animationTime(a,(now-start)/1000);
      for(const c of a.clip.curves){const el=elements.get(c.targetNodeId),n=nodes.get(c.targetNodeId);if(!el||!n)continue;
        const value=sampleCurve(c.keys,time);
        if(c.property==='localEulerAnglesRaw.z'||/^m_LocalScale\.[xy]$/.test(c.property)){
          if(!transforms.has(el))transforms.set(el,{rotation:n.rotation??0,...(n.localScale??{x:1,y:1})});
          const transform=transforms.get(el);if(c.property==='localEulerAnglesRaw.z')transform.rotation=value;else transform[c.property.at(-1)]=value;
        }
        if(c.property==='m_UVRect.x'){const u=n.uvRect;el.style.backgroundPosition=`${-value*n.width/u.width}px ${(u.y+u.height-1)*n.height/u.height}px`;}
      }
    }
    for(const [el,t] of transforms)el.style.transform=`rotate(${-t.rotation}deg) scale(${t.x},${t.y})`;
    frame=requestAnimationFrame(tick);
  }
  if(originals.size)tick(start);
  const stop=()=>{stopped=true;cancelAnimationFrame(frame);for(const [el,style] of originals)Object.assign(el.style,style);};
  stop.pause=()=>{if(stopped||pausedAt!==null)return;pausedAt=performance.now();cancelAnimationFrame(frame);};
  stop.resume=()=>{if(stopped||pausedAt===null)return;const now=performance.now();start+=now-pausedAt;pausedAt=null;if(originals.size)tick(now);};
  return stop;
}
