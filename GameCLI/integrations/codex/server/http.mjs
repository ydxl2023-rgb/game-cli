import {validateDesign} from '../shared/design.mjs';
import {saveProject,projectSnapshot} from './save.mjs';
import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { pathToFileURL } from 'node:url';
import { createStore } from './store.mjs';
import { createTemplateStore } from './templates.mjs';
export function createHttpServer(store = createStore(), templates = createTemplateStore()) {
  const server = createServer(async (req, res) => {
    const send = (status, data, type = 'application/json; charset=utf-8') => { res.writeHead(status, { 'Content-Type': type, 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff' }); res.end(type.startsWith('application/json') ? JSON.stringify(data) : data); };
    try {
      if (!/^127\.0\.0\.1:\d+$/.test(req.headers.host ?? '')) return send(403, { error: '仅允许本机访问。' });
      const url = new URL(req.url, `http://${req.headers.host}`);
      if (req.method === 'GET' && ['/','/track','/studio'].includes(url.pathname)) return send(200, await readFile(new URL('../dist/studio.html', import.meta.url)), 'text/html; charset=utf-8');
      if (req.method === 'GET' && url.pathname === '/api/studio') return send(200, await projectSnapshot(store,templates));
      if (req.method === 'GET' && url.pathname === '/api/studio/templates') return send(200, await templates.list());
      if(req.method==='GET' && /^\/api\/studio\/assets\/[a-f0-9]{64}$/.test(url.pathname)) {
        const asset=await templates.asset(url.pathname.split('/').at(-1));
        if(!asset)return send(404,{error:'资源不存在。'});
        return send(200,asset.body,asset.mime);
      }
      if (req.method === 'POST' && ['/api/studio/save','/api/studio/export','/api/studio/sync'].includes(url.pathname)) {
        if (req.headers.origin && req.headers.origin !== url.origin) return send(403, { error: '不允许跨站写入。' });
        if (!req.headers['content-type']?.startsWith('application/json')) return send(415, { error: '需要 JSON。' });
        const chunks = []; let length = 0;
        for await (const chunk of req) { length += chunk.length; if (length > 1_000_000) return send(413, { error: '请求过大。' }); chunks.push(chunk); }
        const body = JSON.parse(Buffer.concat(chunks).toString('utf8'));
        if(url.pathname.endsWith('sync')){if(!Array.isArray(body.targets)||!body.targets.length)throw new Error('请选择已关联数据库模板的系统根节点，再同步。');return send(200,{sync:{ok:true,templates:await templates.publish({design:validateDesign(body.design),revision:body.revision,savedAt:new Date().toISOString()},body.targets)}});}
        return send(200, url.pathname.endsWith('save') ? await saveProject(store,templates,body) : await store.exportDesign(body.design));
      }
      send(404, { error: '接口不存在。' });
    } catch (e) { send(400, { error: e.message }); }
  });
  server.on('close',()=>void templates.close());
  return server;
}
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const port = Number(process.env.GAMECLI_STUDIO_PORT || 18091);
  createHttpServer().listen(port, '127.0.0.1', () => console.log(`GameCLI UI Studio: http://127.0.0.1:${port}/studio`));
}
