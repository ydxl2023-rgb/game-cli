# Independent business-code review

Reviewer: configuration agent, not the business-code implementer. This agent produced configuration exports; configuration correctness is explicitly excluded from independent claims and remains with the runtime reviewer. No Editor operation, implementation edit, database write, or baseline change occurred during this review.

## Automated evidence

Executed actual `gamecli verify code` with the verified Practice project ID. Inspection of verify-code.mjs confirmed file/DB reads plus local analyzer/report generation only, no Unity bridge.

- Passed syntax policy: Login, RedDot, Mail, Shop, Home, Settings, Function, Common.
- Failed: Notice (9 issues), Loading (2 issues), Master (18 issues). See unmodified system JSON/raw files and exit-codes.json.
- Launch directory scan exits 2 because Launch is XLauncher in LocalPackages, not a RuntimeSystem directory. Supplemental direct analyzer covers that file, PracticePlugin and RuntimeSystem, with actual source hashes; shared-analyzer.json reports 14 errors. This does not alter the raw missing-directory verdict.
- Failures cover NoticeText lifecycle ordering and return separation; UILoading return separation; MasterPropertyDef undocumented uppercase constants and MasterEntity member ordering/docs; XLauncher return separation; PracticePlugin member ordering; RuntimeSystem member order and empty parameter/return documentation. Raw diagnostics are authoritative; no arbitrary waiver was applied.

## Actionable manual findings for acceptance triage

1. **Master login gate ignores payload.** `MasterSystem.OnLoginSucceed` (line 239) calls Load/Launch without checking null or positive PID and never applies the event's player identity. Thus a LoginSucceed carrying pid=0 or -1 can proceed whenever local archive loading succeeds; duplicate events also schedule repeated Login unload/Home startup callbacks. Verify via the actual event bus, then return to development. Existing local-test status does not justify accepting invalid success payloads.
2. **Master exit does not own all integrated children.** `MasterSystem.Unload` (line 117) unloads Shop, Notice and Home but omits Mail and Settings. Both are now routed by Master. Stopping Master while either is open leaves the system/window running; its Close posts SystemDetive to the now-stopped coordinator. Verify stopped-coordinator close behavior and require lifecycle cleanup consistent with product ownership.
3. **Deferred requests have no activation epoch.** ActivateSystem waits a frame then tests only IsActive; a stop/reactivate before continuation can allow a request from the previous activation to run. RuntimeSystem.DoUnloadAsync invokes completion after a frame without owner-generation validation, and Login transition callback only checks current IsActive. Test queued activation and login callbacks across coordinator stop/reactivate. This is a static risk until the specific sequence is reproduced; do not mark runtime failed solely from this observation.
4. **Malformed event/type guards incomplete.** OnSystemActive/OnSystemDetive/OnFunctionChanged dereference data without null guards. IsSystemType does not reject open generic RuntimeSystem types or types lacking a public parameterless constructor, although dispatcher uses Activator.CreateInstance. Gate lookup may reject most unknown types first, so verify actual reachable contracts instead of inventing public exposure.

## Additional inspected boundaries

- LoginEntity owns catalog validation/copying, selected/recent persistence, and CanEnter. LoginSystem uses detached snapshots and delegates selection. No runtime transform lookup found in inspected page routing.
- MailEntity validates a replacement before mutation and returns copied messages/receipt snapshots; claim/delete policy lives in entity. ShopEntity copies catalogs and returned products, owns balance/pending purchase state. SettingsEntity owns private preferences, restoration validation and mutation-plus-save operations.
- UISystem.CloseWidget removes loadingMap requests; asynchronous loader completion compares request identity. Therefore absence of a second IsActive check inside individual page callbacks alone is not reported as a defect; runtime close-during-load tests must validate the framework cancellation.
- UI metadata target correctness, real input, animations, and cross-system runtime behavior remain the dedicated acceptance agent's work. Syntax pass is not full system acceptance.

All findings are sent to acceptance for a single coordinated R1 decision. This report does not independently increment repair rounds.
