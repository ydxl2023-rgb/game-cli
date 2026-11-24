# Miniwow Launch 独立验收

2026-11-24；项目 p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3，数据库 v3，设计 1080×1920。已独立核实 Unity binding verified，两个身份 ID 与 ProjectSettings/GameCLIProject.json 一致。

## 结果

总体 **incomplete**：已测编辑器功能通过，自动 prefab 校验无 differences，但有 28 项未验证证据，不将其改写成 passed。未验证移动真机与独立构建。

- passed：加载/更新两模式分别输入 0、0.375、1、-1、2、NaN，实测百分比依次 0、38、100、0、100、0；对应 fill 横向锚点夹紧，纵向保持 0..1。原始记录 acceptance-runtime.jsonl（32 条成功响应）。属性输出为四舍五入显示，不以 0.38 文本证明 0.375 浮点精度。
- passed：Hide 使根 inactive；Show 恢复；ShowBackground 保持根 active，同时 Loading/Update 均 inactive。
- passed：11 个 XLauncher 序列化引用均非空，引用指向对应面板/标签/填充；根 anchorMin=(0,0)、anchorMax=(1,1)、sizeDelta=(0,0)、anchoredPosition=(0,0)。
- passed（编辑器范围）：修复后重新 Play，实际根可见且两个前景面板均隐藏，符合启动完成保留背景契约。Console error count=0。证据 acceptance-restart.jsonl。日志筛选没有返回 MINIWOW_LAUNCH_READY 条目，因此不声称用该日志证明入口；实际背景态及调用方源码作为本轮证据。
- incomplete：gamecli verify prefab 返回 status=incomplete、differences=[]、unverified=28，包括 XLauncher 自定义组件和 RawImage 资源等能力缺口。完整原始结果 acceptance-prefab.json；不得把无差异等同完整验证。
- not-applicable：Launch 没有 Widget/VM 或业务 Entity，不强行新增；无业务按钮、购买或网络下载实际行为。

## 修复与范围

审查发现原 XApp 非 Editor 启动枚举最大屏幕分辨率，可能转横屏。已回交实现者，新增可选 startupResolution 后进行启动复验。移动端原生输出/方向及桌面指定窗口的实际构建行为尚未运行验证。

SV Launch v7 configurations=null；现有界面静态中文来自 prefab。默认 XApp 加载状态仍可能短暂显示 Loading... / HotfixSucceed；本次未完成独立中文启动阶段、失败重试流程及多种安全区域验证。未创建新的伪下载或自动进度。

验收未修改业务代码、prefab 或场景，未保存运行态测试值；已执行 run stop，并于 2026-11-24T07:07:26Z 用 run status 确认 isPlaying=false、isPlayingOrWillChangePlaymode=false。仅新增验收报告与原始证据。文件哈希见 acceptance-hashes.json（不包含随后由实现者修复的 XApp）。

