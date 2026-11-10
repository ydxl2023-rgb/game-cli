---
name: bundle
description: 使用 com.oathx.bundle 的 XRes、AssetBundle 与热更新能力，维护资源加载及启动下载流程。
---

# Oathx Bundle

必须遵守 [开发规范](../development-rules/SKILL.md)。工程定位见 [Oathx](../SKILL.md)。

先读包内 `Runtime/XRes.cs`、`Bundle/XBundleManager.cs`，热更新任务再读 `Hotfix/Impl` 中的下载、版本、配置及重试实现，并检查 XApp 的实际调用路径。

- 使用项目既有资源入口和路径约定，不额外建立与 XRes 并行的加载/缓存系统。
- 修改加载流程前确认编辑器与运行时资源来源、异步完成回调及释放责任，不把编辑器直接读资产成功等同于构建产物可加载。
- 热更新进度来自真实下载状态；启动视图只显示提示和进度，不持有下载器管理职责。相关显示遵循 [Launch](../launch/SKILL.md)。
- 版本、清单、缓存和重试策略以当前代码与配置为准，不通过伪造成功或跳过校验掩盖加载失败。

按改动验证资源成功/失败路径、释放与实际构建模式。记录未执行的打包或远端下载验证。
