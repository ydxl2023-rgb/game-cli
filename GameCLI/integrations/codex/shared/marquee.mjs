export function selectionRect(start,end){
  return {x:Math.min(start.x,end.x),y:Math.min(start.y,end.y),width:Math.abs(end.x-start.x),height:Math.abs(end.y-start.y)};
}
export function intersectsSelection(rect,card){
  return rect.width>0&&rect.height>0&&rect.x<card.x+card.width&&rect.x+rect.width>card.x&&rect.y<card.y+card.height&&rect.y+rect.height>card.y;
}
