# Task-driven prefab create

This is independent of `ugui`. It reads a task-layout JSON directly; it does not require HTML extraction, visual-ui.json or an asset manifest. It creates assets through Unity Editor APIs in a temporary preview scene, without replacing or saving the user's scene.

```powershell
UnityCLI.exe --project <UnityProject> --format json prefab create --json <task-layout.json> --prefab Assets/UI/Toast.prefab --dry-run
UnityCLI.exe --project <UnityProject> --format json prefab create --json <task-layout.json> --prefab Assets/UI/Toast.prefab
UnityCLI.exe --project <UnityProject> --format json prefab inspect --prefab Assets/UI/Toast.prefab --recursive
```

Use `--replace` explicitly for existing assets (including replacement dry-runs). No default overwrite. Check `ok`, `issues`, exit code and Console errors before proceeding.

## Input contract (schemaVersion 1 or 2)

Top level: `schemaVersion:2` for new layouts (1 remains accepted), nonempty `taskId`, `source` (approved document path/anchor), positive `width`/`height` design resolution, `root`.

Each node: unique `id` (also Unity object name), `type`, positive `width`/`height`, optional `x`/`y` (default 0, relative to parent's top-left, x right/y down), `children` (draw order), `color` (#RRGGBB or #RRGGBBAA, default white), `raycastTarget` (default false). Root must be Panel and match design width/height; its RectTransform is full-screen stretch. Place it under a Canvas with matching reference resolution; the command does not create/change the host Canvas.

Supported types:
- Panel: transparent container and CanvasGroup; `blocksRaycasts` defaults true, explicitly false for nonblocking feedback.
- Container: RectTransform only.
- Image: optional `sprite` existing imported Sprite asset path, `sliced` boolean. No sprite means whitebox solid Image.
- Text: `text`, `fontSize` (1..300), centered. Existing USE_OLD_TEXT project mode selects legacy UI.Text, otherwise TMP. Does not modify project defines or import fonts.
- Button: by default Image and Button on the same root (existing JSON remains supported). To preserve a separate background child, set `targetGraphic` to that direct child Image ID, with `raycastTarget:true` on the background. This creates Button without a root Image and binds its targetGraphic after children are built. Missing, non-Image, non-child or non-raycast targets are rejected. Preserve original node IDs and sibling order; never merge an explicitly separate background. No business event handlers are generated.
- ScrollRect: `direction` horizontal/vertical, `viewport` and `content` node IDs. References must resolve to direct ScrollRect -> Viewport -> Container Content children. Additional siblings, such as a transparent ViewportHitArea before Content, are preserved. Content bounds/item positions are explicit in JSON; no automatic layout group. Clamped movement.
- Viewport: RectMask2D and, by default, Image for backward compatibility. Set `viewportImage:false` to create only RectMask2D; gesture capture may then come from an explicit child Image with `raycastTarget:true`.

## Explicit layout and state parameters

- `name`: Unity GameObject name, separate from the stable `id`. IDs may contain `/` (for example `W01/L01`); names must not contain path separators or newlines. If omitted, name falls back to id. Sibling names must be unique. All references still use IDs. Use the intended root name as the output prefab filename because Unity names the saved prefab root after its asset.
- `active` defaults true; false persists an initially hidden subtree.
- Button `interactable` defaults true; false persists the disabled state. Runtime business state transitions remain client code responsibility.
- `anchorMin`, `anchorMax`, `pivot`: optional two-number arrays in [0,1]. Both anchors must be supplied together, min <= max. Defaults remain top-left. x/y/width/height still describe the rectangle at the parent's authored design size; the generator computes offsets so the same rectangle is preserved with the requested anchors. `[0,0]` / `[1,1]` gives stretch, `[0.5,0.5]` for both gives center anchoring. The root always stays full-screen stretch.
- Text `alignment`: Unity TextAnchor name, such as `UpperLeft`, `MiddleCenter`, `LowerRight`. `wordWrap` defaults true. `overflow`: `Overflow` (default), `Truncate`, or `Ellipsis`; legacy USE_OLD_TEXT rejects Ellipsis explicitly. `autoSize` defaults false; `minFontSize` / `maxFontSize` default 12/72 and must satisfy 1 <= min <= max <= 300.
- Text `font`: optional imported TMP_FontAsset path (or Font asset with USE_OLD_TEXT). A missing or incompatible asset is rejected. For multilingual text and symbols, bind fonts with the actual required glyphs; the default font does not guarantee CJK support.
- `safeArea:true`: attaches the runtime `UnityCliSafeArea` component to a non-root control. It clamps the authored rectangle into `Screen.safeArea`, shrinking only when it exceeds the safe area. It preserves hierarchy, recalculates from original offsets on resize/rotation, and uses the parent Canvas camera. Apply to the relevant top-level content groups, not every descendant. The root must remain full screen and rejects safeArea. This supports Screen Space Overlay/Camera; World Space is not adjusted. A generated prefab alone does not demonstrate device-specific safe-area validation.

Example: `{"id":"W01/L03","name":"Version","type":"Text","x":258,"y":200,"width":700,"height":65,"text":"01234567890123456789012345678901","fontSize":32,"autoSize":true,"minFontSize":18,"maxFontSize":32,"wordWrap":false,"alignment":"UpperLeft","overflow":"Truncate","safeArea":true}`.

Unknown fields/types, duplicate IDs, missing sprites, nonfinite/invalid rectangles, broken scroll hierarchy and output path traversal fail before saving. Limit: 1 MiB input, 512 nodes, depth 24. No scripts or YAML from input are executed.

## Task extraction

Prefab Agent reads the approved task source and matching wireframe; identifies the owned window/list-item, hierarchy, control IDs/types, local rectangles, draw order, input behavior and resource slots; writes task-layout.json and an extraction note mapping each node to source. It must not infer new business scope or fabricate unspecified required dimensions. This is semantic extraction by the Agent, not automatic parsing of arbitrary requirement HTML by the CLI.

Whitebox: use pure colors and separate text. Final assembly: reference actual imported Art sprites. Layout groups, animations and business bindings are not silently inferred. Inspect generated assets and report remaining integration work accurately.

## Regression verification

### Windows execution-account access

If endpoint reading succeeds but pipe connection is denied, the execution account differs from the Editor account. To explicitly authorize a trusted local execution identity, create `Library/UnityCLI/pipe-access.json` in that Unity project: `{"allowedClientSids":["<actual Windows account SID>"]}`. Obtain the SID from `whoami /user` **under the actual execution identity**, not the Editor user. Reload the Editor bridge after changing the file. This local configuration is not distributed or committed.

When configured, the Windows pipe is created with an explicit DACL: owner/System retain full access and the listed accounts gain read/write. Broad built-in group SIDs are rejected, remote pipe clients are rejected, and every request still requires the existing endpoint token. Listing an account grants that trusted identity access to the bridge's commands; it is not a per-command role policy. Do not add unrelated identities or publish the endpoint token. With no configuration, previous pipe creation behavior is retained. Filesystem and Agent sandbox policies are unchanged.

With the target Unity Editor open, run `pwsh.exe -File Tests~/test-prefab-create.ps1 -Project <UnityProject> -OutputDirectory <temporary-output-directory>`. This creates/replaces only `Assets/GameAI/Tests/UIPrefabCreateRegression.prefab`, inspects hierarchy and serialized settings, checks rejection cases, and checks legacy JSON compatibility. It does not change/save the user's scene.


## Offline snapshot restore (layout whitebox)

`prefab restore --json <ui-template-snapshot.json> --prefab <Assets/...prefab> [--dry-run] [--replace]` consumes resolved `ui-template/1` nodes. Unlike task-driven create, it preserves root RectTransform anchors and sizeDelta. It creates an isolated preview scene and never saves/replaces the current user scene. Assets are not overwritten without explicit --replace.

Only the registered built-in UGUI types are accepted. Unsupported properties, ambiguous component references and unresolved PrefabInstance nodes fail rather than silently dropping data. Missing legacy Text fonts use LegacyRuntime.ttf. Missing source sprites, animation and custom scripts are not recreated; this does not promote a reference-only template to production-ready.

Verified fixture: offline SV UIRecallMemoryItem, 16 nodes/29 standard components, 390 serialized layout/property comparisons pass after restore and re-export. RectTransform local XY is derived; compare anchors/pivot/sizeDelta/anchoredPosition/local Z instead. Runtime/visual equivalence was not established.

### Explicit visual snapshot mapping
Restore optionally accepts `visualTextures: [{path, settings}]` for already imported single sprites (source TextureImporter settings: spriteMode=1, spritePixelsToUnits and spriteBorder). Dry-run validates without importing settings; formal restore configures the sprites. Object-reference properties may use `{assetPath:"Assets/..."}` for m_Sprite or m_Font. Source scripts are never loaded. Optional `visualPreview:{width,height,output}` renders the restored object in an isolated preview Canvas to PNG after saving. Preview dimensions are limited to 64..4096. This first visual adapter deliberately does not reconstruct arbitrary materials, sprite sheets, animation or particles.

### prefab restore 贴图交付尺寸

每个 `visualTextures` 条目须填写 `deliveryWidth` 和 `deliveryHeight`（1..8192）。工具在 dry-run 与正式执行前先检查全部源图片像素尺寸，缺失或不匹配直接拒绝，不以 RectTransform 尺寸代替贴图尺寸。旧 restore 参数需要补齐。例：`{"path":"Assets/UI/Button.png","deliveryWidth":92,"deliveryHeight":92,"settings":{"spriteMode":1,"spritePixelsToUnits":100,"spriteBorder":{"x":0,"y":0,"z":0,"w":0}}}`。母版单独留存，绑定经过尺寸处理的交付文件。

### 独立清单验证（GameCLI）

`gamecli ui-library check-manifest --input snapshot.json --project <Unity项目> --output <新report.json>`

输入为 ui-template/1 快照，`visualTextures` 列出 `path`、`deliveryWidth`、`deliveryHeight`、`deliverySha256` 与 `settings`。每个绑定的 Sprite 必须列入清单。第一版验证 PNG 文件头宽高和文件哈希、文件存在、路径、重复项、PPU、九宫格边距及绑定覆盖；不做图像解码或美术风格验收。JSON 报告逐项提供 expected/actual 与 issues。退出码 0 为全部通过，4 为失败；不修改任何资产，报告必须使用新文件名。Unity restore 在写入前复查清单及哈希，不能只凭之前的报告放行。

### Explicit Animator restore and motion preview

`prefab restore` accepts `UnityEngine.Animator` with an explicit `m_Controller: {assetPath:"Assets/...controller"}` reference. Controllers and clips must first be produced through `animator`; source scripts remain excluded. Missing controllers fail restore.

`prefab motion-preview --prefab Assets/...prefab --json <settings.json> [--dry-run]` renders independent Animator samples in an isolated preview scene. Set `UNITYCLI_REQUEST_TIMEOUT_SECONDS=60` for multi-frame renders. Settings include `outputDirectory`, `width`, `height`, `frames` (2..120), `step` (seconds), and optional `reviewScene` (a new Assets/*.unity path). Existing review scenes are never overwritten. The source prefab and open user scenes are preserved.

The optional review scene contains a Canvas, scaler and EventSystem for manual Play Mode review. Preview generation does not perform acceptance checks.

### Generic contract verification (v1)

`prefab verify --prefab Assets/...prefab --json <ui-verification-contract/1.json> --project <project> --format json` evaluates source-owned obligations in an isolated scene. Supported kinds: node (optional active/siblingIndex), component, numeric/boolean property, animator.controller, animation.clip (name/duration/loop), animation.sample (default or named layer-zero state; timeSeconds 0..60; serialized numeric property or Transform localEulerAngles axis). Node IDs map to unique relative hierarchy paths in the contract. Unknown types/kinds return unverified, never pass.

Response contains evidenceJson (ui-verification-evidence/1), contract SHA256, check results and dependency fingerprints. Exit is nonzero when any obligation fails. Sampling uses Animator.Update in Editor; it is not device or Play Mode acceptance. Use UNITYCLI_REQUEST_TIMEOUT_SECONDS up to 300 for large contracts. GameCLI verify-delivery consumes this response and rechecks file hashes. See deliveries/ui-verification-v1/index.html in the GameAI repository for the implementation boundary and real regression evidence.
`motion-preview` only renders frames and records sampling time/Animator count. It performs no acceptance checks and emits no button-checks.json. All acceptance checks must use `prefab verify` with a contract, followed by GameCLI `ui-library verify-delivery`. Preview success never implies verification passed. Project-specific node names, triggers and expected values belong in contract data, not tool code.


## Snapshot whitebox restore

`prefab restore --json <assembly.json> --prefab <Assets/...prefab> --whitebox [--dry-run] [--replace]`

Guard validation always runs before restoration. Whitebox retains hierarchy, native components, transforms, internal references and resource slot metadata. External sprite/font/controller references are deferred, even if old resources exist. Text uses the built-in fallback font. Save the response `deferredBindingsJson` (nodeId, componentId, property, reference) with the unchanged approved assembly input. No final previews or texture imports run at this stage. Texture slot dimensions and nine-slice settings remain validated. Unsupported components/references still fail. Final assembly omits `--whitebox` and requires all resources and normal visual checks; whitebox success is not final delivery success.


## Independent texture configuration and static preview

- `prefab configure-textures --json <ui-template/1 manifest> [--dry-run]` validates all texture files, hashes, dimensions and borders before changing importers. Apply mode reads back Sprite Single, PPU and borders in `textureSettingsJson`. No prefab is changed.
- `prefab preview --json <preview snapshot> --prefab <target path> [--dry-run]` renders `visualPreview` in an isolated preview scene without saving the prefab or changing import settings. Configure textures first. Animator controller references are deferred and reported; this is a static preview, not animation validation. For hidden states use an explicitly derived preview snapshot with active/alpha adjusted; keep the approved source unchanged. Final restore remains strict and requires controllers.

### Isolated preview framing
`visualPreview.width/height` define the design canvas and output pixels. Optional `viewHeight` (positive, at most 100000), `centerX`, `centerY` change only the orthographic inspection camera. UI anchors and node transforms remain unchanged. The camera depth range covers active RectTransforms, including negative local Z. Wider views are inspection evidence, not device-layout acceptance.
`prefab preview` also writes `<output>.text-metrics.json` with actual UGUI font size, generated best-fit size, wrapping mode, rect dimensions and line count. Use these metrics to distinguish source configuration from missing restore properties; do not silently alter text to make a preview pass.

## Frozen production whitebox input

`prefab restore --json <production-input.json> --prefab <window.prefabPath> --whitebox --replace --dry-run` now accepts `ui-task-input/1` with `type=prefab-whitebox`. It deterministically consumes `window.nodes` without consulting historical snapshots. All native components and properties are preserved; declared resource and animation component bindings are explicit deferred slots in `deferredBindingsJson`. Sprite/texture references remain empty and Text uses the builtin font. Output path, root name, placeholder policy, duplicate bindings and non-null slot targets are rejected before writing. This mode cannot be used without `--whitebox` and does not constitute final resource/animation assembly. Use the same frozen input for actual restore after successful dry-run; save response and perform generic verification independently.


### Guarded resource manifest projection

`GameCLI ui-library guard --baseline <raw baseline> --candidate <raw target> --approval <reviewed approval> --manifest <verified Art ui-template snapshot> --output <new envelope>` retains all three pinned originals and adds a hashed resource manifest. Both guards verify identical flat prefab data, unchanged selected texture entries and complete coverage of actual PNG bindings. This explicit projection can omit unused historical import metadata; it cannot alter any target node, binding or retained import setting. Expanded instance provenance remains unchanged in the pinned snapshot. The server also checks the frozen production resource slots and upstream artifact hashes.

Final `prefab restore --json <projected envelope> --prefab <output> --replace --dry-run` must pass without `--whitebox` before formal assembly. A whitebox success does not prove final resource availability.

### Identity-preserving readable UI PNG names

`ProjectSettings/UnityCliAssetAliases.json` may register `unity-asset-path-aliases/1` entries with `from`, `to`, `guid`, `sha256`, `metaSha256`. After approval verification, restore and configure-textures resolve old UI PNG paths only when the new PNG and .meta bytes and Unity GUID match the registered identity. Same-directory moves preserve the original .meta through `asset move --src ... --dst ...`. Existing old paths must not be recreated. Approval JSON is never reserialized or changed to make a rename pass. New exports expose the readable physical names; internal blob hashes are unchanged.

Font aliases also support .ttf, .otf and .ttc with the same GUID/hash checks. The extension must stay unchanged. Name fonts from their actual family/subfamily metadata, for example MicrosoftYaHeiRegular.ttc and MicrosoftYaHeiBold.ttc; do not infer a font family from the UI Text node name.

Whitebox animation declarations may use `specification.tracks` instead of asset-reference
`bindings`. Tracks must name existing frozen target nodes and, when present, existing
component IDs; they are preserved verbatim in `deferredBindingsJson` entries with
`kind=animationTracks`. No Animator, controller, clip, or playback is synthesized during
whitebox conversion. Final animation delivery and runtime checks remain mandatory.
Native EventTrigger components and explicitly empty serialized arrays are supported;
this does not authorize importing custom script callbacks or nonempty callback lists.

## Lossless animation clip JSON

`animator clip import-json --clip Assets/...anim --json <absolute-file> [--dry-run]`
validates all curves before replacing the clip. `animator clip export-json --clip
Assets/...anim` returns the complete JSON in `data.details[0]` for roundtrip checking.
Schema: `animation-clip/1`; fields: `frameRate` (positive), `loop` (boolean),
`curves` array. Each curve has `path` (relative to animation root), `component`
(full Unity type), `property`, `preWrapMode`, `postWrapMode` (Unity enum integers),
and `keys`. Each key requires `time`, `value`, `inSlope`, `outSlope`, `inWeight`,
`outWeight`, `weightedMode`, `tangentMode` (Unity serialized bitfield).
Times must be nonnegative and strictly increasing. Infinite slopes may be encoded
as strings for stepped curves. Import replaces all curves; it does not copy source
AnimationEvents, controllers or business scripts. Preserve original source evidence,
map only approved tracks, and verify the imported asset using export-json.
