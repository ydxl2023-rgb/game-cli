# Master development handoff

Source: frozen MasterSystem v6 database template in templates/MasterSystem.json. Target: Practice project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, Unity identity 44f4f783-645c-41e7-80fe-aead8477c978.

Implemented Master/MasterSystem.cs, Master/MasterEntity.cs and shared Systems/RuntimeSystem.cs only. Master remains code-only. Removed template Home, chapter/config defaults, persistence and unused LoginFailure as outside the local trial. State is an XEntity-owned pid, not a coordinator member. No production saves or network calls.

Public global contracts: `new SystemActive { type = typeof(NoticeSystem) }`, `new SystemDetive { type = typeof(NoticeSystem) }`, `new LoginSucceed { pid = 1 }`; each implements Oathx.Core.IEvent. Consumers post through existing XPluginUtility; integration must route it to the same dispatcher that owns Master.

Root integration: initialize entity manager and explicitly CreateEntity<MasterEntity>() once before loading MasterSystem. Load Master through the dispatcher. Pump dispatcher.Update each frame. UnloadAll and clear owned entities on shutdown. Avoid a second entity on re-entry. Login success records pid only; Login remains visible because no Home or replacement gameplay page is authorized.

RuntimeSystem retains LoadSystem(Type, Action complete = null) and UnloadSystem(Type, Action complete = null). It defers dispatch mutations to the next frame, avoiding dictionary enumeration mutation. Callbacks mean the dispatcher lifecycle operation returned, not that async UI loaded. Pending work does not execute after its owning system deactivates. Master ignores invalid, abstract, generic and self-targeting system requests.

Self-test: source generation and independent node/Roslyn code-convention verification performed; see the saved CLI report in target Library/GameCLI/Verify. Unity compilation/runtime tests are intentionally pending the root-owned Editor slot. No source prefab/scenes, shared packages or development files edited; no commit/push.

Required runtime cases: Master active before local success leaves pid zero; positive LoginSucceed updates entity pid; invalid/null results leave it unchanged; SystemActive opens Notice after current dispatch; SystemDetive closes it; repeated open/close remains functional; deactivating Master removes event handlers; app restart does not duplicate entity or subscriptions. No prefab check applies to Master. Root integration and independent acceptance must exercise these against frozen delivery.
