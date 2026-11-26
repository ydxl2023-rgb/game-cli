# 通用任务与引导修复

2026-11-26。此记录替代 TASK-60105-ENTRY.md 中将 GuideFinger 直接解释为打开地图的临时方案。

## 配置边界

- TaskEntity 继续承载规则与状态。TaskProgressReported 不指定 taskId（0）时按条件类型、Param1/2/3 匹配全部已派发任务，分别应用配置 FinishValue。绝对进度保持幂等；旧代码显式 taskId 的定向观察保留兼容。
- HomeVillageRuntime 改为广播配置条件观察，不把进度限定到单个任务实例；任务派发和奖励领取仍用明确任务 ID。
- GuideFinger(target) 创建同一个 TaskFingerAction。目标是 UI 注册标识，不执行路径查找，也不读取任务 ID。相同类型换 ID、数量或已注册目标仅改原 Task / TaskGuide 配置。
- TaskGuideTarget 随页面启用注册、停用注销，负责原作手指资源和真实 Button 点击；取消、关闭及同帧关闭重开均结束原引导。正常按钮业务仍由页面自己的事件处理。
- Home 已删除 HomeGuideTargets 与 OnTaskPresentation 专用匹配，不再把手指动作当作地图打开命令。
- 未注册目标明确抛出带目标标识的错误；尚无消费者接受的其他表现类型明确失败，不能永久保持 Running。新增动作类型才增加处理器；现有外部消费者须设置 TaskPresentationRequested.accepted，并承担完成和取消。

## 资产

原手指资源在 imports.json 记录路径、用途及 SHA256。finger.json / finger.materials.json 通过正式 UGUI dry-run + write 生成 UITaskGuideFinger.prefab，所有 Graphic 禁用射线。

targets.json 是编辑期 UI 适配绑定清单，MiniwowTaskGuideSetup 在 bind.request 存在时写入序列化标识和引用。新增已有 UI 目标只增加绑定数据，不增加运行时代码分支。重建 UIHome 后应重新执行本绑定步骤。原配置字符串目标保留兼容，不额外发明任务字符串 ID。

## 验证

MiniwowTaskGuideProbe.Verify 使用隔离任务和临时 UI 对象，不写玩家存档。覆盖：不同任务 ID/数量、不同参数排除、绝对进度幂等、两个不同 UI 目标共用动作、等待真实点击、取消、关闭立即重开。

实际正常登录成功，Launch 已隐藏。点击60105任务条后 CampVisible=true、MapVisible=false，截图确认手指；点击入口401后地图可见，返回可用。任务仍60105 Active，金币18740、草药50、药剂0。规范检查含Task/Editor，独立审查已修复关闭重开生命周期问题。

探索地图仍为旧预览；本次没有实现探索事件、战斗或伪造60105完成。
