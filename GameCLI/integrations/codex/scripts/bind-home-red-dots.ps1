param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client',[switch]$SkipHome,[int]$Start=0)
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$folder="$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
$asset='Assets/Data/Widgets/Home/Home.prefab'
$contract=Get-Content "$folder/presentation.json" -Raw | ConvertFrom-Json
$design=(Get-Content "$folder/template.json" -Raw | ConvertFrom-Json).design
$paths=@{}
foreach($node in $design.nodes){$paths[$node.id]=if($node.parent){$paths[$node.parent]+'/'+$node.name}else{$node.name}}
function Invoke-Unity([string[]]$Arguments){
 $r=& $cli --project $Project --format json @Arguments | ConvertFrom-Json
 if(!$r.ok){throw ($r | ConvertTo-Json -Depth 5)}
 return $r.data
}
function Set-Field($path,$type,$field,$value){
 Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value)) | Out-Null
}
$root=$paths['Home0']
$inspection=Invoke-Unity @('prefab','inspect','--prefab',$asset,'--recursive')
if(!(($inspection.nodes | Where-Object path -eq $root).components.type -contains 'RedDotScope')){
 Invoke-Unity @('components','add','--asset',$asset,'--path',$root,'--component','RedDotScope') | Out-Null
}
if (!$SkipHome) {
Set-Field $root UIHome 'indicators.Array.size' $contract.states.Count
$keys=@($contract.states | Sort-Object value | ForEach-Object key)
for($i=0;$i -lt $contract.states.Count;$i++){
 $state=$contract.states[$i]
 Set-Field $root UIHome "indicators.Array.data[$i].indicator" ([array]::IndexOf($keys,$state.key))
 Set-Field $root UIHome "indicators.Array.data[$i].target" "@$($paths[$state.node])"
 if($i%20 -eq 0){Write-Output "Home indicators: $i/$($contract.states.Count)"}
}
}
Set-Field $root RedDotScope 'bindings.Array.size' $contract.redDots.Count
for($i=$Start;$i -lt $contract.redDots.Count;$i++){
 $state=$contract.redDots[$i]
 Set-Field $root RedDotScope "bindings.Array.data[$i].id" $state.redDotId
 Set-Field $root RedDotScope "bindings.Array.data[$i].target" "@$($paths[$state.node])"
 if($i%10 -eq 0){Write-Output "Red dot bindings: $i/$($contract.redDots.Count)"}
}
Write-Output 'Home red dot ownership migrated.'
