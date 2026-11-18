param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$evidence=@()
$contract=Get-Content "$PSScriptRoot/../config/home-entry-badges.json" -Raw|ConvertFrom-Json
$shop=($contract.entries|Where-Object name -eq shopEntry).id
function Call($type,$member,[object[]]$values=@()){
 $r=& $cli --project $Project --format json call --invoke-type $type --invoke-member $member --args-json (ConvertTo-Json -InputObject @($values) -Compress)|ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 8 -Compress)}
 return $r.data.returnSummary
}
function Check($name,$id,$count,$flags){
 Start-Sleep -Milliseconds 150
 $items=Call HomeEntryProbe Read|ConvertFrom-Json
 if(@($items).Count -ne 64 -or @($items|Where-Object bound -eq $false).Count){throw 'Unbound or missing entry'}
 $item=$items|Where-Object id -eq $id
 if(@($item.badges).Count -ne $count -or $item.requested -ne $flags){throw "$name count/flags mismatch: $($item|ConvertTo-Json -Depth 6 -Compress)"}
 if(@($item.badges|Where-Object {$_.raycast -or !$_.active -or $_.images -lt 1 -or ($_.name -like 'New*' -and !$_.animated)}).Count){throw "$name has invalid badge graphics"}
 $script:evidence+=@{name=$name;item=$item}
 Write-Output "$name passed ($count badges)"
}
Call HomeEntryProbe CheckPrefab
Check initialShop $shop 1 2
Call HomePresentationProbe SetBadges @($shop,3)|Out-Null
Check combined $shop 2 3
Call HomePresentationProbe SetBadges @($shop,3)|Out-Null
Check repeated $shop 2 3
Call HomeEntryProbe SetPresentation @($shop,$false,$true)|Out-Null
Check hidden $shop 0 3
Call HomeEntryProbe SetPresentation @($shop,$true,$true)|Out-Null
Check restored $shop 2 3
Call HomePresentationProbe SetBadges @($shop,0)|Out-Null
Check removed $shop 0 0
Call HomePresentationProbe SetBadges @($shop,1)|Out-Null
Check newOnly $shop 1 1
Call HomePresentationProbe SetBadges @($shop,2)|Out-Null
Check advertisementOnly $shop 1 2
# Exercise an entry without an authored NEW or advertisement mount.
Call HomePresentationProbe SetBadges @(0,3)|Out-Null
Check genericBoth 0 2 3
Call HomePresentationProbe SetBadges @(0,0)|Out-Null
Check genericRemoved 0 0 0
Call HomePresentationProbe Close|Out-Null
Start-Sleep -Milliseconds 250
Call HomePresentationProbe Open|Out-Null
Start-Sleep -Seconds 2
Check reopened $shop 1 2
$evidence|ConvertTo-Json -Depth 12|Set-Content "$PSScriptRoot/../.data/home-badges/runtime.json"
