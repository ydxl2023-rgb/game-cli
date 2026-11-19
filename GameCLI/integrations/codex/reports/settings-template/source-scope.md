# SV Settings reference

Source: `Assets/data/UI/Set/@SetView/Prefab/SetSystemView.prefab` from the SV reference. Five tabs retain the original layout and static Chinese labels. Materials contain the resolved source archive, image resources, native prefab, font references, source animation clips/controllers and mapping inventory.

The template owns 30 device preferences: graphics tiers, independent channel levels and mute switches, local gameplay/chat/privacy/push preferences and download network policy. SettingsEntity validates and saves these values. Unknown or malformed persisted values fall back to defaults; snapshots do not expose mutable owned state. A medium graphics preset maps the original two-level model control to Low. Custom is derived from the individual tiers, not a separate preset.

SettingsChanged publishes the local snapshot; the product applies audio mixer levels, SV-specific rendering effects, gameplay, server privacy and OS notification policies. No original SDK, URL, renderer, network stack, account deletion, account switch or actual download is installed. SettingsActionRequested forwards those original service entries without claiming a successful service result. Applications should apply restored preferences during startup, independently of opening the page. The page's initial notification also provides a snapshot to consumers already subscribed.

The original root clip animates the Sound page's headset when selected; the BackGather clip animates the return/title area. Both native clips are preserved through UnityCLI and replayed at the corresponding lifecycle point. Browser preview supports the default BackGather entrance only and does not execute C# preferences or service requests.

Explicit conversion differences: the inactive empty StaticList with negative source width is excluded. Tab mesh/particle glow branches remain archived rather than being silently represented as UGUI animation curves; selected tab artwork is retained. Unknown product server name is blank. Platform-hidden source rows and the account-transfer modal stay hidden, with their controls retained for product adaptation. Static text language switching is not wired; dynamic product messages must use propCommonText.

Install scripts under Settings, RuntimeSystem and the existing shared StorageKey helper; reuse existing identical shared helpers instead of defining duplicate types. Restore materials with adjacent meta GUIDs and verify SHA256. Shared Common fonts must be compared before reuse. Merge SYS_SETTINGS from the configuration package and regenerate with the target exporter; do not overwrite the whole product workbook. Integration/MasterSystem.cs illustrates optional Home routing and must not replace a product coordinator.

Development and acceptance skills are siblings under skills/oathx/system/settings. Runtime test probes are evidence only and are excluded from the template code package. Reports distinguish static review, native compilation, UI/animation observations and remaining unverified behavior.

Regional reference: the Chinese CnBox/CnBox_2 service groups are visible; the overlapping overseas Box (transfer code, account binding, AI help) remains archived and inactive by default. SV switches these groups through regional function/account predicates. Products must select the intended variant instead of activating both. Native function scrolling retains RectMask2D and graphics scrolling retains Mask with showMaskGraphic=false.

Revision 2 removes the Tencent customer-service entry and RiceRoll, Power, Weekend and HeadquartersMatch notification preferences at the user request. The current native/browser nodes and bindings exclude them. Original source archives are historical evidence only. Remaining preference/action enum numbers are stable; deleted string/numeric keys are ignored individually during restore.

Revision 3 also removes ExtremeTrain and HomeDaily notifications and the now-empty GridPushMessage container. Thirty preferences remain; numeric identifiers 10 through 15 stay unassigned and are ignored when restoring old saved values.
