---
name: master
description: 独立 agent 参考 MasterSystem 纯代码模板，为产品开发系统协调、事件路由和玩家数据生命周期，不生成 UI，不硬编码示例业务去向。
---

# Master 产品开发

开发交付后由另一名 agent 按 [独立系统验收](../../system-acceptance/SKILL.md) 验收代码、事件和生命周期；本模块没有 UI，预制检查记录为不适用。

必须读取 [系统开发流程](../../system-development/SKILL.md)、[开发规范](../../development-rules/SKILL.md)、[Core](../../core/SKILL.md)、[Entity](../../entity/SKILL.md)。

## 默认设计

未明确提出设计变更时，保留选用标准模板的布局、视觉、交互和动画，不自行改版。仅做保持表现与行为一致的必要技术适配；产品契约缺失时不虚构业务。遵循[共享默认规则](../../system-development/SKILL.md#默认保留模板设计)。

## 范式与边界

- 模板 ID：MasterSystem，纯代码，payload.kind=code、无 UI 节点。不得生成 UIMaster、VMMaster 或占位预制体。
- Master 协调系统激活/停用与产品级事件；具体页面逻辑属于各业务系统。数据实体负责自身数据，不把所有业务集中到 Master。
- SystemActive、SystemDetive 和 LoginSucceed 是示例契约，优先复用产品已确认公共契约；多个 agent 不能各自定义同名事件或 RuntimeSystem。
- 本地存档、默认玩家属性、CHAPTER_HOME、登录后进入 Home、退出时卸载 Fight/Notice 都是参考业务，不是产品必选流程。不得为满足这些引用生成范围外系统。

## agent 开始时确定

产品系统清单、启动/登录后的去向、事件载荷、调度完成语义、玩家数据来源与保存时机。确认 Master 与公共事件/基础类的所有权；产品没有明确去向时提出待接契约，不照搬 Home。

## 实现路径

1. 读取模板源码，对照目标 RuntimeSystem、dispatcher 和实体管理 API，明确产品所需生命周期。
2. 设计事件订阅与协调流程，避免分发过程中不安全地修改模块集合；回调语义区分系统激活步骤与 UI 异步就绪。
3. 在 Master/ 下实现系统与必要实体，按产品要求接入加载/保存，失败时不覆盖有效数据或伪造就绪。
4. 将跨系统引用限制在确认的契约内；必要注册与共享配置由指定集成负责人合并，不复制示例配置 ID。
5. 保持 OnActive/OnDetive 优先及公共方法在私有方法前的规范；不为迁就旧示例机械增加状态字段或方法。状态管理应有明确需求依据。

开发结果列出公共事件、系统切换输入/结果、玩家数据责任及所需注册项。维护 GameCLI 现有 Master 时再阅读 [参考实现](references/gamecli-example.md)，其中用户确认的具体方法结构只约束该示例维护任务。
