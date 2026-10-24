import math

from lib import *


def bone(b, p0, p1, w=0.06):
    b.beam(p0, p1, w, "bone")
    for p in (p0, p1):
        b.box(p[0], p[1], p[2] - w * 0.9, w * 1.9, w * 1.9, w * 1.8, "bone")


def skull(b, x, y, z, s=1.0, rot=0.0, color="bone"):
    with b.at(x, y, z, rot):
        b.lathe(0, 0, 0.04 * s, [(0.1 * s, 0), (0.13 * s, 0.09 * s), (0.11 * s, 0.19 * s), (0.06 * s, 0.25 * s), (0, 0.27 * s)], 6, color)
        b.box(0, -0.06 * s, 0, 0.13 * s, 0.11 * s, 0.07 * s, "bone_dark")
        for sx in (-1, 1):
            b.box(sx * 0.045 * s, -0.115 * s, 0.13 * s, 0.05 * s, 0.03 * s, 0.05 * s, "black")
        b.box(0, -0.12 * s, 0.1 * s, 0.025 * s, 0.03 * s, 0.04 * s, "black")


def candle(b, x, y, z, h=0.2, r=0.05, lit=True):
    b.cyl(x, y, z, r, h, 5, "wax")
    if lit:
        b.lathe(x, y, z + h, [(r * 0.7, 0), (r * 0.9, r * 1.6), (0, r * 3.2)], 4, "flame")


def flame(b, x, y, z, r=0.14, h=0.5):
    b.lathe(x, y, z, [(r, 0), (r * 1.2, h * 0.25), (r * 0.6, h * 0.65), (0, h)], 5, "flame")
    b.lathe(x, y, z, [(r * 0.55, 0), (r * 0.6, h * 0.2), (0, h * 0.55)], 4, "ember")


@asset("decor", 30, 600)
def torch_standing():
    b = Builder("torch_standing")
    for k in range(3):
        a = 2 * math.pi * k / 3
        b.beam((math.cos(a) * 0.5, math.sin(a) * 0.5, 0.0), (0, 0, 1.45), 0.07, "iron_dark")
    b.cyl(0, 0, 1.4, 0.06, 0.4, 5, "iron")
    b.lathe(0, 0, 1.75, [(0.12, 0), (0.28, 0.2), (0.3, 0.3), (0.24, 0.3)], 8, "iron_dark")
    flame(b, 0, 0, 2.0, 0.2, 0.7)
    return b


@asset("decor", 30, 600)
def brazier_bowl():
    b = Builder("brazier_bowl")
    b.cyl(0, 0, 0, 0.45, 0.2, 8, "stone_dark")
    b.cyl(0, 0, 0.2, 0.22, 0.8, 8, "stone", r2=0.18)
    b.lathe(0, 0, 1.0, [(0.18, 0), (0.55, 0.12), (0.7, 0.4), (0.62, 0.4), (0.0, 0.3)], 8, "iron_dark")
    b.lathe(0, 0, 1.2, [(0.5, 0), (0.45, 0.12), (0, 0.15)], 8, "ember")
    flame(b, 0, 0, 1.3, 0.34, 0.9)
    return b


@asset("decor", 30, 600)
def candelabra_floor():
    b = Builder("candelabra_floor")
    b.cyl(0, 0, 0, 0.35, 0.12, 6, "iron_dark")
    b.cyl(0, 0, 0.12, 0.05, 1.3, 5, "iron_dark")
    b.beam((-0.45, 0, 1.15), (0.45, 0, 1.15), 0.05, "iron")
    for x, z in ((-0.45, 1.15), (0.0, 1.45), (0.45, 1.15)):
        b.cyl(x, 0, z, 0.09, 0.04, 5, "iron")
        candle(b, x, 0, z + 0.04, 0.22 if x else 0.3)
    b.cyl(0, 0, 1.4, 0.09, 0.04, 5, "iron")
    return b


@asset("decor", 30, 600)
def candles_cluster():
    b = Builder("candles_cluster")
    b.cyl(0, 0, 0, 0.5, 0.03, 8, "wax")
    for i, (x, y, h) in enumerate([(0, 0, 0.35), (0.25, 0.1, 0.2), (-0.2, 0.15, 0.28), (0.05, -0.25, 0.15), (-0.15, -0.2, 0.22)]):
        candle(b, x, y, 0.03, h, 0.06 if i == 0 else 0.045)
    return b


@asset("decor", 30, 600)
def chandelier_iron():
    b = Builder("chandelier_iron")
    n = 8
    for k in range(n):
        a0, a1 = 2 * math.pi * k / n, 2 * math.pi * (k + 1) / n
        b.beam((0.9 * math.cos(a0), 0.9 * math.sin(a0), 0.0), (0.9 * math.cos(a1), 0.9 * math.sin(a1), 0.0), 0.09, "iron_dark")
    for k in range(6):
        a = 2 * math.pi * k / 6
        candle(b, 0.9 * math.cos(a), 0.9 * math.sin(a), 0.05, 0.22)
        b.beam((0.9 * math.cos(a), 0.9 * math.sin(a), 0.0), (0, 0, 0.9), 0.04, "iron")
    b.beam((0, 0, 0.9), (0, 0, 2.2), 0.06, "iron")
    return b


@asset("decor", 30, 700)
def chains_hanging():
    b = Builder("chains_hanging")
    for sx, ln in ((-0.3, 2.4), (0.0, 1.9), (0.3, 2.2)):
        n = int(ln / 0.16)
        for i in range(n):
            z = ln - (i + 1) * 0.16
            b.box(sx, 0, z, 0.05 if i % 2 == 0 else 0.12, 0.12 if i % 2 == 0 else 0.05, 0.14, "iron")
    b.box(0, 0, 2.4, 0.9, 0.08, 0.06, "iron_dark")
    return b


@asset("decor", 30, 900)
def skull_pile():
    b = Builder("skull_pile")
    rnd = random.Random(5)
    for i, (x, y, z, s) in enumerate([(0, 0, 0, 1.6), (0.35, 0.1, 0, 1.5), (-0.35, 0.1, 0, 1.5), (0.1, -0.3, 0, 1.4), (0.0, 0.05, 0.22, 1.5), (0.3, -0.15, 0.2, 1.4), (-0.25, -0.1, 0.2, 1.3), (0.05, -0.05, 0.42, 1.4)]):
        skull(b, x, y, z, s, rot=rnd.uniform(-1.5, 1.5))
    bone(b, (-0.6, -0.5, 0.05), (0.2, -0.7, 0.05))
    bone(b, (0.4, 0.5, 0.05), (0.8, -0.2, 0.05))
    return b


@asset("decor", 30, 600)
def bones_scatter_a():
    b = Builder("bones_scatter_a")
    bone(b, (-0.6, -0.2, 0.03), (0.2, 0.1, 0.03), 0.07)
    bone(b, (0.1, -0.5, 0.03), (0.7, 0.3, 0.03), 0.06)
    bone(b, (-0.3, 0.4, 0.03), (0.3, 0.6, 0.03), 0.06)
    skull(b, -0.7, 0.35, 0.0, 1.5, rot=0.9)
    return b


@asset("decor", 30, 600)
def bones_scatter_b():
    b = Builder("bones_scatter_b")
    bone(b, (-0.5, 0.0, 0.03), (0.5, 0.2, 0.03), 0.07)
    bone(b, (-0.2, -0.4, 0.03), (0.4, -0.5, 0.03), 0.06)
    bone(b, (0.0, 0.5, 0.03), (0.6, 0.6, 0.03), 0.05)
    for k in range(4):
        b.box(-0.3 + k * 0.2, -0.7, 0.0, 0.1, 0.05, 0.03, "bone_dark")
    return b


@asset("decor", 30, 600)
def ribcage():
    b = Builder("ribcage")
    b.beam((0, -1.0, 0.1), (0, 1.0, 0.1), 0.09, "bone")
    for i in range(5):
        y = -0.7 + i * 0.35
        rz = 0.55 - abs(i - 2) * 0.07
        prev = None
        for k in range(7):
            a = math.pi * k / 6
            p = (0.5 * math.cos(a) * (1 - abs(i - 2) * 0.1), y, 0.1 + rz * math.sin(a))
            if prev:
                b.beam(prev, p, 0.07, "bone")
            prev = p
    skull(b, 0, -1.25, 0.0, 2.0)
    return b


@asset("decor", 30, 700)
def skeleton_lying():
    b = Builder("skeleton_lying")
    b.beam((0, -0.7, 0.07), (0, 0.45, 0.07), 0.09, "bone")
    for i in range(4):
        y = -0.5 + i * 0.2
        b.beam((-0.28, y, 0.1), (0.28, y, 0.1), 0.05, "bone")
    b.box(0, 0.55, 0.0, 0.4, 0.22, 0.08, "bone")
    skull(b, 0, -0.9, 0.0, 1.8)
    bone(b, (-0.3, -0.5, 0.05), (-0.65, 0.1, 0.05))
    bone(b, (0.3, -0.5, 0.05), (0.75, -0.9, 0.05))
    bone(b, (-0.15, 0.6, 0.05), (-0.3, 1.5, 0.05), 0.07)
    bone(b, (0.15, 0.6, 0.05), (0.5, 1.4, 0.05), 0.07)
    return b


@asset("decor", 30, 700)
def skeleton_chained():
    b = Builder("skeleton_chained")
    y = -0.04
    b.box(0, y, 1.0, 0.36, 0.08, 0.62, "bone_dark")
    b.beam((0, y, 0.7), (0, y, 1.6), 0.08, "bone")
    for i in range(4):
        z = 1.0 + i * 0.14
        b.beam((-0.25, y - 0.03, z), (0.25, y - 0.03, z), 0.045, "bone")
    skull(b, 0, y - 0.04, 1.65, 1.6)
    bone(b, (-0.3, y, 1.5), (-0.7, y, 2.3))
    bone(b, (0.3, y, 1.5), (0.7, y, 2.3))
    bone(b, (-0.15, y, 0.7), (-0.25, y, 0.0))
    bone(b, (0.15, y, 0.7), (0.3, y, 0.05))
    for sx in (-1, 1):
        b.box(sx * 0.7, y - 0.04, 2.3, 0.18, 0.08, 0.14, "iron")
        b.beam((sx * 0.7, y - 0.04, 2.45), (sx * 0.7, y - 0.04, 2.9), 0.04, "iron_dark")
    return b


@asset("decor", 30, 800)
def coffin_closed():
    b = Builder("coffin_closed")
    pts = [(-0.4, -1.1), (0.4, -1.1), (0.55, -0.4), (0.4, 1.1), (-0.4, 1.1), (-0.55, -0.4)]
    b.slab(pts, 0.0, 0.55, "wood_dark")
    b.slab([(x * 1.05, y * 1.03) for x, y in pts], 0.55, 0.7, "wood")
    b.box(0, 0.0, 0.7, 0.1, 0.9, 0.04, "iron")
    b.box(0, -0.3, 0.7, 0.5, 0.1, 0.04, "iron")
    return b


@asset("decor", 30, 800)
def coffin_open():
    b = Builder("coffin_open")
    pts = [(-0.4, -1.1), (0.4, -1.1), (0.55, -0.4), (0.4, 1.1), (-0.4, 1.1), (-0.55, -0.4)]
    b.slab(pts, 0.0, 0.1, "wood_dark")
    for (p, q) in zip(pts, pts[1:] + pts[:1]):
        b.beam((p[0], p[1], 0.05), (q[0], q[1], 0.05), 0.1, "wood_dark")
        b.beam((p[0], p[1], 0.5), (q[0], q[1], 0.5), 0.1, "wood_dark")
        b.beam((p[0], p[1], 0.05), (p[0], p[1], 0.5), 0.1, "wood_dark")
    b.slab(pts, 0.1, 0.12, "cloth_red")
    b.slab([(x * 1.05, y * 1.03) for x, y in pts], 0.0, 0.15, "wood", x=0.95, y=-0.55, rot=0.55)
    bone(b, (-0.15, -0.3, 0.14), (0.2, 0.5, 0.14))
    skull(b, 0, -0.85, 0.1, 1.6)
    return b


@asset("decor", 30, 900)
def sarcophagus_stone():
    b = Builder("sarcophagus_stone")
    b.box(0, 0, 0, 1.5, 2.7, 0.35, "stone_dark")
    b.box(0, 0, 0.35, 1.3, 2.5, 0.6, "stone")
    b.frustum(0, 0, 0.95, 1.4, 2.6, 0.25, "trim", 1.0, 2.2)
    b.box(0, -0.2, 1.2, 0.5, 1.2, 0.06, "trim_dark")
    b.box(0, -0.2, 1.2, 0.14, 1.7, 0.09, "trim_dark")
    b.box(0, -0.6, 1.2, 0.8, 0.14, 0.08, "trim_dark")
    b.plate(circle_poly(0, 1.15, 0.2, 6), -1.28, -1.24, "bone")
    return b


@asset("decor", 30, 900)
def sarcophagus_broken():
    b = Builder("sarcophagus_broken")
    b.box(0, 0, 0, 1.5, 2.7, 0.35, "stone_dark")
    b.box(0, 0, 0.35, 1.3, 2.5, 0.6, "stone")
    b.frustum(0, 0.5, 0.95, 1.4, 1.5, 0.25, "trim", 1.0, 1.2)
    b.box(0.9, -0.4, 0.0, 1.2, 0.9, 0.3, "trim", rot=0.6)
    b.rock(-0.3, -1.6, 0, 0.7, 0.6, 0.35, "trim_dark", 90, sides=5, rings=2)
    b.box(0, -0.3, 0.9, 0.9, 1.5, 0.04, "black")
    skull(b, 0, -0.5, 0.9, 1.5)
    return b


@asset("decor", 30, 500)
def tomb_slab():
    b = Builder("tomb_slab")
    b.box(0, 0, 0, 1.4, 2.4, 0.12, "stone_dark")
    b.box(0, 0, 0.12, 1.2, 2.2, 0.06, "stone")
    b.box(0, -0.1, 0.18, 0.12, 1.4, 0.03, "trim")
    b.box(0, 0.25, 0.18, 0.7, 0.12, 0.03, "trim")
    return b


@asset("decor", 30, 500)
def gravestone_a():
    b = Builder("gravestone_a")
    b.box(0, 0, 0, 0.9, 0.5, 0.18, "stone_dark")
    b.plate(arch_poly(0, 0.18, 0.7, 1.2, 4), -0.12, 0.12, "stone")
    b.box(0, -0.12, 0.9, 0.3, 0.03, 0.06, "trim_dark")
    b.box(0, -0.12, 0.7, 0.06, 0.03, 0.3, "trim_dark")
    return b


@asset("decor", 30, 500)
def gravestone_cross():
    b = Builder("gravestone_cross")
    b.box(0, 0, 0, 0.9, 0.6, 0.2, "stone_dark")
    b.box(0, 0, 0.2, 0.3, 0.2, 1.3, "stone")
    b.box(0, 0, 0.95, 0.9, 0.2, 0.28, "stone")
    b.rock(0.5, -0.4, 0, 0.3, 0.3, 0.2, "stone_dark", 91, sides=5, rings=2)
    return b


@asset("decor", 30, 500)
def gravestone_broken():
    b = Builder("gravestone_broken")
    b.box(0, 0, 0, 0.9, 0.5, 0.18, "stone_dark")
    b.frustum(0, 0, 0.18, 0.7, 0.24, 0.6, "stone", 0.5, 0.22)
    b.box(0.6, -0.4, 0.0, 0.5, 0.3, 0.25, "stone", rot=0.5)
    return b


@asset("decor", 30, 600)
def urn_large():
    b = Builder("urn_large")
    b.lathe(0, 0, 0, [(0.3, 0), (0.55, 0.3), (0.62, 0.65), (0.45, 1.0), (0.3, 1.15), (0.38, 1.25), (0.3, 1.3)], 8, "rock_warm")
    b.cyl(0, 0, 0.7, 0.62, 0.08, 8, "bronze")
    b.cyl(0, 0, 0.0, 0.34, 0.08, 8, "stone_dark")
    return b


@asset("decor", 30, 600)
def urn_small():
    b = Builder("urn_small")
    b.lathe(0, 0, 0, [(0.15, 0), (0.28, 0.2), (0.3, 0.4), (0.2, 0.6), (0.15, 0.7), (0.22, 0.78)], 7, "rock_warm")
    return b


@asset("decor", 60, 900)
def altar_stone():
    b = Builder("altar_stone")
    b.box(0, 0, 0, 3.2, 2.0, 0.25, "stone_dark")
    b.box(0, 0, 0.25, 2.8, 1.6, 0.25, "stone")
    b.box(0, 0, 0.5, 2.4, 1.2, 0.7, "stone")
    b.box(0, 0, 1.2, 2.7, 1.5, 0.18, "trim")
    for sx in (-1, 1):
        candle(b, sx * 1.0, 0, 1.38, 0.35, 0.07)
    b.cyl(0, 0, 1.38, 0.3, 0.1, 8, "bronze")
    b.lathe(0, 0, 1.48, [(0.28, 0), (0.12, 0.15)], 8, "bronze")
    return b


@asset("decor", 60, 1200)
def altar_ritual():
    b = Builder("altar_ritual")
    b.box(0, 0, 0, 3.0, 1.8, 0.3, "stone_dark")
    b.box(0, 0, 0.3, 2.6, 1.4, 0.8, "stone")
    b.box(0, 0, 1.1, 2.8, 1.6, 0.16, "trim")
    b.box(0, 0, 1.26, 1.4, 1.5, 0.03, "cloth_red")
    b.box(0, -0.8, 0.5, 1.3, 0.03, 0.12, "cloth_red")
    for sx in (-1, 1):
        b.box(sx * 1.1, -0.6, 1.26, 0.12, 0.8, 0.03, "rune_red")
        candle(b, sx * 1.1, 0.4, 1.26, 0.4, 0.07)
    skull(b, 0, 0.0, 1.29, 2.2)
    for rz in (0.55, 0.75):
        b.box(0, -0.72, rz, 1.8, 0.03, 0.05, "rune_red")
    b.plate(circle_poly(0, 0.85, 0.16, 6), -0.73, -0.7, "rune_red")
    return b


@asset("decor", 60, 1200)
def summoning_circle():
    b = Builder("summoning_circle")
    n = 16
    for k in range(n):
        a0, a1 = 2 * math.pi * k / n, 2 * math.pi * (k + 1) / n
        b.beam((1.8 * math.cos(a0), 1.8 * math.sin(a0), 0.015), (1.8 * math.cos(a1), 1.8 * math.sin(a1), 0.015), 0.14, "rune_red", d=0.02)
        b.beam((1.4 * math.cos(a0), 1.4 * math.sin(a0), 0.015), (1.4 * math.cos(a1), 1.4 * math.sin(a1), 0.015), 0.07, "rune_red", d=0.02)
    star = [(1.4 * math.cos(math.pi / 2 + 2 * math.pi * i * 2 / 5), 1.4 * math.sin(math.pi / 2 + 2 * math.pi * i * 2 / 5)) for i in range(6)]
    for p, q in zip(star, star[1:]):
        b.beam((p[0], p[1], 0.015), (q[0], q[1], 0.015), 0.09, "rune_red", d=0.02)
    for k in range(8):
        a = 2 * math.pi * k / 8
        b.box(1.62 * math.cos(a), 1.62 * math.sin(a), 0.0, 0.16, 0.16, 0.03, "rune_red", rot=a)
    return b


@asset("decor", 100, 1500)
def throne_dark():
    b = Builder("throne_dark")
    b.box(0, 0, 0, 3.6, 3.2, 0.3, "stone_dark")
    b.box(0, 0, 0.3, 3.2, 2.8, 0.3, "stone")
    b.box(0, 0, 0.6, 2.8, 2.4, 0.3, "flag_b")
    b.box(0, 0.2, 0.9, 1.5, 1.4, 0.5, "iron_dark")
    b.box(0, 0.25, 1.4, 1.3, 1.2, 0.14, "cloth_red")
    for sx in (-1, 1):
        b.box(sx * 0.85, 0.2, 0.9, 0.3, 1.2, 0.9, "iron_dark")
        b.box(sx * 0.85, 0.15, 1.8, 0.4, 0.4, 0.08, "iron")
    b.box(0, 0.8, 0.9, 1.6, 0.35, 2.7, "iron_dark")
    b.frustum(0, 0.8, 3.6, 1.6, 0.35, 0.8, "iron_dark", 0.8, 0.3)
    for sx in (-1, 0, 1):
        b.spike(sx * 0.55, 0.8, 3.6, 0.16, 0.9 + (0.3 if sx == 0 else 0), 5, "iron", seed=sx + 3)
    skull(b, 0, 0.58, 3.0, 2.6)
    b.box(-0.08, 0.57, 3.08, 0.07, 0.03, 0.07, "rune_red")
    b.box(0.08, 0.57, 3.08, 0.07, 0.03, 0.07, "rune_red")
    for sx in (-1, 1):
        b.box(sx * 0.48, 0.58, 1.5, 0.12, 0.05, 1.8, "rune_red")
        skull(b, sx * 1.1, -0.6, 0.9, 1.6, rot=-sx * 0.5)
    b.box(0, -1.35, 0.6, 1.8, 0.3, 0.14, "trim")
    return b


def statue_base(b, w=1.5, h=0.9):
    b.box(0, 0, 0, w + 0.3, w + 0.3, 0.25, "stone_dark")
    b.box(0, 0, 0.25, w, w, h - 0.4, "stone")
    b.box(0, 0, h - 0.15, w + 0.2, w + 0.2, 0.15, "trim")
    return h


@asset("decor", 100, 1500)
def statue_knight():
    b = Builder("statue_knight")
    z = statue_base(b)
    for sx in (-1, 1):
        b.box(sx * 0.2, 0, z, 0.28, 0.32, 0.95, "stone")
        b.box(sx * 0.2, -0.05, z + 0.0, 0.3, 0.45, 0.14, "stone_dark")
        b.box(sx * 0.46, 0, z + 1.55, 0.3, 0.34, 0.34, "trim")
        b.box(sx * 0.5, 0.0, z + 0.75, 0.22, 0.26, 0.85, "stone")
    b.box(0, 0, z + 0.95, 0.7, 0.4, 0.2, "stone_dark")
    b.frustum(0, 0, z + 1.15, 0.8, 0.45, 0.6, "stone", 0.95, 0.5)
    b.box(0, 0, z + 1.65, 0.45, 0.4, 0.14, "stone")
    b.box(0, 0, z + 1.8, 0.4, 0.4, 0.45, "trim")
    b.box(0, -0.21, z + 2.0, 0.34, 0.03, 0.05, "black")
    b.pyramid(0, 0, z + 2.25, 0.4, 0.4, 0.22, "stone_dark")
    b.beam((0.68, -0.55, z + 0.2), (0.68, -0.55, z + 2.1), 0.1, "steel")
    b.box(0.68, -0.55, z + 1.15, 0.5, 0.08, 0.08, "bronze")
    b.pyramid(0.68, -0.55, z + 2.1, 0.1, 0.06, 0.2, "steel")
    b.plate(circle_poly(-0.62, 1.1, 0.4, 6), -0.4, -0.32, "iron_dark", z=z)
    b.plate(circle_poly(-0.62, 1.1, 0.12, 5), -0.42, -0.32, "bronze", z=z)
    return b


@asset("decor", 100, 1500)
def statue_gargoyle():
    b = Builder("statue_gargoyle")
    z = statue_base(b, 1.4, 0.8)
    b.frustum(0, 0.05, z, 1.0, 1.0, 0.8, "stone_dark", 0.8, 0.7)
    b.frustum(0, -0.2, z + 0.6, 0.7, 0.6, 0.6, "stone", 0.5, 0.45)
    b.box(0, -0.45, z + 1.15, 0.4, 0.4, 0.4, "stone")
    b.frustum(0, -0.45, z + 1.5, 0.4, 0.4, 0.15, "stone_dark", 0.3, 0.3)
    for sx in (-1, 1):
        b.pyramid(sx * 0.18, -0.4, z + 1.65, 0.14, 0.14, 0.4, "stone_dark")
        b.box(sx * 0.1, -0.67, z + 1.35, 0.07, 0.04, 0.07, "ember")
        b.plate([(0, 0.6), (sx * 1.7, 1.5), (sx * 1.4, 0.4), (sx * 0.9, 0.7), (sx * 0.5, 0.2)], 0.3, 0.38, "stone_dark", z=z + 0.2)
        b.box(sx * 0.4, -0.3, z, 0.25, 0.4, 0.5, "stone")
    b.box(0, -0.6, z + 1.05, 0.25, 0.3, 0.1, "bone")
    return b


@asset("decor", 100, 1500)
def statue_reaper():
    b = Builder("statue_reaper")
    z = statue_base(b)
    b.lathe(0, 0, z, [(0.55, 0), (0.45, 0.6), (0.3, 1.3), (0.26, 1.6)], 8, "stone_dark")
    b.lathe(0, 0, z + 1.55, [(0.3, 0), (0.34, 0.2), (0.28, 0.45), (0.1, 0.6), (0, 0.62)], 8, "stone")
    b.box(0, -0.2, z + 1.7, 0.26, 0.12, 0.2, "black")
    for sx in (-1, 1):
        b.box(sx * 0.07, -0.26, z + 1.78, 0.06, 0.03, 0.05, "ember")
    b.beam((0.62, -0.35, z + 0.2), (0.62, -0.35, z + 2.7), 0.07, "wood_dark")
    b.plate([(0.62, 2.7), (0.2, 2.9), (-0.3, 2.7), (-0.5, 2.4), (-0.15, 2.6), (0.3, 2.55)], -0.38, -0.34, "steel", z=z)
    b.box(0.35, -0.3, z + 1.1, 0.5, 0.2, 0.18, "stone")
    return b


@asset("decor", 60, 900)
def statue_skull_totem():
    b = Builder("statue_skull_totem")
    b.cyl(0, 0, 0, 0.6, 0.3, 8, "stone_dark")
    b.cyl(0, 0, 0.3, 0.3, 2.2, 6, "wood_dark", r2=0.26)
    for i, z in enumerate((0.7, 1.3, 1.9)):
        skull(b, 0, -0.22, z, 2.2, rot=0.0)
        b.box(0, 0, z + 0.55, 0.6, 0.12, 0.12, "iron_dark")
    b.pyramid(0, 0, 2.5, 0.6, 0.6, 0.5, "bone_dark")
    return b


@asset("decor", 30, 700)
def banner_standing():
    b = Builder("banner_standing")
    for sx in (-1, 1):
        b.cyl(sx * 0.55, 0, 0, 0.05, 2.6, 5, "wood_dark")
        b.pyramid(sx * 0.55, 0, 2.6, 0.12, 0.12, 0.2, "iron")
    b.beam((-0.55, 0, 2.5), (0.55, 0, 2.5), 0.06, "iron_dark")
    b.plate([(-0.5, 2.5), (0.5, 2.5), (0.5, 1.0), (0.25, 0.8), (0.0, 1.1), (-0.25, 0.85), (-0.5, 1.15)], -0.02, 0.02, "cloth_purple")
    b.plate(circle_poly(0, 1.8, 0.22, 6), -0.05, -0.02, "gold")
    b.cyl(0, 0, 0, 0.7, 0.06, 6, "stone_dark")
    return b


@asset("decor", 30, 700)
def weapon_rack():
    b = Builder("weapon_rack")
    for sx in (-1, 1):
        b.box(sx * 1.1, 0, 0, 0.14, 0.5, 1.5, "wood_dark")
    b.box(0, 0, 0.35, 2.4, 0.12, 0.1, "wood")
    b.box(0, 0, 1.3, 2.4, 0.12, 0.1, "wood")
    for i, x in enumerate((-0.85, -0.45, -0.05)):
        b.beam((x, 0, 0.2), (x + 0.1, 0.0, 2.2), 0.05, "wood_dark")
        b.pyramid(x + 0.1, 0.0, 2.2, 0.12, 0.05, 0.25, "steel")
    for x in (0.4, 0.75):
        b.beam((x, 0, 0.4), (x, 0, 1.4), 0.06, "steel")
        b.box(x, 0, 0.42, 0.28, 0.05, 0.06, "bronze")
    b.plate(circle_poly(1.0, 0.8, 0.3, 6), -0.1, -0.05, "iron")
    b.plate(circle_poly(1.0, 0.8, 0.1, 5), -0.11, -0.05, "gold")
    return b


@asset("decor", 30, 800)
def armor_stand():
    b = Builder("armor_stand")
    b.cyl(0, 0, 0, 0.4, 0.08, 6, "wood_dark")
    b.beam((0, 0, 0.08), (0, 0, 1.4), 0.07, "wood_dark")
    b.frustum(0, 0, 0.9, 0.55, 0.35, 0.65, "iron", 0.7, 0.4)
    for sx in (-1, 1):
        b.box(sx * 0.42, 0, 1.4, 0.28, 0.3, 0.2, "iron_dark")
    b.box(0, 0, 1.5, 0.3, 0.3, 0.14, "iron")
    b.box(0, 0, 1.64, 0.34, 0.34, 0.36, "steel")
    b.box(0, -0.18, 1.82, 0.28, 0.03, 0.04, "black")
    b.pyramid(0, 0, 2.0, 0.34, 0.34, 0.14, "iron_dark")
    b.box(0, -0.02, 0.45, 0.5, 0.3, 0.5, "cloth_red")
    return b


@asset("decor", 30, 600)
def shackles_wall():
    b = Builder("shackles_wall")
    y = -0.06
    for sx in (-1, 1):
        b.box(sx * 0.55, y, 2.35, 0.18, 0.1, 0.18, "iron_dark")
        for i in range(6):
            b.box(sx * 0.55, y - 0.04, 2.2 - i * 0.14, 0.05 if i % 2 else 0.1, 0.1 if i % 2 else 0.05, 0.12, "iron")
        b.box(sx * 0.55, y - 0.06, 1.25, 0.3, 0.12, 0.12, "iron_dark")
        b.box(sx * 0.55, y - 0.06, 1.15, 0.3, 0.12, 0.05, "rust")
    return b


@asset("decor", 30, 700)
def hanging_cage():
    b = Builder("hanging_cage")
    n = 8
    for k in range(n):
        a = 2 * math.pi * k / n
        b.beam((0.5 * math.cos(a), 0.5 * math.sin(a), 0.1), (0.35 * math.cos(a), 0.35 * math.sin(a), 1.7), 0.04, "iron_dark")
    for z, r in ((0.1, 0.5), (0.9, 0.52), (1.7, 0.35)):
        for k in range(n):
            a0, a1 = 2 * math.pi * k / n, 2 * math.pi * (k + 1) / n
            b.beam((r * math.cos(a0), r * math.sin(a0), z), (r * math.cos(a1), r * math.sin(a1), z), 0.05, "iron")
    b.cyl(0, 0, 0.05, 0.5, 0.06, 8, "iron_dark")
    b.beam((0, 0, 1.7), (0, 0, 2.6), 0.05, "iron")
    skull(b, 0.1, 0, 0.11, 1.6)
    return b


@asset("decor", 30, 700)
def rubble_a():
    b = Builder("rubble_a")
    for i, (x, y, s, c) in enumerate([(0, 0, 1.0, "brick_a"), (0.8, 0.4, 0.7, "brick_b"), (-0.7, 0.5, 0.8, "brick_light"), (0.2, -0.8, 0.6, "brick_dark"), (-0.4, -0.5, 0.5, "brick_a")]):
        b.rock(x, y, 0, s, s * 0.85, s * 0.55, c, 100 + i, sides=6, rings=2)
    return b


@asset("decor", 30, 700)
def rubble_b():
    b = Builder("rubble_b")
    b.box(0, 0, 0, 0.9, 0.6, 0.4, "brick_a", rot=0.3)
    b.box(0.5, 0.6, 0, 0.7, 0.5, 0.3, "trim", rot=-0.5)
    b.box(-0.6, -0.5, 0, 0.6, 0.5, 0.25, "brick_b", rot=0.9)
    for i, (x, y, s) in enumerate([(0.6, -0.5, 0.4), (-0.7, 0.4, 0.35), (0.0, 0.8, 0.3)]):
        b.rock(x, y, 0, s, s, s * 0.6, "brick_dark", 110 + i, sides=5, rings=2)
    return b


@asset("decor", 30, 700)
def rubble_c():
    b = Builder("rubble_c")
    for i, (x, y, s, c) in enumerate([(0, 0, 1.6, "rock_a"), (1.0, 0.5, 0.9, "rock_c"), (-0.9, 0.5, 1.0, "rock_b"), (0.3, -1.0, 0.8, "rock_a"), (-0.4, 1.0, 0.6, "rock_warm"), (1.0, -0.8, 0.5, "rock_b")]):
        b.rock(x, y, 0, s, s * 0.9, s * 0.6, c, 120 + i, sides=7, rings=2)
    return b


@asset("decor", 30, 700)
def fallen_pillar():
    b = Builder("fallen_pillar")
    b.tube((-1.7, 0, 0.55), (1.5, 0.15, 0.55), 0.55, 8, "stone", r2=0.5)
    for x in (-1.0, 0.0, 1.0):
        b.tube((x - 0.12, 0.03 * x, 0.55), (x + 0.12, 0.03 * x + 0.02, 0.55), 0.6, 8, "trim")
    b.tube((1.5, 0.15, 0.5), (2.0, 0.3, 0.45), 0.45, 6, "stone_dark", r2=0.3)
    b.rock(-0.4, -1.0, 0, 0.8, 0.7, 0.4, "stone", 130, sides=6, rings=2)
    b.rock(0.9, 1.0, 0, 0.6, 0.5, 0.3, "stone_dark", 131, sides=5, rings=2)
    return b


@asset("decor", 30, 700)
def debris_planks():
    b = Builder("debris_planks")
    b.beam((-0.8, -0.3, 0.06), (0.7, 0.2, 0.06), 0.22, "wood_rot", d=0.06)
    b.beam((-0.5, 0.2, 0.06), (0.6, -0.5, 0.12), 0.2, "wood", d=0.06)
    b.beam((-0.2, 0.5, 0.06), (0.9, 0.6, 0.06), 0.18, "wood_rot", d=0.05)
    b.beam((0.1, -0.7, 0.04), (0.9, -0.2, 0.2), 0.16, "wood_dark", d=0.05)
    return b


@asset("decor", 30, 800)
def well_dry():
    b = Builder("well_dry")
    b.lathe(0, 0, 0, [(1.2, 0), (1.2, 0.8), (1.35, 0.8), (1.35, 0.95), (0.8, 0.95)], 8, "stone")
    b.cyl(0, 0, 0.7, 0.85, 0.2, 8, "black")
    for sx in (-1, 1):
        b.box(sx * 1.2, 0, 0.9, 0.2, 0.2, 1.8, "wood_dark")
    b.box(0, 0, 2.6, 2.8, 0.2, 0.2, "wood")
    b.beam((-0.3, 0, 2.5), (-0.3, 0, 1.6), 0.03, "iron_dark")
    b.cyl(-0.3, 0, 1.25, 0.2, 0.3, 6, "wood")
    return b


@asset("decor", 30, 800)
def cauldron_green():
    b = Builder("cauldron_green")
    for k in range(3):
        a = 2 * math.pi * k / 3 + 0.3
        b.beam((math.cos(a) * 0.5, math.sin(a) * 0.5, 0.0), (math.cos(a) * 0.2, math.sin(a) * 0.2, 0.5), 0.1, "iron_dark")
    b.lathe(0, 0, 0.4, [(0.35, 0), (0.7, 0.3), (0.75, 0.6), (0.65, 0.75), (0.55, 0.75)], 8, "iron_dark")
    b.cyl(0, 0, 1.0, 0.6, 0.02, 8, "slime")
    for i, (x, y) in enumerate([(0.2, 0.1), (-0.25, -0.15), (0.0, 0.3)]):
        b.rock(x, y, 1.0, 0.18, 0.18, 0.12, "slime", 140 + i, sides=5, rings=1)
    for i in range(4):
        a = 2 * math.pi * i / 4
        b.beam((math.cos(a) * 0.6, math.sin(a) * 0.6, 0.05), (math.cos(a + 0.8) * 0.2, math.sin(a + 0.8) * 0.2, 0.15), 0.1, "wood_dark")
    flame(b, 0.0, 0.0, 0.15, 0.18, 0.35)
    return b


@asset("decor", 30, 700)
def spike_trap():
    b = Builder("spike_trap")
    b.box(0, 0, -0.04, 2.0, 2.0, 0.08, "iron_dark")
    for i in range(4):
        for j in range(4):
            b.spike(-0.75 + i * 0.5, -0.75 + j * 0.5, 0.04, 0.09, 0.5, 4, "steel", seed=i * 4 + j)
    return b


@asset("decor", 30, 500)
def pressure_plate():
    b = Builder("pressure_plate")
    b.box(0, 0, -0.03, 1.4, 1.4, 0.06, "flag_dark")
    b.box(0, 0, 0.0, 1.0, 1.0, 0.04, "flag_b")
    b.box(0, 0, 0.04, 0.5, 0.06, 0.01, "rune_green")
    b.box(0, 0, 0.04, 0.06, 0.5, 0.01, "rune_green")
    return b


@asset("decor", 30, 500)
def lever_wall():
    b = Builder("lever_wall")
    b.box(0, -0.05, 1.0, 0.5, 0.1, 0.7, "iron_dark")
    b.cyl(0, -0.12, 1.35, 0.07, 0.05, 6, "iron")
    b.beam((0, -0.15, 1.35), (0.3, -0.5, 1.8), 0.06, "iron")
    b.lathe(0.3, -0.5, 1.8, [(0.08, 0), (0.1, 0.1), (0, 0.15)], 6, "rust")
    return b


@asset("decor", 30, 600)
def banner_pole_tattered():
    b = Builder("banner_pole_tattered")
    b.cyl(0, 0, 0, 0.06, 2.8, 5, "wood_dark")
    b.pyramid(0, 0, 2.8, 0.14, 0.14, 0.24, "iron")
    b.plate([(0.04, 2.6), (0.9, 2.5), (0.85, 1.8), (0.7, 1.6), (0.6, 1.9), (0.4, 1.5), (0.3, 1.8), (0.04, 1.7)], -0.02, 0.02, "cloth_red")
    b.cyl(0, 0, 0, 0.5, 0.08, 6, "stone_dark")
    return b
