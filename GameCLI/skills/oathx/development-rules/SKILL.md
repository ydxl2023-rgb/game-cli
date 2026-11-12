---
name: development-rules
description: GameCLI Unity C# 开发强制规范。编写、修改或审查本仓库的 Unity 组件和业务系统，以及创建系统 skill 时使用，落实字段、序列化、UGUI 绑定、英文注释和返回值约定。
---

# GameCLI 开发规范

这些规则来自用户明确确认的工程约定。开始 Unity C# 工作前必须读取，提交结果前必须逐项核对。后续系统 skill 必须引用本规范；独立 agent 同样遵守。用户后续明确修改规则时同步更新本文件，避免保留相互矛盾的要求。

## 先参考工程，再编写代码

- 先阅读目标模块和相邻实现，遵循现有目录、命名、组件生命周期与调用方式。不要用个人偏好替换工程风格。
- 以用户最新确认的规则为准：本次已确认采用私有字段，不再采用公开自动属性、公开 getter 或显式字段加公开属性的形式来暴露 UI 引用。
- 所有使用 com.oathx.* 框架的 skill 集中在 `skills/oathx`；每个系统或展示模块使用独立的 `skills/oathx/system/<系统名>/`，其下分别维护 `development/SKILL.md` 与 `acceptance/SKILL.md`，共享规范集中维护于本文件，系统特有要求留在对应 skill。框架模块也各自独立成目录，不混入业务系统实现。
- Windows 命令必须使用已安装的最新版 PowerShell 7，即 `pwsh.exe`；不使用 `powershell.exe`，除非用户明确要求兼容测试。

## 系统代码目录

所有系统统一按职责存放手写代码：

```text
<System>/
  <System>System.cs
  <System>Entity.cs
  OtherLogic.cs
  VM/
    VM<System>.cs
    VMItem.cs
  UI/
    UI<System>.cs
    UIItem.cs
```

- `VM/` 存放该系统所有 ViewModel，包括页面 VM 和列表项 VM；不再与 UIWidget 混放。
- `UI/` 只存放 UIWidget 实现，包括页面和列表项 Widget。
- 其余代码均放在系统根目录，包括系统逻辑、实体、数据类型及辅助代码，不另建 Components、Data 等子目录，除非用户明确要求例外。
- 本规则用于后续新建和整理系统；一次任务只迁移明确要求的系统，不自动批量搬动其他历史模块。
- 迁移通过 Unity AssetDatabase/UnityCLI 保留 `.meta` GUID；同步维护 SKILL 路径、源码包清单及工具引用，不手改自动生成代码，不改变 VM 字段顺序或 Metadata 绑定。

## 字段与序列化

- UI 引用和内部状态使用 `private` 字段，不对外暴露字段或包装属性。VM 的 `[Binding]` 字段适用下方专门规则，可省略 `private`（C# 类字段默认私有）。
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

## VM 绑定字段格式

- `[Binding]` 或 `[Binding(typeof(...))]` 必须独占一行，字段声明放在下一行，禁止属性与字段声明写在同一行。
- VM 绑定字段保持私有语义和 camelCase 命名，但不强制显式声明 `private`；允许省略，沿用所在 VM 的统一风格。不要把省略 `private` 误判为公开字段，也不要为此强行补写关键字。
- 每组英文注释、Binding 属性、字段声明之间保持紧凑，不同字段组之间空一行。
- 此规则仅针对 VM 绑定字段，不放宽组件 UI 引用的序列化和封装规则。格式调整不得改变字段名称、类型、顺序及 Sync 类型，以免影响生成代码和 Metadata。

```csharp
/// <summary>Close the dialog.</summary>
[Binding]
ButtonClick closeClick;

/// <summary>Available server categories.</summary>
[Binding(typeof(SyncFancyGridScrollView))]
VList<VMServerGroupItem> groups;
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

## 类成员声明顺序

- 字段和属性集中声明在类开头，必须位于构造函数之前，禁止穿插在方法之间或放到构造函数之后。
- 统一顺序为：字段与属性 → 构造函数 → 方法。多个构造函数连续放置；构造函数区域之后只编写方法。
- 方法区域必须按“生命周期重写方法 → 公共方法 → 私有方法”排列。`OnActive`、`OnDetive` 等生命周期重写方法必须放在方法区域最前面，通常按激活、停用顺序排列；这里的“最前面”不改变字段、属性和构造函数在前的规则。
- 普通 `public` 方法必须位于 `private` 方法之前，不得交错排列。存在普通 `protected` / `internal` 方法时放在公共方法之后、私有方法之前。同组内按职责组织，调整顺序时连同 XML 注释和特性整体移动，不改变方法逻辑。
- 没有显式构造函数的类，字段和属性仍必须位于第一个方法之前，不为满足顺序添加空构造函数。
- 调整声明位置时保留字段和属性各自的相对顺序，尤其不得改变 VM 绑定字段顺序或 Metadata 契约。

## 数据存储键

- 数据存储键统一采用 `系统.类名.变量名` 三段格式，各段全部小写，以英文句点分隔，不使用项目名前缀或额外下划线。
- 例如 Login 系统中 LoginEntity.selectedServer 的键为 `login.loginentity.selectedserver`，recentServer 的键为 `login.loginentity.recentserver`。
- 业务代码不得手写存储键字符串。统一调用 `StorageKey.For<TSystem, TEntity>(entity => entity.property)`，通过类型信息和表达式中的 MemberInfo 反射生成键；系统段取系统类名去掉末尾 System，类名和成员名取真实声明名称，最后 ToLowerInvariant。
- 读取和写入使用同一生成接口；不执行或编译属性表达式。只接受实体的直接公共实例字段/属性，不接受嵌套路径或运算表达式。
- 当前 Login 已删除旧大小写键的兼容读取，只使用统一生成的小写键。不得擅自重新加入大写旧键或手写备用键。

## 状态类型

- 有限、明确的业务状态必须定义为 Enum，属性、参数、赋值和判断统一使用枚举类型及命名成员，禁止以 int 和魔法数字表达状态。
- 仅服务于某个实体的枚举与该实体放在同一文件中；枚举类型和成员使用 PascalCase，并提供英文注释。
- 外部数据转换为枚举时校验合法值；不能因为类型是 Enum 就默认所有数值有效。已有持久化或协议数值应保持兼容。

## 方法与属性语法

- 不使用 `=>` 表达式体方法或表达式体属性。采用传统 `{ ... }` 方法体，或符合该类型封装约定的属性写法（显式 `get` / `set` 访问器）。
- 纯数据类型的成员使用公共 camelCase 自动属性，不使用私有 readonly 字段搭配简单取值方法。仅包含读取、赋值，没有其他操作的数据使用属性，不额外编写 Get/Set 方法对。属性声明后换行，再写 `{ get; set; }`，沿用既有实体风格。
- 纯赋值初始化使用对象初始化器，不编写只执行 `this.xxx = xxx` 的传参构造函数，也不为了区分参数与成员添加显式 `this.`。确有初始化行为的构造函数（例如恢复本地数据）可以保留。
- 赋值后还需保存、同步、通知等操作时，使用传统方法体，将赋值及后续操作封装在同一个方法中。禁止拆成“外部属性赋值，再调用保存/同步方法”，避免调用方遗漏必要操作。
- 此类数据可通过 `{ get; private set; }` 属性对外只读，修改必须调用业务方法。例如 LoginEntity.selectedServer 只读，SelectServer(serverId) 统一执行校验、赋值和保存。
- 实体数据属性不属于 UI 引用，不受“UI 引用不对外暴露”的限制。

```csharp
/// <summary>Player identifier.</summary>
public int pid
{ get; set; }

/// <summary>Currently selected server identifier.</summary>
public int selectedServer
{ get; private set; }
```
- 此规则不禁止事件回调、LINQ 等场景中的 lambda 表达式；不能将所有 `=>` 一概替换。UI 引用仍遵循私有字段要求，不因允许属性语法而对外暴露。

## 方法返回值

- 系统级别的类关闭 Widget 时，必须统一调用 `UISystem.CloseWidget(System.Type[] types, System.Action complete = null, bool immediate = false)`，由调用方通过类型数组明确传入要关闭的页面；单个页面也使用数组，禁止继续使用泛型单页关闭接口。同一次系统退出需要关闭多个页面时合并为一次数组调用，保留必要的关闭顺序和 immediate 设置。
- 关闭页面前必须显式声明局部变量 `System.Type[] widgets = new System.Type[] { ... };`，每个 `typeof(...)` 独占一行，再调用 `CloseWidget(widgets, ...)`；禁止在调用参数中内联构造数组，单个页面同样遵守。数组声明与关闭调用之间空一行。数组只包含该操作负责关闭的页面，不扩大到其他系统。

```csharp
System.Type[] widgets = new System.Type[] {
    typeof(UIServerSelect),
    typeof(UILogin)
};

UISystem.GetSingleton().CloseWidget(widgets, immediate: true);
```

- `UISystem.OpenWidget<T>` 是异步页面打开入口，必须传入显式完成回调，禁止传 `null`。调用前在回调外检查 `IsActive()`，未激活则返回 `false`，不发起打开请求。页面就绪后的操作放在回调中；不能把即时返回值当作加载完成，也不能用回调中关闭页面代替调用前的激活检查。系统停用时在 `OnDetive` 中关闭页面。

- 自定义操作方法需要通过提前返回结束某个处理分支时，采用有意义的 `bool` 结果，不写 `void` 方法中的裸 `return;`。例如 `UpdateProgress` 完成当前面板更新后返回 `true`。
- 明确返回值语义：成功完成操作返回 `true`，存在正常的未完成或拒绝执行情况时返回 `false`；不要为了凑分支吞掉异常或伪造成功。
- Unity 生命周期、事件委托、继承重写等框架规定的签名保持兼容；不为这条规则修改固定签名。没有提前返回需求的显隐等简单方法可以保留 `void`。
- 方法返回结果时补充英文 `<returns>` 注释。

## 空行与逻辑分组

- 成员注释前必须与上一组成员声明保留一个空行，禁止字段声明后紧接下一成员的 `/// <summary>`。此规则适用于所有字段、属性、构造函数和方法，不仅限于 VM；类的第一个成员无需在开括号后额外插入空行。注释、特性和其对应声明属于同一组，组内保持紧凑。

- 获取 VM 或实体后立即单独检查有效性，再读取属性或调用方法。支持框架布尔转换的对象统一使用 `if (!vm)`、`if (!entity)`，bool 方法中返回 `false`；不要把 VM 检查藏在获取 Widget 的复合条件中，也不要链式调用 `QueryEntity<T>().Member`。
- `var vm = widget.vm;` 与紧随其后的判空属于同一组；实体查询与判空属于另一组，两组之间空一行。方法内按“获取与校验 → 准备数据 → 修改/保存实体 → 刷新 UI → 后续操作 → 返回结果”分块，不同职责之间空一行。
- `OpenWidget` 完成回调中的 Widget 检查必须使用 `if (widget) { ... }` 正向包裹初始化逻辑，禁止以 `if (!widget) return;` 开头。在该块内获取 VM 后仍单独判空。
- 框架规定的 void 回调保留签名，其中 VM 无效时可使用 `if (!vm) return;`，不要为了返回 bool 改变委托契约。自定义 bool 操作方法仍使用 `if (!vm)` / `if (!entity)` 返回 false，并向调用方传递失败结果。

- 执行赋值、保存、同步或其他操作后，`return` 之前必须保留一个空行，将处理过程与返回结果分开。此规则同样适用于前面是完整条件块的情况。
- 直接作为 `if` 分支的提前返回与条件保持紧凑；方法或分支内只有一条 `return` 时，不在开括号后机械添加空行。

```csharp
selectedServer = serverId;

PlayerPrefs.SetInt(StorageKey.For<LoginSystem, LoginEntity>(entity => entity.selectedServer), selectedServer);
PlayerPrefs.Save();

return true;
```

- 不同职责的操作块之间使用一个空行分隔，不把多个独立步骤紧贴在一起。
- 数据赋值完成后，与后续保存、同步、通知等操作之间必须空一行。例如 `recentServer = selectedServer;` 与 `PlayerPrefs.SetInt(...)` 之间空一行；`SetInt` 和 `Save` 属于同一持久化操作组，保持紧凑。连续赋值属于同一组，不要求每条赋值之间都空行。
- 例如 `OnDetive` 中的 `UnsubscribeAll();` 与关闭页面的 `CloseWidget` 调用之间必须保留一个空行，区分订阅清理和页面关闭。
- 同一逻辑组内保持紧凑，不机械地在每条语句之间插入空行。

## 固定文案与多语言

- 所有固定的界面显示文本统一维护在 `examples/unitys/development/config/propCommonText.xlsx`，包括按钮、标题、提示、状态名称和带参数的完整句式；业务代码不再硬编码这些显示文本，也不另建系统专用文案表。
- 每条文案使用唯一且稳定的 `ID` 别名和 `NumID`。代码引用导表生成的 `Config.Excel` 别名常量，通过 `CommonText.Get(id, args)` 查询，不手写数字 ID，不直接依赖自动语言表的行号别名。
- `Context` 维护源文案；导表自动提取到 `propLanguage.xlsx`。不要将生成的语言表作为新增固定文案的源表，也不要手改生成的 C# 或 bytes 文件。
- 动态值使用 `{0}`、`{1}` 等占位符，整句放在表中，禁止在代码里拼接中文片段。实体只提供状态与数据，由 UI/系统选择文案 ID。
- 用户输入、服务端公告正文等动态内容不作为固定文案；英文代码注释、诊断日志、路径和内部标识也不属于界面文案。
- Excel 编辑使用 UnityCLI，先 dry-run 再写入。导表后验证每个 CommonText ID 都能解析到语言文本，尤其检查最后一行；编译并检查 Console。
- 当前导表器重建语言表，不保留已有外语翻译。导表前检查并备份已有翻译，禁止覆盖丢失；存在翻译时先实现保留/合并流程。本次 Login 迁移确认外语列为空。
- 后续新增和修改 UI 必须遵循本规则；历史系统的全量迁移按任务范围逐步进行，不能宣称未检查的 prefab 固定文本已完成本地化。

## 英文注释与默认参数

- 类、字段和方法使用英文 XML 注释，说明实际职责；带参数的方法补充有意义的英文 `<param>`，有返回值时补充 `<returns>`。
- 实现中的必要说明使用英文注释。注释必须与代码行为一致，不写空洞或过期说明。
- 默认参数禁止中文，使用英文，例如 `ShowUpdate(string message = "Downloading resources...")`。
- 不因为这条规则擅自翻译用户已编辑的 UI 文案或本地化资源；该限制针对代码中的默认参数。

## 验证与完成条件

- 检查字段是否保持私有 camelCase（VM 绑定字段允许省略 private）、VM 的 Binding 是否独占一行、序列化是否使用普通 `[SerializeField]`、是否遗漏英文注释、是否存在中文默认参数或固定 UI 路径搜索。
- 代码或签名修改后执行 Unity 编译并检查 Console。引用变更后核对字段非空且指向正确对象；涉及显示和进度逻辑时验证实际 UI 行为。
- 注释或简单文案修改按影响范围验证，不机械重复完整运行测试。
- 保留任务范围外的用户修改。交接只陈述实际完成的检查，明确区分编译、编辑器验证和完整运行流程验证。
