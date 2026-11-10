import {validateDesign} from '../shared/design.mjs';
import {saveProject,projectSnapshot} from './save.mjs';
import { readFile } from 'node:fs/promises';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { z } from 'zod';
import { createStore } from './store.mjs';
import { createTemplateStore } from './templates.mjs';
export const resourceUri = 'ui://gamecli-ui-studio/v1.html';
export function createMcpServer(store = createStore(), templates = createTemplateStore()) {
  const server = new McpServer({ name: 'gamecli-ui-studio', version: '0.1.0' });
  server.registerResource('ui-studio', resourceUri, { mimeType: 'text/html;profile=mcp-app' }, async () => ({ contents: [{ uri: resourceUri, mimeType: 'text/html;profile=mcp-app', text: await readFile(new URL('../dist/studio.html', import.meta.url), 'utf8'), _meta: { 'openai/ui': { availableDisplayModes: ['inline','fullscreen'] }, ui: { prefersBorder: true, csp: { connectDomains: [], resourceDomains: [] } } } }] }));
  const wrap = fn => async args => { try { return { content: [], structuredContent: await fn(args) }; } catch (e) { return { isError: true, content: [{ type: 'text', text: e.message }] }; } };
  const annotations = { readOnlyHint: true, destructiveHint: false, openWorldHint: false };
  server.registerTool('gamecli_ui_templates', { description: '按分类读取数据库中的 UI 预制控件和系统模板。', inputSchema: {}, annotations }, wrap(()=>templates.list()));
  const close=server.close.bind(server);server.close=async()=>{await templates.close();await close();};
  server.registerTool('gamecli_ui_open', { description: '打开 GameCLI UI Studio 内嵌设计工作台。编辑网页布局稿，不生成 Unity 资源。', inputSchema: {}, annotations, _meta: { ui: { resourceUri }, 'openai/outputTemplate': resourceUri } }, wrap(() => projectSnapshot(store,templates)));
  server.registerTool('gamecli_ui_snapshot', { description: '读取当前 UI Studio 草稿。', inputSchema: {}, annotations }, wrap(() => projectSnapshot(store,templates)));
  server.registerTool('gamecli_ui_save', { description: '按工程保存生产 UI 到 project_ui 表，并保留本机草稿；不更新基础模板。', inputSchema: { design:z.unknown(), revision:z.number().int().nonnegative(), projectVersion:z.number().int().nonnegative().optional() }, annotations:{readOnlyHint:false,destructiveHint:false,openWorldHint:false} },wrap(args=>saveProject(store,templates,args)));
  server.registerTool('gamecli_ui_sync', { description:'显式将指定系统同步到基础模板库。', inputSchema:{design:z.unknown(),revision:z.number().int().nonnegative(),targets:z.array(z.object({rootId:z.string(),templateId:z.string(),version:z.number().int().positive()})).min(1).max(64)},annotations:{readOnlyHint:false,destructiveHint:false,openWorldHint:false}},wrap(async args=>({sync:{ok:true,templates:await templates.publish({design:validateDesign(args.design),revision:args.revision,savedAt:new Date().toISOString()},args.targets)}})));
  server.registerTool('gamecli_ui_export', { description: '将设计导出为本机 HTML 布局稿和 studio.json，不生成 visual-ui.json 或 prefab。', inputSchema: { design: z.unknown() }, annotations: { readOnlyHint: false, destructiveHint: false, openWorldHint: false } }, wrap(({ design }) => store.exportDesign(design)));
  return server;
}
