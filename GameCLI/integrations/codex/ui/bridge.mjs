export class Bridge {
  embedded = window.parent !== window;
  modes = [];
  pending = new Map();
  id = 0;
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
    return this.call('snapshot');
  }
  async call(action, args = {}) {
    if (this.embedded) {
      const result = await this.rpc('tools/call', { name: `gamecli_ui_${action}`, arguments: args });
      if (result?.isError) throw new Error(result.content?.[0]?.text || '操作失败。');
      return result.structuredContent;
    }
    const response = await fetch(action === 'snapshot' ? '/api/studio' : `/api/studio/${action}`, ['snapshot','templates'].includes(action) ? { cache: 'no-store' } : { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(args) });
    const result = await response.json();
    if (!response.ok) throw new Error(result.error || '服务暂不可用。');
    return result;
  }
  async expand() {
    const result = await this.rpc('ui/request-display-mode', { mode: 'fullscreen' });
    if (result?.mode !== 'fullscreen') throw new Error('当前宿主未切换到侧边工作台。');
  }
}
