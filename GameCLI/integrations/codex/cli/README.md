# GameCLI verification

## Code conventions

```powershell
.\gamecli.cmd verify code --project <project-id> --system Login
```

Alternatively: `npm run verify:code -- --project <project-id> --system Login`.
Requires Node 22+ and a .NET SDK supporting net8.0. First run restores Microsoft.CodeAnalysis.CSharp 4.12.0 from NuGet and builds the syntax analyzer. Unity Editor is not required.

The project must have a verified local Unity binding. Scope is only `Assets/Scripts/Systems/<system>` in that project. Generated files (`*.g.cs`, `*.generated.cs`, `*.Designer.cs`, or an auto-generated header) are excluded and listed. Empty scopes and linked source entries are rejected. Source files are never changed. A source hash is recorded per file; sources and binding are rechecked before publishing the report.

Policy `oathx-code/1` currently checks:

| Rule | Check |
| --- | --- |
| OX001 | PascalCase type/method/enum names; camelCase property/variable/parameter names; no underscores |
| OX002 | No expression-bodied methods, properties or accessors; callback lambdas allowed |
| OX003 | SerializeField on explicit private fields; no field-target attribute |
| OX004 | Binding and its field declaration on separate lines |
| OX005–006 | English XML summary; parameter and return documentation |
| OX007 | Fields/properties, constructors, lifecycle overrides, public then nonpublic methods |
| OX008 | Blank line before subsequent member documentation |
| OX009 | Direct VM subclasses in VM/; direct UIWidget subclasses in UI/ |
| OX010 | Blank line before a return following processing in the same block |
| OX011 | No Chinese default parameter literals |
| OX012 | OpenWidget has arguments and no literal null argument |
| OX013 | System CloseWidget calls use nongeneric overload with the identifier widgets |
| OX014 | No literal PlayerPrefs keys |
| OX015–017 | Review findings: transform lookup, Chinese string literals, disabled preprocessor branches |

These are syntax checks, not a Unity compilation or full framework acceptance. Callback validity, array declaration contents, null-guard control flow, reflection key correctness, inherited class roles, meaningful comments and business grouping of blank lines still require review. Inactive preprocessor branches are not checked. A `passed` report means the automated rules passed, not that all development requirements passed; inspect `reviewRequired` and `limitations`.

Statistics report comment-line rate, blank-line rate and documented-member rate separately. No arbitrary percentage threshold determines acceptance. Exit `0`: automated rules pass; `1`: rule violations; `2`: syntax parse failure, missing evidence or operational error. Reports include file, line, rule, severity, message, excluded paths and SHA256 hashes, saved under `Library/GameCLI/Verify/<run-id>/report.json`. `--output <new-file.json>` writes an additional report without overwriting an existing file.

## Prefab comparison

Run from `integrations/codex` on Windows:

```powershell
.\gamecli.cmd verify prefab --project <project-id> --prefab <root-node-id>
```

`gamecli.cmd` can be placed on PATH to use `gamecli verify prefab`. Alternatively run `node cli/gamecli.mjs` with the same arguments.

The package also registers the `gamecli` npm bin entry; `npm link` from this directory installs the command for the current Node environment. `npm run verify:prefab -- --project <project-id> --prefab <root-node-id>` requires no global installation.

The prefab ID is the saved project's UI root node ID, not a database standard-template ID. The command fixes the project revision as its baseline, verifies the project's Unity binding, and uses UnityCLI to read the actual asset. It never generates or repairs the prefab, changes the project design, or publishes templates.

Optional arguments:

- `--asset Assets/...prefab`: explicitly select a prefab when its name differs from the project root or multiple assets share the name. It must remain inside the bound project.
- `--unity-cli <exe>` or `GAMECLI_UNITY_CLI`: choose a UnityCLI build supporting `asset search` and `prefab export`. With a local bridge dependency, first use the CLI build beside that package's owning Unity project; otherwise use the bound project's `Library/UnityCLI/unity-cli.exe`. Every call still explicitly targets the verified bound project, never the bridge's source project. The Editor must be open with a compatible bridge. Older UnityCLI builds lacking export fail explicitly.
- `--output <new-file.json>`: additionally save the report; existing files are never overwritten.

Exit codes: `0` passed, `1` differences found with complete supported evidence, `2` incomplete evidence or operational error. JSON reports include node/property/expected/actual differences, unsupported evidence, project revision, baseline SHA256 and Unity dependency revision. Evidence is kept under the bound project's `Library/GameCLI/Verify/<run-id>`.

Policy `studio-prefab/2` compares structure and sibling order, active state, RectTransform, scale/rotation, basic graphic colors and hit targets, basic text properties and button state/references. Numeric tolerance is fixed at 0.0001; there is no command-line switch to weaken it. Unity GUID/fileID and export node IDs are not comparison keys.

### Widget / VM static binding checks

`verify prefab` also returns a separate `bindings` result. The exporter reflects the compiled `UIWidget<TVM>` and `[Binding]` declarations and reads the existing Metadata from the same disposable prefab. It does not call widget-metadata generation, modify assets, invoke callbacks or initialize VMs.

Checked: Widget/Metadata cardinality, VM type and VM MonoScript identity, field coverage and declaration order, repeated fields, expected SyncAction and required target component type, generated public property setter presence, target/component list counts, null/out-of-hierarchy references, target/component GameObject agreement, component assignability and duplicate component references within a field. UISystem attaches page Widgets at runtime, so root pages without a serialized Widget resolve their unique UI type through its Config prefab path. Multiple distinct targets for a field remain allowed. Inherited VM layouts remain incomplete because generated index ordering cannot safely be inferred across base classes.

The read-only exporter extension is shipped in `integrations/codex/unity/UnityCLI~/UnityCliWidgetBindingEvidence.cs`. Install it explicitly into the Unity project's actual `com.oathx.unitycli` package before running verification:

```powershell
node scripts/install-widget-binding-exporter.mjs <com.oathx.unitycli-package-directory>
# Then run UnityCLI compile --wait against the bound Unity project.
```

Setup adds the extension and one export hook; it does not edit prefabs. Verification never automatically installs code. Old exporters without `oathx-widget-bindings/1` evidence report `incomplete`, including for a seemingly plain prefab, because absence of evidence cannot prove absence of bindings.

Static integrity does not prove a valid component is the product's intended target: scripts alone do not encode an approved field-to-design-node mapping. The existing script-contract warning remains until that mapping is available. Runtime text refresh, callback behavior, Sync factory registration and listener cleanup are not tested. Custom script diagnostics also remain; checking Metadata does not validate every serialized field of a custom component. Thus `bindings.passed` may be true while overall prefab acceptance remains `incomplete`.

This first adapter deliberately **fails closed**: the existing Unity export does not fully expose external resource hashes, fonts, custom script/Metadata references or Animator curves. Those features, exporter diagnostics, unsupported components, specialized controls and missing design contracts produce `incomplete`, never `passed`. It is not yet a complete visual/VM/animation acceptance suite, and it does not prove product business behavior. Extend the exporter and versioned comparison policy before accepting those cases; do not whitelist diagnostic failures to make an asset pass.
