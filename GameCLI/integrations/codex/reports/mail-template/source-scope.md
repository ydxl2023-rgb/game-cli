# SV Mail reference

Source: SV `Assets/data/UI/Mail/@MailView/Prefab/NewMailView.prefab`. The source export, original component descriptions, image references, native animation curves and adaptation inventories travel with the template.

Implemented: local inbox, stable list selection, detail/body scrolling, read state, single/batch attachment receipts, guarded deletion and empty state. This is a reference implementation, not a production mail service. Claims do not grant inventory. Inbox and receipts are in memory for the current run; an optional, clearly labelled example is loaded only when no inbox has been supplied. Example dates are rebased at first activation.

Restore `materials.json` files and adjacent meta GUIDs using the asset service. Binary resources use SHA256 IDs. `Mail/UI` contains UIMail and the two reusable list cells; `Mail/VM` contains their VMs. RuntimeSystem is a framework prerequisite; Integration/MasterSystem.cs is a route example, not permission to overwrite the product coordinator. Merge only the SYS_MAIL configuration entry and regenerate with the target exporter.

Animations: original root Open/Close, EnvelopeOpen/EnvelopeClose and EntryOpen are native clips/controllers with remapped targets. Unsupported special-item particle/mesh and RGB/active effects remain documented in the source archive. The browser card is a static shell with supported default animations; native runtime state machines and C# lists are not executed by browser preview.

Two intentional layout adaptations: hide the original capacity `/50` because this reference has no capacity policy; move the NEW badge right by 20 native units to avoid Fancy viewport clipping. Original normal/bold Chinese font faces are included for native and browser rendering. Shared native font files must be reused by matching hash or migrated through Unity tooling if the target has conflicting GUIDs.

Evidence: entity.json, flow.json, reopen-empty.json, click-claim.json, after-click.json, click-close.json and console.json. Click probes perform EventSystem raycast at the button center and dispatch a pointer click to the hit hierarchy; they are not physical mouse automation. No real service or inventory was touched. Full long-list, rapid reopen and frame-by-frame animation acceptance are recorded separately as unverified rather than inferred from source existence.
