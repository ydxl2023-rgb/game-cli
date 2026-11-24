# Miniwow Home 第一版

2026-11-24。目标：从 App/Launch 正常启动，进入独立 Home 场景和竖屏主 UI。设计基准为 1080×1920；产品设计版本 Home-1，GameCLI 项目版本 5。

## 来源与适配

- 项目 `p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3`，Unity 身份 `83c29c9f-21c1-4597-9691-6707ca146fa7`，已通过 verified 绑定检查。
- 读取 SvHome v31 完整数据库 payload，保存在 `template-source.json`。根 Home0 包含 58 个源码文件和 propCommonText、propFunctionSystem 配置包。复用 Widget/VM、意图事件、打开回调、关闭与动画模式；按用户批准的竖屏小骑士方向重新设计。未安装 SV 的运营、聊天、货币、好友或活动系统，未发布标准模板。
- 小骑士 MainPage 导出声明为 720×1560，组件树未解码；本版不是原 UI 的像素复刻。营地背景、熊猫立绘、地图、面板和按钮取自已提取的独立资源。原路径、SHA256、尺寸与本轮用途见 `asset-manifest.json`。角色为静态插图，不是角色模型或 Spine 动画。
- 所有标题、按钮名与说明保存在 prefab Text，未生成静态文案 VM 属性。没有运行时文案查表、数值样例、开放条件或经济数据，因此未合入参考模板的两张业务源表；产品 config 保持原状。尚不支持语言切换。

## 实现与接口

1. `GamePlugin.Startup` 激活 HomeSystem，Launch 保留背景。
2. HomeSystem 使用 Core 的 XEventModule 生命周期，串行加载 `Assets/Scenes/Home.unity`（Additive），保留 App 中的 XApp、摄像机与 UISystem。
3. Home 场景持有 HomeEnvironmentCanvas 和 CLI 生成的 HomeEnvironment.prefab；背景等比铺满，立绘跟随背景挂点。
4. UISystem 加载 `Assets/Data/Widgets/Home/Home.prefab`，通过 UIHome/VMHome/Metadata 绑定。UI 准备成功后才调用 HideLaunch；加载失败保留 Launch，记录错误，不伪造成功。
5. UIHome 中定义 ReqHomeMap、ReqHomeCamp；Widget 通过注入的 UISender 发出意图，HomeSystem 调用页面方法更新 VM 显隐。冒险地图入口实际打开地图预览，返回按钮实际返回营地。地图明确显示“战斗关卡尚未开放”。
6. 停用关闭 UI、卸载 Home 场景；单协程依据最新请求处理异步场景操作，避免卸载与重开重叠。重复 Active 不重复注册。关闭时取消挂起的 Widget 加载。

第一版没有玩家身份、金币、存档、可玩关卡或奖励，因此不引入空 HomeEntity 或虚构初始数据。地图/营地切换属于局部展示状态，不是玩家进度。下一阶段加入真实玩家/关卡配置时，再由对应 Entity 管理业务数据，Home 只读取展示。不存在战斗、购买、网络或假登录副作用。

## 布局与制作

- `build_design.py` 只组合已有资源和布局，不绘制美术。`home-payload.json` 和 `environment-payload.json` 是正式 GameCLI UGUI 输入。
- 生成入口：`GameCLI/clis/unity-cli/bin/Debug/net8.0/unity-cli.exe ugui --input <payload> --assets-dir <assets> --project <client> --prefab <asset> --dry-run --json`；通过后去掉 dry-run 正式生成。已存在 prefab 时明确加 `--overwrite`。
- `bind-home.ps1` 使用目标工程 Library/UnityCLI 生成 Metadata，绑定安全区与背景比例，触发 request-driven 的 MiniwowHomeSetup。安装器用 Unity Scene/AssetDatabase API 创建 Home 场景与构建条目，保留已有 Home 场景，不手写 YAML。
- SafeArea 是独立适配组件；CanvasScaler 1080×1920，宽度匹配。营地底栏与地图底栏锚定安全区底部。HomeOpen 是 0.6 秒非循环淡入，原生 anim/controller 与 design.animations 同步生成。
- UGUI 导入器不自动绑定 fontAsset，因此 bind-home.ps1 将所有 Text 显式绑定到 Common/Fonts/fangzhenglanting.ttf；不是依赖系统字体回退。v5 补齐两个按钮文本的显式 RectTransform 和背景 uvRect 声明，与实际生成资产一致。
- Studio 保留原 Launch，新增 HomeEnvironment 环境层和 MiniwowHome 主 UI 两个根。Unity 将环境和 UI 合成实际画面；Studio 单独选择 Home 主 UI 时不会自动合成另一场景的背景。完整效果以 Unity 截图为准，不声称网页与场景合成已完全一致。

## 复验

打开 App.unity，点击 Play，等待 Home：活动场景应为 Home，Home UI 只有一个，Launch 不再显示。点击冒险地图，再点击返回营地。退出 Play 不保存运行状态。

`Assets/Editor/MiniwowHomeProbe.cs` 为编辑器专用探针，不进入玩家构建：

- Snapshot：实际场景、UI实例、Launch显隐、VM状态与动画读数。
- Click("MapClick"/"ReturnClick")：屏幕中心点射线命中，确认第一点击目标后发出 PointerDown/Up/Click；不是直接调用 VM。
- CloseHome / OpenHome：真实系统生命周期。
- RapidRestart：同帧 Detive→Active→Detive→Active，覆盖加载/卸载竞态。
- SetResolution(1080,1920)/(1080,2400)：Game View 实际渲染尺寸。验证后恢复 1080×1920。

父代理已验证实际启动、标准画幅的地图/返回射线点击、关闭重开；原始记录见 selftest.jsonl。该文件保留一次探针 JSON Vector2 序列化失败，失败前点击已生效，探针随后修复，后续两个点击均成功。长屏初测发现布局可改善，已将角色挂点与地图底栏改为相对锚点，需按最终资产再复验。

独立验收结果另见 ACCEPTANCE.md，不以开发自测代替独立结论。真机刘海、独立构建、AssetBundle 发布与语言切换尚未验证/实现。没有提交 Git、推送或发布标准模板。
