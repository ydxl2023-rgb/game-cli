param([Parameter(Mandatory=$true)][string]$AcceptanceEndUtc)
$ErrorActionPreference='Stop'
& "$PSScriptRoot/measure-usage.ps1" | Out-Null
$usage=Get-Content "$PSScriptRoot/usage-latest.json" -Raw|ConvertFrom-Json
$baseline=Get-Content "$PSScriptRoot/benchmark-baseline.json" -Raw|ConvertFrom-Json
$end=[datetimeoffset]$AcceptanceEndUtc
$duration=$end-[datetimeoffset]$baseline.startUtc
$requestDuration=$end-[datetimeoffset]$baseline.requestTimestamp
$humanHours=$baseline.humanBaselineDays*$baseline.hoursPerDayAssumption
$noncached=$usage.usage.input_tokens-$usage.usage.cached_input_tokens
$repairFiles=@(Get-ChildItem "$PSScriptRoot/acceptance" -File|Where-Object Name -match '^R\d+\.md$')
$result=[ordered]@{
 status='bounded_local_functional_integration_passed'; systems=10; acceptanceEndUtc=$end; executionStartUtc=$baseline.startUtc;
 executionSeconds=$duration.TotalSeconds; requestToAcceptanceSeconds=$requestDuration.TotalSeconds;
 formalRepairRounds=$repairFiles.Count; developerSelfCheckEvents=7;
 humanBaselinePersonDays=30; assumedHoursPerDay=8; humanBaselineHours=$humanHours;
 elapsedAccelerationRatio=$humanHours/$duration.TotalHours;
 elapsedReductionPercent=(1-$duration.TotalHours/$humanHours)*100;
 tokenCapturedAt=$usage.capturedAt; tokens=$usage.usage; noncachedInputTokens=$noncached;
 noncachedInputPlusOutputTokens=$noncached+$usage.usage.output_tokens;
 agentObservedSpanHours=($usage.agents|Measure-Object observedSpanHours -Sum).Sum;
 agents=$usage.agents;
 limitations=@('Elapsed comparison uses a user-provided 30-person-day baseline and an assumed 8-hour workday; not measured equivalent manual effort.','Existing framework and template development costs are excluded.','Agent observed spans include waits and are not active compute time or human labor.','Strict browser/Unity visual equality and unsupported raw prefab exporter properties are not certified; original failed/incomplete reports retained.','No production authentication/payment/backend or all device/platform certification.','Token totals include cached input repeatedly read by model calls; cache is a subset, not extra tokens. Final response after capture is excluded.')
}
$result|ConvertTo-Json -Depth 10|Set-Content "$PSScriptRoot/benchmark-final.json" -Encoding utf8
$lines=@(
 '# 十系统集成测试结果与效率统计',
 '',
 '十个系统约定的离线本地功能集成用例通过；严格跨渲染器视觉全等不作通过声明。最终编译通过、Console 0、临时测试工具清理，2005 个产品文件经独立哈希核对。',
 '',
 "- 执行：2026-11-20 16:27:01 → $($end.ToOffset([timespan]::FromHours(8)).ToString('yyyy-MM-dd HH:mm:ss'))（北京时间）。",
 "- 执行耗时：$([math]::Floor($duration.TotalMinutes)) 分 $($duration.Seconds) 秒；从用户消息收到至验收：$([math]::Floor($requestDuration.TotalMinutes)) 分 $($requestDuration.Seconds) 秒。",
 "- 正式独立验收返修：$($repairFiles.Count) 轮（R1 身份校验/规范/引用补证，R2 公告锚点，R3 公告输入面）；开发自测与环境事件另记 7 项。",
 "- Token 总计：$('{0:N0}' -f $usage.usage.total_tokens)，其中输入 $('{0:N0}' -f $usage.usage.input_tokens)（缓存输入 $('{0:N0}' -f $usage.usage.cached_input_tokens)），输出 $('{0:N0}' -f $usage.usage.output_tokens)。",
 "- 非缓存输入＋输出：$('{0:N0}' -f ($noncached+$usage.usage.output_tokens)) Token；推理输出已包含在输出口径，不重复加总。",
 "- Token 采样截止：$($usage.capturedAt)；各 Agent 明细与原始日志基线见 benchmark-final.json / benchmark-baseline.json。",
 "- Agent 观测跨度合计：$('{0:N2}' -f $result.agentObservedSpanHours) 小时，含等待，不等于有效算力时间或人工投入。",
 '',
 '## 与人工基线对比',
 '',
 "按用户提供单人 30 天、假设 8 小时/天（240 人时），以本次执行自然耗时对比，交付周期加速约 $('{0:N1}' -f $result.elapsedAccelerationRatio) 倍，缩短约 $('{0:N2}' -f $result.elapsedReductionPercent)%。",
 '',
 '**这是已有模板与框架条件下、已测试本地集成范围的周期对比，不是从零开发的同范围人工实测，也不能直接声称节省了相同数量的人时。** 源模板制作、历史框架开发不计入本次耗时。',
 '',
 '## 结果与边界',
 '',
 '系统：Launch、Login、Loading、Master、RedDot、Notice、Home、Mail、Shop、Settings。公共组件与 Function 依赖不重复计数。主流程、真实射线点击、边界状态、生命周期、红点/标记清理，以及补测的设置可见控件、120封邮件回收、页面自动动画与重开均有证据。',
 '',
 '41个活动配置没有业务 ClassType，只验收入口/点击/开放条件，不代表41个活动业务实现。原生隐藏的质量/ShowFps控件不能冒充实际点击通过；质量偏好通过真实预设影响与保存验证。真实认证、支付、远端SDK、跨平台和逐曲线数学全等不在已完成结论内。',
 '',
 '浏览器 Home 源预演叠字与 Shop 静态空数据壳、原始 prefab 工具未覆盖属性及嵌套展开限制保留在独立报告，不伪造为视觉全通过。',
 '',
 '独立验收详见 [independent-final.md](acceptance/independent-final.md)，正式返修原件见 acceptance/R1.md、R2.md、R3.md；可复算数据见 benchmark-final.json。'
)
$lines|Set-Content "$PSScriptRoot/benchmark-report.md" -Encoding utf8
$result|ConvertTo-Json -Depth 10
