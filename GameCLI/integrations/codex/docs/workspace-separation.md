# 模板页与项目页

- `/studio/projects`：项目列表。`/studio` 保持兼容，打开项目列表。
- `/studio/projects/:id`：项目编辑器，保存项目副本、关系图、系统设计聊天及开发状态。没有模板候选或发布入口。
- `/studio/templates`：模板分类列表，可查看已发布版本、版本历史；模板管理员可打开独立草稿。
- `/studio/templates/:id`：只读已发布版本，默认打开与项目一致的节点、画布、属性和控件布局；预演由用户主动进入，保留原始分辨率与 RectTransform。
- `/studio/template-drafts/:id`：模板草稿编辑器，共用节点、UGUI 属性、预演、代码查看与标准动画功能；不包含项目关系图或系统设计聊天。
- `/studio/templates/review`：独立候选审核列表，保留预演、差异比较、发布与拒绝。

## 数据与写入边界

新增 `gamecli_ui.template_drafts`，首次使用时自动建表。草稿按模板、维护人和基础发布版本隔离，revision 为独立的编辑版本。已发布模板仍使用不可直接覆盖的 templates/template_versions 表；生产项目继续使用 project_ui。

打开草稿复制已发布模板，不创建生产项目。重复打开同一维护人、同一基础版本时继续已有草稿。保存采用乐观锁，只更新草稿；提交读取已保存草稿生成候选，不接受客户端替换候选内容。候选 project_id 为 null。相同草稿 revision 重复提交返回同一候选。标准模板已有新版本时拒绝旧草稿提交，用户从模板页打开基于最新版本的新草稿；旧草稿保留。

HTTP：

- GET `/api/studio/templates/:id`：只读已发布模板快照。
- POST `/api/studio/template-drafts` `{templateId}`：管理员创建或继续本人草稿。
- GET `/api/studio/template-drafts/:id`：读取本人草稿。
- POST `/api/studio/template-drafts/:id/save` `{draftId,revision,design}`：保存草稿。
- POST `/api/studio/template-drafts/:id/submit` `{draftId,revision}`：提交已保存版本。

项目 `/candidate` 写入接口和 `gamecli_ui_candidate` MCP 工具已移除。历史项目候选仍可在审核页查看，以避免丢失旧工作；新候选必须从独立模板草稿或既有源码提取流程提交。模板发布不会自动升级项目；项目仍需明确升级自己的模板副本。

模板与项目共用同一套面板尺寸、拖动分隔条、画布缩放和 UGUI 属性编辑组件。模板编辑区不增加关系图和系统设计聊天；代码列表仅在代码分页显示，保持编辑页右侧控件布局一致。模板资源分页选择独立保存，不覆盖项目的系统设计分页选择。
`/studio/templates` 直接进入共用编辑布局，底部可选择预制控件或系统模板；模板目录移至 `/studio/templates/library`。已发布模板仅查看，通过“编辑独立草稿”进入可写草稿。模板工作区不提供项目关系和系统设计聊天。

## Unity 工程绑定

GameCLIApp (`p-2d314388-18c4-42f5-80eb-f6729dd23f0e`) 已关联 `examples/unitys/development/client`。
数据库 `unity_bindings` 保存项目与工程 UUID 的一对一关系，`unity_locations` 保存每台机器的绝对路径。工程内 `ProjectSettings/GameCLIProject.json` 保存两个 ID，不含本机路径，应纳入版本管理。当前机器标识使用主机名。

在切换项目 → 创建 / 管理项目 → Unity 工程绑定中配置。只允许 owner 建立绑定；不能覆盖别的项目或更换工程 UUID。移动工程后保留关联文件，在对话框确认更新本机目录。没有关联文件的已有绑定不能静默重新初始化。

GET `/api/studio/projects/{id}/unity-binding` 返回 unbound / unavailable / mismatch / verified；POST 同一路径，参数 `{projectId,path,confirm?}`。MCP 提供 `gamecli_ui_unity_binding`（requireVerified）与 `gamecli_ui_bind_unity`。导出接口强制校验双向身份并返回目标绑定信息；导出物仍在项目独立输出目录，不自动写入 Unity。

设计聊天上下文包含绑定信息，但目前仍仅生成方案，没有可执行开发任务的自动派发器。后续 UnityCLI 开发、源码同步应先调用严格绑定校验，不能把“已绑定”理解为 Unity Editor 正在运行或已建立实时连接。独立外部脚本不会被此 HTTP 服务自动拦截，仓库 AGENTS.md 已规定这些流程的校验要求。

绑定过程若在文件落盘后数据库事务失败，关联文件保留以便相同项目重试恢复，绝不覆盖冲突文件。此时状态不会被报告为已验证。
