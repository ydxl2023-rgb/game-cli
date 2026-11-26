param([switch]$RestoreOnly)
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$project/Library/UnityCLI/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    $raw = & $cli --project $project --format json @arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{ arguments = $arguments; result = $result })
    $records | ConvertTo-Json -Depth 35 | Set-Content "$PSScriptRoot/auction-evidence$(if($RestoreOnly){'-restore'}).json"
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result
}
function Invoke([string]$method, [string]$arguments='[]') {
    return Bridge @('call','--invoke-type','MiniwowOpeningProbe','--invoke-member',$method,'--args-json',$arguments)
}
function Snapshot {
    $result = Invoke 'Snapshot'
    return $result.data.returnSummary | ConvertFrom-Json
}
function Require($condition, [string]$message) {
    if (!$condition) { throw "Opening verification failed: $message" }
    Write-Output "PASS: $message"
}
Bridge @('run','play') | Out-Null
Start-Sleep -Seconds 2
Bridge @('call','--invoke-type','MiniwowHomeProbe','--invoke-member','SetResolution','--args-json','[1080,1920]') | Out-Null
Bridge @('call','--invoke-type','UnityEngine.SceneManagement.SceneManager','--invoke-member','LoadScene','--args-json','["App"]') | Out-Null
Start-Sleep -Seconds 2
if (!$RestoreOnly) { Invoke 'ResetLocalSave' | Out-Null }
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','EnterGame') | Out-Null
Start-Sleep -Seconds 3
$state = Snapshot
if ($RestoreOnly) {
    Require ($state.phase -eq 'FetchSecondWater' -and $state.task -eq 60002 -and $state.first -eq 'Claimed') 'Task 60002 restored after full restart'
    Require (!$state.playing -and !$state.musicPlaying) 'Returning save does not replay movie or music'
    Require (!$state.damagedModel -and $state.restoredModel -and $state.merchantVisible) 'Restored auction and merchant survive full restart'
    Require ($state.merchantEntity -and $state.merchantAnimation -eq 'idle') 'Restored merchant retains entity and Spine idle presentation'
    Bridge @('screenshot','Library/OpeningQA') | Out-Null
    return
}
Require ($state.phase -eq 'Movie' -and $state.first -eq 'Active') 'Fresh session starts before rescue'
Invoke 'Click' '["SkipClick"]' | Out-Null
Invoke 'Click' '["ConfirmClick"]' | Out-Null
$state = Snapshot
Require ($state.phase -eq 'Introduction' -and $state.first -eq 'Active') 'Confirmed skip enters dialogue and leaves task active'
Require ($state.damagedModel -and !$state.restoredModel -and !$state.merchantVisible) 'Fresh rescue shows original damaged model and hides merchant'
Require ($state.merchantEntity) 'Hidden merchant has independent actor data'
Bridge @('screenshot','Library/OpeningQA') | Out-Null
Invoke 'Click' '["ContinueClick"]' | Out-Null
$state = Snapshot
Require ($state.phase -eq 'FetchWater' -and $state.water -eq 0) 'Priest dialogue opens actual water objective'
Invoke 'WalkToWater' | Out-Null
Start-Sleep -Seconds 2
$state = Snapshot
Require ($state.phase -eq 'CarryWater' -and $state.water -eq 10 -and $state.first -eq 'Active') 'Real actor movement collects ten water without completing task'
Require (!$state.wellAvailable -and $state.wellCollected) 'Well switches to collected presentation while carrying water'
Bridge @('screenshot','Library/OpeningQA') | Out-Null
Invoke 'Click' '["RescueClick"]' | Out-Null
Start-Sleep -Milliseconds 1400
Bridge @('screenshot','Library/OpeningQA') | Out-Null
Start-Sleep -Seconds 3
$state = Snapshot
Require ($state.phase -eq 'Thanks' -and $state.water -eq 0 -and $state.first -eq 'Completed') 'Navigation and rescue consume ten water and complete 60001'
Require (!$state.damagedModel -and $state.restoredModel -and $state.merchantVisible) 'Rescue restores auction and reveals merchant before task claim'
Require ($state.merchantEntity -and $state.merchantAnimation -eq 'idle') 'Merchant uses ActorEntity and original Spine idle'
Require ($state.wellAvailable -and !$state.wellCollected) 'Well recovers after rescue'
Bridge @('screenshot','Library/OpeningQA') | Out-Null
Bridge @('run','stop') | Out-Null
Start-Sleep -Seconds 2
Bridge @('run','play') | Out-Null
Start-Sleep -Seconds 2
Bridge @('call','--invoke-type','MiniwowHomeProbe','--invoke-member','SetResolution','--args-json','[1080,1920]') | Out-Null
Bridge @('call','--invoke-type','UnityEngine.SceneManagement.SceneManager','--invoke-member','LoadScene','--args-json','["App"]') | Out-Null
Start-Sleep -Seconds 2
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','EnterGame') | Out-Null
Start-Sleep -Seconds 3
$state = Snapshot
Require ($state.phase -eq 'Thanks' -and $state.first -eq 'Completed') 'Unclaimed completion survives full restart'
Require (!$state.damagedModel -and $state.restoredModel -and $state.merchantVisible) 'Repaired scene survives restart before claim'
Invoke 'Capture' | Out-Null
Invoke 'Click' '["ContinueClick"]' | Out-Null
$state = Snapshot
Require ($state.phase -eq 'Claim' -and $state.task -eq 60001) 'Completed task awaits explicit claim'
Invoke 'Click' '["ClaimClick"]' | Out-Null
$state = Snapshot
Require ($state.phase -eq 'FetchSecondWater' -and $state.task -eq 60002 -and $state.first -eq 'Claimed') 'Explicit claim selects 60002'
Bridge @('screenshot','Library/OpeningQA') | Out-Null
$console = Bridge @('console','--type','error','--count','100')
Require ($console.data.totalCount -eq 0) 'No Unity runtime errors'
Bridge @('run','stop') | Out-Null
