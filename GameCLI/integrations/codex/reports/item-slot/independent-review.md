# Independent review: ItemSlot composition

Date: 2026-11-18. Read-only review by mail_acceptance; no Unity operation and no business edits.

## Current decision

**Lifecycle blocker resolved; overall acceptance incomplete.** Current UIRewardEntry.Unref preserves the current Item, sets Item=null while bound to release its subscription, calls base.Unref, and restores Item after the parent subscription is removed. The host-owned child model is therefore retained for recycling.

Reviewed the updated archived ItemSlotProbe.cs.txt and recycle.json (Check=True): a detached item's Text change does not change the old control, Item identity is retained, and three repeated cycles rebind the original model and an alternate model with the expected actual label and child VM. This resolves the specific data-loss defect found in the first review; it is not a long-duration recycling/cache test.

## Positive source review

- VMItemSlot retains the original eight binding positions and appends IconVisible at index 8. Icon now uses object with existing SyncSprite, which supports both string paths and Sprite; null remains unsupported.
- Public int type removes the equipment enum dependency. No old slot PartType call site was found in current application scripts; unrelated ItemConfig.PartType should remain intact.
- VMRewardEntry contains Item/SyncMetadata plus Claimed, and Mail constructs the child model and guards its asynchronous callback with active system, current widget, current reward membership and non-null Item.
- Generic UI/VM remains presentation-only, with no claim or inventory logic.

## Evidence reviewed

- runtime.json records ItemSlotProbe.Check=True, exercising both new and Micros skins; the reviewed probe checks actual image/text/state/fill/color controls and verifies old child updates stop after unbinding. The later recycle.json and updated archived probe cover model preservation and bounded recycling as described above.
- metadata.json records nine fields and zero missing references/issues. reward-metadata.json records two reward fields and zero missing references/issues.
- mail-cell.json and mail-flow.json provide implementer-recorded integration evidence; they do not independently establish recycling safety.
- dependencies.json lists the dependency closure, including shared UI textures and fonts. A clean package reinstall and independent database readback were not performed by the reviewer.

## Limits

No full acceptance claim. Long-duration list recycling and framework binding-cache accumulation, path-based asynchronous loading races, forced delayed icon callbacks, rapid close/reopen, clean reinstall, visual equivalence of all slot states, and frame-level animation verification remain unverified. Prior nested-prefab exporter limitations also remain. publication.json reports ItemSlot 2 / RewardEntry 3 / SvMail 7 and GameCLIApp revision 63, verified=true. Reviewer has not independently queried the database. Final exported database snapshots in .data/item-slot/final/{ItemSlot,RewardEntry,SvMail}.json have versions 2/3/7. Reviewer independently compared every Assets/Scripts entry against local files: zero mismatches, including corrected UIRewardEntry.cs. readback.json records successful source/material verification (113/12/104 materials) and project revision 63. The earlier .data/item-slot/RewardEntry.json is a preserved historical v2 candidate, not the final v3 snapshot; the apparent material discrepancy is resolved. Implementer reports temporary probes removed, final compile successful, Console zero and Edit Mode; reviewer did not operate Unity.


