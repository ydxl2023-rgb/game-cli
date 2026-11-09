import { mkdir, readFile, writeFile, rename, open, unlink } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';
import { randomUUID } from 'node:crypto';
import { starter, validateDesign, designHtml } from '../shared/design.mjs';
export function createStore(root = fileURLToPath(new URL('../.data/', import.meta.url))) {
  const file = join(root, 'draft.json');
  async function snapshot() {
    let data;
    try { data = JSON.parse(await readFile(file, 'utf8')); validateDesign(data.design); }
    catch (e) { if (e.code !== 'ENOENT') throw e; data = { revision: 0, savedAt: null, design: starter() }; }
    return { ...data, capabilities: { persistence: 'local', unity: false, orchestrator: false }, storage: '本机草稿' };
  }
  async function save(design, revision) {
    design = validateDesign(design);
    await mkdir(root, { recursive: true });
    let lock;
    try { lock = await open(join(root, 'draft.lock'), 'wx'); }
    catch (e) { if (e.code === 'EEXIST') throw new Error('另一窗口正在保存，请稍后重试。'); throw e; }
    const temp = join(root, `${randomUUID()}.tmp`);
    try {
      const current = await snapshot();
      if (revision !== current.revision) throw new Error('草稿已被其他窗口更新。请先导出当前设计，再重新加载。');
      await writeFile(temp, JSON.stringify({ revision: revision + 1, savedAt: new Date().toISOString(), design }, null, 2));
      await rename(temp, file);
      return await snapshot();
    } finally { await unlink(temp).catch(() => {}); await lock.close(); await unlink(join(root, 'draft.lock')); }
  }
  async function exportDesign(design) {
    design = validateDesign(design);
    const id = randomUUID();
    const dir = join(root, 'exports', id);
    await mkdir(dir, { recursive: true });
    const html = designHtml(design);
    await writeFile(join(dir, `${design.name}.html`), html);
    await writeFile(join(dir, `${design.name}.studio.json`), JSON.stringify(design, null, 2));
    return { directory: dir, html, design, notice: '已导出网页布局稿和 Studio 草稿。尚未生成 visual-ui.json 或 Unity prefab。' };
  }
  return { snapshot, save, exportDesign };
}
