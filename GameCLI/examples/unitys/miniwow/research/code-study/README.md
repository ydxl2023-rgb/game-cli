# 小骑士冲冲冲：代码与设计学习资料

日期：2026-11-24。对象是本地 `wxecf0e106809a477d` 游戏包。所有恢复、检查和文档均在研究目录进行，不把反编译代码导入 `client`。

## 先明确拿到了什么

已经恢复本批材料中可读取的热更新 C#、微信包 JavaScript、两份 WebAssembly 和 IL2CPP 元数据。这不是产品的全部原始源码，也不是可直接编译的完整 Unity 工程。服务端实现不在本批材料中；分包声明还有未提供的 `wasmcode2` 和外部 UnityPlugin。名称完整、结构可解析、工具退出成功，都不能证明原作所有功能已恢复。

|资料|结果|入口与性质|
|---|---|---|
|核心 HotUpdate.dll|4,239 个 C# 文件，307,421 行；元数据为 5,604 TypeDef、22,683 MethodDef|`managed/decompiled/HotUpdate/`，ILSpy 反编译代码|
|四份随包依赖|1,939 个 C# 文件|`managed/decompiled/`；标准库与 UniTask，不算原作原创业务|
|原始中间语言|完整 HotUpdate IL，41,423,051 字节|`managed/il/HotUpdate.il`，用于核对 C# 歧义|
|微信代码包|4 包、12 个成员，其中 5 个 JS|`packages/members/`，包内原始内容|
|WebAssembly|18,737,819 与 39,932,575 字节，均通过静态结构校验|`packages/decoded/`；反汇编状态见 `packages/README.md`|
|IL2CPP 元数据|76 个 image、8,728 类型、60,277 方法条目|`metadata-symbols/`；只有名称/成员结构，不是方法实现|
|调试 PDB|2,821 个源文档，1 个嵌入源码，0 SourceLink|`managed/pdb/`；唯一嵌入源码是 Unity 自动生成映射，不是业务源码|

两份 WASM 已用独立 WABT 1.0.42 导出完整 WAT（约 231 MB、441 MB），包含 18,713、104,683 个函数体；各 WAT 旁的 `.functions.json` 可按签名、行号和字节范围定位。WAT 是低级指令文本，不能当作原始 C#。

C# 中保留了反编译器提示：HotUpdate 有 4,860 条 `Unknown result type` 注释，部分 Unity 等外部依赖不全。五次程序集导出均退出 0，未出现 `Error decompiling` 标记，但没有重编译或运行验证。

## 推荐阅读顺序

1. 打开 [本地代码索引](index.html)，按类名或路径搜索。完整文件哈希与位置见 [code-index.json](code-index.json)。分类只是路径提示。
2. 阅读 [机制学习](GAMEPLAY-STUDY.md)，从探索、恢复、生产和战报播放理解业务闭环。每个结论附对应文件/方法/行号。
3. 阅读 [架构学习与 GameCLI 映射](ARCHITECTURE-STUDY.md)，理解启动、网络、Proxy 状态、UI、配置、生命周期；映射建议属于今后的自主设计，不是已经改好的模板。
4. 遇到代码含糊之处，查看 IL 与原 DLL；PDB 路径清单帮助识别原来的源码目录，但不应按同名类猜测原文件已经恢复。

第一周不必逐行读六千多个文件。先选择一个闭环：`ExploreSystem` → `LevelProxy` → 探索通知 → 英雄状态 → 展示刷新。为每一步写下输入、状态变化、输出、失败处理和清理时机。能讲清楚这个闭环，比只记住状态机或对象池的名称更有帮助。

## 已经证实的关键区别

- 探索显示有移动、执行、离开等状态；执行时长来自配置，完成后有网络请求与通知。不能仅凭动画结束就当作奖励已由客户端独立结算。
- 所查副本战斗读取 `battleMeta` 并转成动作队列播放；血量、护盾等变化来自战报。这里有值得学习的表现调度，但没有据此恢复服务器权威伤害公式。
- 旅店队列、消耗品制造与补给把经营和探索连接起来；客户端可见校验与请求，不等于拥有完整服务端经济逻辑。
- 配置按 ConfigHead 的偏移长度加载到 Luban 强类型表。配置读取器已经恢复，原策划 Excel 和同版服务端配置尚未取得。

## 可复查证据与复现

- [托管代码恢复记录](managed/MANAGED-REPORT.md)、[输入哈希与导出统计](managed/manifest.json)、`managed/rea-summary.json`。
- [微信包恢复记录](packages/README.md)、`packages/manifest.json`、`packages/decoded-manifest.json`。
- `evidence/rea-metadata.json` 保留 REA 完整元数据证据；核心 ID 为 `ev_ea793f6df819190960113bd4a5998e512c744f418231cf0e7cf01352b17e5498`。
- HotUpdate REA 核心 ID 为 `ev_7ee0842f10a15e972045b9ca8562ee7c02fee5d7c76c4bfe91e5293fc8b37ef9`。
- JS 已完整提取；REA 语义分析先在默认 300 秒超时，缩小到主 game.js 并限制语义条目后仍在 60 秒超时。本次不宣称已完成 JS 调用图，失败记录和部分证据保存在 `evidence/rea-javascript-*.json`、`evidence/rea-session.json`，不影响已提取 JS 内容和 C# 阅读。
- `evidence/source-inventory.json` 记录本批 191 个输入文件的大小、魔数和 SHA256；`evidence/client-baseline.json` 记录研究开始时 Unity 的 Assets、Packages、ProjectSettings 文件哈希。
- `../../tools/index_code_metadata.py` 与 `../../tools/index_recovered_code.py` 可重新生成元数据目录及代码索引；其他提取命令见对应恢复记录。

本次使用 [REA](https://github.com/morluto/rea/tree/main) 做静态证据检查，以 ILSpy 恢复 CIL 为 C#，以专用格式解析处理微信包。IDA 没有用于热更新 C#：这里直接分析 CIL 更合适；不因电脑装有 IDA 就把每种格式都交给它。

## 尚未完成、也不应冒称完成的事项

未获得服务端源码、完整原始工程、全部插件/分包；未运行原作验证机制；未验证代码可重新编译；未将任何系统发布为 GameCLI 模板；未改动 Unity 工程。后续应在已证实的客户端流程基础上设计自己的数据契约和系统，再用独立测试验证所写逻辑。

最终文件校验见 `evidence/final-validation.json`：Unity 的 Assets、Packages、ProjectSettings 共 102 个文件与研究开始哈希一致，191 个原始输入文件未变；6,183 个索引内 C#/JS 文件哈希全部一致。研究产物及局部工具已设置忽略规则，文档与复现脚本保留为可版本管理文件；本次没有提交或推送。
