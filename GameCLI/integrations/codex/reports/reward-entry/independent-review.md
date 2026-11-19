# Independent review: RewardEntry extraction

Date: 2026-11-18. Reviewer: independent mail_acceptance agent. Scope: read-only source, packaging and recorded evidence; reviewer did not operate Unity or independently query the database.

## Decision

No blocking business-code defect found in the extraction. Full acceptance remains **incomplete**. The current automated Mail prefab report is explicitly **failed**, not passed: 92 differences and 176 unverified entries. Do not replace that raw result with a success claim.

The failure pattern is consistent with the exporter treating the nested RewardEntry root as PrefabInstance without expanding its 87 descendants: one kind difference, 87 missing nodes and four binding-reference-invalid differences whose nodeId/componentNodeId are null. Native Metadata validation and actual runtime cell checks provide separate positive evidence, but do not make this automated comparison pass. Exporter support for nested prefab comparison remains an outstanding tool issue.

## Reviewed implementation

- UIRewardEntry and VMRewardEntry are in Systems/Common/UI and Common/UI/VM. Their only responsibility is presentation; no Mail, inventory, entity, persistence or network dependency was found.
- VM preserves the four-field contract: Sprite icon/SyncSprite, string count/SyncText, bool claimed/SyncVisible, bool iconVisible/SyncVisible. UI uses the existing FancyGridScrollViewCell and Metadata; no duplicate synchronization action was introduced.
- Mail creates VList entries of VMRewardEntry, explicitly starts IconVisible=false and owns asynchronous loading. Callbacks reject inactive systems, old widgets and entries no longer present in the current reward list before assigning Icon and visibility. Claim rules remain in MailEntity.
- Existing VMItemSlot/VMGridItemSlot have a different eight-field contract used by older pages; replacing those types or rewriting their contract would introduce unrelated compatibility risk. The separate four-field shared presentation VM is justified.
- Independently compared every packaged Assets/Scripts file in reward.json and mail.json with the current local source: zero mismatches (7 and 28 total package files respectively).
- Development and acceptance skills describe standalone Ref/Unref, host ownership, field ordering, initialization, asynchronous boundaries, exact dependency version and migration with retained GUIDs.

## Recorded evidence reviewed

- metadata.json: VMRewardEntry, expected/actual script GUID match, four fields, zero missing references/issues.
- mail-metadata.json: three Metadata components, zero missing references/issues.
- standalone.json and archived RewardEntryProbe.cs.txt: Check=True; actual Image/Text/activeSelf validated for initial binding, count update, claimed toggle, icon hide/restore and Unref, without requiring a Mail page.
- mail-cell.json: CheckMail=True for the actual nested reward cell; separate from standalone evidence.
- mail-flow.json: empty inbox after local claim/delete flow, zero cells/rewards, two claim receipts.
- console.json: zero error entries at the recorded time. This is not a proof that every possible interaction is error-free.
- dependencies.json: 53 referenced project assets, no missing assets, no Mail-private dependencies, 108 material records.
- verify-mail-code.json: passed, no issues. verify-mail-prefab.json: failed as detailed above.
- publication.json: reported RewardEntry v1, SvMail v5 and GameCLIApp revision 61, verified=true. Packaging code performs database readback checks; reviewer inspected the publication procedure and local candidates, not a fresh database query.
- Latest temporary reward probe has been archived with CheckMail included and removed from its Unity source path during review.

## Not covered

No independent Unity execution, clean-project reinstall from the two database packages, forced late icon callback timing, long-list recycling stress, rapid close/reopen, frame-by-frame animation comparison, browser/native visual equivalence or physical mouse test was performed by this reviewer. Existing Fancy binding-cache limitations remain outside this extraction. Successful local flow and dependency checks must not be described as full acceptance of those cases.
