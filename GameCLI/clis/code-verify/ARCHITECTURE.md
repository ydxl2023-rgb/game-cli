# 系统依赖检查

从仓库根目录运行：

```powershell
node GameCLI/integrations/codex/cli/gamecli.mjs verify architecture --project p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3 --output architecture-report.json
```

命令验证项目绑定，只读扫描绑定工程 Assets/Scripts，使用 Roslyn 源码符号识别 RuntimeSystem/XEventModule 的具体派生系统。Systems/<目录> 是模块边界；同模块内部引用允许，跨模块直接引用报 OXA001。目录外的装配层提供类型上下文，但不按业务模块检查。不要通过移动业务代码到目录外规避规则。

检查类型声明、泛型参数、别名、限定名、转换、方法返回类型推导和方法所属类型；可检出 Load<LoginSystem>()、QuerySystem<TownSystem>()、缓存字段及可解析的辅助方法返回实例。注释和字符串不会被当成类型引用。间接继承也参与识别。

报告包含文件、行号、规则、源码 SHA256、排除文件与覆盖限制；校验期间源文件或绑定改变则报错。退出码 0 表示所覆盖规则通过，1 表示违规，2 表示检查未完成或执行错误。OXA_UNRESOLVED、OXA_DYNAMIC、OXA_CONDITIONAL 表示解析、动态调用或条件编译覆盖不足；存在明确违规时保留 failed，同时列出未覆盖项。没有识别到系统时不能判定通过。

这是新增架构检查，不替代 verify code、Unity 编译、运行验收。当前不加载 Unity 程序集，不保证覆盖接口中转、任意反射、生成代码和所有构建符号；不能将 passed 描述为全部架构规范已经自动证明。原有风格规则和架构检查必须一起运行。

回归测试：`node --test --test-concurrency=1 GameCLI/integrations/codex/test/architecture-verification.test.mjs GameCLI/integrations/codex/test/code-verification.test.mjs`。
