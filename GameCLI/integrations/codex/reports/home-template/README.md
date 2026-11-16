# SvHome v2 — basic lobby

Scope approved on 2026-11-16: basic lobby, with character and seasonal animation left as explicit extension contracts. This is a reusable standard, not the full SV live product.

- Source: SV `Assets/data/UI/Home/@HomeView/Prefab/Home.prefab`, 709 native nodes. Full read-only export remains in `.data/imports/sv-home/unity`; extraction hash and exclusions are in `contract.json`.
- Published: `SvHome`, version 2, Home group. Read back from PostgreSQL and the Studio template-list endpoint.
- Production: 93 nodes including the animation host, 39 independent PNG assets, 46 VM bindings, 29 local navigation callbacks, 11 CommonText rows (2001–2011).
- Unity identity: GameCLIApp `p-2d314388-18c4-42f5-80eb-f6729dd23f0e`, Unity `b99aae81-c4b3-44ea-a032-a8df17831096`, development/client.
- Code: HomeSystem, HomeEntity, HomeEvents, UI/UIHome, VM/VMHome. Shared RuntimeSystem/CommonText are reference dependencies, not duplicate product types.
- Resources: `Assets/Data/Widgets/Home/Home.prefab` and HomeAssets. Database scripts include serialized prefab/metadata plus SHA256 references to PNGs stored in the asset library; configurations separately includes the eleven-row source workbook and merge entries.

## Evidence and findings

Unity compiled with no Console errors. `verify code` passed with no issues. Metadata generated 46/46 bindings. An independent reviewer confirmed all 29 raycast-directed pointer clicks produced exactly one matching event; 17 text/data bindings updated, including all 11 localized labels. Closing removed the page, and reopening restored it and all callbacks.

The first independent run found a real reopen defect. `GamePlugin.Load` respects the startup Active flag when constructing a new module. Explicit `RuntimeSystem.LoadSystem` now activates a returned inactive module, preserving startup installation behavior. Retest passed. Installation-failure behavior of the shared plugin is outside this check; this is not a claim that all module lifecycle paths were verified.

Transparent source Graphics were also corrected during extraction: their alpha no longer hides descendant controls in Studio.

## Remaining boundaries

Overall acceptance is **incomplete**, not a full product acceptance. Deactivation during a pending asynchronous load has not been tested. GameCLIApp revision 25 contains the SvHome v2 card (T1Home0). Project-instance `verify prefab` reports zero differences and passed bindings, but overall incomplete because 166 component/property diagnostics remain unverified. Runtime destination wiring and full relation integration are not claimed. The 1920×1080 asset preview covers the full static layout; the current GameView screenshot is cropped and is only evidence of runtime data rendering.

The character renderer, seasonal animation, activity/advertising catalog, platform SDK, avatar data and destination systems are not supplied. Buttons emit HomeEntryRequested; the product coordinator must map these to SystemActive/SystemDetive. HomePresentationRequested carries optional keys and does not itself render Spine. Original baked image text requires product-specific artwork localization.

The temporary editor probe was archived with evidence and removed from the Unity project. Play Mode was stopped. No Git commit or push was performed.

The prefab check found two lower-aligned text fields imported as middle-aligned. The importer now accepts the Studio start/end values, and both prefab fields were corrected through UnityCLI. Native materials were republished through candidate 32670118-27f1-420c-8766-cc2fd6753e3d as v2.
