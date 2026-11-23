# R1 independent static recheck

Reviewer: `/root/code_recheck`, not involved in implementation, binding, configuration generation or R1 repairs. No Unity Editor operations or delivery edits. The verify-code implementation was inspected: it reads verified binding and source files, runs the local Roslyn analyzer and writes reports; it does not call the Unity bridge.

## Re-executed results

Eleven actual directory checks passed with exit 0 and zero error diagnostics: Login, Notice, Loading, Master, RedDot, Mail, Shop, Home, Settings, Function, Common. Each has fresh raw CLI output, JSON and source SHA256 inventory in this folder; `exit-codes.json` preserves command results.

Launch directory verification retains exit 2 because no Systems/Launch directory exists. This is the expected architecture (XLauncher is a package component), not a fabricated pass. Fresh supplemental `shared-analyzer.json` checks XLauncher.cs, PracticePlugin.cs and RuntimeSystem.cs directly and reports zero issues, with hashes. Together these independently reproduce clearance of the R1 syntax-policy errors.

Common's single OX017 review diagnostic is retained. Its UNITY_EDITOR-only HomeTextOutline.OnValidate branch was manually read: it calls base.OnValidate and Configure, is placed with lifecycle overrides, and adds no runtime lookup or external state. This does not claim shader runtime correctness or all possible compiler defines were analyzed.

## Master source review

- OnLoginSucceed rejects null/nonpositive IDs and repeat success before Load/AcceptIdentity/Save/Launch. AcceptIdentity revalidates positivity in the entity, then applies the stable PID attribute key. This corrects the previously reported invalid-PID source path; actual event-bus confirmation belongs to runtime acceptance.
- OnActive and OnDetive increment generation, reset loginAccepted and pair event subscriptions. Deferred activation and function-close capture generation; RuntimeSystem completion checks the overridden operationVersion as well as IsActive, covering the login transition completion's old-lifetime callback.
- Master Unload now explicitly unloads Login, Mail, Settings, Shop, Notice and Home. Function/RedDot remain plugin-owned as documented. Independent runtime must confirm deferred dispatcher removal and actual widget cleanup.
- Event handlers have null guards; requested types reject null, Master itself, abstract/open-generic and absent public parameterless constructor. Function gates remain checked before and after the deferred activation.
- No new confirmed static defect is raised. Direct generic activation remains a local coordinator API, not an authentication/security boundary; if the product requires every SystemActive to be login-gated, that separate contract needs runtime checking rather than assuming it from an offline login example.

Verdict: R1 static syntax-policy repair passed for the checked frozen files; source-level corrections are present. This report does not establish Unity compilation, lifecycle race behavior, prefab bindings, visual parity, or whole-system acceptance.
