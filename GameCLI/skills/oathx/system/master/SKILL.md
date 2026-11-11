---
name: master
description: 整理、归档和接入 Oathx Master 纯代码系统，维护玩家实体、登录事件及业务系统启动流程；不生成 UI、VM 或预制体。
---

# Master

必须遵守 [开发规范](../../development-rules/SKILL.md)、[Core](../../core/SKILL.md)、[Entity](../../entity/SKILL.md)。以下工程路径相对 GameCLI 根目录。

## 源码和边界

- 当前实现位于 `examples/unitys/client/Assets/Scripts/Systems/Master`：MasterSystem 管理事件和启动流程；MasterEntity 包含 MasterPropertyDef、玩家初始属性及章节读取。
- 公共基类为 `Assets/Scripts/Systems/RuntimeSystem.cs`，封装 dispatcher 加载、卸载及下一帧完成回调。归档时携带源码和 .meta；目标工程已有基类时先比较，不创建重复类型或覆盖不同版本。
- Master 是纯代码系统，不创建 UIMaster、VMMaster、Canvas、Widget Config、空白 prefab 或占位 UI 节点。
- 保留用户确认的 OnActive / OnDetive / Load / Unload / Save / Launch 结构。Launch 卸载 LoginSystem 后仅加载 HomeSystem，不恢复此前的其他业务系统列表。

## 数据库和模板页面

源码清单为 `integrations/codex/config/master-scripts.json`，包含三个 C# 文件及 .meta。标准模板 ID 为 `MasterSystem`，分类 system，分组 master。payload.kind 为 code，root 为 null，design.nodes 为空；源码放 scripts，保留 UTF-8 原文和 SHA256。

首次注册运行 `node scripts/register-master-template.mjs` 预检，用户授权上传后加 `--apply`。已有模板只验证相同内容，不直接覆盖；更新需沿用管理员候选发布流程并递增版本。

系统模板页展示“纯代码”入口，允许查看和下载源码包，可拖入关系区作为逻辑系统卡片，不生成 UI 画布内容。验证数据库回读源码哈希、空节点和模板页展示；仅归档源码时不修改业务行为，不宣称完成登录联调。

## 当前生命周期契约

- MasterSystem 不依赖 RPC 或网络协议。保留 LoginSucceed、LoginFailure 本地通知类型，不恢复 SystemLoadEvent、SystemUnloadEvent 或保存事件。
- GamePlugin 按 FunctionSystemConfig 的 InstallOnStartup/Active 配置创建并激活 Master。OnActive 直接调用 Load，不等待登录事件。
- Load 检查 IsActive 和 MasterEntity；已有本地存档则 Deserialize，无存档则 Serialize 默认数据，成功后调用 Launch。加载失败返回 false，不覆盖损坏存档。
- OnDetive 在 UnsubscribeAll 后空一行调用 Unload；Unload 通过 dispatcher 卸载 Home，不自动保存或返回登录页。
- Save 检查激活状态和实体存在后直接写本地存档，返回 bool。
- 不添加 loaded、launched、activation 或替代状态字段。Launch 的异步完成回调仅检查 IsActive，不宣称具有重复请求拦截或跨激活版本取消能力。
- MasterEntity 依赖 Config.Excel.CHAPTER_HOME、Oathx.Entity；RuntimeSystem 依赖 Oathx.Core。不要在归档时打包业务 UI 或修改用户确认的结构。
