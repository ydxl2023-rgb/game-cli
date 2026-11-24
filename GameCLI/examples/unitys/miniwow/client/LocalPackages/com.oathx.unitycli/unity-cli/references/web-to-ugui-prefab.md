# Web 原型生成 UGUI prefab

## 固定入口

通用 Web visual-ui 提取脚本位于：

```text
npx <extract-visual-ui-package>
```

正式 prefab 生成工具：`Library\UnityCLI\UnityCLI.exe ugui`。

C# 实现：当前项目 UnityCLI 包内的 `UnityCliWebVisualUguiPrefabService` / `ugui` 路由。

## 硬约束

- 设计分辨率必须来自原型 HTML 的显式声明；项目约定只用于校验，未知时先修正 HTML。
- 没有参考图时，基于功能需求和项目风格生成原创 Web 原型。
- Web、截图、`visual-ui.json` 必须放在项目根目录 `UNITY-CLI/<UIName>/` 或当前 unity-cli 工作流目录 `UNITY-CLI/<UIName>/`。同一 View 的 HTML、JSON、`asset-manifest.json` 和 `assets_png/` 必须在同一个目录族内保持一致。
- `visual-ui.json` 只能由固定提取脚本从 Web DOM 生成，禁止手写、拼接或人工编辑。
- `asset-manifest.json` 必须引用同目录下的 `assets_png/` 生产资源，并用 `usedBy` 映射到 `visual-ui.json` 节点；资源项显式提供 `path` 时必须使用该目标路径，未提供有效 `Assets/...` 路径时才回退到 `Assets/Resources/UI/<UIName>/...`。
- UI 根节点固定为全屏 Stretch；普通子节点由生成器根据相对父节点的位置推导左/中/右与上/中/下 Anchor，但 Pivot 默认固定为 `(0.5,0.5)`，并换算 `anchoredPosition` 保持视觉矩形不变。只有 ScrollView Content、特殊展开/旋转轴等明确语义才覆盖 Pivot。
- `Image.type` 默认 `Simple`。只有节点显式声明 `imageType: "sliced"` 且 Sprite 具有非零 `spriteBorder` 时才生成 `Sliced`；进度填充显式使用 `imageType: "filled"`。Button、Panel、InputField、Dropdown 等语义类型不得自动触发九宫格。
- prefab 输出路径必须按当前项目资源结构决定；不能固定照搬 demo 路径。
- Unity Editor 必须打开当前工程，UnityCLI bridge 必须可 `ping`。

## Web 原型分层

### 视觉参考层：不提取

不要加 `data-ui-id`：

- 背景图、天空、地面、山体、建筑、云、光效
- 角色立绘、怪物、武器、坐骑、场景物件
- 纯装饰边框、纹理、粒子、阴影、高光

### 可提取 UI 层：生成 prefab

需要加 `data-ui-id` 和 `data-ui-type`：

- 按钮、文本、面板、进度条、图标占位
- Input、Toggle 等可交互控件
- 需要 UI 绑定或运行态检查的元素

## 固定提取命令

```powershell
npx <extract-visual-ui-package> `
  --input UNITY-CLI/UIMobaHome/UIMobaHome.html `
  --ui UIMobaHome `
  --json UNITY-CLI/UIMobaHome/UIMobaHome.visual-ui.json `
  --png UNITY-CLI/UIMobaHome/UIMobaHome.web.png
```

原型 HTML 必须包含以下声明之一：

```html
<meta name="fui-design-resolution" content="1170x2532">
```

```html
<div data-design-width="1170" data-design-height="2532">
```

`--width/--height` 仅用于旧原型兼容或校验；如果与 HTML 声明不一致，提取脚本会停止。

默认输出：

```text
UNITY-CLI/UIMobaHome/UIMobaHome.visual-ui.json
UNITY-CLI/UIMobaHome/UIMobaHome.web.png
```

## UnityCLI ugui 固定顺序

从 Unity 工程根目录执行：

```powershell
Library\UnityCLI\UnityCLI.exe ping
Library\UnityCLI\UnityCLI.exe compile --wait
Library\UnityCLI\UnityCLI.exe console --type error --count 80
```

如果 Console 没有 error，先 dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe ugui UIMobaHome `
  --json UNITY-CLI/UIMobaHome/UIMobaHome.visual-ui.json `
  --manifest UNITY-CLI/UIMobaHome/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/UIMobaHome.prefab `
  --dry-run
```

dry-run 通过后正式生成：

```powershell
Library\UnityCLI\UnityCLI.exe ugui UIMobaHome `
  --json UNITY-CLI/UIMobaHome/UIMobaHome.visual-ui.json `
  --manifest UNITY-CLI/UIMobaHome/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/UIMobaHome.prefab
```

如果 prefab 要接入 Oathx.Widget / UISystem，且项目已安装并编译 `com.oathx.widget`，再生成 Metadata。正式写入前必须 dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe widget-metadata UIMobaHome `
  --prefab Assets/Resources/UI/Prefabs/UIMobaHome.prefab `
  --dry-run

Library\UnityCLI\UnityCLI.exe widget-metadata UIMobaHome `
  --prefab Assets/Resources/UI/Prefabs/UIMobaHome.prefab
```

`widget-metadata` 会按通用命名规则绑定 `ButtonClick`、文本、子 `VM`、`IContainer` / `VList<T>` 和 `FancyGridClick`。列表字段绑定同名或单数化后的 `FancyGridScrollView`，例如 `rewards -> RewardsGrid` 或 `RewardGrid`；如果 dry-run 有 issue，先修节点命名、组件或 VM 字段。

`com.oathx.widget` 是可选包。没有该包时，`widget-metadata` 返回 `ok=true`、`status=skipped`、`skippedReason=widget_metadata_unavailable`，只表示跳过 Oathx.Widget Metadata 接入；不要因此中断纯 `ugui`、截图和保存流程。

生成后验证并保存：

```powershell
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots/UIMobaHome_UGUI.png
Library\UnityCLI\UnityCLI.exe save
```

## 检查清单

- [ ] 分辨率来自原型 HTML，且与项目约定一致
- [ ] Web 原型区分视觉参考层和 UI 层
- [ ] `visual-ui.json` 由固定提取脚本生成
- [ ] JSON 中不包含背景、角色、装饰节点
- [ ] `ugui --dry-run` 通过，`issues` 为空
- [ ] 正式 `ugui` 输出 `assetsCopied`、`spritesReferenced`、`nodeCount` 等信息正常
- [ ] 需要接入 UISystem 且项目安装 `com.oathx.widget` 的 prefab 已执行 `widget-metadata --dry-run`，且 `boundTargetCount` 覆盖必要字段；缺少 widget 包时已记录跳过 Metadata 接入
- [ ] prefab 路径符合项目资源结构
- [ ] Unity Console 无 error
- [ ] 已截图验证 GameView，并执行 `save`

## EditMode 可用工具

- `Library\UnityCLI\UnityCLI.exe ugui <UIName>` — 从 Web 提取的 `visual-ui.json` 和 `asset-manifest.json` 生成 UGUI prefab。
  - `--json`：`visual-ui.json` 或 `<UIName>.visual-ui.recut.json` 路径。
  - `--manifest`：独立切图资源清单路径。
  - `--prefab`：输出 `.prefab` 路径（按项目资源结构决定）。
  - `--dry-run`：只验证不写入 prefab 和资源。
- `Library\UnityCLI\UnityCLI.exe widget-metadata <UIName>` — 可选：在已有 prefab 上生成 Oathx.Widget Metadata、Widget 组件和 VM 字段绑定；仅在安装 `com.oathx.widget` 且需要 UISystem 接入时使用。
  - `--vm`：可选 VM 类型名；未提供时从 Widget 泛型或 UI 名推断。
  - `--prefab`：目标 `.prefab` 路径；未提供时尝试读取 Widget 的 `ConfigAttribute.path`。
  - `--dry-run`：只验证绑定结果，不保存 prefab。
