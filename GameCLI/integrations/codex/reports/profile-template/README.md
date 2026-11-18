# Profile path-binding optimization

Current delivery removes SyncProfileSprite and its registration entirely. VMProfile uses string Portrait/Frame with existing SyncSprite; PortraitVisible/FrameVisible use existing SyncVisible. Profile.prefab has eight bindings. No custom image ISyncAction or public package modification is introduced. A valid path is required; hiding retains artwork rather than clearing it.

Runtime verification: configured-loader path loading, sequential path replacement, text update, hide/restore, two callbacks and unbinding pass; authored defaults and initial hidden state pass. Home has 312 valid fields, seven correct pointer targets, and Inspector selection preserves bindings. Console errors: zero. See path-runtime.json and path-console.json.

Published PlayerProfile v4, ProfileShortcuts v3, SvHome v27 and GameCLIApp revision 57. Skills and source/material bundles are updated. Independent reviewer identified missing spacing (fixed) and inherited asynchronous limitations (documented): native SyncSprite does not guard out-of-order requests or callbacks after destruction. These scenarios are not claimed as accepted. Do not add another business synchronizer to solve shared loader concerns.

## Previous delivery (historical)

# Profile complete presentation delivery

The two reusable composite assets are Profile/Profile.prefab and Profile/ProfileShortcuts.prefab under Assets/Data/Widgets. Profile now owns UIProfile and VMProfile; the original body remains a nested Profile/Parts/ProfileBody.prefab retaining its GUID. The shortcut group still contains Mail, Settings, Notice, Friends and Affinity.

Common/UI/VM/VMProfile exposes PlayerName, PlayerLevel, Portrait, Frame, ProfileEntry and AvatarEntry. Home provides one VMProfile and no longer binds internal profile Text/Image components separately. Name and level come from the host entity. Portrait and frame accept loaded Sprites supplied by the host; no player-image service or asset-loading policy is assumed. Unassigned image fields preserve authored artwork; explicit null clears and hides the Image, and a subsequent Sprite restores it.

Runtime checks pass for initial and changed text, initial Sprites, authored defaults, initial null, image replacement/clear/restore, both entry callbacks, stale data and click cleanup after Unref, duplicate Ref rejection, and destruction while bound. Home's seven pointer targets hit correctly; 310 Metadata fields remain valid after selecting UIProfile in the Inspector. Existing 64-entry/128-badge-slot checks and close/reopen pass. Final console errors: zero. Screenshot: UnityCLI-Screenshots/profile-complete-home.png.

Published standard templates: PlayerProfile v3, ProfileShortcuts v2, SvHome v26; GameCLIApp revision 56. Database readback checks 95 records for each Profile composite, 565 for Home and 42 for unchanged CurrencyBar v2. Home retains all 71 navigation actions. Development and acceptance skills describe the complete Profile contract.

The complete-* evidence files describe this delivery; earlier group-* files document the grouping correction and other older files are historical. Independent review covers source and supplied runtime evidence rather than a separate Unity execution. Whole-Common verifier findings in unrelated legacy slots remain outside this change. No Git commit or push was requested.
