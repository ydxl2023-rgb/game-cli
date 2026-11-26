# 60003：清理横梁与救人

## 原作事实与本轮范围

依据 `research/code-study/managed/decompiled/HotUpdate/Zombie/GuideStoryController.cs`：

- Story3/Story3_1：医院先为 `priest_initial_2`，目标是医院 Collider。点击“清理”，角色到达后开始烟尘及 3 秒倒计时，结束切到 `priest_initial_3`。
- Story3_2：再次点击“救人”，到达后操作 1 秒；提交 FinishTalk 后自动 TaskReceiveRequest，没有手动领取按钮。
- Story4：进入下一任务后医院切回配置的 InitialModelPath，即 `priest_initial`。随后才是加伦的出现、对白和招募。

本轮接通 60003 → 自动领取 → 跟踪 60004 及对应医院模型结果。**未实现 60004 的加伦生成、招募和英雄所有权**；不能把任务切换解释成已获得英雄，也不表示医院已完整修建。

## 配置及结构

沿用 `propOpening.xlsx/OpeningRescue`，新增数字 ID 4/5：均属于任务 60003 和建筑 60018；不消耗水，目标使用原医院 Collider 坐标。新增 DurationSeconds、ActionTextID、AutoClaim，清理分别为 3 秒/“清理”/不领取，救人为 1 秒/“救人”/自动领取。原三处救火操作的 1.5 秒也统一放到该表，删除 HomeOpening 中旧的共用时长字段。

固定提示、动作名称和倒计时格式均进入 propCommonText。表通过 artifact-tool 修改、e2cs 导出；`third-config.png` 已查看。未直接改生成配置代码或运行数据。

HomeOpening 负责操作阶段、成本和结果；HomeOpeningRuntime 使用原 NavMesh 移动，到达后累计有效时间，再提交操作。HomeSystem 通过事件编排，TaskSystem 负责已有完成及领取事务。没有具体系统相互调用。

`HomeOpeningOperationProgress` 只传递临时倒计时，HomeSystem 交给 UIHomeOpening 更新文字，不重复执行输入开关或重建页面。操作中显示剩余秒数并隐藏水图标，取消后恢复对应资源显示。固定 UI 引用仍为私有序列化字段；VM 的 Metadata 字段契约不变。

原保存值 12 由旧 Finished 明确为 ClearDebris，数值不变；新增 RescuePerson、ThirdComplete、AwaitRecruit 顺序追加。前两项领取收据仅推进尚未进入的阶段，不会每次刷新都把已清理的横梁重置。

## 保存与退出

- 清理过程中取消移动或退出：当次计时作废，横梁仍在；不保存半完成倒计时。
- 清理完成：先保存 RescuePerson，再切换原模型。重进保留清理结果，等待独立“救人”操作。
- 救人完成：先保存 ThirdComplete，报告任务完成，再按配置请求领取；TaskSystem 仍校验领取状态。
- 开场恢复若发现 ThirdComplete，会重报幂等完成观察并请求领取，弥补场景保存与任务保存之间的中断窗口；该窄窗口恢复分支为代码审阅结论，本轮未注入精确崩溃点。
- 自动领取成功后根据实际任务收据进入 AwaitRecruit，医院切回原初始模型。重新进入不重复清理、救人或显示领取按钮。

## 资源与复现

`export_scene.py --variant hospital-cleared` 提取原 `priest/priest_initial_3`，共 11 节点、10 网格、2 材质和 1 贴图，缺依赖 0。`MiniwowThirdRescueSetup` 通过原生 Mesh/Material/场景接口添加该变体；四个医院阶段明确绑定为 burned、collapsed、cleared、initial。未重建整场景或改原导航。

清理烟尘使用原 `ef_tex_smoke_01` 贴图的本地 ParticleSystem 适配，执行期间播放，取消/完成后 StopEmittingAndClear，场景卸载时一起销毁。**并非完整恢复原 e_jianzhu_jiesuo_priest_smoke_1 动画**。世界跟随操作气泡、原进度条和完整原特效仍待还原，目前沿用现有操作界面。

复现步骤：

1. `node tools/opening/build.mjs`，`pwsh.exe -File tools/town/export-scene-config.ps1`。
2. `python tools/main-scene/export_scene.py --variant hospital-cleared`。
3. Unity 编辑模式写入 `tools/opening/third.request`；安装结果见 `third-install.json`。原生预制绑定 WaterIcon，不手改 YAML。
4. `python tools/opening/index-third.py`，`node tools/opening/sync-third.mjs`。14 条源资源记录保留资产 ID、源/输出哈希和用途，复用既有 scene_imports；不覆盖人工 annotations。

## 验收

`verify-third.ps1` 从已领取 60002 的存档开始，通过实际按钮和原导航验证；未伪造完成事件。`third-evidence.json` 全部通过：

- 原旧档进入 ClearDebris，显示倒塌模型；到达后烟尘与倒计时出现，任务仍 Active。
- 清理期间退出，场景/Runtime/UI 均释放；重启仍需要清理。
- 完整清理后切换 priest_initial_3，任务仍 Active；重启保持该阶段。
- 救人移动途中退出，清理结果保留，不错误完成。
- 救人结束自动领取 60003 并跟踪 60004，全程未点击 ClaimClick。
- 医院初始模型及已领取状态在完整重启后保持；无重复操作/领取按钮。
- Console 错误 0，实际 1080×1920 截图 `third-cleared.png`、`third-completed.png` 已检查。

正式静态检查见 `third-gamecli-code.json`、`third-gamecli-architecture.json`；编辑器工具检查见 `third-editor-code.json`。本轮还回归前两项任务，证据为当前 `auction-evidence.json` 和 `second-evidence.json`；开发测试最终停回 60003 待清理，方便用户体验新内容。

development 参考索引 current（559 文件）；继续沿用 RuntimeSystem 生命周期、实体/显示分离、StorageKey、XPluginUtility 事件、现有 TaskSystem 完成及领取接口。未修改 development 工程。
