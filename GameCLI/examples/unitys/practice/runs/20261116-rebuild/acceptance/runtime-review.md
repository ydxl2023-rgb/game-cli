# Independent runtime re-review — 2026-11-16

Overall: **incomplete / awaiting remaining evidence**, not complete. The confirmed first-round ServerSelect importer defects were repaired by development; no repeat MissingReferenceException or Sprite load error appeared during repeated selector/category operations. Final error evidence is preserved; no business source or asset was changed by this reviewer.

## Observed passes (bounded to these cases)

- Login/ServerSelect: Login opened after Play; Change callback opens independent selector; four groups show 1/2/10/2 rows. Group callbacks recycle/refresh without errors. Choosing 11 updates entity and Login label, closes selector. A newly constructed LoginEntity reads persisted 11. Choosing maintenance server **3** makes both VM CanEnter and actual Enter Button.interactable false. Selection was restored to original 1. Evidence: third-probe.log. The earlier second-probe.log selected normal server 12 by mistake; its maintenance failure is a test-input error, not a product failure, and is preserved.
- ServerSelect artwork restored and visually inspected in serverselect-restored.png. Source importer failure no longer present. No physical OS click test is claimed; probe invokes bound callbacks.
- Login main prefab comparison against project v3: zero differences, bindings checked in login-prefab.json; tool remains incomplete because Animator/Metadata/Sprite/font object reference export coverage is incomplete. Original verdict is preserved.
- Notice: three open/content/collapse/empty/close cycles pass. URL whitelist accepts http/https and rejects javascript/file/relative/empty. Actual generated glyph link hit count 2. Announcement and Close EventSystem first raycast hits correct buttons; Announcement uses ExecuteEvents click. No external URL opened. Panel scale sampled from .9527 to 1 over opening while Animator/Content remain 1. Evidence: runtime-probe.log and visual-probe.log.
- Loading: async callback returns widget; actual Text normalization for -1,0,.375,1,2,NaN,-Infinity,+Infinity is 0,0,38,100,100,0,0,100%; null tip clears; close and two reopen cycles show new text and clean close. Evidence: runtime-probe.log, reopen-probe.log.
- Master: real Login Enter binding produces pid1; invalid pid leaves pid1; null/string/self activation requests preserve Login. Login stop/reopen works, Enter routes after reopen; no error. Evidence: runtime-probe.log, reopen-probe.log. This does not prove all Master exit/reentrancy semantics.
- Launch: explicit ShowLoading + .375 yields Review (38%); ShowUpdate + .375 yields Review 38%; ShowBackground keeps root active with zero active text components. Evidence: launch-probe.log. These are explicit test inputs, not download/network evidence.

## Remaining executable checks

1. Independently reconcile 71 adapted inventory paths against project v3 and companion template. For main PNGs validate source bytes; meta Sprite/slicing settings and GUID binding require inspector/export evidence. Review three reference scopes and five adapted code files before approving corresponding review entries. Do not bulk approve.
2. Login: actual pointer/raycast selector close/category/cell interactions, recycled off-screen row correspondence, full Editor restart persistence, invalid catalog SetServers atomic preservation, stop during async open, other undefined/no-action entrances. Current test covers callback and new-entity restoration only.
3. Loading: actual ball rotation samples, image/visual comparison and async close-during-load. Launch: full boundary input/fill/handle anchors, animation sampling and actual startup stage timing. Master: stop/re-activate subscriptions, duplicate callbacks and delayed requests. None are replaced by successful compilation.
4. Notice: multi-line/long/clipped link bounds and drag-to-scroll non-activation, actual scroll/mask limits, preference expiry semantics if required, close during load. Existing valid URL/hit test is not all edge cases.
5. Full main-prefab export's unsupported Sprite/font/Animator/Metadata diagnostics need supplemental evidence; zero differences alone is not whole-result pass.

Per-system overall: Launch incomplete; Login incomplete; Notice incomplete; Loading incomplete; Master incomplete. No known reproducible runtime defect remains in exercised paths after repair. Static integration inventory is still repair/review until legitimate mappings and individual adapted references are reviewed. No all-passed acceptance JSON was issued.

## Cleanup

Temporary PracticeRebuildReview, PracticeAcceptanceProbe and PracticeTrialIntegration were archived here and deleted using UnityCLI AssetDatabase. Ended in Edit Mode, requested compilation and captured final Console. No scene/prefab saved from runtime testing. Shared Unity Editor operation ownership returned to root after cleanup.

## Supplemental bounded runtime acceptance

A subsequent independent round completed the remaining **local trial** cases in `deep-runtime-retest.log`; it supersedes the earlier broad incomplete assessment for these explicitly tested behaviors, not the raw generic prefab tool verdict.

- Login: EventSystem first-hit and pointerClick for Change, group, recycled server10 row, selector Close all pass. Correct row10 entity/name mapping; null/empty/duplicate/invalid-state catalogs reject atomically. Login GameLoginViewFx1 animation changes over .17 seconds. Server3 maintenance gating, persistence through PlayerPrefs.Save + new entity, reopens and original selection restoration covered by prior evidence. Source/companions adapted for local trial independently reviewed; `login-acceptance.json` contains 74 path/reference decisions and six evidence-backed categories.
- Loading: ball rotates approximately30 degrees in .15 seconds; previous eight numeric boundaries, text updates and reopen/close tests passed.
- Launch: both loading and download panels exercised with eight boundaries each; correct percentage, fill/handle x anchors and preserved y anchors. Background mode leaves no foreground Text active. These direct test inputs do not certify real downloads or a time-driven startup service.
- Master: stopped coordinator ignores events; after awaiting deferred removal, two unload/reload cycles each handle duplicate activation requests once; pending request made before coordinator stop is discarded. Same-frame direct Unload/Load is **not supported by current deferred dispatcher contract**; first deep log preserves that observation. The later wait-for-removal test is not claimed as same-frame success.
- Notice: long text content height2069 exceeds viewport705, visible glyph hits1034 and clipped glyph rejects1786; scroll bottom within1e-4 tolerance; dragging=true guard prevents navigation before URL resolution; future suppression restores and expiration comparison becomes false. Prior animation, link/raycast, fold/empty/open/close tests passed. Preference key and entity expiration restored.

`deep-runtime.log` failures for scroll -4.4e-8 versus strict Mathf.Approximately(0), and same-frame reactivation assumptions were test-contract/tolerance mismatches; preserved as historical evidence, corrected in `deep-runtime-retest.log`. No implementation changes made by reviewer.

Bounded module verdicts: **Login passed for this offline integration scope; Loading, Launch, Notice and Master exercised functional cases passed**. This is not exhaustive cross-platform or production service certification. `action:none` entries with no product contract are not applicable to this trial, not falsely implemented. Physical OS input and full Editor restart are not prerequisites for this scoped programmatic verdict and were not claimed. Generic Login prefab exporter remains **incomplete**, zero layout differences, due to unsupported Animator/Metadata/Sprite/font diagnostics; its raw JSON was not changed.

Deep probe archived as PracticeDeepReview.cs, removed using AssetDatabase, compilation completed and final Console error0 in deep-final-errors.json. Final state is Edit Mode; no transient test module remains after Play stopped. Editor operation ownership returned to root.
