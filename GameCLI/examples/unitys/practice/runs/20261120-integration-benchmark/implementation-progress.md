# Installation adaptations and self-check events

- Project revision 6 frozen; all main designs retained. Launch/Login/Notice/Loading legacy project code adapted to current framework conventions.
- Shared components installed from exact dependency snapshots, hashes validated; native materials staged and imported in one AssetDatabase batch.
- Added missing IUIEventSender/ResourceConfigure/UISystem lifecycle contracts from RedDot's framework reference to practice only.
- Home framework Master coordinator adopted for Function gating; Mail/Settings/Notice routing added beside Shop. Removed unrelated Fight unload.
- Login v10 behavior and server-selection companion used with project v7 main layout. Removed network LoginResponse method/import. Master initial player ID 1 is explicit offline test data.
- Configuration uses real e2cs outputs in Resources/PracticeConfig, loaded before entities.

## Self-check events
1. Initial compilation: stale Oathx.HttpProtocol import after removal of network contract. Removed import; recompiled.
2. Compilation: reference Master unload mentions FightSystem outside scope. Removed reference; compilation succeeded with Console zero errors.
3. Loading generation dry-run timed out while native material import was still processing large TTC fonts (~60 s each). No build claimed successful. Wait for importer before repeating.
4. Integration review found absent Login/Notice/Shop FunctionSystem gate rows. Configuration owner preparing source-table merge at existing IDs and re-export.

Independent acceptance return rounds: none yet. These are implementation self-checks, not reviewer returns.
5. Notice legacy adapter expected VM EmptyText absent from frozen project. Retained empty-state label as static prefab text per current convention, removed dynamic assignment; compile passed.
6. Editor binding helper used C# null-coalescing on missing Unity Canvas, causing MissingComponentException during scene integration. Replaced with Unity boolean null check; binding result success.
7. First runtime screenshot showed clean-baseline constant-pixel CanvasScalers cropping generated 1920x1080 pages. Configured all layer scalers to ScaleWithScreenSize/Expand and saved via Editor API. Preserved pre-fix screenshot as diagnostic.

First implementation handoff: 2026-11-20T08:47Z. Independent acceptance now owns Editor; no further product mutation until feedback.

R1 formal return received 2026-11-20T08:53Z: invalid identity login (runtime reproduced), syntax policy errors, Home static exporter ambiguity. Repaired Master identity/lifetime/ownership and all flagged policy errors. Internal R1 syntax checks needed two formatting adjustments (property convention; member spacing); unified compile+Console clean 09:00:43Z. Rehanded to independent acceptance 09:01Z, formal return count 1. Home remained unchanged: all 13 serialized reference identities proved valid, static comparison remains failed as source README requires for migrated badges/nested prefabs. Independent runtime acceptance remains pending.
