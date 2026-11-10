"""Read-only SV Loading animation research export (requires PyYAML).

Produces a proposed design.animations extension, not a publishable UI template.
Usage: python scripts/extract-loading-animation.py --project <SV project> --output <json>
This deliberately supports the inspected Loading clip, not arbitrary Unity animations.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

import yaml


def documents(path):
    text = path.read_text(encoding="utf-8-sig")
    text = re.sub(r"^%.*\n", "", text, flags=re.M)
    text = re.sub(r"^--- !u!\d+ &(-?\d+).*$", r"---\n_fileId: \1", text, flags=re.M)
    return list(yaml.safe_load_all(text))


def keys(curve, axis=None):
    def value(key, field):
        v = key[field]
        return v[axis] if axis else v
    return [{"time": k["time"], "value": value(k, "value"),
             "inTangent": value(k, "inSlope"), "outTangent": value(k, "outSlope"),
             "inWeight": value(k, "inWeight"), "outWeight": value(k, "outWeight"),
             "weightedMode": k["weightedMode"], "sourceTangentMode": k["tangentMode"]}
            for k in curve["m_Curve"]]


def extract(project):
    base = "Assets/data/@00Launch/Loading/loading/Loading_Anim_01"
    clip_path, controller_path = project / (base + ".anim"), project / (base + ".controller")
    clip = documents(clip_path)[0]["AnimationClip"]
    controller = documents(controller_path)
    states = [d for d in controller if "AnimatorState" in d]
    machines = [d for d in controller if "AnimatorStateMachine" in d]
    controllers = [d for d in controller if "AnimatorController" in d]
    if len(states) != 1 or len(machines) != 1 or len(controllers) != 1:
        raise ValueError("Controller changed: inspect states before extracting")
    state = states[0]["AnimatorState"]
    machine = machines[0]["AnimatorStateMachine"]
    ctrl = controllers[0]["AnimatorController"]
    if (state["m_Transitions"] or machine["m_AnyStateTransitions"] or
            machine["m_EntryTransitions"] or machine["m_ChildStateMachines"] or
            ctrl["m_AnimatorParameters"] or len(ctrl["m_AnimatorLayers"]) != 1 or
            machine["m_DefaultState"]["fileID"] != states[0]["_fileId"] or
            state["m_SpeedParameterActive"] or state["m_TimeParameterActive"]):
        raise ValueError("Controller requires more than a default looping state")
    for field in ("m_RotationCurves", "m_CompressedRotationCurves", "m_PositionCurves",
                  "m_ScaleCurves", "m_PPtrCurves", "m_Events"):
        if clip.get(field):
            raise ValueError("Unsupported source data: " + field)
    curves = []
    if len(clip["m_EulerCurves"]) != 1 or len(clip["m_FloatCurves"]) != 1:
        raise ValueError("Loading curve set changed; inspect source")
    euler = clip["m_EulerCurves"][0]
    uv = clip["m_FloatCurves"][0]
    if euler["path"] != "Ball" or uv["path"] != "RateBg/eff_jiantou_01" or uv["attribute"] != "m_UVRect.x":
        raise ValueError("Loading bindings changed; inspect source")
    # IDs are from the existing native-export import convention; remap them when instantiating.
    def binding(entry, node_id, path, component, prop, axis=None):
        curve = entry["curve"]
        return {"targetNodeId": node_id, "sourcePath": entry["path"],
                "bindingPathPreview": path, "component": component, "property": prop,
                "preWrapMode": curve["m_PreInfinity"], "postWrapMode": curve["m_PostInfinity"],
                "sourceRotationOrder": curve["m_RotationOrder"], "keys": keys(curve, axis)}
    for axis in "xyz":
        curves.append(binding(euler, "N0_5", "RateBg/Ball", "UnityEngine.Transform",
                              "localEulerAnglesRaw." + axis, axis))
    curves.append(binding(uv, "N0_3_0", "RateBg/Arrow", "UnityEngine.UI.RawImage", "m_UVRect.x"))
    settings = clip["m_AnimationClipSettings"]
    clip_guid = yaml.safe_load(Path(str(clip_path) + ".meta").read_text())["guid"]
    if state["m_Motion"]["guid"] != clip_guid:
        raise ValueError("Controller no longer references inspected clip")
    return {
        "status": "proposal-extracted-not-integrated",
        "source": {"clip": base + ".anim", "controller": base + ".controller",
                   "clipGuid": clip_guid, "clipSha256": hashlib.sha256(clip_path.read_bytes()).hexdigest(),
                   "controllerSha256": hashlib.sha256(controller_path.read_bytes()).hexdigest(),
                   "rawClipSettings": settings, "rawControllerDocuments": controller},
        "designAnimationsProposal": [{
            "id": "LoadingLoop", "hostSelector": {"animationHost": True},
            "clip": {"name": clip["m_Name"], "frameRate": clip["m_SampleRate"],
                     "startTime": settings["m_StartTime"], "stopTime": settings["m_StopTime"],
                     "loopTime": bool(settings["m_LoopTime"]), "wrapMode": clip["m_WrapMode"],
                     "curves": curves},
            "state": {"name": state["m_Name"], "default": True, "speed": state["m_Speed"],
                      "cycleOffset": state["m_CycleOffset"],
                      "writeDefaultValues": bool(state["m_WriteDefaultValues"])}}],
        "integrationRequirements": [
            "Resolve hostSelector to an explicit hostNodeId within the template before publishing.",
            "Map native-import node IDs to current template IDs; reject missing or ambiguous nodes.",
            "Derive Unity binding paths from node IDs relative to Animator; previews are not authoritative.",
            "Preserve animations in Studio validation, clone ID remapping, save and template publishing.",
            "Extend current UnityCLI ugui importer: full Keyframe tangents/weights, clip settings, default state and host binding.",
            "Verify curves and samples in Unity before database publication; this file is research evidence only."
        ]}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = extract(args.project)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "curves": 4, "status": result["status"]}))
