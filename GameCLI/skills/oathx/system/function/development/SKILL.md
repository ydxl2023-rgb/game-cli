---
name: oathx-function-development
description: 基于 FunctionSystem 表实现本地系统开放状态、条件检测及主协调器门控。
---

# Function 开发

遵循 [开发规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Entity](../../../entity/SKILL.md)；由另一 agent 按相邻 acceptance 验收。

FunctionManagerEntity 统一保存 level、会话开关和已计算开放状态，提供 IsOpen、UpdateLevel、UpdateSwitch、Refresh 与条件计算。集合保持私有，外部不得直接改写状态；Refresh 返回独立的变更快照，不派发事件。FunctionManagerSystem 只负责订阅输入、时间调度、调用实体方法并发送变更事件。FunctionManagerAccess 仅保留类型解析与加载入口适配，开放判断委托实体，不再实现条件规则。Home 等业务按需获取实体并直接调用 IsOpen；系统不持有 Home VM，也不直接打开关闭 UI。

- propFunctionSystem 的 Active 仍表示加载后立即激活，InstallOnStartup 表示启动安装，不能用于代替开放开关。
- Condtions 格式 `{Switch:1,Level:10}`，Level 为非负最低等级；Switch 仅支持 0/1。AND 组合，空条件默认开放，未知条件拒绝开放。本地 FunctionSwitchChanged 覆盖默认开关但不绕过 Level 或时间。
- StartTime/StopTime 使用带时区 ISO 8601（例如 2026-12-01T00:00:00Z 或 +08:00），开始包含、结束排除，空端点无界；拒绝无时区时间、数字 Excel 日期与倒置区间。编辑 Excel 时将时间保存为文本。
- 每个非空 ClassType 必须唯一并可解析为具体 RuntimeSystem；重复类型全部拒绝开放。多个入口要复用同一系统主配置，不复制同类型条件行。空 ClassType 是展示占位，不加载任何业务。
- 输入为 FunctionLevelChanged、FunctionSwitchChanged。先更新实体，再计算状态，最后排队发送 FunctionInputsUpdated/FunctionOpenChanged。Home 只监听完成通知，避免分发顺序导致旧值覆盖新值。
- 一秒 unscaled 时间检查确保关闭 Home 后仍能过期；每次点击与 Master 延迟激活再即时检查。关停后 Master 延迟卸载并复核当前条件，防止旧关闭消息误关已重开的系统。
- Master、FunctionOpen、RedDot 为维持协调、开放和展示服务的基础设施，不受业务开放门控；不得让开放服务自己关闭自己。
- GamePlugin.Load 使用同一 CanActivate 门控，防止绕过 Master 直接加载；闭锁系统可以稍后重新创建。不存在的业务不得凭名称生成假实现或复用旧网络逻辑。
- 示例 SYS_FUNCTION_OPEN 是启动安装且激活的服务行；活动行 InstallOnStartup=0，默认开关来源于已确认的示例布局。不要在产品中强制沿用此运营日程。

表修改通过 UnityCLI dry-run 与正式写入；在隔离目录导出后只复制对应生成文件，保留产品语言表。运行验证须覆盖输入事件、纯边界、异步请求、关闭/重开与重复监听。

## 实体职责约定

FunctionManagerEntity 管理全部开放输入及状态；内部集合不得外露，Refresh 返回独立变化快照。System 只监听、调度和通知，不复制条件判断。
