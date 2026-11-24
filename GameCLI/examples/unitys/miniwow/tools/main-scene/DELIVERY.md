# 小骑士主场景还原

本轮将 Home 的临时营地背景替换为原包 `Building_sdc` 的原生 Unity 场景资产。打开 `client/Assets/Scenes/App.unity` 运行，Launch 完成后进入 Home。设计尺寸仍为 1080×1920。

## 已实现

- 原场景 666 个层级节点，保留名称、父子关系、本地 TRS 和显隐。
- 按 `tbbuilding.InitialModelPath` 给 6 个 Model 挂点安装初始建筑：武器店、饰品店、裁缝店、旅店、医院、拍卖行。此为初始/未解锁外观选择，不表示玩家已有这些业务状态。
- 合计 768 节点、434 个网格渲染器、263 个独立 Mesh、19 个 Material、11 张 Texture。所有 293 个导入资产已校验原台账 ID、源包 SHA256、输出哈希和使用节点。
- 原相机角度、25° FOV、145.9 距离；启动焦点根据 `BuildingPlayer.initPos + (0,3,0)` 与 `CameraController.Focus` 的投影关系恢复。
- 鼠标/单指拖动查看主城，鼠标滚轮缩放。展示移动限制在初始位置周围 45 单位、距离 100–180；这是本项目查看场景的控制边界，不是原作玩法规则。
- Home 改为紧凑标题和地图入口，保留地图预览/返回及入场动画；旧 HomeEnvironmentCanvas 禁用，未删除历史展示资源。

## 证据与索引

- `scene.json`：原始层级、渲染数据、动态建筑引用、未导入组件。
- `asset-manifest.json`：源包、serialized file、PathID、原台账 ID 和中间输出 SHA256。
- [RESOURCE-INDEX.md](RESOURCE-INDEX.md)：按资源名查 Unity 文件与用途。
- `unity-imports.json`：实际 Unity 路径、哈希、完整使用节点路径。
- `research/resource-catalog/catalog.sqlite` 新增 `scene_imports`；PostgreSQL 对应 `miniwow_resources.scene_imports`，不覆盖 annotations，不上传二进制。
- 原代码证据位于 `research/code-study/managed/decompiled/HotUpdate/`：`Zombie/BootModule.cs`、`BuildingGameMode.cs`、`BaseBuildingController.cs`、`BuildingPlayer.cs`、`CameraController.cs`、`Utility.cs`；配置格式见 `Zombie.Table/CfgBuilding.cs`。

## 复现

在 miniwow 根目录运行 `python tools/main-scene/decode_buildings.py`，然后 `python tools/main-scene/export_scene.py`。需要既有原包、research/python-libs/UnityPy 和资源台账。

Unity 编译后，在非 Play 状态写入 `tools/main-scene/install.request`，编辑器安装器通过 AssetDatabase/EditorSceneManager 创建原生资产并保存 Home。检查 `install-result.json`。随后运行 `python tools/main-scene/index_imports.py`，必要时运行 `node tools/main-scene/sync-imports.mjs` 更新数据库索引。

UI 通过 tools/home/build_design.py 与正式 unity-cli ugui 流程生成，再绑定 Home Metadata、字体和安全区。Studio 只保存产品设计、源码和资源清单；3D 场景不能在当前网页 UI 预览器中等价显示，独立导出还需本地 `Assets/Data/Scenes/Home/Recovered` 完整资产目录，不能把网页源码包当成完整场景包。

## 准确性边界

这是主场景静态空间与初始建筑的还原，不是完整原作运行逻辑。没有导入原 MonoBehaviour，也没有实现角色/NPC Spine、角色移动、建筑交互/升级、粒子、动画、碰撞、导航、昼夜、服务端状态。

原 `Zombie/UberShader` 采用兼容 shader 重建基础贴图、颜色、透明裁剪和混合；未恢复原光照/阴影/特效，因此不宣称像素一致。场景层统一为 Default，只有恢复的主相机渲染；原辅助相机和灯光未启用。

本轮验证结果及独立审查见 [independent-review.md](independent-review.md)。没有提交 Git、发布标准模板或连接原作服务器。
