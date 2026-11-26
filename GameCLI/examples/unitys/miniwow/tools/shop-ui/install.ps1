$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../hospital-ui/bridge.ps1"
foreach ($name in @('UIShopOffer','UIShopPurchase','UIShop')) {
    $prefab = "Assets/Data/Widgets/Shop/$name.prefab"
    if ($name -eq 'UIShopOffer') { $prefab = "Assets/Data/Widgets/Common/Micros/$name.prefab" }
    Invoke-HospitalBridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--dry-run','--no-scene-replace') | Out-Null
    Invoke-HospitalBridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--no-scene-replace') | Out-Null
    Invoke-HospitalBridge @('components','add','--asset',$prefab,'--path',$name,'--component',$name) | Out-Null
    if ($name -ne 'UIShop') {
        Invoke-HospitalBridge @('widget-metadata',$name,'--vm',$name.Replace('UI','VM'),'--prefab',$prefab,'--dry-run') | Out-Null
        Invoke-HospitalBridge @('widget-metadata',$name,'--vm',$name.Replace('UI','VM'),'--prefab',$prefab) | Out-Null
    }
}
Set-Content "$PSScriptRoot/bind.request" install
