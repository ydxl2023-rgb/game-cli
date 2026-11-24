// Copy SV artwork referenced by the exported product table.
import {copyFile, mkdir, readFile, readdir} from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const source = process.env.SV_PROJECT ?? 'G:/Works/SV/Development/Project';
const assets = fileURLToPath(new URL('../../../examples/unitys/development/client/Assets/', import.meta.url));
const table = JSON.parse((await readFile(path.join(assets, 'Data/Jsons/Excels/ShopPageItemConfig.bytes'), 'utf8')).replace(/^\uFEFF/, ''));
const names = new Set(Object.values(table.data).flatMap(row => [row.Icon, row.Preview, row.Currency]).filter(Boolean));
const files = new Map();

async function index(directory) {
    for (const entry of await readdir(directory, {withFileTypes: true})) {
        const file = path.join(directory, entry.name);
        if (entry.isDirectory()) await index(file);
        else if (names.has(entry.name.toLowerCase())) files.set(entry.name.toLowerCase(), file);
    }
}

await index(path.join(source, 'Assets/data/UI'));
await index(path.join(source, 'Assets/data/Language/Cn/UI'));
for (const name of names) {
    if (!files.has(name)) throw new Error('Missing configured shop artwork: ' + name);
}
for (const name of names) {
    const directory = path.join(assets, name.startsWith('wallpaper_') ? 'Data/Widgets/Shop/Demo/Images' : 'Data/Widgets/Common/Icons');
    await mkdir(directory, {recursive: true});
    await copyFile(files.get(name), path.join(directory, name));
}
