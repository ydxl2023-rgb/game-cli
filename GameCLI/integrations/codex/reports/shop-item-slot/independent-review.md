# Shop ItemSlot composition — independent review

Scope: only this change extracting the two product cards and composing the shared ItemSlot. Read-only review; reviewer did not drive Unity or modify implementation. Runtime results below are developer-run raw evidence, not a separate independent execution.

## Result
No blocking defect found in the reviewed change.

- VMShopGiftItem and VMShopRechargeItem each expose one SyncMetadata VMItemSlot. ShopSystem supplies string paths and gift quantity through Item; pricing, limits and transaction methods are unchanged in the diff.
- UI card Unref temporarily clears Item, unsubscribes the parent, and restores the same model. Destroy releases nested subscriptions. recycle.json exercises both cards across three same-model rebinds and verifies old child changes no longer update the control after Unref.
- Independently read native prefab text: both standalone card assets reference the actual shared ItemSlot GUID; each parent page references its card GUID. native.json additionally validates the nested source relationships, Fancy cell/template references and missing serialized references.
- Runtime evidence checks actual loaded Image paths and gift Text values, one slot/one Button per card, Open animation state, category changes, card hit testing, purchases and duplicate-confirmation rejection. Click tests use EventSystem raycast/dispatch, not physical mouse automation.
- readback.json verifies 40 code files, 745 material records, both standalone cards and no missing dependencies. Published SvShop v5 and GameCLIApp revision65 use ItemSlot v3. code-verification and final Console evidence are retained beside this report.

## Limits
This is not full mall acceptance. Runtime Inspect checks Open state and elapsed normalized time, not all visual animation curves or source visual equivalence. Rapid asynchronous image request ordering/cancellation, close-during-load, long-list stress, and genuine payment are not proven here. Shared SyncSprite asynchronous lifecycle limitations remain; no new business image loader was introduced. Existing UIWidget binding-cache accumulation is outside this scoped change.
