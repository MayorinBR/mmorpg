import math
import os

import bpy
import bmesh
import numpy as np

PALETTE = {
    "sand_light": (0.93, 0.83, 0.63),
    "sand": (0.84, 0.70, 0.48),
    "sand_dark": (0.68, 0.53, 0.36),
    "trim": (0.96, 0.90, 0.74),
    "stone": (0.74, 0.66, 0.52),
    "stone_dark": (0.52, 0.45, 0.36),
    "slate": (0.34, 0.31, 0.40),
    "slate_light": (0.46, 0.43, 0.54),
    "gold": (0.82, 0.66, 0.24),
    "wood": (0.48, 0.32, 0.18),
    "wood_dark": (0.30, 0.20, 0.12),
    "door": (0.22, 0.15, 0.10),
    "window": (0.14, 0.17, 0.22),
    "cloth_cream": (0.90, 0.90, 0.76),
    "cloth_green": (0.42, 0.60, 0.40),
    "cloth_red": (0.70, 0.20, 0.15),
    "cloth_blue": (0.20, 0.36, 0.62),
    "cloth_ochre": (0.85, 0.60, 0.20),
    "metal": (0.36, 0.38, 0.42),
    "metal_light": (0.62, 0.64, 0.68),
    "palm_leaf": (0.26, 0.52, 0.22),
    "palm_trunk": (0.55, 0.42, 0.26),
    "water": (0.20, 0.68, 0.64),
    "terracotta": (0.72, 0.38, 0.22),
    "fire": (0.95, 0.55, 0.12),
    "black": (0.08, 0.07, 0.07),
}
PALETTE_KEYS = list(PALETTE)
GRID = 8
CELL = 16


class Builder:
    """Accumulates flat-shaded faces in a local frame whose front faces -Y and whose origin is the base center."""

    def __init__(self, name):
        self.name = name
        self.verts = []
        self.faces = []
        self.colors = []
        self._stack = [(0.0, 0.0, 0.0, 0.0)]

    def at(self, x=0.0, y=0.0, z=0.0, rot=0.0):
        return _Frame(self, x, y, z, rot)

    def _apply(self, p):
        tx, ty, tz, r = self._stack[-1]
        c, s = math.cos(r), math.sin(r)
        return (tx + p[0] * c - p[1] * s, ty + p[0] * s + p[1] * c, tz + p[2])

    def add(self, pts, faces, color):
        base = len(self.verts)
        self.verts.extend(self._apply(p) for p in pts)
        for f in faces:
            self.faces.append([base + i for i in f])
            self.colors.append(PALETTE_KEYS.index(color))

    def frustum(self, x, y, z0, w, d, h, color, w2=None, d2=None, rot=0.0):
        w2 = w if w2 is None else w2
        d2 = d if d2 is None else d2
        with self.at(x, y, z0, rot):
            hw, hd, tw, td = w / 2, d / 2, w2 / 2, d2 / 2
            if tw == 0 and td == 0:
                pts = [(-hw, -hd, 0), (hw, -hd, 0), (hw, hd, 0), (-hw, hd, 0), (0, 0, h)]
                faces = [[0, 3, 2, 1], [0, 1, 4], [1, 2, 4], [2, 3, 4], [3, 0, 4]]
            else:
                pts = [(-hw, -hd, 0), (hw, -hd, 0), (hw, hd, 0), (-hw, hd, 0),
                       (-tw, -td, h), (tw, -td, h), (tw, td, h), (-tw, td, h)]
                faces = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]]
            self.add(pts, faces, color)

    def box(self, x, y, z0, w, d, h, color, rot=0.0):
        self.frustum(x, y, z0, w, d, h, color, rot=rot)

    def pyramid(self, x, y, z0, w, d, h, color, rot=0.0):
        self.frustum(x, y, z0, w, d, h, color, 0, 0, rot)

    def gable(self, x, y, z0, w, d, h, color, rot=0.0):
        """Triangular prism with the ridge running along local X."""
        with self.at(x, y, z0, rot):
            hw, hd = w / 2, d / 2
            pts = [(-hw, -hd, 0), (hw, -hd, 0), (hw, hd, 0), (-hw, hd, 0), (-hw, 0, h), (hw, 0, h)]
            faces = [[0, 3, 2, 1], [0, 1, 5, 4], [2, 3, 4, 5], [0, 4, 3], [1, 2, 5]]
            self.add(pts, faces, color)

    def lathe(self, x, y, z0, profile, seg, color, rot=0.0):
        """Revolves a bottom-to-top list of (radius, height) pairs; a zero radius is only allowed on the last ring."""
        with self.at(x, y, z0, rot):
            pts, rings = [], []
            for i, (r, z) in enumerate(profile):
                if r == 0:
                    rings.append([len(pts)])
                    pts.append((0, 0, z))
                else:
                    rings.append(list(range(len(pts), len(pts) + seg)))
                    pts.extend((r * math.cos(2 * math.pi * k / seg), r * math.sin(2 * math.pi * k / seg), z) for k in range(seg))
            faces = []
            if profile[0][0] > 0:
                faces.append(list(reversed(rings[0])))
            for a, b in zip(rings, rings[1:]):
                for k in range(seg):
                    k1 = (k + 1) % seg
                    if len(b) == 1:
                        faces.append([a[k], a[k1], b[0]])
                    else:
                        faces.append([a[k], a[k1], b[k1], b[k]])
            if profile[-1][0] > 0:
                faces.append(rings[-1])
            self.add(pts, faces, color)

    def cyl(self, x, y, z0, r, h, seg, color, r2=None, rot=0.0):
        self.lathe(x, y, z0, [(r, 0), (r if r2 is None else r2, h)], seg, color, rot)

    def plate(self, poly, y0, y1, color, x=0.0, z=0.0, rot=0.0):
        """Extrudes a polygon given as CCW (x, z) points seen from the front, between y0 and y1."""
        with self.at(x, 0, z, rot):
            n = len(poly)
            pts = [(px, y0, pz) for px, pz in poly] + [(px, y1, pz) for px, pz in poly]
            faces = [list(range(n)), list(range(2 * n - 1, n - 1, -1))]
            faces += [[i, n + i, n + (i + 1) % n, (i + 1) % n] for i in range(n)]
            self.add(pts, faces, color)

    def slab(self, poly, z0, z1, color, x=0.0, y=0.0, rot=0.0):
        """Extrudes a polygon given as (x, y) points in the horizontal plane between z0 and z1."""
        with self.at(x, y, 0, rot):
            n = len(poly)
            pts = [(px, py, z0) for px, py in poly] + [(px, py, z1) for px, py in poly]
            faces = [list(range(n)), list(range(2 * n - 1, n - 1, -1))]
            faces += [[i, (i + 1) % n, n + (i + 1) % n, n + i] for i in range(n)]
            self.add(pts, faces, color)

    def extrude_x(self, x, y, z0, w, poly_yz, color):
        """Extrudes a polygon given as (y, z) points along X, centered on x with width w."""
        with self.at(x, y, z0, math.pi / 2):
            self.plate(poly_yz, -w / 2, w / 2, color)

    def finalize(self):
        mesh = bpy.data.meshes.new(self.name)
        mesh.from_pydata(self.verts, [], self.faces)
        uv = mesh.uv_layers.new(name="UVMap")
        coords = []
        for poly, ci in zip(mesh.polygons, self.colors):
            u = ((ci % GRID) + 0.5) / GRID
            v = ((ci // GRID) + 0.5) / GRID
            coords.extend([(u, v)] * poly.loop_total)
        uv.data.foreach_set("uv", np.array(coords, dtype=np.float32).ravel())
        bm = bmesh.new()
        bm.from_mesh(mesh)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bmesh.ops.triangulate(bm, faces=bm.faces, quad_method="BEAUTY", ngon_method="EAR_CLIP")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        obj.data.materials.append(palette_material())
        return obj


class _Frame:
    def __init__(self, b, x, y, z, rot):
        self.b, self.t = b, (x, y, z, rot)

    def __enter__(self):
        b = self.b
        tx, ty, tz, r = b._stack[-1]
        px, py, pz = b._apply((self.t[0], self.t[1], self.t[2]))
        b._stack.append((px, py, pz, r + self.t[3]))

    def __exit__(self, *exc):
        self.b._stack.pop()


def arch_poly(cx, z0, w, h, seg=4):
    """Rectangle topped by a semicircle, CCW seen from the front."""
    zs = z0 + h - w / 2
    arc = [(cx + w / 2 * math.cos(math.pi * i / seg), zs + w / 2 * math.sin(math.pi * i / seg)) for i in range(seg + 1)]
    return [(cx - w / 2, z0), (cx + w / 2, z0)] + arc + [(cx - w / 2, zs)]


def rect_poly(cx, z0, w, h):
    return [(cx - w / 2, z0), (cx + w / 2, z0), (cx + w / 2, z0 + h), (cx - w / 2, z0 + h)]


def window(b, x, z0, w, h, y, arched=True, shutters=False, trim="trim"):
    shape = arch_poly if arched else (lambda cx, zz, ww, hh, *_: rect_poly(cx, zz, ww, hh))
    b.plate(shape(x, z0 - 0.12, w + 0.3, h + 0.3), y - 0.05, y + 0.02, trim)
    b.plate(shape(x, z0, w, h), y - 0.09, y + 0.02, "window")
    b.box(x, y - 0.1, z0 - 0.2, w + 0.5, 0.3, 0.12, trim)
    if shutters:
        for s in (-1, 1):
            b.box(x + s * (w / 2 + 0.2), y - 0.1, z0, 0.3, 0.06, h, "wood")


def door(b, x, z0, w, h, y, arched=True, color="door"):
    shape = arch_poly if arched else (lambda cx, zz, ww, hh, *_: rect_poly(cx, zz, ww, hh))
    b.plate(shape(x, z0, w + 0.4, h + 0.2), y - 0.08, y + 0.02, "trim")
    b.plate(shape(x, z0, w, h), y - 0.14, y + 0.02, color)
    b.box(x, y - 0.1, z0, 0.08, 0.1, h - w / 2 if arched else h, "wood_dark")


def crenels(b, w, d, z0, step=0.9, size=0.45, h=0.45, color="sand_light", skip_front=False):
    nx, nz = max(int(w / step), 2), max(int(d / step), 2)
    for i in range(nx):
        x = -w / 2 + size / 2 + i * (w - size) / (nx - 1)
        b.box(x, d / 2 - size / 2, z0, size, size, h, color)
        if not skip_front:
            b.box(x, -d / 2 + size / 2, z0, size, size, h, color)
    for j in range(1, nz - 1):
        y = -d / 2 + size / 2 + j * (d - size) / (nz - 1)
        b.box(-w / 2 + size / 2, y, z0, size, size, h, color)
        b.box(w / 2 - size / 2, y, z0, size, size, h, color)


def parapet(b, w, d, z0, t=0.3, h=0.7, color="sand_light"):
    b.box(0, -d / 2 + t / 2, z0, w, t, h, color)
    b.box(0, d / 2 - t / 2, z0, w, t, h, color)
    b.box(-w / 2 + t / 2, 0, z0, t, d - 2 * t, h, color)
    b.box(w / 2 - t / 2, 0, z0, t, d - 2 * t, h, color)


def block_body(b, w, d, h, wall="sand", plinth=True, cornice=True, band_z=None):
    if plinth:
        b.box(0, 0, 0, w + 0.3, d + 0.3, 0.45, "stone")
    b.box(0, 0, 0.45 if plinth else 0, w, d, h - (0.45 if plinth else 0), wall)
    if cornice:
        b.box(0, 0, h - 0.05, w + 0.4, d + 0.4, 0.3, "trim")
    if band_z:
        b.box(0, 0, band_z, w + 0.2, d + 0.2, 0.2, "trim")


def slate_dome(b, x, y, z0, r, h, seg=8, spire=True):
    prof = [(r * 1.05, 0), (r * 1.05, h * 0.12), (r, h * 0.2), (r * 0.9, h * 0.45), (r * 0.62, h * 0.7), (r * 0.3, h * 0.88), (r * 0.08, h * 0.98)]
    b.lathe(x, y, z0, prof + [(0, h)], seg, "slate")
    b.cyl(x, y, z0, r * 1.12, h * 0.1, seg, "trim")
    if spire:
        b.cyl(x, y, z0 + h * 0.95, r * 0.05, r * 0.7, 4, "gold")
        b.lathe(x, y, z0 + h * 0.95 + r * 0.55, [(r * 0.14, 0), (r * 0.1, r * 0.15), (0, r * 0.35)], 6, "gold")


def tower_round(b, x, y, z0, r, h, seg=8, dome_h=None, wall="sand", window_rows=1):
    b.cyl(x, y, z0, r * 1.15, 0.5, seg, "stone")
    b.cyl(x, y, z0 + 0.5, r, h, seg, wall, r2=r * 0.95)
    for k in range(1, window_rows + 1):
        zz = z0 + 0.5 + h * k / (window_rows + 1)
        for s in range(seg):
            a = 2 * math.pi * (s + 0.5) / seg
            if s % 2 == 0:
                with b.at(x + math.cos(a) * r * 0.97, y + math.sin(a) * r * 0.97, zz, a + math.pi / 2):
                    b.plate(arch_poly(0, 0, 0.45, 0.9, 3), -0.06, 0.04, "window")
    b.cyl(x, y, z0 + 0.5 + h, r * 1.18, 0.35, seg, "trim")
    if dome_h:
        slate_dome(b, x, y, z0 + 0.85 + h, r * 1.05, dome_h, seg)


def stairs(b, x, y, z0, w, run, rise, steps, color="stone", rot=0.0):
    sh, sd = rise / steps, run / steps
    with b.at(x, y, z0, rot):
        for i in range(steps):
            b.box(0, -run / 2 + sd * (i + 0.5), 0, w, sd, sh * (i + 1), color)


def palette_material():
    mat = bpy.data.materials.get("MorrocPalette")
    if mat:
        return mat
    size = GRID * CELL
    px = np.zeros((size, size, 4), dtype=np.float32)
    px[..., 3] = 1.0
    for i, key in enumerate(PALETTE_KEYS):
        cx, cy = i % GRID, i // GRID
        px[cy * CELL:(cy + 1) * CELL, cx * CELL:(cx + 1) * CELL, :3] = PALETTE[key]
    img = bpy.data.images.new("MorrocPalette", size, size)
    img.pixels.foreach_set(px.ravel())
    mat = bpy.data.materials.new("MorrocPalette")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    tex = nodes.new("ShaderNodeTexImage")
    tex.image = img
    tex.interpolation = "Closest"
    bsdf = nodes["Principled BSDF"]
    bsdf.inputs["Roughness"].default_value = 1.0
    mat.node_tree.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    return mat


def save_palette(path):
    img = bpy.data.images["MorrocPalette"]
    img.filepath_raw = path
    img.file_format = "PNG"
    img.save()


def tri_count(obj):
    return len(obj.data.polygons)


def export_fbx(obj, path):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={"MESH"},
        apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
        bake_space_transform=True, mesh_smooth_type="FACE", add_leaf_bones=False,
        path_mode="COPY", embed_textures=True)


def circle_poly(cx, cz, r, n=8):
    return [(cx + r * math.cos(2 * math.pi * i / n), cz + r * math.sin(2 * math.pi * i / n)) for i in range(n)]


def wall_poly(width, height, openings, seg=6):
    """Wall outline with arched passages; openings is a list of (center_x, half_width, spring_height)."""
    pts = [(-width / 2, 0)]
    for cx, a, zs in sorted(openings):
        pts += [(cx - a, 0), (cx - a, zs)]
        pts += [(cx + a * math.cos(math.pi - math.pi * i / seg), zs + a * math.sin(math.pi - math.pi * i / seg)) for i in range(1, seg + 1)]
        pts.append((cx + a, 0))
    return pts + [(width / 2, 0), (width / 2, height), (-width / 2, height)]


REGISTRY = []


def asset(category, lo, hi):
    """Registers a builder function with its category and triangle budget."""
    def deco(fn):
        REGISTRY.append((category, fn.__name__, lo, hi, fn))
        return fn
    return deco


def wall_frame(side, w, d):
    """Returns (rotation, surface_y) so that features added in that frame sit on the given wall."""
    return {"front": (0.0, -d / 2), "right": (math.pi / 2, -w / 2), "back": (math.pi, -d / 2), "left": (-math.pi / 2, -w / 2)}[side]


def icon(b, kind, x, z, y, s=1.0):
    """Flat emblem plates for shop signs, facing -Y."""
    def P(poly, color, depth=0.06):
        b.plate([(x + px * s, z + pz * s) for px, pz in poly], y - depth, y, color)
    if kind == "sword":
        P([(-0.08, 0), (0.08, 0), (0.08, 0.9), (0, 1.05), (-0.08, 0.9)], "metal_light")
        P(rect_poly(0, -0.02, 0.5, 0.1), "gold", 0.09)
        P(rect_poly(0, -0.3, 0.08, 0.28), "wood_dark", 0.09)
    elif kind == "pick":
        P(rect_poly(0, -0.5, 0.09, 1.1), "wood_dark")
        P([(-0.55, 0.35), (-0.15, 0.42), (0.15, 0.42), (0.55, 0.35), (0.2, 0.55), (-0.2, 0.55)], "metal_light", 0.09)
    elif kind == "hammer":
        P(rect_poly(0, -0.5, 0.1, 1.1), "wood_dark")
        P(rect_poly(0, 0.45, 0.6, 0.3), "metal_light", 0.09)
    elif kind == "arrow":
        P(rect_poly(0, -0.5, 0.06, 1.4), "wood_dark")
        P([(-0.15, 0.9), (0.15, 0.9), (0, 1.2)], "metal_light", 0.09)
        P([(-0.15, -0.5), (0, -0.3), (0.15, -0.5), (0, -0.7)], "cloth_red", 0.09)
    elif kind == "mug":
        P(rect_poly(0, -0.35, 0.5, 0.7), "gold")
        P(rect_poly(0.4, -0.2, 0.2, 0.4), "gold")
        P(rect_poly(0, 0.3, 0.5, 0.12), "trim", 0.09)
    elif kind == "book":
        P(rect_poly(0, -0.4, 0.7, 0.8), "cloth_blue")
        P(rect_poly(0, -0.4, 0.5, 0.12), "gold", 0.09)
    elif kind == "anvil":
        P(rect_poly(0, -0.4, 0.3, 0.3), "metal")
        P([(-0.5, -0.1), (0.5, -0.1), (0.35, 0.1), (-0.35, 0.1)], "metal_light", 0.09)
    elif kind == "shield":
        P(circle_poly(0, 0, 0.5, 8), "cloth_red")
        P(circle_poly(0, 0, 0.2, 6), "gold", 0.09)
    elif kind == "swords":
        for sx in (-1, 1):
            b.plate([(x + (px * math.cos(sx * 0.7) - pz * math.sin(sx * 0.7)) * s, z + (px * math.sin(sx * 0.7) + pz * math.cos(sx * 0.7)) * s)
                     for px, pz in [(-0.1, -0.6), (0.1, -0.6), (0.1, 0.8), (0, 1.0), (-0.1, 0.8)]], y - 0.07, y, "metal_light")


def awning(b, x, y, z, w, d, color="cloth_cream", stripe="cloth_red"):
    """Sloped canvas awning projecting toward -Y with alternating stripes and two poles."""
    n = max(int(w / 0.8), 2)
    sw = w / n
    for i in range(n):
        b.extrude_x(x - w / 2 + sw * (i + 0.5), y, z, sw, [(0, 0), (-d, -0.7), (-d, -0.55), (0, 0.15)], color if i % 2 == 0 else stripe)
    for sx in (-1, 1):
        b.cyl(x + sx * (w / 2 - 0.15), y - d + 0.1, z - 0.7 - 1.6, 0.08, 1.7, 4, "wood_dark")


def balcony(b, x, y, z, w, d=1.3, color="wood"):
    b.box(x, y - d / 2, z, w, d, 0.15, color)
    b.box(x, y - d + 0.05, z + 0.15, w, 0.08, 0.1, "wood_dark")
    b.box(x, y - d + 0.05, z + 0.85, w, 0.1, 0.1, "wood_dark")
    for sx in (-1, 1):
        b.box(x + sx * (w / 2 - 0.05), y - d / 2, z + 0.15, 0.1, d, 0.1, "wood_dark")
        b.box(x + sx * (w / 2 - 0.05), y - d / 2, z + 0.85, 0.1, d, 0.1, "wood_dark")
    n = max(int(w / 0.5), 3)
    for i in range(n + 1):
        b.box(x - w / 2 + 0.06 + i * (w - 0.12) / n, y - d + 0.05, z + 0.15, 0.1, 0.1, 0.8, "wood_dark")
    for sx in (-1, 1):
        for sy in (0.05, d - 0.05):
            b.box(x + sx * (w / 2 - 0.05), y - sy, z + 0.15, 0.1, 0.1, 0.8, "wood_dark")


def vigas(b, w, d, z, n=None, side="front"):
    """Protruding wooden roof beams under the cornice."""
    rot, y = wall_frame(side, w, d)
    span = w if side in ("front", "back") else d
    n = n or int(span / 1.1)
    with b.at(rot=rot):
        for i in range(n):
            b.box(-span / 2 + 0.6 + i * (span - 1.2) / max(n - 1, 1), y - 0.1, z, 0.22, 0.5, 0.22, "wood")


def pot(b, x, y, z0, r=0.35, color="terracotta"):
    b.lathe(x, y, z0, [(r * 0.6, 0), (r, r * 0.9), (r * 0.8, r * 1.8), (r * 0.9, r * 2.1), (0, r * 2.1)], 6, color)


def barrel(b, x, y, z0, r=0.4, h=0.9, color="wood"):
    b.lathe(x, y, z0, [(r * 0.85, 0), (r, h * 0.5), (r * 0.85, h), (0, h)], 8, color)
    b.cyl(x, y, z0 + h * 0.25, r * 0.97, 0.06, 8, "metal")
    b.cyl(x, y, z0 + h * 0.7, r * 0.97, 0.06, 8, "metal")


def crate(b, x, y, z0, s=0.7, rot=0.0):
    b.box(x, y, z0, s, s, s, "wood", rot)
    b.box(x, y, z0 + s * 0.45, s * 1.04, s * 0.12, s * 0.1, "wood_dark", rot)
    b.box(x, y, z0 + s * 0.45, s * 0.12, s * 1.04, s * 0.1, "wood_dark", rot)


def hanging_sign(b, x, z, y, kind, board="wood", scale=0.9):
    """Sign board hung from a wall bracket."""
    b.box(x, y - 0.5, z + 1.55, 0.12, 1.0, 0.12, "wood_dark")
    b.box(x, y - 1.0, z + 0.2, 1.5, 0.1, 1.35, board)
    b.box(x, y - 1.0, z + 1.55, 1.6, 0.14, 0.12, "wood_dark")
    icon(b, kind, x, z + 0.55, y - 1.1, scale)


def quoins(b, w, d, h, z0=0.45, step=0.7):
    """Alternating corner blocks running up each building corner."""
    n = int((h - z0) / step)
    for sx in (-1, 1):
        for sy in (-1, 1):
            for i in range(n):
                k = 0.62 if i % 2 == 0 else 0.46
                b.box(sx * (w / 2), sy * (d / 2), z0 + i * step, k, k, step * 0.9, "trim")
