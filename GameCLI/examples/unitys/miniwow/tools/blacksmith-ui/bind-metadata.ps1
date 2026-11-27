$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../hospital-ui/bridge.ps1"
$result = Get-Content "$PSScriptRoot/bind-result.json" -Raw | ConvertFrom-Json
if (!$result.ok) { throw $result.error }
Invoke-HospitalBridge @('widget-metadata','UIBlacksmith','--vm','VMBlacksmith','--prefab','Assets/Data/Widgets/Blacksmith/UIBlacksmith.prefab','--dry-run') | Out-Null
Invoke-HospitalBridge @('widget-metadata','UIBlacksmith','--vm','VMBlacksmith','--prefab','Assets/Data/Widgets/Blacksmith/UIBlacksmith.prefab') | Out-Null
Set-Content "$PSScriptRoot/bind.request" install

