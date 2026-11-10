# 数据库 UGUI 模板契约

UGUI 的唯一数据契约是现有 PostgreSQL `gamecli_ui.templates.payload`。不使用 `gamecli.ugui`、components/properties/references 或转换后的新持久化格式。`ugui-convert` 已移除。

## 读取入口

```powershell
unity-cli ugui --template SvLoading --project <Unity工程> --prefab Assets/Data/Widgets/Loading/Loading.prefab --dry-run --json
unity-cli ugui --template SvLoading --project <Unity工程> --prefab Assets/Data/Widgets/Loading/Loading.prefab --overwrite --json
unity-cli ugui --input template.json --assets-dir ./images --project <Unity工程> --prefab Assets/UI/Example.prefab --dry-run
```

`--template` 和 `--input` 二选一。数据库模式通过附带的 read-template.mjs 复用 integrations/codex/server/database.mjs 的 PostgreSQL/SSH 配置，使用参数化 SELECT 和只读 repeatable-read 事务读取原始模板行和图片。需要 Node.js 及现有 integration 的依赖。默认从 Unity 工程向上寻找 integrations/codex；其他工程可显式传 `--integration <该目录>`。不使用会补齐/丢弃字段的 validateTemplate API，不写数据库。

文件模式直接读取数据库整行 JSON 或 payload `{root,design,...}`。图片位于 `--assets-dir`，默认 JSON 同目录的 assets 文件夹；文件名必须为 `<image ID>.png`。节点的 image 对应 gamecli_ui.assets.id，正文来自 assets.body，导入前核对 SHA256。当前支持数据库中的 image/png。

## 现有记录结构

```text
record: id, category_id, group_id, name, description, version, payload, created_at, updated_at
payload:
  root                       模板根节点 ID
  design:
    schemaVersion: 1         数据格式版本，与 record.version 模板修订号不同
    rootId: Canvas           网页画布标识，不是模板根节点
    name, width, height
    nodes[]
  source / standard / editedFrom / limitations（可选，原样保留）
```

现有五个模板：ItemSlot v1、ScrollPanel v1、ConfirmDialog v1、InventorySystem v2、SvLoading v9。示例 `Examples/ConfirmDialog.template.json` 是原始数据库行的只读副本。

原始记录和元数据写入 `Library/GameCLI/UGUI/<请求ID>/source.json`，不改写成另一套格式。数据库模式另有 template.json。结果返回 templateId/templateVersion，区分数据库模板与本地草稿。

## 节点语义和当前映射

| 数据字段 | Unity 实现 |
|---|---|
| id / parent / name | ID 构建层级，name 创建对象；数组顺序保留兄弟顺序 |
| rectTransform | 直接恢复 anchorMin、anchorMax、pivot、anchoredPosition、sizeDelta |
| 缺少 rectTransform | x/y/width/height 按父节点局部左上角坐标还原，y 向下转换为 Unity y 向上 |
| localScale / rotation / active | 缩放、Z 轴角度、激活状态 |
| type=Container | RectTransform；无 transparent surface 的普通容器显示颜色背景 |
| type=Image | Image；带 uvRect 或 cover 的图片使用 RawImage |
| type=Text | UnityEngine.UI.Text，文字、颜色、字号、水平/垂直对齐 |
| type=Button | Button + Graphic，支持 interactable；已有 Text 子节点按原层级创建 |
| type=Progress | Slider，绑定名为 Fill 的直接子节点，应用 progress 静态值 |
| image / imageFit | 哈希图片引用，contain、cover、100% 100% |
| imageBorder / imageType | Sprite 边框和 simple/sliced，边框顺序 x/y/z/w=左/下/右/上 |
| uvRect | RawImage UV；重复采样 |
| color / opacity | Graphic 颜色与透明度 |
| role=frame / borderWidth | 4 个原生 UGUI Image 边条，不生成位图 |
| template=ScrollPanel | ScrollRect + RectMask2D；绑定 role=content 子节点 |
| animationHost | Animator 宿主，无凭空生成的动画曲线 |

系统界面根节点归一化全屏 Stretch，独立控件模板保留根节点尺寸；Prefab 不新增 Canvas，挂入项目 Canvas 使用。图片复制到 Prefab 同目录的 `<Prefab名>Assets`。生成资源优先沿用 assets.source_path 的原始英文文件名并统一转为小写；同名资源使用 _2、_3 等序号区分，以隔离各节点的 Sprite 导入设置。缺少来源时使用节点名加 texture（统一小写）。已有同名但内容不同的资源会在预检时拒绝覆盖。旧版哈希加节点序号的资源通过 AssetDatabase.MoveAsset 改名，保留 GUID。

`action` 是现有网页预演行为，当前只校验目标引用并在 warnings 提示，不创建运行时导航；`previewHidden` 仍是预演状态，Unity 以 active 为准。`source/standard/limitations` 保留为来源和限制说明，不执行其中的字符串。`imageTone=light` 暂无材质实现，会明确拒绝，不静默改变视觉。运行时业务绑定、自动加载进度、动画曲线不由静态模板推断。

## 验证和安装

无图片且 `surface: "transparent"` 的 Button 保留透明 Image 作为点击区域，其 Graphic alpha 必须为 0；子节点图片不受影响。不能用默认白色 Image 渲染透明按钮底板，否则禁用态会出现灰块。带图片按钮继续按原图片及颜色渲染。

将 `UnityEditor/Editor/GameCliUguiImporter.cs` 和 `GameCliUguiAnimations.cs` 一起安装到 Unity `Assets/Editor/GameCLI/` 并刷新；依赖 UGUI、Newtonsoft.Json，已在 Unity 2022.3 验证。CLI 和 Editor 使用 Library 文件队列进行请求/响应，所有 Unity 对象操作在主线程执行。

正式生成自动先预检。预检构建并销毁内存对象，不写 Assets；只写 Library 暂存文件。已有 Prefab 需 --overwrite。版本错误、重复 ID、根节点错误、环、悬空父/动作目标、未知节点类型、资源错误会失败。正式写入不是多文件事务，磁盘失败可能留下图片；请看错误结果后重试。

Tests/ugui-integration.ps1 验证数据库原始格式、老坐标、错误输入拦截、payload 文件入口、metadata 兼容及 dry-run 不改 prefab。Loading、ItemSlot、ConfirmDialog、ScrollPanel 已实际生成；InventorySystem 预检通过并报告其 action 限制。


## design.animations（SvLoading v9）

数据库仍使用 design.schemaVersion=1，新增可选 animations 数组。无此字段的旧记录保持兼容；存在时必须验证，不能静默忽略。

- animation.id：动画唯一标识；hostNodeId 指向 animationHost=true 的节点。
- clip：name、frameRate、startTime、stopTime、loopTime、wrapMode、curves。
- curve：targetNodeId、component、property、keys、preWrapMode、postWrapMode。
- key：time、value、inTangent、outTangent、inWeight、outWeight、weightedMode。
- state：name、speed、default、cycleOffset、writeDefaultValues。
- source：来源追踪信息，原样留存在快照，不加载 sourcePath 指定的外部资产，也不执行 rawControllerDocuments。

实现按当前数据库验证器支持的范围恢复非加权 Euler Z（保留零 X/Y）和 RawImage UV X 曲线。绑定路径依据 targetNodeId 到 hostNodeId 的实际 Unity 层级重新计算；sourcePath 只用于追踪。当前每个 host 支持一个默认 state，片段从 0 秒开始；超出已支持范围明确报错。

生成独立 .anim、.controller 并绑定到 Prefab 的 Animator；保留已有资产 GUID，设置曲线切线、循环、帧率、状态速度和周期偏移。文件名使用 host ID 哈希，避免把展示名称当路径。当前结果增加 animationCount。

预检阶段在内存构建片段并采样验证，不写入 Assets。正式生成在保存前后对曲线做 Unity SampleAnimation 验证，与数据库 Hermite 曲线计算值比较。Loading v9 在 0、0.5、1、1.5 秒采样通过，包含 RateBg/Ball 三条 Euler 曲线及 RateBg/Arrow 的 UV 曲线。Tests/ugui-animations.ps1 覆盖错误宿主、目标越界、未知属性、加权曲线、乱序关键帧、非法时长及旧格式兼容。

数据库读取同时输出 asset-names.json（图片哈希到 source_path 的映射），哈希仅用于暂存、查重与完整性校验。文件导入可在 --assets-dir 中提供同名映射文件，原模板 JSON 不改写。
