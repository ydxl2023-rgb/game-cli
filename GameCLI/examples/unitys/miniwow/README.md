# Miniwow Unity 工程

> 当前产品进展：已接入 **Launch → Home 场景与主 UI**，统一设计尺寸 **1080×1920**。Home 已替换为原包 `Building_sdc` 的地形、装饰与 6 类初始建筑，支持拖动查看、地图预览与返回；角色/NPC、建筑业务和战斗尚未实现。打开 `client/Assets/Scenes/App.unity` 并运行。项目规范见 [AGENTS.md](AGENTS.md)，最新说明见 [主场景还原记录](tools/main-scene/DELIVERY.md)及[资源用途索引](tools/main-scene/RESOURCE-INDEX.md)，前序记录见 [Home](tools/home/DELIVERY.md)、[Launch](tools/launch/DELIVERY.md)。以下内容记录最初的干净工程基线，不代表当前没有业务页面。

按 development 的组织方式建立，Unity 版本为 2022.3.62f2。

- `client/`：独立 Unity 客户端；打开 `Assets/Scenes/App.unity`。
- `config/`：配置表预留目录，无 development 业务数据。
- `server/`：服务端预留目录，无旧服务或凭证。
- `tools/`：基线来源记录、生成/验收日志。
- `research/`：保留原产品机制研究与提取资料。
- `wxecf0e106809a477d/`：原始小游戏参考包，未修改。

客户端只保留 XApp、XLauncher、最小 GamePlugin、UI loader、UISystem 层级、EventSystem 和相机。启动后隐藏入口并保持空闲，不安装任何业务 RuntimeSystem，不启动登录、RPC、热更新或远程版本下载。

七个 Oathx 包为 development 当前 branches/gamecli 工作树的独立快照；并非新的 SVN checkout，不含 .svn，不通过链接共享可写内容。来源及本地差异见 tools/baseline-packages.json。保留 development 使用的 DOTween、LitJson、Mono.Cecil，UnityCLI 桥接和 UGUI 导入器；URP 14.0.12 为 UISystem 源码依赖。

已删除上一版 Expedition DEMO 的代码、场景、UI、生成器、构建及专用测试产物。旧 DEMO 不再是当前工程入口。

生成入口为编辑器方法 CleanProjectSetup.Build。运行验证入口为 CleanProjectValidation.Run，验证结果见 tools/runtime-validation.txt（存在且为 PASS 才表示通过）。

## 验证记录

- 干净场景生成、Unity 编译成功（tools/clean-setup.log，退出码0）。
- 独立静态审查通过：七包519个有效文件与 development 一致；仅App场景；XApp/XLauncher引用完整；配置无业务插件、服务地址、热更新和远程存档。
- 图形Editor日志已记录 MINIWOW_CLEAN_ENTRY_READY，说明最小插件实际启动成功。
- 首轮自动验证因 Unity Package Manager 在线搜索网络错误标记失败；不能据此宣称两轮运行验收通过。复验启动受管理员运行提示影响，等待交互式Editor继续。
- 尚未完成 CLI 截图和连续两轮 Play Mode 验证。详见 tools/clean-runtime-graphics.log 和 tools/runtime-validation.txt。
