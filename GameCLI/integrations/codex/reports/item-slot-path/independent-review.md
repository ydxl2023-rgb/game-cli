# Independent review: path-based ItemSlot icons

Date: 2026-11-18. Scope: independent read-only source review. No Unity operation or business-code modification by reviewer.

## Result

The requested string-path contract is consistently implemented in the inspected business source. VMItemSlot.icon is string with SyncSprite, keeping its existing field index and all nine bindings. Mail assigns Item.Icon from MailReward.icon and sets IconVisible=true. The MailSystem.LoadIcon method and its Sprite callback assignment have been removed. No remaining Sprite assignment to VMItemSlot.Icon was found in current Assets/Scripts. No custom ISyncAction or framework change is required for this contract.

Overall acceptance remains incomplete pending actual path-load evidence and final publication snapshots. This is a source-consistency finding, not a claim that asynchronous lifecycle races are resolved.

## Changed guarantees

The existing shared SyncSprite string branch invokes LoadAsync and assigns c.sprite in its callback. Its UnSync is empty; it has no cancellation, current binding identity or request ordering guard. The former Mail-specific active/current-widget/current-reward guards no longer exist. IconVisible=true now means host-requested visibility before the asset necessarily finishes loading; it no longer guarantees that stale images stay hidden until loading completes. Rapid replacement may temporarily display a previous icon, and a late earlier callback can overwrite a newer request. Closing/destroying while pending also has no explicit callback protection in SyncSprite.

These are documented existing framework limitations accepted as outside this small contract change; previous guarded-Sprite test conclusions must not be carried forward. Ordinary successful path loading cannot establish cancellation or race safety. Do not pass null as an icon-reset mechanism.

## Remaining checks

Verify an actual non-empty path loads the intended Image sprite, both item skins retain binding integrity, Mail claim/delete flow remains unchanged, and final ItemSlot/RewardEntry/Mail packages reference the string contract. Preserve the already-reviewed child VM release/recycle behavior. Rapid path changes, forced out-of-order completion, destruction while loading and long-duration Fancy cache accumulation require separate coverage.
