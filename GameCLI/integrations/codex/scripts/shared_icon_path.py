"""Resolve the Unity asset path for an exported UI sprite."""
from pathlib import Path

CLIENT = Path(__file__).resolve().parents[3] / "examples/unitys/development/client"


def exported_image_path(system, identifier):
    shared = f"Assets/Data/Widgets/Common/Icons/Imported/{system}/{identifier}.png"
    if (CLIENT / shared).is_file():
        return shared
    return f"Assets/Data/Widgets/{system}/Images/{identifier}.png"
