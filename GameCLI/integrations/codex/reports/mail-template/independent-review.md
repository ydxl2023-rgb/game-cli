# Independent Mail review

Reviewer: independent mail acceptance agent. Scope: read-only source inspection and review of supplied evidence; no Unity execution or physical input was performed by the reviewer. The reviewer wrote only this report.

## Result

Overall acceptance: **incomplete**. The supplied core-flow evidence passes, but the acceptance skill requires additional runtime cases. Publication is not equivalent to complete acceptance. Publication evidence now records SvMail v4, with GameCLIApp revision 60. This reviewer did not query the database independently.

## Passed by source and supplied evidence

- Entity owns mutable inbox and receipt collections; imports, queries and receipt queries copy attachment elements.
- Claim and deletion qualification queries are shared with operation validation. Unix timestamps are range-checked before rendering.
- Local claims reject repetition and pending attachments block deletion. Reimporting a claimed mail ID retains the claim result.
- Final-source runtime evidence was rerun at 2026-11-18 07:33 UTC. `entity.json` records CheckEntity=True. `flow.json` records zero mail, zero visible cells, two receipts and the empty state after single and batch operations.
- `reopen-empty.json` retains the empty inbox and two receipts; initialization does not repopulate an intentionally empty inbox.
- `click-claim.json` and `after-click.json` show the claim button was hit and generated one receipt; `click-close.json` identifies CloseBtn. These are EventSystem center-raycast plus dispatched pointer events, not physical mouse automation.
- `verify-code.json` passed with no issues. `console.json` contains no error entries at its recorded timestamp.
- Images now bind Sprite objects through existing SyncSprite; image callbacks reject inactive systems, destroyed/replaced pages and obsolete reward snapshots. No business-specific ISyncAction was introduced.

- Final `verify-prefab.json` static bindings passed for three Widgets and 36 fields, with no binding differences or unverified binding entries. This verifies compiled-contract integrity, not all runtime callbacks or intended target semantics.
- Implementer reports final compile success and Edit Mode cleanup. MailProbe was removed from Unity and archived as MailProbe.cs.txt; no final Editor-state operation was performed by this reviewer.

## Resolved final source correction

MailSystem.Refresh now explicitly initializes `IconVisible = false` at line 127. The reviewer checked the current source and packaged MailSystem.cs, both SHA256 `1384db5b55eef4d9c75cdbba87ce89595c777d8088547f668261da5aa2221f70`. This resolves the source-level missed-hide notification. The correction was published in SvMail v2 and is retained in final SvMail v4 (verified=true). The implementing agent reports database readback equality; this reviewer independently compared the local packaged source, not the remote database. Forced asynchronous image timing remains an unexecuted runtime case.
## Incomplete / not independently established

- Long inbox scrolling, large attachment collections, long-body scroll behavior and intended runtime target semantics for all Widget fields.
- Forced slow/failed image loads, rapid selection with different icons and stopping/reopening during pending requests.
- Close completion timing, rapid reopen during the close animation, and loading-time deactivation.
- Frame-by-frame samples of Open, Close, EnvelopeOpen, EnvelopeClose and EntryOpen.
- Full prefab verification remains incomplete: final verify-prefab.json has differences=[] but 294 unverified items. Native/browser visual equivalence is not established. Browser preview does not run C# lists or native runtime state machines.
- Independent database hash readback and asset closure verification.
- Shared framework Fancy rebind/property-cache behavior remains outside this business-source review.

No real network or inventory behavior is claimed. Local receipts and inbox data are memory-only reference behavior.


