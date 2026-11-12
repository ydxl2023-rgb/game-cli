# Independent code acceptance — 2026-11-12

Reviewer: trial_code_acceptance, independent from implementation. Scope: read-only C# and delivery-contract review; no Editor commands, compilation, prefab generation or runtime tests performed.

Identity: p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484 / 44f4f783-645c-41e7-80fe-aead8477c978. Identity file matches CLI reports. Design: brief.md revision 3 / project-final-design.json; SvStart v6, SvLogin v7, SvNotice v4, SvLoading v15, MasterSystem v6.

## Results

| Module | Automated code | Independent conclusion | Evidence / outstanding |
|---|---|---|---|
| Launch | incomplete, exit 2 | incomplete | No Systems/Launch scope. Manually inspected LocalPackages/com.oathx.core/Runtime/XLauncher.cs and XApp launcher integration. Private serialized references, separate Hide/ShowBackground, clamp/NaN progress exist. Scene references, startup visibility and background transition untested. |
| Login | passed, exit 0 | failed | L01 and L02 below; source-level local LoginSucceed and Notice routing exist. |
| Loading | passed, exit 0 | incomplete | UILoading/VMLoading only; no invented loading task/system. UpdateProgress clamps/handles NaN and checks VM; SetTip clears null. Open/close callback integration and actual text display remain untested. |
| Master | passed, exit 0 | incomplete | Three event contracts, deferred system routing and positive pid recording reviewed; no Home dependency. Event subscription cleanup present. Runtime dispatch, reactivation and persistence of lifecycle state untested. Prefab not-applicable: pure code template. |
| Notice | passed, exit 0; recheck exit 0 | incomplete | Content/empty/expand/reminder APIs and HTTP(S)-only hyperlink gate reviewed. Runtime hit geometry, drag exclusion, masks, animation and reopen listener cleanup untested. |

## Confirmed defects

L01 — LoginSystem.cs:29 binds ChangeClick to ReportPending; LoginEntity.cs contains only pid. No local-server selection/data operation exists. brief explicitly requires selecting a local server. Reproduction: open Login and click Change; expected local selection, implementation invokes pending entry handler. This is a business failure despite automated syntax pass. Return to Login developer.

L02 — LoginSystem.cs ReportPending uses GetWidget<UILogin>, while UILogin.cs Config sets cacheWidget:false. The actual UISystem.cs:716 implementation indexes the cache dictionary; TryGetWidget at :726 searches the open stack. Thus pending entry feedback cannot reliably find this noncached page (and may throw on absent dictionary key). Expected pending message for unsupported entries. Return to Login developer; root notified. A subsequent change requires recheck and new hashes.

## Scope and additional review

Reviewed all files listed in code-hashes-start.json (Assets/Scripts), including PracticePlugin, CommonText, StorageKey and RuntimeSystem; supplementary Launch/package and design hashes are in code-additional-hashes.json. PracticePlugin explicitly initializes three entities and requests Login after loading Master; shutdown unloads systems before UI/entities. RuntimeSystem defers dispatch by one frame and checks current active state. StorageKey rejects non-direct/non-public/static selectors without compiling expressions and builds lowercase keys. CommonText loads the exported Resources JSON and resolves generated IDs; export provenance, whole-prefab localization and generation correctness are not independently certified. Source JSON currently has local server/pending/empty strings 1101–1103.

Launch is package code outside automated scope; this manual reading does not claim full package convention certification. Default startup strings are still literal English in XLauncher; source-language table coverage for startup remains incomplete. Static async open guards and explicit widget callbacks were reviewed; close-before-load-completion behavior needs runtime evidence from UISystem.

Notice source changed during review: UINotice, VMNotice and CommonTextIds. Original Notice CLI evidence is invalid for updated files; code-Notice-recheck.json records the new scope with exit 0. EmptyText binding and CommonText 1103 were added. The integration owner reports an added Empty design node; this code-only review has not inspected its approved design mapping or prefab, so design preservation and binding remain incomplete, not passed.

## Evidence and freezing

Raw files: code-{Login,Loading,Master,Notice,Launch}.json and matching stdout; code-exit-codes.txt. Launch's missing-scope result is intentionally preserved. Notice recheck raw JSON/stdout also preserved. Start/end hashes and code-source-changes.json explicitly record invalidated evidence. Stable initial scopes are Login, Loading, Master and root infrastructure at this snapshot. Further edits invalidate the affected conclusions until rechecked. No code, baselines, templates, verifier or resources were repaired by this reviewer.

Overall: failed due to confirmed Login defects; Unity compilation, Console, all applicable prefab/static mapping, visuals/animation and runtime cases remain incomplete. This report is not an end-to-end acceptance pass.
