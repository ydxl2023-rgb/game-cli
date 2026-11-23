param([string]$Round = 'R1-retest')
$ErrorActionPreference = 'Stop'
$out = Join-Path $PSScriptRoot $Round
New-Item -ItemType Directory -Force $out | Out-Null
$repository = 'G:/Works/GameCLI/GameCLI'
$projectId = 'p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484'
$manifest = Get-Content -Raw (Join-Path $PSScriptRoot '../source-manifest.json') | ConvertFrom-Json
Push-Location "$repository/integrations/codex"
try {
  foreach ($root in $manifest.roots) {
    if ($root.name -in @('MasterSystem','RedDotSystem')) { continue }
    $destination = Join-Path $out "$($root.name)-prefab.json"
    if (Test-Path $destination) { throw "Evidence exists: $destination" }
    & node cli/gamecli.mjs verify prefab --project $projectId --prefab $root.root --asset "Assets/Data/Widgets/$($root.name)/$($root.name).prefab" --output $destination *> (Join-Path $out "$($root.name)-prefab-console.txt")
    [pscustomobject]@{ system=$root.name; exit=$LASTEXITCODE; report=$destination } | ConvertTo-Json -Compress | Add-Content (Join-Path $out 'prefab-exit-codes.jsonl')
  }
} finally { Pop-Location }
