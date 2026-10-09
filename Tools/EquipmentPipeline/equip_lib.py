"""Shared helpers for the equipment rigging pipeline (Blender bpy module or blender -b)."""
import bpy  # noqa: I001  (bpy must load before bmesh)
import bmesh
import numpy as np
from mathutils import Matrix
from mathutils.bvhtree import BVHTree

BODY_PARTS = {
    "Body_Torso": "Torso",
    "Body_Pelvis": "Pelvis",
    "Body_UpperArm_L": "UpperArmLeft",
    "Body_UpperArm_R": "UpperArmRight",
    "Body_Hand_L": "HandLeft",
    "Body_Hand_R": "HandRight",
    "Body_Leg_L": "LegLeft",
    "Body_Leg_R": "LegRight",
    "Body_Foot_L": "FootLeft",
    "Body_Foot_R": "FootRight",
}
HEAD_PART = "Body_Head"

CONFIDENT_COVERAGE = 0.85
CONFIDENT_VOTE = 0.04
UNCERTAIN_VOTE = 0.15
UNCERTAIN_COVERAGE = 0.15
NEAR_COVERAGE = 0.30
COVER_RADIUS = 0.04
MAX_INFLUENCES = 4
CLEAN_LIMIT = 0.02
SMOOTH_FACTOR = 0.5
SMOOTH_REPEAT = 3


def reset_scene():
    """Empties the Blender session."""
    bpy.ops.wm.read_factory_settings(use_empty=True)


def import_fbx(path):
    """Imports an FBX and returns the newly created objects."""
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path)
    return [o for o in bpy.data.objects if o not in before]


def world_vertices(obj):
    """Returns the vertices of a mesh object in world space as an (n, 3) array."""
    mw = np.array(obj.matrix_world)
    co = np.empty(len(obj.data.vertices) * 3)
    obj.data.vertices.foreach_get("co", co)
    co = co.reshape(-1, 3)
    return co @ mw[:3, :3].T + mw[:3, 3]


def build_bvh(obj):
    """Builds a BVH tree of a mesh object in world space."""
    mw = obj.matrix_world
    verts = [mw @ v.co for v in obj.data.vertices]
    polys = [tuple(p.vertices) for p in obj.data.polygons]
    return BVHTree.FromPolygons(verts, polys)


def nearest_distances(tree, points):
    """Distance from each point to the closest surface point of a BVH tree."""
    out = np.empty(len(points))
    for i, p in enumerate(points):
        loc, _, _, dist = tree.find_nearest(p.tolist())
        out[i] = dist if loc is not None else 1e9
    return out


def body_objects():
    """Returns ({BodyPart name: Body_* object}, head object or None) from the scene."""
    parts, head = {}, None
    for obj in bpy.data.objects:
        if obj.type != "MESH":
            continue
        if obj.name in BODY_PARTS:
            parts[BODY_PARTS[obj.name]] = obj
        elif obj.name == HEAD_PART:
            head = obj
    return parts, head


def analyze(item_obj, parts, head):
    """Measures how an item relates to each body region.

    coverage: share of the region's vertices within COVER_RADIUS of the item surface.
    votes: share of the item's vertices whose nearest body surface is that region.
    confident: coverage and votes both high, the item replaces that region.
    uncertain: the item is near the region but does not clearly replace it, needs a human call.
    """
    item_pts = world_vertices(item_obj)
    item_tree = build_bvh(item_obj)
    names = list(parts.keys())
    trees = {n: build_bvh(parts[n]) for n in names}
    objs = dict(parts)
    if head is not None:
        trees["Head"] = build_bvh(head)
        objs["Head"] = head

    dist = {n: nearest_distances(t, item_pts) for n, t in trees.items()}
    keys = list(dist.keys())
    stack = np.stack([dist[k] for k in keys], axis=1)
    nearest = np.argmin(stack, axis=1)
    votes = {k: float(np.mean(nearest == i)) for i, k in enumerate(keys)}
    coverage = {
        k: float(np.mean(nearest_distances(item_tree, world_vertices(objs[k])) <= COVER_RADIUS))
        for k in keys
    }

    confident = [n for n in names if coverage[n] >= CONFIDENT_COVERAGE and votes[n] >= CONFIDENT_VOTE]
    uncertain = [
        n for n in names
        if n not in confident and votes[n] >= UNCERTAIN_VOTE and coverage[n] >= UNCERTAIN_COVERAGE
    ]
    near = [
        n for n in names
        if n not in confident and n not in uncertain and coverage[n] >= NEAR_COVERAGE and votes[n] >= CONFIDENT_VOTE
    ]
    return {
        "near": near,
        "votes": {k: round(v, 3) for k, v in votes.items()},
        "coverage": {k: round(v, 3) for k, v in coverage.items()},
        "confident": confident,
        "uncertain": uncertain,
        "mean_distance": round(float(stack.min(axis=1).mean()), 4),
        "bbox_min": item_pts.min(axis=0).round(4).tolist(),
        "bbox_max": item_pts.max(axis=0).round(4).tolist(),
    }


def slot_for(covered):
    """Maps a covered-part list to (equipment slot, output folder)."""
    s = set(covered)
    torso_group = {"Torso", "UpperArmLeft", "UpperArmRight"}
    leg_group = {"Pelvis", "LegLeft", "LegRight"}
    foot_group = {"FootLeft", "FootRight"}
    hand_group = {"HandLeft", "HandRight"}
    if s & {"Torso"}:
        return "Body", "Torso"
    if s & leg_group:
        return "Pants", "Legs"
    if s & foot_group:
        return "Boots", "Feet"
    if s & hand_group:
        return "Gloves", "Hand"
    if s & torso_group:
        return "Body", "Arms"
    return None, None


def prepare_item(objs, name):
    """Bakes transforms, drops parents, rigs and modifiers, and joins the meshes into one object."""
    meshes = [o for o in objs if o.type == "MESH"]
    if not meshes:
        raise RuntimeError("the input has no mesh")
    for o in meshes:
        mw = o.matrix_world.copy()
        o.parent = None
        o.data.transform(mw)
        o.matrix_world = Matrix.Identity(4)
        o.data.update()
        for m in list(o.modifiers):
            o.modifiers.remove(m)
        o.vertex_groups.clear()
        if o.data.shape_keys:
            o.shape_key_clear()
    for o in objs:
        if o.type != "MESH":
            bpy.data.objects.remove(o, do_unlink=True)
    bpy.ops.object.select_all(action="DESELECT")
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    item = bpy.context.view_layer.objects.active
    item.name = name
    item.data.name = name
    return item


def mirror_in_place(item):
    """Adds an X-mirrored copy of the item geometry, so a one-sided piece becomes a pair."""
    bm = bmesh.new()
    bm.from_mesh(item.data)
    mirrored = bm.copy()
    bmesh.ops.scale(mirrored, vec=(-1, 1, 1), verts=mirrored.verts)
    bmesh.ops.reverse_faces(mirrored, faces=mirrored.faces)
    tmp = bpy.data.meshes.new("mirror_tmp")
    mirrored.to_mesh(tmp)
    mirrored.free()
    bm.free()
    tmp_obj = bpy.data.objects.new("mirror_tmp", tmp)
    bpy.context.scene.collection.objects.link(tmp_obj)
    bpy.ops.object.select_all(action="DESELECT")
    item.select_set(True)
    tmp_obj.select_set(True)
    bpy.context.view_layer.objects.active = item
    bpy.ops.object.join()


def is_one_sided(item, tolerance=0.01):
    """True when the whole item sits on one side of the body midline."""
    x = world_vertices(item)[:, 0]
    return bool(x.min() > -tolerance or x.max() < tolerance)


def transfer_weights(item, ref_parts, armature):
    """Copies skin weights from the nearest reference surface, then cleans them.

    Order matches the manual guide: interpolate, smooth, limit to 4 bones, clean small values,
    normalize.
    """
    bones = [b.name for b in armature.data.bones]
    bone_index = {b: i for i, b in enumerate(bones)}

    ref_verts, ref_tris, ref_w = [], [], []
    offset = 0
    for obj in ref_parts:
        mw = obj.matrix_world
        mesh = obj.data
        w = np.zeros((len(mesh.vertices), len(bones)))
        for v in mesh.vertices:
            for g in v.groups:
                name = obj.vertex_groups[g.group].name
                if name in bone_index:
                    w[v.index, bone_index[name]] = g.weight
        ref_verts.extend([mw @ v.co for v in mesh.vertices])
        mesh.calc_loop_triangles()
        ref_tris.extend([tuple(offset + i for i in t.vertices) for t in mesh.loop_triangles])
        ref_w.append(w)
        offset += len(mesh.vertices)
    ref_w = np.vstack(ref_w)
    ref_np = np.array([v[:] for v in ref_verts])
    tree = BVHTree.FromPolygons(ref_verts, ref_tris)

    pts = world_vertices(item)
    weights = np.zeros((len(pts), len(bones)))
    for i, p in enumerate(pts):
        loc, _, tri, _ = tree.find_nearest(p.tolist())
        a, b, c = (ref_np[j] for j in ref_tris[tri])
        u, v, w_ = barycentric(np.array(loc), a, b, c)
        i0, i1, i2 = ref_tris[tri]
        weights[i] = u * ref_w[i0] + v * ref_w[i1] + w_ * ref_w[i2]

    weights = smooth(item, weights)
    weights = limit_total(weights, MAX_INFLUENCES)
    weights = clean(weights, CLEAN_LIMIT)
    weights = normalize(weights)

    item.vertex_groups.clear()
    groups = {}
    for bi in np.nonzero(weights.sum(axis=0) > 0)[0]:
        groups[bi] = item.vertex_groups.new(name=bones[bi])
    for vi in range(len(pts)):
        for bi in np.nonzero(weights[vi] > 0)[0]:
            groups[bi].add([vi], float(weights[vi, bi]), "REPLACE")
    return {
        "groups": len(groups),
        "max_influences": int((weights > 0).sum(axis=1).max()),
        "unweighted_vertices": int((weights.sum(axis=1) == 0).sum()),
    }


def barycentric(p, a, b, c):
    """Barycentric coordinates of the point p in the triangle abc."""
    v0, v1, v2 = b - a, c - a, p - a
    d00, d01, d11 = v0 @ v0, v0 @ v1, v1 @ v1
    d20, d21 = v2 @ v0, v2 @ v1
    den = d00 * d11 - d01 * d01
    if abs(den) < 1e-18:
        return 1.0, 0.0, 0.0
    v = (d11 * d20 - d01 * d21) / den
    w = (d00 * d21 - d01 * d20) / den
    u = 1.0 - v - w
    u, v, w = max(u, 0.0), max(v, 0.0), max(w, 0.0)
    s = u + v + w
    return u / s, v / s, w / s


def smooth(item, weights):
    """Averages each vertex weight with its edge neighbours, SMOOTH_REPEAT times."""
    edges = np.empty(len(item.data.edges) * 2, dtype=np.int64)
    item.data.edges.foreach_get("vertices", edges)
    edges = edges.reshape(-1, 2)
    n = len(weights)
    for _ in range(SMOOTH_REPEAT):
        acc = np.zeros_like(weights)
        cnt = np.zeros(n)
        np.add.at(acc, edges[:, 0], weights[edges[:, 1]])
        np.add.at(acc, edges[:, 1], weights[edges[:, 0]])
        np.add.at(cnt, edges[:, 0], 1)
        np.add.at(cnt, edges[:, 1], 1)
        mean = np.divide(acc, cnt[:, None], out=weights.copy(), where=cnt[:, None] > 0)
        weights = weights * (1 - SMOOTH_FACTOR) + mean * SMOOTH_FACTOR
    return weights


def limit_total(weights, limit):
    """Keeps the strongest `limit` influences of every vertex."""
    out = weights.copy()
    drop = np.argsort(-out, axis=1)[:, limit:]
    np.put_along_axis(out, drop, 0.0, axis=1)
    return out


def clean(weights, threshold):
    """Removes influences below the threshold but always keeps the strongest one."""
    out = weights.copy()
    strongest = np.argmax(out, axis=1)
    keep = out >= threshold
    keep[np.arange(len(out)), strongest] = True
    out[~keep] = 0.0
    return out


def normalize(weights):
    """Rescales every vertex so its weights sum to 1."""
    total = weights.sum(axis=1, keepdims=True)
    return np.divide(weights, total, out=np.zeros_like(weights), where=total > 0)


def weight_to_bone(item, armature, bone_name):
    """Binds every vertex fully to one bone, used for headgear that must follow the head rigidly."""
    if bone_name not in armature.data.bones:
        raise RuntimeError("armature has no bone " + bone_name)
    item.vertex_groups.clear()
    group = item.vertex_groups.new(name=bone_name)
    group.add(list(range(len(item.data.vertices))), 1.0, "REPLACE")
    return {"groups": 1, "max_influences": 1, "unweighted_vertices": 0}


def name_materials(item, base_name):
    """Gives the item materials the set name, so Unity maps them to one shared toon material."""
    mats = [m for m in item.data.materials if m]
    for i, m in enumerate(mats):
        m.name = base_name if len(mats) == 1 else "%s_%d" % (base_name, i + 1)


def attach_armature(item, armature):
    """Parents the item to the armature with an Armature modifier."""
    item.parent = armature
    item.matrix_parent_inverse = Matrix.Identity(4)
    for m in list(item.modifiers):
        item.modifiers.remove(m)
    mod = item.modifiers.new("Armature", "ARMATURE")
    mod.object = armature


def export_fbx(path, objects):
    """Exports the given objects with the settings the project uses for equipment."""
    bpy.ops.object.select_all(action="DESELECT")
    for o in objects:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="STRIP",
        mesh_smooth_type="FACE",
    )
