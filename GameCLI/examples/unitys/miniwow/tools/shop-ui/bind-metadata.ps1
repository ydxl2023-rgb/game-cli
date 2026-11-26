$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../hospital-ui/bridge.ps1"
$result = Get-Content "$PSScriptRoot/bind-result.json" -Raw | ConvertFrom-Json
if (!$result.ok) { throw $result.error }
Invoke-HospitalBridge @('widget-metadata','UIShop','--vm','VMShop','--prefab','Assets/Data/Widgets/Shop/UIShop.prefab','--dry-run') | Out-Null
Invoke-HospitalBridge @('widget-metadata','UIShop','--vm','VMShop','--prefab','Assets/Data/Widgets/Shop/UIShop.prefab') | Out-Null
Set-Content "$PSScriptRoot/bind.request" install
