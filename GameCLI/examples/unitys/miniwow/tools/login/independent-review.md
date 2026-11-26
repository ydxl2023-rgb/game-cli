# Master 与本地 Login 独立验收

日期：2026-11-25。审查者：town_review，未参与本轮实现。

本轮产品功能的独立静态与运行验收 passed，未发现未解决业务阻断。正式 Login prefab 检查原始状态为 **failed**，存在已解释的输入遮挡差异及比较器覆盖限制；不能将其改称工具通过，也不作“所有检查全通过”的结论。范围为已批准的本地开始游戏：不含网络认证、区服或存档选择。Master v9 / SvLogin v11 是结构参考，未声称完整照搬其业务或UI。

## 身份与静态范围

独立 requireVerified 核对项目 p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3、Unity 83c29c9f-21c1-4597-9691-6707ca146fa7，路径为 miniwow/client；ProjectSettings/GameCLIProject.json一致。

审查 MasterSystem/MasterEntity、LoginSystem/LoginEntity、UILogin/VMLogin、GamePlugin、HomeSystem、RuntimeSystem、底层dispatcher与UISystem取消契约。MasterEntity拥有会话阶段，LoginEntity拥有请求与重试状态；Plugin完成配置后只启动Master。模块新增延后一帧，不在事件枚举中直接添加；Home实际就绪后才提交会话并启用Town输入。停用清订阅并取消旧代际。

初审两项缺陷已修复并复验：Home局部ready遮蔽字段导致永不就绪，已改widgetReady；重复Active重置会话，已增加requestedActive门禁。当前运行可成功进入Town并验证重复激活不重置状态。

## 验收矩阵

| 项目 | 状态 | 实证 |
| --- | --- | --- |
| 登录门控与开始按钮 | passed | 独立verify-session.ps1：Login先打开且Town未就绪、输入关闭；真实按钮射线首命中检查后经pointerClick派发，重复请求被拒绝。 |
| 实际就绪交接 | passed | Master进入Town前等待Town与Home；提交后Login实例0、Home打开、Town就绪且input=true。 |
| 失败重试 | passed | 内存注入无效Actor配置，退回Login、显示失败提示且输入关闭；finally恢复配置后重新进入成功。未修改源表或资产。 |
| 取消与退出 | passed | Entering阶段Return及Stop→立即Active均不被旧任务恢复；稳定Town停用后无Login/Home/Town输入残留，重新激活可再进入。 |
| 重复Active | passed | Login与Town阶段的重复激活探针通过，无重复页面或会话重置。 |
| VM与Metadata | passed | 独立widget-metadata --dry-run：fieldCount=4、boundTargetCount=4、metadataAdded=false、issues=[]；运行验证开始、进入、失败显隐与请求绑定。 |
| 画面 | passed | 独立查看本次生成login-ready/failed/tall.png，Logo完整、中文开始按钮和失败说明可读；1080×2400按钮可点击。图片为原生Camera+Canvas渲染，非桌面截图。 |
| 编译/Console | passed | 主agent编译成功；独立冻结版运行及Console查询error=0。 |
| 代码检查 | passed | code-login.json、code-master.json均passed/0issues；独立逐项核对源码SHA256匹配。检查器由主agent执行。 |
| Login正式prefab检查 | failed（原始） | Studio v23的prefab-verification.json仅有一项difference：Background.raycastTarget期望false、实际true。实际值用于全屏输入遮挡，为正确产品行为；未修改验证器或移除遮挡来获得绿色结果。unverified按下文逐类补证，原始状态不变。 |
| Master UI/VM/预制 | not-applicable | Master纯代码，没有占位UI。 |
| 联网、区服、存档选择 | not-applicable | 本地模式明确排除，不用假身份冒充认证。 |

## 原始证据

独立先Return回Login，再执行tools/login/verify-session.ps1，退出码0、输出PASS。完整步骤与响应、Metadata dry-run、Console、源码及预制SHA256保存于 independent-evidence.json。运行约06:51–06:52 UTC；结束保持Town就绪、1080×1920，已交还Editor。未改业务实现、资源或停止播放。

编译依赖变化另行静态检查：Assets/csc.rsp和Standalone PlayerSettings启用USE_OLD_TEXT；WidgetMetadataPrefabGenerator移除可选SyncTextMeshPro硬引用后，仍经TypeCache解析ISyncAction。运行InspectFonts与三张预览验证当前中文Text呈现；未进行独立Standalone构建或TMP配置回归。

## 覆盖限制

本轮Entering取消探针发生在延迟加载提交前；没有伪称穷尽每个异步加载时间点。失败测试是Actor配置提前拒绝，不是注入Home/Town加载超时；部分加载回退经静态审查，未独立模拟40秒超时。玩家移动完整回归由主agent另行执行，本验收没有重复其全部步骤。没有进行原作联网或物理触屏端到端测试。

## 正式 prefab 原始结果及逐类补证

已独立读取tools/login/prefab-verification.json，projectVersion=23，status=failed、passed=false、differences=1；bindings.status=passed、4字段，renderOrder.status=passed。唯一差异是Background射线拦截；比较器只为Button建立可接收射线的期望，无法在当前契约表达全屏背景拦截需求。此差异不构成产品缺陷，仍保留工具失败记录。

| unverified类别 | 补充证据与剩余边界 |
| --- | --- |
| UILogin、Metadata自定义组件未支持导出 | 独立审查UILogin/VM；widget-metadata dry-run为4/4，正式bindings也为4字段passed。运行点击、重复请求、进入/失败/恢复显隐通过。不声称导出器已支持所有组件字段。 |
| script-binding-contract-unavailable | 实际四项为EnterClick、EnterVisible、LoadingVisible、FailedVisible；真实首射线按钮点击及进入/失败/恢复快照证明本轮目标语义。Studio源包未提供比较器认可的产品目标映射，原始unverified保留。 |
| Image.m_Sprite与resource-hash-unavailable | 独立读取prefab的sprite GUID并对应.meta：背景736d5d047cf88464c94b59b168a7eb70、按钮8e11a25a9c46be64f93028462321e017。磁盘PNG的SHA256分别为B0A44A6A5ED48A38F98A3B3B80D613EB1AEF2C7BA0F8ED98E136B5F417BFAB34与97FD24A254977713B763F0967564A06E0B96B51EFF6B21CD7871878B90006C9A，匹配正式报告expected及materials.json。 |
| Text字体对象未支持导出及font-contract-unavailable | InspectFonts运行输出与ready/failed/tall预览验证实际Text使用及可见中文；预览文字完整。Studio没有可比较字体身份，因此补证只证明当前展示，不宣称与未定义的字体设计身份相等。 |
| Background.AspectRatioFitter未声明组件 | 原生适配用于背景比例与顶端Logo保留；1080×1920、1080×2400预览均经独立查看，按钮射线通过。比较器未表达该组件，不删除组件或篡改基准来消除提示。 |

Studio源包组织现为两个独立根节点：MasterSystem为systemKind=code、无UI；UILogin为独立页面包；Home保留共享启动包。这是源包归属调整，不是本轮运行源码变更；纯代码Master的预制检查仍不适用。
