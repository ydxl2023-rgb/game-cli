# 通用插件装配独立静态复核

日期：2026-11-25。审查者：town_review，未参与实现。

结论：最终修订静态检查 passed；此前发现的两项问题已修复，未发现本轮剩余阻断问题。本轮没有操作 Unity、修改实现或执行故障注入，不将静态判断描述为独立运行通过。

## 已修复发现

1. 首次Load遵守Active=0，但已有实例的再次Load会隐式激活。现已有实例分支先判断configuration.Active==0，与新建分支一致；Type与泛型调用共用一套Load逻辑。
2. 构造/Install/Active异常原先会中断装配，Active异常还会遗留注册实例。现Load统一try/catch，失败返回null；已创建实例调用ReleaseSystem，删除相应注册。组合调用收到null后报告失败。

ReleaseSystem分别保护Detive和Uninstall；前者异常不阻止后者执行，正常Unload也复用该方法并安排延迟移除。重复Unload通过removes队列防重复释放；UnloadAll使用快照并清理注册及移除队列。

## 架构与边界

- GamePlugin从FunctionSystem表统一解析系统类型，配置加载通过IConfigData并在提交前检查完整字段契约及系统元数据；不再依赖MasterPhase或ActorConfiguration做会话业务判断。
- 通用组合请求复制ID数组；校验重复ID、配置存在性、Active/Startup标记及timeout。Update先处理移除队列，再分阶段卸载、加载和等待IsReady，避免同步Unload后立即取回待移除实例。
- Master按业务阶段选择配置ID、期限和Gameplay事件。Town自己检查角色配置；返回false的激活会被管理器回滚。
- Startup代际检查阻止Shutdown后的配置回调重建系统。Shutdown顺序为系统、实体、UI；停机时不再记录新的组合请求。
- 构造函数抛出后不存在可取得的实例，管理器无法清理构造函数在外部留下的副作用；模块构造应保持轻量，资源分配留在Install/Active。ReleaseSystem的异常保护保证尝试两个阶段，不保证有缺陷的模块一定释放了其所有内部资源。
- 本次未独立验证任意第三方模块的回调重入、配置运行时热改或所有异常组合；没有将正常会话回归等同于这些边界已覆盖。

## 证据归属与版本

主agent负责Unity编译及会话/玩家回归，本审查者只读复核源码。若交付引用运行结果，应保留主agent实际输出，不能使用旧版本独立报告替代本轮异常用例。

最终GamePlugin.cs SHA256：`D1855235EBC424B2B40D2AA4420D67A69948D3875A86D13168B8AE44E5DE1F66`。

附带静态范围：RuntimeSystem通用请求/结果及IsReady契约、MasterSystem阶段到配置ID的编排、TownSystem角色配置激活检查。本文件不修改此前Login正式prefab检查的failed原始状态。

复核后仅移除 GamePlugin 文件末尾多余空行，以上 SHA256 已对应该排版清理。
