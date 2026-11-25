# 原作摇杆与导航修复独立验收

日期：2026-11-25。审查者：独立子 agent town_review，未参与实现。

本轮独立验收总体 passed：运行、静态正确性及最新规范检查报告复核均通过，未发现剩余阻断问题。此前玩家首版报告不能证明本次修复；本文件已按当前原导航版本重新记录。

## 独立检查与实证

| 项目 | 状态 | 结果 |
| --- | --- | --- |
| 原导航数据 | passed | 独立读取提取的 navmesh-mesh.asset.json 与 Library/OriginalNavigationSerialized.json，两者均30个tile；逐项m_MeshData相同，16字节hash全部一致。没有把重新烘焙描述为原数据。 |
| 摇杆原像素 | passed | 原74×74 common_bg_2与214×214还原图在(70,70)-(144,144)逐像素相同，透明bbox也为该区域；底图按原0.8比例转换，方向图引用原common_bg_12。 |
| 原逻辑对照 | passed | 直接读取已提取BuildingPlayer.MovePlayer及UIJoystickCom：NavMeshAgent.Move、非零意图run、48源UI单位半径、输入方向与相机XZ投影已对应；接口转换保留数据/显示分离。 |
| 所有权/生命周期 | passed | 独立NavMeshAgent位于场景代理而非Shape子树，实体只持IActorMovement；Release禁用代理，实体Destroy清适配引用并拒绝后续动作；不再由Shape写位置。 |
| 原可走区 | passed | 独立CompareCoverage返回原导航872个采样命中，旧rebake729个；这是指定网格采样数量，不能解释为全地图面积。 |
| 边缘滑动 | passed | 独立VerifyWallSlide找到12个实际原导航边缘案例，沿切线越过旧射线停止点并仍落在原导航上。 |
| 玩家回归 | passed | 独立verify-player.ps1退出0/PASS，覆盖输入/时间非法值、数据隔离、取消、隐藏Shape继续移动、重绑、run/idle、动画时间推进、镜头、地图停止、快速重启及两种竖屏比例。 |
| 坡面 | passed | 当前VerifySlope使用NavMeshAgent代理，验证连续上坡及XZ预算，不沿用上一版三维速度预算结论。 |
| 画面 | passed | 已查看本次player-moving.png：小摇杆帽、原底图、方向箭头、玩家run均可见；图片为实时原生Camera+Canvas渲染，不是桌面截屏。 |
| Console | passed | 独立运行后error查询0条。主agent执行编译，独立调用冻结版运行探针成功。 |
| 代码规范 | passed | 主agent重新执行code-actor.json与code-town.json，均passed、issues为空；本审查者独立核对全部报告源码SHA256与磁盘相同，并核对本轮运行冻结源码/资产哈希未改变。没有冒称独立执行检查器。 |

## 文件与结束状态

本轮独立命令的完整结构化记录、玩家回归步骤、当前源码和关键资产SHA256位于 independent-original-evidence.json。CompareCoverage与VerifyWallSlide还分别生成navigation-coverage.json、wall-slide-evidence.json。原文件一致性由本审查者以Python逐项和逐像素比较，未修改原资源。

结束Snapshot：玩家id=4、config=10001，位置(-23.90721,35.1613922,14.11279)，Idle/idle、Shape启用，导航顶点1973。配置恢复原出生点Y=35.17353，运行Y为原NavMesh采样表面高度。已恢复营地1080×1920，不再操作Unity，未改实现。

## 不作过度结论

- 本轮没有恢复全部原版UI行为：项目在静止时隐藏整套摇杆，原作控制器默认态另有表现。
- 恢复原出生点后旧旅店点击探针的目标不在当前视野，因此没有通过改变出生点来伪造旧Town脚本通过。
- 测试通过真实场景指针处理方法进入；未声称完成物理手机触摸、所有原版地图边缘、自动导航、水域或建筑触发业务验收。
- Town根节点禁用仍为退出解绑语义；恢复须经模块生命周期。模型Shape隐藏不会停止逻辑代理。
- 导航tile完全一致不代表Unity版本间的全部运行行为完全相同；本轮已对可走区、坡面与贴边滑动做上述实际验证。
