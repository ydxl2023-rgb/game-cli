export class Bridge {
  embedded = window.parent !== window;
  modes = [];
  pending = new Map();
  id = 0;
  projectId=decodeURIComponent(location.pathname.match(/\/studio\/projects\/([^/]+)/)?.[1]??'');
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
    if (this.embedded) {
      const result = await this.rpc('ui/initialize', { appInfo: { name: 'gamecli-ui-studio', version: '0.1.0' }, appCapabilities: { availableDisplayModes: ['inline','fullscreen'] }, protocolVersion: '2026-01-26' });
      this.modes = result?.hostContext?.availableDisplayModes ?? [];
      window.parent.postMessage({ jsonrpc: '2.0', method: 'ui/notifications/initialized', params: {} }, '*');
      this.onContext?.();
    }
    if(!this.projectId){const result=await this.call('projects');this.projectId=await new Promise(resolve=>{const dialog=document.createElement('dialog'),title=document.createElement('h2');dialog.className='sync-confirm';title.textContent='选择项目工作区';dialog.append(title);for(const project of result.projects){const button=document.createElement('button');button.textContent=project.name+' · '+project.id;button.onclick=()=>{dialog.close();dialog.remove();resolve(project.id);};dialog.append(button);}if(!result.projects.length){const p=document.createElement('p');p.textContent='请先在本机 /studio 页面创建项目。';dialog.append(p);}dialog.oncancel=e=>e.preventDefault();document.body.append(dialog);dialog.showModal();});}
    return this.call('snapshot');
  }
  async call(action, args = {}) {
    const scoped=['snapshot','save','export','candidate','revisions','restore'].includes(action);
    if(scoped){if(!this.projectId)throw new Error('请先选择项目。');args={...args,projectId:this.projectId};}
    if (this.embedded) {
      const result = await this.rpc('tools/call', { name: `gamecli_ui_${action}`, arguments: args });
      if (result?.isError) throw new Error(result.content?.[0]?.text || '操作失败。');
      return result.structuredContent;
    }
    const path=scoped?`/api/studio/projects/${encodeURIComponent(this.projectId)}${action==='snapshot'?'':'/'+action}`:`/api/studio/${action}`;
    const response = await fetch(path, ['snapshot','templates','projects','revisions'].includes(action) ? { cache: 'no-store' } : { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(args) });
    const result = await response.json();
    if (!response.ok) throw new Error(result.error || '服务暂不可用。');
    return result;
  }
  async expand() {
    const result = await this.rpc('ui/request-display-mode', { mode: 'fullscreen' });
    if (result?.mode !== 'fullscreen') throw new Error('当前宿主未切换到侧边工作台。');
  }
}
