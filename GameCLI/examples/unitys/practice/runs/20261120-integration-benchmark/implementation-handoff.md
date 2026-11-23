# Ten-system implementation handoff

Target: practice/client; verified Studio project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, unityProjectId 44f4f783-645c-41e7-80fe-aead8477c978. Frozen project revision 6, exact root source versions in source-manifest.json.

## Installed behavior
PracticePlugin loads the real exported five configuration tables before creating [Initialize] entities. Starts Master, FunctionManager dependency, RedDot and Login. Login is explicitly local, master initial ID 1, with v10 server-selection behavior adapted to project v7 main design (no authentication). Enter invokes LoginSucceed and Master loads local data then Home. Home Shop/Mail/Settings/Notice requests route through Master with FunctionManager gates. Other unconnected product entrances remain events, not fake services. Loading is caller controlled, not a system or automatic timer. Shop has no real payment SDK. Mail has local example receipt semantics. Settings has no external SDK service.

Native materials for Home/Mail/Settings/Shop plus ItemSlot/Reward/Currency/Profile and Login companions were SHA256 verified and restored by one controlled AssetDatabase batch (staged-materials). All four legacy main pages generated from frozen project inputs with preserved animations. Their Metadata, component references and Notice scroll/Empty derived node bound by PracticeBindLegacy. Empty state static text belongs to prefab, not VM. VM/auxiliary paths moved via AssetDatabase retaining GUID. Scene Launch is the prefab instance referenced by XApp; all UI layer CanvasScalers use Expand at reference 1920x1080.

Framework adaptations: RedDot IUIEventSender/UIResourceManager/UISystem reference; Master coordinator from Home framework, extends routes to Mail/Settings/Notice and omits unrelated Fight. Notice code adapted from previous bounded offline implementation with Entity-owned reminder expiration. Current project raw sources remain unmodified in run. Standard database and development not written.

## Developer smoke evidence
Compile succeeded and console0: developer-final-compile.json, developer-final-console.json. In Play, actual UILogin.vm.EnterClick returned true and UIHome appeared. Master activation opened Mail, Shop with Recharge, Settings and Notice; each showed concrete Metadata components and console0. Screenshot client/UnityCLI-Screenshots/ten-home.png captured before final Expand scaler fix (preserved diagnostic). These are developer smoke checks, NOT independent acceptance. Test probe PracticeSelfCheck and editor import/binding helpers remain temporary pending acceptance cleanup.

## Interfaces
Runtime static event request: XPluginUtility.PostEvent(new SystemActive { type = typeof(MailSystem) }), SystemDetive for closing. UI query UISystem.GetSingleton().TryGetWidget<T>(out widget), then widget.vm callbacks. Data XEntityManager.GetSingleton().QueryEntity<T>(); use entity methods, not mutable internals. Loading UISystem.OpenWidget<UILoading>(explicit callback), widget.UpdateProgress(float,string), SetTip. CLI general run/call/screenshot: G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe. New CLI for compile/console/ugui/excel: GameCLI/clis/unity-cli/bin/Release/net8.0/unity-cli.exe. Explicit --project required.

## Status
Editor in Edit Mode, independent acceptance may take ownership. Build hash inventory implementation-build-manifest.json. No independent verdict yet. Known service limitations are outside local test scope; do not report production authentication, payment, language switching or character rendering as complete.
