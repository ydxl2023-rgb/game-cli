// Load source fonts once for project, candidate and template previews.
const pending=new Map();
export function loadDesignFonts(design){
  for(const id of new Set(design.nodes.map(n=>n.fontAsset).filter(Boolean))){
    if(!/^[a-f0-9]{64}$/.test(id)||pending.has(id))continue;
    const face=new FontFace('GameCLI'+id,`url(/api/studio/assets/${id})`);
    document.fonts.add(face);
    pending.set(id,face.load().catch(error=>{console.error('Template font failed to load',id,error);return null;}));
  }
}
