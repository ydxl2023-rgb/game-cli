# MainPage → UIHome 实现记录

2026-11-24；项目 miniwow；Unity 2022.3.62f2；1080×1920。

## 工程入口与职责

- `client/Assets/Data/Widgets/Home/UIHome.prefab`：主界面；旧 Home.prefab 通过 AssetDatabase move 改名，保留 GUID `c38e1d4ea7a037942a474c489d46eb0c`。MainPage 是原作 FairyGUI 组件名，不是可直接改后缀使用的 Unity 预制。
- `client/Assets/Scripts/Systems/Home/HomeEntity.cs`：校验源表、持有本地状态、生成独立快照；点击时复核入口显隐与解锁。UpdateResources 接受有效本地资源更新；UpdateEntryAvailability 接受宿主开放状态。初始化一次，关闭重开保留内存状态；无存档和线上服务。
- `HomeSystem.cs`：串行加载/卸载 Home 场景、配置加载、请求路由、Refresh、Launch 交接；不缓存 Entity 或 VM。
- `UI/UIHome.cs` 与 `VM/VMHome.cs`：页面绑定与快照展示。`UI/UIHomeEntry.cs`、`VM/VMHomeEntry.cs`：20 个入口统一通过 SyncGroup Ref；刷新不重复绑定。锁定入口仍可点击获取解释。
- `Systems/Common/UI/UIRequestEvent.cs`：共享 UI 请求事件基类。
- `Home/Common/HomeSafeArea.cs`、`Home/Common/HomeSceneCamera.cs`：Home 通用表现组件，迁移保留原 GUID。
- `Assets/Scripts/GamePlugin.cs` 内的 `GameUISender` 类：独立 UISender 适配器，与 GameUILoader 同文件；GamePlugin 注入自身的 XEventDispatcher，仅负责装配，不再实现 UISender。Send 仍同步派发，Post 仍排入框架事件队列。
- 通用图标位于 `Widgets/Common/Icons`，两套原字体位于 `Widgets/Common/Fonts`，页面背景和入场动画位于 `Widgets/Home/UIHomeAssets`。

## 原作依据与产品适配

结构依据为 `Main/MainPage`（ui://1pyb9685q2my4z），参考 UIMainView、UIMainTabsCom、UIMainFunctionBtnsCom、UIMainActivitiesCom 和已解出的 tbmainfunctions/tbmainactivity。详见同目录 README.md、COMPONENTS.md、resource-index.json。

实现顶部资源及玩家信息、左侧活动、右侧功能、顶部快捷入口、任务和聊天摘要、招募、底部导航。202 个独立 UGUI 节点，38 张引用图片；不是截图整页贴图。微信白色导航条不属于游戏 UI，不导入。

静态预制和 Studio 预览使用配置初始布局；运行时活动/功能入口随显隐重新排列。固定标签来自 propCommonText 源表，制作时写入 Text。来源图内含字的功能图标裁去底部文字区，使用独立 Text；社区/公告/问卷的书法字仍属于原美术本地化依赖。头像使用包内默认 hero_head_20032，并非截图账户头像。货币/行动力是本地示例语义，第三资源图标尚未通过原 GlobalTable.MainRes→TbItem 完整映射确认。

探索当前进入已有地图预览；其他允许入口派发 HomeEntryRequested，并明确显示“该系统尚未接入”，不声称目标业务已经打开。府邸/试炼/公会/领地按本地源表处于锁定状态。原作完整开放条件后续由独立功能系统接管；当前没有伪造等级或时间条件算法。

## 尚未恢复的边界

- 首充入口依赖 e_ui_shouchong_icon_1；配置中隐藏且没有替代图片。未恢复原作全部活动特效、运营状态与全部活动集合。
- 红点须由独立业务红点系统计算，本次不以截图伪造通知。头像、任务、活动真实状态尚无对应业务系统。
- 原作建筑/人物气泡需要场景对象、建筑系统及坐标投影；不作为固定 HUD 贴图生成。夜鳞鱼汤提示属于旅店缺粮逻辑，尚未接入。
- 保留既有静态营地场景，不包含 NPC、角色动画、场景粒子或完整建筑交互。当前视角不等于用户截图的相机位置。
- 0.6 秒入场淡入为本产品实现，不宣称恢复原 FairyGUI 的全部过渡曲线。
- 本次没有安装标准 SvHome 的全部业务依赖；SvHome v31 用作职责与工具链参考，原作 MainPage 为本次明确指定的 UI 结构。

## 配置与资源追踪

`config/Home.xlsx` 是唯一配置源，包含 Entries、Profile、propCommonText 和 Evidence。金额、进度、昵称、开放状态是本地演示数据，非原作账户数据。后续编辑使用 UnityCLI excel（先 dry-run）；不直接改导出的 JSON。

`export_config.py` 导出到 `client/Assets/Data/Config/Home.json`，保存工作簿 SHA256。HomeSystem 在打开页面前加载；缺表/无效表不回退硬编码。UIHome 序列化 entryOrder 与配置 ID 严格匹配，换序或增删必须重新生成预制。

`production-resources.json` 记录源路径、源资源 ID、源 SHA256、恢复原始尺寸与偏移、裁剪参数、输出 SHA256 和 Unity 目标路径。通过既有资源库新增 home_ui_production_assets 表索引，不覆盖人工资源标注。图片只有恢复裁剪/透明边距操作，没有重新绘制原图。

## 正式制作流程

使用 pwsh.exe，从 miniwow 根目录执行；Unity 必须打开正确工程并退出播放。

1. 用 `UnityCLI excel` 修改产品源表，执行 `python tools/home-ui/build_uihome.py`（Python 依赖位于 research/python-libs；需设置 PYTHONPATH）。该脚本导表、恢复图片边距并生成 uiHome 设计输入。
2. 用 GameCLI `clis/unity-cli/bin/Debug/net8.0/unity-cli.exe ugui`，指定 `--input tools/home-ui/uihome-payload.json --assets-dir tools/home-ui/assets --project client --prefab Assets/Data/Widgets/Home/UIHome.prefab --overwrite`，先 `--dry-run --json`，成功后正式执行。
3. 构建当前 client/LocalPackages/com.oathx.unitycli/UnityCLI~/UnityCLI/UnityCLI.csproj 到 tools/home-ui/cli，再执行 `pwsh.exe -File tools/home-ui/bind-uihome.ps1`。此完整绑定步骤用于刚生成的新预制，不在已绑定预制上重复添加组件；字体、Metadata 和共享图标重定向均走 CLI。没有手写 Unity YAML。
4. 编译、从 App.unity 播放，执行 verify-uihome.ps1，观察随后 RapidRestart 最终状态。截图、运行记录和 binding-evidence.json 保存同目录。

工具缺口仅在 miniwow 的独立 LocalPackages 中补齐：widget-metadata 接受 --path，将 Metadata 绑定到预制内的明确子 Widget；旧 Generate 入口保留。未改 development 工程或发布共享模板。SyncAnchoredPosition 为框架已有同步器，在 GamePlugin.Install 显式注册。

## 验证记录

- 正式 UGUI dry-run/import 成功；根 Metadata 校验通过，缺失引用为 0；20 个子 Metadata 均逐一 dry-run 后生成。
- 1080×1920 与 1080×2400 画面检查；上下区域锚定，中央场景保持可见；没有设备刘海实机验收。
- Launch 隐藏时 Home 场景已加载，UIHome 唯一实例；原字体缺失数为 0。
- 真正 GraphicRaycaster 命中验证探索往返、锁定府邸、英雄未接入反馈、关闭提示。
- 连续刷新 20 次后每个入口仍只有一个运行监听；活动隐藏后重排；独立快照修改不影响实体；负数资源事务被拒绝。
- RapidRestart 后恢复单实例、营地模式；本轮运行 Console error 为 0。
- 独立 agent 完成静态审查，初始化安全与预制 ID 契约问题已修复；运行结果是主 agent 自测，不冒称独立运行验收或完整原作复刻。

未提交、推送 Git，未发布标准模板。
