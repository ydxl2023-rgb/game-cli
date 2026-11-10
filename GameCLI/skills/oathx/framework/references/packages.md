# 工程与包安装

以下路径除目标工程外均相对 GameCLI 根目录。以实际源码/manifest 为准；版本号是定位参考，不代替兼容性验证。

## Unity 与命令环境

- Windows 始终使用最新版已安装的 `pwsh.exe`，检查 `git --version`、`svn --version --quiet`、`dotnet --list-sdks`；数据库模板工作还需 Node.js >=22。
- 新工程优先使用参考基线 Unity 2022.3.62f2；通过已安装的 Unity Editor/Hub 创建。已有工程读取 ProjectSettings/ProjectVersion.txt，用对应 Editor 打开，不静默转换版本。
- SVN 不在 PATH 时可定位 TortoiseSVN 的 svn.exe，不能假设系统已有。Editor 版本或许可缺失时记录阻塞，不循环启动失败。

## 七个运行时包

目标目录统一为 `<project>/LocalPackages/<package>`：

| 包 | 当前来源 |
| --- | --- |
| com.oathx.avatar | svn://gitee.com/oathxv/com.oathx.avatar/branches/gamecli |
| com.oathx.bundle | svn://gitee.com/oathxv/com.oathx.bundle/branches/gamecli |
| com.oathx.core | svn://gitee.com/oathxv/com.oathx.core/branches/gamecli |
| com.oathx.entity | svn://gitee.com/oathxv/com.oathx.entity/branches/gamecli |
| com.oathx.rpc | svn://gitee.com/oathxv/com.oathx.rpc/branches/gamecli |
| com.oathx.widget | svn://gitee.com/oathxv/com.oathx.widget/branches/gamecli |
| com.oathx.widget.extension | svn://gitee.com/oathxv/com.oathx.widget.extension/branches/gamecli |

参考清单：`examples/unitys/client/LocalPackages/install.bat` 和 `svn.txt`。将清单转换为 pwsh 中逐项执行的 SVN 命令，不直接用会依赖当前目录且忽略中途失败的批处理作为自动化验收。

```powershell
svn checkout svn://gitee.com/oathxv/com.oathx.core/branches/gamecli <project>/LocalPackages/com.oathx.core
```

每次检查退出码。已存在目录用 `svn info`、`svn status` 核对；正确来源且已满足需求就复用，不能重复 checkout、删除或自动 relocate。用户指定版本时用明确 revision 安装。来源变更与更新保留本地修改，记录实际 URL/revision。

这些是独立 SVN 工作副本，远端由 Gitee Git 分支承载；外层 Git 提交不等于包已提交。安装任务不自动 commit/push。Git 镜像只有经核实确为对应仓库/分支才可替代 SVN。

## Manifest 与依赖

合并 `Packages/manifest.json`，每个包使用 `file:../LocalPackages/com.oathx.<name>`，保留其他依赖与注册表。不得直接覆盖为示例完整 manifest。

- 检查各包 package.json、asmdef 和外部 using。示例 package.json 并未完整声明所有依赖，七个包下载完不代表可以编译。
- `com.oathx.bundle` 声明 `com.unity.nuget.newtonsoft-json` 3.2.1；检查已有 Newtonsoft DLL，避免重复程序集。
- Avatar 使用 `DG.Tweening`；从项目已有授权资源/可靠安装源配置 DOTween 与 asmdef。不要凭空创建同名空类消除编译错误。
- 检查 LitJson 等实际引用及目标工程已有插件。不要把参考 Assets/Plugins 全量复制进已有项目。
- `com.unity.ai.navigation` 是 Unity 包，不是 Oathx 主干包；仅在代码需要时安装兼容版本，不能套用 Oathx SVN 地址。
- URP、TMP、Unity 模块根据实际目标和程序集依赖合并。参考 URP 14.0.12、UGUI 1.0.0 不等于所有目标工程必须强制同版本。
- `com.oathx.unitycli` 是额外 Editor 桥接依赖，按 [CLI 安装](cli.md) 单独处理，不虚构第八个 Gitee SVN 地址。

让 Unity 完成 Package Manager 解析；记录 packages-lock.json 的实际结果。包安装失败或有缺失程序集时先修复依赖，不跳过 Console 错误进入初始化。
