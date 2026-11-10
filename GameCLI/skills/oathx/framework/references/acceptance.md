# 验收与交付

每项记录实测结果；未执行、失败或依赖缺失分别说明，不把它们写成通过。

| 检查 | 通过条件 |
| --- | --- |
| 工程 | Editor 与 ProjectVersion 匹配，项目路径正确，Package Manager 无缺失依赖 |
| 七个包 | 目录、package.name、manifest 指向一致，版本/来源/revision 有记录 |
| CLI | 本地 CLI 与完整 CLI 路径明确，目标 Editor 端点可用，所需命令实际可调用 |
| 编译 | compile --wait 完成且 Console 无相关 error，无缺失程序集、重复类型或脚本 |
| 生成器 | 至少一个 VM 的生成属性编译成功，Metadata dry-run 与实际绑定均通过 |
| 启动 | PlayMode 真实经过 XApp 和目标插件，最小页面显示；不是手工打开 prefab 代替启动 |
| UI | 文本更新和按钮回调有效，透明点击区域不画白块，根节点全屏 Stretch |
| Launch | 页面就绪后 Loading/Update 隐藏，背景保留，无前景重叠 |
| 清理 | 系统停用后页面关闭、监听释放，重开不重复回调 |
| 幂等性 | 再次预检/安装不产生重复包、组件、DLL 或覆盖用户配置 |
| 规范 | AGENTS.md 能定位编码规范和相关 skill，文件/引用按实际位置可解析 |

首次 Play 前检查数据清理选项和远端服务配置，避免参考工程自动清空存档或触发无关网络请求。启动后等到具体页面就绪再截图。回报编译、离线交互和联网测试的实际边界。

## 报告内容

将报告保存在目标工程适当的本地输出位置（如 `Library/GameCLI/Setup/report.md`），正文不含 token/密码：

- 目标工程、Unity 版本/平台、GameCLI 源码位置。
- 七个包与桥接包的来源、revision/version、是否存在本地修改。
- 两种 CLI 的路径、构建版本、Editor 桥接状态及所用命令。
- 生成器版本/哈希、Analyzer 设置、宏与外部依赖选择。
- 新建/复用的场景、XAppConfigure、插件、UI/VM 及引用位置。
- 验收结果、Console/截图等证据路径、未完成项及准确原因。

只把需要共享的源码、manifest/lockfile、meta 和文档纳入版本控制；Library、endpoint、生成 csproj、临时日志、凭证不提交。环境搭建本身不自动授权 commit/push 或模板发布。
