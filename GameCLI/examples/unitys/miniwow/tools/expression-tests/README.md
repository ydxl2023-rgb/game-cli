# Expression 包验收

运行时库：`client/LocalPackages/com.oathx.expression`。仅新增独立包和UPM引用，未修改任务/Home业务。

```powershell
dotnet run --project tools/expression-tests/ExpressionTests.csproj -c Release
python -X utf8 tools/expression-tests/check-style.py
```

代码检查工具使用 `GameCLI/clis/code-verify/bin/Release/net8.0/CodeVerify.exe`，规则 oathx-code/9。先构建当前工具版本再检查；Debug目录中的旧版本不构成本次依据。

Unity验证先运行 `asset refresh` 和 `compile --wait`，确保 Library/ScriptAssemblies 下实际出现 Oathx.Expression.dll 和 Oathx.Expression.Checks.dll，再进入Play Mode调用 `ExpressionChecks.RunAll`，最后恢复编辑模式。

2026-11-26最终结果：

- .NET8：1077项检查通过，预热后10万次公式执行约9.34ms，循环托管分配0字节。
- Unity Editor：1077项检查通过，10万次约12.24ms，循环托管分配0字节。见 unity-tests.json。
- Unity编译完成，最终错误日志0。见 unity-compile.json、unity-console.json。
- 包Runtime的7份源码直接检查，oathx-code/9问题0。见 style-report.json。
- 架构检查问题0，扫描系统数为0（此包没有玩法System）；不能将此结果扩大为完整项目业务验收。
- 验证没有IL2CPP Player构建，也没有线程并发或任意业务委托性能保证。

测试覆盖了1000组与直接C#计算结果的对比，其余覆盖短路、注册/注销/重绑定、参数变更不解析、独立上下文、同步重入、异常后栈释放、错误位置、语法与类型错误、清空及预热分配。检查数量包含上述循环样本，不是1077种不同场景。
