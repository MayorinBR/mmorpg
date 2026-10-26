import math

from lib import *
from buildings import banner, column, obelisk, sphinx, statue_seated, stool, table


def warrior(b, x, y, z0, s=1.0, rot=0.0):
    with b.at(x, y, z0, rot):
        b.box(0, 0, 0, 2.8 * s, 2.8 * s, 0.4 * s, "stone")
        b.box(0, 0, 0.4 * s, 2.4 * s, 2.4 * s, 0.4 * s, "stone_dark")
        b.frustum(0, 0, 0.8 * s, 2.0 * s, 2.0 * s, 1.5 * s, "sand_light", 1.6 * s, 1.6 * s)
        b.box(0, 0, 2.3 * s, 1.9 * s, 1.9 * s, 0.22 * s, "trim")
        b.plate(circle_poly(0, 1.5 * s, 0.35 * s, 6), -1.0 * s - 0.04, -1.0 * s + 0.02, "gold")
        for sx in (-1, 1):
            b.box(sx * 0.26 * s, 0, 2.52 * s, 0.34 * s, 0.4 * s, 0.5 * s, "metal")
            b.box(sx * 0.26 * s, 0, 3.0 * s, 0.38 * s, 0.42 * s, 0.9 * s, "stone")
            b.box(sx * 0.26 * s, -0.12 * s, 2.52 * s, 0.3 * s, 0.34 * s, 0.1 * s, "wood_dark")
        b.frustum(0, 0, 3.9 * s, 1.0 * s, 0.6 * s, 0.3 * s, "gold", 1.05 * s, 0.65 * s)
        b.frustum(0, 0, 4.2 * s, 1.0 * s, 0.6 * s, 1.1 * s, "stone", 1.25 * s, 0.7 * s)
        for sx in (-1, 1):
            b.box(sx * 0.82 * s, 0, 5.0 * s, 0.5 * s, 0.55 * s, 0.4 * s, "gold")
        b.box(0, 0.45 * s, 4.2 * s, 1.3 * s, 0.1 * s, 1.6 * s, "cloth_red")
        b.cyl(0, 0, 5.3 * s, 0.22 * s, 0.25 * s, 6, "stone")
        b.lathe(0, 0, 5.55 * s, [(0.3 * s, 0), (0.34 * s, 0.25 * s), (0.28 * s, 0.5 * s), (0.1 * s, 0.6 * s)], 6, "metal_light")
        b.box(0, 0, 6.1 * s, 0.1 * s, 0.4 * s, 0.35 * s, "cloth_red")
        for sx in (-1, 1):
            b.box(sx * 0.82 * s, 0, 3.4 * s, 0.3 * s, 0.34 * s, 0.9 * s, "stone")
        b.cyl(1.05 * s, -0.1 * s, 2.6 * s, 0.05 * s, 3.6 * s, 4, "wood_dark")
        b.pyramid(1.05 * s, -0.1 * s, 6.2 * s, 0.2 * s, 0.2 * s, 0.5 * s, "metal_light")
        b.plate(circle_poly(-0.95 * s, 4.4 * s, 0.6 * s, 8), -0.45 * s, -0.3 * s, "cloth_red")
        b.plate(circle_poly(-0.95 * s, 4.4 * s, 0.38 * s, 8), -0.5 * s, -0.3 * s, "metal_light")
        b.plate(circle_poly(-0.95 * s, 4.4 * s, 0.14 * s, 6), -0.54 * s, -0.3 * s, "gold")


@asset("decor", 500, 1500)
def statue_warrior():
    b = Builder("statue_warrior")
    warrior(b, 0, 0, 0, 1.0)
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * 1.8, sy * 1.8
            b.cyl(x, y, 0, 0.3, 0.9, 6, "sand_light", r2=0.24)
            b.cyl(x, y, 0.9, 0.38, 0.14, 6, "gold")
            b.lathe(x, y, 1.04, [(0.2, 0), (0.12, 0.18), (0, 0.3)], 6, "gold")
    return b


@asset("decor", 500, 1500)
def statue_colossus_seated():
    b = Builder("statue_colossus_seated")
    statue_seated(b, 0, 0, 0, 2.0)
    stairs(b, 0, -3.6, 0, 6.0, 1.8, 0.4, 2)
    for sx in (-1, 1):
        obelisk(b, sx * 4.2, -1.0, 0, 0.55, glyphs=False)
        b.cyl(sx * 3.0, -3.8, 0, 0.5, 1.0, 8, "stone", r2=0.4)
        b.cyl(sx * 3.0, -3.8, 1.0, 0.7, 0.35, 8, "metal")
        b.lathe(sx * 3.0, -3.8, 1.35, [(0.5, 0), (0.3, 0.5), (0.1, 0.9), (0, 1.1)], 6, "fire")
    return b


@asset("decor", 500, 1500)
def statue_sphinx():
    b = Builder("statue_sphinx")
    sphinx(b, 0, 0, 0, 1.6)
    stairs(b, 0, -5.6, 0, 4.0, 1.6, 0.4, 2)
    for sx in (-1, 1):
        obelisk(b, sx * 3.6, -4.2, 0, 0.5, glyphs=False)
        b.cyl(sx * 3.6, 3.2, 0, 0.35, 1.2, 6, "stone")
        b.cyl(sx * 3.6, 3.2, 1.2, 0.5, 0.25, 6, "metal")
    return b


@asset("decor", 500, 1500)
def obelisk_tall():
    b = Builder("obelisk_tall")
    obelisk(b, 0, 0, 0, 2.0)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cyl(sx * 2.4, sy * 2.4, 0, 0.3, 1.0, 6, "stone")
    return b


@asset("decor", 60, 600)
def column_standing():
    b = Builder("column_standing")
    column(b, 0, 0, 0, 0.55, 6.0, "papyrus")
    return b


@asset("decor", 60, 600)
def column_broken():
    b = Builder("column_broken")
    b.box(0, 0, 0, 2.0, 2.0, 0.35, "stone")
    b.lathe(0, 0, 0.35, [(0.75, 0), (0.55, 0.2), (0.55, 2.6), (0.5, 2.8)], 8, "sand_light")
    b.frustum(0, 0, 3.1, 1.0, 1.0, 0.5, "sand_light", 0.5, 0.3)
    b.cyl(1.9, -0.8, 0, 0.5, 2.4, 8, "sand_light")
    b.cyl(1.9, -0.8, 0, 0.5, 0.1, 8, "stone")
    b.box(-1.3, 1.1, 0, 0.9, 0.7, 0.5, "stone")
    b.box(0.9, 1.4, 0, 0.6, 0.5, 0.35, "sand")
    return b


@asset("decor", 60, 600)
def brazier():
    b = Builder("brazier")
    for k in range(3):
        a = 2 * math.pi * k / 3
        b.cyl(0.5 * math.cos(a), 0.5 * math.sin(a), 0, 0.07, 1.3, 4, "metal")
    b.cyl(0, 0, 1.2, 0.7, 0.4, 8, "metal", r2=0.9)
    b.cyl(0, 0, 1.55, 0.8, 0.05, 8, "black")
    b.lathe(0, 0, 1.6, [(0.5, 0), (0.3, 0.5), (0.1, 0.9), (0, 1.1)], 6, "fire")
    return b


@asset("decor", 60, 600)
def lamp_post():
    b = Builder("lamp_post")
    b.cyl(0, 0, 0, 0.4, 0.5, 6, "stone", r2=0.3)
    b.lathe(0, 0, 0.5, [(0.18, 0), (0.12, 0.3), (0.1, 3.0), (0.2, 3.1), (0.2, 3.3)], 6, "metal")
    b.box(0.45, 0, 3.0, 0.9, 0.08, 0.08, "metal")
    b.cyl(0.9, 0, 2.2, 0.22, 0.7, 6, "gold", r2=0.3)
    b.lathe(0.9, 0, 2.9, [(0.38, 0), (0.15, 0.3), (0, 0.45)], 6, "slate")
    b.cyl(-0, 0, 3.3, 0.22, 0.6, 6, "gold", r2=0.3)
    b.lathe(0, 0, 3.9, [(0.38, 0), (0.15, 0.3), (0, 0.45)], 6, "slate")
    return b


@asset("decor", 60, 600)
def lamp_post_street():
    b = Builder("lamp_post_street")
    b.box(0, 0, 0, 0.8, 0.8, 0.5, "stone")
    b.cyl(0, 0, 0.5, 0.15, 2.6, 6, "wood_dark")
    b.box(0, 0, 3.0, 0.9, 0.12, 0.12, "wood_dark")
    for s in (-1, 1):
        b.cyl(s * 0.4, 0, 2.2, 0.05, 0.8, 4, "metal")
        b.cyl(s * 0.4, 0, 1.6, 0.2, 0.6, 6, "gold", r2=0.25)
        b.lathe(s * 0.4, 0, 2.2, [(0.3, 0), (0.1, 0.25), (0, 0.4)], 6, "slate")
    return b


@asset("decor", 500, 1500)
def fountain_round():
    b = Builder("fountain_round")
    b.cyl(0, 0, 0, 3.6, 0.9, 8, "stone")
    b.cyl(0, 0, 0.9, 3.8, 0.25, 8, "trim")
    b.cyl(0, 0, 0.7, 3.1, 0.1, 8, "water")
    b.cyl(0, 0, 0, 0.6, 1.6, 8, "sand_light")
    b.cyl(0, 0, 1.6, 1.4, 0.3, 8, "stone")
    b.cyl(0, 0, 1.9, 1.15, 0.12, 8, "water")
    b.cyl(0, 0, 1.9, 0.25, 1.4, 8, "sand_light")
    b.lathe(0, 0, 3.3, [(0.5, 0), (0.3, 0.3), (0, 0.6)], 8, "water")
    for k in range(8):
        a = 2 * math.pi * (k + 0.5) / 8
        b.box(3.7 * math.cos(a), 3.7 * math.sin(a), 0.9, 0.5, 0.5, 0.4, "trim", a)
        b.box(4.2 * math.cos(a), 4.2 * math.sin(a), 0, 0.9, 0.9, 0.35, "stone", a)
        b.cyl(3.1 * math.cos(a), 3.1 * math.sin(a), 0.8, 0.12, 0.4, 5, "gold")
        b.cyl(1.0 * math.cos(a), 1.0 * math.sin(a), 1.9, 0.18, 0.8, 5, "trim")
        b.pyramid(1.0 * math.cos(a), 1.0 * math.sin(a), 2.7, 0.3, 0.3, 0.3, "gold")
    for k in range(4):
        a = math.pi * k / 2
        b.lathe(2.3 * math.cos(a), 2.3 * math.sin(a), 0.9, [(0.15, 0), (0.1, 0.5), (0, 0.8)], 5, "water")
    return b


@asset("decor", 100, 1000)
def well_stone():
    b = Builder("well_stone")
    b.cyl(0, 0, 0, 1.3, 1.0, 8, "stone")
    b.cyl(0, 0, 1.0, 1.4, 0.2, 8, "trim")
    b.cyl(0, 0, 0.8, 1.0, 0.05, 8, "black")
    for s in (-1, 1):
        b.box(s * 1.1, 0, 1.0, 0.2, 0.2, 2.2, "wood_dark")
    b.box(0, 0, 3.0, 2.8, 0.2, 0.2, "wood_dark")
    b.cyl(0, 0, 2.5, 0.12, 0.6, 4, "wood")
    b.gable(0, 0, 3.2, 3.2, 1.8, 0.8, "slate")
    return b


def palm(name, lean, height):
    b = Builder(name)
    x = y = 0.0
    z = 0.0
    seg = 5
    r = 0.38
    for i in range(seg):
        hh = height / seg
        nx, ny = x + lean * 0.35, y + lean * 0.1
        b.lathe(x, y, z, [(r, 0), (r * 0.88, hh)], 6, "palm_trunk")
        r *= 0.88
        x, y, z = nx, ny, z + hh
    b.lathe(x, y, z, [(r, 0), (0.55, 0.4), (0.3, 0.7)], 6, "palm_trunk")
    for k in range(9):
        a = 2 * math.pi * k / 9
        for j, (ln, dz) in enumerate(((3.0, 0.7), (2.6, 0.1))):
            ox = math.cos(a) * (0.3 + j * 2.8)
            oy = math.sin(a) * (0.3 + j * 2.8)
            b.slab([(0, 0), (ln * 0.4, 0.7), (ln, 0), (ln * 0.4, -0.7)], z + 0.55 + dz - j * 0.55, z + 0.62 + dz - j * 0.55, "palm_leaf", x + ox, y + oy, a)
    for k in range(3):
        a = 2 * math.pi * k / 3
        b.lathe(x + 0.3 * math.cos(a), y + 0.3 * math.sin(a), z + 0.2, [(0.12, 0), (0.15, 0.2), (0, 0.3)], 5, "wood_dark")
    return b


@asset("decor", 150, 1000)
def palm_tree_a():
    return palm("palm_tree_a", 0.6, 6.5)


@asset("decor", 150, 1000)
def palm_tree_b():
    return palm("palm_tree_b", -0.2, 8.0)


@asset("decor", 60, 600)
def urn_large():
    b = Builder("urn_large")
    b.cyl(0, 0, 0, 0.5, 0.2, 8, "stone")
    b.lathe(0, 0, 0.2, [(0.35, 0), (0.7, 0.4), (0.85, 0.9), (0.6, 1.5), (0.4, 1.8), (0.55, 1.95), (0.5, 2.1), (0, 2.1)], 8, "terracotta")
    b.cyl(0, 0, 1.3, 0.8, 0.1, 8, "gold", r2=0.78)
    return b


@asset("decor", 30, 600)
def bench_stone():
    b = Builder("bench_stone")
    b.box(0, 0, 0.45, 2.6, 0.8, 0.2, "sand_light")
    for s in (-1, 1):
        b.frustum(s * 1.0, 0, 0, 0.4, 0.7, 0.45, "stone", 0.3, 0.6)
    return b


@asset("decor", 30, 600)
def signpost():
    b = Builder("signpost")
    b.cyl(0, 0, 0, 0.1, 2.8, 4, "wood_dark")
    b.box(0.5, -0.1, 2.0, 1.4, 0.08, 0.5, "wood")
    b.box(-0.5, -0.1, 1.3, 1.4, 0.08, 0.5, "wood")
    b.pyramid(0, 0, 2.8, 0.25, 0.25, 0.25, "wood_dark")
    return b


@asset("decor", 60, 600)
def banner_pole():
    b = Builder("banner_pole")
    banner(b, 0, 0, 0, 5.0, "cloth_red")
    return b


@asset("market", 200, 1200)
def stall_cloth_a():
    b = Builder("stall_cloth_a")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cyl(sx * 1.8, sy * 1.1, 0, 0.08, 2.6 if sy < 0 else 2.2, 4, "wood_dark")
    b.box(0, -0.9, 0, 3.4, 0.9, 1.0, "wood")
    b.box(0, -0.9, 1.0, 3.6, 1.0, 0.1, "wood_dark")
    n = 6
    for i in range(n):
        b.extrude_x(-1.95 + 0.65 * (i + 0.5), 0, 2.3, 0.65, [(-1.3, 0.3), (1.3, -0.1), (1.3, 0.05), (-1.3, 0.45)], "cloth_cream" if i % 2 == 0 else "cloth_green")
    for i in range(5):
        b.lathe(-1.3 + i * 0.65, -0.9, 1.1, [(0.2, 0), (0.26, 0.15), (0.1, 0.3), (0, 0.32)], 6, ["cloth_red", "cloth_ochre", "cloth_blue", "cloth_green", "cloth_red"][i])
    crate(b, 2.2, -1.3, 0, 0.7)
    barrel(b, -2.3, -1.3, 0, 0.35, 0.8)
    return b


@asset("market", 200, 1200)
def stall_cloth_b():
    b = Builder("stall_cloth_b")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cyl(sx * 1.5, sy * 1.0, 0, 0.08, 2.5, 4, "wood_dark")
    b.box(0, 0.7, 0, 3.0, 0.5, 2.0, "wood")
    for i in range(4):
        b.box(-1.0 + i * 0.7, 0.55, 0.5 + (i % 2) * 0.7, 0.55, 0.2, 0.45, ["cloth_red", "cloth_blue", "cloth_ochre", "cloth_green"][i])
    b.box(0, -0.7, 0, 2.6, 0.8, 0.9, "wood_dark")
    b.box(0, -0.7, 0.9, 2.8, 0.9, 0.08, "wood")
    for i in range(6):
        b.extrude_x(-1.75 + 0.7 * (i + 0.5), 0, 2.2, 0.7, [(-1.2, 0.3), (1.2, 0.0), (1.2, 0.12), (-1.2, 0.42)], "cloth_red" if i % 2 == 0 else "cloth_cream")
    pot(b, 1.8, -1.2, 0)
    pot(b, -1.8, -1.2, 0, 0.3)
    return b


@asset("market", 100, 800)
def stall_fruit():
    b = Builder("stall_fruit")
    b.box(0, 0, 0, 2.6, 1.2, 0.7, "wood")
    b.box(0, 0, 0.7, 2.8, 1.3, 0.08, "wood_dark")
    cols = ["cloth_red", "cloth_ochre", "palm_leaf", "cloth_red", "cloth_ochre", "palm_leaf"]
    for i in range(6):
        for j in range(2):
            b.lathe(-1.0 + i * 0.4, -0.3 + j * 0.55, 0.78, [(0.12, 0), (0.18, 0.12), (0.12, 0.25), (0, 0.28)], 6, cols[(i + j) % 6])
    for sx in (-1, 1):
        b.cyl(sx * 1.4, -0.7, 0, 0.07, 2.4, 4, "wood_dark")
    b.extrude_x(0, -0.1, 2.3, 3.0, [(-0.9, 0.2), (0.9, -0.1), (0.9, 0.03), (-0.9, 0.33)], "cloth_ochre")
    crate(b, 1.8, 0.2, 0, 0.6)
    return b


@asset("market", 100, 800)
def tent_octagon():
    b = Builder("tent_octagon")
    b.cyl(0, 0, 0, 2.2, 1.2, 8, "cloth_cream")
    b.cyl(0, 0, 1.0, 2.3, 0.3, 8, "cloth_green")
    b.lathe(0, 0, 1.3, [(2.5, 0), (2.0, 0.25), (1.0, 1.1), (0, 1.8)], 8, "cloth_cream")
    b.cyl(0, 0, 3.0, 0.06, 0.6, 4, "gold")
    return b


@asset("market", 100, 800)
def tent_round_awning():
    b = Builder("tent_round_awning")
    for k in range(6):
        a = 2 * math.pi * k / 6
        b.cyl(2.3 * math.cos(a), 2.3 * math.sin(a), 0, 0.07, 2.0, 4, "wood_dark")
    b.lathe(0, 0, 2.0, [(2.8, 0), (2.4, 0.2), (1.2, 0.75), (0, 1.2)], 6, "cloth_red")
    b.cyl(0, 0, 3.1, 0.05, 0.6, 4, "gold")
    table(b, 0, 0, 0, 0.9)
    stool(b, -1.1, 0)
    stool(b, 1.1, 0)
    return b


@asset("market", 100, 800)
def cart_wooden():
    b = Builder("cart_wooden")
    b.box(0, 0, 0.7, 3.0, 1.6, 0.15, "wood")
    for sx in (-1, 1):
        b.box(sx * 1.45, 0, 0.85, 0.12, 1.6, 0.5, "wood_dark")
    b.box(0, -0.75, 0.85, 3.0, 0.12, 0.5, "wood_dark")
    b.box(0, 0.75, 0.85, 3.0, 0.12, 0.5, "wood_dark")
    for s in (-1, 1):
        b.plate(circle_poly(0, 0.7, 0.7, 10), s * 0.95 - 0.07, s * 0.95 + 0.07, "wood_dark", x=0.1)
        b.plate(circle_poly(0, 0.7, 0.15, 6), s * 0.95 - 0.12, s * 0.95 + 0.12, "metal", x=0.1)
    b.box(-2.0, 0, 0.8, 1.6, 0.1, 0.1, "wood_dark")
    b.box(-2.0, 0.6, 0.8, 1.6, 0.1, 0.1, "wood_dark")
    sack(b, 0.6, 0, 0.85)
    sack(b, -0.4, 0.2, 0.85)
    crate(b, 0.9, 0.4, 0.85, 0.6)
    return b


def sack(b, x, y, z0):
    b.lathe(x, y, z0, [(0.3, 0), (0.42, 0.25), (0.3, 0.6), (0.12, 0.75), (0.2, 0.85), (0, 0.9)], 6, "cloth_cream")


@asset("market", 60, 600)
def crates_barrels_group():
    b = Builder("crates_barrels_group")
    crate(b, 0, 0, 0, 0.9)
    crate(b, 1.0, 0.1, 0, 0.8, 0.3)
    crate(b, 0.5, 0.0, 0.9, 0.7, 0.6)
    barrel(b, -1.0, 0.2, 0)
    barrel(b, -1.7, -0.2, 0, 0.35, 0.8)
    sack(b, 0.2, -1.0, 0)
    sack(b, 0.9, -1.1, 0)
    pot(b, -0.8, -1.0, 0, 0.3)
    return b


@asset("walls", 500, 1500)
def wall_straight():
    b = Builder("wall_straight")
    L, t, h = 8.0, 2.0, 6.0
    b.box(0, 0, 0, L, t + 0.5, 0.6, "stone")
    b.box(0, 0, 0.6, L, t, h - 0.6, "sand")
    b.box(0, 0, h - 0.1, L, t + 0.4, 0.4, "trim")
    for s in (-1, 1):
        for i in range(5):
            b.box(-L / 2 + 0.5 + i * (L - 1.0) / 4, s * (t / 2 - 0.2), h + 0.3, 0.7, 0.4, 0.6, "sand_light")
        for i in range(9):
            b.box(-L / 2 + 0.4 + i * (L - 0.8) / 8, s * (t / 2 + 0.25), h - 0.5, 0.35, 0.35, 0.4, "trim")
    for s in (-1, 1):
        b.box(s * (L / 2 - 0.55), 0, 0.6, 1.0, t + 0.5, h - 0.6, "sand_dark")
        for sy in (-1, 1):
            for k in range(5):
                b.box(s * (L / 2 - 0.55), sy * (t / 2 + 0.28), 0.9 + k * 1.0, 1.2 if k % 2 == 0 else 0.9, 0.12, 0.9, "stone")
    for sy in (-1, 1):
        for sx in (-2.0, 2.0):
            y0 = sy * (t / 2)
            with b.at(0, 0, 0, 0 if sy < 0 else math.pi):
                b.plate(arch_poly(sx * (1 if sy < 0 else -1), 2.5, 0.35, 1.0, 3), -t / 2 - 0.05, -t / 2 + 0.05, "window")
                b.plate(arch_poly(sx * (1 if sy < 0 else -1), 4.2, 0.35, 1.0, 3), -t / 2 - 0.05, -t / 2 + 0.05, "window")
    b.box(0, 0, 3.0, L - 1.4, t + 0.16, 0.2, "trim")
    return b


@asset("walls", 200, 2500)
def wall_corner():
    b = Builder("wall_corner")
    b.cyl(0, 0, 0, 2.9, 0.6, 8, "stone")
    b.cyl(0, 0, 0.6, 2.4, 6.2, 8, "sand", r2=2.3)
    b.cyl(0, 0, 6.6, 2.8, 0.4, 8, "trim")
    for k in range(8):
        a = 2 * math.pi * k / 8
        b.box(2.35 * math.cos(a), 2.35 * math.sin(a), 7.0, 0.7, 0.7, 0.6, "sand_light", a)
    for k in (1, 3, 5, 7):
        a = 2 * math.pi * (k + 0.5) / 8
        with b.at(2.3 * math.cos(a), 2.3 * math.sin(a), 3.2, a - math.pi / 2):
            b.plate(arch_poly(0, 0, 0.45, 1.1, 3), -0.05, 0.05, "window")
    slate_dome(b, 0, 0, 6.9, 2.2, 2.4)
    return b


@asset("walls", 100, 2500)
def wall_end_buttress():
    b = Builder("wall_end_buttress")
    b.box(0, 0, 0, 3.0, 2.8, 0.6, "stone")
    b.frustum(0, 0, 0.6, 2.4, 2.2, 5.4, "sand", 2.0, 2.0)
    b.box(0, 0, 5.9, 2.8, 2.6, 0.4, "trim")
    for i in range(3):
        b.box(-0.85 + i * 0.85, -0.95, 6.3, 0.6, 0.5, 0.6, "sand_light")
        b.box(-0.85 + i * 0.85, 0.95, 6.3, 0.6, 0.5, 0.6, "sand_light")
    return b


@asset("walls", 2000, 5000)
def wall_gate_main():
    b = Builder("wall_gate_main")
    W, H, T = 14.0, 9.0, 4.2
    b.box(0, 0, 0, W + 1.0, T + 0.8, 0.6, "stone")
    b.plate(wall_poly(W, H, [(0, 2.7, 4.6)], 8), -T / 2, T / 2, "sand", z=0.6)
    b.box(0, 0, H + 0.5, W + 0.6, T + 0.6, 0.4, "trim")
    crenels(b, W, T, H + 0.9, 1.4, 0.55, 0.6, "sand_light")
    for s in (-1, 1):
        tower_round(b, s * (W / 2 + 0.4), 0, 0.0, 3.0, 12.0, 8, 3.6, "sand_dark", 2)
        b.cyl(s * (W / 2 + 0.4), 0, 12.5, 3.6, 0.35, 8, "trim")
    for s in (-1, 1):
        b.plate(arch_poly(s * 1.3, 0.6, 2.4, 4.5, 4), -T / 2 + 0.9, -T / 2 + 1.0, "wood_dark")
    for k in range(7):
        b.box(-2.1 + k * 0.7, -T / 2 + 0.3, 0.6, 0.12, 0.12, 5.0, "metal")
    b.box(0, -T / 2 + 0.3, 3.0, 5.0, 0.1, 0.12, "metal")
    for i in range(14):
        b.box(-W / 2 + 0.5 + i * (W - 1.0) / 13, -T / 2 - 0.3, H - 0.3, 0.4, 0.5, 0.5, "trim")
    for k in (-1, 1):
        banner(b, k * 4.2, -T / 2 - 0.7, 0.6, 7.0, "cloth_red")
        warrior(b, k * 5.0, -T / 2 + 1.0, H + 0.9, 0.35)
    icon(b, "swords", 0, 7.3, -T / 2, 1.4)
    stairs(b, 0, -T / 2 - 1.4, 0, 6.0, 2.2, 0.6, 3)
    return b


@asset("walls", 200, 3000)
def wall_gate_small():
    b = Builder("wall_gate_small")
    W, H, T = 6.0, 5.0, 2.2
    b.box(0, 0, 0, W + 0.6, T + 0.5, 0.45, "stone")
    b.plate(wall_poly(W, H, [(0, 1.3, 2.4)], 6), -T / 2, T / 2, "sand", z=0.45)
    b.box(0, 0, H + 0.4, W + 0.4, T + 0.4, 0.35, "trim")
    for i in range(4):
        b.box(-W / 2 + 0.5 + i * (W - 1.0) / 3, -T / 2 + 0.2, H + 0.75, 0.7, 0.4, 0.6, "sand_light")
        b.box(-W / 2 + 0.5 + i * (W - 1.0) / 3, T / 2 - 0.2, H + 0.75, 0.7, 0.4, 0.6, "sand_light")
    for s in (-1, 1):
        b.box(s * (W / 2 - 0.1), 0, 0.45, 0.9, T + 0.3, H + 0.1, "sand_dark")
    b.plate(arch_poly(0, 0.45, 2.4, 3.6, 5), -T / 2 + 0.7, -T / 2 + 0.8, "wood_dark")
    return b


@asset("towers", 500, 3500)
def tower_watch_round():
    b = Builder("tower_watch_round")
    tower_round(b, 0, 0, 0, 2.6, 11.0, 8, None, "sand", 3)
    b.cyl(0, 0, 11.5, 3.6, 0.5, 8, "trim")
    b.cyl(0, 0, 12.0, 3.4, 0.25, 8, "wood_dark")
    for k in range(8):
        a = 2 * math.pi * k / 8
        b.cyl(3.2 * math.cos(a), 3.2 * math.sin(a), 12.25, 0.07, 1.0, 4, "wood_dark")
    for k in range(8):
        a = 2 * math.pi * k / 8
        b.box(3.3 * math.cos(a), 3.3 * math.sin(a), 13.2, 1.3, 0.12, 0.1, "wood_dark", a + math.pi / 2)
    for k in range(4):
        a = 2 * math.pi * (k + 0.5) / 4
        b.cyl(1.9 * math.cos(a), 1.9 * math.sin(a), 12.25, 0.2, 2.4, 6, "sand_light")
    slate_dome(b, 0, 0, 14.6, 2.9, 3.2)
    with b.at(0, -2.55, 0):
        b.plate(arch_poly(0, 0.5, 1.2, 2.3, 4), -0.1, 0.1, "door")
    return b


@asset("towers", 500, 3500)
def tower_watch_square():
    b = Builder("tower_watch_square")
    b.box(0, 0, 0, 6.4, 6.4, 0.6, "stone")
    b.frustum(0, 0, 0.6, 5.6, 5.6, 12.0, "sand", 4.6, 4.6)
    b.box(0, 0, 12.4, 6.2, 6.2, 0.5, "trim")
    for k in range(8):
        a = k * math.pi / 2
        for i in (-1, 0, 1):
            b.box(2.6 * math.cos(a) - 1.1 * math.sin(a) * i * 1.0, 2.6 * math.sin(a) + 1.1 * math.cos(a) * i, 12.9, 0.7, 0.7, 0.7, "sand_light", a) if k < 4 else None
    for z in (3.0, 6.5, 9.5):
        for side in ("front", "right", "back", "left"):
            rot, ys = wall_frame(side, 4.6 + (6 - z) * 0.0, 4.6)
            with b.at(rot=rot):
                b.plate(arch_poly(0, z, 0.5, 1.3, 3), ys - 0.04 - (z - 3) * 0.015, ys + 0.12, "window")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cyl(sx * 2.3, sy * 2.3, 12.9, 0.1, 2.3, 4, "wood_dark")
    b.frustum(0, 0, 15.2, 5.6, 5.6, 1.8, "cloth_red", 0.3, 0.3)
    banner(b, 0, -3.3, 0.6, 7.0, "cloth_blue")
    return b


@asset("bridge", 500, 3500)
def bridge_stone():
    b = Builder("bridge_stone")
    L, Wd, H = 16.0, 6.0, 4.4
    b.plate(wall_poly(L, H, [(-4.8, 1.8, 1.5), (0, 2.2, 1.9), (4.8, 1.8, 1.5)], 8), -Wd / 2, Wd / 2, "sand", x=0)
    b.box(0, 0, H, L + 0.6, Wd + 0.6, 0.35, "trim")
    for s in (-1, 1):
        b.box(0, s * (Wd / 2 - 0.15), H + 0.35, L + 0.4, 0.5, 0.9, "sand_light")
        for i in range(9):
            b.box(-L / 2 + 0.4 + i * (L - 0.8) / 8, s * (Wd / 2 - 0.15), H + 1.25, 0.6, 0.6, 0.4, "trim")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * (L / 2 + 0.2), sy * (Wd / 2 - 0.15), H + 0.35, 0.9, 0.9, 1.6, "sand_light")
            b.cyl(sx * (L / 2 + 0.2), sy * (Wd / 2 - 0.15), H + 1.95, 0.4, 0.3, 6, "gold")
    b.box(-L / 2 - 0.5, 0, 0, 1.2, Wd + 0.4, 0.5, "stone")
    b.box(L / 2 + 0.5, 0, 0, 1.2, Wd + 0.4, 0.5, "stone")
    return b


@asset("ruins", 100, 2000)
def ruin_arch():
    b = Builder("ruin_arch")
    b.box(0, 0, 0, 9.0, 2.6, 0.5, "stone")
    pts = [(-4.0, 0), (-1.6, 0), (-1.6, 4.0)]
    arc = [(-1.6 + 1.6 * (1 - math.cos(math.pi * i / 8)), 4.0 + 1.6 * math.sin(math.pi * i / 8)) for i in range(0, 5)]
    poly = [(-4.0, 0), (-1.6, 0), (-1.6, 4.0)] + arc[1:] + [(0.2, 6.0), (-4.0, 6.5)]
    b.plate(poly, -1.0, 1.0, "sand", z=0.5)
    b.plate([(2.4, 0), (4.0, 0), (4.0, 3.2), (3.4, 3.8), (2.4, 3.0)], -1.0, 1.0, "sand", z=0.5)
    b.box(-2.6, 0, 6.8, 2.6, 2.0, 0.4, "trim")
    b.box(1.0, 0, 0.5, 1.2, 1.0, 0.8, "sand_dark")
    b.box(-0.3, 2.1, 0.5, 1.0, 0.8, 0.6, "sand_dark", 0.5)
    b.box(2.0, -2.1, 0.5, 1.1, 0.8, 0.5, "sand_dark", -0.4)
    return b
