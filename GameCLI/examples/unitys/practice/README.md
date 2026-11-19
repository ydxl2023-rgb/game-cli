# GameCLI practice

Unity project: `client/`, Unity 2022.3.62f2.

This project uses independent snapshots of development's seven `branches/gamecli` Oathx packages, including local fixes. Package provenance is recorded in `package-sources.json`; `package-verification.json` verifies 517 source files with zero hash differences. VCS metadata and package build caches were excluded. The CLI Editor bridge currently resolves from the same external local path as development.

The original clean baseline contained XApp, XLauncher, PracticePlugin, UISystem layers, an EventSystem and camera, without game business systems or data tables. The current workspace is the database-driven rebuild in runs/20261116-rebuild/; it is no longer the clean baseline. The older five-template trial below is historical.

The 2026-11-12 trial installs Launch, Login, Loading, Master and Notice from the recorded database template versions. Development and acceptance use separate agents. Requirements, original file hashes, frozen Studio design, template sources and evidence are retained in `runs/20261112-fullflow/`. This is a local test with no server authentication or Home system. Findings and incomplete checks must be read from that run's reports; successful baseline startup is not acceptance of the installed modules.

`PracticeSetup.Build` rebuilds the entry scene and updates the minimal local Mono configuration using Unity Editor APIs. It is intended for baseline initialization, not preserving user edits to the entry scene.

Original clean-baseline validation (before the five-template trial): batch compilation and scene generation exit 0; interactive CLI ping targets practice/client; multiple Play/stop/re-enter cycles reached the PracticePlugin ready log (4 occurrences in Library/visible-editor.log). Temporary PracticeProbe was removed through AssetDatabase via CLI, and final compile passed. CLI Console returned zero errors; no NullReference/Missing Script/compiler errors were found in the Editor log. That baseline run ended in edit mode. Final console evidence is Library/practice-console.json; screenshot is Library/practice-ready.png. Interactive Editor must be launched visibly on this machine; hidden non-batch startup did not initialize the bridge. Batch mode intentionally disables the UnityCLI endpoint.



## Reset on 2026-11-16
The Unity client has been restored to clean baseline 43262ba, preserving ProjectSettings/GameCLIProject.json. The five-template trial above is historical; its run records and Studio design remain intact. Source/assets/packages/settings were backed up under integrations/codex/.data/practice-reset-20261116-140553. Fresh runtime acceptance has not been performed.

Reset verification: Unity batch import/compilation completed with return code 0; log: client/Library/practice-reset-compile.log. No fresh Play Mode acceptance was performed.

## Database rebuild on 2026-11-16

Rebuilt from Studio Practice project revision 3. Main layouts remain project-owned; Login uses published v8 companion code/materials to implement its independent server selector. Source/identity snapshots, explicit path mapping, code reports and independent runtime findings are in `runs/20261116-rebuild/`. See `implementation.md` and `acceptance/runtime-review.md` there. Missing items or incomplete acceptance must not be reported as a completed project. Database standard templates and project design were not overwritten.

2026-11-16 rebuild outcome: independent offline flow checks passed, including ServerSelect. SvLogin integration workflow is complete with explicit path mapping and reviewed acceptance evidence. Final Unity compile passed and Console error count was zero. Full scope and limits remain in the run's acceptance/runtime-review.md.

