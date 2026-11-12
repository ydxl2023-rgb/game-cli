# Independent runtime and prefab acceptance

Reviewer: trial_runtime_acceptance (not a developer). Prepared 2026-11-12.
Project: p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484; Unity identity 44f4f783-645c-41e7-80fe-aead8477c978.
Target: G:/Works/GameCLI/GameCLI/examples/unitys/practice/client.
Baseline: project-final-design.json revision 3; SvStart v6, SvLogin v7, SvNotice v4, SvLoading v15, MasterSystem v6.

## Status before Editor slot
No UnityCLI, Editor, compilation, Play or screenshots executed. Resources still being developed; no delivery hashes frozen. All required runtime/prefab checks are incomplete. Code verdict belongs to the independent code reviewer; this document does not turn source inspection into runtime evidence.

| Module | Code | Prefab | Runtime |
|---|---|---|---|
| Launch | incomplete | incomplete | incomplete |
| Login | failed: required local selection missing | incomplete | incomplete |
| Loading | incomplete | incomplete | incomplete |
| Master | incomplete | not-applicable: pure code template | incomplete |
| Notice | incomplete | incomplete | incomplete |

## Planned independent cases

Each case will retain input, actual, assertion, timestamps and original JSON. No simulated result is a pass.

- LCH-01: launch startup visible; supplied progress changes intended text/bar; ready Login retains Launch background while launch foreground is hidden. Screenshot before/after and active/component state.
- LGN-01: startup asynchronously opens one Login; VM ServerName/VersionText/CanEnter/Friends changes reach matching actual Text/Button/GameObject; restore test values.
- LGN-02: real Announcement Button.onClick invokes Notice via Master. Close/reopen 3 cycles; one page and one callback per click. Capture references, visible state and event count.
- LGN-03: local server selection changes entity/display and Enter records local pid in MasterEntity; no server calls and no Home. Pending entries remain clickable and visibly report pending integration.
- LGN-04: deactivate/reactivate Login; capture old button reference and observe listener cleanup without manufacturing fresh callbacks. Fresh instance is usable and has no duplicated callback.
- LDG-01: open UILoading with explicit completion callback; UpdateProgress values -1, 0, 0.375, 1, 2, NaN, negative/positive infinity produce 0,0,38,100,100,0,0,100 percent. Null message/tip clear safely. Compare VM and actual labels. Progress 1 does not auto-close.
- LDG-02: direct VM Rate/TipText update actual labels; close/reopen; verify non-cached widget lifecycle and no task growth while idle. Unbound interface behavior tested only on temporary test instance.
- MST-01: valid SystemActive/SystemDetive control Login/Notice; invalid null/string/abstract/self type rejected without exception. Local LoginSucceed pid>0 updates MasterEntity; pid<=0 leaves existing value.
- MST-02: unload Master then dispatch event; no effect; reactivate once and verify one response. Requires framework-supported lifecycle API and event observer that does not alter product code.
- NTC-01: visible standard content, collapse/expand via actual Buttons; empty/whitespace body toggles actual entry/empty widgets; SetContent and VM changes reach intended labels.
- NTC-02: normalize http/https successfully; reject javascript/file/relative/empty. Inspect actual link geometry after Canvas rebuild; TryGetLink on glyph center, outside and clipped area. Never invoke OnPointerClick on valid links or Application.OpenURL. Browser dispatch remains outside execution scope.
- NTC-03: sample opening animation across frames, confirm dialog content scale changes and fullscreen mask does not scale. Actual input/raycast checks establish click blocking.
- NTC-04: Close via Master, reopen 3 times; listener count/event count and no retained visible orphan. Weekly suppression only after root verifies isolated productName GameCLIPracticeTrial; never clear existing prefs.

## Probe API and isolation
Temporary probe, if required, is staged under this run directory then copied to Assets/Editor/PracticeAcceptance only after explicit Editor slot and verified binding. It may inspect scene objects and private metadata via reflection solely as test evidence, never become product bindings. Prefer public UISystem.TryGetWidget<T>, UILoading.UpdateProgress/SetTip, UINotice.SetContent, NoticeText.TryNormalizeUrl/TryGetLink, generated VM properties, Button.onClick.Invoke, XPluginUtility.PostEvent and XEntityManager.QueryEntity<T>. Read framework event Subscribe/Unsubscribe and lifecycle APIs before implementing observer; asynchronous event dispatch must be awaited across frames. Screen captures and visible hit tests supplement, not replace, state assertions. Cleanup removes only test material/instances; no asset saving or preference deletion.

Read-only PlayerSettings check on preparation: companyName=v0; productName=client. Persistent-action cases blocked until isolation confirmation.

## Prefab evidence policy
Run original verify prefab for Launch T1T2Start0, Login T1T1Login0, Loading T1T1T1N0, Notice T1Notice0 with explicit asset paths, preserve stdout/stderr/exit code and report.json. Root pages plus delivered list/subpage resources are separately enumerated. Preserve overall incomplete even if bindings pass; supplementary runtime evidence maps to individual unverified entries and cannot prove unsupported asset identities or full animation curves by itself.

## Open preparation observations
PRE-01: current Login ChangeClick is ReportPending and LoginEntity has only pid; local server selection required by brief is absent in currently inspected source. Root confirmed this is a frozen delivery gap, not pending development. Requirement result: failed. Reproduce from source: ChangeClick invokes ReportPending, which only replaces ServerStatus; LoginEntity has no selected-server state or selection method. Expected: select a local server. Actual: pending-integration message only. Owner: Login developer / integration owner. No new UI or template baseline edits authorized to conceal the gap. Runtime execution remains incomplete.


## Executed acceptance — 2026-11-12 10:47–10:52 UTC

Verified Studio binding through gamecli_ui_unity_binding(requireVerified=true), identity file matches. Independently read productName=GameCLIPracticeTrial. Original project CLI lacks run/call; used actual bridge owner G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe explicitly targeting practice for all Editor operations. No browser/network calls or prefs writes performed. Temporary probe compiled with zero errors, ran only in Play, then stopped and deleted using AssetDatabase CLI. cleanup-compile.json and probe-cleanup.json preserve cleanup results. Product files were not edited by this reviewer. tested-final-hashes.json records delivery files after tests; original freeze snapshots reside in baseline-files.json and independent code-review hashes. No claim that a new start hash was taken before first probe.

### Final per-module conclusions

| Module | Code | Prefab original | Runtime | Overall |
|---|---|---|---|---|
| Launch | refer independent code report; Unity compile passed | incomplete, 0 differences/34 unverified | background retained passed; full startup/progress incomplete | incomplete |
| Login | failed required local selection absent | incomplete, 0/101 | VM text, Enter/Master, Announcement raycast, deactivate/reopen passed; complete listener counts incomplete | failed |
| Loading | refer independent code report; Unity compile passed | incomplete, 0/24 | 8 progress boundaries, null tip, close/reopen and new updates passed | incomplete due prefab gaps |
| Master | refer independent code report; Unity compile passed | not-applicable, code-only template | local success/invalid pid/type handling and lifecycle routing passed; Master itself unload/reload incomplete | incomplete |
| Notice | refer independent code report; Unity compile passed | incomplete, 2/55 | 3 open/close cycles, body VM, collapse, empty, link parsing/glyph hits, raycasts and animation passed | incomplete |

Raw prefab report files: prefab-Launch.json, prefab-Login.json, prefab-Loading.json, prefab-Notice.json, with corresponding stdout and exit files (all exit 2). Never reinterpret original overall incomplete as passed. Binding subreports are only static evidence; Launch has zero VM widgets and its VM binding check is not-applicable.

Runtime-probe.log retains every original assertion and PROBE_COMPLETED. Reopen-probe.log independently proves Login SystemDetive closes page, SystemActive reopens, Enter changes MasterEntity pid from probe value 42 to 1, Announcement remains functional; Loading opens/updates/closes twice. These assertions do not count callbacks or prove all old listeners are removed.

Visual-probe.log proves EventSystem.RaycastAll first hit Announcement and Close, and ExecuteEvents pointerClickHandler on Announcement opened Notice. Two independently generated hyperlink glyph regions resolve URLs using TryGetLink; no browser dispatch was invoked. Notice Panel local scale samples move 0.9563→0.9688→…→0.9999→1 within the 0.22s opening interval, while Animator/Content local scales stay 1. Standard animation curves target Panel only (NoticeOpen.anim), not the root overlay. The first exploratory visual probe sampled Animator/Content only; final saved probe correctly samples Panel and uses four decimals.

Screenshots acceptance-login-stable.png and acceptance-notice.png were viewed by this reviewer and show actual pages at 933x481. First immediate screenshot was solid cyan: stable repeated capture displayed UI, consistent with the CLI's immediate focused GameView capture before repaint. No product rendering defect inferred from that transient image. Screenshot is visual evidence; raycast evidence is separate.

Launch private reference readback in launch-runtime-refs.json: loading→Loading, update→Update, loadingText→DownloadTips, tip→Tip, downloadText→DownloadTips, fill→Fill RectTransform, handle→Handle RectTransform. Optional loadingPercent/downloadPercent are null, consistent with combined-label implementation. Root's disabled legacy component is not counted as the active template component. Background retained runtime assertion and Login screenshot pass; full foreground timing and progress behavior remain incomplete.

Notice differences: MainText expected exact UnityEngine.UI.Text count 1 but exporter reports 0 because actual NoticeText subclasses Text. Independent source, cast to Text, live text updates and glyph hit evidence support the intended text/link behavior; original diff remains. Empty is a real additional node not in frozen design, retained as a design delta requiring owner disposition, not silently ignored. Source hierarchy has Panel sibling before fullscreen Image mask, making mask draw on top; source template itself carries this hierarchy. No change made; visual darkening is retained and not classified as an implementation deviation without source-approved design change.

Remaining incomplete: complete listener subscription counts after close, Master unload/reactivate, in-flight asynchronous cancellation, reminder persistence, explicit scroll/drag/nonlink/clipped-link negatives, all unsupported prefab resource identity/font/custom script/animation exporter details, complete Launch foreground startup/progress. Nothing in the passing test subset closes these gaps. Server selection remains confirmed failed per brief. No new UI, baseline or tool policy changes were made to obtain a pass.

### Launch core interface supplemental execution — 10:53 UTC

launch-probe.log supersedes the earlier incomplete core progress/interface coverage. Probe obtains XApp's actual private serialized launcher, verifies enabled TemplateLaunch (not legacy), exercises ShowLoading and progress 0/0.375/1 and reads actual combined Text values 0/38/100 percent. SetTip changes actual Text. ShowUpdate switches loading=false/update=true; progress 0.375 produces download text 38%, fill.anchorMax.x=0.375 and both handle anchor x=0.375. ShowBackground restores active root and both panels inactive. All 11 assertions passed. Launch core display interfaces and background behavior: passed; automatic startup frame-by-frame timing remains outside the executed subset. Overall Launch remains incomplete because the original prefab evidence gaps are unresolved.

LaunchAcceptanceProbe.cs is retained only as evidence here. Temporary Assets/Editor/PracticeAcceptance deleted again via CLI, Play stopped, cleanup compilation passed. launch-probe-errors.json records zero runtime errors. No delivery asset or source edits, no persistence writes.
