---
name: rpc
description: 使用 com.oathx.rpc 接入网络会话、消息与协议回调，维护既有业务通信流程。
---

# Oathx RPC

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/XNetRpc.cs`、`XNetSession.cs`、`XNetPacket.cs`、`INetParser.cs`，按实际协议再读 Http、Tcp 或 Web；业务调用对照当前 Rpc 封装及其实际调用方。旧 LoginSystem 通信已删除，新 LoginSystem 仅管理登录页面，不能作为通信蓝本。

- 区分包级会话/传输与业务请求/响应。复用项目现有协议类型、状态码及 Rpc 封装，不根据 UI 文案发明服务端接口。
- 创建、复用和销毁会话按现有调用链处理，检查连接回调与请求回调的职责，不能把发起请求当作业务成功。
- 只有明确包含通信职责的系统才接入 RPC；LoginSystem 当前只管理页面，不加入 Host/Login、超时或重试代码。
- 修改消息结构时检查发送与接收两端、解析器和生成代码来源，不只修改客户端一个字段。
- 测试区分离线模拟与实际服务端请求；回报实际完成的验证，不将编译或模拟结果描述成线上登录成功。
