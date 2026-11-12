---
name: oathx-launch-development
description: 独立 agent 参考 SvStart 标准模板，为具体产品开发 Launch 启动组件、进度与背景控制，接入 XApp；不照搬示例下载业务。
---

# Launch 产品开发

开发交付后由另一名 agent 按 [Launch 独立验收](../acceptance/SKILL.md) 和本模块需求验收；保留 Launch 作为组件的边界，不为验收增加 System。

必须读取 [系统开发流程](../../../system-development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)。使用 UnityCLI 处理预制体、动画和序列化引用。

## 默认设计

未明确提出设计变更时，保留选用标准模板的布局、视觉、交互和动画，不自行改版。仅做保持表现与行为一致的必要技术适配；产品契约缺失时不虚构业务。遵循[共享默认规则](../../../system-development/SKILL.md#默认保留模板设计)。

## 范式与边界

- 模板 ID：SvStart。读取实际版本和源码包；它提供启动展示与 XApp 接入范式，不要求使用 SV 美术或固定下载流程。
- XLauncher 是普通 UGUI 组件；不使用单例、不复用 XBootstartup、不增加运行时路径查找。XApp 通过私有序列化引用控制它。
- Launch 在业务 UI 系统就绪前可用，负责展示状态，不承担下载调度、认证、业务系统切换。进度由真实任务提供。
- 隐藏整个 Launch 与仅保留背景是不同操作；由产品启动流程决定何时调用，不硬编码等待 Login 页面。

## agent 开始时确定

目标启动场景、启动阶段、进度来源、失败提示、完成后的展示行为，以及是否需要独立背景。检查目标 XApp 和 XLauncher API，复用已存在实现；框架包差异由共享修改负责人协调。

## 实现路径

1. 获取产品编辑的启动 UI 和标准模板，确定需要的展示区域，不照搬参考组件字段数量。
2. 资源放入 Assets/Data/Widgets/Launch，按产品实际层级制作组件与私有序列化绑定；保持全屏根布局。
3. 为调用方提供阶段切换、真实进度、提示、隐藏/仅背景等实际需要的接口，默认参数和文案遵循共享规范。
4. 将组件引用接入目标 XApp/启动场景，说明资源绑定在 prefab 还是场景实例；不主动修改其他业务系统。

开发结果说明包含调用入口、进度语义、完成行为及必要 XApp 集成项。只有维护当前 GameCLI 示例时读取 [参考实现](references/gamecli-example.md)，其中下载阶段、节点路径与背景切换时机不是产品默认需求。
