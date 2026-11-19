# Practice rebuild — independent acceptance plan

Status: **not executed / incomplete**. This is a checklist, not acceptance evidence.

## Frozen inputs and scope

- Studio project: `p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484`, snapshot `project.json`, revision **3**.
- Expected Unity identity: `44f4f783-645c-41e7-80fe-aead8477c978`; validate live binding and ProjectSettings/GameCLIProject.json before tests.
- Project layout references: SvStart v6 (Launch), SvLogin v7 (Login), SvNotice v4, SvLoading v15, MasterSystem v6.
- New source/companion snapshots: SvStart **v7**, SvLogin **v8**, SvNotice v4, SvLoading v15, MasterSystem v6. Preserve project layouts/relations; document adaptations rather than replacing the frozen project design.
- Explicit project relation: Login/Announcement opens Notice. Notice Close hides itself; TitleGreen/TitleArrow and ImgFrame/WeekNomoreBtn toggle their internal targets. Most other buttons have `action: none`; roots have no startupComplete relation. This is not evidence that undefined product destinations should be invented.
- Login v8 declares UILogin and an independent UIServerSelect prefab, two inline Fancy cells, and SyncFancyGridScrollView/SyncFancyGridClick registrations. Main-page generation alone is insufficient.
- This is a local test catalog and local login event, not server authentication. No invented Home/network flow. Record unspecified business entries separately; missing requirements remain incomplete, not passed.

## Admission and evidence rules

1. Developer freezes delivered source/resource hashes, paths, interfaces, mappings from template paths, and approved adaptations. Confirm no implementation edits during each test run.
2. Save live verified project/Unity binding, Unity version, git revision, template versions, beginning/end hashes.
3. Serialize all operations on the same Editor; coordinate ownership with developer. No resource/code/template edits during acceptance. Return defects for repair, then freeze and repeat affected tests.
4. Run Unity compile and Console error checks; preserve unmodified JSON, exit codes, timestamps. Archive evidence under this run, not solely Library.
5. Run verify code for Login, Notice, Loading, Master; review shared plugin/events/RuntimeSystem/StorageKey and XApp/XLauncher separately. Run verify prefab for all four UI roots with explicit actual paths. Preserve differences, unverified and bindings independently.
6. A code or binding pass is not runtime or whole-system success. Assets copied with matching hashes are not integration proof. Missing-path mappings must be reviewed explicitly.
7. Runtime evidence must identify input, expectation, actual visible/component state, and screenshot/log/object snapshot. Real pointer/raycast checks are required for UI clicks; invoking handlers alone is insufficient.
8. Do not clear existing storage. Capture/restore only test-owned values. No real services, browser links or persistent resource changes without the relevant task authorization.

## Launch

- Actual XApp serialized launcher references current CLI-authored Launch instance, not disabled baseline; prefab and scene bindings separately verified.
- Loading/update panels, progress text/fill/handle and hints: 0, 0.375, 1 and invalid/out-of-range values, with actual component readback.
- Show/Hide/ShowBackground switch correctly; loading foreground is absent once Login ready, background remains, no blocked input or duplicate foreground.
- Canvas reference resolution/scaling/order and stretch match project. Launch animation actually plays with correct target identity.
- No fake automatic progress/network calls; distinguish explicit test input from startup sequence evidence.

## Login and ServerSelect (mandatory)

- Login active lifecycle opens asynchronously; inactive request rejected. Main page metadata matches current VM order and actual components. Stop/reopen and stop-during-load leave no stale page/listeners.
- **Click Change in actual Login UI → independent ServerSelect opens**. ReportPending/default server label is a failure, not an acceptable substitute.
- Verify ServerSelect prefab, UIServerSelect/VMServerSelect, UIServerGroupItem/VMServerGroupItem, UIServerItem/VMServerItem; both Fancy sync registrations exist before open.
- Recent/recommended/numbered groups populate from declared catalog. Switch groups; scroll enough to recycle cells; each visible label/state and click still maps to the displayed server.
- Choose a different server; selection writes LoginEntity through its method, closes selector, updates actual Login server name/status. Maintenance selection follows declared policy and disables/rejects Enter; normal/busy server behavior matches contract.
- Restart/re-enter restores persisted selection and recent server behavior; absent/stale IDs use declared fallback. Isolate test keys and restore prior values.
- Validate SetServers rejects empty/duplicate/invalid state atomically and preserves prior valid data; no unintended network request.
- Selector close/back and Login OnDetive close all owned pages. Reopen has one listener and correct selected state; show animation and masking work.
- Start game real click produces exactly the local event/payload expected by Master, obeys maintenance/CanEnter gates, and handles repeated clicks without duplicate flow.
- Announcement routes to Notice through project relation. Other no-action buttons remain visually correct/interactable and are individually catalogued as unimplemented/unspecified where appropriate; do not mark full functional completion based on absence of an action mapping.
- Fixed labels resolve through approved CommonText aliases; no raw keys/blank final exported row. Resource path adaptation from SVLogin to Login is documented and verified.

## Notice

- Open via actual Login Announcement; close via own Close and system lifecycle, then reopen repeatedly. No leaked instances/callbacks or input blocker.
- Title/body, long/multiline/empty data render correctly. Empty state binding requires actual node/evidence; do not silently edit frozen design.
- TitleGreen/TitleArrow fold/unfold correct internal body. Height, scroll limits and RectMask2D follow actual content.
- Legal link spans hit in one/multiple lines; invalid schemes, plain text, clipped text and drag gestures do not activate. Actual external navigation is separately scoped.
- WeekNomore controls toggle correct internal target; persistence/expiry/manual-entry behavior only per declared product contract.
- Opening animation controller/state and samples match supplied scale curve; only content panel scales, full screen mask does not. Mask remains below content but blocks underlying Login.

## Loading

- Async open then update; actual text/fill for -1, 0, 0.375, 1, 2, NaN, +/-Infinity according to declared normalization (reference 0/0/38/100/100/0/0/100 percent).
- Prompt replacement/null/empty, consecutive updates, close/reopen and lifecycle cancellation are safe.
- Progress does not advance itself; 100% does not invent automatic navigation. Ball rotation loops and actual images/decoding/rendering match design.

## Master and shared integration

- Real PracticePlugin initializes required entities and Master exactly once. Test startup event order and actual Login open; do not call handlers directly as sole proof.
- SystemActive/SystemDetive valid types and callbacks; duplicate requests and invalid/null/self/abstract types do not corrupt state or fire false completions.
- Local LoginSucceed PID and receiver entity update verified; no Home/online authentication added to satisfy example dependencies.
- Master stop removes subscriptions; reactivate handles each event once. Distinguish system active from async UI ready.
- Master has no prefab/VM; those checks are not-applicable with rationale, not fake UI tests.

## Cross-system and completion gate

Test fresh Play → Launch → Login ready → Change → select another server → Login updated → Announcement → Notice fold/scroll/close → Login still usable → local Enter event; stop/re-enter and verify storage/lifecycle. Loading is tested through explicit caller, not inserted into undocumented project relations.

Each case records passed/failed/incomplete/not-applicable, evidence, delivery hash and reviewer. Any confirmed defect → **return for repair**. Static pass without sufficient runtime evidence → **awaiting acceptance**. Only all applicable required runtime cases with evidence → **complete**. The generic six-category acceptance JSON must summarize these cases, not replace them. An evidence path string alone is not proof. Run report fingerprints must be current; report tooling gaps or false positives without falsifying input/hash or rewriting original verdicts.

Final handoff includes per-system reports, cross-system report, unmodified tool outputs, exact remaining defects, cleanup of temporary test objects/probes, and final Editor/Play state. No Git commit/push or standard-template publication is authorized by acceptance.
