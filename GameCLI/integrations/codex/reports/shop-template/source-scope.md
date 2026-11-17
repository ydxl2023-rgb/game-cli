# SV Shop standard reference

## Selected scope

The user selected the current SV Home mall RechargeMainView: recharge and normal gifts. The older ShopTabView and ordinary goods store are not the selected source.

Four native pages are delivered under Assets/Data/Widgets: Shop, ShopRecharge, ShopGift and ShopPurchase. Eight UIWidget/VM pairs use serialized Metadata and four real Fancy lists. ShopSystem owns lifecycle; ShopEntity owns catalog, selection, simulated balance, tokens and stock. ShopDemoCatalog supplies explicit examples only when no catalog exists.

## Installation without the source project

1. Read the standard version, scripts and configurations together. Restore materials.json resources, fetch PNG assets by ID and validate SHA256. Preserve adjacent meta GUIDs, including animations and script references.
2. Install Shop root logic, UI and VM folders. Reuse the product RuntimeSystem and CommonText rather than duplicating shared references. Register ShopEntity and ShopSystem with product identifiers.
3. Merge the 21 SHOP aliases from propCommonText and generate/load CommonText plus Language with the product exporter. The independent workbook contains only Shop rows. Preserve existing translations.
4. Ensure UISystem cancellation invalidates pending requests by identity before a late callback can instantiate a page. FrameworkReference/UISystem.cs is a reviewed reference, not a second UISystem to compile beside the installed package.
5. Route SystemActive/SystemDetive through the product coordinator. The development example routes HomeEntryRequested.Shop to Shop; this is optional integration, not a dependency on Home.
6. Open ShopSystem. Purchases are local simulations; no payment SDK, network request or actual wallet is used.

## Source pages

- Assets/data/UI/Shop/@Common/Prefab/RechargeMainView.prefab
- Assets/data/UI/Shop/@Common/Prefab/Recharge.prefab
- Assets/data/UI/Activity/@NormalGift/Prefab/NormalGiftView.prefab
- Assets/data/UI/Activity/@NormalGift/Prefab/NormalGiftBagBuyView.prefab

The source inventory records GUIDs and read-only export hashes. Source assets were not modified.

## Animation and adaptations

Default entrance curves are retained in native clips/controllers and companion design JSON: main slide/fade, category tabs, return/currency headers, product cell entrance and purchase popup scale/fade. Original gift click scale (1 to 0.9 to 1 over 0.2 seconds) is a separate native Click state with full curve JSON. Recycled product cells replay Open. Main input and product selection obey the half-second opening gate.

Transparent sprite-less Image import now preserves alpha-zero graphics instead of white overlays. Dynamic price sizes and visibility defaults are explicit. The confirmation button is widened for the simulation label and amount.

Differences and product extensions:

- Recharge normally invokes an SDK; this reference deliberately uses local confirmation.
- First-recharge promotion/bonus computation is not implemented. Source UI hooks remain; demo products do not promise a first-pay bonus.
- Wallpaper, growth fund, special-price gifts, operator recommendations, activities, remote catalogs, real payment and customer service remain extensions.
- Mesh/particle branches and inactive item-conversion RGB/active effects are excluded. This is not every SV animation or the complete operational mall.
- Browser main card represents the dynamic shell. Companion designs and native materials travel in scripts; browser preview does not execute C# list population or purchases. Unity is the functional preview.
- Image-baked title artwork remains original artwork. Dynamic fixed text comes from propCommonText.

## Verification

Compilation, isolated transaction checks and real raycast pointer actions are recorded separately from independent review. Consult packaged acceptance.json for exact results and remaining limitations; publishing does not establish acceptance.

Target: GameCLIApp p-2d314388-18c4-42f5-80eb-f6729dd23f0e; Unity identity b99aae81-c4b3-44ea-a032-a8df17831096.

Current delivery: SvShop v3, GameCLIApp revision 30. Native Home entry and project relationship both open Shop. See delivery.json and evidence/ for exact checks. No commit or push was performed for this task.
