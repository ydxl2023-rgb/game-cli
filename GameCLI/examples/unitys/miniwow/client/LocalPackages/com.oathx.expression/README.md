# Oathx Expression

独立的表达式编译、缓存与执行库。无 UnityEngine、业务系统、反射调用、动态 IL 或第三方运行时依赖。

公开接口为 `Oathx.Expression.IExpressionEngine`，实现为 `ExpressionEngine`。每个引擎拥有自己的方法注册、变量槽位和编译缓存，不强制使用单例。

## 快速使用

```csharp
using Oathx.Expression;

IExpressionEngine engine = new ExpressionEngine();
ExpressionContext context = engine.CreateContext();
context.Set("hp", new ExpressionValue(100.0));

CompiledExpression formula = engine.Compile("hp * 0.1");
double first = engine.Run(formula, context).AsNumber(); // 10

context.Set("hp", new ExpressionValue(250.0));
double second = engine.Run(formula, context).AsNumber(); // 25; no parsing

// Text-based execution also reuses the exact-source cache.
double third = engine.Evaluate("hp * 0.1", context).AsNumber();
```

同一引擎内，相同文本和相同执行模式返回同一个 CompiledExpression；参数值变化不影响缓存。大小写、空白不同视为不同文本。Evaluate/Execute分别缓存，不能用纯模式句柄绕过命令权限。失败编译不缓存。变量在编译时绑定槽位，运行时按索引读取；Set按名字找槽位，不参与公式解析。

## 方法注册与调用

先注册，再编译调用它的表达式。签名固定，注册参数数组会复制，禁止同名活动注册覆盖；明确Unregister后再注册。

```csharp
engine.Register("Scale", ExpressionFunctionKind.Query, ExpressionValueKind.Number,
    Scale, ExpressionValueKind.Number, ExpressionValueKind.Number);

ExpressionValue result = engine.Evaluate("Scale(hp, 0.1)", context);
ExpressionValue direct = engine.Invoke("Scale", new ExpressionValue(100.0), new ExpressionValue(0.1));

// Host method:
private static ExpressionValue Scale(ExpressionArguments arguments)
{
    double value = arguments[0].AsNumber();
    double factor = arguments[1].AsNumber();

    return new ExpressionValue(value * factor);
}
```

- `Register(name, kind, result, callback, parameterTypes)`：注册查询或命令。
- `Invoke(name, arguments)`：直接调用，无文本解析；允许命令。
- `Evaluate(text, context)`：计算或判断，只允许Query。
- `Execute(text, context)`：允许Query与Command。
- `Compile(text, mode)` / `Run(program, context)`：预编译和高频执行。
- `Unregister(name)`：释放回调；旧编译句柄中的调用立即失效。返回是否移除了活动方法。
- 重新注册同名方法：旧句柄直接使用新的绑定，不重新解析；每次调用仍校验参数数量、类型、结果类型和纯模式权限。
- `ClearCache()`：清空文本缓存；调用方仍持有的编译句柄可继续使用。
- `Clear()`：注销全部方法（包括min/max/clamp/abs）并清空文本缓存。不清除已有context参数。
- `context.Clear()`：清空该上下文参数，释放其字符串值，保留数组容量。

清空后保留轻量的名字/槽位元数据，以保证旧句柄、现有上下文和同名重新注册的语义一致。彻底释放这些元数据需释放引擎及其上下文/句柄引用。缓存不自动淘汰，以满足“解析一次后复用”；配置热更新或批次结束时由宿主主动ClearCache。

## 语法与类型

支持有限double数值（含小数和科学计数）、true/false、单/双引号字符串、变量、括号、多参数与嵌套调用。

优先级由高到低：

1. 一元 `+ - !`
2. `* / %`
3. `+ -`
4. `< <= > >=`
5. `== !=`
6. `&&`
7. `||`

二元运算左结合。数值运算只接受Number；逻辑运算只接受Boolean；相等比较要求两边类型一致，字符串按Ordinal比较。没有隐式数字/字符串/布尔转换，没有字符串加法。数字采用double，不适合超过2^53的精确整数经济账本。

字符串支持 `\\`、`\'`、`\"`、`\n`、`\r`、`\t`、`\uXXXX`。标识符区分大小写，允许字母/下划线开头，后续可含数字；true/false为保留字。语法不支持赋值、成员访问、循环、多个语句、反射或任意C#代码。最大语法嵌套128层，用于避免错误配置耗尽解析调用栈。

内置Query：`min(a,b)`、`max(a,b)`、`clamp(value,lower,upper)`、`abs(value)`。

```text
maxHp * 0.1
min(maxHp - hp, maxHp * 0.1)
hp > 0 && attack / hp >= 2
HasHero(125) && BuildingLevel(60018) >= 1
Heal(targetId, maxHp * 0.1)
```

后三类业务函数由宿主注册，库不认识英雄、建筑、任务或技能。条件中的函数必须注册为Query；这是宿主承诺，库不能检查委托内部是否偷偷修改状态。

## 指令与执行边界

编译器按运算优先级解析并直接输出后缀指令。`&&`和`||`输出条件跳转，因此未选择分支中的变量、除法和函数不会执行。所有方法名和参数数量在编译时检查；动态变量类型及方法参数/返回类型在执行时检查。不会静默用0、false或null代替缺失变量和方法。

ExpressionException携带原文本的零基position与InnerException；直接Invoke失败的位置为-1。除零、非法类型、NaN/Infinity和无效clamp区间明确失败。命令已经产生的外部副作用不会因为后续表达式失败而自动回滚，业务事务应由宿主实现。

Command目前是同步调用。可用它提交导航/对话请求，但异步完成和取消仍由后续ITaskAction/业务生命周期负责；本库不假装等待整个场景动作完成。void命令返回 `default(ExpressionValue)`，不能把void结果当数值计算。

## 内存、并发与AOT

- CompiledExpression不捕获参数。不同context可以运行同一公式并使用各自变量。
- 执行栈按context与嵌套深度复用；同步重入同一context使用独立栈，异常后释放临时值。
- 引擎、注册表和context不是线程安全的；并行工作使用独立引擎/上下文。不要一边跨线程注册一边执行。
- ExpressionArguments是临时栈视图，回调返回后不得保存或异步使用。
- 预热后的数值解释循环不分配托管内存；首次编译、context扩容和新重入深度会分配。params形式直接Invoke可能由调用端创建数组，热点调用请复用数组。
- 使用静态委托调用和解释指令，无Expression.Compile、Reflection.Emit或DynamicInvoke。设计上兼容IL2CPP/AOT；本轮未构建IL2CPP Player，不将Editor测试当作该平台验证。
- 当前未做常量折叠或运行时JIT优化，性能证据基于实际解释器。

## 验证

Tests/Editor/ExpressionChecks.cs为无测试框架依赖的验收入口，在Editor专用程序集内，不打入Player。覆盖数值/布尔/字符串、1000组C#算式对照、短路、命令权限、函数重入、缓存、注销/重绑定、错误恢复与分配检查。

宿主测试：在miniwow根执行 `dotnet run --project tools/expression-tests/ExpressionTests.csproj -c Release`。
Unity测试：刷新资产、编译，进入Play Mode后通过UnityCLI调用 `ExpressionChecks.RunAll`；运行结束恢复Editor状态。

2026-11-26：.NET8与Unity Editor均通过1087项检查。最后一次Unity Editor预热后100,000次 `hp * 0.1` 约12.17ms，执行循环托管分配0字节；这不是所有公式、平台或业务函数的性能保证。包源码另以当前GameCLI规则直接扫描，不依赖默认排除LocalPackages的项目扫描。

## 配置命令元组

支持 `{"Navigate", 60018}`、`{"ShowDialogue", 3045}`、`{"OpenUI", "Hospital"}`，
以及无参数形式 `{"ShowMain"}`。这些业务命令由宿主注册，库本身不实现导航或页面操作。
元组和普通函数调用共用注册、类型检查与编译缓存。参数可以使用变量、算式和嵌套查询，
例如 `{"Heal", hp * 0.1}`。执行时hp可以变化，不需要重新编译。
元组不是JSON对象，不支持尾随逗号；本轮未增加多条命令序列语法。
条件列保持 `ATTR(SELF, HP) > 100 && enabled` 等布尔表达式，&&和||均短路。
动作返回void时不能用&&或||组合，应由任务后继节点编排。

新增元组语法的.NET8与Unity Editor测试累计1087项通过；覆盖嵌套参数、缓存、权限和错误语法。
