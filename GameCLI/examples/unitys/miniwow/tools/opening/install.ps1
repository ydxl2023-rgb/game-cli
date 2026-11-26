$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$prefab = 'Assets/Data/Widgets/Home/UIHomeOpening.prefab'
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
Bridge @('ugui','UIHomeOpening','--json',"$PSScriptRoot/ui.json",'--manifest',"$PSScriptRoot/materials.json",'--prefab',$prefab,'--dry-run','--no-scene-replace')
Bridge @('ugui','UIHomeOpening','--json',"$PSScriptRoot/ui.json",'--manifest',"$PSScriptRoot/materials.json",'--prefab',$prefab,'--no-scene-replace')
Bridge @('components','add','--asset',$prefab,'--path','UIHomeOpening','--component','UIHomeOpening')
Bridge @('widget-metadata','UIHomeOpening','--vm','VMHomeOpening','--prefab',$prefab,'--dry-run')
Bridge @('widget-metadata','UIHomeOpening','--vm','VMHomeOpening','--prefab',$prefab)
Set-Content "$PSScriptRoot/install.request" 'install'

