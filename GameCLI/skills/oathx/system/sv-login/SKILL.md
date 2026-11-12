---
name: sv-login
description: 独立 agent 参考 SvLogin 标准模板，为产品开发 LoginSystem、LoginEntity、UI 与 VM，并按产品契约接入登录、选区和系统入口。
---

# Login 产品开发

开发交付后由另一名 agent 按 [独立系统验收](../../system-acceptance/SKILL.md) 和本系统产品需求验收，覆盖本轮实际交付的主页面、子页面及入口。

必须读取 [系统开发流程](../../system-development/SKILL.md)、[开发规范](../../development-rules/SKILL.md)、[Core](../../core/SKILL.md)、[Entity](../../entity/SKILL.md)、[Widget](../../widget/SKILL.md)、[Widget Extension](../../widget-extension/SKILL.md)。

## 默认设计

未明确提出设计变更时，保留选用标准模板的布局、视觉、交互和动画，不自行改版。仅做保持表现与行为一致的必要技术适配；产品契约缺失时不虚构业务。遵循[共享默认规则](../../system-development/SKILL.md#默认保留模板设计)。

## 范式与边界

- 模板 ID：SvLogin。目录名 sv-login 用于定位来源；产品代码不必保留 SV 前缀。
- LoginSystem 协调页面生命周期和登录业务，LoginEntity 存放业务数据，UI/VM 负责展示和交互绑定。System 不缓存 VM，不承担其他系统内部逻辑。
- 选区可作为 Login 的子页面，不因多一个页面就新增系统。其他入口通过产品统一系统调度契约接入。
- 标准示例采用本地 LoginSucceed 事件，不代表未来产品必须离线或禁止通信；网络业务只能按明确的产品接口实现，不恢复旧 Host/Login RPC 作为默认方案。

## agent 开始时确定

登录模式、成功/失败数据、身份来源、是否选区、区服状态、按钮用途、成功后的交接对象。读取产品已有事件与实体契约；MasterEntity.PID、LoginResponse、十二条测试区服和进入 Home 均不是必需依赖。

## 实现路径

1. 读取模板布局、源码和动画，核对目标框架生成器及 UI 加载入口。
2. 按需求组织 Login/VM、Login/UI，System、Entity 及实体专用类型放 Login 根目录。复用已存在公共基类与事件。
3. 将身份、区服选择等数据放入实体；状态使用枚举，显示文案从 propCommonText 查表；保存操作封装在实体方法中。
4. 按产品 UI 建立 Metadata/Fancy 列表绑定，字段顺序以本次 VM 为准；不沿用历史九项绑定数量作为新产品契约。保留按钮视觉，未接业务不等于统一禁用。
5. OnActive 在异步打开前检查激活状态，回调用 if (widget) 包裹初始化并检查 VM。OnDetive 取消订阅，通过局部 widgets 类型数组关闭本系统页面。
6. 按产品约定发出登录结果/入口事件，配置共享负责人完成系统注册及必要配置。没有登录服务契约时不伪造成功，不擅自开发 Master/Home/Notice。

开发结果列出事件载荷、实体数据、页面入口及待接业务接口。具体 SV 换区资源、存储示例、源码归档历史见 [参考实现](references/gamecli-example.md)，仅在需要复用时读取。
