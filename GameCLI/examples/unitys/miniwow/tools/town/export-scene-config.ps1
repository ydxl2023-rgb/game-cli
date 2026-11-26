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
Copy-Item -LiteralPath "$root/config/propFunctionSystem.xlsx" -Destination "$inputDirectory/propFunctionSystem.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propCommonText.xlsx" -Destination "$inputDirectory/propCommonText.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propTask.xlsx" -Destination "$inputDirectory/propTask.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propHomeBuilding.xlsx" -Destination "$inputDirectory/propHomeBuilding.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propShop.xlsx" -Destination "$inputDirectory/propShop.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propVillage.xlsx" -Destination "$inputDirectory/propVillage.xlsx" -Force
Copy-Item -LiteralPath "$root/config/propOpening.xlsx" -Destination "$inputDirectory/propOpening.xlsx" -Force
$exporter = [IO.Path]::GetFullPath("$root/config/bin/Debug/net8.0/e2cs.exe")
& dotnet build "$root/tools/e2cs/e2cs.csproj" --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Failed to build the project-local e2cs exporter.' }
& $exporter -i $inputDirectory -o $outputDirectory -f Config
if ($LASTEXITCODE -ne 0) { throw 'Scene export failed.' }
& python -X utf8 "$root/tools/task-config/verify.py" --output $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Task configuration verification failed; generated files were not installed.' }
& dotnet run --project "$root/tools/task-config/ExpressionValidation/ExpressionValidation.csproj" -- $root $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Task expression validation failed; generated files were not installed.' }
& python -X utf8 "$root/tools/hospital-ui/verify.py" --output $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Hospital configuration verification failed.' }
& python -X utf8 "$root/tools/shop-ui/verify.py" --output $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Trading configuration verification failed.' }
$table = Get-Content -Raw "$outputDirectory/json/SceneConfig.bytes" | ConvertFrom-Json
if (!$table.data.'10001'.Path -or !$table.data.'10002'.Path) { throw 'Required scene entries are missing.' }
foreach ($file in @('client/SceneConfig.cs', 'client/ActorConfig.cs', 'client/FunctionSystemConfig.cs', 'client/CommonTextConfig.cs', 'client/TaskConfig.cs', 'client/TaskChapterConfig.cs', 'client/TaskUnlockConfig.cs', 'client/TaskGuideConfig.cs', 'client/TaskSettingConfig.cs', 'client/HomeHospitalSettingConfig.cs', 'client/HomeBuildingConfig.cs', 'client/HomeBuildingLevelConfig.cs', 'client/HomeBuildingCostConfig.cs', 'client/HomeBuildingConditionConfig.cs', 'client/HomeBuildingGoodsConfig.cs', 'client/HomeBuildingItemConfig.cs', 'client/VillageSettingConfig.cs', 'client/VillageTaskConfig.cs', 'client/HomeOpeningConfig.cs', 'client/OpeningDialogueConfig.cs', 'client/OpeningRescueConfig.cs', 'client/ExcelEnumDefine.cs', 'language/LanguageConfig.cs', 'common/Excel.cs')) {
    $target = "$root/client/Assets/Scripts/Config/$file"
    New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
    Copy-Item -LiteralPath "$outputDirectory/csharp/$file" -Destination $target -Force
}
$destination = "$root/client/Assets/Data/Jsons/Excels"
foreach ($name in @('ShopGoods', 'ShopSetting')) {
    Copy-Item -LiteralPath "$outputDirectory/csharp/client/${name}Config.cs" -Destination "$root/client/Assets/Scripts/Config/client/${name}Config.cs" -Force
    Copy-Item -LiteralPath "$outputDirectory/json/${name}Config.bytes" -Destination "$destination/${name}Config.bytes" -Force
}
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Copy-Item -LiteralPath "$outputDirectory/json/SceneConfig.bytes" -Destination "$destination/SceneConfig.bytes" -Force
Write-Output 'Exported propScene.xlsx through e2cs.'

Copy-Item -LiteralPath "$outputDirectory/json/ActorConfig.bytes" -Destination "$destination/ActorConfig.bytes" -Force
Copy-Item -LiteralPath "$outputDirectory/json/FunctionSystemConfig.bytes" -Destination "$destination/FunctionSystemConfig.bytes" -Force
Copy-Item -LiteralPath "$outputDirectory/json/CommonTextConfig.bytes" -Destination "$destination/CommonTextConfig.bytes" -Force
Copy-Item -LiteralPath "$outputDirectory/json/LanguageConfig.bytes" -Destination "$destination/LanguageConfig.bytes" -Force
Copy-Item -LiteralPath "$inputDirectory/propLanguage.xlsx" -Destination "$root/config/propLanguage.xlsx" -Force
foreach ($name in @('HomeHospitalSetting', 'HomeBuilding', 'HomeBuildingLevel', 'HomeBuildingCost', 'HomeBuildingCondition', 'HomeBuildingGoods', 'HomeBuildingItem', 'VillageSetting', 'VillageTask', 'Task', 'TaskChapter', 'TaskUnlock', 'TaskGuide', 'TaskSetting', 'HomeOpening', 'OpeningDialogue', 'OpeningRescue')) {
    Copy-Item -LiteralPath "$outputDirectory/json/${name}Config.bytes" -Destination "$destination/${name}Config.bytes" -Force
}
