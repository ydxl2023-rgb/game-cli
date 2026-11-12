export class Bridge {
  embedded = window.parent !== window;
  modes = [];
  pending = new Map();
  id = 0;
  projectId=decodeURIComponent(location.pathname.match(/\/studio\/projects\/([^/]+)/)?.[1]??'');
  templateId=decodeURIComponent(location.pathname.match(/\/studio\/templates\/([^/]+)/)?.[1]??'');
  draftId=decodeURIComponent(location.pathname.match(/\/studio\/template-drafts\/([^/]+)/)?.[1]??'');
  templateLanding=location.pathname==='/studio/templates';
  get templateMode(){return this.templateLanding||!!(this.templateId||this.draftId);}
  constructor() {
    window.addEventListener('message', e => {
      if (!this.embedded || e.source !== window.parent || e.data?.jsonrpc !== '2.0') return;
      const m = e.data, entry = this.pending.get(m.id);
      if (entry) { clearTimeout(entry.timer); this.pending.delete(m.id); m.error ? entry.reject(new Error(m.error.message)) : entry.resolve(m.result); }
      if (m.method === 'ui/notifications/host-context-changed') { this.modes = m.params?.availableDisplayModes ?? this.modes; this.onContext?.(); }
    });
  }
  rpc(method, params) {
    return new Promise((resolve, reject) => {
      const id = ++this.id;
      const timer = setTimeout(() => { this.pending.delete(id); reject(new Error('Codex 宿主响应超时，请重新打开面板。')); }, 15000);
      this.pending.set(id, { resolve, reject, timer });
      window.parent.postMessage({ jsonrpc: '2.0', id, method, params }, '*');
    });
  }
  async init() {
    if(this.templateLanding)return {design:{schemaVersion:1,name:'TemplateWorkspace',width:1920,height:1080,nodes:[]},revision:0,workspace:{id:'',name:'请选择模板',role:'viewer',version:0}};
    if(this.templateMode)return this.call('snapshot');
    if (this.embedded) {
      const result = await this.rpc('ui/initialize', { appInfo: { name: 'gamecli-ui-studio', version: '0.1.0' }, appCapabilities: { availableDisplayModes: ['inline','fullscreen'] }, protocolVersion: '2026-01-26' });
      this.modes = result?.hostContext?.availableDisplayModes ?? [];
      window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/initialized', params: {} }, '*');
      this.onContext?.();
    }
    if(!this.projectId){
      const {projects}=await this.call('projects');let recent;try{recent=localStorage.getItem('gamecli.studio.lastProject');}catch{}
      this.projectId=(projects.find(p=>p.id===recent)??projects[0])?.id??'';
      if(!this.projectId)return {design:{schemaVersion:1,name:'ProjectWorkspace',width:1920,height:1080,nodes:[]},revision:0,project:{id:'',name:'尚未创建项目',role:'viewer',version:0}};
    }
    const snapshot=await this.call('snapshot');
    try{localStorage.setItem('gamecli.studio.lastProject',this.projectId);}catch{}
    if(!this.embedded&&location.pathname==='/studio/projects')window.history.replaceState(null,'','/studio/projects/'+encodeURIComponent(this.projectId));
    return snapshot;
  }
  async call(action, args = {}) {
    if(['unity_binding','bind_unity'].includes(action)){
      if(this.embedded){const result=await this.rpc('tools/call',{name:'gamecli_ui_'+action,arguments:args});if(result.isError)throw Error(result.content?.[0]?.text);return result.structuredContent;}
      const response=await fetch('/api/studio/projects/'+encodeURIComponent(args.projectId)+'/unity-binding',action==='unity_binding'?{cache:'no-store'}:{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify(args)});const result=await response.json();if(!response.ok)throw Error(result.error);return result;
    }
    if(this.templateMode&&['snapshot','save','candidate','export','revisions','restore'].includes(action)){
      if(action!=='snapshot'&&(!this.draftId||!['save','candidate'].includes(action)))throw Error('已发布模板只读；请在模板页打开草稿。');
      const path=this.draftId?'template-drafts/'+encodeURIComponent(this.draftId)+(action==='snapshot'?'':'/'+(action==='candidate'?'submit':action)):'templates/'+encodeURIComponent(this.templateId);
      const r=await fetch('/api/studio/'+path,action==='snapshot'?{cache:'no-store'}:{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify({draftId:this.draftId,design:args.design,revision:args.revision})});const result=await r.json();if(!r.ok)throw Error(result.error);return result;
    }
    const scoped=['snapshot','save','export','candidate','revisions','restore'].includes(action);
    if(scoped){if(!this.projectId)throw new Error('请先选择项目。');args={...args,projectId:this.projectId};}
    if (this.embedded) {
      const result = await this.rpc('tools/call', { name: `gamecli_ui_${action}`, arguments: args });
      if (result?.isError) throw new Error(result.content?.[0]?.text || '操作失败。');
      return result.structuredContent;
    }
    const path=scoped?`/api/studio/projects/${encodeURIComponent(this.projectId)}${action==='snapshot'?'':'/'+action}`:`/api/studio/${action}`;
    const response = await fetch(path, ['snapshot','templates','projects','revisions','animations'].includes(action) ? { cache: 'no-store' } : { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(args) });
    const result = await response.json();
    if (!response.ok) throw new Error(result.error || '服务暂不可用。');
    return result;
  }
  async expand() {
    const result = await this.rpc('ui/request-display-mode', { mode: 'fullscreen' });
    if (result?.mode !== 'fullscreen') throw new Error('当前宿主未切换到侧边工作台。');
  }
}
