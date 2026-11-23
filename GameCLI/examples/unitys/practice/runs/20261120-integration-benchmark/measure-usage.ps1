param([string]$SessionRoot = 'C:/Users/Administrator/.codex/sessions')
$ErrorActionPreference = 'Stop'
$baseline = Get-Content "$PSScriptRoot/benchmark-baseline.json" -Raw | ConvertFrom-Json
$rootId = '01a1bfec-b177-70b3-b7b4-068df4067d0a'
$requestTime = [datetimeoffset]$baseline.requestTimestamp
$files = @(Get-Item $baseline.parentRollout) + @(Get-ChildItem "$SessionRoot/2026/11/20" -File -Filter '*.jsonl')
$known = @{$rootId = $true}
$rows = @()
foreach ($file in ($files | Sort-Object Name)) {
    $first = Get-Content $file.FullName -TotalCount 1 | ConvertFrom-Json
    $id = $first.payload.id
    $spawn = $first.payload.source.subagent.thread_spawn
    if ($id -ne $rootId -and (!$spawn -or !$known.ContainsKey($spawn.parent_thread_id))) { continue }
    $known[$id] = $true
    $last = $null
    foreach ($line in (Get-Content $file.FullName)) {
        if (!$line.Contains('"token_count"')) { continue }
        try { $event = $line | ConvertFrom-Json } catch { continue }
        if ($event.type -eq 'event_msg' -and $event.payload.type -eq 'token_count' -and [datetimeoffset]$event.timestamp -ge $requestTime) { $last = $event }
    }
    if (!$last) { continue }
    $agentStart = if ($id -eq $rootId) { [datetimeoffset]$baseline.startUtc } else { [datetimeoffset]$first.timestamp }
    $values = [ordered]@{threadId=$id;agentPath=$(if ($id -eq $rootId) {'/root'} else {$spawn.agent_path});startedAt=$agentStart;observedAt=$last.timestamp;observedSpanHours=([datetimeoffset]$last.timestamp-$agentStart).TotalHours}
    foreach ($key in @('input_tokens','cached_input_tokens','cache_write_input_tokens','output_tokens','reasoning_output_tokens','total_tokens')) {
        $subtract = if ($id -eq $rootId) { [long]$baseline.baselineTokenEvent.payload.info.total_token_usage.$key } else { 0 }
        $values[$key] = [long]$last.payload.info.total_token_usage.$key - $subtract
    }
    $rows += [pscustomobject]$values
}
$totals = [ordered]@{}
foreach ($key in @('input_tokens','cached_input_tokens','output_tokens','reasoning_output_tokens','total_tokens')) { $totals[$key] = ($rows | Measure-Object $key -Sum).Sum }
$report = [ordered]@{capturedAt=[datetimeoffset]::UtcNow;requestTime=$requestTime;executionStart=$baseline.startUtc;elapsedHours=([datetimeoffset]::UtcNow-[datetimeoffset]$baseline.startUtc).TotalHours;status='in_progress';usage=$totals;agents=$rows;notes='Cumulative usage deltas. Cached input is a subset of input; reasoning is reported separately, not added again. Final reporting tokens after capture are excluded. No efficiency verdict until acceptance completes.'}
$report | ConvertTo-Json -Depth 8 | Set-Content "$PSScriptRoot/usage-latest.json" -Encoding utf8
$report | ConvertTo-Json -Depth 8
