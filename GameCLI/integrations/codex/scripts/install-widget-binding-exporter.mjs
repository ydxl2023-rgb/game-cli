import {readFile,writeFile,copyFile} from 'node:fs/promises';
import {resolve,join} from 'node:path';
import {fileURLToPath} from 'node:url';

// Explicit setup only. Verification never installs code or edits prefab assets.
const directory=process.argv[2];
if(!directory)throw Error('Usage: node scripts/install-widget-binding-exporter.mjs <com.oathx.unitycli directory>');
const packageRoot=resolve(directory),manifest=JSON.parse(await readFile(join(packageRoot,'package.json'),'utf8'));
if(manifest.name!=='com.oathx.unitycli')throw Error('Expected the com.oathx.unitycli package.');
const exporter=join(packageRoot,'Editor','Services','UnityCliPrefabExportService.cs');
let source=await readFile(exporter,'utf8');
const hook='["widgetBindings"] = UnityCliWidgetBindingEvidence.Export(root, ids, path),';
source=source.replace('["widgetBindings"] = UnityCliWidgetBindingEvidence.Export(root, ids),',hook);
const anchor='["resources"] = resources,';
if(!source.includes(hook)){
 if(source.split(anchor).length!==2)throw Error('Unsupported exporter version; install the hook manually after review.');
 source=source.replace(anchor,anchor+'\n                    '+hook);
}
await copyFile(fileURLToPath(new URL('../unity/UnityCLI~/UnityCliWidgetBindingEvidence.cs',import.meta.url)),join(packageRoot,'Editor','Services','UnityCliWidgetBindingEvidence.cs'));
await writeFile(exporter,source);
console.log('Installed read-only binding evidence exporter. Compile the consuming Unity project.');
