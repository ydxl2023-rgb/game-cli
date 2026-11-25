# 固定摇杆与跨 UI 捕获独立复核

日期：2026-11-25。审查者：town_review，未参与本轮实现。结果：本轮静态与专项运行验收 passed，未发现阻断问题。

## 目标与静态检查

本次用户要求摇杆固定、捕获后的拖动越过 UI 不停止。原 UIJoystickCom.OnTouchBegin 会重定位摇杆，本实现有意遵循最新用户要求，不能宣称此行为与原代码完全一致。

- ActorJoystick.Begin 只记录输入起点，保持 Stick 原生序列化位置；ResetInput 归中帽子并隐藏方向标记，不隐藏固定底座。SetVisible 由 Town 输入门禁控制。
- 安装器将 Stick 底部锚点设为(0.5,0)，位置(-1.5,511.5)，对应原 MainPage pivot(359,1219)、720×1560源坐标按宽度1.5转换。未改运行时跟随触点摆放。
- BeginPointer 仍拒绝 UI 上起手；已捕获 MovePointer 不再因经过 UI 或越过视口边缘取消。释放时先清零输入，再拒绝 UI 上的场景点击。
- 地图门禁、暂停、失焦、指针所有权及多指取消保持有效；退出仍由宿主释放代理和实体。

## 本审查者独立执行

使用明确 miniwow/client --project 的 UnityCLI：

- VerifyPointerCapture=True：UI起手拒绝，场景起手捕获，跨UI保持原指针所有权，帽子移动而底座不动；UI上释放归中、玩家Idle、固定底座继续可见。
- VerifyCancellation=True：错误指针不能接管/释放，失焦与暂停清零动作。
- Console error=0。结束Snapshot为原出生点(-23.90721,35.1613922,14.11279)，玩家Idle/idle、Shape启用、导航顶点1973。

原始响应及本轮源码/场景/预制SHA256保存于 fixed-joystick-independent.json。没有修改实现或停止播放。

## 证据边界

主agent执行含新探针的完整verify-player.ps1并报告PASS，独立验收没有重复全部导航回归。专项探针通过真实TownScene入口调用，未替代物理手机触摸/操作系统窗口外释放测试；视口外拖动的保留逻辑经静态确认，专项运行重点覆盖跨UI。原生idle预览由主agent检查，本报告不将其描述为独立桌面截图验收。
