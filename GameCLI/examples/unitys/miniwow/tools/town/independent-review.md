# TownSystem 独立验收

日期：2026-11-25（Asia/Shanghai）。审查者：独立 agent `/root/town_review`，未参与实现；只写本报告与验收证据，没有修改交付源码或资源。

## 范围与身份

- 需求：TownSystem 接管现有 Home 场景，保留 Home UI；原作 Spine 店员与一名访客；旅店点击、访客移动/等待/返回；输入阻挡与生命周期清理。
- Studio 项目：`p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3`。
- Unity 身份：`83c29c9f-21c1-4597-9691-6707ca146fa7`。
- `gamecli_ui_unity_binding(requireVerified:true)` 返回 verified，路径与 `client/ProjectSettings/GameCLIProject.json` 一致。
- 依照项目规范、共享 development-rules、system-development 和 Home acceptance 的相关生命周期/输入要求复核；未将 Home 标准模板的完整业务需求扩大为本轮 Town 要求。

## 结论

**本轮限定范围 passed。** 两项初审问题均已修正；没有发现仍阻断该首版场景流程的缺陷。此结论不代表完整原作主城、全部建筑系统或全产品模板验收通过。

| 检查 | 结果 | 证据 |
| --- | --- | --- |
| 原作骨骼初始化 | passed | 独立 Snapshot 观察 3 个有效 SkeletonAnimation，网格顶点数分别为 8、244、261，贴图名正确 |
| Spine 动画真实推进 | passed | 两次独立 Snapshot 的三个 idle trackTime 均递增；移动期间 OriginalHeroVisual 为 walk 且 trackTime > 0 |
| 旅店交互与往返 | passed | 独立调用实际屏幕投影点的 BeginPointer/EndPointer 路径，触发 Moving → Waiting → Returning → Idle；到达计数只加 1 |
| 地图输入隔离 | passed | 通过 Home 的真实 UI 点击开启地图；CLI search 观察 HomeSceneCamera.enabled=false；建筑手势 began=false、accepted=false；返回营地后 enabled=true |
| 拖动拒绝与重复请求 | passed | 复核开发原始 runtime-evidence：超阈值拖动不触发建筑；Moving 中重复访问被拒绝；实现中撤销后不恢复点击资格 |
| 同实例生命周期 | passed | 复核运行记录中移动中停用后 scene/context/visitor/Home 清理，再激活及快速切换恢复单一实例；静态 generation 复核正确 |
| 跨实例场景交接 | passed | 开发记录遵循框架延迟 Remove 后再 Load；静态 sceneOperationInProgress 跨实例门禁位于 try/finally，避免旧卸载与新加载重叠，并有 SubsystemRegistration 重置 |
| 1080×2400 | passed | 复核开发原始记录，旅店点击、到达和返回完成；独立视觉检查以 1080×1920 为主 |
| 视觉资源 | passed（所选角色） | 已查看 focused.png 与独立 accepted.png，旅店店员与访客可见；3 套骨骼运行有效，不代表恢复其余 NPC |
| Console | passed | 独立 `/console` 返回 totalCount=0 errors |
| 自动代码规范 | passed | 最新 code-town.json 为 0 issues，3 个源码 SHA256 均与当前文件一致 |

## 初审问题与处理

1. 原地图背景不参与 UI raycast，地图可穿透点击和拖动主城。修订后 HomeSystem 显式控制 TownSystem.SetInputEnabled，TownScene 同步关闭相机控制组件与自身手势；已独立实测关闭/恢复。
2. 初版异步场景互斥仅限同一模块实例，可能被旧实例卸载影响新实例。修订后使用跨实例场景操作门禁，并遵守 dispatcher 的延迟移除语义。已复核运行记录及实现，不以同步 Unload+Load（会取回待移除旧实例）冒充跨实例测试。

## 原始证据与复现

- `independent-runtime.json`：独立 CLI 原始请求与完整响应，包括连续骨骼采样、相机 enabled、地图 UI 点击、建筑/访客手势、Console。
- `runtime-evidence.json` / `verify-town.ps1`：开发完整生命周期、屏幕比例与交互复现；独立复核了原始数据和脚本断言。
- `client/Library/Town/accepted.png`：独立 CLI 截图，截图后又完成一次往返，最终恢复营地与 Idle。截图是忽略目录中的本地证据。
- Town 手势探针使用实际投影屏幕位置、UI raycast、碰撞体 raycast 和与鼠标/触摸相同的处理方法；未使用物理触屏设备验证操作系统触摸投递，不应把它表述为真机触摸验收。

## 冻结源码 SHA256

审查始末以下文件内容一致，最后与最新规范报告交叉核对：

| 文件 | SHA256 |
| --- | --- |
| TownNpc.cs | `364FDCDDD83560DA9E6BB77C58686A5E17F4DE8D0807C80BAF83D2FC69570C02` |
| TownScene.cs | `8F0DDB684ECF3CEA34896051D02FF1BEF6BEC8B14E3AB387FB0405B415FF348F` |
| TownSystem.cs | `5C3EF1EF38C3A9B11768C60BC73DF6C079E0BAFC8B32A4BCED678B6B856104A0` |
| HomeSystem.cs | `D92A5AC1FF0283A1308F3D9FCAAD7F914ED258D28FB14255010F8EC61B4F224A` |

## 不在本轮完成范围

没有建筑解锁、升级、消费/恢复事务或存档；旅店入口只是场景交互目标，不代表建筑已建成或可提供服务。移动使用短的明确路线，不是完整 NavMesh 或任意位置寻路。没有验证所有建筑与 NPC、原作全场景像素一致性、联网和战斗循环。不得以本报告发布标准模板或自动提交推送。
