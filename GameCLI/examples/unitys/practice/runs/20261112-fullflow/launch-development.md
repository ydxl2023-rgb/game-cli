# Launch development handoff

Developer: trial_launch_dev. Target: practice/client only. Source: frozen SvStart v6 in templates/SvStart.json; Studio project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, root T1T2Start0.

## Code delivered

The practice package XLauncher already matches the v6 template source. Reused it rather than adding another launcher or system. Changed only UpdateProgress whitespace/conditional formatting to comply with the current return and grouping rules. Existing script GUID a7430fbd036945eab090df8dd3354b58 is unchanged. No development or shared external package was edited.

File: client/LocalPackages/com.oathx.core/Runtime/XLauncher.cs. Roslyn analyzer evidence: launch-code-check.json, policy oathx-code/1, zero issues, 18/18 documented members. This was a direct analyzer call because verify code --system does not cover package files; it is not Unity compilation evidence.

## Public integration contract

- Show()/Hide(): root activation only.
- ShowLoading(message): switches overlays and resets visible progress to zero.
- ShowUpdate(message): switches to the download overlay and resets visible progress to zero.
- UpdateProgress(progress, text): normalized [0,1], NaN becomes 0; returns true after updating the selected overlay. Supply real observed startup progress; no simulated task.
- SetTip(text): sets the auxiliary loading text.
- ShowBackground(): disables Loading and Update while retaining the root/background. Root integration can call existing XApp.ShowLaunchBackground() after UILogin is actually ready. Do not invoke PracticePlugin's old HideLaunch() at that point.

The template has no separate percentage nodes. Keep loadingPercent and downloadPercent null; the existing implementation combines message and percent into the corresponding DownloadTips label. Required references must be assigned explicitly; no hierarchy search fallback is added.

## Required resource bindings (not yet executed)

Asset: Assets/Data/Widgets/Launch/Launch.prefab. Add Oathx.Core.XLauncher to Launch root through UnityCLI. Names below are root-relative and derived from v6 nodes, not guessed legacy paths.

| Serialized field | Target | Component |
| --- | --- | --- |
| loading | Animator/Loading | GameObject |
| update | Animator/Update | GameObject |
| loadingText | Animator/Loading/DownloadTips | UnityEngine.UI.Text |
| loadingPercent | none | null, combined label |
| tip | Animator/Loading/Tip | UnityEngine.UI.Text |
| downloadText | Animator/Update/TipsGather/DownloadTips | UnityEngine.UI.Text |
| downloadPercent | none | null, combined label |
| fill | Animator/Update/Slider/FillArea/Fill | RectTransform |
| handle | Animator/Update/Slider/HandleSlideArea/Handle | RectTransform |

No VM/Metadata is expected: Launch is a plain UIBehaviour. Its serialized references require direct inspection; a zero-widget bindings report does not validate XLauncher fields.

## Resource sequence for the granted Editor slot

1. Verify practice binding and ping target. Use inputs/Launch database-derived assets and source-design.json. The CLI ugui name must be UILaunch; output asset remains Launch/Launch.prefab. Dry-run then generate without scene replacement.
2. Preserve the actual template name Launch, rect transforms, active states, UV data and image references through the available declarative UnityCLI prefab restore path. Do not hand-write YAML. Keep template default Loading visible and Update hidden.
3. Add XLauncher and assign the references above using CLI component/modify commands, then read back.
4. Create the original Loading_Anim_01 clip/controller through animator commands. Frozen source has four curves: Ball Euler X/Y/Z and RateBg/Arrow RawImage m_UVRect.x. Resolve targetNodeId to the final paths relative to Animator: Loading/Ball and Loading/RateBg/Arrow. The historical sourcePath strings differ and must not be copied blindly. Preserve full keys/tangents/wrap settings from frozen JSON; original host is Animator in this template.
5. Ensure background/graphic raycastTarget is false so the retained background cannot block Login clicks. This is particularly relevant if the new Launch canvas stays above the UISystem canvas.
6. Root integration replaces the minimal placeholder Launch scene instance via supported Editor/AssetDatabase APIs, binds the new XLauncher instance to XApp.launcher, and saves the intended scene. Do not call PracticeSetup.Build: it rebuilds the clean baseline and would erase trial integration.

## Still pending

No Unity/CLI Editor action was performed because another agent owns the slot. Prefab creation, raw-image preservation, animation creation, XApp scene binding, Unity compile, runtime progress/NaN/boundary checks, background-only behavior, and independent acceptance remain pending. The existing practice scene still has its minimal placeholder launcher; it is not the v6 visual delivery.

Fixed default status strings remain the template's English API defaults for pre-plugin startup compatibility. Product-localized startup messages should be supplied by the integration owner; XLauncher cannot depend on application-level Config.Excel from its package assembly.

## Offline resource script prepared
inputs/Launch/generate-launch.ps1 is ready, syntax-parsed without errors and not executed. Loading_Anim_01.clip.json retains all four curves, eight keys, original tangents/weights and wrap modes, 30 fps and loopTime=true. Use root-owned Editor integration to instantiate the finished prefab; ugui rebuilds its output and must not be reused merely to instantiate after binding. Run the script only with the exclusive Editor slot. CLI logs are saved to launch-cli.jsonl. Final clip readback is requested for comparison.


## Resource execution completed
The authorized Editor slot produced Launch.prefab, Launch.controller and Loading_Anim_01.anim. All CLI operations completed. Readback confirms 23 nodes, no missing scripts/references, seven required XLauncher references correct, two optional percent references null. Animation readback matches all four curves/eight keys within 0.00001, including paths, tangents, weights, modes, 30fps and loop. Console returned zero errors. Evidence is inputs/Launch/launch-cli.jsonl (token redacted), prefab-inspection.json, Loading_Anim_01.readback.json and animation-comparison.json. Scene is unchanged; root still owns instantiation/XApp binding and runtime checks. Editor slot released.

