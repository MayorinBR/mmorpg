import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bpy
from mathutils import Vector

import build
import lib


def render(names, path, cols=4, cell=18.0, res=(1400, 900), pitch=38, yaw=-30, samples=24, ortho=None):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    import importlib
    for mod in ("houses", "buildings", "props"):
        if os.path.exists(os.path.join(os.path.dirname(lib.__file__), mod + ".py")):
            importlib.import_module(mod)
    items = [r for r in lib.REGISTRY if r[1] in names] if names else list(lib.REGISTRY)
    rows = (len(items) + cols - 1) // cols
    for i, (_, name, _, _, fn) in enumerate(items):
        o = fn().finalize()
        o.location = ((i % cols) * cell, -(i // cols) * cell, 0)
    bpy.ops.mesh.primitive_plane_add(size=1, location=((cols - 1) * cell / 2, -(rows - 1) * cell / 2, -0.02))
    g = bpy.context.object
    g.scale = (cols * cell + 10, rows * cell + 10, 1)
    gm = bpy.data.materials.new("g")
    gm.diffuse_color = (0.8, 0.68, 0.5, 1)
    gm.use_nodes = True
    gm.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.50, 0.40, 0.28, 1)
    g.data.materials.append(gm)
    sun = bpy.data.objects.new("sun", bpy.data.lights.new("sun", "SUN"))
    sun.data.energy = 3.0
    sun.rotation_euler = (math.radians(50), math.radians(10), math.radians(-40))
    bpy.context.scene.collection.objects.link(sun)
    w = bpy.data.worlds.new("w")
    w.use_nodes = True
    w.node_tree.nodes["Background"].inputs["Color"].default_value = (0.75, 0.85, 1.0, 1)
    w.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.8
    bpy.context.scene.world = w
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
    bpy.context.scene.collection.objects.link(cam)
    bpy.context.scene.camera = cam
    center = Vector(((cols - 1) * cell / 2, -(rows - 1) * cell / 2, 2))
    span = max(cols * cell, rows * cell * 1.4)
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = ortho or span * 0.95
    dist = 120
    p, y = math.radians(pitch), math.radians(yaw)
    cam.location = center + Vector((math.sin(y) * math.cos(p), -math.cos(y) * math.cos(p), math.sin(p))) * dist
    cam.rotation_euler = (math.radians(90 - pitch), 0, y)
    sc = bpy.context.scene
    sc.view_settings.view_transform = "Standard"
    sc.render.engine = "CYCLES"
    sc.cycles.samples = samples
    sc.cycles.use_denoising = False
    sc.render.resolution_x, sc.render.resolution_y = res
    sc.render.filepath = path
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    out = sys.argv[1]
    names = sys.argv[2:]
    render(names, out)
