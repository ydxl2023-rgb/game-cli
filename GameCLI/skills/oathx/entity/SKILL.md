---
name: entity
description: 使用 com.oathx.entity 的 XEntity、XEntityManager 与 XNumerical，维护业务实体、查询和 UI 数据刷新边界。
---

# Oathx Entity

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/XEntity.cs`、`XEntityManager.cs`、`XNumerical.cs`，再读目标业务实体和已有创建、查询、刷新、释放调用方。

- 复用已有业务 Entity 和管理器，不为同一数据在 UI 内另建实体仓库。
- QueryEntity 的结果按当前调用契约检查有效性；不要把查询当成创建或网络加载。
- 实体字段、属性 ID、数值和刷新方式遵循该业务既有定义，不用 VM 字段反推持久化结构。
- 业务数据由对应 Entity 持有，System 按需查询并读写 Entity，不在系统成员中另存一份当前选择、最近登录等业务状态。System 负责流程编排和将实体数据同步到 VM，实体负责数据修改及其已授权的本地持久化。
- LoginEntity 持有 selectedServer、recentServer；LoginSystem 通过只读属性读取，通过 SelectServer / RecordLoginServer 方法统一完成赋值和持久化，不能直接操作对应 PlayerPrefs 键，也不缓存 LoginEntity 成员。页面分类索引等临时展示状态与持久业务数据应区分。
- 数据保存或通信需阅读调用方及 [RPC](../rpc/SKILL.md)，不因实体存在就自动新增远端读写。

验证实体缺失、正常刷新和相关页面展示；不以编译成功替代实际数据契约验证。

- 纯读取和赋值使用属性，声明行后换行写 `{ get; set; }`；赋值后有保存、同步等操作时，使用传统方法统一完成，不能拆成属性赋值加保存调用。数据可通过 `{ get; private set; }` 对外只读。

- 实体属性与字段统一放在构造函数之前，构造函数之后只放方法；禁止将属性夹在业务方法之间。遵循共享开发规范的类成员声明顺序。

- 仅服务于某个 Entity 的简单附属数据类型，与该 Entity 放在同一个源码文件中，不机械地按“一类一文件”拆分。例如 ServerEntry 声明在 LoginEntity.cs 中；只有确有独立复用或复杂职责时才考虑拆分。

- 简单数据类型（如 ServerEntry）使用公共 camelCase 自动属性和对象初始化器，不编写仅传参赋值的构造函数、`this.xxx = xxx` 或仅返回私有字段的 getter 方法；有实际计算或保存行为的传统方法保留。

- 有限业务状态使用枚举。LoginEntity.cs 中的 ServerState 定义 Normal、Busy、Maintenance，ServerEntry.state 使用该类型；目录接入时检查枚举值是否合法，不再使用数字状态判断。

- Entity 及附属数据类型只提供业务状态（例如 ServerEntry.state 枚举），不负责把状态转换成中文、其他语言文案、图标或显示格式。具体展示由外部 UI/展示层根据状态决定，不在实体中提供 GetStatus 之类返回展示文案的方法。
