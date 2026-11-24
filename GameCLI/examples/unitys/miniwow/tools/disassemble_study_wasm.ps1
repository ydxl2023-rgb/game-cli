$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot '../research/code-study/packages'
$expected=Get-Content "$root/tool/expected-sha256.json" | ConvertFrom-Json
$actual=(Get-FileHash "$root/tool/wabt.tar.gz" -Algorithm SHA256).Hash.ToLower()
if($expected.digest -ne "sha256:$actual"){throw 'WABT archive hash mismatch'}
$exe=Get-ChildItem "$root/tool" -Recurse -Filter wasm2wat.exe | Select-Object -First 1
$bin=$exe.DirectoryName
$version=& $exe.FullName --version
if($version -ne '1.0.42'){throw 'Wrong WABT version'}
$records=@()
foreach($moduleFile in Get-ChildItem "$root/decoded" -Recurse -Filter *.wasm){
    & "$bin/wasm-validate.exe" $moduleFile.FullName
    if($LASTEXITCODE -ne 0){throw 'WASM validation failed'}
    & $exe.FullName $moduleFile.FullName -o "$($moduleFile.FullName).wat"
    if($LASTEXITCODE -ne 0){throw 'WAT decode failed'}
    & "$bin/wasm-objdump.exe" -h $moduleFile.FullName | Set-Content "$($moduleFile.FullName).sections.txt"
    $wat=Get-Item "$($moduleFile.FullName).wat"
    $records+=@{input=$moduleFile.FullName;wat=$wat.FullName;wat_bytes=$wat.Length;wat_sha256=(Get-FileHash $wat.FullName).Hash;validated=$true}
}
@{provider='WABT CLI (not REA)';version=$version;archive_sha256=$actual;wasm2wat_sha256=(Get-FileHash $exe.FullName).Hash;modules=$records} | ConvertTo-Json -Depth 8 | Set-Content "$root/wat-manifest.json"

