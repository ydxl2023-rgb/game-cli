param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$folder = "$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
$project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client'
$config = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/config'
$cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$report = Get-Content "$folder/report.json" -Raw | ConvertFrom-Json
function Invoke-Excel([string[]]$Arguments) {
    $result = & $cli --project $project --format json excel @Arguments | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 6 -Compress) }
    return $result.data
}
$query = Invoke-Excel @('query','--file',"$config/propCommonText.xlsx",'--sheet','CommonText','--header-row','2','--count','10000')
$existing = @{}
$maximum = 0
foreach ($row in $query.rows) {
    $values = @{}
    foreach ($cell in $row.cells) { $values[$cell.field] = $cell.value }
    $existing[$values.ID] = $values
    $maximum = [Math]::Max($maximum,[int]$values.NumID)
}
if ($Apply) {
    Copy-Item -LiteralPath "$config/propCommonText.xlsx" -Destination "$folder/propCommonText.before.xlsx"
    Copy-Item -LiteralPath "$config/propLanguage.xlsx" -Destination "$folder/propLanguage.before.xlsx"
}
$added = 0
foreach ($text in $report.texts) {
    if ($existing.ContainsKey($text.alias)) {
        if ($existing[$text.alias].Context -ne $text.text) { throw "Existing alias differs: $($text.alias)" }
        continue
    }
    $maximum++
    $values = @{ID=$text.alias;NumID=$maximum;Context=$text.text} | ConvertTo-Json -Compress
    $arguments = @('insert','--file',"$config/propCommonText.xlsx",'--sheet','CommonText','--header-row','2','--values-json',$values)
    Invoke-Excel ($arguments + '--dry-run') | Out-Null
    if ($Apply) { Invoke-Excel $arguments | Out-Null }
    $added++
}
Write-Output "Home aliases to merge: $added; applied: $Apply"
