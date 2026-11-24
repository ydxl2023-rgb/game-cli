# Miniwow Home 独立验收

2026-11-24，产品 Home-1，最终 Studio v5；项目 p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3，Unity 身份 83c29c9f-21c1-4597-9691-6707ca146fa7。已独立调用绑定工具确认 verified。范围以 DELIVERY.md 的产品适配为准，不将 SV 全部运营入口强加到本次营地页面。

总体 **incomplete**：已测编辑器功能通过；完整自动验收仍有工具能力缺口和一项人工复核的代码规则误报。不能描述为所有自动检查通过。

## 实测通过

- 从 App 正常 Play：Home.unity 为活动场景，Home 场景加载，UIHome 实例 1，营地可见、地图隐藏、Launch 隐藏。
- 两个按钮通过 EventSystem 实际射线第一目标核对后发送 PointerDown/Up/Click；MapClick 打开地图，ReturnClick 返回营地。非直接赋值 VM。最终 v5 再次复验。
- CloseHome / OpenHome / RapidRestart 使用真实模块生命周期。关闭后 Home 场景卸载、UI inactive；快速多次停用激活后恢复一个 Home UI 与一个加载场景，无残留可见页面。关闭瞬间 Destroy 延迟一帧，因此即时实例数 1 不等于残留。
- 重开自动动画实测 alpha 0.125 → 0.459 → 1，未手动采样动画；最终布局稳定。首次启动记录了完成状态，未捕获其淡入中间帧。
- 1080×1920 和 1080×2400 实际截图可读，角色与按钮无裁切，长屏底栏保持底部，地图返回按钮可射线命中。home-preview.png、map-preview.png 为最终 v5 标准尺寸截图；acceptance-tall.png、acceptance-map-tall.png 为最终长屏截图。
- 最终 prefab 11 个 Text 均引用 fangzhenglanting.ttf 的 GUID abf46940d0d2f24458e1104e0c89f6b3。最终 Console error count=0。

## 修复与工具结果

静态审查发现旧实现未串行等待场景卸载，以及加载中停用/重开被 transitioning 拒绝。已回交开发，修复为按最新请求协调的 Reconcile，之后执行上述复验。

最初预制比对发现按钮 Text 锚点和地图 Graphic 契约差异，开发补充显式 rectTransform/uvRect 后，acceptance-prefab-v5.json 为 **incomplete，differences=[]，unverified=48**。未知项包含字体/资源/自定义组件等，不改写为 passed。原始 acceptance-prefab.json 保留。

acceptance-code-v5.json 保留 **failed**，仅 OX007：UIHome.Bind 位于公共 ShowMap 前。人工依据 Widget SKILL 将 Bind 视为绑定生命周期重写，符合共享“生命周期重写在前”规则，判为检查器分类限制；没有修改工具报告或为消除提示改变生命周期排序。旧 ScenePath 命名已修复。原始 acceptance-code.json 保留。

## 范围与缺口

纯展示状态不强行新增 HomeEntity；无玩家经济、存档、战斗、网络与假登录副作用。固定文字保存在 prefab，未引入虚构数字。配置、活动红点、货币刷新、64 个 SV 入口等为本次产品范围外，不据此宣称其已实现。

未验证移动真机安全区、独立构建、网页合成预览、首次入场中间帧、Inspector 人工选择重绘及失败注入。本轮快速生命周期验证为同一系统实例；不扩大为任意跨系统协调测试。

原始证据 acceptance-runtime.jsonl，最终核心资产哈希 acceptance-hashes.json。验收仅新增报告、证据和截图，未修改交付代码/资产。最终已恢复 1080×1920，并于 07:38:02Z 确认 isPlaying=false、isPlayingOrWillChangePlaymode=false，未保存运行态测试值。
