# 60105 任务条入口

> 已被 [通用任务与引导修复](../task-guide/README.md) 替代。下文记录的是已移除的直接跳转方案；当前使用注册目标和真实点击完成引导。

2026-11-26：点击主界面任务条，将未绑定村庄事务且有 GuideID 的当前任务交给 TaskSystem 执行配置引导。60105 使用 1018001，配置为 GuideFinger("Main/MainPage/ExploreBtn")；Home 对此原作目标做入口适配，直接显示既有地图预览。

依据：research/config-study/CHAPTER-EXPERIENCE.md 的 60105；原反编译 UIMainTabsCom.OnClickExploreBtn 打开 Level/ExploreMapPage。任务的 AdventureEventTimes/4 条件与 UI 点击分开。

本次仅接通任务开始入口，未实现探索地图上的英雄事件模拟，也不把地图打开当作60105完成。没有新增配置 ID、改写任务进度或发放奖励。现有完成任务的点击领取逻辑保留。

验证：真实 TaskClick 射线点击后 MapVisible=true，ReturnClick 返回后可以再次点击任务条进入；任务仍为60105 Active，金币18740、药剂0、已有领取记录保持不变。Unity编译、本地规范/架构检查、独立只读复核通过。

验证限制：运行态真实按钮射线及 VM 显隐验证通过；本轮图像验收未通过。测试期间重复加载 App 导致两个 XApp 与残留 Launch，清理后没有成功重建完整登录画面，因此不把截图列为已通过验收。已退出测试播放，未修改用户存档。后续完整探索开发应从干净 App 启动重新做画面验收。
