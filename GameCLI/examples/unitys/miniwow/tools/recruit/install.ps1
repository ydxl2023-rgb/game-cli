$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../hospital-ui/bridge.ps1"
$hospitalEvidence = "$PSScriptRoot/install-evidence.jsonl"
foreach ($pair in @(@('UIRecruit','Recruit'),@('UIHomeRecruitEntry','Home'),@('UIBattleEntry','BattleEntry'))) {
    $name = $pair[0]
    $prefab = "Assets/Data/Widgets/$($pair[1])/$name.prefab"
    Invoke-HospitalBridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--dry-run','--no-scene-replace') | Out-Null
    Invoke-HospitalBridge @('ugui',$name,'--json',"$PSScriptRoot/$name.json",'--manifest',"$PSScriptRoot/$name.materials.json",'--prefab',$prefab,'--no-scene-replace') | Out-Null
    Invoke-HospitalBridge @('components','add','--asset',$prefab,'--path',$name,'--component',$name) | Out-Null
    $vm = $name.Replace('UI','VM')
    Invoke-HospitalBridge @('widget-metadata',$name,'--vm',$vm,'--prefab',$prefab,'--dry-run') | Out-Null
    Invoke-HospitalBridge @('widget-metadata',$name,'--vm',$vm,'--prefab',$prefab) | Out-Null
    Write-Output "Installed $name"
}
Set-Content "$hospitalProject/Library/OpeningQA/recruit-setup.request" widgets
