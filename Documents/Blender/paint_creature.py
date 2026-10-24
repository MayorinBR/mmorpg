"""Paints the albedo and emission textures of a creature from its debug .blend.
Usage: python3 paint_creature.py -- Spider|Tarantula|Beetle|Davidis <build_dir> <texture_dir>
Tarantula reuses the Spider mesh and Davidis has its own mesh; <build_dir> holds the matching debug .blend."""
import sys
import bpy
import numpy as np
from scipy import ndimage

NAME, BUILD, OUT = sys.argv[sys.argv.index("--") + 1:][:3]
SIZE = 2048

MODEL = {"Tarantula": "Spider"}.get(NAME, NAME)
KIND = "spider" if MODEL == "Spider" else "beetle"
bpy.ops.wm.open_mainfile(filepath=f"{BUILD}/{MODEL}_debug.blend")
ob = next(o for o in bpy.data.objects if o.type == "MESH")
arm = next((o for o in bpy.data.objects if o.type == "ARMATURE"), None)
me = ob.data
me.calc_loop_triangles()

nv = len(me.vertices)
P = np.array([v.co[:] for v in me.vertices])
shell = np.zeros(nv, int)
me.attributes["shell"].data.foreach_get("value", shell)

groups = [g.name for g in ob.vertex_groups]
dominant = np.zeros(nv, int)
for v in me.vertices:
    if v.groups:
        dominant[v.index] = max(v.groups, key=lambda x: x.weight).group

bone_t = np.full(nv, 0.5)
if arm is not None:
    for v in me.vertices:
        bone = arm.data.bones.get(groups[dominant[v.index]])
        if bone is None:
            continue
        h, t = np.array(bone.head_local[:]), np.array(bone.tail_local[:])
        d = t - h
        n = d @ d
        if n > 1e-12:
            bone_t[v.index] = np.clip((P[v.index] - h) @ d / n, 0, 1)

bone_name = np.array([groups[i] for i in dominant])
leg_seg = np.array([int(b.split("_")[-1]) if b.startswith("Leg_") else 0 for b in bone_name])
is_leg = np.array([b.startswith("Leg_") for b in bone_name])

loop_uv = np.zeros((len(me.loops), 2))
me.uv_layers["UVMap"].data.foreach_get("uv", loop_uv.ravel())
loop_n = np.zeros((len(me.loops), 3))
me.corner_normals.foreach_get("vector", loop_n.ravel()) if hasattr(me, "corner_normals") else None

H = SIZE
mask = np.zeros((H, H), bool)
img_p = np.zeros((H, H, 3))
img_n = np.zeros((H, H, 3))
img_vi = np.zeros((H, H), int)
img_t = np.zeros((H, H))

for tri in me.loop_triangles:
    loops = tri.loops
    verts = tri.vertices
    uv = loop_uv[list(loops)] * H
    xmin, ymin = np.floor(uv.min(0)).astype(int)
    xmax, ymax = np.ceil(uv.max(0)).astype(int)
    xs, ys = np.meshgrid(np.arange(max(xmin, 0), min(xmax + 1, H)), np.arange(max(ymin, 0), min(ymax + 1, H)))
    px, py = xs + 0.5, ys + 0.5
    (x0, y0), (x1, y1), (x2, y2) = uv
    den = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2)
    if abs(den) < 1e-12:
        continue
    a = ((y1 - y2) * (px - x2) + (x2 - x1) * (py - y2)) / den
    b = ((y2 - y0) * (px - x2) + (x0 - x2) * (py - y2)) / den
    c = 1 - a - b
    inside = (a >= -1e-6) & (b >= -1e-6) & (c >= -1e-6)
    if not inside.any():
        continue
    w = np.stack([a, b, c], -1)[inside]
    ix, iy = xs[inside], ys[inside]
    img_p[iy, ix] = w @ P[list(verts)]
    img_n[iy, ix] = w @ loop_n[list(loops)]
    img_vi[iy, ix] = np.array(verts)[w.argmax(1)]
    img_t[iy, ix] = w @ bone_t[list(verts)]
    mask[iy, ix] = True

filled = mask.copy()
_, (iy, ix) = ndimage.distance_transform_edt(~mask, return_indices=True)
near = (ndimage.distance_transform_edt(~mask) <= 8)
for arr in (img_p, img_n, img_vi, img_t):
    arr[...] = arr[iy, ix]
valid = mask | near

sh = shell[img_vi]
bn = bone_name[img_vi]
seg = leg_seg[img_vi]
leg = is_leg[img_vi]
t = img_t
x, y, z = img_p[..., 0], img_p[..., 1], img_p[..., 2]
nz = img_n[..., 2]

rng = np.random.default_rng(7)


def noise(scale, amp):
    n = ndimage.gaussian_filter(rng.standard_normal((H, H)), scale)
    return 1 + amp * n / (n.std() + 1e-9)


def smooth(edge, width):
    return np.clip((edge + width) / (2 * width), 0, 1)


def mix(base, color, k):
    k = np.clip(k, 0, 1)[..., None]
    return base * (1 - k) + np.array(color) * k


def hexagons(u, v, cell):
    """Hex grid on float coordinates. Returns (edge factor 0 at the cell center to 1 at its border,
    random value per cell, local offset x, local offset y, both in cell units)."""
    q = (2 / 3 * u) / cell
    r = (-1 / 3 * u + np.sqrt(3) / 3 * v) / cell
    s_ = -q - r
    rq, rr, rs = np.round(q), np.round(r), np.round(s_)
    dq, dr, ds = abs(rq - q), abs(rr - r), abs(rs - s_)
    fix_q = (dq > dr) & (dq > ds)
    fix_r = ~fix_q & (dr > ds)
    rq = np.where(fix_q, -rr - rs, rq)
    rr = np.where(fix_r, -rq - rs, rr)
    ox = u / cell - 1.5 * rq
    oy = v / cell - np.sqrt(3) * (rr + rq / 2)
    edge = np.clip(np.maximum(abs(oy) / (np.sqrt(3) / 2), (abs(oy) * 0.5 + abs(ox) * np.sqrt(3) / 2) / (np.sqrt(3) / 2)), 0, 1)
    h = np.sin(rq * 12.9898 + rr * 78.233) * 43758.5453
    return edge, h - np.floor(h), ox, oy


def shell_plane(shell_id):
    """Per-pixel coordinates (meters) on the tangent plane of a shell, plus its center and radius."""
    sel = sh == shell_id
    normal = img_n[sel].mean(0)
    normal /= np.linalg.norm(normal)
    a_ = np.cross(normal, [0.0, 0.0, 1.0])
    a_ = a_ / np.linalg.norm(a_) if np.linalg.norm(a_) > 1e-6 else np.array([1.0, 0.0, 0.0])
    b_ = np.cross(normal, a_)
    center = img_p[sel].mean(0)
    d = img_p - center
    u, v = d @ a_, d @ b_
    radius = np.hypot(u[sel], v[sel]).max()
    return sel, u, v, np.hypot(u, v) / radius


def fang_param(shell_id):
    """Position along a fang from its root (0) to its tip (1) and the angle around its axis."""
    sel = sh == shell_id
    pts = img_p[sel]
    center = pts.mean(0)
    axis = np.linalg.svd(pts - center, full_matrices=False)[2][0]
    axis = axis if axis[2] < 0 else -axis
    proj = (img_p - center) @ axis
    lo, hi = proj[sel].min(), proj[sel].max()
    side = np.cross(axis, [1.0, 0.0, 0.0])
    side /= np.linalg.norm(side)
    up = np.cross(axis, side)
    d = img_p - center
    return sel, np.clip((proj - lo) / (hi - lo), 0, 1), np.arctan2(d @ up, d @ side)


albedo = np.zeros((H, H, 3))
emit = np.zeros((H, H, 3))


def surface_maps():
    """Smoothness (alpha) and metallic (red) of the Unity metallic map."""
    if KIND == "spider":
        gloss = np.full((H, H), 0.1)
        gloss = np.where(leg & ((t < 0.14) | (t > 0.86)), 0.2, gloss)
        gloss = np.where((sh == 5) | (sh == 17), 0.6, gloss)
        gloss = np.where(sh >= 100, 0.97, gloss)
        return gloss, np.zeros((H, H))
    glossy = NAME == "Davidis"
    gloss = np.full((H, H), 0.72 if glossy else 0.5)
    gloss = np.where(leg, 0.5 if glossy else 0.4, gloss)
    gloss = np.where(sh == 15, 0.88 if glossy else 0.8, gloss)
    gloss = np.where((sh == 22) | (sh == 23), 0.97, gloss)
    metal = np.where(sh == 15, 0.1 if glossy else 0.25, 0.0)
    return gloss, metal


def paint_fangs(img, shell_ids):
    for sid in shell_ids:
        sel, s_, ang = fang_param(sid)
        root = np.array([0.1, 0.095, 0.09])
        ivory = np.array([0.86, 0.85, 0.8])
        tip = np.array([0.42, 0.42, 0.43])
        color = np.ones((H, H, 3)) * ivory
        color = mix(color, root, smooth(0.22 - s_, 0.12))
        color = mix(color, tip, smooth(s_ - 0.82, 0.08))
        grain = 1 + 0.06 * np.sin(ang * 7) + 0.04 * noise(3, 1.0)
        color = color * grain[..., None]
        img = np.where(sel[..., None], color, img)
    return img


PALETTES = {
    "Spider": {"base": [0.02, 0.02, 0.024], "joint": [0.19, 0.17, 0.16], "fur": [0.05, 0.042, 0.036], "accent": [0.62, 0.035, 0.03], "mark": [0.55, 0.035, 0.03]},
    "Tarantula": {"base": [0.05, 0.026, 0.013], "joint": [0.24, 0.13, 0.055], "fur": [0.12, 0.062, 0.028], "accent": [0.95, 0.42, 0.04], "mark": [0.9, 0.38, 0.04]},
    "Beetle": {"base": [0.2, 0.1, 0.045], "leg": [0.13, 0.065, 0.03], "tip": [0.3, 0.3, 0.32], "eye": [0.045, 0.045, 0.05], "eye_gain": [0.035, 0.036, 0.04], "eye_edge": [0.012, 0.012, 0.014], "eye_rim": [0.02, 0.02, 0.023]},
    "Davidis": {"eye": [0.62, 0.62, 0.64], "eye_gain": [0.3, 0.3, 0.3], "eye_edge": [0.3, 0.3, 0.32], "eye_rim": [0.5, 0.5, 0.52], "base": [0.15, 0.068, 0.03], "leg": [0.115, 0.052, 0.024], "tip": [0.07, 0.04, 0.028], "elytra": [0.095, 0.052, 0.009], "elytra_light": [0.165, 0.095, 0.017]},
}
PAL = PALETTES[NAME]


def paint_spider():
    img = np.ones((H, H, 3)) * np.array(PAL["base"])
    img *= noise(14, 0.12)[..., None]
    hair = ndimage.gaussian_filter(rng.standard_normal((H, H)), (0.8, 5.0))
    hair = hair / hair.std()
    fine = ndimage.gaussian_filter(rng.standard_normal((H, H)), 0.7)
    fur = np.clip(0.5 + 0.28 * hair + 0.12 * fine / fine.std(), 0, 1)
    img = mix(img, PAL["fur"], fur * (0.6 if NAME == "Spider" else 0.7) * (sh < 100) * (sh != 5) * (sh != 17))

    edge = np.where(leg, np.maximum(smooth(0.12 - t, 0.06), smooth(t - 0.88, 0.06)), 0)
    img = mix(img, PAL["joint"], edge)

    abdomen = sh == 13
    stripe = smooth(0.028 - abs(x), 0.012)
    hourglass = smooth(0.045 * (1 - abs(y - 0.2) / 0.1) - abs(x), 0.01) * (abs(y - 0.2) < 0.1)
    rear = smooth(y - 0.405, 0.012) * smooth(0.07 - abs(x), 0.02)
    spots = smooth(0.02 - np.hypot(abs(x) - 0.09, y - 0.12), 0.008)
    red = np.clip(np.maximum.reduce([stripe * (y > 0.04), hourglass, rear, spots]), 0, 1)
    red = np.where(abdomen, red * smooth(nz + 0.1, 0.25) + rear * (1 - smooth(nz + 0.1, 0.25)), 0)
    img = mix(img, PAL["accent"], red)

    thorax = sh == 0
    mark = smooth(0.02 - abs(x), 0.01) * smooth(0.05 - abs(y + 0.03), 0.012) * smooth(nz - 0.5, 0.15)
    img = mix(img, PAL["mark"], np.where(thorax, mark, 0))

    img = paint_fangs(img, [5, 17])

    eyes = sh >= 100
    img = np.where(eyes[..., None], np.array([0.004, 0.004, 0.005]), img)
    em = np.zeros((H, H, 3))
    return img, em


def paint_beetle():
    img = np.ones((H, H, 3)) * np.array(PAL["base"]) * noise(12, 0.1)[..., None]
    img = np.where(leg[..., None], np.ones((H, H, 3)) * np.array(PAL["leg"]) * noise(10, 0.1)[..., None], img)
    elytra = sh == 15

    if NAME == "Davidis":
        sheen = ndimage.gaussian_filter(smooth(z - 0.38, 0.12), 8)
        chestnut = (np.array(PAL["elytra"]) * (1 - sheen[..., None]) + np.array(PAL["elytra_light"]) * sheen[..., None]) * noise(26, 0.1)[..., None]
        dots = ndimage.gaussian_filter(rng.standard_normal((H, H)), 1.1)
        chestnut = chestnut * (1 - 0.22 * smooth(dots / dots.std() - 1.6, 0.3))[..., None]
        blotch = ndimage.gaussian_filter(rng.standard_normal((H, H)), (16, 24))
        blotch = smooth(blotch / blotch.std() - 0.9, 0.35)
        chestnut = mix(chestnut, [0.04, 0.021, 0.005], blotch * 0.8)
        img = np.where(elytra[..., None], chestnut, img)
    else:
        sheen = ndimage.gaussian_filter(smooth(z - 0.4, 0.1), 8)
        blue = (np.array([0.01, 0.028, 0.17]) * (1 - sheen[..., None]) + np.array([0.025, 0.075, 0.38]) * sheen[..., None]) * noise(30, 0.12)[..., None]
        img = np.where(elytra[..., None], blue, img)

        weight = ndimage.gaussian_filter(elytra.astype(float), 10)
        blurred_x = ndimage.gaussian_filter(np.where(elytra, x, 0.0), 10) / np.maximum(weight, 1e-6)
        w = abs(np.where(elytra, blurred_x, x))
        body = smooth(0.034 - w, 0.0035)
        border = smooth(0.034 - w, 0.0035) * (1 - smooth(0.027 - w, 0.0035))
        ridges = 1 + 0.07 * np.sin(y * 420)
        gold = (np.array([0.95, 0.7, 0.1]) * (1 - 0.35 * smooth(w - 0.012, 0.02))[..., None]) * ridges[..., None]
        img = mix(img, gold, np.where(elytra, body, 0))
        img = mix(img, [0.42, 0.25, 0.025], np.where(elytra, border, 0) * 0.85)
        img = mix(img, [0.05, 0.03, 0.01], np.where(elytra, smooth(0.0035 - w, 0.002), 0) * 0.6)

    for sid in (22, 23):
        eye, u, v, r = shell_plane(sid)
        edge_h, cid, ox, oy = hexagons(u, v, 0.0075)
        dome = 1 - edge_h ** 2
        facet = np.array(PAL["eye"]) + np.array(PAL["eye_gain"]) * (dome * (0.5 + 0.5 * cid))[..., None]
        facet = mix(facet, PAL["eye_edge"], smooth(edge_h - 0.8, 0.07))
        rim = smooth(0.9 - r, 0.1)
        eye_color = facet * rim[..., None] + np.array(PAL["eye_rim"]) * (1 - rim[..., None])
        img = np.where(eye[..., None], eye_color, img)

    tip = leg & (seg == 3) & (t > 0.35)
    k = np.where(tip, smooth(t - 0.4, 0.12), 0)
    img = mix(img, PAL["tip"], k)
    return img, np.zeros((H, H, 3))


albedo, emit = paint_spider() if KIND == "spider" else paint_beetle()
gloss, metallic = surface_maps()

albedo = np.where(valid[..., None], albedo, 0.0)
srgb = np.clip(albedo, 0, 1) ** (1 / 2.2)
srgb[~valid] = [0.02, 0.02, 0.02]


def save(name, arr, linear=False):
    img = bpy.data.images.new(name, H, H, alpha=arr.shape[-1] == 4)
    if linear:
        img.colorspace_settings.name = "Non-Color"
        img.alpha_mode = "CHANNEL_PACKED"
    rgba = np.ones((H, H, 4), np.float32)
    rgba[..., : arr.shape[-1]] = arr
    img.pixels.foreach_set(rgba.ravel())
    img.filepath_raw = f"{OUT}/{name}.png"
    img.file_format = "PNG"
    img.save()


import os
os.makedirs(OUT, exist_ok=True)
save(f"{NAME}_Albedo", srgb)
packed = np.zeros((H, H, 4))
packed[..., 0] = metallic
packed[..., 3] = np.where(valid, gloss, 0.1)
save(f"{NAME}_Metallic", packed, linear=True)
print("painted", NAME, "coverage", mask.mean().round(3))
