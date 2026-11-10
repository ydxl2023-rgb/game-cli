export function hierarchyVisibility(design, hidden = [], runtime = false) {
  const nodes = new Map(design.nodes.map(n => [n.id, n]));
  const excluded = new Set(hidden), result = new Map();
  const visible = n => {
    if (result.has(n.id)) return result.get(n.id);
    const parent = nodes.get(n.parent);
    const value = (runtime || n.active !== false) && !excluded.has(n.id) && (!parent || visible(parent));
    result.set(n.id, value);
    return value;
  };
  for (const n of design.nodes) visible(n);
  return result;
}

export function defaultAction(design, node) {
  if (node.action) return node.action;
  let parent = design.nodes.find(n => n.id === node.parent);
  while (parent) {
    if (parent.template === 'ConfirmDialog' && (node.type === 'Button' || node.id === parent.id + 'Mask')) return {type:'hide',target:parent.id};
    parent = design.nodes.find(n => n.id === parent.parent);
  }
  return null;
}
export function createPreviewState(design) {
  return {hidden:design.nodes.filter(n=>n.active === false || (n.previewHidden ?? n.template === 'ConfirmDialog')).map(n=>n.id),history:[]};
}
export function runPreviewAction(design, state, action) {
  if (!action || action.type === 'none') return state;
  if (action.type === 'back') {
    if (!state.history.length) return state;
    return {hidden:state.history.at(-1),history:state.history.slice(0,-1)};
  }
  const target=design.nodes.find(n=>n.id===action.target);
  if (!target) return state;
  const hidden=new Set(state.hidden);
  if (action.type==='hide' || (action.type==='toggle' && !hidden.has(target.id))) hidden.add(target.id);
  else {
    if (action.type==='navigate') for (const n of design.nodes) if (n.parent===target.parent && n.type==='Container' && n.id!==target.id) hidden.add(n.id);
    let current=target;
    while(current) { hidden.delete(current.id); current=design.nodes.find(n=>n.id===current.parent); }
  }
  return {hidden:[...hidden],history:[...state.history,state.hidden].slice(-50)};
}
