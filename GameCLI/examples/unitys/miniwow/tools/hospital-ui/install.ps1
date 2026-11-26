param([switch]$PageOnly)
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$project/Library/UnityCLI/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    $raw = & $cli --project $project --format json @arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{ arguments = $arguments; result = $result })
    $records | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/install-evidence.json" -Encoding utf8
    if (!$result.ok) { throw ($raw -join "`n") }
}
$names = if ($PageOnly) { @('UIHomeHospital') } else { @('UIHomeHospitalEntry','UIHomeHospital') }
foreach ($name in $names) {
    $prefab = "Assets/Data/Widgets/Home/$name.prefab"
    if ($name -eq "UIHomeHospital") { $prefab = "Assets/Data/Widgets/Hospital/$name.prefab" }
    Bridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--dry-run','--no-scene-replace')
    Bridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--no-scene-replace')
    Bridge @('components','add','--asset',$prefab,'--path',$name,'--component',$name)
    $vm = $name.Replace('UI','VM')
    Bridge @('widget-metadata',$name,'--vm',$vm,'--prefab',$prefab,'--dry-run')
    Bridge @('widget-metadata',$name,'--vm',$vm,'--prefab',$prefab)
}

Set-Content "$PSScriptRoot/bind.request" $(if ($PageOnly) { "page" } else { "all" })
