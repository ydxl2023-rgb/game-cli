import {alignedDiff,changedSpan} from '../shared/line-diff.mjs';
const el=(tag,text)=>{const e=document.createElement(tag);if(text!==undefined)e.textContent=text;return e;};
function stable(value){if(Array.isArray(value))return value.map(stable);if(value&&typeof value==='object')return Object.fromEntries(Object.keys(value).sort().map(k=>[k,stable(value[k])]));return value;}
export function mountCandidateDiff(host,data){
 const oldFiles=new Map((data.current_scripts?.files??[]).map(f=>[f.path,f.content])),newFiles=new Map((data.scripts?.files??[]).map(f=>[f.path,f.content]));
 const files=[{name:'布局参数.json',before:JSON.stringify(stable(data.current_payload),null,2),after:JSON.stringify(stable(data.payload),null,2)},...[...new Set([...oldFiles.keys(),...newFiles.keys()])].sort().map(name=>({name,before:oldFiles.get(name)??'',after:newFiles.get(name)??''}))];
 const toolbar=el('div');toolbar.className='diff-toolbar';const select=el('select');select.setAttribute('aria-label','选择对比文件');files.forEach((f,i)=>select.add(new Option((f.before===f.after?'＝ ':'● ')+f.name,String(i))));
 const previous=el('button','上一处差异'),next=el('button','下一处差异'),count=el('span');toolbar.append(select,previous,next,count);
 const legend=el('p','红色：删除　绿色：新增　黄色：修改（深色标记变化字符）');legend.className='diff-legend';
 const viewport=el('div');viewport.className='diff-scroll';viewport.tabIndex=0;viewport.setAttribute('aria-label','左右同步差异对比');
 const board=el('div');board.className='diff-board';const header=el('div');header.className='diff-header';header.append(el('strong','当前发布版本'),el('strong','候选版本'));
 const body=el('div');body.className='diff-lines';board.append(header,body);viewport.append(board);host.append(toolbar,legend,viewport);
 let rows=[],changes=[],index=-1,frame=0;
 function draw(){frame=0;header.style.width=viewport.clientWidth+'px';header.style.transform=`translateX(${viewport.scrollLeft}px)`;const start=Math.max(0,Math.floor((viewport.scrollTop-30)/24)-8),end=Math.min(rows.length,start+Math.ceil(viewport.clientHeight/24)+18),fragment=document.createDocumentFragment();
  for(let i=start;i<end;i++){const row=rows[i],line=el('div');line.className='diff-line '+row.kind;line.style.top=i*24+'px';line.style.left=viewport.scrollLeft+'px';line.style.width=viewport.clientWidth+'px';const span=row.kind==='changed'?changedSpan(row.before,row.after):null;
   for(const [side,text,num]of [['left',row.before,row.left],['right',row.after,row.right]]){const cell=el('div');cell.className='diff-cell '+side+(num===null?' empty':'');cell.append(el('span',num===null?'':String(num)));const code=el('code');if(span){code.append(document.createTextNode(text.slice(0,span.start)),el('mark',text.slice(span.start,side==='left'?span.leftEnd:span.rightEnd)),document.createTextNode(text.slice(side==='left'?span.leftEnd:span.rightEnd)));}else code.textContent=text;const textBox=el('div');textBox.className='diff-text';code.style.transform=`translateX(${-viewport.scrollLeft}px)`;textBox.append(code);cell.append(textBox);line.append(cell);}fragment.append(line);
  }body.replaceChildren(fragment);
 }
 function choose(){const f=files[Number(select.value)];rows=alignedDiff(f.before,f.after);changes=[];rows.forEach((r,i)=>{if(r.kind!=='same'&&(i===0||rows[i-1].kind==='same'))changes.push(i);});index=-1;count.textContent=changes.length?changes.length+' 处差异':'内容一致';previous.disabled=next.disabled=!changes.length;body.style.height=rows.length*24+'px';let width=0;for(const r of rows)width=Math.max(width,r.before.length,r.after.length);board.style.minWidth=Math.max(viewport.clientWidth,viewport.clientWidth/2+width*8+70)+'px';viewport.scrollTop=0;viewport.scrollLeft=0;draw();}
 function jump(delta){if(!changes.length)return;index=(index+delta+changes.length)%changes.length;viewport.scrollTop=changes[index]*24;count.textContent=(index+1)+' / '+changes.length+' 处差异';draw();}
 previous.onclick=()=>jump(index<0?0:-1);next.onclick=()=>jump(1);select.onchange=choose;viewport.onscroll=()=>{if(!frame)frame=requestAnimationFrame(draw);};choose();
}
