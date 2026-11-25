$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath("$PSScriptRoot/../..")
$inputDirectory = Join-Path $root 'client/Library/SceneConfigExport/input'
$outputDirectory = Join-Path $root 'client/Library/SceneConfigExport/output'
$workspace = [IO.Path]::GetFullPath((Join-Path $root 'client/Library/SceneConfigExport'))
foreach ($path in @($inputDirectory, $outputDirectory)) {
    if (![IO.Path]::GetFullPath($path).StartsWith($workspace + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Exporter output escaped the temporary workspace.'
    }
    New-Item -ItemType Directory -Force -Path $path | Out-Null
}
Copy-Item -LiteralPath "$root/config/propScene.xlsx" -Destination "$inputDirectory/propScene.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propActor.xlsx" -Destination "$inputDirectory/propActor.xlsx" -Force
$exporter = [IO.Path]::GetFullPath("$root/../development/config/bin/Debug/net8.0/e2cs.exe")
& $exporter -i $inputDirectory -o $outputDirectory -f Config
if ($LASTEXITCODE -ne 0) { throw 'Scene export failed.' }
$table = Get-Content -Raw "$outputDirectory/json/SceneConfig.bytes" | ConvertFrom-Json
if (!$table.data.'10001'.Path -or !$table.data.'10002'.Path) { throw 'Required scene entries are missing.' }
foreach ($file in @('client/SceneConfig.cs', 'client/ActorConfig.cs', 'common/Excel.cs')) {
    $target = "$root/client/Assets/Scripts/Config/$file"
    New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
    Copy-Item -LiteralPath "$outputDirectory/csharp/$file" -Destination $target -Force
}
$destination = "$root/client/Assets/Data/Jsons/Excels"
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Copy-Item -LiteralPath "$outputDirectory/json/SceneConfig.bytes" -Destination "$destination/SceneConfig.bytes" -Force
Write-Output 'Exported propScene.xlsx through e2cs.'

Copy-Item -LiteralPath "$outputDirectory/json/ActorConfig.bytes" -Destination "$destination/ActorConfig.bytes" -Force
