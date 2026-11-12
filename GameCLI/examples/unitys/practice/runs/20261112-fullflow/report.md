# 五模板全流程试跑

测试工程：`examples/unitys/practice/client`。Studio 项目为 Practice，设计 revision 3。范围为 Launch、Login、Loading、Master、Notice；标准模板版本、需求、工程身份见 brief.md。模块开发与独立验收由不同 agent 承担。

**综合结论：failed。** 基础交付与运行链路已执行，但 Login 本地选区需求缺失，同时仍存在预制验收证据缺口。不能据此认定五个模板已全部验收通过。

## 已完成的交付链

读取并冻结数据库模板与源码 → 独立模块适配 → UnityCLI 生成真实资源、Metadata 和动画 → 集成 App 启动场景 → 独立代码复验 → 独立预制与运行验收。

启动场景已连接 Launch；PracticePlugin 创建实体并启动 Master，由 SystemActive 打开 Login。公告通过 Login 请求 Master 打开和关闭；开始游戏仅发送明确标识的本地 LoginSucceed，不执行网络认证。Loading 只提供 UI/VM 与进度展示接口。

## 代码与开发自测

| 模块 | 代码检查 | 资源自测 |
| --- | --- | --- |
| Launch | 位于框架包，超出 verify code 系统目录范围，另有人工检查 | 23 节点，7 个必需引用；动画 4 曲线、8 关键帧读回匹配 |
| Login | passed，独立复验完成 | 布局差异 0，17 项绑定通过，最终路径 Login/Login.prefab |
| Loading | passed | 布局差异 0，2 项绑定通过，4 曲线循环动画 |
| Master | passed | 纯代码，无预制，不适用预制验收 |
| Notice | passed，独立复验完成 | 12 项绑定通过；标准 0.22 秒打开动画 |

这些是检查子项结果，不能据此认定系统整体通过。独立 verify prefab 结果如下；原始 incomplete 状态保留。

| 预制 | 布局/组件差异 | 未验证项 | 原始整体状态 |
| --- | --- | --- | --- |
| Launch | 0 | 34 | incomplete |
| Login | 0 | 101 | incomplete |
| Loading | 0 | 24 | incomplete |
| Notice | 2 | 55 | incomplete |

Login、Loading、Notice 的静态绑定通过；Launch 无 Widget/VM，其零字段绑定结果不证明组件引用正确。Launch 需单独检查序列化引用及实际启动行为。

独立运行首轮完成 37 项状态断言，涵盖 VM 更新、本地登录事件、公告三次开关和 Loading 八种进度边界。稳定帧 Login/Notice 截图可见；公告入口与关闭按钮的中心射线命中正确，实际 pointerClick 进入公告。两个超链接的真实字符区域命中通过，没有打开外部浏览器。公告 Panel 的多帧缩放为 0.9563 → 0.9999 → 1，外层保持 1。完整逐项结果和范围以 acceptance/runtime-review.md 为准。

补测确认 Login 经 SystemDetive 关闭、SystemActive 重开后，开始游戏与公告入口仍可用；Loading 连续两次打开、刷新、关闭通过。尚未完成的监听计数、Master 自身卸载重启、加载中取消、偏好持久化和链接拖动/裁剪负例均保留 incomplete。

Launch 核心展示接口另有 11 项独立断言通过：实际 XApp 序列化引用、Loading 0/38/100% 文本、提示、下载面板切换及文本、0.375 进度条与指示器位置、仅背景恢复。完整自动启动每帧时序未作证明。

## 已确认问题

1. Login 发布模板缺少本地区服选择页面、数据和逻辑，切换区按钮只能提示尚未接入；试跑要求中的本地选区失败。
2. Notice 发布模板有空状态绑定但缺少对应节点；本轮增加 Empty 节点以实现空状态，没有修改冻结设计来掩盖差异。另一项原始差异是 MainText 使用 NoticeText 子类，而比较器只按 UnityEngine.UI.Text 精确类型计数；已由继承关系、实际文本和超链接命中补证，但没有改写原始报告。
3. Loading 一张图片声明为 PNG，实际内容是 JPEG；本轮仅按真实解码尺寸修正生成输入，保留原字节与哈希。
4. 工具尚不能完整证明字体、资源身份、全部自定义组件和动画契约，预制整体验收不能标为通过。
5. Notice 原始模板将全屏半透明遮罩排在 Panel 后面，实际会同时压暗面板；这是沿用模板的视觉问题，未擅自更改原始设计。

已修复并复验：Login 对非缓存页面的错误 GetWidget 调用；Login 的 SVLogin 路径与根名不匹配；Notice 新增空文案绑定。

## 证据

- 独立代码报告：acceptance/code-review.md
- 独立运行和预制报告：acceptance/runtime-review.md
- 集成说明：integration.md
- 原始设计：project-final-design.json
- 模板来源：templates/
- 模块生成与自测：inputs/、各模块 development.md
- 原工程文件变化：baseline-changes.json；841 个原有文件中仅 App 场景、PracticePlugin、PlayerSettings、独立包中的 XLauncher 发生修改。

当前工程已安装五个模板，不再是空白基线。本轮未提交推送、未发布回标准模板库；完整多语言导表、真实登录和其他业务系统不在本轮实现范围内。

运行测试结束后已停止 Play，通过 UnityCLI 删除临时验收探针，清理编译通过，Console 零错误。证据源码仅保留在 run 目录，稳定帧截图位于 acceptance/screenshots/。
