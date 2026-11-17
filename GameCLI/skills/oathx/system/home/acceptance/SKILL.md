---
name: oathx-home-acceptance
description: 由未参与实现的 agent 独立验收 Home 基础大厅的展示、数据刷新、导航命中、生命周期和配置依赖，明确角色和季节扩展的未实现边界。
---

# Home 独立验收

由未参与本轮开发的 agent 执行。读取 [Home 开发契约](../development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [CLI 说明](../../../../../integrations/codex/cli/README.md)。不自动派发 agent，不修改交付或基准来消除差异。

记录需求、设计版本、工程绑定身份、起止哈希与实际资源路径。Studio 任务先核对 verified 绑定与 ProjectSettings/GameCLIProject.json；与开发串行操作同一 Editor。

| 检查 | 必要证据 |
| --- | --- |
| 布局与透明度 | 完整截图核对玩家信息、三组货币、四周入口及静态背景；透明父 Graphic 不得隐藏子文字或图标。检查网页预演和 Unity 各自效果。 |
| 数据刷新 | 更新 HomeEntity 的昵称、等级、三组货币和聊天后调用 Refresh，检查真实控件变化；实体或页面缺失返回 false。 |
| 导航 | 实际命中每个入口，核对 HomeEntryRequested 枚举和调用次数；特别检查 Profile/Avatar、Chat/ChatChannel 嵌套按钮。不将直接调用 VM 当作真实点击通过。 |
| 外部系统边界 | 无 RPC、假登录或自动 Fight。未实现目标只发入口请求；若产品已接入，由 Master 协调 SystemActive/SystemDetive。 |
| 固定文本与配置 | 固定文案保存在预制 Text，关闭重开后仍显示；无 Label 加数字的 Entity/VM 属性及转发。VM 仅绑定真实动态值与交互，Metadata 与字段一致。原多语言表可作为源资料保留，不把预制固定文字描述为已支持运行时切换语言。未实现活动占位保持空白。 |
| 生命周期 | 激活、停用、重开、加载中停用；无残留实例、重复监听、停用后打开。 |
| 入场动画 | 实际首次和再次打开时采样默认 Open 状态，核对顶部／右侧／底部位移、信息区淡入及完成后的布局。网页预演消费同一曲线，停止预演恢复样式。不能只检查 controller 存在或手动 SampleAnimation 后宣称自动播放通过。 |
| 状态展示 | 分别切换 NEW、红点、商城蓝色广告提示以及 None/Double/Triple；检查真实显隐、对应原图及原生尺寸。活动按可见项重排，逐项核对中文资源，不使用默认/Tw 图替代运行时 Cn 图。 |
| 字体和循环动画 | 核对原始字体、BestFit、描边材质及 shader；NEW 缩放、掉落上下浮动与 Left 入场均实际播放。网页预演必须加载字体资源且保留这些动画。 |
| 动态扩展 | 角色/季节键正确传给扩展消费者；无消费者时基础页面正常。明确无 Spine/动态季节实现；活动和广告展示不能被错误归为已批准排除项，远端运营服务与 SDK 仍不要求接入。 |

执行 verify code --project <ID> --system Home，以及 verify prefab --project <ID> --prefab <项目页面ID> --asset Assets/Data/Widgets/Home/Home.prefab。保存原始输出和退出码；未知/未验证项不能改写成通过。无项目页面实例时注明该项 incomplete，不拿其他页面 ID 代替。

独立报告列出 passed/failed/incomplete/not-applicable、覆盖范围、原始证据、复现步骤与缺陷。编译、Metadata、视觉与真实交互分开判定；必要项缺证据则总体 incomplete。缺陷交回开发修复并冻结新哈希后复验。本系统通过不代表全产品集成通过，也不授权提交推送。

## 动态开放专项

检查本地开关关闭/重开后实例数与 ID；等级上下跨阈值以及不跨阈值时等级显示都更新；UTC 开始包含、结束排除、空端点与非法条件均明确。原条目必须通过 SyncGroup 绑定 VM；显示数量随开放状态改变，隐藏项不占布局，不应再有源模板克隆或 HomeActivityList。验证数组与直接子节点顺序一致，各项 SyncVisible 指向自身根节点。检查重复刷新不重复监听，关闭重开后无残留项；可用系统经 Master 打开，关停后关闭现有窗口且延迟请求不能重新打开。空 ClassType 不实例化旧业务系统。配置源码包必须包含 Function 服务、启动行、全部活动条件行及集成参考。

## 实体边界验收

按开发规范的统一实体职责，检查本系统业务数据与规则归属、输入与输出集合隔离、非法操作不改变状态，以及 System/UI 无直接改写内部数据。纯展示模块不强制新增实体；区分实体数据与展示/调度状态。
