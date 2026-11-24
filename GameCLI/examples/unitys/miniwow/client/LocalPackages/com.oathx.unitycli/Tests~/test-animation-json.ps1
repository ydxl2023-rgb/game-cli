param(
    [Parameter(Mandatory=$true)][string]$Project,
    [Parameter(Mandatory=$true)][string]$Cli,
    [Parameter(Mandatory=$true)][string]$InputJson,
    [string]$Clip = 'Assets/GameAIValidation/CurveRoundtrip.anim'
)
$ErrorActionPreference = 'Stop'
function Invoke-Clip([string[]]$Arguments) {
    $result = (& $Cli --project $Project --format json animator clip @Arguments | ConvertFrom-Json)
    if (-not $result.ok) { throw $result.message }
    return $result
}
# Use a dedicated test asset; never overwrite an existing production clip.
if (Test-Path (Join-Path $Project $Clip)) { throw 'Test output already exists.' }
$source = Get-Content -LiteralPath $InputJson -Raw | ConvertFrom-Json
Invoke-Clip @('import-json','--clip',$Clip,'--json',$InputJson,'--dry-run') | Out-Null
try {
    Invoke-Clip @('import-json','--clip',$Clip,'--json',$InputJson) | Out-Null
    $response = Invoke-Clip @('export-json','--clip',$Clip)
    $actual = $response.data.details[0] | ConvertFrom-Json
    if ($source.frameRate -ne $actual.frameRate -or $source.loop -ne $actual.loop -or $source.curves.Count -ne $actual.curves.Count) { throw 'Clip metadata mismatch.' }
    foreach ($curve in $source.curves) {
        $found = @($actual.curves | Where-Object { $_.path -ceq $curve.path -and $_.component -ceq $curve.component -and $_.property -ceq $curve.property })
        if ($found.Count -ne 1) { throw 'Curve binding mismatch.' }
        $target = $found[0]
        if ($curve.preWrapMode -ne $target.preWrapMode -or $curve.postWrapMode -ne $target.postWrapMode -or $curve.keys.Count -ne $target.keys.Count) { throw 'Curve metadata mismatch.' }
        for ($i=0; $i -lt $curve.keys.Count; $i++) {
            foreach ($field in @('time','value','inSlope','outSlope','inWeight','outWeight','weightedMode','tangentMode')) {
                $a=[double]$curve.keys[$i].$field
                $b=[double]$target.keys[$i].$field
                if ($a -ne $b -and [Math]::Abs($a-$b) -gt [Math]::Max(0.0000001,[Math]::Abs($a)*0.000001)) { throw "Key field mismatch: $field" }
            }
        }
    }
    Write-Output 'Animation clip roundtrip passed.'
} finally {
    & $Cli --project $Project --format json asset delete --src $Clip | Out-Null
}
