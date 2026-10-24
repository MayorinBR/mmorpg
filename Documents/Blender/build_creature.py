"""Builds a game-ready, rigged and animated creature FBX from a raw static mesh.

Usage: python3 build_creature.py -- Spider|Beetle|Davidis <input_dir> <output_dir>

Pipeline: import -> bake transforms -> weld/clean -> orient (front = -Y) and ground ->
automatic UV -> armature + skin weights -> Idle/Walk/Attack actions -> FBX export.
"""
import math
import sys

import bpy
import bmesh
import numpy as np
from mathutils import Matrix, Vector
from scipy.spatial import cKDTree

NAME, SRC, OUT = sys.argv[-3], sys.argv[-2], sys.argv[-1]
FPS = 30
Z = Vector((0, 0, 1))
FRONT = Vector((0, -1, 0))

# Shell indices come from a BFS over the welded source mesh; they are stable for these two files.
SPECS = {
    "Spider": {
        "yaw": 0.0,
        "anchor_shell": 0,
        "rigid": {"Body": [0, 14, 7], "Abdomen": [13], "Fang_A": [5], "Fang_B": [17]},
        "strays": [1, 2, 3, 9],
        "legs": [[6, 4], [18], ([11], 2), [10, 16], [12], [8], [15]],
        "leg_kind": "arch",
        "uv_boost": {5: 1.8, 17: 1.8},
        "eyes": [
            (sx * 0.0305, -0.205, 0.34, 0.026) for sx in (-1, 1)
        ] + [
            (sx * 0.074, -0.178, 0.346, 0.0155) for sx in (-1, 1)
        ] + [
            (sx * 0.062, -0.172, 0.398, 0.0125) for sx in (-1, 1)
        ],
    },
    "Beetle": {
        "yaw": -90.0,
        "anchor_shell": 18,
        "rigid": {"Body": [18, 15, 16], "Head": [17, 22, 23, 24]},
        "strays": [12],
        "legs": [[1, 2, 0], [20, 21, 19], [4, 5, 3], [14, 13, 11], [10, 9], [7, 8, 6]],
        "leg_kind": "segments",
        "uv_boost": {22: 3.0, 23: 3.0, 15: 1.4},
    },
}
SPECS["Davidis"] = dict(
    SPECS["Beetle"],
    source="Beetle",
    horn_fork={
        "shell": 17,
        "cut_z": 0.55,
        "blend_radius": 0.09,
        "blend_passes": 0,
        "origin": (0.0, -0.262, 0.5),
        "direction": (0.0, -0.5, 0.85),
        "length": 0.24,
        "radii": (0.052, 0.004),
        "tine_direction": (0.75, -0.3, 0.6),
        "bone": "Head",
    },
)
SPEC = SPECS[NAME]
SOURCE = SPEC.get("source", NAME)


def log(*a):
    print("[build]", *a, flush=True)


# ------------------------------------------------------------------ import and clean
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=f"{SRC}/{SOURCE}.fbx")
src_ob = [o for o in bpy.data.objects if o.type == "MESH"][0]
bpy.context.view_layer.objects.active = src_ob
src_ob.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

bm = bmesh.new()
bm.from_mesh(src_ob.data)
bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0001)
bm.verts.ensure_lookup_table()

shell_of = {}
shells = []
for v in bm.verts:
    if v.index in shell_of:
        continue
    stack, cur = [v], [v.index]
    shell_of[v.index] = len(shells)
    while stack:
        x = stack.pop()
        for e in x.link_edges:
            y = e.other_vert(x)
            if y.index not in shell_of:
                shell_of[y.index] = len(shells)
                stack.append(y)
                cur.append(y.index)
    shells.append(cur)

# orient: front = -Y, centered on XY, feet on Z = 0
rot = Matrix.Rotation(math.radians(SPEC["yaw"]), 4, "Z")
bmesh.ops.transform(bm, matrix=rot, verts=bm.verts)
P0 = np.array([tuple(v.co) for v in bm.verts])
shift = Vector((-(P0[:, 0].min() + P0[:, 0].max()) / 2, -(P0[:, 1].min() + P0[:, 1].max()) / 2, -P0[:, 2].min()))
bmesh.ops.translate(bm, vec=shift, verts=bm.verts)
P = np.array([tuple(v.co) for v in bm.verts])
shell_ids = np.array([shell_of[i] for i in range(len(P))])
log(NAME, "verts", len(P), "shells", len(shells), "size", (P.max(0) - P.min(0)).round(3))


def shell_pts(ids):
    return P[np.isin(shell_ids, ids)]


anchor = shell_pts([SPEC["anchor_shell"]])
BODY_CENTER = (anchor.min(0) + anchor.max(0)) / 2


def contact(a_ids, b_ids):
    """Midpoint of the closest vertex pair between two groups of shells."""
    A, B = shell_pts(a_ids), shell_pts(b_ids)
    d, idx = cKDTree(B).query(A)
    i = int(d.argmin())
    return (A[i] + B[idx[i]]) / 2


def seg_dist(p, a, b):
    ab = b - a
    t = np.clip(np.dot(p - a, ab) / max(np.dot(ab, ab), 1e-12), 0, 1)
    return np.linalg.norm(p - (a + t * ab))


def lowest_point(V):
    z0 = V[:, 2].min()
    return V[V[:, 2] < z0 + 0.012].mean(0)


# ------------------------------------------------------------------ legs
legs = []  # dicts: verts (indices), joints (np arrays), side, rank


def arch_joints(V):
    root = V[np.linalg.norm(V - BODY_CENTER, axis=1).argmin()]
    apex = V[V[:, 2].argmax()]
    return [root, apex, lowest_point(V)]


def split_fused(idx, k):
    V = P[idx]
    low = np.where(V[:, 2] < V[:, 2].min() + 0.02)[0]
    centers = [V[low[0]]]
    for _ in range(k - 1):
        far = low[np.argmax([min(np.linalg.norm(V[j] - c) for c in centers) for j in low])]
        centers.append(V[far])
    root = V[np.linalg.norm(V - BODY_CENTER, axis=1).argmin()]
    lines = [(root, c) for c in centers]
    for _ in range(6):
        lab = np.array([np.argmin([seg_dist(p, a, b) for a, b in lines]) for p in V])
        lines = []
        for c in range(k):
            sub = V[lab == c]
            r = sub[np.linalg.norm(sub - BODY_CENTER, axis=1).argmin()]
            lines.append((r, lowest_point(sub)))
    return [idx[lab == c] for c in range(k)]


for entry in SPEC["legs"]:
    fused = isinstance(entry, tuple)
    ids = entry[0] if fused else entry
    k = entry[1] if fused else 1
    idx = np.where(np.isin(shell_ids, ids))[0]
    parts = split_fused(idx, k) if k > 1 else [idx]
    for part in parts:
        V = P[part]
        if SPEC["leg_kind"] == "arch":
            joints = arch_joints(V)
        else:
            tip = lowest_point(shell_pts(ids[-1:]))
            root = V[np.linalg.norm(V - BODY_CENTER, axis=1).argmin()]
            if len(ids) == 3:
                knee, ankle = contact(ids[:1], ids[1:2]), contact(ids[1:2], ids[2:3])
            else:
                knee = contact(ids[:1], ids[1:2])
                tip = lowest_point(shell_pts(ids[1:2]))
                ankle = knee + (tip - knee) * 0.6
            joints = [root, knee, ankle, tip]
        legs.append({"verts": part, "joints": joints})

for lg in legs:
    lg["side"] = "L" if lg["joints"][-1][0] > 0 else "R"
legs.sort(key=lambda l: (l["joints"][0][1], l["joints"][0][0]))
count = {"L": 0, "R": 0}
for lg in sorted(legs, key=lambda l: (l["joints"][-1][1])):
    count[lg["side"]] += 1
    lg["name"] = f"Leg_{lg['side']}{count[lg['side']]}"
    lg["bones"] = [f"{lg['name']}_{i + 1}" for i in range(len(lg["joints"]) - 1)]
log("legs", [(l["name"], len(l["verts"])) for l in legs])

# ------------------------------------------------------------------ weights
bone_names = ["Root", "Body"]
if NAME == "Spider":
    bone_names += ["Abdomen", "Fang_A", "Fang_B"]
else:
    bone_names += ["Head"]
for lg in legs:
    bone_names += lg["bones"]

weights = {}  # vertex index -> {bone: weight}
for bone, ids in SPEC["rigid"].items():
    for i in np.where(np.isin(shell_ids, ids))[0]:
        weights[int(i)] = {bone: 1.0}
for lg in legs:
    J = lg["joints"]
    for i in lg["verts"]:
        d = np.array([seg_dist(P[i], J[s], J[s + 1]) for s in range(len(J) - 1)])
        w = 1.0 / (d + 1e-4) ** 4
        w /= w.sum()
        weights[int(i)] = {lg["bones"][s]: float(w[s]) for s in range(len(w)) if w[s] > 0.02}

# write weights and shell ids on the BMesh (they survive vertex deletion), then drop strays
dl = bm.verts.layers.deform.verify()
shell_layer = bm.verts.layers.int.new("shell")
for v in bm.verts:
    v[shell_layer] = shell_of[v.index]
    for b, w in weights.get(v.index, {}).items():
        v[dl][bone_names.index(b)] = w
stray = [v for v in bm.verts if shell_of[v.index] in SPEC["strays"] or v.index not in weights]
bmesh.ops.delete(bm, geom=stray, context="VERTS")



def curve_points(origin, start_direction, end_direction, length):
    """Control and end points of the quadratic curve that leaves along start_direction and arrives along end_direction."""
    control = origin + start_direction * length * 0.55
    return control, control + end_direction * length * 0.65


def build_tube(origin, start_direction, end_direction, length, radii, segments=10, rings=8, flare=(0.0, 1.0)):
    """Adds a closed, tapering tube following a quadratic curve and returns its vertices."""
    control, tip = curve_points(origin, start_direction, end_direction, length)
    reference = Vector((1.0, 0.0, 0.0))
    rows = []
    for i in range(rings + 1):
        s = i / rings
        center = origin * (1 - s) ** 2 + control * 2 * s * (1 - s) + tip * s * s
        tangent = ((control - origin) * (1 - s) + (tip - control) * s).normalized()
        a = reference.cross(tangent).normalized()
        b = tangent.cross(a).normalized()
        radius = radii[0] + (radii[1] - radii[0]) * s + flare[0] * max(0.0, 1.0 - s / flare[1]) ** 2
        rows.append([bm.verts.new(center + (a * math.cos(2 * math.pi * j / segments) + b * math.sin(2 * math.pi * j / segments)) * radius) for j in range(segments)])
    for lower, upper in zip(rows, rows[1:]):
        for j in range(segments):
            bm.faces.new((lower[j], lower[(j + 1) % segments], upper[(j + 1) % segments], upper[j]))
    bm.faces.new(rows[0])
    bm.faces.new(reversed(rows[-1]))
    return [v for row in rows for v in row]


fork = SPEC.get("horn_fork")
if fork:
    tip = [v for v in bm.verts if v[shell_layer] == fork["shell"] and abs(v.co.x) < 0.06 and v.co.y < -0.29 and v.co.z > fork["cut_z"]]
    bmesh.ops.delete(bm, geom=tip, context="VERTS")
    base = Vector(fork["origin"])
    around = [v for v in bm.verts if v[shell_layer] == fork["shell"] and (v.co - base).length < fork["blend_radius"]]
    for _ in range(fork["blend_passes"]):
        bmesh.ops.smooth_vert(bm, verts=around, factor=0.5, use_axis_x=True, use_axis_y=True, use_axis_z=True)
    origin, trunk = Vector(fork["origin"]), Vector(fork["direction"]).normalized()
    for k, sign in enumerate((-1, 1)):
        tine = Vector(fork["tine_direction"])
        tine.x *= sign
        for v in build_tube(origin, trunk, tine.normalized(), fork["length"], fork["radii"]):
            v[shell_layer] = 200 + k
            v[dl][bone_names.index(fork["bone"])] = 1.0

for k, (cx, cy, cz, radius) in enumerate(SPEC.get("eyes", [])):
    eye_verts = bmesh.ops.create_uvsphere(bm, u_segments=20, v_segments=12, radius=radius)["verts"]
    bmesh.ops.translate(bm, vec=(cx, cy, cz), verts=eye_verts)
    for v in eye_verts:
        v[shell_layer] = 100 + k
        v[dl][bone_names.index("Body")] = 1.0

# ------------------------------------------------------------------ clean geometry
bmesh.ops.dissolve_degenerate(bm, edges=bm.edges, dist=1e-5)
bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
bmesh.ops.triangulate(bm, faces=bm.faces)
bm.normal_update()
open_edges = sum(1 for e in bm.edges if len(e.link_faces) == 1)
nonmanifold = sum(1 for e in bm.edges if len(e.link_faces) > 2)
degenerate = sum(1 for f in bm.faces if f.calc_area() < 1e-9)
log("clean: faces", len(bm.faces), "open", open_edges, "nonmanifold", nonmanifold, "degenerate", degenerate)

# ------------------------------------------------------------------ automatic UV
# One island per body part, in meters so texel density is uniform: legs are unrolled as cylinders
# around their principal axis and every other part as a sphere whose poles sit on the sides.
uv_layer = bm.loops.layers.uv.new("UVMap")
bm.verts.ensure_lookup_table()
bm.faces.ensure_lookup_table()
leg_axes = {}
for lg in legs:
    for i, bn in enumerate(lg["bones"]):
        leg_axes[bn] = (np.array(lg["joints"][i]), np.array(lg["joints"][i + 1]))


def dominant_bone(v):
    k, _ = max(v[dl].items(), key=lambda kv: kv[1])
    return bone_names[k]


def face_key(f):
    names = [dominant_bone(v) for v in f.verts]
    top = max(set(names), key=names.count)
    if top in leg_axes:
        return ("bone", top)
    return ("ball", f.verts[0][shell_layer])


def unwrap_angle(angles):
    ref = angles[0]
    return [a + 2 * math.pi * round((ref - a) / (2 * math.pi)) for a in angles]


groups_of_faces = {}
for f in bm.faces:
    groups_of_faces.setdefault(face_key(f), []).append(f)

for (kind, ident), faces in groups_of_faces.items():
    pts = np.array([v.co[:] for f in faces for v in f.verts])
    if kind == "bone":
        head, tail = leg_axes[ident]
        head = pts.mean(0)
        axis = np.linalg.svd(pts - head, full_matrices=False)[2][0]
        axis = axis if axis @ (tail - leg_axes[ident][0]) >= 0 else -axis
        side = np.cross(axis, [0.0, 0.0, 1.0])
        side = side / np.linalg.norm(side) if np.linalg.norm(side) > 1e-6 else np.array([1.0, 0.0, 0.0])
        up = np.cross(axis, side)
        radius = float(np.linalg.norm(np.cross(pts - head, axis), axis=1).mean()) or 0.01

        def project(p):
            d = np.array(p) - head
            return math.atan2(float(d @ up), float(d @ side)), float(d @ axis)

        for f in faces:
            ang = unwrap_angle([project(l.vert.co)[0] for l in f.loops])
            for l, a in zip(f.loops, ang):
                l[uv_layer].uv = (a * radius, project(l.vert.co)[1])
    else:
        center = pts.mean(0)
        radius = float(np.linalg.norm(pts - center, axis=1).mean()) or 0.01

        def project(p):
            d = np.array(p) - center
            return math.atan2(float(d[2]), float(d[1])), math.asin(max(-1.0, min(1.0, float(d[0]) / (np.linalg.norm(d) or 1.0))))

        for f in faces:
            ang = unwrap_angle([project(l.vert.co)[0] for l in f.loops])
            for l, a in zip(f.loops, ang):
                boost = SPEC.get("uv_boost", {}).get(ident, 1.0)
                l[uv_layer].uv = (a * radius * boost, project(l.vert.co)[1] * radius * boost)

ob = bpy.data.objects.new(f"{NAME}_Mesh", bpy.data.meshes.new(f"{NAME}_Mesh"))
bpy.context.scene.collection.objects.link(ob)
for n in bone_names:
    ob.vertex_groups.new(name=n)
for f in bm.faces:
    f.smooth = True
bm.to_mesh(ob.data)
ob.data.normals_split_custom_set_from_vertices([v.normal.copy() for v in bm.verts])
bm.free()
bpy.data.objects.remove(src_ob, do_unlink=True)
bpy.context.view_layer.objects.active = ob
ob.select_set(True)
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="SELECT")
bpy.ops.uv.select_all(action="SELECT")
bpy.ops.uv.pack_islands(rotate=True, margin=0.008)
bpy.ops.object.mode_set(mode="OBJECT")
uvs = np.array([l.uv[:] for l in ob.data.uv_layers.active.data])
log("uv islands", len(groups_of_faces), "range", uvs.min(0).round(3), uvs.max(0).round(3))
mat = bpy.data.materials.new(f"{NAME}")
mat.diffuse_color = (0.6, 0.6, 0.6, 1)
ob.data.materials.append(mat)

# ------------------------------------------------------------------ armature
arm_data = bpy.data.armatures.new(f"{NAME}_Rig")
arm = bpy.data.objects.new(f"{NAME}_Rig", arm_data)
bpy.context.scene.collection.objects.link(arm)
bpy.context.view_layer.objects.active = arm
bpy.ops.object.mode_set(mode="EDIT")
eb = arm_data.edit_bones


def add_bone(name, head, tail, parent=None, connect=False):
    b = eb.new(name)
    b.head, b.tail = Vector(head), Vector(tail)
    if (b.tail - b.head).length < 0.01:
        b.tail = b.head + Vector((0, 0, 0.02))
    if parent:
        b.parent = eb[parent]
        b.use_connect = connect
    return b


add_bone("Root", (0, 0, 0), (0, 0, 0.05))
add_bone("Body", BODY_CENTER, BODY_CENTER + np.array([0, -0.08, 0.0]), "Root")
if NAME == "Spider":
    j = contact([0], [13])
    top = shell_pts([13]).max(0)
    add_bone("Abdomen", j, np.array([j[0], top[1] * 0.9, j[2] + 0.1]), "Body")
    for fang, sid in (("Fang_A", 5), ("Fang_B", 17)):
        pts = shell_pts([sid])
        add_bone(fang, pts[pts[:, 2].argmax()], pts[pts[:, 2].argmin()], "Body")
else:
    j = contact([17, 22, 23, 24], [16])
    front_pt = shell_pts([17]).min(0)
    add_bone("Head", j, np.array([j[0], front_pt[1], j[2]]), "Body")
for lg in legs:
    for i, bn in enumerate(lg["bones"]):
        add_bone(bn, lg["joints"][i], lg["joints"][i + 1], "Body" if i == 0 else lg["bones"][i - 1], i > 0)
bpy.ops.object.mode_set(mode="OBJECT")

ob.parent = arm
mod = ob.modifiers.new("Armature", "ARMATURE")
mod.object = arm

# ------------------------------------------------------------------ animation helpers
REST = {b.name: b.matrix_local.copy() for b in arm.data.bones}
RREST = {n: m.to_3x3() for n, m in REST.items()}
PARENT = {b.name: (b.parent.name if b.parent else None) for b in arm.data.bones}
ORDER = [b.name for b in arm.data.bones]  # parents are listed before children
for pb in arm.pose.bones:
    pb.rotation_mode = "QUATERNION"


def rot_axis(axis, deg):
    return Matrix.Rotation(math.radians(deg), 3, Vector(axis).normalized())


def smooth(t):
    t = min(max(t, 0.0), 1.0)
    return t * t * (3 - 2 * t)


def curve(keys, u):
    """Smoothstep interpolation through (u, value) keys; keys are sorted by u."""
    if u <= keys[0][0]:
        return keys[0][1]
    for (u0, v0), (u1, v1) in zip(keys, keys[1:]):
        if u <= u1:
            return v0 + (v1 - v0) * smooth((u - u0) / (u1 - u0))
    return keys[-1][1]


def pivot_matrix(pivot, rotation3, offset=(0, 0, 0)):
    p = Vector(pivot)
    return Matrix.Translation(Vector(offset) + p) @ rotation3.to_4x4() @ Matrix.Translation(-p)


LEG = {}
for lg in legs:
    J = [Vector(j) for j in lg["joints"]]
    d = Vector((J[-1].x - J[0].x, J[-1].y - J[0].y, 0)).normalized()
    lift_axis = Z.cross(d).normalized()
    rel = J[-1] - J[0]
    back_sign = 1.0 if Z.cross(Vector((rel.x, rel.y, 0))).dot(-FRONT) >= 0 else -1.0
    LEG[lg["name"]] = {"axis": lift_axis, "back": back_sign, "bones": lg["bones"], "lg": lg}


def key_frame(frame, pose):
    """pose: {'root': 4x4 matrix, bone_name: 3x3 world rotation delta relative to the parent's delta}."""
    D = {}
    for name in ORDER:
        pb = arm.pose.bones[name]
        parent = PARENT[name]
        if name == "Root":
            M = pose.get("Root", Matrix.Identity(4))
            pb.matrix_basis = REST["Root"].inverted() @ M @ REST["Root"]
            D["Root"] = M.to_3x3()
            pb.keyframe_insert("location", frame=frame)
            pb.keyframe_insert("rotation_quaternion", frame=frame)
            continue
        E = pose.get(name, Matrix.Identity(3))
        Dp = D[parent]
        D[name] = E @ Dp
        local = RREST[name].inverted() @ Dp.inverted() @ E @ Dp @ RREST[name]
        pb.rotation_quaternion = local.to_quaternion()
        pb.keyframe_insert("rotation_quaternion", frame=frame)


def new_action(name, frames, loop):
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    arm.animation_data_create()
    arm.animation_data.action = act
    for f in range(frames + 1):
        u = (f % frames) / frames if loop else f / frames
        yield f, u


def leg_pose(pose, name, forward_deg, lift_deg, knee_deg=0.0):
    info = LEG[name]
    swing = rot_axis(Z, -info["back"] * forward_deg)
    lift = rot_axis(info["axis"], -lift_deg)
    pose[info["bones"][0]] = swing @ lift
    if len(info["bones"]) > 1 and knee_deg:
        pose[info["bones"][1]] = rot_axis(info["axis"], -knee_deg)


def groups():
    names = [l["name"] for l in legs]
    if NAME == "Spider":
        a = [n for n in names if (n[4] == "L") == (int(n[5:]) % 2 == 1)]
    else:
        ranked = sorted(legs, key=lambda l: l["joints"][-1][1])
        a = []
        for i, l in enumerate(ranked):
            front_row = i // 2
            if (l["side"] == "L") == (front_row % 2 == 0):
                a.append(l["name"])
    return a, [n for n in names if n not in a]


GROUP_A, GROUP_B = groups()
log("gait groups", GROUP_A, GROUP_B)
rear_y = max(j[1] for l in legs for j in l["joints"])
front_y = min(j[1] for l in legs for j in l["joints"])
front_legs = sorted(legs, key=lambda l: l["joints"][-1][1])[:2]
STRIDE = 16.0 if NAME == "Spider" else 18.0
LIFT = 12.0 if NAME == "Spider" else 16.0


def build_idle():
    N = 60
    for f, u in new_action("Idle", N, True):
        s = math.sin(2 * math.pi * u)
        pose = {"Root": Matrix.Translation((0, 0, 0.006 * s))}
        if NAME == "Spider":
            pose["Abdomen"] = rot_axis((1, 0, 0), 3.0 * math.sin(2 * math.pi * u + 1.0))
            pose["Fang_A"] = rot_axis((0, 1, 0), 4.0 * math.sin(4 * math.pi * u))
            pose["Fang_B"] = rot_axis((0, 1, 0), -4.0 * math.sin(4 * math.pi * u))
            for i, l in enumerate(front_legs):
                leg_pose(pose, l["name"], 3.0 * math.sin(2 * math.pi * u + i), 3.0 * max(0.0, math.sin(2 * math.pi * u + 2.5 * i)))
        else:
            pose["Head"] = rot_axis((1, 0, 0), 3.0 * math.sin(2 * math.pi * u + 0.8))
            pose["Body"] = rot_axis((1, 0, 0), 0.8 * s)
        key_frame(f, pose)


def build_walk():
    N = 24
    for f, u in new_action("Walk", N, True):
        pose = {"Root": Matrix.Translation((0, 0, 0.008 * abs(math.sin(2 * math.pi * u * 2)) - 0.004))}
        for name in LEG:
            ph = (u + (0.0 if name in GROUP_A else 0.5)) % 1.0
            if ph < 0.6:
                fwd, lift, knee = STRIDE * (1 - 2 * ph / 0.6), 0.0, 0.0
            else:
                k = (ph - 0.6) / 0.4
                fwd = STRIDE * (-1 + 2 * smooth(k))
                lift = LIFT * math.sin(math.pi * k)
                knee = lift * 0.6
            leg_pose(pose, name, fwd, lift, knee)
        if NAME == "Spider":
            pose["Abdomen"] = rot_axis((1, 0, 0), 2.0 * math.sin(2 * math.pi * u * 2))
            pose["Fang_A"] = rot_axis((0, 1, 0), 3.0 * math.sin(2 * math.pi * u * 2))
            pose["Fang_B"] = rot_axis((0, 1, 0), -3.0 * math.sin(2 * math.pi * u * 2))
        else:
            pose["Head"] = rot_axis((1, 0, 0), 2.5 * math.sin(2 * math.pi * u * 2))
            pose["Body"] = rot_axis((0, 1, 0), 1.5 * math.sin(2 * math.pi * u))
        key_frame(f, pose)


def build_attack():
    N = 24 if NAME == "Spider" else 30
    pivot_back = (0, rear_y, 0)
    pivot_front = (0, front_y, 0)
    for f, u in new_action("Attack", N, False):
        pose = {}
        if NAME == "Spider":
            pitch = curve([(0, 0), (0.30, -12), (0.45, 4), (0.55, 4), (1, 0)], u)
            pose["Root"] = pivot_matrix(pivot_back, rot_axis((1, 0, 0), pitch))
            raise_ = curve([(0, 0), (0.30, 38), (0.42, 0), (0.55, 0), (1, 0)], u)
            fwd = curve([(0, 0), (0.30, 8), (0.42, 22), (0.55, 18), (1, 0)], u)
            spread = curve([(0, 0), (0.30, 1), (0.45, 0.4), (1, 0)], u)
            for i, l in enumerate(front_legs):
                leg_pose(pose, l["name"], fwd + (8 if i else -8) * spread, raise_, raise_ * 0.5)
            fang = curve([(0, 0), (0.30, 18), (0.42, -6), (1, 0)], u)
            pose["Fang_A"] = rot_axis((0, 1, 0), fang)
            pose["Fang_B"] = rot_axis((0, 1, 0), -fang)
            pose["Abdomen"] = rot_axis((1, 0, 0), curve([(0, 0), (0.30, 6), (0.45, -4), (1, 0)], u))
        else:
            back_pitch = curve([(0, 0), (0.33, -14), (0.50, 0), (1, 0)], u)
            front_pitch = curve([(0, 0), (0.33, 0), (0.50, 13), (0.62, 13), (1, 0)], u)
            lunge = curve([(0, 0), (0.33, -0.03), (0.50, 0.34), (0.62, 0.30), (1, 0)], u)
            M = pivot_matrix(pivot_back, rot_axis((1, 0, 0), back_pitch)) @ pivot_matrix(pivot_front, rot_axis((1, 0, 0), front_pitch), (0, -lunge, 0))
            pose["Root"] = M
            pose["Head"] = rot_axis((1, 0, 0), curve([(0, 0), (0.33, -12), (0.50, 16), (0.62, 16), (1, 0)], u))
            sweep = curve([(0, 0), (0.33, -6), (0.50, 12), (0.62, 10), (1, 0)], u)
            for name in LEG:
                leg_pose(pose, name, sweep, curve([(0, 0), (0.33, 6), (0.50, 3), (1, 0)], u))
        key_frame(f, pose)


bpy.context.view_layer.objects.active = arm
bpy.ops.object.mode_set(mode="POSE")
bpy.context.scene.render.fps = FPS
bpy.context.preferences.edit.keyframe_new_interpolation_type = "LINEAR"
build_idle()
build_walk()
build_attack()
bpy.ops.object.mode_set(mode="OBJECT")
arm.animation_data.action = bpy.data.actions["Idle"]
log("actions", [(a.name, tuple(a.frame_range)) for a in bpy.data.actions])

# ------------------------------------------------------------------ export
import os

os.makedirs(OUT, exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
arm.select_set(True)
ob.select_set(True)
bpy.context.view_layer.objects.active = arm
bpy.ops.export_scene.fbx(
    filepath=f"{OUT}/{NAME}.fbx",
    use_selection=True,
    object_types={"ARMATURE", "MESH"},
    add_leaf_bones=False,
    bake_anim=True,
    bake_anim_use_all_actions=True,
    bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True,
    bake_anim_simplify_factor=0.0,
    apply_scale_options="FBX_SCALE_ALL",
    apply_unit_scale=True,
    global_scale=1.0,
    mesh_smooth_type="OFF",
    use_mesh_modifiers=True,
    path_mode="COPY",
    embed_textures=False,
)
log("exported", f"{OUT}/{NAME}.fbx")
bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/{NAME}_debug.blend")
