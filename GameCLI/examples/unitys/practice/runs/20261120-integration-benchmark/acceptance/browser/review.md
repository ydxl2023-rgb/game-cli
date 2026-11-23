# Read-only browser preview review

Scope: Practice project p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484, frozen v6 supplied by parent; Home/Shop browser rendering only is independently reviewed here. Reviewer previously changed NoticeText/UILoading/XLauncher/PracticePlugin, so does not claim independent implementation review of those files. No Unity operations, design saves, template publication or data mutation.

Method: existing Playwright with isolated headless installed Chrome, viewport 1600x1000, Studio canvas 1920x1080 at 46%. Every non-GET/HEAD/OPTIONS request is aborted; focused run blocked count 0. Open project relation-card preview, wait 1800 ms, capture settled screenshot, click actual replay control, collect 15 DOM style samples 100 ms apart, capture reopened screenshot, click stop and record restored DOM. Browser exceptions: none. Original Windows graphics capture failed FrameArrived/window capture timeout; initial bundled Chromium headless binary unavailable, existing Chrome worked.

Evidence: Home-settled.png, Home-reopened.png, Shop-settled.png, Shop-reopened.png; Home-evidence.json, Shop-evidence.json with raw samples, DOM style, text bounds/computed font, loaded font faces and HTTP status, stopped styles. focused-previews.cjs is reproducible. all-previews.cjs also captured Login/Notice/Loading/Mail/Settings/Launch screens, but these screenshots have not been reviewed as independent full visual acceptance. Initial summary.json changed counts contain non-data-node template DOM and MUST NOT be used as animation proof. focused-summary.json and focused Home/Shop evidence use .ui-node[data-node], superseding those counts.

## Home

- PASS narrow font-loading check: three actual font asset HTTP 200 responses, all document.fonts entries loaded. Text uses original GameCLI hash-named fonts; no browser exception. This does not verify Unity shader/BestFit equivalence.
- Observed static background, player info and edge navigation. NOT full layout pass: advertiser text overlaps shop label. T1Home0_6_3_0_2_1 (Label063021, 广告获取) has 30px font in 63.6501x26.0374 design box. Actual rect (444.907,550.687,29.522,12.076); shop label rect (426.315,558.856,60.364,21.230). Screenshot shows two-line ad text crossing shop label. This is fixed text, not missing runtime VM data.
- CurrencyOne/Two/Three image nodes have opacity 0; currency amount strings empty. Empty amounts cannot prove Unity runtime failure. Browser screenshot does not present all three currency groups. Must distinguish missing preview simulation data from transparent graphic subtree behavior; not waived as a passed layout requirement.
- Browser replay changes 60 Home node styles within 1.5 seconds (NEW nodes, ImgDoubleReward, BottomRight, entrance navigation). Raw per-node samples prove browser animation state progression. Some animated nodes are hidden by default, so this is NOT proof that all NEW/Left/drop effects are visually visible, nor numerical curve equivalence to Unity. First/reopen screenshots retained. Complete animation contract remains incomplete.

## Shop

- PASS narrow font resources check: same three assets HTTP 200 and loaded. No visible text in Shop root, so loaded resources alone cannot prove rendered typography correct.
- Actual preview is background plus three empty currency strips, not a populated shop: no category text/products/purchase dialog. TabTitle is empty in DOM. Dynamic VM/product data is not populated in browser preview; no claim that Unity purchasing fails.
- Focused replay sampling finds zero changing T1Shop node styles. Cannot claim automatic browser shop entrance animation passes. First-run broad sampler also included unrelated/undefined nodes; ignored.
- Companion pages, cell reuse, product popup and click feedback were not exercised; incomplete, not passed.

Overall: real browser evidence available, required full Home/Shop visual/animation acceptance INCOMPLETE with concrete Home fixed-text overlap finding. Acceptance agent owns defect classification and unified R2. No implementation changes performed.
