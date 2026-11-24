# 托管代码恢复记录

日期：2026-11-24。未修改 client，未执行游戏程序集。输入来自 extracted-resources/assets/data/hotupdate；仅复制并移除 .bytes 后缀，完整输入哈希见 manifest.json。

## 结果

REA 完整读取 HotUpdate 的 CLI 元数据：5,604 个 TypeDef、22,683 个 MethodDef，CIL / AnyCPU，元数据覆盖 complete，无解析问题。原始证据 ev_7ee0842f10a15e972045b9ca8562ee7c02fee5d7c76c4bfe91e5293fc8b37ef9；本地 rea-summary.json 保存摘要与证据链接。

ILSpy 9.1.0.7988 全项目导出 5 个程序集，均退出 0，日志中无错误；生成 C# 中未出现 Error decompiling 标记。

|程序集|C# 文件|行数|Unknown result type 注释|
|---|---:|---:|---:|
|HotUpdate|4239|307421|4860|
|mscorlib|1315|182161|0|
|System|375|54298|36|
|System.Core|23|6418|0|
|UniTask|226|23873|105|

文件数与 TypeDef 不相等：嵌套类型、编译器生成状态机等会合并或被还原。退出 0 不代表结果能重新编译。Unknown result type 多见于未提供 Unity 等依赖的外部类型调用，仍需阅读 IL/补充同版依赖验证。标准库和 UniTask 属依赖，不应当作游戏原创业务。

## 复现

在本目录 PowerShell 7 执行（工具已局部安装）：

```powershell
$env:DOTNET_ROLL_FORWARD='Major'
./tool/ilspycmd.exe -p -o ./decompiled/HotUpdate -r ./input ./input/hotupdate.dll
```

其余四个程序集使用相同命令替换输入和输出目录。安装命令为 `dotnet tool install ilspycmd --version 9.1.0.7988 --tool-path ./tool`。

## 学习入口

- decompiled/HotUpdate/ExploreSystem.cs：探索系统。
- decompiled/HotUpdate/Battle/BattleStateMachine.cs：战斗状态组织。
- decompiled/HotUpdate/Battle/BattleUtility.cs：战斗计算工具。
- decompiled/HotUpdate/Battle/BattleCharacterEntity.cs：战斗角色行为。
- decompiled/HotUpdate/Dungeon/GroupBattleController.cs：组队副本控制。
- decompiled/HotUpdate/Zombie.Table.*：配置声明及读取逻辑。

这是从编译后的客户端恢复的可读 C#，不是开发者原始工程；原注释、项目组织、宏配置、部分局部变量及编译前表达式可能不可恢复。不能据此声称取得服务端代码或整个产品所有代码。未做重新编译、运行、服务端协议调用。

## IL 导出
已通过 ilspycmd --ilcode ./input/hotupdate.dll 导出 il/HotUpdate.il（41,423,051 字节），退出 0，stderr 日志为空。供核对 C# 反编译歧义。

## Portable PDB 静态检查
使用 .NET 9 System.Reflection.Metadata 读取 PDB，命令：dotnet run --project ./pdb-inspector -- ./input/hotupdate.pdb ./pdb。完整读取 2821 个 Document、2070 个 CustomDebugInformation，发现 1 个 EmbeddedSource、0 个 SourceLink。清单在 pdb/pdb-documents.json，保留原路径、哈希、语言、所有调试信息 GUID；嵌入源按本地生成 ID 导出，未使用原路径写盘，也未访问远程。其余 Document 路径只证明编译时源文件名，并不意味着源码存在。
