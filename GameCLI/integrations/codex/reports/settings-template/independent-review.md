# Settings independent review

Scope: SV five-tab reference, local preference persistence, product service events, native UI/animation adaptation and the narrowly related Master pending-activation cancellation fix. No implementation or Unity-state changes were made by this reviewer.

## Outcome
No remaining blocker found in the reviewed code and final screenshots. Publication has now completed; strict prefab comparison failed and is not certified as passing. See the publication addendum below.

## Independent work performed
- Read Settings Entity/System/UI/VM and development rules. Entity owns validated private preference data and read-only snapshots; System orchestrates refresh and events; UI owns tab/animation presentation. Service actions do not call SDK, RPC, URLs, account deletion or actual downloads. Local saved preferences do not imply actual product audio/rendering/push adapters are implemented.
- Found the unsupported Model.Medium preset bug and reviewed its fix: medium preset maps Model to Low, reverse preset detection uses the same mapping, invalid direct/persisted Model.Medium is rejected.
- Traced white hit-area and slider geometry errors to source-adaptation omissions; Image alpha and native RectTransform restoration are now documented and implemented.
- Independently viewed current screen-final, sound-final, function-final, function-scrolled, privacy-final and download-final screenshots. Confirmed white hit-area coverage and oversized sliders are gone. Reported and visually rechecked function viewport overflow and privacy regional-control overlap; both fixed. Bottom function entries are visible after scrolling while remaining within the viewport.
- Reviewed Master request-identity cancellation: same-frame deactivation invalidates pending activation; stale requests cannot consume a newer request. Repeated same-type activations now use latest-request-wins semantics, not completion of all duplicate requests.

## Developer-executed evidence reviewed
metadata-final/code-final/entity-final, per-tab click/state reports, sliders-final, signals-final, scroll-final, animations-final, reopen-animations, closed-final, rapid-closed and console-final support binding, persistence, interaction, local-event, animation and lifecycle claims. rapid-closed is True and Console reports zero errors. These are implementing-agent runs, not independent reviewer-driven Unity execution. Click probes dispatch UGUI events rather than physical mouse input. Final screenshots use actual ScreenCapture after stale EditorWindow capture was diagnosed.

## Limits
No certification of all animation curves or pixel-perfect SV identity; source particle/mesh adaptations remain documented boundaries. Service/payment/account/platform functionality is intentionally only an event contract. Product startup must apply restored preferences through its own adapters. Strict verify prefab and final DB/source/material readback require their own results after publication; this review does not infer those passes.


## Final publication addendum
Read publication-readback.json and the final comparator/result reports. SvSettings v1 is published with 25 script-package files, 97 materials and one configuration row; the readback records verified hashes and GameCLIApp revision69, instance T1Settings0. This is publication/readback evidence, not proof of strict prefab equivalence.

prefab.json explicitly reports status=failed and passed=false, with 45 differences and unsupported coverage. Differences include the comparator's Button-only raycast expectation versus retained hit areas, root-wrapper anchors, RawImage-to-Image adaptation, disabled mask graphics and numeric drift. These explanations do not change the failed result or prove every difference harmless. result.json correctly limits the outcome to a reference validated with explicit limits. Do not describe this delivery as fully passing strict prefab acceptance. Source/binding/interaction and screenshot checks remain separately documented above.
