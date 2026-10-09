"""Regression check for the coverage classifier.

Runs the analysis on the equipment already in the project and compares it with the covered
parts authored in ArmorManifest.json. Every authored part must appear as confident or uncertain,
and a confident part that is not authored must be listed in TOLERATED (a known judgement call).

Run: python3 -I check_classification.py -- <Assets/Models folder> <ArmorManifest.json>
"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import equip_lib as L  # noqa: E402

TOLERATED = {"MaidBody": {"UpperArmLeft", "UpperArmRight"}}


def main():
    models, manifest = sys.argv[sys.argv.index("--") + 1:][:2]
    items = json.load(open(manifest))["items"]
    failures = []
    L.reset_scene()
    L.import_fbx(os.path.join(models, "Character Customization/Male/Male_Body_LP.fbx"))
    parts, head = L.body_objects()
    for entry in items:
        path = os.path.join(models, "Items/Equipaments", entry["models"][0] + ".fbx")
        new = L.import_fbx(path)
        mesh = L.prepare_item(new, entry["assetName"])
        result = L.analyze(mesh, parts, head)
        truth = set(entry["coveredParts"])
        seen = set(result["confident"]) | set(result["uncertain"])
        extra = set(result["confident"]) - truth - TOLERATED.get(entry["assetName"], set())
        missing = truth - seen
        status = "ok" if not extra and not missing else "FAIL"
        print("CHECK %-16s %-4s confident=%s uncertain=%s" % (entry["assetName"], status, result["confident"], result["uncertain"]))
        if status == "FAIL":
            failures.append((entry["assetName"], sorted(extra), sorted(missing)))
        import bpy
        bpy.data.objects.remove(mesh, do_unlink=True)
        for o in [o for o in bpy.data.objects if o.name not in L.BODY_PARTS and o.name != L.HEAD_PART and o.type == "MESH"]:
            bpy.data.objects.remove(o, do_unlink=True)
    assert not failures, "classification regressions: %s" % failures
    print("CHECK all ok")


if __name__ == "__main__":
    main()
