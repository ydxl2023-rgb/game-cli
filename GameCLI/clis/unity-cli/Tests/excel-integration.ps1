param([string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference='Stop'
$cli=Join-Path $PSScriptRoot '../bin/Debug/net8.0/unity-cli.dll'
$file=Join-Path $Project ('Library/ExcelPluginTest-'+[guid]::NewGuid().ToString('N')+'.xlsx')
function Run($items) {
 $result=& dotnet $cli excel @items --project $Project --json | ConvertFrom-Json
 if($LASTEXITCODE -ne 0 -or !$result.ok){throw ($result|ConvertTo-Json -Depth 8)}
 return $result
}
try {
 Run @('create','--file',$file,'--headers-json','["Id","Name"]','--dry-run') | Out-Null
 if(Test-Path $file){throw 'Dry-run wrote file'}
 Run @('create','--file',$file,'--headers-json','["Id","Name"]') | Out-Null
 Run @('schema','--file',$file,'--header-row','1') | Out-Null
 Run @('insert','--file',$file,'--header-row','1','--values-json','{"Id":1,"Name":"Before"}') | Out-Null
 Run @('set','--file',$file,'--header-row','1','--row','2','--field','Name','--value','After') | Out-Null
 $cell=Run @('cell','--file',$file,'--header-row','1','--row','2','--field','Name')
 if($cell.data.cell.value -ne 'After'){throw 'Cell round-trip failed'}
 $query=Run @('query','--file',$file,'--header-row','1','--where','Name=After')
 if($query.data.totalMatched -ne 1){throw 'Query failed'}
 Run @('delete','--file',$file,'--header-row','1','--row','2') | Out-Null
 Run @('delete-file','--file',$file,'--dry-run') | Out-Null
 if(!(Test-Path $file)){throw 'Delete dry-run removed file'}
 Run @('delete-file','--file',$file) | Out-Null
 if(Test-Path $file){throw 'Delete failed'}
 'PASS: workbook create/schema/insert/set/cell/query/delete/delete-file'
} finally { if(Test-Path $file){Run @('delete-file','--file',$file)|Out-Null} }
