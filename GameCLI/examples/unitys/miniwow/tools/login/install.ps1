$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$prefab = 'Assets/Data/Widgets/Login/UILogin.prefab'
$records = [Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    $raw = & $cli --project $project --format json @arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{ arguments = $arguments; result = $result })
    $records | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/install-evidence.json" -Encoding utf8
    if (!$result.ok) { throw ($raw -join "`n") }
}
Bridge @('asset','refresh')
Bridge @('compile','--wait')
Bridge @('ugui','UILogin','--json',"$PSScriptRoot/login.json",'--manifest',"$PSScriptRoot/materials.json",'--prefab',$prefab,'--dry-run','--no-scene-replace')
Bridge @('ugui','UILogin','--json',"$PSScriptRoot/login.json",'--manifest',"$PSScriptRoot/materials.json",'--prefab',$prefab,'--no-scene-replace')
Bridge @('components','add','--asset',$prefab,'--path','UILogin','--component','UILogin')
Bridge @('widget-metadata','UILogin','--vm','VMLogin','--prefab',$prefab,'--dry-run')
Bridge @('widget-metadata','UILogin','--vm','VMLogin','--prefab',$prefab)
Set-Content "$PSScriptRoot/../town/setup.request" 'login'
