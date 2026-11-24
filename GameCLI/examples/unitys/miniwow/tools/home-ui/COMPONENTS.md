# 主页面组件结构（包内证据）

源坐标720×1560；visible只是序列化值，最终可见性还受控制器与运行时逻辑影响。

## Building/BuildingOpBubble
`ui://vl61mvdhicyxbk340t`，源尺寸 `[157, 106]`。
控制器：IsFinish, IsEnough, ShowArrow。
过渡：out, in。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [0, 0] / [157, 106] | ui://vl61mvdhicyxbk340u |
| Title | Text | [46, 6] / [64, 42] |  |
| Icon | Loader | [-21, -10] / [154, 157] |  |
| n4 | Image | [127, -2] / [37, 31] | ui://g1d8o2bxq2mybk340e |
| Cost | Text | [77, 51] / [43, 32] |  |
| Effect | Graph | [79, -53] / [0, 0] |  |

## Building/BuildingEntranceBubble
`ui://vl61mvdhtf72bk3407`，源尺寸 `[78, 83]`。
控制器：Enable, ShowArrow, Type, IsShow。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| EnterBtn | Component | [0, 0] / [78, 83] | ui://g1d8o2bxstj74y<br>ui://vl61mvdhq3pgbk340w |
| TalkBtn | Component | [0, 0] / 继承资源 | ui://vl61mvdhq3pgbk340y |
| Effect | Graph | [36, -60] / [0, 0] |  |
| n8 | Group | [0, -60] / [78, 143] |  |

## Building/TalkButton
`ui://vl61mvdhq3pgbk340y`，源尺寸 `[78, 77]`。
控制器：button。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n7 | Image | [0, 0] / 继承资源 | ui://vl61mvdhq3pgbk340v |
| title | Text | [15, 21] / [48, 32] | 交谈 |

## Common/CommonButton
`ui://g1d8o2bxstj74y`，源尺寸 `[200, 100]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [200, 100] |  |

## Building/FactoryBubble
`ui://vl61mvdhh10w5e`，源尺寸 `[98, 100]`。
控制器：Type, IsEdge, EdgeType, Enable, IsShow。
过渡：idle。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n5 | Image | [0, 0] / [98, 100] | ui://vl61mvdhlnczbk33z1 |
| n21 | Loader | [0, -6] / [98, 100] | ui://vl61mvdhk8kxbk33zh |
| arrow | Image | [-10, 1] / [109, 98] | ui://vl61mvdhfty7bk33z7 |
| n22 | Loader | [5, 0] / [98, 100] |  |
| Btn | Loader | [0, 0] / [98, 100] |  |
| n23 | Group | [-10, -6] / [113, 106] |  |

## Building/BuildingResTipsBubble
`ui://vl61mvdhe37rbk340z`，源尺寸 `[101, 111]`。
控制器：HideResCount, CanClick。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [34, 11] / [98, 100] | ui://vl61mvdhlnczbk33z1 |
| Icon | Loader | [12, 10] / [154, 157] |  |
| Count | Text | [28, 69] / [43, 32] |  |
| Button | Loader | [9, 12] / [84, 77] |  |

## Building/BuildingPeopleBubble
`ui://vl61mvdhi60bbk340o`，源尺寸 `[263, 69]`。
控制器：type。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n2 | Image | [11, 14] / 继承资源 | ui://vl61mvdhi60bbk340q |
| n0 | Loader | [0, 0] / [86, 92] | ui://vl61mvdhi60bbk340r |
| txt | Text | [76, 17] / [81, 34] |  |

## Building/BuildingTitleBubble
`ui://vl61mvdhq63lbk33zk`，源尺寸 `[100, 29]`。
控制器：Hide。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| title | Text | [4, -2] / [92, 32] | 旅店老板 |

## Main/MainPage
`ui://1pyb9685q2my4z`，源尺寸 `[720, 1560]`。
控制器：HideMainUI, IsGuide, MansionExpandBtnPanel。
过渡：Out, In, In_2, Out_2, FuncButton_Open, FuncButton_Close, Out_3, In_3, Tabs_In, Tabs_Out。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| TouchLayer | Component | [0, 0] / 继承资源 | ui://1pyb9685op293m |
| BubbleLayer | Component | [0, 0] / [720, 1560] | ui://1pyb96859gxf4m |
| EntranceBubbleLayer | Component | [0, 0] / [720, 1560] | ui://1pyb96859gxf4m |
| Joystick | Component | [110, 110] / 继承资源 | ui://1pyb9685myzx4n |
| JoystickDirCom | Component | [359, 940] / 继承资源 | ui://1pyb9685bmzw4x |
| TaskCom | Component | [0, 1259] / 继承资源 | ui://1pyb9685ba1ybk34c5 |
| SideTaskCom | Component | [0, 1170] / 继承资源 | ui://1pyb9685vakfbk34ke |
| JoystickPivot | Loader | [359, 1219] / [2, 2] |  |
| n197 | Image | [0, 0] / [720, 187] | ui://g1d8o2bxmn96bk34ct |
| ResList | List | [18, 81] / [481, 48] | ui://g1d8o2bxq4ovbk34az |
| LeaderCom | Component | [10, 136] / 继承资源 | ui://1pyb9685oljr5p |
| GameSettingBtn | Loader | [10, 130] / [115, 115] |  |
| FuncBtn | Component | [0, 1252] / 继承资源 | ui://1pyb9685wag25a |
| ReportCom | Component | [0, 1351] / 继承资源 | ui://g1d8o2bximiu5o |
| PreviewBtn | Component | [382, 305] / 继承资源 | ui://1pyb9685dzov5o<br>ui://1pyb9685wag258 |
| ManorWarTipsCom | Component | [360, 1077] / 继承资源 | ui://1pyb9685ssycbk34dl |
| TipsCom | Component | [357, 1169] / 继承资源 | ui://1pyb9685ssycbk34dl |
| n190 | Image | [0, 1422] / [720, 139] | ui://1pyb9685fr4hbk349k |
| n211 | Image | [508, 1426] / 继承资源 | ui://1pyb9685m672bk34ej |
| n212 | Image | [0, 1426] / 继承资源 | ui://1pyb9685m672bk34ej |
| n214 | Group | [0, 1422] / [720, 139] |  |
| ExploreBtn | Component | [-5, 1426] / 继承资源 | ui://1pyb9685m672bk34ei<br>ui://1pyb9685m672bk34ea |
| Tabs | List | [130, 1426] / [463, 135] | ui://1pyb9685q65dbk3491 |
| WorldBtn | Component | [580, 1427] / 继承资源 | ui://1pyb9685m672bk34ei<br>ui://1pyb9685axf5bk34ia |
| ExFunctionBtn1 | Component | [485, 191] / 继承资源 | ui://1pyb9685m672bk34e5<br>ui://1pyb9685dyzrbk34hl |
| ExFunctionBtn2 | Component | [552, 191] / 继承资源 | ui://1pyb9685m672bk34e5<br>ui://1pyb9685ohyubk34i8 |
| ExFunctionBtn3 | Component | [621, 191] / 继承资源 | ui://1pyb9685m672bk34e5<br>ui://1pyb9685m672bk34e2 |
| n206 | Component | [631, 1081] / [75, 71] | ui://g1d8o2bxstj74y<br>ui://1pyb9685m672bk34e9 |
| RecruitBtn | Component | [631, 1260] / 继承资源 | ui://1pyb9685m672bk34em |
| n203 | Image | [629, 249] / [73, 550] | ui://1pyb9685lhvt4u |
| FunctionBtns | List | [634, 275] / [73, 496] | ui://1pyb9685dzov5o |
| n210 | Group | [629, 249] / [78, 550] |  |
| ActivityButtonListCom | Component | [21, 278] / 继承资源 | ui://1pyb9685m672bk34en |
| MineWarCom | Component | [-1, 473] / 继承资源 | ui://1pyb9685nm79bk34h2 |
| MansionFuncBtnsPanel | Component | [0, 0] / 继承资源 | ui://1pyb9685pywabk34hf |
| n215 | Group | [-5, 0] / [729, 1562] |  |

## Main/MansionFuncBtnsPanel
`ui://1pyb9685pywabk34hf`，源尺寸 `[720, 1560]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| MansionFuncBtnList | List | [179, 1018] / [252, 406] | ui://1pyb9685pywabk34he |

## Main/MansionFunctionBtn
`ui://1pyb9685pywabk34he`，源尺寸 `[252, 103]`。
控制器：isLock。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n219 | Image | [0, 0] / [252, 103] | ui://1pyb9685pywabk34hb |
| n220 | Image | [13, 12] / 继承资源 | ui://1pyb9685pywabk34hc |
| RedDot | Component | [218, 5] / 继承资源 | ui://g1d8o2bxppaebk343s |
| icon | Loader | [34, 19] / [86, 92] | ui://14pwp6qngcmx1o |
| title | Text | [94, 28] / [124, 39] | 圣印 |
| n224 | Image | [13, 12] / 继承资源 | ui://1pyb9685f23nbk34ic |
| n225 | Image | [52, 33] / 继承资源 | ui://g1d8o2bxhojkbk3449 |

## Common/RedDot
`ui://g1d8o2bxppaebk343s`，源尺寸 `[27, 28]`。
控制器：hide。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Loader | [0, 0] / [27, 28] | ui://g1d8o2bxppaebk343r |

## Main/MineWarCom
`ui://1pyb9685nm79bk34h2`，源尺寸 `[222, 148]`。
控制器：state。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n5 | Image | [-11, 117] / [226, 31] | ui://1pyb9685sz18bk34bo |
| n0 | Image | [-13, 0] / [235, 122] | ui://g1d8o2bxic9l1geiuw9 |
| n1 | Image | [-11, 7] / [235, 50] | ui://g1d8o2bxic9l1geiuw7 |
| n2 | Image | [-45, -36] / 继承资源 | ui://1pyb9685nm79bk34h5 |
| n3 | Image | [-20, 7] / 继承资源 | ui://1pyb9685nm79bk34h6 |
| n4 | Text | [77, 15] / [136, 32] | 矿山保卫战！ |
| timeTxt | Text | [-9, 118] / [215, 26] |  |
| rewardList | List | [63, 60] / [490, 155] | ui://g1d8o2bxlhvtbk33wt |
| staminaBar | Component | [80, 71] / 继承资源 | ui://1pyb9685nm79bk34h9 |
| staminaLoader | Loader | [49, 59] / [50, 50] | ui://p5rgfp2dnq541geiuzj |
| rankTxt | Text | [86, 56] / [100, 56] |  |
| n11 | Component | [199, -4] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/MineStaminaBar
`ui://1pyb9685nm79bk34h9`，源尺寸 `[126, 27]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [0, 0] / [126, 27] | ui://1pyb9685nm79bk34h7 |
| bar | Image | [0, 0] / [126, 27] | ui://1pyb9685nm79bk34h8 |
| title | Text | [0, -4] / [126, 34] |  |

## Common/CommonItem
`ui://g1d8o2bxlhvtbk33wt`，源尺寸 `[154, 157]`。
控制器：quality, num, tag, Turn。
过渡：anim。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| border | Loader | [0, 0] / [154, 157] | ui://g1d8o2bxppaebk343k |
| icon | Loader | [0, 0] / [154, 157] |  |
| n24 | Image | [79, 107] / 继承资源 | ui://g1d8o2bxbwitbk3439 |
| starList | List | [-3, 111] / [160, 36] | ui://g1d8o2bxu8iy2f |
| title | Text | [140, 124] / [126, 46] |  |
| tag | Loader | [5, 5] / [48, 48] |  |
| eggStar | Component | [-8, 0] / 继承资源 | ui://g1d8o2bxjg5i1geiv1c |
| tierLabel | Component | [-8, 0] / 继承资源 | ui://g1d8o2bxoyj41geiuwn |
| n21 | Image | [-8, 108] / 继承资源 | ui://g1d8o2bxh87wbk34qc |
| effect | Graph | [77, 81] / [0, 0] |  |
| tagLoader | Loader | [67, 20] / [69, 37] |  |

## Common/ItemTier
`ui://g1d8o2bxoyj41geiuwn`，源尺寸 `[69, 42]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n29 | Image | [0, 0] / 继承资源 | ui://g1d8o2bxzp4obk34j2 |
| title | Text | [0, 0] / [69, 37] |  |

## Common/EggStar
`ui://g1d8o2bxjg5i1geiv1c`，源尺寸 `[69, 42]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n29 | Image | [0, 0] / 继承资源 | ui://g1d8o2bxzp4obk34j2 |
| n30 | Image | [37, 6] / 继承资源 | ui://g1d8o2bxppaebk343x |
| title | Text | [7, 3] / [32, 32] |  |

## Common/HeroStar
`ui://g1d8o2bxu8iy2f`，源尺寸 `[40, 40]`。
控制器：tier。
过渡：anim。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [40, 40] | ui://g1d8o2bxd23kbk34g2 |

## Main/ActivityButtonListCom
`ui://1pyb9685m672bk34en`，源尺寸 `[146, 178]`。
控制器：Show。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| Activities | List | [0, 0] / [76, 82] | ui://1pyb9685m672bk34eq |
| DrawerBtn | Component | [288, 94] / 继承资源 | ui://1pyb9685m672bk34eq<br>更多<br>ui://1pyb9685p5kgbk34kn |

## Main/ActivityButton
`ui://1pyb9685m672bk34eq`，源尺寸 `[84, 84]`。
控制器：button。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [84, 84] | ui://1pyb9685m672bk34eo |
| effect | Graph | [42, 42] / [0, 0] |  |
| title | Text | [18, 52] / [48, 32] |  |
| RedDot | Component | [57, 2] / 继承资源 | ui://g1d8o2bxppaebk343s |
| time | Text | [18, 53] / [48, 32] |  |

## Main/MainButton
`ui://1pyb9685dzov5o`，源尺寸 `[66, 58]`。
控制器：button。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [66, 58] |  |
| RedDot | Component | [47, -16] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/MainButton3
`ui://1pyb9685m672bk34em`，源尺寸 `[78, 76]`。
控制器：button。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [78, 76] | ui://1pyb9685m672bk34ek |
| red | Component | [49, -4] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/ExFunctionButton
`ui://1pyb9685m672bk34e5`，源尺寸 `[49, 46]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [49, 46] | ui://1pyb9685m672bk34e3 |
| redDot | Component | [30, -7] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/MainBigTab
`ui://1pyb9685m672bk34ei`，源尺寸 `[144, 135]`。
控制器：button, Lock。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [20, -3] / [102, 110] |  |
| title | Loader | [37, -7] / [66, 36] | ui://1pyb9685m672bk34eb |
| RedDot | Component | [95, 2] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/MainTab
`ui://1pyb9685q65dbk3491`，源尺寸 `[110, 135]`。
控制器：button, Lock, isExpand。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [24, 8] / [63, 67] | ui://1pyb9685m672bk34ee |
| RedDot | Component | [60, 2] / 继承资源 | ui://g1d8o2bxppaebk343s |
| DeployRedDot | Component | [46, 0] / 继承资源 | ui://1pyb9685iplnbk34fu |
| title | Text | [0, 63] / [110, 42] |  |
| n18 | Image | [0, 0] / [110, 135] | ui://g1d8o2bxy54f1c |
| expandIcon | Image | [26, 30] / 继承资源 | ui://g1d8o2bxpywa1geiuzz |
| expandText | Text | [0, 63] / [110, 42] | 收起 |
| expand | Group | [0, 0] / [110, 135] |  |

## Main/DeployRedDot
`ui://1pyb9685iplnbk34fu`，源尺寸 `[73, 34]`。
控制器：hide。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n16 | Image | [0, 0] / 继承资源 | ui://1pyb9685iplnbk34ft |
| n17 | Text | [7, 3] / [58, 26] | 可上阵 |

## Main/MainTipsCom
`ui://1pyb9685ssycbk34dl`，源尺寸 `[353, 80]`。
控制器：type。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n197 | Image | [153, -8] / [157, 83] | ui://1pyb9685rz8fbk34kq |
| icon | Loader | [181, -31] / [154, 157] |  |
| txt | Text | [170, 41] / [103, 29] |  |
| n200 | Image | [0, 11] / 继承资源 | ui://1pyb9685ssycbk34dj |
| txt2 | Text | [58, 25] / [144, 29] | 队长已发起挑战 |
| n1 | Component | [282, 11] / [60, 57] | ui://g1d8o2bxfsvg1geiuwd<br>ui://1pyb9685mix02n |
| btn | Loader | [153, 0] / [200, 80] |  |
| btn2 | Loader | [0, 1] / [353, 80] |  |

## Common/CommonButton2
`ui://g1d8o2bxfsvg1geiuwd`，源尺寸 `[100, 100]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [100, 100] |  |

## Common/ReportCom
`ui://g1d8o2bximiu5o`，源尺寸 `[720, 71]`。
控制器：Module。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [0, 0] / [720, 71] | ui://g1d8o2bxq65dbk348v |
| Contents | List | [32, 9] / [623, 54] | ui://g1d8o2bximiu5p |
| n2 | Image | [660, 16] / 继承资源 | ui://g1d8o2bxfr4hbk34cj |

## Common/ReportContent
`ui://g1d8o2bximiu5p`，源尺寸 `[623, 31]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| title | Text | [68, 0] / [555, 29] | [color=#d1a47d]安度因.洛萨：[/color]出售大法师 |
| tag | Component | [0, 0] / 继承资源 | ui://g1d8o2bxgcmx1geiuxr |

## Common/TalkLabel
`ui://g1d8o2bxgcmx1geiuxr`，源尺寸 `[61, 31]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n1 | Image | [0, 0] / 继承资源 | ui://g1d8o2bxgcmx1geiuxq |
| title | Text | [10, 2] / [40, 26] |  |

## Main/FuncButton
`ui://1pyb9685wag25a`，源尺寸 `[720, 309]`。
控制器：button, type, hide。
过渡：in, out。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Graph | [0, 269] / [720, 40] |  |
| n8 | Image | [0, 98] / [720, 211] | ui://g1d8o2bxgqi4g1f |
| n9 | Image | [0, 1] / 继承资源 | ui://g1d8o2bxq2mybk3402 |
| n10 | Image | [559, -4] / 继承资源 | ui://g1d8o2bxh1mobk348d |
| Button | Component | [220, 139] / 继承资源 | ui://g1d8o2bxlhvtbk33ws<br>招募 |
| Button2 | Component | [382, 139] / 继承资源 | ui://g1d8o2bxlhvtbk33ws<br>招募 |
| RedDot | Component | [472, 130] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Common/CommonButtonBig
`ui://g1d8o2bxlhvtbk33ws`，源尺寸 `[280, 111]`。
控制器：button, color, line, enough。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| back | Loader | [0, 0] / [280, 111] | ui://g1d8o2bxq2mybk340g |
| title | Text | [139, 50] / [210, 62] | 确定 |
| icon | Loader | [107, 56] / [38, 38] |  |
| num | Text | [149, 55] / [22, 39] |  |
| numRed | Text | [149, 55] / [22, 39] |  |
| n12 | Group | [107, 55] / [64, 39] |  |

## Main/LeaderCom
`ui://1pyb9685oljr5p`，源尺寸 `[460, 130]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n135 | Image | [69, 50] / 继承资源 | ui://1pyb9685lhvt4s |
| playerHead | Component | [2, -5] / [160, 160] | ui://g1d8o2bxraa1bk34ip |
| playerName | Text | [119, 4] / [180, 32] |  |
| guildIcon | Loader | [108, 54] / [170, 39] |  |
| n153 | Image | [288, 50] / 继承资源 | ui://1pyb9685m672bk34dz |
| n149 | Image | [306, 61] / 继承资源 | ui://g1d8o2bxql4nbk348l |
| playerPower | Text | [339, 53] / [94, 39] |  |
| redDot | Component | [272, 38] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Common/PlayerHead
`ui://g1d8o2bxraa1bk34ip`，源尺寸 `[160, 160]`。
控制器：empty, type, IsShowBorder。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n53 | Image | [-33, -34] / 继承资源 | ui://g1d8o2bxmm2sbk34q4 |
| head | Component | [-33, -33] / 继承资源 | ui://g1d8o2bxot0l1geiuxo |
| n54 | Image | [-33, -34] / 继承资源 | ui://g1d8o2bxmm2sbk34q4 |
| border | Loader | [-33, -34] / [226, 226] | ui://p5rgfp2dfgz01geiv2g |
| effect | Graph | [80, 80] / [0, 0] |  |
| redDot | Component | [111, 8] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Common/PlayerHeadMask
`ui://g1d8o2bxot0l1geiuxo`，源尺寸 `[226, 226]`。
控制器：empty, type。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| icon | Loader | [0, 0] / [226, 226] |  |
| wxIcon | Loader | [51, 53] / [123, 123] |  |
| n52 | Image | [0, 0] / 继承资源 | ui://y9cg3f1jq63l13h2 |
| n53 | Image | [0, 0] / 继承资源 | ui://g1d8o2bxmm2sbk34q4 |

## Common/CommonRes
`ui://g1d8o2bxq4ovbk34az`，源尺寸 `[146, 48]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n36 | Image | [18, 6] / [128, 36] | ui://g1d8o2bxfr4hbk34ci |
| icon | Loader | [0, 5] / [38, 38] |  |
| plus | Image | [26, 21] / 继承资源 | ui://g1d8o2bxq4ovbk34ay |
| title | Text | [45, 10] / [89, 29] |  |
| red | Component | [125, -9] / 继承资源 | ui://g1d8o2bxppaebk343s |

## Main/SideTaskCom
`ui://1pyb9685vakfbk34ke`，源尺寸 `[398, 86]`。
控制器：status。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| taskBtn | Component | [58, 12] / 继承资源 | ui://1pyb9685vakfbk34kf |
| Pivot | Loader | [207, 44] / [2, 2] |  |
| n8 | Image | [16, 2] / 继承资源 | ui://g1d8o2bxppaebk343p |
| n6 | Image | [10, -4] / 继承资源 | ui://1pyb9685vakfbk34kh |
| n7 | Image | [10, -4] / 继承资源 | ui://1pyb9685vakfbk34kj |
| reward | Loader | [20, 5] / [154, 157] |  |
| rewardCount | Text | [37, 53] / [42, 29] |  |
| n5 | Image | [40, -1] / 继承资源 | ui://1pyb9685vakfbk34kk |

## Main/SideTaskButton
`ui://1pyb9685vakfbk34kf`，源尺寸 `[340, 62]`。
控制器：status, button, quality, hide。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n161 | Image | [-6, -6] / 继承资源 | ui://1pyb9685vakfbk34kg |
| n162 | Image | [-6, -6] / 继承资源 | ui://1pyb9685vakfbk34ki |
| doEffect | Graph | [0, 0] / [0, 0] |  |
| finishEffect | Graph | [0, 1] / [0, 0] |  |
| descText | Text | [50, 17] / [130, 29] |  |
| valueText | Text | [183, 16] / [88, 29] | ( 50/50 ) |
| n2 | Group | [0, 0] / [271, 46] |  |
| n4 | Text | [104, 11] / [116, 39] | 敬请期待 |

## Main/TaskCom
`ui://1pyb9685ba1ybk34c5`，源尺寸 `[398, 86]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| taskBtn | Component | [58, 12] / 继承资源 | ui://1pyb9685lhvt4v |
| chapterBtn | Component | [14, 0] / 继承资源 | ui://1pyb9685ba1ybk34c7 |
| RedDot | Component | [67, -4] / 继承资源 | ui://g1d8o2bxppaebk343s |
| Pivot | Loader | [207, 44] / [2, 2] |  |

## Main/ChapterButton
`ui://1pyb9685ba1ybk34c7`，源尺寸 `[86, 86]`。
控制器：IsLoop。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n2 | Image | [0, 0] / 继承资源 | ui://1pyb9685ba1ybk34c6 |
| n3 | Image | [-3, 60] / [92, 27] | ui://g1d8o2bxih3sbk34gc |
| title | Text | [21, 58] / [43, 29] |  |

## Main/TaskButton
`ui://1pyb9685lhvt4v`，源尺寸 `[340, 62]`。
控制器：status, button, quality, hide。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [0, 0] / [340, 62] | ui://1pyb9685fr4hbk349i |
| n1 | Image | [0, 0] / [340, 62] | ui://1pyb9685fr4hbk34a0 |
| effect | Graph | [0, 1] / [0, 0] |  |
| descText | Text | [50, 17] / [130, 29] |  |
| valueText | Text | [183, 16] / [88, 29] | ( 50/50 ) |
| n2 | Group | [0, 1] / [271, 45] |  |
| n4 | Text | [104, 11] / [116, 39] | 敬请期待 |

## Main/JoystickDirectionCom
`ui://1pyb9685bmzw4x`，源尺寸 `[37, 200]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [0, 0] / 继承资源 | ui://1pyb9685bmzw4w |

## Main/JoystickCom
`ui://1pyb9685myzx4n`，源尺寸 `[214, 214]`。
控制器：ShowType。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
| n0 | Image | [21, 21] / 继承资源 | ui://1pyb9685myzx4o |
| Joystick | Image | [0, 0] / 继承资源 | ui://1pyb9685myzx4p |

## Main/BubbleLayer
`ui://1pyb96859gxf4m`，源尺寸 `[720, 1560]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|

## Main/TouchLayer
`ui://1pyb9685op293m`，源尺寸 `[720, 1560]`。
控制器：。
过渡：。

| 子节点 | 类型 | 位置 / 尺寸 | 引用 / 文本 |
|---|---|---|---|
