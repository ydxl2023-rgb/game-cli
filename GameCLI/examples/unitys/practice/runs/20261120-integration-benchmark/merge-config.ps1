$ErrorActionPreference='Stop'
$cli='G:/Works/GameCLI/GameCLI/clis/unity-cli/bin/Release/net8.0/unity-cli.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$evidence=Join-Path $PSScriptRoot 'config-merge.jsonl'
function CallExcel($arguments){$raw=& $cli excel @arguments --project $project --json; $raw|Add-Content $evidence; $r=$raw|ConvertFrom-Json; if(!$r.ok){throw $raw}; return $r}
foreach($module in @('Home','Shop','SvLogin','Mail','Settings','RedDotSystem','MasterSystem')){
 $config=Get-Content "$PSScriptRoot/configurations/$module/configurations.json" -Raw|ConvertFrom-Json
 foreach($table in $config.tables){
  $file="$PSScriptRoot/config-source/$($table.source.fileName)"
  $existing=(CallExcel @('query','--file',$file,'--header-row','2','--count','1000')).data.rows
  $rows=@{}; $numbers=@{}
  foreach($row in $existing){$data=@{};foreach($cell in $row.cells){$data[$cell.field]=[string]$cell.value};$rows[$data.ID]=$data;$numbers[$data.NumID]=$data.ID}
  foreach($entry in $table.entries){
   if($rows.ContainsKey([string]$entry.ID)){
    foreach($p in $entry.PSObject.Properties){if([string]$rows[[string]$entry.ID][$p.Name] -ne [string]$p.Value){throw "Conflict $module $($entry.ID) $($p.Name)"}}
    continue
   }
   if($numbers.ContainsKey([string]$entry.NumID)){throw "Numeric collision $module $($entry.ID)"}
   $json=$entry|ConvertTo-Json -Compress
   $args=@('insert','--file',$file,'--header-row','2','--values-json',$json)
   $null=CallExcel ($args+@('--dry-run'))
   $null=CallExcel $args
  }
 }
}
'Merged configuration entries successfully.'
