# 业务系统：状态归属、流程与边界

以下路径均以 [Assets/Scripts](../../../../examples/unitys/development/client/Assets/Scripts) 为根；逐文件定位见 SOURCE-INDEX。这里描述静态实现，不把存在代码等同于运行验收。最新约定要求具体业务系统之间不互相引用；参考工程中的旧引用不得成为复制依据。

## 通用职责

Entity 保存业务状态、校验及变更规则；System 订阅请求、协调实体与页面、发布结果；Widget 持有 VM、绑定交互并投影实体数据。VM 是显示绑定，不是业务存档。RuntimeSystem 继承 XEventModule，封装系统装配请求；装配的实际所有者仍是 GamePlugin。不要在 Master 中缓存各系统实例或用泛型查询绕过系统隔离。

Widget 的 Bind 发送意图，Invalidate 更新显示；不要让 System 获取 VM 后逐字段赋值。动态列表保存稳定业务 ID，不能把可变化的显示序号当作实体 ID。资源和订阅应由创建它们的生命周期负责释放。

## Login、Master、Function、Home

|系统 / 主要入口|状态和流程|可复用机制|产品迁移限制|
|---|---|---|---|
|Login：LoginEntity、LoginSystem、UILogin、UIServerSelect|实体校验并复制服务器目录；维护选中/最近服务器，维护状态禁止进入；Widget 按最近、推荐及分组展示|稳定服务器 ID、实体入口校验、统一 StorageKey、界面选择与业务状态分开|System 构造的 12 个样例服务器不是正式目录；不要带入 miniwow|
|Master：MasterEntity、MasterSystem|加载/恢复主实体，响应登录和入口事件；待处理系统请求延后执行|明确阶段、事件协调、避免分发时修改注册表|历史 Type/Action 事件与具体系统调用不符合最新隔离要求；帧末回调不代表场景完成|
|Function：FunctionEntity、FunctionSystem、FunctionAccess|按等级、开关、UTC 时间窗计算开放性；轮询仅发布变化；停用停止时钟|规则在实体，配置驱动，变化通知|未知条件失败；FunctionAccess 的具体类型装配不是业务系统相互调用的许可|
|Home：HomeEntity、HomeSystem、UIHome|汇集玩家展示、货币、入口和徽标；用户意图交系统处理|复合 Widget、刷新投影、入口业务 ID 与显示枚举分离|测试徽标/默认玩家数值不应硬编码进产品；参考横屏布局不覆盖 miniwow 竖屏|

MasterEntity 的初始金币、钻石、等级等是样例默认值。首次建档规则必须来自产品配置。Master 的职责是监听产品事件并调度系统，不自行建立平行会话阶段状态机，不接管场景移动或 Building 的升级规则。参考列中的“阶段”不构成在 miniwow 引入 MasterPhase 的依据。

## Mail 与旧 Email 不可混为一套

`Systems/Mail`：MailEntity 深复制邮箱和奖励，校验正 ID、重复项和有效期；处理已读、领取、删除。领取收据用于阻止同一邮件再次领取，列表按规则排序。UIMail/系统中的样例资产会调整日期用于演示，不能作线上邮箱来源。

“记录领取”不等于“奖励已发入背包”：接入产品时必须定义奖励提交与领取收据的一致性。本轮不自行实现这套经济事务。

`Systems/Email` 是较早的 SDK/RPC 邮件流程：读取网络实体，System 修改领取状态并发布 GrabItemEvent，奖励解析较粗。它与本地 Mail 的快照/收据模型不同。选择哪套机制要依据产品联网决策，不能同时复制两者造成双份权威状态。

## Shop

ShopEntity 从 ShopOffer、ShopSetting、ShopPageItem 等配置组织商品与页面；选择商品和确认购买分离。确认时重新检查库存、余额和溢出，再扣除/发放、清除待确认项，避免重复确认。同一页面按钮禁用不替代实体校验。

ShopSystem 协调打开、购买、反馈；UIShop 负责子页异步缓存与当前页展示，用代次过滤过期加载，释放子页时解除绑定并销毁实例。加载成功前不要先丢弃有效旧页。UIShopProductList 使用配置商品 ID、列表绑定与布局；不要用位置索引决定价格。

UIShopGift/Recharge 的历史 Invalidate 中仍有实体分类选择等混合职责；只借鉴资源和列表机制，不把这段当成新的职责标准。MonthCard 是目录展示组件，不是完整月卡购买/每日权益结算。参考本地余额操作不是支付认证。

## Settings、Notice、RedDot

SettingsEntity 校验枚举、浮点有限值与范围，管理预设和持久化副本；System 用统一变更路径刷新/通知，Widget 管理当前页签。SettingsActionRequested 是请求事件，不证明注销、删除账号等操作已实现。配置项存在也不证明音频/画质已应用到 Unity 对应 API，须核对消费端。

NoticeEntity 包含本地演示文案和提醒抑制时间；旧存储键不符合新三段命名。NoticeText 解析链接并根据文字网格计算可点区域，限制 URL scheme，拖动阈值避免滑动时误打开；NoticeLayout 处理内容高度。复用交互算法时保留这些约束，不复制演示正文。

RedDotEntity 校验节点 ID、父节点、循环与模式；计算 SignalCount/聚合数量并限制溢出，禁用节点不展示。RedDotSystem 用集合合并刷新，帧末统一更新，停用取消协程；RedDotScope 通过序列化绑定和启停接入。装饰徽章、通知红点和库存计数不是同一种业务状态。

## 尚不完整的模块

|模块|源码实际内容|不能据此宣称|
|---|---|---|
|Backpack|槽位列表、Grab/Drop；DropItems 顺序处理|批量操作原子性、完整容量/堆叠/奖励事务|
|Character|实体与页面骨架，主要为打开/关闭|成熟角色养成系统|
|SevenDayLogin|配置天数、选择与时间计算；GiveReward、UIClaimSucceed 留空|完整签到奖励闭环|
|Fight|FightManager 管理实例，Entity/Shape 分离；Load/Ready/Fight/Over 状态骨架|完整伤害计算、寻路、AI 或战斗结束结算|
|Loading|Widget 展示消息与约束后的进度|需要额外建立一个业务系统|

Fight/ActorEntity 的构造和属性初始化有可疑实现：`level = level` 没使用传入默认等级；先设置 propId 再按“不相等才初始化”的逻辑可能跳过属性装载。ActorShape 的状态机结构值得参考，空的 AIAction/Move/Idle 不代表算法已实现。迁移前需做独立用例，不在本次研究中擅改参考工程。

## Common 与协议

UICurrency/UIProfile 等复合 Widget 避免重复 Ref；Unref 时重置绑定，子 VM 要解除关系。UIActivityEntry 将徽标展示交同步适配器，按生命周期释放；固定节点通过序列化引用，不按路径搜索兜底。

CommonText 通过稳定文本 ID 读取配置并格式化；StorageKey.For 使用直接成员表达式取得类型和成员名，不编译执行表达式。两者应复用，不重新写字符串拼接工具。

UIRequestEvent 携带来源及帧信息，UI 输入通过 UISender 进入调度。Protocols 下的 DTO/接口和 Rpc.Load/Save 是网络适配，Rpc.Load 可能先用本地档案再向服务端请求；不能由接口存在推断可靠认证或云存档冲突处理已完成。
