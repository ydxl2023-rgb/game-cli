# 代码检查规则配置

`rules.json` 是随 CLI 版本管理的强制规则表，不是游戏 Excel 数据。每次 `gamecli verify code` 都加载该文件，报告记录版本、SHA256 和全部规则的严重级别。丢失、损坏、重复、缺少规则或未知规则会使检查报错，不能按“通过”处理。

- `error`：违规导致命令退出码 1。
- `review`：保留人工复核事项，不等于已验证符合规范。
- 配置/工具错误：退出码 2。

默认检查整个 `Assets/Scripts` 手写代码，包括 GamePlugin 和 RuntimeSystem。自动生成文件及约定的 Excel 生成目录 `Assets/Scripts/Config` 排除并列入报告；不检查 LocalPackages、Editor 和第三方目录。指定 `--system` 缩小业务范围，但仍附带 GamePlugin、RuntimeSystem。全量交付使用默认范围。

```text
gamecli verify code --project <ID> [--output <新报告.json>]
gamecli verify code --project <ID> --system Home
gamecli verify architecture --project <ID>
```

## OX018：多行初始化分组

局部变量使用多行对象、集合或数组初始化器时，声明前后与同一语句块的相邻语句之间必须存在空行。块的第一/最后一条语句无需在大括号内侧补空行。注释不代替空行；普通连续单行声明、赋值、guard/return 不因本规则被逐条拆开。回调内部同样检查，不将回调外的变量声明误判为内部初始化器。

```csharp
Register();

var configure = new ResourceConfigure(sender) {
    loader = new GameUILoader(),
    cacher = new UIDefaultCacher()
};

Startup(configure);
```

`initializerSpacing.before/after` 明确记录规则参数，目前两项均强制为 true。OX008 检查成员分组，OX010 检查返回前空行，OX018 检查上述可确定的语法边界。其他“不同职责”的语义分组仍需人工复核，不能宣称空行规范已全部自动化。

## 新增规则流程

### OX019：代码行宽

`maxCodeLineLength` 当前为 120，包含缩进；Tab 按 4 列制表位展开。按 Roslyn 代码 token 位置测量，排除纯注释、行尾注释与多行字面量正文。单行字符串仍计入所在代码行；优先将参数或表达式换行，不改变文本值。超过上限产生 error，报告实际列宽及上限。配置必须是 80–200 的整数，缺失或无效时报配置错误。

长条件把 `||` / `&&` 放在续行开头，保留原有括号和求值顺序；长调用拆参数或调用链。规则只判断超长，不声称自动验证所有换行位置的可读性。

同步修改共享 development-rules、规则表、Roslyn 检测逻辑及正反例测试；只有登记描述不会自动产生检测能力。检测器拒绝未注册或未知编号，防止配置与实现脱节。调整严重级别属于规范变更，应由代码评审确认，不能为了让产品通过而降级规则。

验证：`node --test GameCLI/integrations/codex/test/code-verification.test.mjs`（从仓库根运行）。

### OX020 参数判空

后续操作已能明确报告失败时，删除仅用于抛出 ArgumentNullException 的重复参数检查。必需参数由正常操作暴露错误。不要用提前返回、默认值或跳过操作掩盖调用方错误。检测器对参数与 null 比较后直接 return 或抛出 ArgumentNullException 的分支提示 review，包括 || 组合。可选输入、配置读取失败、正常生命周期状态等分支应能说明具体行为。构造时需要拒绝无效依赖、外部 API 明确约定异常类型等情形按契约复核。

### OX021 系统激活

RuntimeSystem.Active 统一处理重复激活。业务 OnActive 负责初始化，禁止再用 IsActive 或随激活和停用翻转的字段提前返回。检测覆盖直接继承 RuntimeSystem 的系统和方法顶层的简单判断。异步请求版本必须能指出具体的旧回调、替代请求和结果消费位置。

### OX022 事件订阅

Subscribe 的事件参数必须直接传入具名回调方法，命名为 On 加 PascalCase 动作或事件名，例如 Subscribe<EnterLocalGame>(OnEnterGame)。禁止订阅时使用 lambda、匿名委托和 Handle 前缀。检测器检查单参数 Subscribe 调用的语法和名称。

### OX023 分开实体查询与使用

调用管理器的 QueryEntity<T>() 后先用有意义的局部变量接收结果，下一句再操作实体。直接访问查询结果的成员、使用条件访问或将查询内联为方法实参均报错。

## oathx-code/9 新增检查

|规则|级别|实际覆盖|仍需人工核对|
|---|---|---|---|
|OX024|error|普通、限定、条件访问的 DispatchEvent 调用|仅按方法名识别；非应用同名 API、别名/反射绕过及 Fire/Post 时序|
|OX025|review|名为 route 的标识符/成员与字符串字面量的 ==/!= 比较，左右均检查|switch、字符串常量、其他命名、协议文本；实体/事件是否端到端枚举化|
|OX026|error|直接继承 RuntimeSystem 的 OnActive 内 XCoroutine.Run；非 override 的 Install/Uninstall 声明|间接继承、别名、其他启动 API；InstallHome/UninstallHome 完整配对、bool 返回和清理正确性|
|OX027|error|业务源中 Assets/ 开头、.unity 结尾的字符串字面量，含 const|拼接和变量来源、配置表实际读取；Editor/框架不在扫描范围|
|OX028|review|SetAnimation/FindAnimation 参数中的字符串字面量|是否确实动画 API；其他播放方法、字段默认值、常量类型位置|
|OX029|error|VariableDeclarationSyntax 包含多个变量，含字段、局部、for|拆分后的作用域及求值顺序|
|OX030|review|直接 RuntimeSystem 子类中名为 generation/requestedActive 的字段|其他字段名、ready 等独立业务意义、异步取消和迟到结果的实际风险|

规则配置为 `rules.json`，加载器严格要求 CS_PARSE 与 OX001–OX030，未知/遗漏编号均失败。新增规则实现为 `AdditionalRules.cs`；正反例在 `code-verification.test.mjs`，包含同步/排队合法入口、条件调用、枚举、普通文本、真实 override、常量动画、独立变量和非系统 generation。不以列出的语法例子声称完整语义覆盖。

完整需求落点见 [确认台账](../../skills/oathx/references/development/CONFIRMED-CONVENTIONS.md)。本版本不修改被检查源码，不自动修改 Excel、资源或第三方包。
