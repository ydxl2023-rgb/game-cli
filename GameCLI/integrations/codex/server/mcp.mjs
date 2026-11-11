import {readFile} from 'node:fs/promises';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';
import {z} from 'zod';
import {createStore} from './store.mjs';
import {createTemplateStore} from './templates.mjs';
import {createWorkspaces} from './workspaces.mjs';
export const resourceUri='ui://gamecli-ui-studio/v1.html';
export function createMcpServer(store=createStore(),templates=createTemplateStore(),workspaces=createWorkspaces()){
 const server=new McpServer({name:'gamecli-ui-studio',version:'0.2.0'});
 server.registerResource('ui-studio',resourceUri,{mimeType:'text/html;profile=mcp-app'},async()=>({contents:[{uri:resourceUri,mimeType:'text/html;profile=mcp-app',text:await readFile(new URL('../dist/studio.html',import.meta.url),'utf8'),_meta:{'openai/ui':{availableDisplayModes:['inline','fullscreen']},ui:{prefersBorder:true,csp:{connectDomains:[],resourceDomains:[]}}}}]}));
 const wrap=fn=>async args=>{try{return {content:[],structuredContent:await fn(args)};}catch(e){return {isError:true,content:[{type:'text',text:e.message}]};}};
 const read={readOnlyHint:true,destructiveHint:false,openWorldHint:false},write={...read,readOnlyHint:false};
 const projectId=z.string().min(1),body={projectId,design:z.unknown(),revision:z.number().int().nonnegative(),projectVersion:z.number().int().nonnegative()};
 server.registerTool('gamecli_ui_open',{description:'打开项目工作台，先选择项目。',inputSchema:{},annotations:read,_meta:{ui:{resourceUri},'openai/outputTemplate':resourceUri}},wrap(()=>workspaces.list()));
 server.registerTool('gamecli_ui_projects',{description:'列出当前身份可访问的项目。',inputSchema:{},annotations:read},wrap(()=>workspaces.list()));
 server.registerTool('gamecli_ui_snapshot',{description:'读取指定项目，禁止隐式当前项目。',inputSchema:{projectId},annotations:read},wrap(a=>workspaces.snapshot(a.projectId)));
 server.registerTool('gamecli_ui_animations',{description:'读取最新已发布的标准动画，只读。',inputSchema:{},annotations:read},wrap(()=>workspaces.animations()));
 server.registerTool('gamecli_ui_templates',{description:'读取标准模板及当前发布版本，只读。',inputSchema:{},annotations:read},wrap(()=>templates.list()));
 server.registerTool('gamecli_ui_save',{description:'只保存指定项目，不修改标准模板。',inputSchema:body,annotations:write},wrap(a=>workspaces.save(a.projectId,a)));
 server.registerTool('gamecli_ui_export',{description:'导出指定项目到独立项目目录。',inputSchema:{projectId,design:z.unknown()},annotations:write},wrap(a=>workspaces.exportDesign(a.projectId,a)));
 server.registerTool('gamecli_ui_candidate',{description:'提交模板候选，不发布、不覆盖标准库。',inputSchema:{...body,rootId:z.string(),templateId:z.string(),version:z.number().int()},annotations:write},wrap(a=>workspaces.candidate(a.projectId,a)));
 server.registerTool('gamecli_ui_revisions',{description:'读取指定项目历史版本。',inputSchema:{projectId},annotations:read},wrap(a=>workspaces.revisions(a.projectId)));
 server.registerTool('gamecli_ui_restore',{description:'将历史版本恢复为指定项目的新版本。',inputSchema:{projectId,version:z.number().int(),revision:z.number().int(),projectVersion:z.number().int()},annotations:write},wrap(a=>workspaces.restore(a.projectId,a)));
 const close=server.close.bind(server);server.close=async()=>{await templates.close();await workspaces.close();await close();};return server;
}
