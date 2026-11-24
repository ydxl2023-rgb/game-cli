# 主场景独立静态验收

日期：2026-11-24。审查者：scene_acceptance（未参与实现）。目标 miniwow，Unity ID 83c29c9f-21c1-4597-9691-6707ca146fa7。

- passed：768 个节点 ID 唯一，父节点引用完整。
- passed：293 条 Mesh/Material/Texture 导出记录输出哈希一致；263 个网格索引范围与三角形数量有效。
- passed：6 个动态初始模型挂点原本为空，无重复模型；模型根 TRS 为 identity，符合原 BaseBuildingController.SwitchModel 的实例化方式。
- passed：选择初始资源有 tbbuilding 配置与 BuildingGameMode.CreateBuildingModels 锁定分支依据；不代表任意玩家存档的建筑状态。
- passed：本地原生依赖 263 Mesh、19 Material、11 Texture，场景 GUID 引用无缺失（Unity 内置 GUID 除外）。
- incomplete：本轮未独立操作 Editor，编译、实际启动、视觉、点击、生命周期等待串行运行验证。
- incomplete：原 UberShader 未恢复，当前为贴图/顶点色兼容 shader；原灯光、阴影、NPC、Spine、粒子、动画、碰撞导航、建筑业务不在静态还原交付内。
- not-applicable：标准 SvHome 的货币、玩家档案、运营入口等未获本轮产品实现授权，不应强行套入主场景。

未发现静态资产链阻断缺陷。此结论不是完整运行验收或原作像素一致证明。

审查冻结哈希：
- `client/Assets/Scenes/Home.unity`: `2480063e0d25480c022d037805c27127a6b06434dbb10f27112b4dddc08aee6a`
- `client/Assets/Editor/MiniwowMainSceneSetup.cs`: `4c5a5ebc027b8d73997db3734b7cf8b10d7f76fcff842e1556b8dbd19d03f851`
- `client/Assets/Scripts/Systems/Home/RecoveredScene.shader`: `e959a93357acdb372f5a65db5057ace8c5fab0a3d225fe87c6a1bf808ad72401`
- `tools/main-scene/scene.json`: `2ee42ba4f4e64dcfddea8df668b1be45d4a032f7c328c1d374f71b6dbbaa3c0a`
- `tools/main-scene/asset-manifest.json`: `0b47bdc33c06dfb4dc4533fcc6e8851b383ddcc974d97e8f6f695d04bd91426f`

## 本轮独立运行补验

证据 independent-runtime.jsonl、final.png。实际 1080×1920 Home：地图和返回通过射线真实命中；RapidRestart 后 Home 实例恰好 1，Launch 隐藏，营地显示，alpha=1。截图已检查，无缺材质粉色、黑屏或缺字。

Pan(Vector2,Vector2) CLI 参数类型不受支持，拖动验证 incomplete；不以方法存在代替输入验证。发现 HomeSceneCamera 的公共 Pan 方法位于私有方法之后，已交开发调整。完整标准 verify code/prefab 本轮未独立执行，仍 incomplete。

最新导出器哈希：`4faa08f62530fcad3c22a71e2227cce9c47c29dc26de7349fdc9db4c4a2a3029`。

## 相机操作补验（08:56）

- passed：方法声明顺序已修复，生命周期 → public Pan → private helpers。
- passed：通过 Editor PanScene 调用实际 Pan，200 像素水平输入使相机移动 3.906278 单位；超大输入后与原点距离为 45（容差 0.001），符合夹持范围。
- passed：RapidRestart 后相机恢复原点 (-62.4568939, 0, -24.4369049)，Home 实例仍为 1，营地主视图和 1080×1920 正确。
- incomplete：这是相机操作方法验证，不是操作系统真实鼠标/触屏拖拽验证；滚轮输入、UI 遮挡手势仍未独立验证。

原始证据 independent-pan.jsonl；final.png 已刷新为恢复后的初始视图。
- `client/Assets/Scripts/Systems/Home/HomeSceneCamera.cs`: `8ffe7a43062f80643260dc97a35aaa688c45b4ca22c46986869ca7a12f94beda`
- `client/Assets/Editor/MiniwowHomeProbe.cs`: `86b87be4a164e3f71461d532492eb3928141e7a4c112866f686eb7c98ad284ec`
