# Configuration integration delivery

Completed on 2026-11-20. Work performed only in this run directory; practice's existing source workbook and development were not changed.

- Input: frozen project configuration packages for Home, Shop, Mail, Settings, RedDot; published SvLogin and MasterSystem packages supply missing dependencies. Raw database workbook SHA256 verification was performed by source export.
- Preserved all 13 existing practice CommonText entries. Merged Home and Shop aliases via UnityCLI Excel: each insertion passed dry-run before the write. No ID/schema conflicts were found. There was no existing practice language workbook or foreign translation to overwrite.
- FunctionSystem starts from Home's 43-row source and merges Mail, Settings, RedDot entries. Chapter and RedDot use their supplied source workbooks. Example activity and chapter data retain source semantics and are not production service data.
- Ran the real existing e2cs executable with absolute input/output paths and Config namespace. C#, JSON/bytes, language workbook and compilation DLL were generated; no exporter exception was found. Do not install the DLL alongside source C#.
- All `Excel.*` references found in frozen project `.cs` sources resolve in generated source. CommonText has 54 entries; every numeric language reference resolves, including the final record. Output hashes and row counts are in config-output-manifest.json.

Installation contract: copy config-output/csharp sources and config-output/json bytes into target project; deserialize and load all five dictionaries before activating business systems. The integration owner performs installation and runtime verification. This delivery is not independent runtime acceptance.

Evidence: config-merge.jsonl, config-export.log, config-validation.json, function-source-query.json. Reproduction: merge-config.ps1 then e2cs.exe -i <run>/config-source -o <run>/config-output -f Config -l propLanguage.xlsx -n Config.

No acceptance-return repair rounds occurred in this configuration work. One reporting-only correction replaced a PowerShell property-count array with the actual entry count; no workbook or generated runtime content changed for that correction.

## Developer self-test repair 1
On integration request, added product routing gates SYS_LOGIN=10002, SYS_NOTICE=10009, SYS_SHOP=10010, reusing existing reference aliases/IDs without collisions. These are product adaptation rows, Switch=1/Level=0 with InstallOnStartup=0/Active=0. FunctionSystem now contains 49 rows. Master/Function/RedDot infrastructure already has its explicit gate semantics; Launch/Loading are views and do not receive fake RuntimeSystem rows. Re-exported with real e2cs; evidence config-routing-repair.jsonl and config-export-repair1.log. This is one developer self-test configuration repair, not an independent acceptance return.
