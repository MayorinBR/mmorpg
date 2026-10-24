"""Detects coplanar, same-facing, overlapping triangles that would z-fight."""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
import numpy as np

import build
import lib


def overlaps(obj, tol=2e-3):
    mesh = obj.data
    verts = np.array([v.co[:] for v in mesh.vertices])
    groups = {}
    for p in mesh.polygons:
        n = np.array(p.normal[:])
        if n[2] < 0.5:
            continue
        pts = verts[list(p.vertices)]
        d = float(np.dot(n, pts[0]))
        key = (round(n[0], 2), round(n[1], 2), round(n[2], 2), round(d / tol))
        groups.setdefault(key, []).append((p.index, pts, n))
    found = []
    for key, items in groups.items():
        if len(items) < 2:
            continue
        for i, (ia, pa, n) in enumerate(items):
            for ib, pb, _ in items[i + 1:]:
                ca = pa.mean(axis=0)
                cb = pb.mean(axis=0)
                if inside(ca, pb, n) or inside(cb, pa, n):
                    found.append((ia, ib, tuple(np.round(ca, 2))))
    return found


def inside(point, tri, n):
    ax = int(np.argmax(np.abs(n)))
    keep = [i for i in range(3) if i != ax]
    p = point[keep]
    q = tri[:, keep]
    sign = None
    for i in range(len(q)):
        a, b = q[i], q[(i + 1) % len(q)]
        cross = (b[0] - a[0]) * (p[1] - a[1]) - (b[1] - a[1]) * (p[0] - a[0])
        if abs(cross) < 1e-9:
            continue
        s = cross > 0
        if sign is None:
            sign = s
        elif sign != s:
            return False
    return sign is not None


if __name__ == "__main__":
    only = sys.argv[1:] or None
    bpy.ops.wm.read_factory_settings(use_empty=True)
    build.load_modules()
    bad = 0
    for cat, name, lo, hi, fn in lib.REGISTRY:
        if only and name not in only:
            continue
        obj = fn().finalize()
        hits = overlaps(obj)
        if hits:
            bad += 1
            print(f"{name}: {len(hits)} overlapping faces, e.g. {hits[0]}")
    print("assets with coplanar overlaps:", bad)
