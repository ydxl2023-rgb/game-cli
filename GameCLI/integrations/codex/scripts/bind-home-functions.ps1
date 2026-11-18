param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client',[switch]$ValidateOnly)
# Rebind an already migrated prefab using explicit authored paths. Never rebuild its artwork.
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$asset='Assets/Data/Widgets/Home/Home.prefab'
$contract=Get-Content "$PSScriptRoot/../config/home-entry-badges.json" -Raw | ConvertFrom-Json
function Invoke-Unity([string[]]$Arguments){
 $r=& $cli --project $Project --format json @Arguments | ConvertFrom-Json
 if(!$r.ok){throw ($r|ConvertTo-Json -Depth 8 -Compress)}
 return $r.data
}
function Set-Field($path,$type,$field,$value){
 Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value))|Out-Null
}
function Bind($path,$index,$name,$type,$targets){
 $prefix="metadatas.Array.data[$index]"
 Set-Field $path Metadata "$prefix.name" $name
 Set-Field $path Metadata "$prefix.type" $type
 Set-Field $path Metadata "$prefix.targets.Array.size" $targets.Count
 Set-Field $path Metadata "$prefix.components.Array.size" $targets.Count
 for($j=0;$j -lt $targets.Count;$j++){
  Set-Field $path Metadata "$prefix.targets.Array.data[$j]" "@$($targets[$j][0])"
  Set-Field $path Metadata "$prefix.components.Array.data[$j]" "@$($targets[$j][0])#$($targets[$j][1])"
 }
}
if(!$ValidateOnly){
 $guid=(Get-Content "$Project/Assets/Scripts/Systems/Home/VM/VMActivityEntry.cs.meta"|Select-String '^guid:').ToString().Substring(6)
 foreach($entry in $contract.entries){
  $path=$entry.path
  Set-Field $path UIActivityEntry id $entry.id
  Set-Field $path Metadata className VMActivityEntry
  Set-Field $path Metadata classGuid $guid
  Set-Field $path Metadata 'metadatas.Array.size' 4
  Bind $path 0 openClick SyncClick $entry.buttons
  Bind $path 1 visible SyncVisible @(,@($path,'RectTransform'))
  Bind $path 2 interactable SyncInteractable $entry.buttons
  Bind $path 3 badges SyncActivityBadges @(,@($path,'UIActivityEntry'))
  Set-Field $path UIActivityEntry 'badgeSlots.Array.size' 2
  for($j=0;$j -lt 2;$j++){
   $kind=$j+1
   $mount=$contract.mounts|Where-Object { $_.entryId -eq $entry.id -and $_.kind -eq $kind }|Select-Object -First 1
   $anchor=if($mount){$mount.path}else{$path}
   $prefab=if($mount){$mount.prefab}else{$contract.defaults."$kind"}
   Set-Field $path UIActivityEntry "badgeSlots.Array.data[$j].kind" $kind
   Set-Field $path UIActivityEntry "badgeSlots.Array.data[$j].anchor" "@$anchor#RectTransform"
   Set-Field $path UIActivityEntry "badgeSlots.Array.data[$j].prefab" $prefab
  }
  if(!$entry.existing){$owner=if($entry.bindingOwner){$entry.bindingOwner}else{'Home'}; Bind $owner $entry.rootIndex $entry.name SyncMetadata @(,@($path,'Metadata'))}
  Write-Output "Bound entry $($entry.id): $path"
 }
}
Invoke-Unity @('widget-metadata','UIHome','--vm','VMHome','--prefab',$asset,'--validate-existing')|ConvertTo-Json -Depth 6
Invoke-Unity @('call','--invoke-type','HomeEntryProbe','--invoke-member','CheckPrefab')|ConvertTo-Json -Depth 6
