# Miniwow 竖屏 Launch

## 需求与来源

用户确认本产品统一采用 **1080×1920 竖屏**，按小骑士启动 UI 结合数据库 Launch 模板制作项目专用 Launch。此约定已写入项目 Skill。

- GameCLI 项目：`p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3`（miniwow）。
- Unity 身份：`83c29c9f-21c1-4597-9691-6707ca146fa7`；工程为 `../../client`，绑定核验为 verified。
- 数据库基准：`SvStart` / SV Launch **v7**；完整只读记录 `template-source.json`。配置包为 null；本次组件不引用配置表，不以此推断其他模板没有配置依赖。
- 原作启动插画：微信包 `images/background.jpg`，720×1560，标题已经包含在原始插画中。它不是我们制作的整屏 UI 确认稿。进度条、状态、百分比及提示都是独立节点，未用整图叠加透明点击区。
- 进度条：FirstPackage 的 `Login_bg_3` 和 `Login_bg_4`；字体：`fangzhenglanting.ttf`。来源及哈希见 `asset-manifest.json`，字体来自资源目录，随 Launch 资产导入。

## 产品适配

Launch 根节点全屏 Stretch，CanvasScaler 参考尺寸 1080×1920。插画等比放大，使用原图顶部 720×1280 的区域，不横向拉伸；下方非关键暗部裁切。加载信息位于 y=1590–1847 范围，底边保留间距。此适配与原作 720×1560 画幅不同，未声称逐像素复刻原作运行截图。

数据库横屏布局改为竖屏插画与底部进度区；保留 XLauncher 普通组件及加载/更新/隐藏/仅背景接口。本版不照搬 SV 的旋转动画节点，没有增加模拟下载或自动增长进度。独立 Loading 与 Update 面板各自使用真实填充条，Update 默认不激活。

Prefab：`Assets/Data/Widgets/Launch/Launch.prefab`。资源：同目录 `LaunchAssets/`。通过正式 `unity-cli ugui`（数据库 payload 入口）先 dry-run 后生成，17 个节点、3 个图像来源、无 dry-run warning；由 `components` 和 `modify` 命令绑定 XLauncher 与字体。未手写 prefab YAML。

## 启动接入

`Assets/Scenes/App.unity` 的 `LaunchCanvas/Launch` 为实际 prefab 实例；XApp 的私有序列化 launcher 指向此实例。LaunchCanvas 排序 -10，使未来业务 Canvas 可以覆盖启动背景；所有 Launch Graphic 不拦截点击。

本产品 LocalPackages 的 XLauncher 增加两个可选的 loadingFill/loadingHandle 字段，将与更新面板相同的规范化进度应用于加载条；默认未绑定时仍兼容旧模板。NaN 归零，其余值 Clamp01，文字百分比按 RoundToInt 显示。

真实入口仍由 XApp 加载本地 XAppConfigure 并初始化资源；没有发起原作登录或下载请求。GamePlugin.Startup 调用 ShowLaunchBackground，因后续业务页面尚未实现，完成后保留插画，不假装进入游戏。

Scene 安装工具 `Assets/Editor/MiniwowLaunchSetup.cs` 只响应 `tools/launch/install.request`，校验两个项目身份后，通过 Unity Editor API 接入场景和设置；不生成或直接编辑 prefab。`bind-launch.ps1` 负责正式 CLI 绑定，绑定中断后使用 `-ResumeBindings` 避免重复添加组件。

XApp 增加可选私有序列化 `startupResolution`，本场景设为 1080×1920。非 Editor 的桌面启动采用此窗口尺寸，移动端保留原生屏幕输出与 PlayerSettings 竖屏方向；未配置时保留既有自动选择策略。这样不再让本产品桌面构建自动跳回显示器最大横屏。该非 Editor 分支已编译和代码检查，尚未进行桌面构建或真机验证。

## 项目管理与边界

Miniwow 的项目副本保存了竖屏设计、原图、字体和 Launch/XApp 代码；保存前检查 owner 和精确版本，备份 `project-before-v*.json`。`project-saved.json` 是此次项目内容快照，最终为 v3。没有修改/发布 SvStart 标准模板，没有提交或推送 Git。

## 验证证据

- `install-result.json`：场景与尺寸接入成功。
- 编译完成，Console error 查询为 0；生成后字段绑定曾因 Text 字段路径使用 `m_Font` 失败，已改为 `m_FontData.m_Font`；重复组件添加被 Unity 拒绝，没有产生第二个 XLauncher。
- 真正 Play 入口完成并显示背景；完整渲染帧为 1080×1920。
- `launch-loading.png`：显式调用 ShowLoading 和 UpdateProgress(0.375) 后的画面，显示 38%。这是接口测试输入，不代表真实下载速度或进度。
- 独立验收结果与未覆盖项以 `ACCEPTANCE.md` 为准；UnityCLI 窗口截图受 Game View 缩放裁切影响，完整帧通过 Unity ScreenCapture API 获取。

设备安全区、真机触摸和不同宽高比并非仅凭桌面截图即可证明；后续面向具体设备交付时补验证。当前无认证、错误重试按钮和下一个业务页面的产品契约，未擅自添加对应功能。

中文静态文字和测试画面已经制作；真实 XApp 启动仍沿用模板默认的 `Loading...` 与本地资源初始化状态，随后迅速进入背景态。本次没有把阶段提示扩展为完整的多语言配置系统；截图中中文状态由测试调用输入，不冒充已接入完整本地化启动流程。
