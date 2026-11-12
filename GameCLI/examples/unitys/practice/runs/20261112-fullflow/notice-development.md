# Notice development handoff

Source: SvNotice v4 snapshot in templates/SvNotice.json. Target: practice only, project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, root T1Notice0.

Prepared six C# files under Assets/Scripts/Systems/Notice. UI/UINotice, VM/VMNotice; NoticeSystem, NoticeEntity, NoticeText, NoticeLayout at module root. Original script GUIDs preserved for the five template scripts. VM field order unchanged (11 fields).

Adaptations: local English test announcement explicitly labelled Practice Trial; no official announcement or network request. Reminder persistence belongs to NoticeEntity and uses shared StorageKey.For. Public methods precede protected/private implementation; explicit VM guard in open callback. Close posts SystemDetive through Master; OnDetive closes a local Type[] widgets array. Hyperlink parser only accepts HTTP/HTTPS, excludes drag gestures and uses generated glyph bounds plus masks.

Root integration requirements: create NoticeEntity through XEntityManager before first Notice opening; provide shared StorageKey helper; install/register NoticeSystem and Master events using RuntimeSystem. Root owns these shared changes. UINotice.SetContent(title,text) supports empty state; Initialize sets actions; IsReminderSuppressed queries entity. Manual opening remains possible despite suppression.

Resource work NOT complete: restore/generate Assets/Data/Widgets/Notice/Notice.prefab via approved CLI. Add NoticeText in place of the body's Text (it inherits Text and works with SyncText), keep raycastTarget enabled for hyperlinks. Attach NoticeLayout and serialize body, expanded, entry, entries references. Generate Metadata for UINotice/VMNotice with dry-run first; verify all 11 fields, allowing NoticeText as Text subclass. Apply dialog.open.scale to content container, never full-screen mask. Root-generated inputs/Notice are generation inputs, not independent evidence of fidelity. Original metadata target mapping should guide ambiguous field names.

No Editor/Unity commands executed during code preparation. Code verifier run is independent of Editor; latest result reported to coordinator. Unity compilation, generated VM, prefab bindings, runtime links/scroll/close/reopen/reminder and animation still require serialized editor slot and independent acceptance. No development or database template modifications, commit or push.

## Resource completion (2026-11-12)

Editor slot released to root after resource preparation. `ugui` dry-run and generation succeeded (33 nodes / 15 images), but automatic Metadata dry-run exposed missing field-name mapping and Text conversion. Rebuilt through approved `prefab restore` from exact database-derived snapshot; runtime variant adds one Empty text node because SvNotice v4 has VM.empty but no empty-state layout node. Frozen design was not edited: acceptance must retain this extra-node difference and template-gap finding.

Final prefab: Assets/Data/Widgets/Notice/Notice.prefab, 34 nodes. NoticeText replaces body Text; font size restored to source 35, raycast on for links. NoticeLayout serializes body/expanded/entry/entries. ScrollRect and RectMask2D added. Explicit Metadata fields preserve VM order and map actual authored nodes; static bindings pass (1 widget / 11 fields).

Animation: NoticeOpen.clip.json is lossless translation of DB dialog.open.scale v1 curves (in/out slopes and weights preserved). CLI animator import-json dry-run and import succeeded, Controller Open state added, controller bound to Notice/Animator. Curves target Panel only, duration 0.22, scale 0.94 to 1. Mask stays outside scale. Texture importer borders configured through CLI and four Image components switched to Sliced.

Evidence: inputs/Notice/notice-cli.jsonl records component/Metadata/animation CLI results. inputs/Notice/Notice.runtime.snapshot.json records runtime extra Empty node; it predates latest image-border update, so rerunning generation also requires latest Notice.snapshot visualTextures configuration and four sliced Image settings. Verification report: client/Library/GameCLI/Verify/23ddceb1-10a3-48db-91b5-8dae4daefc36/report.json; binding subreport passed, overall incomplete retained. Code verification passed with no issues; Console returned zero error entries. No Play runtime self-test yet because Login prefab was not ready and root requested slot release.

Remaining integration: Empty fixed label currently `No announcements`; root must add approved CommonText ID and bind/localize it (not silently hardcode permanent product text). Independent runtime checks still needed for actual font rendering, scrolling, hyperlink hit regions, close/reopen/reminder and visual animation. No claim of overall acceptance.

## Empty-state localization follow-up
Appended VMNotice.emptyText as binding index 11 (12th field), preserving the original 11 field indices. UINotice.SetContent assigns CommonText.Get(Config.Excel.NOTICE_EMPTY). Only two source files changed; hashes in notice-emptytext-hashes.json. No Editor operation performed: serialized Metadata must be extended to 12 entries; index 11 name emptyText / SyncText / Notice/Animator/Panel/Empty Text. Previous 11-field static binding evidence predates this follow-up and is no longer current for acceptance.

## Empty-state Metadata completed
Appended index 11 through seven UnityCLI modify calls; original 11 entries preserved, no prefab/animation regeneration. Raw calls: inputs/Notice/emptytext-cli.jsonl. Fresh verify prefab evidence: client/Library/GameCLI/Verify/97b62121-39eb-4fdc-8400-86864ebffe0b/report.json. bindings passed: 1 Widget, 12 fields, zero differences/unverified. Overall remains incomplete for other evidence limits. Editor slot released immediately after this check.
