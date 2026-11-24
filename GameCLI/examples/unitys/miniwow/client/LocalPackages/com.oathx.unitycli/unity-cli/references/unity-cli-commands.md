# unity-cli bridge 命令说明

本文是 UnityCLI bridge 命令速查入口。用户输入 `unity-cli`、询问 `console` / `compile` / `ping` 等命令、要求调用 Unity Editor bridge、或需要精确执行 prefab 生成/验证命令时，优先读取本文。

## 通用格式

```powershell
Library\UnityCLI\UnityCLI.exe [--project <path>] [--format human|json] <command> [options]
```

- 从 Unity 工程根目录执行时通常不需要 `--project`。
- `--project <path>`：显式指定 Unity 工程根目录。
- `--format human`：默认人类可读输出。
- `--format json`：输出原始 bridge JSON，适合脚本解析。
- Unity Editor 必须打开当前工程。
- 如果命令返回路由不存在，通常表示 Editor 仍在运行旧 bridge；切回 Unity、保存脚本或执行 `compile --wait` 后重试。

## 命令总览

| 命令 | 用途 |
|------|------|
| `ping` | 检查 Unity Editor bridge 是否可用，并查看支持的路由。 |
| `console` | 读取 Unity Console 日志、warning、error。 |
| `search` | 按名称或模式查找场景中的 GameObject，并输出组件与属性摘要。 |
| `modify` | 修改场景对象上某个组件的 SerializedProperty。 |
| `run` | 控制 Play Mode：play、pause、resume、stop。 |
| `compile` | 请求 Unity 脚本重编译，可等待完成。 |
| `save` | 保存打开场景和 AssetDatabase。 |
| `components` | 给场景 GameObject 添加或移除 Component。 |
| `asset` | 搜索、创建文件夹、导入、删除、移动、复制 Unity 资产。 |
| `ugui` | 根据 Web visual-ui 与 asset-manifest 生成 UGUI prefab。 |
| `widget-metadata` | 可选：在已有 prefab 上生成 Oathx.Widget Metadata、Widget 组件和 VM 字段绑定；仅在安装 `com.oathx.widget` 且需要 UISystem 接入时使用。 |
| `particle` | 创建 ParticleSystem 特效 prefab，并可放置到当前打开场景中。 |
| `animator` | 创建/编辑 AnimatorController、AnimationClip、参数、状态、过渡、曲线和场景 Animator 绑定。 |
| `excel` | 读取、查询、校验和保守写入 `.xlsx` 配置表。 |
| `jira` | 访问 Jira：使用地址和访问令牌配置鉴权、查询任务、JQL 搜索、评论、流转、创建和更新 Issue。 |
| `screenshot` | 截取当前 Unity Game View。 |
| `call` | 在 Play Mode 中通过反射调用 CLR 方法。 |

## ping

检查 bridge 是否连通：

```powershell
Library\UnityCLI\UnityCLI.exe ping
```

常用时机：

- 每次开始调用 UnityCLI 前先执行。
- `ugui`、`screenshot`、`run` 等命令失败时确认 bridge 状态。
- 查看 `supportedRoutes`，确认当前 Editor 是否已加载最新路由。

## console

读取 Unity Console：

```powershell
Library\UnityCLI\UnityCLI.exe console [--count <n>] [--type all|log|warning|error] [--search <text|pattern>]
```

常用示例：

```powershell
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe console --type warning --count 40
Library\UnityCLI\UnityCLI.exe console --search MissingReference --count 20
```

参数：

- `--count <n>`：最多返回条数，默认 `20`。
- `--type all|log|warning|error`：日志类型过滤，默认 `all`。
- `--search <text|pattern>`：按文本或模式搜索 Console 内容。

流程规则：

- prefab 正式生成前后都应检查 `console --type error --count 80`。
- Console 有 error 时停止继续生成或保存，先修复错误。

## search

搜索场景对象：

```powershell
Library\UnityCLI\UnityCLI.exe search <name|pattern> [--count <n>]
Library\UnityCLI\UnityCLI.exe search --name <name|pattern> [--count <n>]
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe search UIShop --count 10
Library\UnityCLI\UnityCLI.exe search --name BtnClose --count 20
```

输出包含：

- GameObject 名称、instanceId、层级路径、场景名。
- `activeSelf` / `activeInHierarchy`。
- 组件列表、组件 instanceId。
- Transform 信息和可序列化 Inspector 属性摘要。

用途：

- 找到后续 `modify` / `components` 需要的 `--id` 或 `--path`。
- 检查 prefab 实例或运行态 UI 节点是否存在。

## modify

修改组件属性：

```powershell
Library\UnityCLI\UnityCLI.exe modify ((--id <id>|--path <path>)|--asset <prefab> --path <path>) --component <type> --property <path> --value <value>
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe modify --id 12345 --component RectTransform --property m_AnchoredPosition.x --value 10
Library\UnityCLI\UnityCLI.exe modify --path Canvas/UIShop --component CanvasGroup --property m_Alpha --value 1
Library\UnityCLI\UnityCLI.exe modify --asset Assets/Data/Widgets/Home/UIHome.prefab --path UIHome --component Metadata --property className --value VMHome
Library\UnityCLI\UnityCLI.exe modify --asset Assets/Data/Widgets/Home/UIHome.prefab --path UIHome --component Metadata --property metadatas.Array.data[0].components.Array.data[0] --value @BtnHome#Button
```

参数：

- `--id <id>`：目标 GameObject instanceId。
- `--path <path>`：目标场景层级路径。
- `--asset <prefab>`：目标 prefab 资产路径；和 `--path` 一起使用时，`--path` 表示 prefab 内层级路径。
- `--component <type>`：组件类型名。
- `--property <path>`：SerializedProperty 路径。
- `--value <value>`：写入值；ObjectReference 支持 `null`、instanceId、资产路径，或 `@GameObjectPath#ComponentType`。

注意：

- 优先用 `search` 找到准确对象和属性。
- 场景修改走 Undo，并会标记场景 dirty；prefab 修改通过 `PrefabUtility.LoadPrefabContents` 打开、保存和卸载。

## run

控制 Play Mode：

```powershell
Library\UnityCLI\UnityCLI.exe run <play|pause|resume|stop>
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe run play
Library\UnityCLI\UnityCLI.exe run pause
Library\UnityCLI\UnityCLI.exe run resume
Library\UnityCLI\UnityCLI.exe run stop
```

用途：

- 进入 Play Mode 做运行态检查。
- 暂停、恢复或退出 Play Mode。
- 配合 `call` 调用运行态方法。

## compile

请求脚本重编译：

```powershell
Library\UnityCLI\UnityCLI.exe compile [--wait]
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe compile --wait
```

参数：

- `--wait`：等待编译完成，最多约 5 分钟。

用途：

- 脚本改动后刷新 Editor bridge。
- 命令提示路由不存在时，先执行 `compile --wait`。
- prefab 生成前确认项目可编译。

## save

保存场景和资产：

```powershell
Library\UnityCLI\UnityCLI.exe save [--scene <asset path>]
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe save
Library\UnityCLI\UnityCLI.exe save --scene Assets/Scenes/SampleScene.unity
```

行为：

- 默认保存所有已打开且有资产路径的 dirty scene。
- 总会调用 `AssetDatabase.SaveAssets()`。
- `--scene <path>` 只保存指定已加载场景。

## components

添加或移除 Component：

```powershell
Library\UnityCLI\UnityCLI.exe components <add|remove> ((--id <id>|--path <path>)|--asset <prefab> --path <path>) --component <type>
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe components add --path Canvas/UIShop --component CanvasGroup
Library\UnityCLI\UnityCLI.exe components remove --id 12345 --component UnityEngine.CanvasGroup
Library\UnityCLI\UnityCLI.exe components add --asset Assets/Data/Widgets/Home/UIHome.prefab --path UIHome --component Metadata
```

参数：

- `<add|remove>`：动作。
- `--id <id>` 或 `--path <path>`：目标 GameObject。
- `--asset <prefab>`：目标 prefab 资产路径；和 `--path` 一起使用时，`--path` 表示 prefab 内层级路径。
- `--component <type>`：组件短名、完整名或 assembly-qualified 名。

行为：

- 通过 Undo 执行。
- 成功后标记场景 dirty。

## asset

Unity 资产操作：

```powershell
Library\UnityCLI\UnityCLI.exe asset <action> [options]
```

支持动作：

```powershell
Library\UnityCLI\UnityCLI.exe asset search <query> [--type <Type>] [--count <n>]
Library\UnityCLI\UnityCLI.exe asset create-folder --dst <Assets/Sub/...>
Library\UnityCLI\UnityCLI.exe asset import --src <disk path> --dst <Assets/...>
Library\UnityCLI\UnityCLI.exe asset delete --src <Assets/...>
Library\UnityCLI\UnityCLI.exe asset move --src <Assets/...> --dst <Assets/...>
Library\UnityCLI\UnityCLI.exe asset copy --src <Assets/...> --dst <Assets/...>
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe asset search UIShop --type GameObject --count 20
Library\UnityCLI\UnityCLI.exe asset create-folder --dst Assets/Resources/UI/UIShop
Library\UnityCLI\UnityCLI.exe asset import --src C:/Temp/icon.png --dst Assets/Resources/UI/UIShop/icon.png
```

规则：

- Unity 资产路径必须位于 `Assets/` 或 `Packages/`。
- Unity 资产路径使用正斜杠 `/`。
- `create-folder` 会创建中间文件夹。
- `delete` 可用 `--src`，也兼容 `--dst`。

## ugui

从 Web visual-ui 工作流生成 UGUI prefab：

```powershell
Library\UnityCLI\UnityCLI.exe ugui <UIName> [--json <visual-ui.json>] [--manifest <asset-manifest.json>] [--prefab <Assets/...prefab>] [--dry-run] [--no-scene-replace]
```

dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe ugui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab `
  --dry-run
```

正式生成：

```powershell
Library\UnityCLI\UnityCLI.exe ugui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json `
  --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab
```

参数：

- `<UIName>`：UI 名称。若未提供，必须提供 `--json`。
- `--json`：`visual-ui.json` 或 `<UIName>.visual-ui.recut.json`。
- `--manifest`：`asset-manifest.json`。
- `--prefab`：输出 prefab 路径。
- `--dry-run`：只验证，不写 prefab 和资源。
- `--no-scene-replace`：不替换当前打开场景中的实例。

硬规则：

- 创建或更新 prefab 必须先 dry-run。
- dry-run 有 `issue` 时禁止正式生成。
- `visual-ui.json` 禁止手写，必须由提取脚本生成。
- `asset-manifest.json` 必须引用独立切图资源，不允许用整屏设计图冒充 UI。

## widget-metadata

在已有 prefab 上生成 Oathx.Widget 的 `Metadata`、Widget 组件和 VM 字段绑定。这是可选包能力，仅在项目安装并编译 `com.oathx.widget` 且 prefab 需要接入 UISystem 时执行：

```powershell
Library\UnityCLI\UnityCLI.exe widget-metadata <UIName> [--vm <VMName>] [--prefab <Assets/...prefab>] [--dry-run]
```

dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe widget-metadata UIHome `
  --prefab Assets/Data/Widgets/Home/UIHome.prefab `
  --dry-run
```

正式生成：

```powershell
Library\UnityCLI\UnityCLI.exe widget-metadata UIHome `
  --prefab Assets/Data/Widgets/Home/UIHome.prefab
```

参数：

- `<UIName>`：Widget 类型名，例如 `UIHome`。
- `--vm <VMName>`：可选 VM 类型名；未提供时优先从 Widget 泛型参数推断，其次使用 `VM` + UI 名后缀。
- `--prefab`：目标 prefab；未提供时尝试读取 Widget 的 `ConfigAttribute.path`。
- `--dry-run`：只验证和报告绑定，不保存 prefab。

注意：

- 该命令由 `com.oathx.widget` 包实现，unity-cli 只做反射转发，因此不会让 `com.oathx.unitycli` 依赖 `com.oathx.widget`。
- `ugui` 生成的是纯 UGUI prefab；需要运行时 `UISystem` 绑定，且 `com.oathx.widget` 可用时，再执行 `widget-metadata`。
- 如果返回 `ok=true`、`status=skipped`、`skippedReason=widget_metadata_unavailable`，表示项目缺少可选的 `com.oathx.widget` 生成器；纯 UGUI prefab 交付流程应跳过该步骤并继续截图、保存，不应视为 `ugui` 失败。
- `ButtonClick` 字段按字段名绑定到 Button，例如 `buyButtonClick -> BuyButton`。
- `VM` 子字段按字段名绑定到子 Metadata，例如 `rewardSlot -> RewardSlot`。
- 文本字段按字段名绑定到同名或 `Text` 后缀文本节点，例如 `levelText -> LevelText`。
- `IContainer` / `VList<T>` 字段会绑定到同名或单数化后的 `FancyGridScrollView`，例如 `rewards -> RewardsGrid` 或 `RewardGrid`。
- `FancyGridClick` 字段会绑定到同名或单数化后的 `FancyGridScrollView`，例如 `premiumRewardClick -> PremiumRewardGrid`。
- `widget-metadata --dry-run` 的 `boundTargetCount` 应覆盖所有必须绑定的字段；存在 issue 时不要正式写入，先修正节点命名或组件。

## particle

创建 ParticleSystem 特效 prefab，并可选择把实例放到当前打开场景中：

```powershell
Library\UnityCLI\UnityCLI.exe particle create <Name> [options]
```

dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe particle create HitSpark `
  --prefab Assets/Resources/Effects/HitSpark.prefab `
  --shape cone `
  --start-color "#FFD36AFF" `
  --duration 1.2 `
  --loop false `
  --start-lifetime 0.45 `
  --start-speed 3 `
  --start-size 0.35 `
  --emission-rate 80 `
  --max-particles 120 `
  --fade-out `
  --dry-run `
  --no-scene-replace
```

正式创建并放入场景：

```powershell
Library\UnityCLI\UnityCLI.exe particle create HitSpark `
  --prefab Assets/Resources/Effects/HitSpark.prefab `
  --path Canvas/LoginView `
  --position 0,0,0 `
  --shape cone `
  --start-color "#FFD36AFF" `
  --duration 1.2 `
  --loop false `
  --start-lifetime 0.45 `
  --start-speed 3 `
  --start-size 0.35 `
  --emission-rate 80 `
  --max-particles 120 `
  --fade-out `
  --sorting-order 20
```

参数：

- `<Name>`：粒子特效名称，也是默认 prefab 名称来源。
- `--prefab <Assets/...prefab>`：输出 prefab 路径；未提供时默认写到 `Assets/Resources/Effects/<Name>.prefab`。
- `--path <parent scene path>`：把场景实例挂到指定父节点下；未提供时放到场景根部。
- `--position x,y,z`：场景实例的 localPosition。
- `--material <Assets/...mat>`：使用已有粒子材质。
- `--texture <Assets/...png>`：为粒子自动创建透明材质时使用的贴图。
- `--shape cone|sphere|hemisphere|circle|box`：发射形状，默认 `cone`。
- `--shape-scale x,y,z`、`--shape-radius <n>`、`--shape-angle <deg>`：形状模块参数。
- `--start-color <#RRGGBB|#RRGGBBAA>`：粒子初始颜色。
- `--duration <s>`、`--loop [true|false]`：主模块时长与循环。
- `--start-lifetime <s>`、`--start-speed <n>`、`--start-size <n>`：粒子生命周期、速度和大小。
- `--emission-rate <n>`、`--max-particles <n>`：发射率和最大粒子数。
- `--velocity-x min,max` / `--vx min,max`：启用 X 轴 Velocity over Lifetime。
- `--velocity-y min,max` / `--vy min,max`：启用 Y 轴 Velocity over Lifetime。
- `--velocity-z min,max` / `--vz min,max`：启用 Z 轴 Velocity over Lifetime。
- `--noise-strength <n>`、`--noise-frequency <n>`、`--noise-scroll-speed <n>`：启用 Noise 模块。
- `--rotation-z min,max`：启用 Rotation over Lifetime。
- `--size-over-lifetime start,end`：启用 Size over Lifetime。
- `--fade-out`、`--fade-start 0..1`：启用 Color over Lifetime 淡出。
- `--sorting-order <n>`：设置 ParticleSystemRenderer sortingOrder。
- `--dry-run`：只校验并返回将要创建的配置，不写 prefab 和场景。
- `--no-scene-replace`：只创建/更新 prefab，不替换或创建场景实例。

规则：

- 创建或更新特效 prefab 前建议先执行 `particle create ... --dry-run`。
- `--texture` 和 `--material` 路径必须是 Unity 资产路径，位于 `Assets/` 或 `Packages/` 下。
- 正式创建后应执行 `console --type error --count 80`、必要时 `screenshot` 和 `save`。

## prefab

把一个 prefab 作为嵌套子节点挂接到另一个 prefab 的指定节点下：

```powershell
Library\UnityCLI\UnityCLI.exe prefab attach `
  --parent-prefab Assets/Data/Widgets/Home/UIHome.prefab `
  --parent-path Background `
  --child-prefab Assets/Resources/Effects/UI/PetalDriftTopRight.prefab `
  --child-name PetalDriftTopRight `
  --local-position 1540,-130,0 `
  --local-scale 9.35,9.35,9.35 `
  --replace
```

dry-run：

```powershell
Library\UnityCLI\UnityCLI.exe prefab attach `
  --parent-prefab Assets/Data/Widgets/Home/UIHome.prefab `
  --parent-path UIHome/Background `
  --child-prefab Assets/Resources/Effects/UI/PetalDriftTopRight.prefab `
  --child-name PetalDriftTopRight `
  --dry-run `
  --no-scene-replace
```

参数：

- `--parent-prefab <Assets/...prefab>`：被写入的父 prefab 资产。
- `--parent-path <path>`：父 prefab 内部节点路径；可写 `Background` 或带根节点的 `UIHome/Background`。
- `--child-prefab <Assets/...prefab>`：要实例化并挂接的子 prefab 资产。
- `--child-name <name>`：挂接后的子节点名称；未提供时使用子 prefab 根名称。
- `--local-position x,y,z`、`--local-rotation x,y,z`、`--local-scale x,y,z`：挂接实例的本地变换；未提供的项保留子 prefab 默认值。
- 默认递归继承父节点 layer，适合把特效挂到 UI prefab 时自动变成 `UI` layer，确保 UI Camera 可渲染。
- `--layer <name|index>`：显式指定挂接实例及其所有子节点 layer。
- `--keep-layer`：保留子 prefab 原始 layer，不继承父节点。
- `--replace`：若同名子节点已存在则替换；默认开启。
- `--no-replace`：若同名子节点已存在则报错，不覆盖。
- `--dry-run`：只验证父路径、资产路径和参数，不写入 prefab。
- `--no-scene-replace`：只修改 prefab 资产，不同步当前打开场景中的同名 prefab 实例。

规则：

- `prefab attach` 使用 Unity 的 `PrefabUtility.LoadPrefabContents` 与 `PrefabUtility.InstantiatePrefab`，不要手写 `.prefab` YAML。
- 默认会在当前打开场景里查找和父 prefab 根同名的实例，并把同样的子 prefab 挂到对应内部路径，方便立即截图验证。
- 如果场景里不存在匹配实例，prefab 资产仍会正常写入，结果中的 `sceneInstanceReplaced` 为 `false`。
- 正式挂接后应执行 `console --type error --count 80`、必要时 `screenshot` 和 `save`。

## animator

通过 Unity Editor API 创建和编辑 AnimatorController 与 AnimationClip。不要手写 `.controller` 或 `.anim` YAML。

创建 Controller：

```powershell
Library\UnityCLI\UnityCLI.exe animator controller create `
  --controller Assets/Animations/Player.controller `
  --dry-run

Library\UnityCLI\UnityCLI.exe animator controller create `
  --controller Assets/Animations/Player.controller
```

新增或更新参数：

```powershell
Library\UnityCLI\UnityCLI.exe animator parameter set `
  --controller Assets/Animations/Player.controller `
  --parameter Speed `
  --type Float `
  --default 0

Library\UnityCLI\UnityCLI.exe animator parameter set `
  --controller Assets/Animations/Player.controller `
  --parameter IsGrounded `
  --type Bool `
  --default true
```

创建 AnimationClip 并写入 float 曲线：

```powershell
Library\UnityCLI\UnityCLI.exe animator clip create `
  --clip Assets/Animations/Idle.anim `
  --loop true

# 根节点曲线可以省略 --path；子节点曲线使用 --path Body/Arm。
Library\UnityCLI\UnityCLI.exe animator clip set-curve `
    --clip Assets/Animations/Idle.anim `
    --component Transform `
    --property m_LocalPosition.x `
    --keys "0:0,0.5:1,1:0"

Library\UnityCLI\UnityCLI.exe animator clip sample `
    --clip Assets/Animations/Idle.anim `
    --path PlayerRoot `
    --time 0.5
```

添加状态与过渡：

```powershell
Library\UnityCLI\UnityCLI.exe animator state add `
  --controller Assets/Animations/Player.controller `
  --layer "Base Layer" `
  --state Idle `
  --clip Assets/Animations/Idle.anim

Library\UnityCLI\UnityCLI.exe animator transition add `
  --controller Assets/Animations/Player.controller `
  --layer "Base Layer" `
  --from Idle `
  --to Run `
  --condition "Speed Greater 0.1" `
  --duration 0.15
```

绑定场景对象 Animator：

```powershell
Library\UnityCLI\UnityCLI.exe animator bind `
  --path PlayerRoot `
  --controller Assets/Animations/Player.controller `
  --dry-run
```

检查 Controller 或 Clip：

```powershell
Library\UnityCLI\UnityCLI.exe animator inspect --controller Assets/Animations/Player.controller
Library\UnityCLI\UnityCLI.exe animator inspect --clip Assets/Animations/Idle.anim
```

参数：

- `--controller <Assets/...controller>`：AnimatorController 资产路径。
- `--clip <Assets/...anim>`：AnimationClip 资产路径。
- `--parameter <name>`、`--type Float|Int|Bool|Trigger`、`--default <value>`：参数名、类型和默认值。
- `--layer <name>`：AnimatorController Layer 名称，默认通常是 `Base Layer`。
- `--state <name>`：状态名。
- `--from <state>`、`--to <state>`：过渡来源和目标状态。
- `--condition "Param Greater 0.1"`：Transition 条件，支持 `If`、`IfNot`、`Greater`、`Less`、`Equals`、`NotEqual`。
- `--component <type>`、`--property <property>`、`--keys "0:0,1:1"`：AnimationCurve 绑定类型、属性和关键帧。
- `--path <relative/path>`：`clip set-curve` 时表示 clip 内部绑定路径；省略表示根节点。`bind` 时表示场景对象路径。
- `--dry-run`：只校验并输出将要执行的操作，不写资产或场景。

规则：

- `.controller`、`.anim` 只能通过 `unity-cli animator` 或 Unity Editor API 修改，不要手写 YAML。
- 写入曲线时，`--component` 必须能解析到 Unity 类型，例如 `Transform`、`RectTransform`、`Image`。
- 正式创建或修改后应执行 `console --type error --count 80` 和 `save`。

## excel

读取、查询、校验和保守写入 `.xlsx` 配置表：

```powershell
Library\UnityCLI\UnityCLI.exe excel list --root ../config --count 20
Library\UnityCLI\UnityCLI.exe excel schema --table FunctionSystemConfig --root ../config
Library\UnityCLI\UnityCLI.exe excel query --table FunctionSystemConfig --root ../config --where "ID=SYS_HOME" --count 5
Library\UnityCLI\UnityCLI.exe excel cell --table FunctionSystemConfig --root ../config --key-field ID --key SYS_HOME --field ClassType
Library\UnityCLI\UnityCLI.exe excel set --table FunctionSystemConfig --root ../config --key-field ID --key SYS_HOME --field ClassType --value HomeSystem --dry-run
Library\UnityCLI\UnityCLI.exe excel validate --table FunctionSystemConfig --root ../config --key-field ID
```

动作：

- `list`：列出 `.xlsx` 表文件。
- `schema`：读取字段行；默认自动识别字段行，也可用 `--header-row <n>` 指定。
- `query`：按 `--where` 查询，支持 `= != > < >= <= contains startsWith endsWith`，可用 `and/or` 组合简单条件。
- `row`：按 `--row`、`--index` 或 `--key-field --key` 读取单行。
- `cell`：按行选择器加 `--field`/`--col` 读取单元格。
- `set`：修改单元格；正式写入前必须先跑 `--dry-run`。
- `insert`：用 `--values-json "{...}"` 追加一行。
- `delete`：按选择器删除行；正式写入前必须先跑 `--dry-run`。
- `validate`：检查空字段、重复字段；指定 `--key-field` 时检查主键空值和重复。

规则：

- 目前只支持 `.xlsx`，不支持旧 `.xls`。
- 配置表如果是“字段行 + 类型行 + 数据行”结构，会自动跳过字段行下一行的类型声明。
- `--table FunctionSystemConfig` 会自动尝试匹配 `FunctionSystemConfig.xlsx`、`propFunctionSystemConfig.xlsx`、`FunctionSystem.xlsx`、`propFunctionSystem.xlsx`。
- 写入采用保守模式：`set` 修改单元格，`insert` 追加行，`delete` 删除匹配行，不主动重排样式或复杂公式。
- 机器读取时使用 `--format json`。

## jira

访问 Jira REST API。该命令是 standalone CLI HTTP 客户端，不走 Unity Editor Bridge，只需要 Jira 地址和访问令牌。

配置：

```powershell
Library\UnityCLI\UnityCLI.exe jira config get
Library\UnityCLI\UnityCLI.exe jira config set --site http://jira.company.com:8080 --token <access-token>
Library\UnityCLI\UnityCLI.exe jira auth
```

也可以使用环境变量，优先级高于本机配置：

```powershell
$env:JIRA_SITE="http://jira.company.com:8080"
$env:JIRA_TOKEN="<access-token>"
```

常用命令：

```powershell
Library\UnityCLI\UnityCLI.exe jira get PROJ-123
Library\UnityCLI\UnityCLI.exe jira search --jql "project = PROJ AND assignee = currentUser() ORDER BY updated DESC" --count 20
Library\UnityCLI\UnityCLI.exe jira mine --count 20
Library\UnityCLI\UnityCLI.exe jira projects
Library\UnityCLI\UnityCLI.exe jira issue-types --project PROJ
Library\UnityCLI\UnityCLI.exe jira comment PROJ-123 --body "处理进展..."
Library\UnityCLI\UnityCLI.exe jira transitions PROJ-123
Library\UnityCLI\UnityCLI.exe jira transition PROJ-123 --to "In Progress" --dry-run
Library\UnityCLI\UnityCLI.exe jira create --project PROJ --type Task --summary "标题" --description "描述" --dry-run
Library\UnityCLI\UnityCLI.exe jira update PROJ-123 --summary "新标题" --dry-run
```

规则：

- 目前按通用 Jira REST API v2 实现。
- 鉴权统一使用 `Authorization: Bearer <token>`；优先从命令参数、环境变量、本机配置读取。
- 本机配置路径为 `%USERPROFILE%\.unitycli\jira.json`，不要提交到仓库。
- `config set --token` 会写入当前 Windows 用户目录下的本机配置，不写入 Unity 工程。
- 环境变量推荐使用 `JIRA_TOKEN`；同时兼容 `JIRA_API_TOKEN` 和 `JIRA_PAT`。
- `comment`、`transition`、`create`、`update` 支持 `--dry-run`，正式写入前建议先预演。
- 机器读取时使用 `--format json`。

## screenshot

截取当前 Game View：

```powershell
Library\UnityCLI\UnityCLI.exe screenshot [--dst <root-relative folder-or-file.png>] [--output <path>] [--super-size <n>]
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots/UIShop_UGUI.png
Library\UnityCLI\UnityCLI.exe screenshot --super-size 2
```

参数：

- `--dst <path>`：项目根相对输出目录或 `.png` 文件。
- `--output <path>`：兼容输出参数。
- `--super-size <n>`：Unity 截图倍率，默认 `1`。

默认输出：

```text
UnityCLI-Screenshots/GameView_<timestamp>.png
```

用途：

- prefab 生成后验证 Game View。
- 给用户回看 UI 实际效果。

## call

Play Mode 中反射调用 CLR 方法：

```powershell
Library\UnityCLI\UnityCLI.exe call --invoke-type <CLR type> --invoke-member <method> [--args-json <json array>] [--arg <token> ...]
```

示例：

```powershell
Library\UnityCLI\UnityCLI.exe call --invoke-type Game.DebugCommands --invoke-member OpenShop
Library\UnityCLI\UnityCLI.exe call --invoke-type Game.DebugCommands --invoke-member SetGold --args-json "[1000]"
Library\UnityCLI\UnityCLI.exe call --invoke-type Game.DebugCommands --invoke-member SelectTab --arg Equipment
```

规则：

- 需要 Play Mode。
- 静态方法使用 null target。
- 实例方法要求场景中只有一个匹配对象。
- `--args-json` 接收 JSON array。
- 可重复使用 `--arg <token>` 传递简单 token。

## 推荐调用顺序

Prefab 交付阶段：

```powershell
Library\UnityCLI\UnityCLI.exe ping
Library\UnityCLI\UnityCLI.exe compile --wait
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe ugui <UIName> --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json --manifest UNITY-CLI/<UIName>/asset-manifest.json --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab --dry-run
Library\UnityCLI\UnityCLI.exe ugui <UIName> --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json --manifest UNITY-CLI/<UIName>/asset-manifest.json --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab
# 仅当项目安装 com.oathx.widget 且该 prefab 需要 UISystem 接入时执行：
Library\UnityCLI\UnityCLI.exe widget-metadata <UIName> --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab --dry-run
Library\UnityCLI\UnityCLI.exe widget-metadata <UIName> --prefab Assets/Resources/UI/Prefabs/<UIName>.prefab
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots/<UIName>_UGUI.png
Library\UnityCLI\UnityCLI.exe save
```

运行态诊断阶段：

```powershell
Library\UnityCLI\UnityCLI.exe ping
Library\UnityCLI\UnityCLI.exe run play
Library\UnityCLI\UnityCLI.exe search <UIName> --count 20
Library\UnityCLI\UnityCLI.exe console --type error --count 80
```

## Web 工作流辅助脚本

这些不是 UnityCLI bridge 命令，但属于 `unity-cli` skill 的 Web→UGUI prefab 流程。

提取 `visual-ui.json`：

```powershell
npx <extract-visual-ui-package> `
  --input UNITY-CLI/<UIName>/<UIName>.html `
  --ui <UIName> `
  --json UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --png UNITY-CLI/<UIName>/<UIName>.web.png
```

bbox review：

```powershell
node C:/Users/Administrator/.codex/skills/unity-cli/scripts/bbox-review/bbox-review.mjs `
  --ui <UIName> `
  --design UNITY-CLI/<UIName>/design-master.png `
  --visual UNITY-CLI/<UIName>/<UIName>.visual-ui.json `
  --layer-plan UNITY-CLI/<UIName>/layer_plan.json
```

UI 资产后处理：

```powershell
node C:/Users/Administrator/.codex/skills/unity-cli/scripts/process-ui-assets/process-ui-assets.mjs `
  --manifest UNITY-CLI/<UIName>/asset-manifest.json
```

## 停止条件

- `ping` 失败。
- `compile --wait` 失败或超过等待时间。
- `console --type error` 返回 error。
- `ugui --dry-run` 返回 issue。
- `asset-manifest.json` 资源不存在、尺寸不匹配、`usedBy` 对不上 `visual-ui.json` 节点。
- 用户尚未确认效果图、切图预览或 bbox review。

## 继续阅读

- 完整 Web 到 prefab 流程：`@references/web-to-ugui-prefab.md`
- 资产生成与切图流程：`@references/asset-generation-workflow.md`
- 验证与排障：`@references/verification-and-troubleshooting.md`
