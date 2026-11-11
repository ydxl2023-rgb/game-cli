---
name: oathx
description: 整理和使用 com.oathx.* Unity 框架的开发规范，按框架模块或业务系统选择独立 skill；用于框架接入、系统开发和框架维护。
---

# Oathx 框架

先读取 [开发规范](development-rules/SKILL.md)，再按任务读取相关 skill。本文只负责定位与职责划分，不替代各系统规范。

## 目录与职责

| 目录 | 适用任务 |
| --- | --- |
| [framework](framework/SKILL.md) | 一次性建立开发环境：包安装、CLI、生成器、最小启动与验收 |
| [core](core/SKILL.md) | XApp、插件、系统生命周期与事件 |
| [widget](widget/SKILL.md) | UIWidget、VM、Binding 与 Metadata |
| [widget-extension](widget-extension/SKILL.md) | UISystem、页面打开关闭与 Fancy 列表 |
| [entity](entity/SKILL.md) | XEntity、实体管理与数据刷新 |
| [rpc](rpc/SKILL.md) | 网络会话、协议与业务通信接入 |
| [bundle](bundle/SKILL.md) | 资源加载、AssetBundle 与热更新 |
| [avatar](avatar/SKILL.md) | 角色、特效与子弹组件 |
| [launch](launch/SKILL.md) | Launch UI、预制体与 XLauncher |
| [system/sv-login](system/sv-login/SKILL.md) | LoginSystem 与 UILogin |
| [system/loading](system/loading/SKILL.md) | Loading 页面、进度和提示更新 |
| [system/master](system/master/SKILL.md) | Master 纯代码系统、玩家实体和业务启动流程 |

## 组织规则

- 所有使用该框架的 skill 放在 `skills/oathx`，一个系统一个目录，每个目录独立维护 `SKILL.md`；新增系统采用 `skills/oathx/system/<system-name>/SKILL.md`。
- 共享编码规范只维护一份；框架模块记录通用契约，业务目录记录该系统的数据、页面、配置和验收要求。不要将某个业务的限制扩展为整个框架限制。
- 新系统 skill 必须引用开发规范及实际使用的模块 skill，并补充到此索引。不要为尚未实现的系统创建空目录。
- 独立 agent 可直接读取目标系统 skill；skill 不自动创建 agent，也不自动授权提交、推送或数据库发布。

## 源码与项目定位

本目录向上两级为 GameCLI 根目录。当前参考工程为 `examples/unitys/client`，包在其 `LocalPackages/com.oathx.*`，业务在 `Assets/Scripts`。其他工程使用本规范时，先定位该工程实际包与业务目录，不硬编码本机绝对路径。

使用前检查目标版本源码和调用方；包内模板化 README 不能作为 API 依据。历史代码与最新用户规则冲突时，以最新规则为准，不批量改造无关旧代码。

这些包在多个工程复用，修改业务应优先落在业务目录。确需修改包时检查兼容性和该包自己的版本控制状态，不把外层 Git 状态当作包已提交。`com.unity.ai.navigation` 不属于 Oathx 框架。
