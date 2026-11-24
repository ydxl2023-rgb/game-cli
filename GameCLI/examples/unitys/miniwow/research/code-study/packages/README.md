# 微信代码包恢复记录

四个 V1MMWX 包已用 appid `wxecf0e106809a477d` 解密，12 个成员已按包内路径提取。没有执行 JS/WASM，也没有修改原包或 client。

- `decrypted/`：解密后的完整 wxapkg。
- `members/`：原始成员内容；`__WITHOUT_MULTI_PLUGINCODE__/game.js` 是主要 JS 入口，另有 workers.js。
- `decoded/`：Brotli 解压后的 WebData 和两个 WASM。
- `manifest.json`：原包哈希、解密包哈希、成员路径、偏移、大小、哈希。
- `decoded-manifest.json`：解压结果与静态 WebAssembly.validate 校验结果。

两个 WASM 均通过 Node WebAssembly.validate（不实例化、不执行），分别为 18,737,819 与 39,932,575 字节，是独立可解析模块而非简单字节分片。此结论不代表已经理解两者的运行时职责，也不代表取得原始 C# 源码。

REA inspect_wasm_artifact 实际调用返回 capability_unavailable：REA_WABT_BIN_DIRECTORY 未配置；要求 WABT 1.0.42 的 wasm-validate、wasm-objdump、wasm2wat。该次 REA 调用没有生成 WASM 反汇编证据；后续改用独立 WABT CLI 完成，见下文。

复现：运行 tools/unpack_study_packages.py，再运行 tools/decode_study_packages.cjs。Python 使用已安装 cryptography；Node 使用内置 zlib Brotli。

格式依据：[BlackTrace/pc_wxapkg_decrypt](https://github.com/BlackTrace/pc_wxapkg_decrypt) 的 PBKDF2/AES/XOR 说明；实施时校验标志、目录边界、成员边界和解包结果哈希。提取脚本为本次编写，不执行下载的目标程序。

完整性限制：app-config.json 声明 wasmcode2，但当前四个原包中没有该分包；UnityPlugin 属于外部插件，未提供完整内容。两份 WASM 的结构有效不能证明产品代码完整。

后续已完成：从 WebAssembly/wabt 官方 release 下载 Windows x64 1.0.42 到本目录 tool/，归档 SHA256 与 GitHub release API digest 一致。通过独立 WABT CLI（未修改 REA 注册）对两个 WASM 执行 wasm-validate、wasm2wat 和 wasm-objdump -h，均成功。两个完整 WAT 为 230,689,431 和 441,476,885 字节，与 WASM 放在同目录；wat-manifest.json 记录工具版本/哈希与输出哈希。这是 WAT 低级指令文本，不是原始 C#。复现脚本 tools/disassemble_study_wasm.ps1。

每个 WAT 旁另有 `.functions.json`，记录函数签名、行号及字节范围，方便定点读取而不必一次打开数百 MB 文件。由 tools/index_wat_functions.py 生成。
