// Asset paths for the mall shell and the three independently loaded Shop widgets.
export function shopWidgetPath(name) {
  const folder = name === 'Shop' ? 'Shop' : 'Shop/' + name;
  return `Assets/Data/Widgets/${folder}/${name}.prefab`;
}
