---
name: unity-cli
description: 使用 Unity CLI 与 UnityCLI 进行 UI 运行态检查、交互诊断、ViewModel/元素状态修改、Animator/AnimationClip 资产操控、Excel 配置表读取查询写入、Jira 任务访问，以及通过 unity-cli ugui 命令把 Web visual-ui 与独立切图资源生成 UGUI prefab 的正式工具链工作流
license: MIT
metadata:
  audience: developers
  workflow: unity-ui, ugui, runtime-inspection, diagnostics, web-to-ugui-prefab, animator, excel-config, jira
  aliases: ugui cli, ugui workflow, ugui runtime, ugui diagnostics, unity-cli, web to ugui, web prefab, animator cli, excel config, jira cli
  tags: unity, ugui, mvvm, unitycli, diagnostics, runtime, prefab, animator, animationclip, excel, xlsx, config, jira
  concepts: ViewBinding, Binding, Command, ViewModel, UIManager, WebVisualUi, UGUIPrefab, AnimatorController, AnimationClip, ExcelConfig, Jira
  related: unitycli
  tools: unitycli.exe, imagegen
---

## 这个技能是干什么的

`unity-cli` 指导代理在 **Unity UGUI + UnityCLI** 体系下完成 UI 工作，覆盖两大场景：

- **Web → Prefab**：从 Web 原型提取布局 JSON，通过 `Library\UnityCLI\UnityCLI.exe ugui` 生成 UGUI prefab
- **运行时验证**：PlayMode 下检查视图、诊断绑定、执行交互、修改 ViewModel 状态
- **Animator 资产操控**：通过 `Library\UnityCLI\UnityCLI.exe animator` 创建/编辑 AnimatorController、AnimationClip、参数、状态、过渡、曲线和 Animator 组件绑定
- **Excel 配置表**：通过 `Library\UnityCLI\UnityCLI.exe excel` 读取 schema、查询行/单元格、校验主键，并用 dry-run 保护写入
- **Jira 任务访问**：通过 `Library\UnityCLI\UnityCLI.exe jira` 使用 Jira 地址和访问令牌配置鉴权、查询 Issue、JQL 搜索、评论、流转、创建和更新 Jira 任务

## 硬约束

**不要用代码绕过工具链去修改 Unity 资源。** 创建或更新 prefab 必须使用 `unity-cli` 的 `ugui` 命令入口：先 `ugui --dry-run`，通过后再执行正式 `ugui`。不要手写、直接编辑或脚本拼接 `.prefab`。

**不要用代码创作 UI 位图资源。** 需要生成 `design-master.png`、按钮底图、面板、图标、装饰等真实美术图片时，必须使用 Codex 的 `imagegen` 图片生成能力。代码只能用于布局提取、截图、裁剪参考图、透明度后处理、拼合预览、报告生成和 Unity 导入设置；禁止用 HTML/CSS/canvas/SVG/Python/Unity 代码绘制最终美术资源。

**不要把确认稿或设计图当整屏主视觉 Sprite。** `design-master.png` 和用户确认稿只能作为参考图，禁止作为 prefab 的整屏背景图来显示完整 UI，也禁止在其上叠透明按钮/协议区/点击热区。prefab 必须由 `asset-manifest.json` 中的独立资源图拼装；拼不准就重新生成或修正对应资源图。

**默认直接采用 imagegen 修补后的独立资源。** 资源拆分流程必须先用代码从 `design-master.png` 裁出 `Source Crop`、生成 `cutout`、`repair_mask`、`edit_target` 和报告；随后由 Codex `imagegen` 修复遮挡、文字、缺失和破边区域。需要透明的修补资源必须要求 imagegen 输出到纯色 chroma-key 背景（默认 `#ff00ff`），再由代码扣色生成 alpha。修补后的完整资源可以作为 `assets_png` 中的 Production Asset，代码只允许做尺寸对齐、alpha/透明边处理、chroma key、预览和校验。`Source Crop` 是修复参考和抠图基底，不再默认要求把 donor 局部回贴到原图；只有在需要严格锁定可见像素、AI 明显跑偏或用户明确要求时，才使用 Source-First Composition 作为回退策略。

**裁图边界必须以设计图确认为准。** HTML / `visual-ui.json` 的元素 rect 只表示运行时布局、语义和交互热区，不能直接作为最终资源裁切框或资源尺寸。进入 `Source Crop` 裁切前，必须用 `bbox-review.html` 把 `design-master.png` 作为 1:1 背景，叠加 `html_rect` 与可调整的 `design_visual_bbox`；执行者调整到完整包含描边、阴影、发光、圆角、外扩装饰和透明边缘，并让用户确认后，才允许把 `design_visual_bbox` / `source_crop_bbox` 写入 `layer_plan.json` 并进入后续 imagegen 流程。

**不要污染原始 `visual-ui.json`。** `visual-ui.json` 是固定提取脚本从 HTML DOM 生成的布局基准，只能作为 `html_rect` 和节点语义来源；禁止写入设计图 bbox、新资源尺寸或新 Sprite 路径。bbox review 必须使用原始 `visual-ui.json` 加已确认 bbox 的 `layer_plan.json`。如果确认 bbox 后确实需要改变 prefab 中 Image 的 rect 或 Sprite，必须生成派生的 `<UIName>.visual-ui.recut.json` 给 `unity-cli ugui` 使用。

**Prefab 生成固定走 UnityCLI。** 进入 Unity 交付阶段时，按顺序执行 `ping`、必要时 `compile --wait`、`console --type error`、`ugui --dry-run`、正式 `ugui`、`console --type error`、`screenshot`、`save`。只有项目安装并编译了 `com.oathx.widget` 且该 prefab 明确要接入 Oathx.Widget / UISystem 时，才在正式 `ugui` 后追加执行 `widget-metadata --dry-run` 和正式 `widget-metadata`。dry-run 有 issue 或 Console 有 error 时禁止继续正式生成。
**Animator / AnimationClip 必须走 UnityCLI。** 创建或修改 `.controller`、`.anim`、Animator 参数、状态机、Transition、AnimationCurve 或场景对象 Animator 绑定时，必须使用 `unity-cli animator`，不要手写 YAML 或直接拼接序列化文本。正式写入前优先 `--dry-run`，写入后执行 `console --type error --count 80` 和 `save`。
**UGUI 根节点必须永远是全屏 Stretch。** 通过 `unity-cli ugui` 生成或替换场景实例时，UI 根节点 `RectTransform` 必须保持 `anchorMin=(0,0)`、`anchorMax=(1,1)`、`pivot=(0.5,0.5)`、`anchoredPosition=(0,0)`、`offsetMin=(0,0)`、`offsetMax=(0,0)`。不要把根节点改成 center anchor；所有视觉位置都应由根节点下的子节点在设计分辨率坐标系中表达。
**ListView 列表必须使用真实列表结构。** 在 Web visual-ui 中，列表容器必须写 `data-ui-type="ListView"`，并按方向写 `data-list-layout="vertical"`、`horizontal` 或 `grid`。垂直列表生成到 Unity 后必须挂 `VerticalLayoutGroup`，水平列表必须挂 `HorizontalLayoutGroup`，网格列表才使用 `GridLayoutGroup`。列表容器的直接子节点必须是空的 `RectTransform` item 根节点，例如 `LeftPlayerItem00`；数字、名称、职位、能力、图标、行底图等控件都必须作为 item 根节点的子节点，不能直接平铺到 UI 根节点或列表容器外。
**Widget Metadata 是可选包能力。** `com.oathx.unitycli` 只反射转发 `widget-metadata`，不直接依赖 `com.oathx.widget`。如果项目没有安装 `com.oathx.widget`，纯 `ugui` prefab 生成、截图和保存流程必须继续完成，不应因为 `widget-metadata` 不可用而中断；只需记录“跳过 Oathx.Widget Metadata 接入”。安装了 widget 包时，`widget-metadata` 会按通用命名规则绑定 `ButtonClick`、文本、子 `VM`、`IContainer` / `VList<T>` 和 `FancyGridClick`。列表字段会绑定同名或单数化后的 `FancyGridScrollView`，例如 `rewards -> RewardsGrid` 或 `RewardGrid`；点击字段例如 `premiumRewardClick -> PremiumRewardGrid`。正式写入前必须先 `widget-metadata --dry-run`，并确认没有 issue、`boundTargetCount` 覆盖必要字段。
**FancyGrid Cell 的根 pivot 必须匹配中心排布。** `FancyGridViewCell` 按 cell 中心点排布多列；作为 `FancyGridScrollView.cellPrefab` 的 cell 根 `RectTransform.pivot` 应为 `(0.5,0.5)`。如果 cell 根仍是左上角 pivot，运行时会出现三列错位、裁切或互相压住。
**按钮和复合控件不能退化成整图热区。** 按钮应按 UGUI 标准层级拆成 `Button + Image` 根节点、图标 Image、文本 Text；按钮主背景 Sprite 必须直接绑定在按钮根节点上，不要做成额外的 `XxxButtonBg` 子节点，否则 Unity Button 的颜色状态不会作用到真实可见底图。货币栏、导航项、入口按钮、玩家信息等重复或可绑定结构应做成可复用组合节点，禁止用单张包含图标、文字、背景、等级、VIP 或加号的整图代替。所有货币栏必须至少拆成底图数字框、货币图标、金额文本和添加按钮；加号/添加按钮不能烘焙在底图数字框里。详细规则见 `@references/ugui-component-composition.md`。
**文本组件必须支持旧版 Unity。** 默认通过 `Assets/csc.rsp` / `Assets/mcs.rsp` 开启 `USE_OLD_TEXT` 宏；开启时 `unity-cli ugui` 必须生成 `UnityEngine.UI.Text`、`InputField`、`Dropdown`，Metadata 绑定使用 `SyncText`，并且源码不能编译到任何 `TMPro` 类型。只有关闭 `USE_OLD_TEXT` 时才允许默认生成 `TextMeshProUGUI`、`TMP_InputField`、`TMP_Dropdown`，并绑定 `SyncTextMeshPro`。

**bbox 改变后必须重新生成资源链路。** 一旦 `design_visual_bbox` / `source_crop_bbox` 调整，旧 `Source Crop`、mask、`edit_target`、imagegen 输出、`alphaSource` 和 `asset-manifest.json.size` 都视为过期。复用旧 AI 输出再缩放只能用于临时验证问题，不能作为最终交付资源；最终必须基于新的 `Source Crop` 重新跑 imagegen 修复和后处理。

## 文档索引

按需跳文档，不要一次加载全部：

| 场景 | 文档 |
|------|------|
| unity-cli 命令速查与精确调用 | `@references/unity-cli-commands.md` |
| Web 原型设计规范 | `@references/web-prototype-design.md` |
| Web 原型生成 Prefab 完整流程 | `@references/web-to-ugui-prefab.md` |
| UGUI 标准按钮、玩家信息、货币栏、复合控件拆分 | `@references/ugui-component-composition.md` |
| 从设计图生成 UI 精灵资源 | `@references/asset-generation-workflow.md` |
| 验证流程与排障指南 | `@references/verification-and-troubleshooting.md` |

用户输入 `unity-cli`、询问命令、要求调用 UnityCLI、或进入 prefab 交付阶段时，优先读取 `@references/unity-cli-commands.md`，再按任务需要跳转到完整流程文档。
