# 开场体验：登录至任务 60002

本次实现可玩的首段：登录成功 → 原作开场视频 → 牧师对话 → 任务 60001 → 取水 10 → 交易行救火 → 商人对话 → 手动领取 → 当前任务 60002。

60002 的旅店/医院救火仍是后续工作。本次不把它自动完成，也不把首次救火的演出当成通用建筑解锁、背包或经济系统已经完成。

## 原作对照后的结论

见 [原作差异核对](ORIGINAL-COMPARISON.md)。当前是业务流程可玩版，未通过原作表现还原验收。已确认缺少资源飞入/飞出、携水动画与减速、世界挂载气泡、模型切换及原对话交互；取水导航点还被误用为碰撞盒中心。后续应按该核对表修正，不能继续把任务状态通过当作表现通过。

## 操作

从 App 场景运行并进入游戏。新存档播放原视频，可确认跳过，也可取消跳过继续观看。牧师对话后使用原有固定摇杆走到水井黄色标记范围，进入范围自动获得 10 份水。点击“救火”后沿原 NavMesh 走到交易行，播放喷水并等待 1.5 秒；也可用摇杆取消自动移动，水不会提前扣除。商人对话结束后点击“任务完成 · 点击领取”或原任务栏，才领取 60001 并跟踪 60002。

重进读取本地存档，已领取任务不重复领取，已结束视频不重播。救火中断保留携水状态；完成灭火时才扣水并保存，任务系统随后接受幂等完成观察。任务收据先保存再通知界面。当前为单一本地存档，没有多账号、云同步、背包经济原子存档或引导节点检查点。

编辑器菜单 `Miniwow → Opening → Reset Local Progress` 现在会停止当前 Play 会话，等旧系统和实体释放后清除本功能的 Home 开场键和 Task 集合键，再自动从配置的 App 场景启动。运行中也可使用，不再只删磁盘而保留内存进度。正常登录仍会恢复存档，不会每次自动清档。底层 `MiniwowOpeningProbe.ResetLocalSave` 仅供明确控制重启顺序的验收脚本使用。

## 配置与职责

- `config/propOpening.xlsx/HomeOpening`：数字 ID 1，任务 60001、下一任务 60002、水道具 83011、数量与消耗 10、灭火 1.5 秒、原水井体积、交易行交互点、视频引用和对话组 ID。
- `OpeningDialogue`：数字 ID 3042、3043，引用公共文本表中的说话人和原对白。公共文案均经 `propCommonText.xlsx` 导出至语言表。
- `propTask.xlsx` 的原始 Task/TaskChapter/TaskGuide 行保持原数据；60001 的 GuideID 仍是原作的 0，没有虚构 Guide 节点替代原剧情业务。
- `HomeEntity.opening` 持有 `HomeOpening` 业务数据与转移规则；不存场景或 UI 引用。存档键使用 development 的 StorageKey 元数据方案。
- `HomeSystem` 接收 UI 意图、刷新 Widget，并通过事件对接 TaskSystem；没有具体系统互相引用。
- `HomeRuntime` 持有普通 `HomeOpeningRuntime` 执行对象，沿 ActorEntity/HomeNavigation 移动。它不是新增玩法系统，也不直接移动 ActorShape。
- `HomeOpeningScene` 只有序列化场景表现引用。UIHomeOpening/VMHomeOpening 通过正式 UGUI、Metadata 和原生资产绑定制作。非缓存 Widget 在 Unref 中停止视频并释放 RenderTexture。
- `TaskCollectionEntity` 存储任务集合和本地收据；`TaskEntity` 校验恢复数据。TaskSystem 在派发、进度、领取和章节切换后保存。

## 原作证据与适配范围

依据 `research/config-study/PLAYER-JOURNEY.md`、恢复的 `GuideStoryController.Story1`、`tbconversation` 3042/3043、原 Building_sdc 的 GetWaterCollider/AuctionPoint。视频实际长度 29.75 秒，零音轨，因此另接恢复的 `start_game.0.m4a`，转 PCM wav，不重配音乐。视频按比例显示。

原取水碰撞盒中心 (-17.23,35.3,14.68)、大小 (7,1,6)；交易行交互点 (-9.38,35.3,9.21)。没有重新烘焙 NavMesh。1.2 的到达半径是本地交互容差。原位移与取水区域数据保留，黄色目标圈为本项目指引。

美术使用原视频、音乐、牧师/商人头像、水图标、卷轴与按钮、橙色火焰序列图。火焰粒子和弧形水流仍是本地原生表现适配，未恢复原 `e_jiaoshui_1` 粒子。交易行烧毁/修复模型、商人场景 Spine 和水井恢复现已接通，详见 [60001 场景结果](AUCTION-RESULT.md)；其他火灾阶段尚未实现。

资源用途、原资产 ID、原路径证据和源/目标 SHA256 见 `unity-imports.json`，数据库复用 `miniwow_resources.scene_imports`，不上传二进制，也不覆盖人工 annotations。

## 复现

60003 现已接通：清理横梁 3 秒 → 独立救人 1 秒 → 自动领取 → 跟踪 60004。医院模型、倒计时、烟尘适配与中断恢复见 [清理横梁与救人](THIRD-RESCUE.md)。60004 招募尚未接入。

60002 后续现已接通：再次取水 10 → 旅店用 5 → 3053 对白 → 医院用 5 → 领取并跟踪 60003。阶段模型、旧档接续和中断恢复详见 [连续救火实现与验收](SECOND-RESCUE.md)。这里只实现灭火，不表示建筑已经完整修建。

1. `node tools/opening/build.mjs`，再 `pwsh.exe -File tools/town/export-scene-config.ps1`。使用已有 artifact-tool 工作簿工具与 e2cs，不手改生成配置。
2. `python tools/opening/design.py`；音频工具安装在临时目录：`python -m pip install imageio-ffmpeg --target client/Library/OpeningTools`，再执行 `python tools/opening/prepare-audio.py`。
3. `pwsh.exe -File tools/opening/install.ps1`：正式 UGUI dry-run/生成、组件、Metadata dry-run/绑定；Editor 请求在编辑模式下追加本场景引用，不重建原 Home 场景。
4. `python tools/opening/index_resources.py`，再同步资源台账。
5. `verify-play.ps1`、`verify-play.ps1 -RestoreOnly`、`verify-interruption.ps1`。测试直接调用真实按钮和 Actor 移动适配，不伪造完成事件或瞬移角色。验收脚本会清除本功能存档，只用于明确的新存档验收。

## 验证记录

- `export-log.txt`：全量原任务字段回查、1594 个引导节点和条件表达式校验通过。
- `play-evidence.json`：真实视频/音乐、跳过确认与取消、取水 10、导航救火、扣水、完成待领取、领取后 60002，无 Unity 错误。
- `play-evidence-restore.json`：完整停止/重启后保持 60002 和 60001 已领取，视频音乐不重放。
- `interruption-evidence.json`：视频中退出释放纹理、播放器、界面、场景和 Runtime；重进可播放。救火中退出保留 10 水和未完成任务，重进后可正常完成。
- 原视频自然播放结束进入牧师对话已实测；其时任务仍为 Active。最终自动分支验证主要覆盖跳过与恢复。
- `gamecli-code-validated.json` 与 `gamecli-architecture-validated.json`：共享检查，最终结果以交付更新的文件为准。已有人工 review 不表示已覆盖所有动态路径。

Development 漂移检查为 current（559 文件）；沿用 RuntimeSystem 生命周期、Entity 规则、Widget/VM、配置导表和 StorageKey。产品差异为本地救火库存与表现适配，没有改 development 产品代码或原始资料。
