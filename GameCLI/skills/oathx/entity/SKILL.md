---
name: entity
description: 使用 com.oathx.entity 的 XEntity、XEntityManager 与 XNumerical，维护业务实体、查询和 UI 数据刷新边界。
---

# Oathx Entity

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/XEntity.cs`、`XEntityManager.cs`、`XNumerical.cs`，再读目标业务实体和已有创建、查询、刷新、释放调用方。

- 复用已有业务 Entity 和管理器，不为同一数据在 UI 内另建实体仓库。
- QueryEntity 的结果按当前调用契约检查有效性；不要把查询当成创建或网络加载。
- 实体字段、属性 ID、数值和刷新方式遵循该业务既有定义，不用 VM 字段反推持久化结构。
- 区分实体更新与 VM 展示同步；只有系统职责要求时才查询或刷新实体。例如 LoginSystem 当前不负责实体查询和通信，UILogin 保留 Invalidate(LoginEntity) 入口。
- 数据保存或通信需阅读调用方及 [RPC](../rpc/SKILL.md)，不因实体存在就自动新增远端读写。

验证实体缺失、正常刷新和相关页面展示；不以编译成功替代实际数据契约验证。
