# 主场景牧师缺失修复（2026-11-27）

## 原因与范围

`Home.unity` 的 `NPC_priest_2` 只有 Transform，未挂 Spine 渲染组件，也未进入 HomeScene 的 Actor 绑定数组。医院入口文字、按钮、光圈由独立的交互逻辑显示，因此可以出现“入口存在、角色缺失”。不是任务重置删除了角色。

`MiniwowPriestSetup` 使用 Unity 原生资源与场景 API 补齐 SkeletonAnimation、ActorShape，以及 Actor 5（配置身份 60018）。不增加系统，不改变任务规则。安装按实例 ID 查找，可重复执行。

原始位置、旋转、缩放保留（见 inspection.json 的 NPC_priest_2）。世界骨骼 scale=0.01，与已接入的场景居民一致；独立世界材质使用 Spine/Skeleton。复用已导入的原始 npc_priest_2 骨骼文件、atlas 文本和纹理引用，不修改医院页面使用的 scale=1 骨骼或 UI 材质。

## 参考机制

- development 源码漂移检查：559 文件，current，无变化。
- FRAMEWORK.md 的 Entity/状态机职责 → 沿用 HomeEntity 集合、ActorEntity 数据和 ActorShape 状态机。
- MiniwowTownSetup.ConfigureShape / ConfigureActor / InstallResident → 使用同样的序列化绑定和 idle 表现。
- MiniwowVillageSetup → 复用显式请求、编辑模式安装和场景表读取机制；产品差异仅为补齐牧师，保留现有角色与原场景坐标。

## 验证

- Unity 编译、Console：无错误。
- 当前 code-verify 规则：新增工具 0 issue；系统架构检查 72 文件、8 系统，0 issue。
- 实际 App 登录进入 Home：actors=5；NPC_priest_2 骨骼 valid=true，idle 时间持续推进，192 个网格顶点。
- 实际截图已查看，牧师显示正常；再次加载 App 后进入 Home，仍保持同一个牧师绑定和有效 idle 动画。
- 验收前后开场、村庄、任务三个存档值逐字一致，没有重置用户本次体验进度。
- 本次未宣称验证全部医院业务流程；仅验证缺失角色、资源绑定及重新进入。

本机证据位于 client/Library/OpeningQA：priest-fixed.png、priest-evidence.jsonl、priest-code-review.json、priest-architecture-review.json 和 priest-user-checkpoint.json。
