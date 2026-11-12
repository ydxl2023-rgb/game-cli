# Loading development handoff

Source: database SvLoading v15, captured in templates/SvLoading.json. Target project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, Unity identity 44f4f783-645c-41e7-80fe-aead8477c978. Developer: trial_loading_dev.

## Code delivery

- Assets/Scripts/Systems/Loading/UI/UILoading.cs and its original GUID 246121cfd90811f4898216f0e4fb5ecc.
- Assets/Scripts/Systems/Loading/VM/VMLoading.cs and its original GUID bd6493db3e6a639428855765e180a7d2. VM moved from the template's UI directory to the required VM directory before initial import; binding order unchanged.
- No LoadingSystem, task scheduler, simulated progress or network integration.
- UpdateProgress(float progress, string message) returns false without a bound VM; otherwise normalizes NaN to 0 and clamps to [0,1], writes message plus rounded percentage, returns true. The old hardcoded default Loading... was removed. Caller supplies localized message or dynamic task status.
- SetTip(string text) returns false without VM; otherwise updates the tip (null clears) and returns true.
- Shared CommonText additions are not required by this component. Test driver should pass localized message/tip from its own configured data.

Open through UISystem.OpenWidget<UILoading> with an explicit callback and positive widget guard. Initialize only after the callback; caller checks system activation before opening. Close through a separately declared System.Type[] widgets containing typeof(UILoading). Reaching 100% does not auto-close.

## Binding plan

Prefab path: Assets/Data/Widgets/Loading/Loading.prefab. Root display name Loading, host Animator.

| Index | Field | Sync | Target relative to prefab root |
| --- | --- | --- | --- |
| 0 | rate | SyncText | Animator/RateBg/Rate (UnityEngine.UI.Text) |
| 1 | tipText | SyncText | Animator/RateBg/TipText (UnityEngine.UI.Text) |

Use widget-metadata UILoading --vm VMLoading --prefab Assets/Data/Widgets/Loading/Loading.prefab, dry-run first. Check classGuid points to VMLoading, two fields in this order, and both target/component pairs match. UISystem can attach UILoading at runtime; do not infer failure merely because no Widget is serialized.

## Resource and animation plan (not yet executed)

Root prepared inputs/Loading/Loading.visual-ui.json, asset-manifest.json, PNGs and source-design.json. These are generation inputs, not accepted assets. UnityCLI ugui requires UI-prefixed command name UILoading. Correct the generated root name and exact RectTransform/active state with the formal CLI restore mechanism; never edit prefab YAML. Preserve the template hierarchy of 12 nodes and five PNG assets, active flags, opacity and RawImage UV settings.

Create the template animation Loading_Anim_01 in Assets/Data/Widgets/Loading/Animations through UnityCLI Animator commands, default state Loading_Anim_01, speed 1, loop true, 30 fps and 2 second duration. Animator belongs to Loading/Animator. Map curves by targetNodeId in the locked design, not historical sourcePath:

- T1T1N0_5 -> RateBg/Ball, Transform localEulerAnglesRaw.y and localEulerAnglesRaw.z.
- T1T1N0_3_0 -> RateBg/Arrow, UnityEngine.UI.RawImage m_UVRect.x.

The source snapshot still stores old sourcePath Ball and RateBg/eff_jiantou_01, which no longer match the actual hierarchy. Reusing these strings would silently break animation. Transfer all provided key times, values, tangents and wrap modes; do not invent a replacement rotation.

## Verification

Executed: gamecli verify code --project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484 --system Loading, exit 0, passed, two source files, zero issues.

Evidence: client/Library/GameCLI/Verify/783291a0-a73c-4969-924f-ff049069dead/report.json (relative to practice).

Not executed: Unity compilation, resource creation, Metadata/animation installation, runtime open/progress/close/reopen, independent acceptance. Editor slot belongs to another developer; no Unity commands were issued by this agent.

Independent runtime cases: normal 0/0.5/1, below/above range, NaN, null message/tip, update before VM bind, close/reopen, verify only caller-supplied progress is shown. Test driver must explicitly activate the page despite the template asset's inactive root, using UISystem's supported open lifecycle.

## Resource completion update

Executed under exclusive Editor slot: UGUI dry-run/generation, snapshot restore dry-run/write, Metadata dry-run/write, full animation JSON import dry-run/write, controller/state creation, Animator binding, Console check. No Play mode entered.

Final prefab: Assets/Data/Widgets/Loading/Loading.prefab. Metadata has two correct bindings. Animation: Assets/Data/Widgets/Loading/LoadingLoop.anim and Loading.controller on Loading/Animator, curves mapped to RateBg/Ball and RateBg/Arrow; loop explicitly true. Console returned zero errors.

Prefab verification: overall incomplete due existing tool evidence limitations, zero layout differences; bindings passed (one Widget, two fields). Raw report inputs/Loading/prefab-verification.json; command evidence loading-cli.jsonl; animation readback clip-inspect.json. Initial restore failure was fixed in this module's snapshot input: image hash 86b4bf... is JPEG bytes labelled PNG in the database, so IHDR parsing produced invalid dimensions. Used actual decoded dimensions without changing source bytes/hash. This source MIME mismatch remains a template production-material defect to report.

Remaining: runtime open/progress/tip/close/reopen and independent acceptance. Editor slot released after this update.
