import importlib
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy

import lib

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")


def write_catalog(rows):
    lines = ["# Morroc kit catalog", "", "Units: meters. Front faces -Y, origin at the base center.", "",
             "| Category | Asset | Triangles | Budget | Size (x, y, z) |", "|---|---|---|---|---|"]
    for cat, name, n, lo, hi, dims in rows:
        lines.append(f"| {cat} | {name} | {n} | {lo}-{hi} | {dims[0]} x {dims[1]} x {dims[2]} |")
    lines.append("")
    lines.append(f"Total assets: {len(rows)}")
    open(os.path.join(OUT, "CATALOG.md"), "w").write("\n".join(lines))


def layout_and_save(objs):
    """Lays the assets out in category rows and saves a single .blend kit sheet."""
    bpy.data.images["MorrocPalette"].pack()
    cats = {}
    for cat, obj in objs:
        cats.setdefault(cat, []).append(obj)
    y = 0.0
    for cat, items in cats.items():
        x = 0.0
        row_depth = 0.0
        for obj in items:
            dx, dy = obj.dimensions.x, obj.dimensions.y
            obj.location = (x + dx / 2 + 4, y - 4, 0)
            x += dx + 10
            row_depth = max(row_depth, dy)
        y -= row_depth + 25
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "MorrocKit.blend"))


def main(only=None, export=True):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for mod in ("houses", "buildings", "props"):
        if os.path.exists(os.path.join(os.path.dirname(lib.__file__), mod + ".py")):
            importlib.import_module(mod)
    rows, objs = [], []
    for cat, name, lo, hi, fn in lib.REGISTRY:
        if only and name not in only:
            continue
        obj = fn().finalize()
        n = lib.tri_count(obj)
        dims = tuple(round(v, 1) for v in obj.dimensions)
        rows.append((cat, name, n, lo, hi, dims))
        objs.append((cat, obj))
        if export:
            if len(rows) == 1:
                lib.save_palette(os.path.join(OUT, "MorrocPalette.png"))
            lib.export_fbx(obj, os.path.join(OUT, cat, name + ".fbx"))
    if export:
        write_catalog(rows)
        layout_and_save(objs)
    return rows


if __name__ == "__main__":
    args = sys.argv[1:]
    export = "--export" in args
    only = [a for a in args if a != "--export"] or None
    for cat, name, n, lo, hi, dims in main(only, export=export):
        flag = "" if lo <= n <= hi else ("  LOW" if n < lo else "  HIGH")
        print(f"{cat:10s} {name:34s} {n:6d} [{lo}-{hi}] {dims}{flag}")
