"""Import original settings animation curves through UnityCLI without editing native YAML."""
import json
import pathlib
import subprocess
import sys

base = pathlib.Path(__file__).resolve().parents[1]
folder = base / ".data/imports/sv-settings"
project = str(base.parents[1] / "examples/unitys/development/client")
source = pathlib.Path("G:/Works/SV/Development/Project")
cli = "G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe"
archive = json.loads((folder / "source/main/unity-prefab.json").read_text(encoding="utf-8-sig"))
design = json.loads((folder / "main/template.json").read_text(encoding="utf8"))["design"]
nodes = {node["id"]: node for node in design["nodes"]}
names = {}
for node in archive["nodes"]:
    names[node["path"]] = names[node["parent"]] + "/" + node["name"] if node["parent"] else ""

def native(identifier):
    node = nodes[identifier]
    return native(node["parent"]) + "/" + node["name"] if node["parent"] else node["name"]

def invoke(*args):
    result = subprocess.run([cli, "--project", project, "--format", "json", *args], capture_output=True, encoding="utf-8-sig")
    assert result.stdout, result.stderr
    response = json.loads(result.stdout)
    assert response["ok"], response
    return response["data"]

clips = [
    ("Sound", "0", "Assets/data/UI/Set/@SetView/Effect/SetSystemView/SetSystemView_Anim_01.anim", "SettingsSound"),
    ("Open", "0/2", "Assets/data/UI/Public/@Effect/Common/Animation_Common/BackGather_Animation_001.anim", "SettingsBack"),
]

inventory = []
controllers = set()
attachments = []
inspection = invoke("prefab", "inspect", "--prefab", "Assets/Data/Widgets/Settings/Settings.prefab", "--recursive")
animators = {n["path"] for n in inspection["nodes"] if any(c["type"].split(".")[-1] == "Animator" for c in n["components"])}
for state, host, filename, controller in clips:
    src = source / filename
    dst = "Assets/Data/Widgets/Settings/" + state + ".anim"
    invoke("asset", "import", "--src", str(src), "--dst", dst)
    exported = invoke("animator", "clip", "export-json", "--clip", dst)
    data = json.loads(exported["details"][0])
    keep, skipped = [], []
    for curve in data["curves"]:
        source_name = names[host] + ("/" + curve["path"] if curve["path"] else "")
        target = next((key for key, value in names.items() if value == source_name), None)
        identifier = "Settings" + target.replace("/", "_") if target else None
        if identifier not in nodes:
            skipped.append(curve["path"])
            continue
        host_native = "Settings/Animator" if host == "0" else native("Settings" + host.replace("/", "_"))
        target_native = native(identifier)
        curve["path"] = target_native[len(host_native):].lstrip("/")
        if curve["component"] == "UnityEngine.MonoBehaviour" and curve["property"].startswith("m_Color"):
            curve["component"] = "UnityEngine.UI.Text" if nodes[identifier]["type"] == "Text" else "UnityEngine.UI.Image"
        keep.append(curve)
    data["curves"] = keep
    path = folder / (state + "-native.json")
    path.write_text(json.dumps(data, ensure_ascii=False), encoding="utf8")
    invoke("animator", "clip", "import-json", "--clip", dst, "--json-file", str(path), "--dry-run")
    invoke("animator", "clip", "import-json", "--clip", dst, "--json-file", str(path))
    controller_path = "Assets/Data/Widgets/Settings/" + controller + ".controller"
    if controller not in controllers:
        if not (pathlib.Path(project) / controller_path).exists():
            invoke("animator", "controller", "create", "--controller", controller_path)
        controllers.add(controller)
    info = invoke("animator", "inspect", "--controller", controller_path)
    if not any("state " + state + " motion=" in line for line in info["details"]):
        invoke("animator", "state", "add", "--controller", controller_path, "--state", state, "--clip", dst)
    q = "Settings/Animator" if host == "0" else native("Settings" + host.replace("/", "_"))
    attachments.append({"path": q, "controller": controller_path, "sound": host == "0"})
    if '--prepare-only' not in sys.argv:
        if q not in animators:
            invoke("components", "add", "--asset", "Assets/Data/Widgets/Settings/Settings.prefab", "--path", q, "--component", "Animator")
            animators.add(q)
        invoke("modify", "--asset", "Assets/Data/Widgets/Settings/Settings.prefab", "--path", q, "--component", "Animator", "--property", "m_Controller", "--value", controller_path)
        if host == "0":
            invoke("modify", "--asset", "Assets/Data/Widgets/Settings/Settings.prefab", "--path", "Settings", "--component", "UISettings", "--property", "soundAnimation", "--value", "@Settings/Animator#Animator")
    inventory.append({"state": state, "source": str(src), "clip": dst, "curves": len(keep), "staleSourcePaths": sorted(set(skipped))})
    print(state, len(keep), flush=True)
(folder / "native-animation-inventory.json").write_text(json.dumps(inventory, indent=2), encoding="utf8")


(folder / "animation-attachments.json").write_text(json.dumps(attachments, indent=2), encoding="utf8")
