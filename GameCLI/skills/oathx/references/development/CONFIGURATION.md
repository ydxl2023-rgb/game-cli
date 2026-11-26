# 配置与启动：先沿用机制，再适配产品

## Development 的实际调用链

源码入口：[GamePlugin.cs](../../../../examples/unitys/development/client/Assets/Scripts/GamePlugin.cs)、[XAppConfigure.cs](../../../../examples/unitys/development/client/LocalPackages/com.oathx.core/Runtime/XAppConfigure.cs)。方法名中的历史拼写 `TryLoadAsyncConfiyFiles`、`InstallEntitys` 是源码事实，不要求新代码沿用拼写错误。

`XApp` 读取应用配置并建立基础运行环境，加载配置指定的插件。GamePlugin 的 Install 安装 SDK、RPC、UI 适配器；Startup 读取配置，完成后安装实体和系统。业务系统不能抢先使用尚未加载的 Excel 静态表。

配置入口来自 `GetAppConfigure()` 的 `cfgBundle` 和 `bundleSource`：

|来源|读取机制|关键约束|
|---|---|---|
|Local，Unity Editor|AssetDatabase 扫描配置目录中的 TextAsset|目录来自 cfgBundle；不是按配置类型拼固定文件路径|
|资源包|XRes.LoadBundleAsync → bundle.LoadAllAssets|资源索引与 Bundle 必须实际存在；发起加载不等于完成|

`ReadConfigFiles` 建立 Excel 字段索引，然后遍历资源：资源名映射 `Config.<asset.name>` → 反序列化为 `IConfigData` → `GetName()` 找字段 → `GetData()` 写入字段。`TryGetExcelFields` 的依据是生成代码中的 public/static/readonly 字段。不要把普通字段、属性或任意类型扫描替代这一契约。

资源文件名、配置类名、JSON 中的 name、Excel 字段名必须匹配。修改源表后走现有导表链路，不能只改运行时 JSON 或手写生成类。配置值校验属于对应业务配置/装配边界；通用读取器不应嵌入 FunctionSystem 的业务规则。

## Miniwow 修复后的对应关系

目标：[GamePlugin.cs](../../../../examples/unitys/miniwow/client/Assets/Scripts/GamePlugin.cs)。

|Development 机制|Miniwow 对应|必要差异|
|---|---|---|
|应用配置决定来源|TryLoadConfigFilesAsync|修正拼写，保留返回值含义：请求是否被接受|
|本地目录读取|LoadConfigDirectory，读取 cfgBundle|编辑器同步读取，不虚称异步；本地模式不作为 Player 的打包方案|
|Bundle 全资源读取|LoadConfigBundleAsync|等候协程结束；底层缺包不回调时仍报告失败|
|按资源名反序列化并映射 Excel|ReadConfigFiles、GetConfigTypeName、TryGetExcelFields|拒绝缺表、重复表及不兼容数据；未知非配置资产跳过|
|配置加载后安装实体/系统|InstallEntities → InstallSystems|配置资源读取失败会阻止装配，关闭后旧异步回调失效|
|系统表控制启动|FunctionSystemConfig|InstallSystems 按表的枚举顺序读取启动项，解析到类型就调用 Load。类型缺失时跳过，已解析系统的 Load 失败时抛出异常。整表规则属于配置验收。|

`Assets/Resources/XAppConfigure.asset` 的 cfgBundle 通过 Unity AssetDatabase 保存为 `Assets/Data/Jsons`。不再硬编码 `Assets/Data/Jsons/Excels/<类型名>.bytes`。配置资产及 `.meta` 必须进入源码包，否则干净安装仍会缺入口。

完整性检查不等于事务：当前沿用逐字段写入方式，读取中途失败可能已更新部分静态字段，但不会安装业务系统。下次成功启动重新加载全部字段。不得再把它描述为“完整验证后一次提交快照”。

Load(Type) 和 Load<T>() 成功时返回系统实例，失败时清理本次安装或激活并抛出异常。独立的前置判断之间留一个空行。动态装配在请求边界记录异常并通知请求方失败，以便会话恢复。

## 生命周期与命名

GamePlugin 负责装配与卸载，保留 `InstallUISystem/UninstallUISystem`、`InstallSystems/UninstallSystems` 等对称方法。GameUISender 是同文件中的独立适配类，和 GameUILoader 一样不要求独立文件；GamePlugin 不直接继承 UISender。

新建 ResourceConfigure 使用独立局部变量，多行初始化块前后按共享规则留空行。长条件与调用链按 OX019 的 120 列限制换行；这不是改变条件求值顺序的许可。

关闭按业务系统 → 实体 → UI 处理，使系统清理仍能访问必要设施。启动返回 true 仅表示已接受，不表示 Town、UI 或网络完成；使用业务就绪事件。异步完成不得在关闭后重新激活系统；优先使用现有取消句柄、加载取消和真实生命周期状态，不强制增加启动代次字段。只有无法取消且存在实际迟到回调时才评估额外机制。

## 不应照搬的历史行为

- Reference 的缺资源路径不充分报告失败；不能把回调未调用当成成功。
- `TryGetFunctionSystem` 历史实现可能抛异常；Try 接口应对普通未找到返回 false。
- `Load<T>` 与 `Load(Type)`、`Unload<T>` 与 `Unload(Type)` 不保证拥有相同 Install/Active/Shutdown 行为，必须读重载实现。
- SDK/RPC 属于参考工程能力，不是 miniwow 离线会话必须安装的依赖。
- 不复制无依据的强制 GC、测试数据或不符合新隔离约定的具体系统引用。

## 验证边界

本次本地配置启动、Login→Town/Home、返回/重复进入、插件重启及玩家控制回归已执行。代码检查另行保存报告。实际构建 Bundle 并在 Player 中启动尚未验证；不能将 Editor Local 成功描述成两种发布来源均验收通过。
