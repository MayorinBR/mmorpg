import math

from lib import *
from decor import candle, flame, skull


@asset("props", 30, 600)
def barrel_old():
    b = Builder("barrel_old")
    b.lathe(0, 0, 0, [(0.4, 0), (0.52, 0.45), (0.4, 0.9), (0, 0.9)], 8, "wood")
    for z in (0.18, 0.7):
        b.cyl(0, 0, z, 0.49 if z < 0.5 else 0.47, 0.07, 8, "iron_dark")
    b.cyl(0, 0, 0.9, 0.38, 0.02, 8, "wood_dark")
    return b


@asset("props", 30, 600)
def barrel_broken():
    b = Builder("barrel_broken")
    b.lathe(0, 0, 0, [(0.4, 0), (0.52, 0.45), (0.45, 0.65)], 8, "wood")
    b.cyl(0, 0, 0.18, 0.5, 0.07, 8, "iron_dark")
    for k in range(5):
        a = 2 * math.pi * k / 5
        b.beam((0.45 * math.cos(a), 0.45 * math.sin(a), 0.6), (0.5 * math.cos(a) + 0.12, 0.5 * math.sin(a), 0.95 - 0.1 * k % 0.3), 0.1, "wood_rot", d=0.04)
    b.beam((0.7, -0.6, 0.04), (1.3, -0.2, 0.06), 0.14, "wood", d=0.04)
    return b


@asset("props", 30, 600)
def crate_a():
    b = Builder("crate_a")
    b.box(0, 0, 0, 0.9, 0.9, 0.86, "wood")
    for z in (0.0, 0.8):
        b.box(0, 0, z, 0.96, 0.96, 0.1, "wood_dark")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.42, sy * 0.42, 0.1, 0.1, 0.1, 0.7, "wood_dark")
    return b


@asset("props", 30, 600)
def crate_broken():
    b = Builder("crate_broken")
    b.box(0, 0, 0, 0.9, 0.9, 0.35, "wood")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.42, sy * 0.42, 0.0, 0.1, 0.1, 0.7 if sx * sy > 0 else 0.45, "wood_dark")
    b.beam((-0.1, -0.7, 0.05), (0.9, -0.3, 0.1), 0.16, "wood_rot", d=0.05)
    b.beam((0.3, 0.6, 0.05), (1.0, 0.9, 0.05), 0.14, "wood", d=0.05)
    return b


@asset("props", 30, 700)
def crate_stack():
    b = Builder("crate_stack")
    b.box(-0.5, 0, 0, 0.9, 0.9, 0.86, "wood")
    b.box(0.5, 0.1, 0, 0.9, 0.9, 0.86, "wood_dark")
    b.box(0.0, 0.05, 0.9, 0.9, 0.9, 0.86, "wood", rot=0.2)
    for x, y, r in ((-0.5, 0, 0), (0.5, 0.1, 0), (0, 0.05, 0.2)):
        z = 0.0 if r == 0 else 0.9
        b.box(x, y, z + 0.0, 0.96, 0.96, 0.08, "wood_dark", rot=r)
        b.box(x, y, z + 0.82, 0.96, 0.96, 0.08, "wood_dark", rot=r)
    return b


@asset("props", 30, 700)
def chest_wood():
    b = Builder("chest_wood")
    b.box(0, 0, 0, 1.2, 0.7, 0.5, "wood")
    extrude_x(b, 0, 0, 0.5, 1.24, [(-0.37, 0), (0.37, 0), (0.3, 0.3), (0, 0.4), (-0.3, 0.3)], "wood_dark")
    for sx in (-1, 1):
        b.box(sx * 0.45, 0, 0.0, 0.12, 0.74, 0.47, "iron_dark")
        extrude_x(b, sx * 0.45, 0, 0.5, 0.12, [(-0.39, 0), (0.39, 0), (0.32, 0.31), (0, 0.42), (-0.32, 0.31)], "iron_dark")
    b.box(0, -0.37, 0.4, 0.16, 0.06, 0.2, "gold")
    return b


@asset("props", 30, 700)
def chest_iron():
    b = Builder("chest_iron")
    b.box(0, 0, 0, 1.3, 0.8, 0.55, "iron_dark")
    b.box(0, 0, 0.55, 1.34, 0.84, 0.3, "iron")
    for sx in (-1, 0, 1):
        b.box(sx * 0.5, 0, 0.0, 0.1, 0.86, 0.87, "rust")
    b.box(0, -0.43, 0.45, 0.2, 0.06, 0.24, "gold")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.6, sy * 0.36, 0.0, 0.14, 0.14, 0.14, "iron")
    return b


@asset("props", 30, 800)
def chest_open_gold():
    b = Builder("chest_open_gold")
    b.box(0, 0, 0, 1.2, 0.7, 0.5, "wood")
    b.box(0, 0, 0.5, 1.1, 0.6, 0.02, "gold")
    for i, (x, y, s) in enumerate([(-0.3, 0.0, 0.3), (0.1, 0.1, 0.35), (0.35, -0.1, 0.25), (-0.1, -0.15, 0.2)]):
        b.rock(x, y, 0.5, s, s, 0.22, "gold", 150 + i, sides=5, rings=2)
    b.box(0, 0.5, 0.5, 1.24, 0.08, 0.6, "wood_dark", rot=0.0)
    for sx in (-1, 1):
        b.box(sx * 0.45, 0.5, 0.5, 0.12, 0.1, 0.62, "iron_dark")
        b.box(sx * 0.45, 0, 0.0, 0.12, 0.74, 0.47, "iron_dark")
    return b


@asset("props", 30, 600)
def sack_pile():
    b = Builder("sack_pile")
    for i, (x, y, s) in enumerate([(0, 0, 0.8), (0.7, 0.2, 0.65), (-0.1, 0.7, 0.6)]):
        b.lathe(x, y, 0, [(0.3 * s, 0), (0.5 * s, 0.3 * s), (0.42 * s, 0.7 * s), (0.18 * s, 0.9 * s), (0.22 * s, 1.0 * s), (0, 1.0 * s)], 6, "cloth_tan")
        b.cyl(x, y, 0.82 * s, 0.17 * s, 0.05 * s, 6, "wood_dark")
    return b


@asset("props", 30, 900)
def bookshelf_ruined():
    b = Builder("bookshelf_ruined")
    for sx in (-1, 1):
        b.box(sx * 1.0, 0, 0, 0.14, 0.5, 2.4, "wood_dark")
    for z in (0.0, 0.8, 1.6, 2.3):
        b.box(0, 0, z, 2.1, 0.5, 0.12 if z > 2 else 0.1, "wood")
    b.box(0, 0.22, 0.1, 2.0, 0.06, 2.2, "wood_dark")
    rnd = random.Random(9)
    for z in (0.1, 0.9):
        x = -0.85
        while x < 0.8:
            w = rnd.uniform(0.1, 0.2)
            h = rnd.uniform(0.4, 0.62)
            b.box(x + w / 2, -0.02, z, w, 0.3, h, rnd.choice(["cloth_red", "cloth_purple", "wood", "cloth_tan", "cloth_grey"]))
            x += w + 0.02
    b.box(-0.2, -0.05, 1.7, 0.9, 0.3, 0.1, "cloth_red", rot=0.0)
    b.beam((0.2, -0.3, 1.7), (0.9, -0.3, 1.1), 0.25, "cloth_purple", d=0.06)
    return b


@asset("props", 30, 800)
def table_wood_old():
    b = Builder("table_wood_old")
    b.box(0, 0, 0.75, 2.0, 1.0, 0.1, "wood")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.85, sy * 0.38, 0.0, 0.12, 0.12, 0.75, "wood_dark")
    b.box(0, 0, 0.25, 1.8, 0.07, 0.07, "wood_dark")
    candle(b, 0.5, 0.1, 0.85, 0.25, 0.05)
    b.cyl(-0.4, 0.1, 0.85, 0.12, 0.1, 6, "bronze")
    return b


@asset("props", 30, 700)
def bench_wood_broken():
    b = Builder("bench_wood_broken")
    b.box(-0.35, 0, 0.45, 1.5, 0.45, 0.08, "wood")
    b.box(0.9, 0.1, 0.3, 0.7, 0.4, 0.08, "wood_rot", rot=0.4)
    for sx in (-0.9, 0.2):
        b.box(sx, 0, 0.0, 0.1, 0.4, 0.45, "wood_dark")
    return b


@asset("props", 30, 700)
def chair_broken():
    b = Builder("chair_broken")
    b.box(0, 0, 0.45, 0.6, 0.6, 0.08, "wood")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.25, sy * 0.25, 0.0, 0.07, 0.07, 0.45 if sx * sy > 0 else 0.3, "wood_dark")
    b.box(0, 0.28, 0.53, 0.6, 0.06, 0.9, "wood")
    b.beam((0.3, -0.3, 0.55), (0.55, -0.55, 0.2), 0.07, "wood_rot")
    return b


@asset("props", 30, 700)
def bed_straw():
    b = Builder("bed_straw")
    b.box(0, 0, 0.0, 1.0, 2.0, 0.2, "wood_dark")
    b.box(0, 0, 0.2, 0.9, 1.9, 0.14, "cloth_tan")
    b.box(0, 0.7, 0.34, 0.5, 0.4, 0.12, "cloth_grey")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.45, sy * 0.95, 0.0, 0.1, 0.1, 0.3, "wood")
    return b


@asset("props", 30, 600)
def bucket_wood():
    b = Builder("bucket_wood")
    b.cyl(0, 0, 0, 0.28, 0.45, 7, "wood", r2=0.36)
    b.cyl(0, 0, 0.1, 0.3, 0.04, 7, "iron_dark")
    b.cyl(0, 0, 0.35, 0.35, 0.04, 7, "iron_dark")
    b.cyl(0, 0, 0.42, 0.33, 0.02, 7, "black")
    return b


@asset("props", 30, 700)
def shelf_potions():
    b = Builder("shelf_potions")
    y = -0.12
    b.box(0, y, 1.2, 1.8, 0.24, 0.08, "wood")
    b.box(0, y, 1.9, 1.8, 0.24, 0.08, "wood")
    for sx in (-1, 1):
        b.box(sx * 0.85, y + 0.05, 1.15, 0.08, 0.1, 0.9, "wood_dark")
    cols = ["slime", "crystal_blue", "rune_red", "crystal_purple", "slime", "crystal_blue"]
    for i, c in enumerate(cols):
        x = -0.7 + i * 0.28
        b.lathe(x, y, 1.28, [(0.1, 0), (0.12, 0.12), (0.06, 0.24), (0.05, 0.3), (0.0, 0.3)], 5, c)
    for i in range(4):
        x = -0.6 + i * 0.4
        b.cyl(x, y, 1.98, 0.08, 0.28, 5, "bone_dark" if i % 2 else "rock_warm")
    return b


@asset("props", 30, 700)
def torch_ground():
    b = Builder("torch_ground")
    b.beam((-0.4, 0, 0.08), (0.4, 0.1, 0.08), 0.1, "wood_dark")
    b.lathe(0.4, 0.1, 0.05, [(0.08, 0), (0.12, 0.05), (0.0, 0.05)], 5, "iron")
    flame(b, 0.4, 0.1, 0.1, 0.12, 0.3)
    return b


@asset("props", 30, 700)
def mining_pickaxe_prop():
    b = Builder("mining_pickaxe_prop")
    b.beam((0, 0, 0.05), (0.9, 0.2, 0.9), 0.06, "wood")
    b.beam((0.7, 0.2, 0.9), (1.1, 0.2, 0.7), 0.12, "steel", d=0.05)
    b.beam((0.7, 0.2, 0.9), (0.45, 0.2, 1.1), 0.1, "steel", d=0.05)
    b.rock(-0.5, 0.4, 0, 0.5, 0.4, 0.3, "rock_a", 160, sides=5, rings=2)
    return b


@asset("props", 30, 800)
def cart_ruined():
    b = Builder("cart_ruined")
    b.box(0, 0, 0.5, 2.2, 1.3, 0.1, "wood")
    for sx in (-1, 1):
        b.box(sx * 1.05, 0, 0.6, 0.1, 1.3, 0.5, "wood_dark")
    b.box(0, -0.6, 0.6, 2.2, 0.1, 0.5, "wood_dark")
    b.beam((-1.1, 0, 0.5), (-2.2, 0.3, 0.2), 0.12, "wood_dark")
    for sy in (-1, 1):
        n = 8
        for k in range(n):
            a0, a1 = 2 * math.pi * k / n, 2 * math.pi * (k + 1) / n
            b.beam((0.1 + 0.5 * math.cos(a0), sy * 0.7, 0.5 + 0.5 * math.sin(a0)), (0.1 + 0.5 * math.cos(a1), sy * 0.7, 0.5 + 0.5 * math.sin(a1)), 0.08, "wood")
        b.beam((0.1, sy * 0.7, 0.5), (0.1 + 0.5, sy * 0.7, 0.5), 0.06, "wood_dark")
    return b
