# 加伦移动朝向修复

问题来自场景制作，不是寻路路线：MiniwowTownSetup 曾把加伦模型旋转直接复制自旅店NPC。运行诊断显示加伦旋转为 `(325,45,0)`，其模型正X与相机正X点积为 `-1`；玩家旋转为 `(30,225,0)`，点积为 `+1`。ActorShape按屏幕运动方向翻转Skeleton，因此加伦出现视觉反向。

原作 `research/code-study/managed/decompiled/HotUpdate/Zombie/BuildingHero.cs:510` 明确将英雄模型设为 `Quaternion.Euler(30,-135,0)`；玩家制作脚本已经采用这个角度。现在 Town/Village 两个加伦制作入口均恢复该角度，当前Home场景通过 MiniwowActorFacingSetup 的原生EditorSceneManager流程保存。保留1.65缩放、原NavMesh、实体位置与ActorShape方向计算，不对所有NPC盲目反转符号。

验收工具 `MiniwowActorFacingProbe.Inspect` 读取各实际模型轴；`VerifyDirections` 使用隔离ActorEntity，分别向屏幕左/右推进并绑定实际加伦与玩家Shape，检查显示正方向与移动一致，finally恢复原实体绑定，不写任务或经济数据。

本轮证据保存在 `client/Library/OpeningQA/actor-facing-evidence.jsonl`；用户存档单独备份至 `actor-facing-user-checkpoint.json`，验收后必须恢复三个键并停止Play。

实际复验：加伦旋转恢复为 `(30,225,0)`，屏幕正X点积由-1变为+1；加伦与玩家左右两方向测试全部通过，Console零错误，新增编辑器工具代码检查零问题。三个用户存档键已逐值比较一致并停止Play。检查没有修改任务、寻路或移动速度规则。
