param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
# UnityCLI prefab component operations retain the original controller, without editing animation YAML.
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$asset='Assets/Data/Widgets/Home/Home.prefab'
$contract=Get-Content "$PSScriptRoot/../config/home-entry-badges.json" -Raw|ConvertFrom-Json
function Call([string[]]$Arguments){
 $r=& $cli --project $Project --format json @Arguments|ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 8 -Compress)}
 return $r.data
}
$inspection=Call @('prefab','inspect','--prefab',$asset,'--recursive')
foreach($mount in $contract.mounts|Where-Object kind -eq 1){
 $node=$inspection.nodes|Where-Object path -eq $mount.path
 if($node.components.type -contains 'UnityEngine.Animator'){
  Call @('components','remove','--asset',$asset,'--path',$mount.path,'--component','Animator')|Out-Null
 }
 # The original Animator writes absolute scale 1..1.3. Move that scale to the attached visual.
 Call @('modify','--asset',$asset,'--path',$mount.path,'--component','RectTransform','--property','m_LocalScale','--value','1,1,1')|Out-Null
 Write-Output "Moved pulse from mount: $($mount.path)"
}
$prefabs=@($contract.mounts|Where-Object kind -eq 1|ForEach-Object prefab)+@($contract.defaults.'1')|Sort-Object -Unique
foreach($prefab in $prefabs){
 $root=[IO.Path]::GetFileNameWithoutExtension($prefab)
 $info=Call @('prefab','inspect','--prefab',$prefab,'--recursive')
 if(!(($info.nodes|Where-Object path -eq $root).components.type -contains 'UnityEngine.Animator')){
  Call @('components','add','--asset',$prefab,'--path',$root,'--component','Animator')|Out-Null
 }
 Call @('modify','--asset',$prefab,'--path',$root,'--component','Animator','--property','m_Controller','--value','Assets/Data/Widgets/Common/HomeNewTag.controller')|Out-Null
 Write-Output "Bound standalone pulse: $prefab"
}
