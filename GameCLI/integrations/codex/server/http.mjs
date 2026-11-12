import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { pathToFileURL } from 'node:url';
import { createStore } from './store.mjs';
import { createTemplateStore } from './templates.mjs';
import {createWorkspaces} from './workspaces.mjs';
export function createHttpServer(store = createStore(), templates = createTemplateStore(),workspaces=createWorkspaces()) {
  const server = createServer(async (req, res) => {
    const send = (status, data, type = 'application/json; charset=utf-8') => { res.writeHead(status, { 'Content-Type': type, 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff' }); res.end(type.startsWith('application/json') ? JSON.stringify(data) : data); };
    try {
      if (!/^127\.0\.0\.1:\d+$/.test(req.headers.host ?? '')) return send(403, { error: '仅允许本机访问。' });
      const url = new URL(req.url, `http://${req.headers.host}`);
      if (req.method === 'GET' && ['/','/track','/studio','/studio/projects/library','/studio/templates/library','/studio/templates/review'].includes(url.pathname)) return send(200, await readFile(new URL('../dist/workspace-portal.html', import.meta.url)), 'text/html; charset=utf-8');
      if(req.method==='GET'&&['/studio/templates','/studio/projects'].includes(url.pathname))return send(200,await readFile(new URL('../dist/studio.html',import.meta.url)),'text/html; charset=utf-8');
      const templatePage=url.pathname.match(/^\/studio\/(templates|template-drafts)\/([^/]+)$/);
      if(req.method==='GET'&&templatePage){await workspaces.templateEditor[templatePage[1]==='templates'?'published':'read'](decodeURIComponent(templatePage[2]));return send(200,await readFile(new URL('../dist/studio.html',import.meta.url)),'text/html; charset=utf-8');}
      const draft=url.pathname.match(/^\/api\/studio\/template-drafts\/([^/]+)(?:\/(save|submit))?$/);
      const published=url.pathname.match(/^\/api\/studio\/templates\/([^/]+)$/);
      if(req.method==='GET'&&published)return send(200,await workspaces.templateEditor.published(decodeURIComponent(published[1])));
      if(req.method==='GET'&&draft&&!draft[2])return send(200,await workspaces.templateEditor.read(draft[1]));
      const page=url.pathname.match(/^\/studio\/projects\/([A-Za-z0-9_-]+)$/);
      if(req.method==='GET'&&page){await workspaces.access(page[1]);return send(200,await readFile(new URL('../dist/studio.html',import.meta.url)),'text/html; charset=utf-8');}
      const unityBinding=url.pathname.match(/^\/api\/studio\/projects\/([A-Za-z0-9_-]+)\/unity-binding$/);
      if(req.method==='GET'&&unityBinding)return send(200,await workspaces.unityBindings.read(unityBinding[1]));
      const composition=url.pathname.match(/^\/api\/studio\/projects\/([A-Za-z0-9_-]+)\/composition$/);
      const chat=url.pathname.match(/^\/api\/studio\/projects\/([A-Za-z0-9_-]+)\/design-chat$/);
      if(req.method==='GET'&&chat)return send(200,await workspaces.designChat.list(chat[1]));
      const scoped=url.pathname.match(/^\/api\/studio\/projects\/([A-Za-z0-9_-]+)(?:\/(save|export|revisions|restore))?$/);
      if(req.method==='GET'&&url.pathname==='/api/studio/projects')return send(200,await workspaces.list());
      if(req.method==='GET'&&scoped)return send(200,scoped[2]==='revisions'?await workspaces.revisions(scoped[1]):await workspaces.snapshot(scoped[1]));
      if(req.method==='GET'&&url.pathname==='/api/studio/admin/candidates')return send(200,await workspaces.candidates({summary:url.searchParams.get('summary')==='1'}));
      if(req.method==='GET'&&url.pathname==='/api/studio/admin/candidate-detail')return send(200,await workspaces.candidateDetail(url.searchParams.get('id')));
      if(req.method==='GET'&&url.pathname==='/api/studio/template-versions')return send(200,await workspaces.templateVersions(url.searchParams.get('id')));
      if (req.method === 'GET' && url.pathname === '/api/studio') return send(403,{error:'请选择项目，再读取 /api/studio/projects/<projectId>。'});
      if(req.method==='GET'&&url.pathname==='/api/studio/animations')return send(200,await workspaces.animations());
      if (req.method === 'GET' && url.pathname === '/api/studio/templates') return send(200, await templates.list());
      if(req.method==='GET' && /^\/api\/studio\/assets\/[a-f0-9]{64}$/.test(url.pathname)) {
        if(!await workspaces.assetAllowed(url.pathname.split('/').at(-1),url.searchParams.get('projectId')))return send(403,{error:'无权读取该资源。'});
        const asset=await templates.asset(url.pathname.split('/').at(-1));
        if(!asset)return send(404,{error:'资源不存在。'});
        return send(200,asset.body,asset.mime);
      }
      if (req.method === 'POST' && url.pathname.startsWith('/api/studio/')) {
        if (req.headers.origin && req.headers.origin !== url.origin) return send(403, { error: '不允许跨站写入。' });
        if (!req.headers['content-type']?.startsWith('application/json')) return send(415, { error: '需要 JSON。' });
        const chunks = []; let length = 0;
        for await (const chunk of req) { length += chunk.length; if (length > 16_000_000) return send(413, { error: '请求过大。' }); chunks.push(chunk); }
        const body = JSON.parse(Buffer.concat(chunks).toString('utf8'));
        if(unityBinding)return send(200,await workspaces.unityBindings.bind(unityBinding[1],body));
        if(url.pathname==='/api/studio/template-drafts')return send(200,await workspaces.templateEditor.open(body));
        if(draft&&draft[2])return send(200,await workspaces.templateEditor[draft[2]](draft[1],body));
        if(composition){if(body.command!=='apply'||body.projectId!==composition[1])throw Error('编排操作或项目无效。');return send(200,await workspaces.composition.apply(composition[1],body));}
        if(chat){if(!['send','stop','apply'].includes(body.command)||body.projectId!==chat[1])throw Error('设计操作或项目无效。');return send(200,await workspaces.designChat[body.command](chat[1],body));}
        if(url.pathname==='/api/studio/projects')return send(200,await workspaces.create(body));
        if(url.pathname==='/api/studio/admin/publish')return send(200,await workspaces.publish(body));
        if(url.pathname==='/api/studio/admin/reject')return send(200,await workspaces.reject(body));
        if(scoped){const action={save:'save',export:'exportDesign',restore:'restore'}[scoped[2]];if(action)return send(200,await workspaces[action](scoped[1],body));}
        return send(403,{error:'旧写入入口已关闭。请进入指定项目工作区；标准模板只能经候选审核发布。'});
      }
      send(404, { error: '接口不存在。' });
    } catch (e) { send(400, { error: e.message }); }
  });
  server.on('close',()=>{void templates.close();void workspaces.close();});
  return server;
}
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const port = Number(process.env.GAMECLI_STUDIO_PORT || 18091);
  createHttpServer().listen(port, '127.0.0.1', () => console.log(`GameCLI UI Studio: http://127.0.0.1:${port}/studio`));
}
