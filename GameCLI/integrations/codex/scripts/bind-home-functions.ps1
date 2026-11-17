param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client',[int]$Start=0)
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$folder="$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
$asset='Assets/Data/Widgets/Home/Home.prefab'
$items=Get-Content "$PSScriptRoot/../config/home-functions.json" -Raw | ConvertFrom-Json
$contract=Get-Content "$folder/presentation.json" -Raw | ConvertFrom-Json
$design=(Get-Content "$folder/template.json" -Raw | ConvertFrom-Json).design
$paths=@{}
foreach($node in $design.nodes){$paths[$node.id]=if($node.parent){$paths[$node.parent]+'/'+$node.name}else{$node.name}}
function Invoke-Unity([string[]]$Arguments){
 $r=& $cli --project $Project --format json @Arguments | ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 8 -Compress)}
 return $r.data
}
function Set-Field($path,$type,$field,$value){Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value))|Out-Null}
$inspection=Invoke-Unity @('prefab','inspect','--prefab',$asset,'--recursive')
function Add($path,$type){
 if(!(($inspection.nodes|Where-Object path -eq $path).components.type -contains $type)){
  Invoke-Unity @('components','add','--asset',$asset,'--path',$path,'--component',$type)|Out-Null
 }
}
function RedDots($path,$entries){
 Add $path 'RedDotScope'
 Set-Field $path RedDotScope 'bindings.Array.size' $entries.Count
 for($j=0;$j -lt $entries.Count;$j++){
  Set-Field $path RedDotScope "bindings.Array.data[$j].id" $entries[$j].redDotId
  Set-Field $path RedDotScope "bindings.Array.data[$j].target" "@$($paths[$entries[$j].node])"
 }
}
$root=$paths['Home0'];$list=$paths['Home0_6_2_0'];$keys=@($contract.states|Sort-Object value|ForEach-Object key)
Set-Field $root UIHome 'activities.Array.size' $items.Count
$vmGuid=(Get-Content "$Project/Assets/Scripts/Systems/Home/VM/VMActivityEntry.cs.meta"|Select-String '^guid:').ToString().Substring(6)
for($i=$Start;$i -lt $items.Count;$i++){
 $item=$items[$i];$path=$paths[$item.node]
 Add $path 'UIActivityEntry';Add $path 'Oathx.Widget.Metadata'
 Set-Field $path Metadata className VMActivityEntry
 Set-Field $path Metadata classGuid $vmGuid
 Set-Field $path Metadata 'metadatas.Array.size' 2
 Set-Field $path Metadata 'metadatas.Array.data[0].name' openClick
 Set-Field $path Metadata 'metadatas.Array.data[0].type' SyncClick
 Set-Field $path Metadata 'metadatas.Array.data[0].targets.Array.size' $item.buttons.Count
 Set-Field $path Metadata 'metadatas.Array.data[0].components.Array.size' $item.buttons.Count
 for($j=0;$j -lt $item.buttons.Count;$j++){
  $button=$paths[$item.buttons[$j].node]
  Set-Field $path Metadata "metadatas.Array.data[0].targets.Array.data[$j]" "@$button"
  Set-Field $path Metadata "metadatas.Array.data[0].components.Array.data[$j]" "@$button#Button"
 }
 Set-Field $path UIActivityEntry 'indicators.Array.size' $item.states.Count
 for($j=0;$j -lt $item.states.Count;$j++){
  Set-Field $path UIActivityEntry "indicators.Array.data[$j].indicator" ([array]::IndexOf($keys,$item.states[$j].key))
  Set-Field $path UIActivityEntry "indicators.Array.data[$j].target" "@$($paths[$item.states[$j].node])"
 }
 RedDots $path @($item.redDots)
 Set-Field $path UIActivityEntry id $item.id
 Set-Field $root UIHome "activities.Array.data[$i]" "@$path#UIActivityEntry"
 Set-Field $path Metadata 'metadatas.Array.data[1].name' visible
 Set-Field $path Metadata 'metadatas.Array.data[1].type' SyncVisible
 Set-Field $path Metadata 'metadatas.Array.data[1].targets.Array.size' 1
 Set-Field $path Metadata 'metadatas.Array.data[1].components.Array.size' 1
 Set-Field $path Metadata 'metadatas.Array.data[1].targets.Array.data[0]' "@$path"
 Set-Field $path Metadata 'metadatas.Array.data[1].components.Array.data[0]' "@$path#RectTransform"
 Write-Output "Activity template $i / $($items.Count): $($item.name)"
}
$remaining=@($contract.states|Where-Object {$_.sourcePath -notlike '0/6/2/0/*'})
Set-Field $root UIHome 'indicators.Array.size' $remaining.Count
for($i=0;$i -lt $remaining.Count;$i++){
 Set-Field $root UIHome "indicators.Array.data[$i].indicator" ([array]::IndexOf($keys,$remaining[$i].key))
 Set-Field $root UIHome "indicators.Array.data[$i].target" "@$($paths[$remaining[$i].node])"
}
RedDots $root @($contract.redDots|Where-Object {$_.sourcePath -notlike '0/6/2/0/*'})
Invoke-Unity @('widget-metadata','UIHome','--vm','VMHome','--prefab',$asset,'--dry-run')|ConvertTo-Json -Depth 4
Invoke-Unity @('widget-metadata','UIHome','--vm','VMHome','--prefab',$asset)|ConvertTo-Json -Depth 4
