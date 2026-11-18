# Home drop promotion VM binding

Removed UIHome.RefreshPresentation(HomeEntity). HomeSystem maps entity state into VMHome.DropSprite, DropSize and DropVisible using existing SyncSprite, SyncSizeDelta and SyncVisible. UIHome.GetDropPresentation only reads authored serialized artwork and computes native size from the target Canvas reference pixels and Sprite pixels per unit. No ISyncAction added. Notice layout is unchanged.

Home prefab retains the original centered anchors and animation target. Three appended Metadata fields bind the original ImgDoubleReward node. Double artwork is 214x125; Triple is 194x104 at the reference Canvas settings.

Compiled without errors. Live None/Double/Triple/None/Triple transitions and identical tests after close/reopen passed, with original state restored. Inspector selection preserves all 315 live bindings. Final Console has zero errors. A separate reviewer checked code and native-size reasoning without independently running Unity.

Published SvHome v28 and GameCLIApp revision 58, preserving 71 navigation actions. Source generation reports and Home development rules are updated. Runtime JSON evidence accompanies this report. No commit or push requested.
