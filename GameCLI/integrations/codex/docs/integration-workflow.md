# Integration workflow

Run `gamecli verify integration --project <id> --template <id>` before declaring a system complete. The command stores the latest report per project and template, and the Studio project footer displays its workflow status.

- `repair`: missing materials, placeholder handlers, unresolved adaptations/reference review, empty inventory, or a failed runtime check. Fix the delivery and rerun.
- `awaiting-acceptance`: static inventory/reviews passed, but current runtime acceptance is missing or incomplete.
- `complete`: static checks passed and all six runtime checks have passed records from a reviewer with evidence references.

Supply an independent review JSON using `--acceptance <file>`. It must contain `fingerprint` copied from the current report, `reviewer`, and `checks` entries with `name`, `status: passed`, and `evidence`. Check names are listed in the generated report. Adapted source files and reference materials require `reviews` entries with `path`, `approved: true`, and a nonempty `reason`. A missing file cannot be waived. For legitimate product path changes, pass `--mapping <file>` with `{templateId, templateVersion, paths: {"Assets/Source": "Assets/Target"}}`. Targets must exist inside the bound project. Mapped files always require explicit review, even when bytes match; their target paths and current hashes are included in the fingerprint. Mapping is not an exclusion list.

A report fingerprint covers project/Unity identity, project revision, template version, inspected file hashes, and detected gaps. Rerun after source/resource changes; project revision changes automatically invalidate the displayed result. Reports are not a live Unity monitor. Evidence references are reviewer attestations, not automatically executed or authenticated Unity tests. This command never fabricates runtime acceptance, automatically fixes the delivery, or starts a development agent. Each system is reported independently; one completed system does not mark the entire project complete.
