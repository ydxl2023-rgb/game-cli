# Home entrance animation

The basic lobby now includes the retained UI portions of SV's designated
`RootView_Anim_01` opening animation. Character, seasonal, shader and decorative
3D effects remain outside the agreed basic template scope.

- 15 position/CanvasGroup curves; duration 1.2666667 seconds.
- Final positions are adjusted to the basic lobby layout.
- Unity assets: `HomeOpen.anim`, `Home.controller`, and the prefab controller reference.
- The default Open state runs automatically on each new Home instance.
- SvHome version 3 includes animation design data and native source materials.
- GameCLIApp revision 26 includes the mapped HomeEntrance animation.
- Web preview supports anchored position and CanvasGroup alpha, including restoration.

Validation: Unity compilation and animated UGUI import dry-run passed. The Studio
test suite passed 133 tests with 2 skips; its build succeeded. `runtime.json`
records two automatic openings without Animator.Play or manual sampling. The
TopLeft region moves and fades at 0.15 seconds and reaches its final layout by
0.4 seconds on both openings. `HomeEntranceProbe.cs` is archived evidence only;
the temporary probe was removed from the Unity project.

Independent animation review found no blocking defect in the 15-curve mapping,
timing, browser coordinate conversion or sampled replay. Runtime sampling directly
covers TopLeft, not every animated region. The opening callback precedes the first
Animator evaluation, so it does not establish first-rendered-frame appearance.
This report validates the animation change, not overall system acceptance.
