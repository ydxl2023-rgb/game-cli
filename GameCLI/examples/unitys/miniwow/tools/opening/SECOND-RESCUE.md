# 60002：旅店与医院连续救火

## 本轮范围

60001 领取 → 60002 再取水 10 → 旅店消耗 5 → 对话 3053 → 医院消耗剩余 5 → 完成待领取 → 领取后跟踪 60003。

**灭火不等于建筑修建。** 旅店切回 `Hotel/Hotel_initial`；医院切到 `priest/priest_initial_2`（倒塌阶段），并非完整医院。60003 的救人、招募与之后修建/生产未在本轮实现。

原作依据为 `research/code-study/managed/decompiled/HotUpdate/Zombie/GuideStoryController.cs` 的 Story2、Story2_1、Story2_2、Story2_3、ExtinguisBuildings，以及 `research/config-study/tables/tbconversation.json` 的 3053001。原作完整效果尚未全部还原，以下区分规则和表现。

## 配置与职责

- `propOpening.xlsx/HomeOpening` 保留共用取水范围、容量、计时和视频配置；删除被新表取代的耗水、交易行坐标和致谢对白字段，避免双份配置。
- 新增 `OpeningRescue` 表，数字 ID 1/2/3 分别为交易行/旅店/医院；配置任务 ID、建筑 ID、耗水、目标坐标、取水引导坐标、对白 ID 和目标语言 ID。
- 旅店目标取原 `Hotel_60017/Collider`；医院取 `Hospital_60018/Collider`；第二次取水使用原 `GetWaterPoint2`。导航仍使用恢复的原 NavMesh，不重烘焙。
- 原 3053 的旅店老板对白完整放入语言表，保留换行。`tools/e2cs/Language.cs` 修复生成 XML 注释时未处理换行的问题；只处理注释文本，未删掉实际对白换行。
- `HomeOpening` 持有剧情规则和持久阶段；`HomeOpeningRuntime` 执行移动/计时；`HomeOpeningScene` 只投影结果；HomeSystem 编排 UI 意图和任务事件；TaskSystem 不引用 HomeSystem，不直接更改建筑。
- 旅店老板复用现有实例 2 的 ActorEntity/ActorShape，开始隐藏，旅店灭火后显示。没有额外建立重复 NPC 或新玩法系统。
- 原作完整医院修建、升级、生产和集合式 BuildingEntity 仍未实现，不能把本段剧情状态当成通用建筑系统。

沿用 development 的 RuntimeSystem、实体/显示分离、StorageKey、配置加载和正式 e2cs 流程；参考索引 current（559 文件）。Unity 场景、材质、网格与预制绑定均经 AssetDatabase/PrefabUtility，未手改 YAML。

## 存档与完成边界

旧阶段 0–6 的枚举顺序保留；原 `NextTask`（6）作为旧档边界，读取已有 60001 领取收据后进入 `FetchSecondWater`，不重放开场。

旅店结束持久化 `HotelThanks`，恢复时按配置重建剩余 5 份水；确认对白才允许去医院。医院移动或喷水过程中退出，没有完成灭火就不扣水、不完成任务；重进后可重做未完成的一次交互。旅店已完成的结果保留。

只有医院灭火结束才报告 60002 的 FinishTalk 条件；旅店灭火不能提前完成任务。完成未领取时恢复 `SecondClaim`；明确领取后跟踪 60003，保存为 Finished。仍使用已有 Home 开场保存键和 TaskCollection 领取收据，无额外建筑布尔存档。

## 资产与复现

新增模型变体：

|导出参数|原模型|用途|
|---|---|---|
|`--variant hotel`|Hotel/Hotel_initial_1|旅店灭火前|
|`--variant hospital-burned`|priest/priest_initial_1|医院灭火前|
|`--variant hospital-rescued`|priest/priest_initial_2|医院灭火后倒塌阶段|

1. 运行 `tools/opening/build.mjs` 和 `tools/town/export-scene-config.ps1` 更新配置。配置预览 `second-config.png` 已检查。
2. 使用 `tools/main-scene/export_scene.py` 分别执行上表三个参数。输出每组的中立数据和源资产清单；默认全场景导出及 `--auction` 保留。
3. 编辑模式写 `tools/opening/second.request`，`MiniwowSecondRescueSetup` 将模型导入既有 Model 锚点，绑定火焰、NPC 和对白头像；不重建整个 Home。
4. `index-second.py` 记录 31 条网格/材质/贴图/头像的原资源 ID、来源、用途与输出哈希；`sync-second.mjs` 写既有资源台账及 MW-012，状态为 review，保留人工 annotations。

## 验收

`verify-second.ps1` 从已领取 60001 的保存状态开始，使用实际按钮、实体移动、原导航和正式领取事件。`second-evidence.json` 已记录通过：

- 旧档进入 60002，正确显示两处损坏模型和隐藏旅店老板。
- 原第二取水路线获得 10；旅店扣 5，任务仍 Active，店主显示，医院仍损坏。
- 旅店对白中完整停止/重启：对白、5 水和旅店结果恢复。
- 医院移动途中返回登录：不扣水、不错误完成；重进可以继续。
- 医院扣最后 5，模型为 priest_initial_2，任务 Completed 并等待领取。
- 领取前重启保持完成与建筑结果；领取后进入 60003，再次重启保持 Claimed。
- Console 错误 0。`second-completed.png` 已查看实际 1080×1920 渲染。

最终配置去重后又执行 `verify-auction.ps1`，60001 全流程、领取前重启、模型/NPC 结果和交接 60002 均通过。正式检查见 `second-gamecli-code.json`、`second-gamecli-architecture.json`；编辑器工具报告 `second-editor-code.json` 的 OX015 是编辑期场景装配查找，运行期仍使用序列化引用。

## 尚未覆盖的原作表现

现阶段使用本地火焰粒子、水流和固定交互按钮。原资源飞行动画、原喷水粒子、抱水动作/减速、世界跟随气泡、医院求救气泡和引导手指尚未复原。旅店对白暂用原 NPC 图标，未恢复 FullBody/Conversation_hotel_2 的完整大立绘和打字机。此前发现的开场视频启动问题未在本轮修复；本轮回归通过实际跳过确认继续，不宣称视频回归通过。
