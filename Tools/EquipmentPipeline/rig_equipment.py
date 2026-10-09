"""Rigs a raw equipment model to the project skeleton and exports a skinned FBX.

Two phases, so a human confirms the classification before anything is written:

  1. --analyze-only  measures the model against the character body and writes a JSON report.
  2. (default)       rigs the model with the confirmed covered parts and exports the FBX.

Run with the bpy module:  python3 -I rig_equipment.py -- <args>
Run with Blender:         blender -b -P rig_equipment.py -- <args>
"""
import argparse
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy  # noqa: E402
import equip_lib as L  # noqa: E402

PAIR_MIRROR = {
    "HandLeft": "HandRight", "HandRight": "HandLeft",
    "FootLeft": "FootRight", "FootRight": "FootLeft",
    "UpperArmLeft": "UpperArmRight", "UpperArmRight": "UpperArmLeft",
    "LegLeft": "LegRight", "LegRight": "LegLeft",
}
BODY_FILES = {
    "male": "Character Customization/Male/Male_Body_LP.fbx",
    "female": "Character Customization/Female/Female_Body_LP.fbx",
}
MAX_TRIANGLES = 15000
MAX_ACCEPTED_DISTANCE = 0.10


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True, help="raw FBX from the Equipaments root")
    p.add_argument("--models", required=True, help="Assets/Models folder (holds Character Customization)")
    p.add_argument("--out", required=True, help="Equipaments folder that receives the region subfolder")
    p.add_argument("--name", help="item name, defaults to the input file name")
    p.add_argument("--body", default="auto", choices=["auto", "male", "female"])
    p.add_argument("--covered", help="comma list of confirmed BodyPart names")
    p.add_argument("--mirror", default="auto", choices=["auto", "yes", "no"])
    p.add_argument("--analyze-only", action="store_true")
    p.add_argument("--force", action="store_true", help="ignore the distance sanity check")
    p.add_argument("--head", action="store_true", help="treat the model as headgear even if the detector disagrees")
    p.add_argument("--material", help="material name (set name) written to the FBX, defaults to the item name")
    p.add_argument("--report", required=True)
    return p.parse_args(argv)


def load_scene(args, gender, name):
    """Imports the reference body and the item, returns (armature, parts, head, item)."""
    L.reset_scene()
    body_objs = L.import_fbx(os.path.join(args.models, BODY_FILES[gender]))
    armature = next(o for o in body_objs if o.type == "ARMATURE")
    item_objs = L.import_fbx(args.input)
    item = L.prepare_item(item_objs, name)
    parts, head = L.body_objects()
    return armature, parts, head, item


def pick_body(args, name):
    """Measures the item against both bodies and returns the closest gender plus both results."""
    results = {}
    for gender in ("male", "female"):
        _, parts, head, item = load_scene(args, gender, name)
        results[gender] = L.analyze(item, parts, head)
    best = min(results, key=lambda g: results[g]["mean_distance"])
    return best, results


def mirror_parts(parts):
    """Adds the opposite side of every paired part."""
    out = set(parts)
    for p in parts:
        out.add(PAIR_MIRROR.get(p, p))
    return sorted(out)


def count_triangles(item):
    item.data.calc_loop_triangles()
    return len(item.data.loop_triangles)


def ensure_uv(item, warnings):
    if item.data.uv_layers:
        return
    bpy.ops.object.select_all(action="DESELECT")
    item.select_set(True)
    bpy.context.view_layer.objects.active = item
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    warnings.append("model had no UV: generated with Smart UV Project (replace if you paint your own)")


def main():
    args = parse_args()
    name = args.name or os.path.splitext(os.path.basename(args.input))[0]
    report = {"name": name, "input": os.path.basename(args.input), "warnings": []}

    if args.body == "auto":
        gender, both = pick_body(args, name)
        report["body_distances"] = {g: both[g]["mean_distance"] for g in both}
    else:
        gender = args.body
    report["body"] = gender

    armature, parts, head, item = load_scene(args, gender, name)
    analysis = L.analyze(item, parts, head)
    report["analysis"] = analysis
    report["triangles"] = count_triangles(item)
    report["materials"] = [m.name for m in item.data.materials if m]
    report["has_uv"] = bool(item.data.uv_layers)

    if analysis["mean_distance"] > MAX_ACCEPTED_DISTANCE and not args.force:
        report["warnings"].append(
            "model is far from the body (mean %.3f): scale or axes differ from the reference" % analysis["mean_distance"]
        )
        report["status"] = "rejected"
        write_report(args, report)
        return

    covered = [c for c in args.covered.split(",") if c] if args.covered else list(analysis["confident"])
    report["covered_proposed"] = analysis["confident"]
    report["covered_uncertain"] = analysis["uncertain"]
    one_sided = L.is_one_sided(item)
    report["one_sided"] = one_sided
    if report["triangles"] > MAX_TRIANGLES:
        report["warnings"].append("%d triangles is heavy for a chibi equipment piece" % report["triangles"])

    head_vote = analysis["votes"].get("Head", 0.0)
    is_head = args.head or (not covered and head_vote >= 0.5)
    if is_head:
        report["region"] = "Head"
        report["covered"] = []
        report["slot"], report["folder"] = "HeadTop", "Head"
        report["mirrored"] = False
        report["weight_reference"] = ["Head"]
        return finish_item(args, report, armature, item, name, gender, rig="head")
    if not covered:
        report["status"] = "unclassified"
        report["warnings"].append("no body region is clearly covered: pass --covered to decide")
        write_report(args, report)
        return

    do_mirror = args.mirror == "yes" or (
        args.mirror == "auto" and one_sided and any(c in PAIR_MIRROR and c.endswith(("Left", "Right")) for c in covered)
        and not any(c in covered for c in ("Torso", "Pelvis"))
    )
    if do_mirror:
        covered = mirror_parts(covered)
    report["mirrored"] = do_mirror
    report["covered"] = sorted(covered)
    reference = set(covered) | set(analysis["uncertain"]) | set(analysis["near"])
    if do_mirror:
        reference = set(mirror_parts(reference))
    report["weight_reference"] = sorted(reference)
    slot, folder = L.slot_for(covered)
    report["slot"], report["folder"] = slot, folder

    return finish_item(args, report, armature, item, name, gender, rig="body", covered=covered, reference=reference, do_mirror=do_mirror, folder=folder)


def finish_item(args, report, armature, item, name, gender, rig, covered=None, reference=None, do_mirror=False, folder=None):
    """Writes the analysis report or rigs and exports the item."""
    if args.analyze_only:
        report["status"] = "analyzed"
        write_report(args, report)
        return

    if do_mirror:
        L.mirror_in_place(item)
        item = bpy.context.view_layer.objects.active
        item.name = name
    ensure_uv(item, report["warnings"])
    L.name_materials(item, args.material or name)
    report["materials_out"] = [m.name for m in item.data.materials if m]

    if rig == "head":
        report["weights"] = L.weight_to_bone(item, armature, "Head")
        folder = "Head"
    else:
        report["weights"] = L.transfer_weights(item, [parts_of(reference)[c] for c in sorted(reference)], armature)

    for obj in list(bpy.data.objects):
        if obj.type == "MESH" and obj is not item:
            bpy.data.objects.remove(obj, do_unlink=True)
    armature.name = "Armature"
    L.attach_armature(item, armature)

    out_dir = os.path.join(args.out, folder)
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, name + ".fbx")
    L.export_fbx(out_path, [item, armature])
    report["output"] = out_path
    report["status"] = "exported"
    write_report(args, report)


def parts_of(_reference):
    """Returns the Body_* objects of the open scene keyed by BodyPart name."""
    return L.body_objects()[0]


def write_report(args, report):
    with open(args.report, "w") as f:
        json.dump(report, f, indent=2)
    print("STATUS", report.get("status"), json.dumps({k: report.get(k) for k in ("covered", "covered_uncertain", "slot", "folder", "mirrored", "weights")}))


if __name__ == "__main__":
    main()
