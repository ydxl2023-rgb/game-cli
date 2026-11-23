---
name: oathx-settings-development
description: 独立开发 Settings 设置系统，参考 SV 五页签界面、原版动画、本地偏好保存和产品服务事件，交付 Entity、UI、VM 与配置资料。
---

# Settings 开发

先读[系统开发流程](../../../system-development/SKILL.md)、[开发规范](../../../development-rules/SKILL.md)、[Entity](../../../entity/SKILL.md)和[Widget](../../../widget/SKILL.md)。由另一名 agent 按[Settings 验收](../acceptance/SKILL.md)检查。

从数据库读取 SvSettings 的实际发布版本、scripts、configurations 和 materials。未明确提出产品设计变更时，保留五页签、字体、控件状态、滚动区域与原版动画；不要把所有设置改为通用列表。产品开发不依赖 SV 源工程，不覆盖产品已有账号、音频或渲染系统。

页面和子窗口的业务请求事件统一定义在 `UISettings.cs`。Widget 通过事件提交用户操作，System 调用 Widget 的 `Invalidate` 刷新实体数据。

## 参考契约

- SettingsSystem 协调生命周期、实体操作和事件；SettingsEntity 校验、保存与恢复设备偏好。UISettings 仅管理当前页签与原生动画；VMSettings 只放动态值及交互，固定标签在 prefab 上。
- 页面在 `Assets/Data/Widgets/Settings/Settings.prefab`；代码按 `Settings/UI`、`Settings/VM`、Settings 根目录组织。
- 保留画面、声音、功能、隐私、下载五个页签。默认画面页；原版隐藏的平台选项和账号弹层不擅自显示。声音页的耳机动画在切入时重播，返回栏入场动画每次打开重播。
- 30 项偏好使用有名枚举；不接受未知枚举、NaN、Infinity 或非法档位。模型只有高低两档，中档预设映射低档，预设反推遵循同一映射。音量与静音独立，静音不覆盖已保存音量；下载策略只选网络，不启动下载。
- 偏好保存在反射生成的 `settings.settingsentity.values` 键，禁止业务硬编码键。恢复时未知或损坏值回退本项默认；快照独立且只读，调用方不能改写实体内部状态。
- SettingsChanged 提供本地快照。产品负责接入音频分组、SV 专有渲染效果、游戏功能、服务端隐私与系统推送。保存开关不代表服务已启用、SDK 已授权或下载已完成。恢复应用须由产品启动流程读取 Entity 或请求快照，不能依赖用户先打开设置页。
- SettingsActionRequested 仅表达账号、客服、协议、礼包兑换等入口请求，不内置原 SV URL、SDK、RPC、账号删除或账号切换行为。不得伪造服务成功。若产品需要未接入提示，文案进入 propCommonText，不在代码硬编码中文。

## 原生与配置

复用现有 SyncSlider、SyncSliderChange、SyncClick、SyncVisible、SyncText，不创建同功能 ISyncAction。滑块回写必须有实体无变化保护，避免刷新导致保存/事件递归。只在页面初始化时绑定回调，重复 Refresh 不增加监听。

当前 Widget 包有 SyncSliderChange 实现但默认工厂未注册；参考系统在打开前注册现有实现。移植时先核对目标包，不能遗漏注册，也不要复制实现。源透明 Image 的颜色必须完整恢复，不能把点击区域转换为白色底图；滑条必须保留原生 anchor、pivot、sizeDelta 和 anchoredPosition，不能拿网页绝对尺寸替代 Slider 驱动的 RectTransform。

CLI snapshot restore 后检查全部节点的 UI Layer；当前工具未完整恢复 layer 时，用正式 prefab attach/extract 链路恢复并保持主预制 GUID。截图必须确认是最新渲染帧；若 EditorWindow 捕获重复旧帧，可通过 CLI 调用 UnityEngine.ScreenCapture.CaptureScreenshot 交叉核对，不能把旧截图当作实际页面验收。

材料包含原始导出、独立图片、中文字体、原生 AnimationClip/Controller、映射清单和完整代码。保留源粒子/网格资料并在合同中记录当前 UGUI 转换范围，不能把未转换的特效宣称为已还原。浏览器不执行 C# 和产品服务；原生动画与浏览器动画分别验收。

只合并 propFunctionSystem 的 SYS_SETTINGS 行并用目标导表器再生成，不整表覆盖。Master 的 HomeEntry.Settings 路由只是参考集成；独立产品无需安装 Home。关闭交给 SystemDetive，由协调器卸载，系统用局部 Type[] 调 CloseWidget。

制作端脚本位于 integrations/codex/scripts，import-sv-settings、prepare-settings-input、build-settings-contract、bind-settings-native 用于提取和初次制作。build-settings-contract 会生成初始 VM 与枚举，维护阶段先比对再运行，不能覆盖产品修改。Unity 资产修改必须走 UnityCLI。

默认中文参考只显示原版 CnBox/CnBox_2 协议入口；海外引继码/绑定/AI客服 Box 作为完整资料保留并默认隐藏。原版在运行时通过区域功能和账号状态互斥这些组，产品接入时必须保持互斥，不能把两组同时打开。ScrollRect 保留原版 Mask.showMaskGraphic=false 或 RectMask2D；二者不能简单互换而遗漏背景隐藏/裁剪。

用户确认的模板精简：删除腾讯客服 TxKefu、暖心饭团 RiceRoll、体力 Power、周末福利 Weekend、豪强赛 HeadquartersMatch、极限特训 ExtremeTrain、合宿日常 HomeDaily 的节点、VM 绑定、事件/枚举入口，禁止后续从原始资料重新带回。保留其他未点名设置。偏好总数为 30；保留枚举数字不得因删除而移位，读取旧 JSON 必须逐键忽略废弃项，保留有效音量/画质。原始 source archive 是溯源资料，不是现行交付契约。
