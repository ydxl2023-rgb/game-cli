# 配置读取修复复核

以下为当时版本的复核记录。此后 InstallSystems 已按 development 简化，当前行为见[配置与启动](../../../../../skills/oathx/references/development/CONFIGURATION.md)。

2026-11-25，独立静态复核者 /root/town_review；未修改源码或操作 Unity。

未发现新增阻断问题。cfgBundle/bundleSource、按资源名映射配置类型及 Excel 字段写入符合 development 机制；系统配置校验位于 InstallSystems。文档区分逐字段写入与事务，并保留 Local 仅 Editor、Bundle Player 未实测边界。代码/架构报告的源码哈希匹配。

code-config-final.json 为 passed，仍有 OX017 条件编译分支、OX016 既有 Home 文案两项 review。architecture-config-final.json 无 issues，但反射和非激活条件编译分支等静态限制仍适用。不能称为零提示或完整运行验收。

实施者实测：config-source-evidence.json 中空配置、缺表、重复表均拒绝，缺目录阻止系统安装，原 cfgBundle 恢复；随后 verify-session.ps1 全部通过。此前玩家回归通过。故障注入期间预期打印 MINIWOW_CONFIG_LOAD_FAILED，不作为正常启动错误隐瞒。

复核时学习索引含 556 文件；随后补入已阅读的 export.bat、install.bat、generate.sh 三个工具脚本，最终 559 文件。索引扩展不改变运行代码或上述验证。

本地源码包已重新生成，主包 126 文件，含 XAppConfigure.asset 和 meta。向绑定 GameCLI 远端保存被自动审批拒绝，原因是未取得明确的本批私有源码外发授权；本次没有完成远端同步。project-sync.json 中的旧记录不能作为本次同步成功证据。
