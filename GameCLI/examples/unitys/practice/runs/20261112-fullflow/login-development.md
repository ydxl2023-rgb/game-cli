# Login source preparation

Source: database SvLogin v7 snapshot (8 files, four C# scripts), not development business files. Target Practice project identity and paths are recorded in brief.md. No Editor operations performed by this developer yet.

Implemented source adaptation: LoginEntity removes LoginResponse/network dependency and explicitly identifies local test player 1. LoginSystem opens asynchronously with Widget/VM/entity checks, closes through widgets array, emits shared LoginSucceed and SystemActive events. Master owns lifecycle and shared events. UILogin is retained; VMLogin moved to Login/VM with original GUID and original seven binding indices unchanged, then ten existing-button callbacks appended. Pending integrations display a shared CommonText message rather than disable buttons or fabricate services. Friends remains visible to preserve template design.

Root integration requirements: initialize XEntityManager so [Initialize] LoginEntity exists; activate Login through Master. Provide CommonText.Get(Config.Excel.LOGIN_SERVER_DEFAULT) and CommonText.Get(Config.Excel.LOGIN_INTEGRATION_PENDING). Config prefab path remains Assets/Data/Widgets/SVLogin/SVLogin.prefab. No real authentication, network, Home or server selector was added.

Template delivery defects: SvLogin v7 contains old LoginResponse dependency, obsolete generic CloseWidget, no server-selector source/assets and no announcement/change bindings. Full server selection cannot be claimed. This trial adapts required Notice entry and logs missing selector as a delivery gap; do not replace it with an undisclosed development copy.

Metadata order and intended targets relative to Login root:

1. enterClick -> Animator/Content/Enter Button (SyncClick)
2. canEnter -> same Button (SyncInteractable)
3. serverName -> Animator/Content/Server/Name Text
4. serverStatus -> Animator/Content/Server/Status/Status Text
5. versionText -> ButtonGroup/Version Text
6. username -> actual template username Text (resolve exact node from generated design before binding)
7. friends -> Animator/Content/Friends RectTransform
8. announcementClick -> ButtonGroup/LayoutGroup/Announcement Button
9. changeClick -> Animator/Content/Server/Change Button
10. friendClick -> Animator/Content/Friends/FriendButton Button
11–17. userCenterClick, cancellationClick, clearClick, legalClick, noticeClick, ageIconBtnClick, exitClick -> corresponding uniquely named template Button components. Resolve generated hierarchy paths, never runtime Find.

Auto Metadata name heuristics may be ambiguous because Change/Version have duplicate object names and VM field names differ from template targets. Root must bind actual exported references through UnityCLI, preserve field order, and verify each target. Source preparation is not resource generation or runtime acceptance.

Remaining: root coordinated resource generation and Metadata; compile and local open/close/Notice/login event tests; independent acceptance. No commits, database template updates or development changes.

## Resource implementation and static self-check

Completed under an exclusive Editor slot: UGUI dry-run and generation (67 nodes, 21 images), exact snapshot restore dry-run and application, 17 Metadata fields through formal CLI component/serialized-property commands, and database looping animation import/controller binding. The generator dry-run bound 12/17 fields; explicit mappings resolved five naming ambiguities without changing design node names. Raw outputs and repeatable scripts are in inputs/Login/login-cli.jsonl, ugui-result.json, restore-result.json, metadata-dry.json, finish-login.ps1 and animate-login.ps1.

Animation conversion preserves database keys; inTangent/outTangent/sourceTangentMode become import API inSlope/outSlope/tangentMode. Current prefab root becomes SVLogin when Unity saves SVLogin.prefab; original design root is Login, so this difference remains explicit rather than changing the baseline.

ReportPending was corrected to TryGetWidget because this page is uncached. Static code check passes with no issues. Prefab binding check passes all 17 fields; overall remains incomplete. Evidence: client/Library/GameCLI/Verify/af512600-5956-4583-a79e-600c6db46ccf/report.json. Console query returned zero errors. No full Play-mode test was executed while other modules awaited installation. Server selection remains a failed product requirement due to missing template delivery; do not claim it implemented.

## Path repair iteration

Moved prefab via UnityCLI asset move to Assets/Data/Widgets/Login/Login.prefab; GUID remains 1af722323688c6d49ab45a3285d651a7. Updated UILogin Config to the new path. After compilation, static verification reports zero layout differences and all 17 bindings passed. Root now matches Login. Existing Animator controller reference is preserved. Current report: client/Library/GameCLI/Verify/2e52fc88-c713-42d0-a2f2-b4a25e824e08/report.json. Overall incomplete still reflects unsupported evidence, not a passed whole-system verdict. No Play-mode operation performed. Earlier generation scripts record the original SVLogin path and are historical evidence, not current replay instructions.
