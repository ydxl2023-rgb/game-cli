# Practice full-flow trial

Authorized scope: independently develop and independently accept all five current standard modules: Launch, Login, Loading, Master, Notice. Preserve the selected template designs. This is a local test product, with no server calls, real authentication, Home or other game systems.

Unity target: G:/Works/GameCLI/GameCLI/examples/unitys/practice/client

Studio project: p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484

Unity identity: 44f4f783-645c-41e7-80fe-aead8477c978

Baseline design revision: 3 (revision 2 instantiated the five modules; revision 3 connected Login's Announcement button to Notice). Template source snapshots are in templates/ beside this file: SvStart v6, SvLogin v7, SvNotice v4, SvLoading v15, MasterSystem v6. The saved project-baseline.json retains the pre-connection revision 2 for provenance; use project-final-design.json for the acceptance design.

Root prefab IDs: Launch T1T2Start0; Login T1T1Login0; Notice T1Notice0; Loading T1T1T1N0; Master CodeSystem1 (code only).

Read repository AGENTS.md and skills/oathx/system-development, development-rules, module skills. Acceptance uses system-acceptance and a different agent. Do not edit development, standard templates, shared package sources, or other modules. Do not commit/push.

Integration owner: root agent. Only root edits PracticePlugin, global startup, shared text/config and project composition. Module developers may propose required integrations. Shared events owned by Master developer: SystemActive(Type), SystemDetive(Type), LoginSucceed; use actual framework event signatures and report the exact API before consumers depend on it. Root coordinates any changes to this contract.

Trial behavior: Launch shows startup and then retains only background behind Login. Login opens/closes, selects a local server and requests Notice through Master. Enter emits the existing template's local LoginSucceed event; this is an explicitly local trial, not real authentication. Master coordinates lifecycle and records success without adding Home. Loading provides callable open/close/progress/text updates; the test driver supplies explicit values, not a fake loading task. Notice supports test announcement/empty state, hyperlink behavior and standard dialog.open.scale animation. Keep other template entries visible and interactive; unsupported business entries report their pending integration rather than inventing services.

All Unity/CLI editor actions are serialized by root. During source preparation agents must NOT invoke compile, asset generation, Play mode, or any Editor mutation until granted the editor slot. No one adjusts acceptance policy or baselines to obtain a pass. Final artifacts and raw evidence live in this run directory or the target's Library/GameCLI, with explicit paths in reports.

Development handoff: code/resources touched, template provenance and adaptations, public integration APIs, dependencies, self-test evidence, remaining issues. Acceptance consumes the frozen delivered files and independently executes checks; defects return to the original developer for repair.
