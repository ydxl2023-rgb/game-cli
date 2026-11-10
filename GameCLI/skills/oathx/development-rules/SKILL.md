---
name: development-rules
description: GameCLI Unity C# 开发强制规范。编写、修改或审查本仓库的 Unity 组件和业务系统，以及创建系统 skill 时使用，落实字段、序列化、UGUI 绑定、英文注释和返回值约定。
---

# GameCLI 开发规范

这些规则来自用户明确确认的工程约定。开始 Unity C# 工作前必须读取，提交结果前必须逐项核对。后续系统 skill 必须引用本规范；独立 agent 同样遵守。用户后续明确修改规则时同步更新本文件，避免保留相互矛盾的要求。

## 先参考工程，再编写代码

- 先阅读目标模块和相邻实现，遵循现有目录、命名、组件生命周期与调用方式。不要用个人偏好替换工程风格。
- 以用户最新确认的规则为准：本次已确认采用私有字段，不再采用公开自动属性、公开 getter 或显式字段加公开属性的形式来暴露 UI 引用。
- 所有使用 com.oathx.* 框架的 skill 集中在 `skills/oathx`；每个业务系统使用独立的 `skills/oathx/system/<系统名>/SKILL.md`，共享规范集中维护于本文件，系统特有要求留在对应 skill。框架模块也各自独立成目录，不混入业务系统实现。
- Windows 命令必须使用已安装的最新版 PowerShell 7，即 `pwsh.exe`；不使用 `powershell.exe`，除非用户明确要求兼容测试。

## 字段与序列化

- UI 引用和内部状态使用 `private` 字段，不对外暴露字段或包装属性。
- 字段、局部变量和参数使用小写字母开头的 camelCase，例如 `loading`、`loadingText`、`downloadPercent`、`launcher`。禁止 `m_Loading`、`_loading` 等前缀和下划线命名。
- 需要 Inspector 绑定的字段使用普通 `[SerializeField]`，禁止 `[field: SerializeField]` 和序列化自动属性。
- 纯运行时状态保持私有，仅在确实需要持久化或 Inspector 配置时才添加序列化标记。
- 类和方法沿用工程的 PascalCase 命名。

```csharp
/// <summary>
/// Status message displayed during loading.
/// </summary>
[SerializeField]
private Text loadingText;

/// <summary>
/// Whether progress updates target the download panel.
/// </summary>
private bool downloading;
```

## UGUI 组件与引用绑定

- UI 资源固定放在 `Assets/Data/Widgets/`，每个页面或模块使用独立子目录，例如 `Assets/Data/Widgets/Loading/Loading.prefab`。配套图片、动画和材质随模块管理，不再生成到 `Assets/Resources/UI`。
- Widget 的 `[Config]`、CLI 生成目标、模板 scripts 中的资源路径必须一致使用该目录。初始化新工程时建立此目录；迁移现有资源使用 Unity AssetDatabase/UnityCLI 保留 `.meta` GUID，并核对场景、Metadata、序列化引用和资源加载。
- 路径或代码迁移时同步数据库当前标准模板与源码包，重新计算源码 SHA256；保留历史版本用于回滚，不覆盖模板布局。AssetBundle 发布前重新生成资源索引并构建资源包，不能复用旧路径的构建产物。

- 根据当前 prefab 的实际层级、组件和展示需求实现功能。先检查资源，不凭节点名称假设组件类型，不复制旧界面字段来代替当前设计。
- 固定 UI 引用全部通过序列化字段绑定，禁止 `transform.Find`、`Require<T>("路径")`、全场景搜索等运行时查找绑定，也不添加自动查找兜底。
- 组件通过实例方法对外提供操作，内部 UI 引用不暴露给调用方。引用该组件的一方同样使用私有序列化字段；跨系统操作提供明确的方法入口。
- 修改字段名称或序列化方式时迁移已有引用，保存并核对实际场景或 prefab 中的引用，不能只让代码编译通过。
- 区分 prefab 资产与场景实例绑定；交接时准确说明保存在哪一层，不能把场景绑定描述成 prefab 已绑定。
- 使用现有 UnityCLI 工具链进行资源生成、组件绑定和保存；生成 prefab 先 dry-run，再正式生成。不要手写或拼接 Unity 资源 YAML。

## 方法返回值

- `UISystem.OpenWidget<T>` 是异步页面打开入口，必须传入显式完成回调，禁止传 `null`。调用前在回调外检查 `IsActive()`，未激活则返回 `false`，不发起打开请求。页面就绪后的操作放在回调中；不能把即时返回值当作加载完成，也不能用回调中关闭页面代替调用前的激活检查。系统停用时在 `OnDetive` 中关闭页面。

- 自定义操作方法需要通过提前返回结束某个处理分支时，采用有意义的 `bool` 结果，不写 `void` 方法中的裸 `return;`。例如 `UpdateProgress` 完成当前面板更新后返回 `true`。
- 明确返回值语义：成功完成操作返回 `true`，存在正常的未完成或拒绝执行情况时返回 `false`；不要为了凑分支吞掉异常或伪造成功。
- Unity 生命周期、事件委托、继承重写等框架规定的签名保持兼容；不为这条规则修改固定签名。没有提前返回需求的显隐等简单方法可以保留 `void`。
- 方法返回结果时补充英文 `<returns>` 注释。

## 空行与逻辑分组

- 不同职责的操作块之间使用一个空行分隔，不把多个独立步骤紧贴在一起。
- 例如 `OnDetive` 中的 `UnsubscribeAll();` 与关闭页面的 `CloseWidget` 调用之间必须保留一个空行，区分订阅清理和页面关闭。
- 同一逻辑组内保持紧凑，不机械地在每条语句之间插入空行。

## 英文注释与默认参数

- 类、字段和方法使用英文 XML 注释，说明实际职责；带参数的方法补充有意义的英文 `<param>`，有返回值时补充 `<returns>`。
- 实现中的必要说明使用英文注释。注释必须与代码行为一致，不写空洞或过期说明。
- 默认参数禁止中文，使用英文，例如 `ShowUpdate(string message = "Downloading resources...")`。
- 不因为这条规则擅自翻译用户已编辑的 UI 文案或本地化资源；该限制针对代码中的默认参数。

## 验证与完成条件

- 检查字段是否全部为私有 camelCase、序列化是否使用普通 `[SerializeField]`、是否遗漏英文注释、是否存在中文默认参数或固定 UI 路径搜索。
- 代码或签名修改后执行 Unity 编译并检查 Console。引用变更后核对字段非空且指向正确对象；涉及显示和进度逻辑时验证实际 UI 行为。
- 注释或简单文案修改按影响范围验证，不机械重复完整运行测试。
- 保留任务范围外的用户修改。交接只陈述实际完成的检查，明确区分编译、编辑器验证和完整运行流程验证。
