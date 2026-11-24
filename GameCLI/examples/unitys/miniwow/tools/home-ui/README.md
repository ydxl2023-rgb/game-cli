# 截图主界面：预制结构与资源核对

日期：2026-11-24。输入：[用户截图](reference.png)。本轮是实现前的结构与依赖审计，未修改 Unity 客户端、未生成或替换 Home.prefab。

## 结论

原作不是一个 Unity 主界面 prefab，而是 FairyGUI `Main/MainPage`（`ui://1pyb9685q2my4z`）和多个子组件，再由运行时代码装入图标、头像、资源数量及世界跟随气泡。

- 包内主页面尺寸 **720×1560**，直接子节点 **36**；根控制器为 `HideMainUI / IsGuide / MansionExpandBtnPanel`，包含 `Tabs_In / Tabs_Out` 等 10 个过渡名称。
- 本轮闭包覆盖 MainPage、6 类世界气泡，以及 `tbmainfunctions / tbmainactivity` 配置图标：**46 个组件、135 张图片，共 181 条资源记录**。已解析引用无缺失；这不是整个产品或全部运行时状态的完整依赖闭包。
- 所有 181 条资源 ID 均匹配既有台账；135 张图片的 SHA256 全部校验通过。
- SQLite `research/resource-catalog/catalog.sqlite` 新增 `home_ui_resources`，保存 URL、源资源 ID、包、名称及完整用途 payload；未覆盖 annotations。本轮未同步 PostgreSQL。

直接打开 [可搜索资源审阅页](index.html)，可按原节点、资源名、活动名和 `ui://` 搜索并点击图片。完整结构在 [COMPONENTS.md](COMPONENTS.md)，机器可读清单在 [inventory.json](inventory.json) 和 [resource-index.json](resource-index.json)。

## 截图对应关系

下表“原作”列为包内结构与代码证据；“Unity 目标”是本项目建议结构，尚未创建。

| 截图区域 | 原作组件/节点 | 已确认图片与依赖 | Unity 目标 |
|---|---|---|---|
| 顶部金币、紫色货币、行动力 | `ResList` → `Common/CommonRes` | 资源底图、`icon / title / plus / red`；最多3项，运行时 `GlobalTable.MainRes → TbItem.SmallIcon` | Home 顶栏内 CurrencyStrip，3个独立 CurrencyEntry；数量、上限、加号分开 |
| 头像、昵称、初级村庄、战力 | `LeaderCom` → `Common/PlayerHead` | `Main/hub_bg_1`、`hub_bg_1_0`、`Common/common_icon_19`；头像和边框动态；村庄等级图来自 `TbGuildLevel.Name` | ProfilePanel：头像/边框/昵称/等级图/战力图标/文本/红点独立 |
| 左侧商城、七日签到、开服冲刺、首充、成长试炼 | `ActivityButtonListCom/Activities` → `Main/ActivityButton` | 商城 `hub_button_18`，签到 `hub_button_29`，成长 `hub_button_31`，开服 `hub_button_52`；首充为特效 `e_ui_shouchong_icon_1` | ActivityList + ActivityEntry（icon/effect/title/time/redDot）；按配置和状态排布 |
| 右上社区、公告、问卷 | `ExFunctionBtn1/2/3` → `ExFunctionButton` | `hub_icon_18 / hub_icon_19 / hub_icon_9`，各自红点 | QuickActionEntry 三个独立点击项 |
| 右侧集市、任务、背包、邮件 | `FunctionBtns` → `Main/MainButton` | 背板 `hub_bg_3`；集市 `hub_button_12`、任务 `hub_button_20`、背包 `hub_button_21`、邮件 `hub_button_16` | FunctionList + FunctionEntry；不固定为只有截图四项 |
| 左下任务进度与章节 | `TaskCom` → `TaskButton / ChapterButton` | `descText / valueText / effect`、章节 `title`、红点 | TaskTracker：任务描述、进度、章节按钮分开；保留 SideTask 扩展 |
| 村庄聊天/播报条 | `ReportCom` → `Common/ReportContent / TalkLabel` | 列表 `Contents`、正文 `title`、频道 `tag`；不是一张背景加固定一句话 | ReportStrip + ReportEntry |
| 底部探索、英雄、府邸、试炼、公会、领地 | `ExploreBtn / Tabs / WorldBtn` | `MainBigTab / MainTab`；底板 `hub_bg_7`、装饰 `hub_bg_25`；探索 `hub_button_22`、领地 `hub_button_32_1`；Tabs四条原始标题与图片在 inventory 中 | BottomNavigation：两侧大入口+中间4个NavigationEntry，独立锁定/红点/展开状态 |
| 右下招募 | `RecruitBtn` → `Main/MainButton3` | `Main/hub_icon_13` 与红点 | RecruitEntry |
| 右下“夜鳞鱼汤:0”提示 | `TipsCom` → `Main/MainTipsCom` | `hub_title_8 / hub_bg_23 / newterr_icon_17`；物品图片来自 `TbItem.Icon` | SupplyHint：物品图、数量、警告按钮独立。是缺食物提示，不是夜间倒计时 |
| 世界中的“进入”、建筑名、感叹号 | `BubbleLayer / EntranceBubbleLayer` + `Building/*Bubble` | 进入图 `Building/hub_button_15_0`；FactoryBubble 有控制器换图；名称与位置由对象状态驱动 | WorldMarkers 独立层：EntranceMarker、BuildingTitle、StatusBubble，跟随场景挂点投影 |
| 篝火、地形、NPC、角色、绿色选中圈 | 场景与模型/特效系统 | 不属于 MainPage 的屏幕背景图片 | 保留 Home.unity；NPC/角色/特效按场景系统实现 |
| 截图最上面白色标题与胶囊按钮 | 微信容器 | 不属于游戏 MainPage | 不制作成游戏 prefab；只处理设备安全区 |

## 建议的 Unity 层级

维持现有 `Assets/Data/Widgets/Home/Home.prefab` 主入口，替换当前18节点的临时 Home UI；保留 `HomeSystem` 的 Launch → Home 生命周期。下面是建议的职责层级，不是已经落地的资产：

```text
Home（全屏 Stretch / UIHome / Metadata / Animator）
├─ WorldMarkers（进入气泡、建筑名、状态标记；场景投影）
└─ SafeArea
   ├─ TopBar（CurrencyStrip + ProfilePanel）
   ├─ ActivityList
   ├─ QuickActions
   ├─ FunctionList
   ├─ ContextHints（SupplyHint / RecruitEntry）
   ├─ TaskTracker
   ├─ ReportStrip
   ├─ BottomNavigation
   └─ MansionExpandedPanel
```

Home 专属组合归 `Assets/Data/Widgets/Home/`；跨系统复用的头像、货币、红点等先检查现有 Common 能力，再放 `Assets/Data/Widgets/Common/Micros/`；通用图标遵循 `Common/Icons/`。不要为每个截图小图标创建一个业务 System。

UIWidget/VM 负责展示与点击意图，业务状态由对应 Entity 所有，HomeSystem 负责协调。截图里的昵称、1.9万、4262、48/20、3.7万和任务1/3只作参考状态，不能硬编码成产品事实。开放、锁定、红点、活动与资源数要分别绑定，不由静态截图推导规则。

## 1080×1920 布局适配

截图实际419×788，顶部约44像素属于微信；其余游戏区域约419×744，与1080×1920接近。原包720×1560比例不同，不能整体乘1.5后压缩高度。

采用宽度基准缩放并重建锚点：顶栏锚顶、底栏锚底、活动锚左上、功能锚右上、任务与播报锚底栏上方；中央留给可移动场景。设备安全区单独处理。原节点XY/尺寸用于相对间距与资源比例参考，最终要在1080×1920运行截图核对。

## 已确认的动态规则

- `UIMainView.OnShow` 取 `GlobalTable.MainRes` 前3项；`RefreshRes` 区分带上限的行动力与普通资源，不能共用固定金额样例。
- `UIMainFunctionBtnsCom.RefreshBtns` 依据 `tbmainfunctions` 与开放条件生成右侧列表。表有8项，截图只显示其中部分；背包 Feature=0，但其他按钮不能因此全部常开。
- `UIMainActivitiesCom` 使用活动数据、排序和最多8个外显项；活动标题、倒计时、红点和 effect 分开。包内表有20条，不应一次全部塞入截图。
- `UIMainTabsCom` 中间4项对应英雄/府邸/试炼/公会；府邸还有 Estate/Relic/Pet/Book 展开面板。锁定由条件决定，不是修改图片灰度就完成。
- `UIMainTipsCom` 的缺食物分支取 `bestFood → TbItem.Name/Icon` 并引导旅店；截图“夜鳞鱼汤:0”与该显示方式相符，具体物品 ID 仍待解表确认。
- `BuildingInteractiveModule` 将 `BuildingEntranceBubble` 挂在 `EntranceBubble` 场景点，`BaseBuildingController` 将标题气泡挂在 `Bubble` 点；应跟随相机，不按截图位置硬摆。

## 字体、切图和未闭合依赖

原 `StartGame.cs:45–47` 注册 `FangZhengLanTing` 默认字体及 `HanYiJingKai`。本次解析中战力等5个文本节点显式引用 HanYiJingKai；两份 TTF 已在 `extracted-resources/assets/data/fonts/`。当前 Unity Home 只绑定方正兰亭，后续应补齐汉仪字体及原描边参数。

图片是原图集的独立裁片；`sprite.rect / offset / original_size / rotated` 都保存在清单。生产导入需处理 trim 和九宫格，不能把裁片尺寸直接当原组件尺寸。部分功能图如邮件、交流群自带文字，应登记为美术本地化依赖；正式 UGUI 拆分图标和文本时要处理原图里的烘焙文字，不能再叠一遍同样标签。

以下仍需在正式生成前闭合，不能称已得到完整可运行 prefab：

1. 三个顶部资源的具体配置 ID/SmallIcon、截图头像和边框、村庄等级徽章、鱼汤物品 ID/Icon：已确认调用链，未完成其全部配置解码与截图一一匹配。
2. 首充、魔术夜、转盘及任务扫光等效果：记录了配置 effect 名，尚未展开模型/材质/动画依赖，不能以空图标判定资源缺失。
3. 当前解析覆盖源引用、Loader、List模板/条目、Button换图、图标/文本Gear、字体和控制器页名；尚未还原 Relation、过渡曲线、完整布局约束、列表属性覆盖、分支/多分辨率资源与控制器执行结果。
4. 世界角色、头顶任务标记和场景选择特效不是静态HUD闭包；已加入6类建筑气泡，其余角色链按后续NPC任务补充。
5. 当前关卡/角色/商业化业务尚未实现；本轮未接入网络或执行原游戏程序集。

## 工具与证据

`python tools/home-ui/analyze.py` 从原始 FGUI 描述与既有台账解析结构和两张配置表；`python tools/home-ui/build_report.py` 校验引用、生成网页/结构文档/联系表，并更新本地 SQLite。`reference.png` 是用户原图，不能作为生产整屏 Sprite。

二进制字段读取依据官方 [UIPackage](https://github.com/fairygui/FairyGUI-unity/blob/master/Assets/Scripts/UI/UIPackage.cs)、[GComponent](https://github.com/fairygui/FairyGUI-unity/blob/master/Assets/Scripts/UI/GComponent.cs)、[GObject](https://github.com/fairygui/FairyGUI-unity/blob/master/Assets/Scripts/UI/GObject.cs) 与 GList/GButton/GearBase 的公开读取流程；本地参考快照位于被忽略的 `research/code-study/evidence/fgui/`。

业务证据均为 `research/code-study/managed/decompiled/HotUpdate/Zombie/` 下的反编译代码：`UIMainView.cs:181–298,427–485`、`UIMainTabsCom.cs`、`UIMainFunctionBtnsCom.cs`、`UIMainActivitiesCom.cs`、`UICommonRes.cs`、`UIMainTipsCom.cs:250–280`、`BaseBuildingController.cs:300–342`、`BuildingInteractiveModule.cs:118–145`。原始配置schema见 `Zombie.Table/CfgMainFunctions.cs`、`CfgMainActivity.cs`。
