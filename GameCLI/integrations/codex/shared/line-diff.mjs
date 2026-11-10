// Bounded line alignment: unique anchors split large files; small gaps use LCS.
export function alignedDiff(before,after){
 const a=before.replaceAll('\r\n','\n').split('\n'),b=after.replaceAll('\r\n','\n').split('\n'),ops=[];
 function gap(x,y,u,v){
  while(x<y&&u<v&&a[x]===b[u])ops.push([x++,u++]);
  let tail=0;while(x<y-tail&&u<v-tail&&a[y-1-tail]===b[v-1-tail])tail++;
  y-=tail;v-=tail;
  if((y-x)*(v-u)<=40000){
   const width=v-u+1,dp=new Uint32Array((y-x+1)*width);
   for(let i=y-1;i>=x;i--)for(let j=v-1;j>=u;j--)dp[(i-x)*width+j-u]=a[i]===b[j]?1+dp[(i-x+1)*width+j-u+1]:Math.max(dp[(i-x+1)*width+j-u],dp[(i-x)*width+j-u+1]);
   let i=x,j=u;while(i<y||j<v){if(i<y&&j<v&&a[i]===b[j])ops.push([i++,j++]);else if(i<y&&(j===v||dp[(i-x+1)*width+j-u]>=dp[(i-x)*width+j-u+1]))ops.push([i++,null]);else ops.push([null,j++]);}
  }else{
   const unique=(lines,start,end)=>{const m=new Map();for(let i=start;i<end;i++)m.set(lines[i],m.has(lines[i])?-1:i);return m;};
   const left=unique(a,x,y),right=unique(b,u,v),pairs=[];
   for(const [text,i]of left)if(i>=0&&right.get(text)>=0)pairs.push([i,right.get(text)]);
   const tails=[],previous=[];for(let i=0;i<pairs.length;i++){let lo=0,hi=tails.length;while(lo<hi){const m=(lo+hi)>>1;if(pairs[tails[m]][1]<pairs[i][1])lo=m+1;else hi=m;}previous[i]=lo?tails[lo-1]:-1;tails[lo]=i;}
   const anchors=[];for(let i=tails.at(-1);i!==undefined&&i>=0;i=previous[i])anchors.push(pairs[i]);anchors.reverse();
   if(!anchors.length){for(let i=x;i<y;i++)ops.push([i,null]);for(let j=u;j<v;j++)ops.push([null,j]);}
   else{let i=x,j=u;for(const [ai,bi]of anchors){gap(i,ai,j,bi);ops.push([ai,bi]);i=ai+1;j=bi+1;}gap(i,y,j,v);}
  }
  for(let i=0;i<tail;i++)ops.push([y+i,v+i]);
 }
 gap(0,a.length,0,b.length);
 const rows=[];let removed=[],added=[];
 const flush=()=>{for(let i=0;i<Math.max(removed.length,added.length);i++){const l=removed[i],r=added[i];rows.push({left:l===undefined?null:l+1,right:r===undefined?null:r+1,before:l===undefined?'':a[l],after:r===undefined?'':b[r],kind:l===undefined?'added':r===undefined?'removed':'changed'});}removed=[];added=[];};
 for(const [l,r]of ops){if(l!==null&&r!==null){flush();rows.push({left:l+1,right:r+1,before:a[l],after:b[r],kind:'same'});}else if(l!==null)removed.push(l);else added.push(r);}flush();return rows;
}
export function changedSpan(a,b){let start=0;while(start<a.length&&start<b.length&&a[start]===b[start])start++;let end=0;while(end<a.length-start&&end<b.length-start&&a[a.length-end-1]===b[b.length-end-1])end++;return {start,leftEnd:a.length-end,rightEnd:b.length-end};}
