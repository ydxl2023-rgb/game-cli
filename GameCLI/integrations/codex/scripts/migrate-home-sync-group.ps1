param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$asset='Assets/Data/Widgets/Home/Home.prefab'
$items=Get-Content "$PSScriptRoot/../config/home-functions.json" -Raw | ConvertFrom-Json
$design=(Get-Content "$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate/template.json" -Raw | ConvertFrom-Json).design
$paths=@{}
foreach($node in $design.nodes){$paths[$node.id]=if($node.parent){$paths[$node.parent]+'/'+$node.name}else{$node.name}}
function Invoke-Unity([string[]]$Arguments){
 $r=& $cli --project $Project --format json @Arguments | ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 8 -Compress)}
 return $r.data
}
function Set-Field($path,$type,$field,$value){Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value))|Out-Null}
$root=$paths['Home0'];$list=$paths['Home0_6_2_0']
Set-Field $root UIHome 'activities.Array.size' $items.Count
for($i=0;$i -lt $items.Count;$i++){
 $item=$items[$i];$path=$paths[$item.node]
 Set-Field $path UIActivityEntry id $item.id
 Set-Field $root UIHome "activities.Array.data[$i]" "@$path#UIActivityEntry"
 Set-Field $path Metadata 'metadatas.Array.size' 2
 Set-Field $path Metadata 'metadatas.Array.data[1].name' visible
 Set-Field $path Metadata 'metadatas.Array.data[1].type' SyncVisible
 Set-Field $path Metadata 'metadatas.Array.data[1].targets.Array.size' 1
 Set-Field $path Metadata 'metadatas.Array.data[1].components.Array.size' 1
 Set-Field $path Metadata 'metadatas.Array.data[1].targets.Array.data[0]' "@$path"
 Set-Field $path Metadata 'metadatas.Array.data[1].components.Array.data[0]' "@$path#RectTransform"
 Write-Output "Bound SyncGroup item $i / $($items.Count): $($item.name)"
}
Set-Field $root Metadata 'metadatas.Array.data[0].type' SyncGroup
Set-Field $root Metadata 'metadatas.Array.data[0].components.Array.data[0]' "@$list#RectTransform"
Invoke-Unity @('components','remove','--asset',$asset,'--path',$list,'--component','HomeActivityList') | ConvertTo-Json -Depth 4
Invoke-Unity @('widget-metadata','UIHome','--vm','VMHome','--prefab',$asset,'--dry-run') | ConvertTo-Json -Depth 4
