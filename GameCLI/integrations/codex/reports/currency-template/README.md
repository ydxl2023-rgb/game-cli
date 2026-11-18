# Currency component delivery — 2026-11-18

CurrencyBar v1 is an independent UI reference template with four C# classes, CurrencyBar.prefab, CurrencyEntry.prefab, images, font and metadata. It does not implement a RuntimeSystem or own wallet/navigation data. Home v21 embeds the bar as a nested prefab and declares the CurrencyBar dependency. GameCLIApp revision 51 retains its existing navigation.

Native resources live under Assets/Data/Widgets/Currency; code lives under Assets/Scripts/Systems/Common/UI and its VM subdirectory. The original bar docking and slot artwork are retained. The single-entry prefab is a reference for separately placed slots; the bar preserves three authored slot layouts.

The host supplies three VMCurrencyEntry objects and callbacks once. Amount is a formatted string; Icon, visibility and interaction state update in place. The current framework appends SyncClick listeners and does not clear UIWidget property caches on Unref: do not replace callbacks or VList on a bound instance, and destroy/recreate after Unref. Guards reject rebinding a released instance. See Currency development/acceptance skills for the complete contract. Dependency manifests describe installation order; automatic recursive installation is not claimed.

Validation: Unity compilation succeeded; independent component runtime test verified three amounts, three icon callbacks, three add callbacks, visibility/interactability and removal of subscriptions on Unref. Home opened with all 306 binding fields valid; its remaining 64 activity/function entries and 128 badge slots passed the existing badge attach/hide/remove/reopen regression. Console reported zero errors. Screenshot: development/client/UnityCLI-Screenshots/currency-home.png.

A separate agent reviewed code and prefab dependencies, identified the original Home font reference and lifecycle guard issue, then confirmed both were corrected. It reviewed raw runtime evidence; runtime execution was performed by the implementing agent. No claim is made that the original full Home static-layout comparison passes: optional badge node migration already differs from the source layout.

Database readback compared 44 Currency source/material records and 555 Home source/material records with local SHA256, including binary asset bodies. The Home package references CurrencyBar rather than duplicating its source/resources. Browser preview remains a static design and does not execute C#.

Reproduction tools: scripts/register-currency-template.mjs (first registration only; dry-run by default), scripts/build-home-source.mjs, scripts/build-home-presentation.mjs, scripts/package-home-fidelity.mjs, scripts/publish-home-fidelity.mjs, scripts/verify-currency-publication.mjs. Existing templates must use candidate publication rather than rerunning first registration. Extraction used the official UnityCLI prefab extract operation; no native prefab YAML was edited directly.

## Shared UI path migration

Currency code now lives in Systems/Common/UI, with VM classes in its VM subdirectory. All four MonoScript GUIDs and source contents are unchanged. CurrencyBar v2 carries the new paths; Home dependency metadata is updated accordingly. Both prefab Metadata checks passed at the new paths, and compilation/Console checks reported no errors. Earlier verification files describe the initial v1 delivery and retain their historical source paths.
