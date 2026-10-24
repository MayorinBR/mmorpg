"""Dungeon kit helpers: a flat-shaded polygon builder whose faces are colored by a palette atlas.

Units are meters. Blender Z is up, the front of every piece faces -Y and the origin is the base center
(for floor tiles the origin is the walking surface). Each palette entry also carries the pattern slice
(see Painted/patterns) and an emissive strength that the DungeonPainted shader reads from the category map.
"""
import math
import os
import random

import bpy
import bmesh
import numpy as np

# key: (rgb, pattern slice, emissive 0-255)
# slices: 0 smooth stone, 1 bricks, 2 planks, 3 flagstone, 4 cloth, 5 metal, 6 rough rock, 7 flat
PALETTE = {
    "brick_a": ((0.43, 0.41, 0.45), 1, 0),
    "brick_b": ((0.34, 0.32, 0.38), 1, 0),
    "brick_dark": ((0.22, 0.21, 0.26), 1, 0),
    "brick_light": ((0.55, 0.53, 0.54), 1, 0),
    "brick_warm": ((0.47, 0.35, 0.30), 1, 0),
    "stone": ((0.48, 0.47, 0.50), 0, 0),
    "stone_dark": ((0.30, 0.29, 0.34), 0, 0),
    "trim": ((0.60, 0.58, 0.56), 0, 0),
    "trim_dark": ((0.37, 0.36, 0.40), 0, 0),
    "flag_a": ((0.42, 0.42, 0.46), 3, 0),
    "flag_b": ((0.30, 0.30, 0.35), 3, 0),
    "flag_dark": ((0.19, 0.19, 0.23), 3, 0),
    "flag_warm": ((0.46, 0.39, 0.34), 3, 0),
    "moss": ((0.30, 0.43, 0.20), 6, 0),
    "moss_dark": ((0.18, 0.28, 0.16), 6, 0),
    "rock_a": ((0.37, 0.34, 0.36), 6, 0),
    "rock_b": ((0.27, 0.25, 0.29), 6, 0),
    "rock_c": ((0.47, 0.42, 0.40), 6, 0),
    "rock_dark": ((0.15, 0.14, 0.18), 6, 0),
    "rock_warm": ((0.43, 0.33, 0.27), 6, 0),
    "dirt": ((0.31, 0.23, 0.17), 6, 0),
    "dirt_dark": ((0.18, 0.13, 0.11), 6, 0),
    "bone": ((0.84, 0.80, 0.68), 0, 0),
    "bone_dark": ((0.60, 0.56, 0.46), 0, 0),
    "wood": ((0.41, 0.27, 0.15), 2, 0),
    "wood_dark": ((0.24, 0.16, 0.10), 2, 0),
    "wood_rot": ((0.30, 0.28, 0.21), 2, 0),
    "iron": ((0.31, 0.32, 0.36), 5, 0),
    "iron_dark": ((0.16, 0.17, 0.20), 5, 0),
    "rust": ((0.50, 0.28, 0.16), 5, 0),
    "steel": ((0.60, 0.62, 0.68), 5, 0),
    "gold": ((0.82, 0.63, 0.22), 5, 0),
    "bronze": ((0.56, 0.39, 0.20), 5, 0),
    "cloth_red": ((0.47, 0.10, 0.12), 4, 0),
    "cloth_purple": ((0.31, 0.14, 0.40), 4, 0),
    "cloth_grey": ((0.36, 0.35, 0.38), 4, 0),
    "cloth_tan": ((0.60, 0.52, 0.39), 4, 0),
    "blood": ((0.30, 0.04, 0.05), 7, 0),
    "wax": ((0.86, 0.81, 0.66), 7, 0),
    "black": ((0.04, 0.04, 0.05), 7, 0),
    "flame": ((1.00, 0.62, 0.16), 7, 255),
    "ember": ((0.95, 0.30, 0.08), 7, 200),
    "crystal_blue": ((0.30, 0.66, 1.00), 7, 190),
    "crystal_purple": ((0.66, 0.36, 0.96), 7, 190),
    "rune_green": ((0.30, 1.00, 0.60), 7, 230),
    "rune_red": ((1.00, 0.25, 0.20), 7, 210),
    "slime": ((0.36, 0.68, 0.26), 7, 90),
}
PALETTE_KEYS = list(PALETTE)
GRID = 8
CELL = 16
MATERIAL_NAME = "DungeonPalette"


class Builder:
    """Accumulates flat-shaded faces in a local frame."""

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
        with self.at(x, y, z0, rot):
            hw, hd = w / 2, d / 2
            pts = [(-hw, -hd, 0), (hw, -hd, 0), (hw, hd, 0), (-hw, hd, 0), (-hw, 0, h), (hw, 0, h)]
            faces = [[0, 3, 2, 1], [0, 1, 5, 4], [2, 3, 4, 5], [0, 4, 3], [1, 2, 5]]
            self.add(pts, faces, color)

    def lathe(self, x, y, z0, profile, seg, color, rot=0.0):
        """Revolves a bottom-to-top list of (radius, height) pairs; a zero radius is only allowed on the last ring."""
        with self.at(x, y, z0, rot):
            pts, rings = [], []
            for r, z in profile:
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
                    faces.append([a[k], a[k1], b[0]] if len(b) == 1 else [a[k], a[k1], b[k1], b[k]])
            if profile[-1][0] > 0:
                faces.append(rings[-1])
            self.add(pts, faces, color)

    def cyl(self, x, y, z0, r, h, seg, color, r2=None, rot=0.0):
        self.lathe(x, y, z0, [(r, 0), (r if r2 is None else r2, h)], seg, color, rot)

    def plate(self, poly, y0, y1, color, x=0.0, z=0.0, rot=0.0):
        """Extrudes a CCW polygon of (x, z) points, seen from the front, between y0 and y1."""
        with self.at(x, 0, z, rot):
            n = len(poly)
            pts = [(px, y0, pz) for px, pz in poly] + [(px, y1, pz) for px, pz in poly]
            faces = [list(range(n)), list(range(2 * n - 1, n - 1, -1))]
            faces += [[i, n + i, n + (i + 1) % n, (i + 1) % n] for i in range(n)]
            self.add(pts, faces, color)

    def slab(self, poly, z0, z1, color, x=0.0, y=0.0, rot=0.0):
        """Extrudes a CCW polygon of (x, y) points between z0 and z1."""
        with self.at(x, y, 0, rot):
            n = len(poly)
            pts = [(px, py, z0) for px, py in poly] + [(px, py, z1) for px, py in poly]
            faces = [list(range(n)), list(range(2 * n - 1, n - 1, -1))]
            faces += [[i, (i + 1) % n, n + (i + 1) % n, n + i] for i in range(n)]
            self.add(pts, faces, color)

    def beam(self, p0, p1, w, color, d=None):
        """Box of cross-section w x d running from point p0 to point p1."""
        d = w if d is None else d
        ax = np.array(p1, dtype=float) - np.array(p0, dtype=float)
        length = float(np.linalg.norm(ax))
        if length < 1e-6:
            return
        ax /= length
        ref = np.array([0.0, 0.0, 1.0]) if abs(ax[2]) < 0.95 else np.array([1.0, 0.0, 0.0])
        u = np.cross(ax, ref)
        u /= np.linalg.norm(u)
        v = np.cross(ax, u)
        pts = []
        for end in (p0, p1):
            for su, sv in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
                pts.append(tuple(np.array(end) + u * su * w / 2 + v * sv * d / 2))
        faces = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]]
        self.add(pts, faces, color)

    def tube(self, p0, p1, r, seg, color, r2=None, caps=True):
        """Prism or truncated cone running from p0 to p1 with the given number of sides."""
        r2 = r if r2 is None else r2
        ax = np.array(p1, dtype=float) - np.array(p0, dtype=float)
        length = float(np.linalg.norm(ax))
        ax /= length
        ref = np.array([0.0, 0.0, 1.0]) if abs(ax[2]) < 0.95 else np.array([1.0, 0.0, 0.0])
        u = np.cross(ax, ref)
        u /= np.linalg.norm(u)
        v = np.cross(ax, u)
        pts = []
        for end, rad in ((p0, r), (p1, r2)):
            for k in range(seg):
                a = 2 * math.pi * k / seg
                pts.append(tuple(np.array(end) + (u * math.cos(a) + v * math.sin(a)) * rad))
        faces = [[k, (k + 1) % seg, seg + (k + 1) % seg, seg + k] for k in range(seg)]
        if caps:
            faces += [list(reversed(range(seg))), list(range(seg, 2 * seg))]
        self.add(pts, faces, color)

    def rock(self, x, y, z0, w, d, h, color, seed, sides=8, rings=3, jitter=0.22, top=0.55, rot=0.0, flat_bottom=True):
        """Irregular rock mass: stacked rings of a jittered ellipse that narrow toward a flat top."""
        rnd = random.Random(seed)
        with self.at(x, y, z0, rot):
            pts, ring_idx = [], []
            for k in range(rings + 1):
                t = k / rings
                scale = 1.0 - (1.0 - top) * (t ** 1.4)
                z = h * t
                idx = []
                for s in range(sides):
                    a = 2 * math.pi * s / sides + rnd.uniform(-0.12, 0.12)
                    j = 1.0 + rnd.uniform(-jitter, jitter) if 0 < k else 1.0 + rnd.uniform(-jitter * 0.5, jitter * 0.5)
                    zz = z + (rnd.uniform(-0.08, 0.08) * h if 0 < k < rings else 0.0)
                    idx.append(len(pts))
                    pts.append((math.cos(a) * w / 2 * scale * j, math.sin(a) * d / 2 * scale * j, max(zz, 0.0) if flat_bottom and k == 0 else zz))
                ring_idx.append(idx)
            cap = len(pts)
            pts.append((rnd.uniform(-0.1, 0.1) * w, rnd.uniform(-0.1, 0.1) * d, h * rnd.uniform(1.0, 1.08)))
            faces = [list(reversed(ring_idx[0]))]
            for a, b in zip(ring_idx, ring_idx[1:]):
                for s in range(sides):
                    s1 = (s + 1) % sides
                    faces.append([a[s], a[s1], b[s1], b[s]])
            for s in range(sides):
                s1 = (s + 1) % sides
                faces.append([ring_idx[-1][s], ring_idx[-1][s1], cap])
            self.add(pts, faces, color)

    def spike(self, x, y, z0, r, h, seg, color, seed=0, lean=0.0, up=True):
        """Cone with a jittered base; up=False hangs the tip downward from z0 + h."""
        rnd = random.Random(seed)
        with self.at(x, y, z0):
            base_z, tip_z = (0.0, h) if up else (h, 0.0)
            pts = [(math.cos(2 * math.pi * k / seg) * r * rnd.uniform(0.82, 1.15), math.sin(2 * math.pi * k / seg) * r * rnd.uniform(0.82, 1.15), base_z) for k in range(seg)]
            pts.append((lean * h, 0.0, tip_z))
            faces = [list(reversed(range(seg))) if up else list(range(seg))]
            for k in range(seg):
                k1 = (k + 1) % seg
                faces.append([k, k1, seg] if up else [k1, k, seg])
            self.add(pts, faces, color)

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


def palette_pixels():
    size = GRID * CELL
    px = np.zeros((size, size, 4), dtype=np.float32)
    px[..., 3] = 1.0
    for i, key in enumerate(PALETTE_KEYS):
        cx, cy = i % GRID, i // GRID
        px[cy * CELL:(cy + 1) * CELL, cx * CELL:(cx + 1) * CELL, :3] = PALETTE[key][0]
    return px


def category_pixels():
    """8x8 RGBA map: R = pattern slice, G = emissive strength, same cell order as the palette."""
    px = np.zeros((GRID, GRID, 4), dtype=np.uint8)
    px[..., 3] = 255
    for i, key in enumerate(PALETTE_KEYS):
        _, slice_index, emissive = PALETTE[key]
        px[i // GRID, i % GRID, 0] = slice_index
        px[i // GRID, i % GRID, 1] = emissive
    px[len(PALETTE_KEYS) // GRID + 1:, :, 0] = 7
    return px


def palette_material():
    mat = bpy.data.materials.get(MATERIAL_NAME)
    if mat:
        return mat
    img = bpy.data.images.new(MATERIAL_NAME, GRID * CELL, GRID * CELL)
    img.pixels.foreach_set(palette_pixels().ravel())
    mat = bpy.data.materials.new(MATERIAL_NAME)
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
    img = bpy.data.images[MATERIAL_NAME]
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
        path_mode="STRIP", embed_textures=False)


def circle_poly(cx, cz, r, n=8):
    return [(cx + r * math.cos(2 * math.pi * i / n), cz + r * math.sin(2 * math.pi * i / n)) for i in range(n)]


def rect_poly(cx, z0, w, h):
    return [(cx - w / 2, z0), (cx + w / 2, z0), (cx + w / 2, z0 + h), (cx - w / 2, z0 + h)]


def arch_poly(cx, z0, w, h, seg=4):
    """Rectangle topped by a semicircle, CCW seen from the front."""
    zs = z0 + h - w / 2
    arc = [(cx + w / 2 * math.cos(math.pi * i / seg), zs + w / 2 * math.sin(math.pi * i / seg)) for i in range(seg + 1)]
    return [(cx - w / 2, z0), (cx + w / 2, z0)] + arc + [(cx - w / 2, zs)]


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


def extrude_x(b, x, y, z0, w, poly_yz, color):
    """Extrudes a polygon of (y, z) points along X, centered on x with width w."""
    with b.at(x, y, z0, math.pi / 2):
        b.plate(poly_yz, -w / 2, w / 2, color)


def stair_flight(b, x, y, z0, w, run, rise, steps, color="flag_a", rot=0.0):
    """Solid steps rising toward +Y; the first tread is at the -Y end."""
    sh, sd = rise / steps, run / steps
    with b.at(x, y, z0, rot):
        for i in range(steps):
            b.box(0, -run / 2 + sd * (i + 0.5), 0, w, sd, sh * (i + 1), color)


POINT = 1.5


def pointed_arc(cx, a, zs, seg=4, radius_factor=POINT):
    """Points of a pointed arch from the left spring over the apex to the right spring."""
    r = radius_factor * a
    theta = math.acos((a - r) / r)
    left = [(cx - a + r + r * math.cos(math.pi - (math.pi - theta) * i / seg), zs + r * math.sin(math.pi - (math.pi - theta) * i / seg)) for i in range(seg + 1)]
    right = [(cx + a - r - r * math.cos(math.pi - (math.pi - theta) * i / seg), zs + r * math.sin(math.pi - (math.pi - theta) * i / seg)) for i in range(seg + 1)]
    return left + list(reversed(right))[1:]


def arch_rise(a, radius_factor=POINT):
    r = radius_factor * a
    return r * math.sin(math.acos((a - r) / r))


def pointed_wall_poly(width, height, openings, seg=4):
    """Wall outline with pointed-arch passages; openings is a list of (center_x, half_width, spring_height)."""
    pts = [(-width / 2, 0)]
    for cx, a, zs in sorted(openings):
        pts += [(cx - a, 0), (cx - a, zs)]
        pts += pointed_arc(cx, a, zs, seg)[1:-1]
        pts += [(cx + a, zs), (cx + a, 0)]
    return pts + [(width / 2, 0), (width / 2, height), (-width / 2, height)]


def arch_ring(cx, a_in, a_out, zs, seg=4):
    """C-shaped polygon that frames a pointed opening."""
    inner = pointed_arc(cx, a_in, zs, seg)
    outer = pointed_arc(cx, a_out, zs, seg)
    return [(cx - a_out, 0), (cx - a_in, 0)] + inner + [(cx + a_in, 0), (cx + a_out, 0)] + list(reversed(outer))


def arch_slab(cx, a, zs, seg=4):
    """Solid pointed-arch shaped polygon (door leaf, gate backing)."""
    return [(cx - a, 0), (cx + a, 0)] + [(cx + a, zs)] + list(reversed(pointed_arc(cx, a, zs, seg)))[1:-1] + [(cx - a, zs)]
