# Unity CLI

Unity 工具链的独立命令行入口。UnityPlugin 提供 `ping`、Console 日志读取和 `compile`；UguiPlugin 提供数据库驱动的 prefab 生成。

## 前提

- .NET 8 SDK 或更新版本用于构建，.NET 8 runtime 用于运行。
- Unity 项目已安装参考包 `com.oathx.unitycli`，并在 Editor 中打开、完成编译。本目录当前提供客户端，Unity Editor 服务端复用参考包。
- 参考包位置：`G:/gameai/ugame-ai-cli/app/client/LocalPackages/com.oathx.unitycli`。
- 服务端自动生成 `Library/UnityCLI/endpoint.json`。不要手工填写或提交其中的会话 token。

## 使用（PowerShell 7）

```powershell
dotnet build ./UnityCLI.csproj
dotnet run --project ./UnityCLI.csproj -- ping --project G:/path/to/unity-project
dotnet run --project ./UnityCLI.csproj -- console --project G:/path/to/unity-project --type error --count 80 --json
dotnet run --project ./UnityCLI.csproj -- logs --project G:/path/to/unity-project --search '*Exception*'
dotnet run --project ./UnityCLI.csproj -- compile --project G:/path/to/unity-project --wait --json
dotnet publish ./UnityCLI.csproj -c Release --no-self-contained
```

发布后直接使用 `unity-cli.exe`。`--project` 省略时从当前工作目录向上查找端点。`console` 与 `logs` 等价；默认最近 20 条，范围 1–200，类型支持 all/log/warning/error。过滤与日志读取由 Editor 执行，读取的是 Console 条目而非磁盘 Editor.log。

`--json` 对成功、服务端错误和客户端异常均输出 JSON。退出码：0 成功、1 连接或服务错误、2 参数错误。`UNITYCLI_REQUEST_TIMEOUT_SECONDS` 可设置请求超时（5–300 秒，默认 5 秒）。

## 源码插件结构

```text
UnityCLI.csproj                唯一项目，所有插件源码统一编译
Program.cs                    命令分发、冲突校验、错误输出
PluginRegistry.cs             插件注册入口
Abstractions/IPlugin.cs        统一插件接口
Plugins/UnityPlugin/           ping、console、logs、compile 及桥接通信源码
Tests/smoke.ps1                命令入口验证
```

每组功能实现 IPlugin，包含 Name、Commands、Help 和 ExecuteAsync(command, args)。插件自行解析领域参数。

新增 JiraPlugin、PrefabPlugin 时，在 Plugins 下创建对应源码目录，实现 IPlugin，然后在 PluginRegistry.Create() 中添加实例。移除注册即可禁用该组命令；删除插件时同时移除其源码目录与注册。修改后重新编译主工程。没有独立插件 DLL、运行时目录扫描或动态程序集加载。

UnityPlugin 和 UguiPlugin 已实现；JiraPlugin、PrefabPlugin 尚未实现。Unity Editor 服务端仍复用参考包。Protocol、CLIClient、ResponseFormatter 和 Commands 从参考包提取并保留原通信协议。

验证：执行 `dotnet build`，然后用 PowerShell 7 运行 `./Tests/smoke.ps1`。

`compile` 刷新资源并请求 Unity 脚本编译；`--wait` 最多等待 5 分钟，期间重新读取端点并重试域重载造成的临时连接中断。等待完成表示 Editor 已停止编译，不代表没有编译错误；随后用 `console --type error` 检查诊断。单次通信仍受 `UNITYCLI_REQUEST_TIMEOUT_SECONDS` 限制。

## UguiPlugin：直接使用数据库模板

UGUI 直接消费现有 `gamecli_ui.templates.payload`（root + design.schemaVersion=1 + nodes），按 image ID 读取 assets 表。详细字段与限制见 [数据库格式](docs/ugui-format.md)。不需要转换成新格式，原始数据库记录和元数据保存在请求目录。

```powershell
# 从 Unity 工程根目录执行；预检通过后去掉 --dry-run，已存在输出添加 --overwrite
.\Library\UnityCLI\unity-cli.exe ugui --template SvLoading --project . --prefab Assets/Data/Widgets/Loading/Loading.prefab --dry-run --json
```

也支持 `--input <数据库行或payload.json> --assets-dir <图片目录>`。数据库模式复用 integrations/codex 的配置和 Node.js 依赖，可用 --integration 指定目录。安装时将 UnityEditor/Editor/ 下的两个 GameCliUgui*.cs 复制到 Unity Assets/Editor/GameCLI，并刷新编译。

支持数据库五种节点类型及现存 Loading 布局字段；兼容没有 rectTransform 的老节点。已生成 SvLoading v9、ItemSlot v1、ConfirmDialog v1、ScrollPanel v1；InventorySystem v2 预检通过。action 预演行为暂不生成运行时逻辑，会返回 warnings。

执行 Tests/ugui-integration.ps1 进行真实 Editor 格式验证；Examples/ConfirmDialog.template.json 是原始数据库样例。

Loading v9 的 design.animations 已支持：生成 Clip/Controller 并绑定 Animator，恢复球旋转和箭头 UV 动画。执行 Tests/ugui-animations.ps1 验证动画格式及采样；旧模板仍兼容。

## AnimatorPlaugin：UI 动画编辑

源码插件实现 IPlugin，注册入口为 `animator`，通过现有 `/animator` Editor 桥接操作资源。`animator --help` 列出完整命令。所有写操作建议先加 `--dry-run` 验证，再去掉该参数执行；`--json` 控制输出，`--input` 指定导入文件，两者可同时使用。

```powershell
unity-cli animator clip create --clip Assets/Data/Widgets/MyUI/Slide.anim --duration 1 --loop true --dry-run
unity-cli animator clip set-curve --clip Assets/Data/Widgets/MyUI/Slide.anim --path Panel --component UnityEngine.RectTransform --property m_AnchoredPosition.x --keys 0:0,1:100 --dry-run
unity-cli animator controller create --controller Assets/Data/Widgets/MyUI/UI.controller --dry-run
unity-cli animator state add --controller Assets/Data/Widgets/MyUI/UI.controller --state Slide --clip Assets/Data/Widgets/MyUI/Slide.anim --dry-run
unity-cli animator inspect --clip Assets/Data/Widgets/MyUI/Slide.anim --json
```

支持 controller create/import-json、parameter add/set/remove、clip create/set-curve/sample/import-json/export-json、state add、transition add、bind、inspect。曲线路径相对于 Animator 根节点；bind/sample 的路径指向场景对象，sample 会改变场景对象姿态。完整曲线交换使用桥接的 animation-clip/1 JSON（export-json 输出位于 data.details）；controller 导入使用 animation-controller/1。能力和校验由安装的 Editor 桥接提供，不支持的组件或属性会返回错误。

通用参数 `--project` 指定 Unity 工程，省略时向上查找；`--loop` 必须显式传 true/false。退出码遵循 0 成功、1 桥接失败、2 参数错误。参数验证测试：`Tests/animator.ps1`。

## ExcelPlugin：Excel 配置表操作

源码插件 `ExcelPlugin : IPlugin` 注册 `excel` 命令，仅支持 `.xlsx`。创建/删除整个工作簿由 CLI 本地执行，不需要 Editor；表格内容操作复用 Unity `/excel` 桥接，需要工程已打开。创建仅生成一个工作表和表头，不覆盖已有文件。

```powershell
unity-cli excel create --file Config/Items.xlsx --headers-json '["Id","Name","Price"]' --sheet Items --dry-run
unity-cli excel schema --file Config/Items.xlsx --header-row 1
unity-cli excel insert --file Config/Items.xlsx --header-row 1 --values-json '{"Id":1,"Name":"Sword","Price":100}' --dry-run
unity-cli excel set --file Config/Items.xlsx --header-row 1 --row 2 --field Price --value 120 --dry-run
unity-cli excel query --file Config/Items.xlsx --header-row 1 --where 'Price>=100' --json
unity-cli excel delete --file Config/Items.xlsx --header-row 1 --row 2 --dry-run
unity-cli excel delete-file --file Config/Items.xlsx --dry-run
```

去掉 `--dry-run` 执行写入。`delete` 删除行，`delete-file` 删除整个工作簿；文件删除不会移入回收站。`list` 查找配置表，`schema` 读取字段，`row`/`cell` 查询单行/单元格，`validate` 检查表格。可用 `--key-field Id --key 1` 按主键定位，`--col A` 按列定位；详细参数见 `excel --help`。新建表使用 `--header-row 1`，既有游戏配置表按实际表头行设置。内容编辑的公式、样式等保护范围以现有 Editor 桥接为准，不提供公式计算或任意工作表结构编辑。

建议显式传 `--project`，确保本地文件操作与 Editor 使用相同工程根目录。验证脚本：`Tests/excel-integration.ps1 -Project <Unity根目录>`，使用 Library 下唯一临时文件并自动清理。
