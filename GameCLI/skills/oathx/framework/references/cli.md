# CLI 与 Editor 桥接

先读 GameCLI 的 `clis/unity-cli/README.md` 和 `docs/ugui-format.md`；命令以当前 `--help`、源码和 Editor 返回能力为准。

## 必须区分三部分

1. **Editor 桥接包** `com.oathx.unitycli`，运行在目标 Unity Editor 中，生成 `Library/UnityCLI/endpoint.json`。
2. **本仓库客户端** `clis/unity-cli`，当前提供 ping、compile、console/logs 和数据库 UGUI 导入。
3. **完整参考客户端**，提供 search、modify、components、asset、widget-metadata、excel、run 等。不能把完整客户端的命令直接套到本仓库客户端。

当前参考桥接来源是 `G:/gameai/ugame-ai-cli/app/client/LocalPackages/com.oathx.unitycli`，完整客户端为同工程的 `Library/UnityCLI/UnityCLI.exe`。这些只是当前机器已验证的位置，不是可移植安装地址。其他机器先读取任务提供的来源，或定位已授权参考 checkout；缺失时报告具体依赖，不复制指向 G 盘的 manifest。

## 安装步骤

- 将已确认来源的桥接包安装/复制到目标 `LocalPackages/com.oathx.unitycli`，保留包内部 meta，排除 `.svn` 等来源工作副本元数据，记录来源版本；已有目标先比较，不覆盖本地改动。也可使用用户明确提供的有效包引用。
- manifest 合并 `"com.oathx.unitycli": "file:../LocalPackages/com.oathx.unitycli"`。不要自动安装无关 Unity MCP、浏览器插件或外部服务。
- 构建本仓库客户端，发布到独立目录，避免与完整参考客户端互相覆盖：

```powershell
dotnet publish <gamecli-root>/clis/unity-cli/UnityCLI.csproj -c Release --no-self-contained -o <project>/Library/GameCLI/CLI
```

  检查 .NET 8 SDK/runtime；发布需完整输出，包括 DLL、deps、runtimeconfig 和 `read-template.mjs`，不能只复制 exe。
- 完整参考客户端也部署完整运行文件到单独位置（如 `Library/UnityCLI`），或以明确路径调用已验证来源。报告两个客户端路径和各自能力。
- 安装本仓库 `clis/unity-cli/UnityEditor/Editor/GameCliUguiImporter.cs` 与 `GameCliUguiAnimations.cs` 到目标 `Assets/Editor/GameCLI`，避免项目内重复定义同名类型。已有副本比较差异后更新，并保留 meta。
- 打开目标 Unity Editor，等待初次导入完成。端点应由 Editor 生成；不得手写 endpoint、跨工程复用 token，或提交 Library。

## 连通与资源生成

```powershell
& <local-cli> ping --project <project> --json
& <local-cli> compile --project <project> --wait --json
& <local-cli> console --project <project> --type error --count 80 --json
```

ping 的原始响应可能带 token；仅输出连接状态/版本/能力，报告不得包含认证信息。compile 完成只表示停止编译，必须再查错误。

无端点时先查 Editor 是否打开正确工程、编译错误、桥接包加载状态；CLI 此时可能不可用，可读 Editor.log。不要靠无限重试修复程序集错误。

UGUI 文件模式可完全不连数据库。需要数据库模板时，在 `integrations/codex` 通过 lockfile 安装 Node 依赖，复用已配置的连接机制；不得从示例复制数据库密码或创建虚构项目 ID。

资源生成遵循 `ugui --dry-run` → 无 issue 后正式生成；Widget 页面再使用完整 CLI 的 `widget-metadata --dry-run` → 正式绑定。不手写 prefab/场景 YAML。透明按钮规则、动画导入能力以当前导入器为准。
