import importlib
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy

import lib

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")
MODULES = ("structure", "cave", "decor", "props")


def load_modules():
    for mod in MODULES:
        if os.path.exists(os.path.join(os.path.dirname(lib.__file__), mod + ".py")):
            importlib.import_module(mod)


def save_category_map(path):
    from PIL import Image
    pixels = lib.category_pixels()[::-1]
    Image.fromarray(pixels, "RGBA").save(path)


def write_catalog(rows):
    lines = ["# Dungeon kit catalog", "", "Units: meters. Front faces -Y, origin at the base center (floor tiles: the walking surface). Module grid 4 m, wall height 3.6 m, wall thickness 0.8 m.", "",
             "| Category | Asset | Triangles | Budget | Size (x, y, z) |", "|---|---|---|---|---|"]
    for cat, name, n, lo, hi, dims in rows:
        lines.append(f"| {cat} | {name} | {n} | {lo}-{hi} | {dims[0]} x {dims[1]} x {dims[2]} |")
    lines += ["", f"Total assets: {len(rows)}"]
    open(os.path.join(OUT, "CATALOG.md"), "w").write("\n".join(lines))


def main(only=None, export=True):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    load_modules()
    rows = []
    for cat, name, lo, hi, fn in lib.REGISTRY:
        if only and name not in only:
            continue
        obj = fn().finalize()
        n = lib.tri_count(obj)
        dims = tuple(round(v, 1) for v in obj.dimensions)
        rows.append((cat, name, n, lo, hi, dims))
        if export:
            if len(rows) == 1:
                lib.save_palette(os.path.join(OUT, "DungeonPalette.png"))
                save_category_map(os.path.join(OUT, "DungeonCategory.png"))
            lib.export_fbx(obj, os.path.join(OUT, cat, name + ".fbx"))
    if export:
        write_catalog(rows)
    return rows


if __name__ == "__main__":
    args = sys.argv[1:]
    export = "--export" in args
    only = [a for a in args if a != "--export"] or None
    for cat, name, n, lo, hi, dims in main(only, export=export):
        flag = "" if lo <= n <= hi else ("  LOW" if n < lo else "  HIGH")
        print(f"{cat:10s} {name:30s} {n:6d} [{lo}-{hi}] {dims}{flag}")
