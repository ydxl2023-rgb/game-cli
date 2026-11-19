# Practice database rebuild — 2026-11-16

Project: p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, design revision 3. Unity identity: 44f4f783-645c-41e7-80fe-aead8477c978. Frozen project design: project.json.

The project's five roots are Launch (SvStart v6), Login (SvLogin v7), Notice (SvNotice v4), Loading (SvLoading v15), and MasterSystem v6. The main layouts and animations are generated from project revision 3 through GameCLI ugui. Published SvLogin v8 supplies current server-selection source and companion materials; SvStart v7 supplies XLauncher source. Database project/template data has not been overwritten.

Product contract remains the documented local practice trial: no network authentication or Home implementation. Master coordinates Login and Notice; Enter records a local login result. Loading is a caller-controlled page. Action:none entries remain outside the defined business flow; no fake service callbacks are added.

Adapters reused from this project's archived implementation: Master, Notice, Loading, RuntimeSystem, StorageKey, CommonText, isolated text export and PracticePlugin. Login was replaced with database v8 sources; Refresh(LoginResponse) was removed because this product has no network protocol; login.pid initializes to the explicitly local test ID 1; Enter uses that ID instead of development MasterPropertyDef. The UILogin path maps SVLogin/SVLogin.prefab to Login/Login.prefab. Fancy grid sync actions are registered in PracticePlugin.

Generation adaptations: the standalone Login prefab omits the cross-system Notice target from its generation input; LoginSystem routes Announcement to Notice through Master. Notice has a derived Empty text node for its existing empty-state VM contract. This node does not alter the frozen database design. Existing code/config test adapters are not claimed to be byte-identical standard sources.

ServerSelect materials are decoded from database materials.json and SHA256-checked. Initial per-file CLI import was rejected by independent runtime acceptance because Unity discarded orphan .meta files before image import. Fixed through a controlled Editor AssetDatabase batch importing original asset/meta bytes together; root and inline cell Metadata class GUIDs are resolved against the installed VM MonoScripts. No Unity YAML was composed or edited.

Evidence: metadata-*.json, code-*.json, inputs/*/generate-*.json and acceptance/. Compiler success and metadata checks are developer checks only. Acceptance belongs to the independent reviewer; final completion is conditional on its results. No source changes to development or standard template publishing are part of this run.

Final result: independent bounded offline runtime verification passed for the exercised five-module flows. SvLogin integration gate passed with login-path-mapping.json and acceptance/login-acceptance.json; database workflow is complete at project revision 3. Final compile succeeded with Console error0, probes removed, Editor in Edit Mode. Generic prefab exporter incomplete diagnostics remain unchanged; see acceptance/runtime-review.md for same-frame reload and full Editor-restart limits.

