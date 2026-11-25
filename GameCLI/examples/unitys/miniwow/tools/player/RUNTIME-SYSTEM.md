# RuntimeSystem 基类整理

2026-11-25：参考 development/client/Assets/Scripts/Systems/RuntimeSystem.cs，在 miniwow 同路径引入共享基类。HomeSystem 与 TownSystem 改为继承 RuntimeSystem；该类继续继承 XEventModule。

保留模板 LoadSystem(Type, Action) 和 UnloadSystem(Type, Action) 的调度、激活与帧末回调语义；仅整理方法顺序、空行和英文注释。帧末回调不保证场景、UI、资源已就绪或已释放，原 TownReadyChanged 与生命周期版本校验保持不变。GamePlugin 初始装配不变。

验证：Unity 编译通过；Home/Town 的 GameCLI 检查及 RuntimeSystem 的同一分析器检查均 passed、0 error；Home 另有一项既有 OX016 review：UIHome.FormatAmount 的“万”单位字符串，本次未修改该显示格式；verify-player.ps1 通过启动/重建、地图切换中断、移动停止、Shape 隐藏及两种竖屏比例；Console error=0。报告见 code-home.json、code-town.json、code-runtime.json 与 runtime-evidence.json。

源码包由 prepare_studio.py 收录 RuntimeSystem.cs 和 Unity 生成的 .meta，公共基类归 UIHome 根源码包，Town 继续依赖该公共包。未发布标准模板或提交 Git。

独立静态复核：town_review 确认基类与 development 行为一致，Home/Town 仅继承变更，原就绪和生命周期机制不变。
