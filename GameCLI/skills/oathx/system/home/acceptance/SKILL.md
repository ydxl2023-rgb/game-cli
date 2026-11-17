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
| 配置 | 独立配置包包含所有固定 Text 别名与源表，目标导表和 CommonText/Language 加载成功；最后一条文案也能解析。 |
| 生命周期 | 激活、停用、重开、加载中停用；无残留实例、重复监听、停用后打开。 |
| 入场动画 | 实际首次和再次打开时采样默认 Open 状态，核对顶部／右侧／底部位移、信息区淡入及完成后的布局。网页预演消费同一曲线，停止预演恢复样式。不能只检查 controller 存在或手动 SampleAnimation 后宣称自动播放通过。 |
| 状态展示 | 分别切换 NEW、红点、商城蓝色广告提示以及 None/Double/Triple；检查真实显隐、对应原图及原生尺寸。活动按可见项重排，逐项核对中文资源，不使用默认/Tw 图替代运行时 Cn 图。 |
| 字体和循环动画 | 核对原始字体、BestFit、描边材质及 shader；NEW 缩放、掉落上下浮动与 Left 入场均实际播放。网页预演必须加载字体资源且保留这些动画。 |
| 动态扩展 | 角色/季节键正确传给扩展消费者；无消费者时基础页面正常。明确无 Spine/动态季节实现；活动和广告展示不能被错误归为已批准排除项，远端运营服务与 SDK 仍不要求接入。 |

执行 verify code --project <ID> --system Home，以及 verify prefab --project <ID> --prefab <项目页面ID> --asset Assets/Data/Widgets/Home/Home.prefab。保存原始输出和退出码；未知/未验证项不能改写成通过。无项目页面实例时注明该项 incomplete，不拿其他页面 ID 代替。

独立报告列出 passed/failed/incomplete/not-applicable、覆盖范围、原始证据、复现步骤与缺陷。编译、Metadata、视觉与真实交互分开判定；必要项缺证据则总体 incomplete。缺陷交回开发修复并冻结新哈希后复验。本系统通过不代表全产品集成通过，也不授权提交推送。
