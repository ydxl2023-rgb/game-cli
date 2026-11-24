# Contract assembly

## Rollback and cache

Handled failures after preflight restore the exact previous prefab bytes, prefab
metadata, affected texture-import metadata and declared state-preview files.
Previously nonexistent outputs are removed. Restoration uses byte snapshots, not
authored prefab serialization. Failure reports include `rollback: restored` or the
restoration error. Process termination/OS crash is not covered by these in-memory
snapshots. Image generation and `ui-prepare` writes are outside this transaction.

Builds with `--output` save derived evidence under `Library/UnityCLI/build-cache`.
A subsequent `--replace` build can reuse outputs only after hashing the contract,
manifest, current resource files and dependency metadata, Unity version, tool
assembly, project settings, output prefab and state previews/text metrics.
Reports expose `cacheHit`. Any missing/changed output, resource, importer metadata
or relevant input invalidates the whole cached build. Dry-runs always validate;
cache reuse never grants approval or changes Track task state. No partial-node
rebuild or image-generation cache is implemented.

The regression script includes unchanged reuse, corrupted-preview invalidation,
contract-change invalidation, post-save failure rollback and GUID preservation.

## One-command production

`ugui --json contract.json --asset-manifest resources.json --output <report-directory> [--replace]`

This performs contract compilation, resource/skin preflight, prefab construction,
binding, save, saved-asset readback and all declared preview states in one bridge
request. The compiled snapshot is shared by preflight and construction. Previews
instantiate the saved prefab in isolated scenes; Animator playback is disabled for
deterministic static states. No separately authored preview snapshot is required.

The output contains `build-report.json`, state PNGs and text metrics. Reports include
stage, elapsed time, prefab/contract/preview hashes, node and component counts, and
failure messages. `--dry-run --output <directory>` writes a validation report only,
without prefab writes or previews. Without `--output`, the existing assembly-only
behavior remains compatible. Valid unchanged outputs can use the hash-verified
cache described above; changed inputs or outputs regenerate all preview states.
Static previews do not validate business interactions or animation playback.

Preview states may specify `animationTime` in seconds (0 through 300). The builder
samples each active Animator's single verified clip on the isolated preview copy,
using wrapped time for looping clips. Reports include sampled clip times and node
positions, scales and CanvasGroup alpha. This shows deterministic poses, not live
state-machine acceptance. Use `prefab verify` animation checks for actual Animator
Rebind/Update evaluation, and `prefab motion-preview` for playable review scenes.

## Direct Art workflow

### Fixed resource preparation

Image quality is controlled by optional `imageProcessing`:
`{"resizeMode":"contain","sampling":"bicubic","maxUpscale":2,"maxAspectDifference":0.05,"maxCropFraction":0.15}`.
Defaults are proportional `contain` for icons/effects/decorations, centered `cover`
for backgrounds, and `nine-slice` for sliced surfaces. No arbitrary anisotropic
resize is supported. Over-limit aspect mismatch, upscale or background crop fails
before production writes. Existing images are not automatically reprocessed.

`bicubic` (default) and `lanczos3` use linear-light, premultiplied-alpha resizing;
nearest-neighbor is available only through explicit `sampling: pixel-art`.
Sliced surfaces require measured `imageProcessing.sourceBorder` (left/top/right/
bottom) in the alpha-cropped source image, plus target `border`. The nine patches
retain proportional corners and stretch only designated middle bands. Missing,
overlapping or inconsistent borders fail rather than inventing a slicing plan.
The final locked visible-bounds and coverage checks still apply: proportional
fitting never bypasses contract geometry. Incompatible artwork must be regenerated.

`ui-prepare --project <project> --json contract.json --asset-manifest source-receipts.json --output resources.json [--replace]`

Source receipts retain `contractFileSha256` and `resources`/`assets`. A generated
sprite receipt can add `sourcePath` (physical generated PNG) beside `path` (the
destination Assets path). The CLI aligns transparent padding and delivery size,
checks frozen bounds and coverage, prepares PNGs and computes the final mapping.
No separate image-processing script is required. Resources without `sourcePath`
are read and hashed unchanged. Reuse assets cannot be transformed.

Generated sprite slots explicitly declare `purpose`: `background`, `surface`,
`icon`, `divider`, `effect` or `decoration`, and `reskinStrategy`: `preserve` or
`align-visible`. `preserve` requires exact input size. `align-visible` fits the
alpha silhouette to the declared rectangle; it does not invent missing content.
`surface` requires positive whole/center coverage floors. A wrong panel shape
fails and must be regenerated. All resources are validated in memory before
writing images; write-time I/O failures are not a complete filesystem transaction.

Baseline capture now requires explicit `purpose`; it no longer guesses from
filenames. Existing build-only contracts remain compatible. The CLI does not
create artwork, alter UI layout or loosen frozen checks.

`ui-resources --json contract.json --output resource-info.json` extracts slots
and their node usage without Unity or a separately authored production document.
Art reads this information, applies the task's style and generates/reuses resources.
Optional slot `description` carries visual semantics; when absent Art reads the
existing task context rather than inventing new requirements.

After generation, Art records only actual paths:

```json
{
  "contractFileSha256": "hash from resource-info.json",
  "resources": [{"slotId":"resource0","path":"Assets/UI/font.ttf"}],
  "assets": [{"path":"Assets/UI/Login.controller"}]
}
```

`ui-resources --json contract.json --asset-manifest receipts.json --output resources.json`
computes actual hashes and produces the assembly manifest below. Use `--project`
to identify the Unity project when running outside it. No image tool is invoked by
this command: Art still uses its available image-generation capability. These
machine outputs do not create another design or approval step. Actual dimensions,
import settings and component bindings are checked by `ugui --dry-run`.

`ugui --json contract.json --asset-manifest resources.json --dry-run`

After a successful dry-run, omit `--dry-run` to build. Existing output requires
`--replace`. The command does not modify open scenes. A multi-window production
contract requires `--prefab Assets/...prefab` to select exactly one window.

Accepted inputs are `ui-production-contract/1` and `ui-task-input/1` with type
`prefab-assembly`. Original Web visual-ui behavior remains available.

## Explicit resources

```json
{
  "schemaVersion": "ui-contract-resources/1",
  "contractFileSha256": "SHA256 of exact contract file bytes",
  "resources": [
    { "slotId": "resource0", "path": "Assets/UI/font.ttf", "sha256": "file SHA256" }
  ],
  "assets": [
    { "path": "Assets/UI/Login.controller", "sha256": "file SHA256" },
    { "path": "Assets/UI/Login.anim", "sha256": "file SHA256" }
  ]
}
```

Every resource slot must map exactly once. Paths are project-relative Assets
paths; hashes verify bytes, not authorization. Fonts and single sprites are
supported. Sprite dimensions, PPU and border come from the contract, never from
model inference. Reuse slots must match the sourceReference hash. Controllers
and their animation/controller dependencies must appear in `assets` with hashes.

For geometry-preserving reskins, sprite slots can declare `visibleBounds`:
`{"x":1,"y":0,"width":418,"height":84,"alphaThreshold":128,"tolerance":2}`.
Coordinates are top-left pixels in the delivery image. Both build and dry-run
decode the actual image and reject an alpha bounding box outside the tolerance.
Capture these values from the original approved skin. This measures the outer
silhouette, not internal button-face geometry; matching bounds does not replace
visual comparison. Existing contracts without the field remain compatible.

Run `Tools~/capture-skin-baseline.ps1 -Project <project> -Contract <new-contract>
-BaselineResources <original-resources.json> -Output <guarded-contract.json>` before
reskinning. It verifies original asset hashes and captures every sprite's bounds.
The emitted `.baseline.json` records original measurements and hashes. Filled
surfaces (Background/Panel/Header/Bar/Button/Plate filenames) additionally require
at least 65% of original whole-image alpha coverage and 70% of original central
coverage. Icons/particles retain bounds checks without an opaque-center rule.
These filename categories are a default heuristic; review the captured thresholds
against semantic usage before freezing. Bounds are scaled to delivery resolution.
New Art must use the original asset as a structural reference. Normalize transparent
padding only when the design is otherwise correct; regenerate a wrong panel shape.
Never lower a frozen guard merely to make a bad asset pass.

The command binds existing Art assets. It does not generate images, fonts or
animation curves. Controller animations require explicit controllerPath and component
bindings. Declarative tracks can instead reference one existing `.anim` asset via
`assets[].animationId`; the builder checks rate, duration, loop, all target paths,
component/property bindings, track count and absence of events/object curves.
Such clips remain external runtime assets: client lifecycle playback is not
implemented by static assembly and is explicit in the report. Effects must
reference a declared animation. Other resource/effect forms fail explicitly.

## Validation and execution

The same in-memory snapshot compiler runs in dry-run and write modes. The existing
restore service validates actual Unity component types/properties, references and
hierarchy in an isolated scene before the write pass. Formal execution configures
sprite importers and saves the prefab, then checks the saved hierarchy, layout,
rotation, scale and declared serialized component properties. No intermediate
assembly JSON or model-authored hierarchy is consumed.

Root RectTransform must be full-screen stretch with centered pivot and zero
sizeDelta/anchoredPosition. No scene instance is replaced. Existing prefab GUID
is preserved on replacement by Unity's prefab save operation.

Preview states, external style references, animation playback and business
interactions are not acceptance-tested by this assembly command. A successful
build is not full task acceptance. Writes are not an asset transaction: importer
changes or an output may remain after a write-pass failure. Do not claim rollback.

## Regression

Run in an open Unity project containing this package:

`pwsh.exe -File Tests~/test-contract-build.ps1 -Project <project> -OutputDirectory <evidence>`

The regression uses unique test prefabs under Assets/UnityCliContractTests and
retains them for inspection. It tests dry-run isolation, manifest hash rejection,
extra slot rejection, generation, overwrite protection, byte-stable replacement
and GUID preservation.
