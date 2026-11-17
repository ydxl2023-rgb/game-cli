param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$evidence=@()
function Call($member,[object[]]$values=@()){
 $r=& $cli --project $Project --format json call --invoke-type HomeFunctionProbe --invoke-member $member --args-json (ConvertTo-Json -InputObject @($values) -Compress) | ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 6 -Compress)}
 return $r.data
}
function Snapshot($name,$count,$level=-1,$shop=$null){
 Start-Sleep -Milliseconds 350
 $data=Call Read
 $state=$data.returnSummary|ConvertFrom-Json
 if($state.items.Count -ne $count){throw "$name count expected $count got $($state.items.Count)"}
 if($state.activeTemplates -ne 0){throw 'Source templates are visible'}
 if($level -ge 0 -and ($state.level -ne $level -or [int]$state.displayLevel -ne $level)){throw "$name level mismatch"}
 if($null -ne $shop -and $state.shopOpen -ne $shop){throw "$name Shop mismatch"}
 $script:evidence+=@{name=$name;state=$state}
 Write-Output "$name passed: $count items, level $($state.level), shop $($state.shopOpen)"
}
Snapshot baseline 9
Call Switch @(12000,$false)|Out-Null
Snapshot disabled 8
Call Switch @(12002,$true)|Out-Null
Snapshot added 9
Call RequireLevel @(12002,5)|Out-Null
Call Level @(0)|Out-Null
Snapshot levelLocked 8 0
Call Level @(5)|Out-Null
Snapshot levelUnlocked 9 5
Call Level @(6)|Out-Null
Snapshot levelOnlyChanged 9 6
Call Schedule @(12002,3,7)|Out-Null
Call Switch @(12002,$true)|Out-Null
Snapshot beforeStart 8
Start-Sleep -Seconds 4
Snapshot windowOpen 9
Start-Sleep -Seconds 4
Snapshot windowClosed 8
Call Switch @(10010,$false)|Out-Null
Call OpenShop|Out-Null
Start-Sleep -Seconds 1
Snapshot blockedShop 8 -1 $false
Call Switch @(10010,$true)|Out-Null
Start-Sleep -Milliseconds 350
Call OpenShop|Out-Null
Start-Sleep -Seconds 2
Snapshot openShop 8 -1 $true
Call Switch @(10010,$false)|Out-Null
Start-Sleep -Seconds 1
Snapshot closedShop 8 -1 $false
$evidence | ConvertTo-Json -Depth 12 | Set-Content "$PSScriptRoot/../.data/home-functions-export/runtime.json"
