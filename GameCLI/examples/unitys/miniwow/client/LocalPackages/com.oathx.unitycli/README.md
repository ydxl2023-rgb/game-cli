# UnityCLI

UnityCLI 是一个 Unity Editor bridge + Codex skill 工具链，用于在 Unity 工程中执行编译、Console 检查、运行态诊断、资源操作、截图，以及通过 `ugui` 命令把 Web visual-ui 和独立切图资源生成 UGUI prefab。

当前统一使用 `unity-cli`，不再使用旧的 `fui-cli` 目录或触发词。

## 安装 unity-cli skill

1. 在 Unity Editor 中打开 UnityCLI 窗口。
2. 点击 `Skills` 按钮。
3. 工具会把包内的 `unity-cli/` skill 安装到本机 Codex skills 目录：

```text
%USERPROFILE%\.codex\skills\unity-cli
```

如果设置了 `CODEX_HOME` 环境变量，则安装到：

```text
%CODEX_HOME%\skills\unity-cli
```

安装后，在 Codex 中使用 `unity-cli` 或 `ugui workflow` 相关说法即可触发该 skill。

## 推荐触发说法

在 Codex 中推荐这样说：

```text
使用 unity-cli 创建一个界面
使用 unity-cli 流程生成 UGUI prefab
使用 ugui workflow 从效果图/切图生成 Unity 预制
调用 unity-cli 的 ugui 命令创建预制
使用 unity-cli 检查 Unity Console
使用 unity-cli 调用 compile 和 screenshot
```

推荐触发词：

```text
unity-cli
ugui cli
ugui workflow
ugui runtime
ugui diagnostics
web to ugui
web prefab
```

## UnityCLI 可执行文件

UnityCLI 安装后，可执行文件位于 Unity 工程根目录：

```powershell
Library\UnityCLI\UnityCLI.exe
```

通用格式：

```powershell
Library\UnityCLI\UnityCLI.exe [--project <path>] [--format human|json] <command> [options]
```

- 从 Unity 工程根目录执行时通常不需要 `--project`。
- `--format human` 是默认人类可读输出。
- `--format json` 输出原始 bridge JSON，适合脚本解析。
- Unity Editor 必须打开当前工程。

## 当前 bridge 命令速查

| 命令 | 用途 |
|------|------|
| `ping` | 检查 Unity Editor bridge 是否可用，并查看支持路由。 |
| `console` | 读取 Unity Console 日志、warning、error。 |
| `search` | 按名称或模式查找场景 GameObject。 |
| `modify` | 修改场景或 prefab 对象组件的 SerializedProperty。 |
| `run` | 控制 Play Mode：play、pause、resume、stop。 |
| `compile` | 请求 Unity 脚本重编译，可等待完成。 |
| `save` | 保存打开场景和 AssetDatabase。 |
| `components` | 给场景或 prefab GameObject 添加或移除 Component。 |
| `asset` | 搜索、创建文件夹、导入、删除、移动、复制 Unity 资产。 |
| `prefab` | `attach` 挂接嵌套 prefab；`group` 在 prefab 内新建容器节点并迁入指定直接子节点；`inspect` 只读检查层级、引用与 Widget/FancyView profile；`visual-import` 保存去脚本的可视化副本。 |
| `ugui` | 根据 Web visual-ui 与 asset-manifest 生成 UGUI prefab。 |
| `widget-metadata` | 可选：在 prefab 上生成 Oathx.Widget Metadata、Widget 组件和 VM 字段绑定；仅在安装 `com.oathx.widget` 且需要 UISystem 接入时使用。 |
| `screenshot` | 截取当前 Unity Game View。 |
| `call` | 在 Play Mode 中通过反射调用 CLR 方法。 |

常用命令：

```powershell
Library\UnityCLI\UnityCLI.exe ping
Library\UnityCLI\UnityCLI.exe compile --wait
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots/GameView.png
Library\UnityCLI\UnityCLI.exe save
```

更多详细命令说明见：

```text
unity-cli/references/unity-cli-commands.md
```

## 从效果图到 UGUI prefab 的完整流程

这个流程用于让 Codex 通过 `imagegen` 生成 UI 效果图、确认切图、生成独立资源，然后调用 UnityCLI `ugui` 创建 prefab。

### 1. 生成布局或效果图

先让 Codex 使用 `unity-cli` 流程生成布局或效果图，并明确分辨率、布局、复用规则和交互需求。

推荐说法：

```text
使用 unity-cli 创建一个商店界面，先生成布局给我确认
使用 unity-cli 流程生成一个 1280x720 的背包界面效果图，我确认后再切图
```

注意：

- 效果图和 `design-master.png` 只能作为参考图。
- prefab 不能把整张效果图当背景直接贴上去。
- 按钮、框体、图标、蒙版等必须拆成独立、可复用的资源。
- 按钮文字、数量、名称、价格等必须是运行时文本，不应烘焙进按钮图片。

### 2. 用户确认效果图

Codex 生成效果图后，必须由用户确认。

未确认前，不进入切图流程。

### 3. 切图与 bbox review

确认效果图后，进入切图流程。

切图边界必须以设计图视觉为准，不能只用 HTML 节点 rect。需要时使用 bbox review：

```powershell
node C:/Users/Administrator/.codex/skills/unity-cli/scripts/bbox-review/bbox-review.mjs `
  --ui <UIName> `
  --design UNITY-CLI/<UIName>/design-master.png `
  --visual UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --layer-plan UNITY-CLI/<UIName>/layer_plan.json
```

切图完成后必须生成预览，并再次让用户确认。

未确认切图预览前，不调用 `ugui` 创建 prefab。

### 4. 生成 Web visual-ui

`extract-visual-ui` 不再随 Unity package 发布。请通过独立 npm 包或外部脚本工具安装，然后用该工具生成 `visual-ui.json` 和 Web 截图。

Web 原型和中间文件默认放在：

```text
UNITY-CLI/<UIName>/
```

提取 `visual-ui.json`：

```powershell
npx <extract-visual-ui-package> `
  --input UNITY-CLI/<UIName>/<UIName>.html `
  --ui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --png UNITY-CLI/<UIName>/<UIName>.web.png
```

HTML 必须声明设计分辨率，例如：

```html
<meta name="fui-design-resolution" content="1280x720">
```

### 5. 生成和检查 asset-manifest

`asset-manifest.json` 描述独立切图资源和 Unity 目标路径。

资源后处理命令：

```powershell
node C:/Users/Administrator/.codex/skills/unity-cli/scripts/process-ui-assets/process-ui-assets.mjs `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json
```

只验证不写文件：

```powershell
node C:/Users/Administrator/.codex/skills/unity-cli/scripts/process-ui-assets/process-ui-assets.mjs `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json `
  --dry-run
```

规则：

- `asset-manifest.json` 必须引用独立切图资源。
- 资源路径必须真实存在。
- `usedBy` 必须能对应 `visual-ui.json` 中的节点。
- 需要复用的按钮、框体、蒙版等资源只切一份，多个节点引用同一资源。

### 6. 调用 UnityCLI ugui 创建 prefab

正式创建 prefab 前必须先检查 bridge 和 Console：

```powershell
Library\UnityCLI\UnityCLI.exe ping
Library\UnityCLI\UnityCLI.exe compile --wait
Library\UnityCLI\UnityCLI.exe console --type error --count 80
```

先 dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe ugui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab `
  --dry-run
```

dry-run 无 issue 后，正式生成：

```powershell
Library\UnityCLI\UnityCLI.exe ugui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab
```

如果该 prefab 需要接入 Oathx.Widget / UISystem，且项目已安装并编译 `com.oathx.widget`，生成纯 UGUI 后再生成 Metadata：

```powershell
Library\UnityCLI\UnityCLI.exe widget-metadata <UIName> `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab `
  --dry-run

Library\UnityCLI\UnityCLI.exe widget-metadata <UIName> `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab
```

`widget-metadata` 会按通用命名规则绑定 VM 字段：

- `ButtonClick` 绑定同名 Button，例如 `closeButtonClick -> CloseButton`。
- `VM` 子字段绑定子 Metadata，例如 `rewardSlot -> RewardSlot`。
- 文本字段绑定同名或 `Text` 后缀文本节点，例如 `levelText -> LevelText`。
- `IContainer` / `VList<T>` 绑定同名或单数化后的 `FancyGridScrollView`，例如 `rewards -> RewardsGrid` 或 `RewardGrid`。
- `FancyGridClick` 绑定同名或单数化后的 `FancyGridScrollView`，例如 `premiumRewardClick -> PremiumRewardGrid`。

正式写入前必须先执行 `--dry-run`；如果输出存在 issue 或 `boundTargetCount` 少于必要字段数，先修正 prefab 节点命名、组件或 VM 字段。

`com.oathx.widget` 是可选包。没有安装该包时，`widget-metadata` 会返回 `ok=true`、`status=skipped`、`skippedReason=widget_metadata_unavailable`；这只表示跳过 Oathx.Widget Metadata 接入，不应中断 `ugui`、截图、保存等纯 UGUI prefab 交付流程。

生成后检查、截图、保存：

```powershell
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots/<UIName>_UGUI.png
Library\UnityCLI\UnityCLI.exe save
```

## 停止条件

遇到以下情况必须停止，不要继续正式生成 prefab：

- 用户尚未确认效果图。
- 用户尚未确认切图预览。
- `ping` 失败。
- `compile --wait` 失败。
- `console --type error` 有 error。
- `ugui --dry-run` 返回 issue。
- `asset-manifest.json` 资源缺失、尺寸不匹配或 `usedBy` 对不上。

## 关键约束

- 统一使用 `unity-cli`。
- 工作目录使用 `UNITY-CLI/<UIName>/`。
- `visual-ui.json` 必须由固定脚本提取，禁止手写。
- prefab 必须由 `Library\UnityCLI\UnityCLI.exe ugui` 创建或更新。
- 不要手写、直接编辑或脚本拼接 `.prefab`。
- 不要用整屏设计图作为最终 prefab 的整屏 UI Sprite。
- 美术位图资源由 `imagegen` 生成或修补，代码只做提取、后处理、预览和验证。

## 从任务结构直接创建预制件

新增 `prefab create --json <task-layout.json> --prefab <Assets/...prefab> [--dry-run] [--replace]`。与现有 `ugui` 流程独立，详见 [任务预制生成说明](Docs~/prefab-create.md)。

## UI 状态机和资源批量导入

- `asset refresh` 同步导入由 `ui-prepare` 写入的文件。离线图片处理完成后调用，再执行契约预检，避免依赖编辑器窗口焦点触发导入。
- `animator controller import-json --controller <Assets/...controller> --json <animation-controller.json> [--dry-run]` 接收 `animation-controller/1`。必须写入新路径；先校验所有 clip 引用、默认状态、参数和切换目标，再创建无业务脚本的状态机。支持单层或多层的平面状态、Any State、退出时间、切换时长及条件；不导入源脚本、行为或嵌套子状态机。
- 契约的定时状态预览支持单层状态机的默认 clip；默认状态没有 motion 时如实记录而不伪造采样。真实状态运行验证仍使用 `prefab verify`。
- `prefab motion-preview` 的 JSON 可附 `activePaths`（相对层级路径到布尔值），仅用于隔离渲染及新建 review scene 的初始页面，不改源 prefab 或用户场景。

### 删除指定的重复组件

`components remove --component "Oathx.Widget.Metadata[1]"` 删除同类型组件中的第二个（从 0 开始），支持场景与 `--asset` 预制目标。省略序号仍删除第一个；负数、无效或越界序号会报错，且不保存预制。用于保留有效绑定，仅清理额外的重复组件。
