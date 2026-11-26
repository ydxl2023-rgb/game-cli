# 系统事件解耦独立静态复核

日期：2026-11-25。审查者：town_review，未参与实现。范围：GamePlugin、MasterSystem、LoginSystem、HomeSystem、TownSystem。

结论：最终修订静态审查 passed，未发现本轮剩余阻断问题。此结论不冒充独立运行验收；本轮未操作 Unity，也未修改实现。

## 架构与生命周期

- Master 不缓存、查询或直接操作其他具体业务系统；通过 SessionRouteRequested/Completed 表达请求、处理最新版本完成结果，并由自身实体决定会话阶段。
- 已有管理器 GamePlugin 负责组合模块。在 Update 先处理框架移除队列，再执行路由；事件处理只记录路由和发布门禁通知，避免在事件字典枚举期间增删模块。
- Login 路线先请求卸载 Home/Town，再于后续 Update 清理完成后加载 Login；当前注册表查询缺席安全，等待就绪不持有跨帧系统实例。
- Login 接收重置事件；Home 通过 TownReadinessRequested/TownReadyChanged 获取状态、发布营地可见性；Town 将会话允许与营地可见性结合控制输入。原有地图、退出清零和场景操作门禁保留。
- Master 停用清订阅并发出 Stopped，重新激活的新版本请求替代旧路由；回报校验版本，旧结果不能提交新会话。

## 已修复问题

初审发现 Login 等待40秒超时会使路由 Complete，而 Master 不接受该失败，迟到页面随后点击进入永久等待。最终 GamePlugin 的超时条件已限定为 MasterPhase.Entering：Login 保持 WaitingReady，实际就绪后仍可完成入口交接。

这是保留入口等待语义，不是新增 Login 资源永久缺失时的自动重试机制；永久缺失资源仍不会就绪。本轮没有独立注入超过40秒的加载延迟。

## 运行证据归属

主 agent 报告编译成功、完整 verify-session.ps1 与 verify-player.ps1 均 PASS，架构检查0 issues。本审查者仅复核最终源码，未独立重跑这些检查，不沿用旧架构的独立运行记录冒称本版已独立运行。

## 审查版本 SHA256

| 文件 | SHA256 |
| --- | --- |
| GamePlugin.cs | 4B786B116CDAE3670D43B63D7D0E0C3C44820604F7C5D49F52CCA1CDF0EF3D4F |
| MasterSystem.cs | 2B4ED02696EB4FA470D1AC2D649C910E9678138A360E077A59F5D8D564D3E5CE |
| LoginSystem.cs | F5EC2BD0EC870CA164CB164184DB3DE8EFB42AC4C68BAA2969C1E4A26A481C42 |
| HomeSystem.cs | A0163594EF3CE4291BF23111E196C9660FD5A1790EAE78FFC0B073C94D77925E |
| TownSystem.cs | E2ED216A7E119A0AECF01294636FB6F8AF0CC18E31A0E17CA907469BB044F402 |
