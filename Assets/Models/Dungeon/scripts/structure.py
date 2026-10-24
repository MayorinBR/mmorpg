import math

from lib import *

H = 3.6
T = 0.8
M = 4.0
OPEN_A = 1.1
OPEN_ZS = 1.7


def wall_shell(b, w=M, h=H, body="brick_a", plinth="brick_dark", cap="trim", bands=True, t=T, top=0.3):
    b.box(0, 0, 0, w, t + 0.16, 0.55, plinth)
    b.box(0, 0, 0.55, w, t, h - 0.55 - top, body)
    if bands:
        b.box(0, 0, 1.75, w, t + 0.06, 0.1, "brick_light")
    b.box(0, 0, h - top, w, t + 0.2, top, cap)


def sconce(b, x, y, z, flame=True):
    """Iron wall torch holder facing -Y; y is the wall surface."""
    b.box(x, y - 0.12, z, 0.14, 0.24, 0.14, "iron_dark")
    b.cyl(x, y - 0.3, z + 0.02, 0.07, 0.5, 6, "wood_dark", r2=0.09)
    b.lathe(x, y - 0.3, z + 0.5, [(0.14, 0), (0.1, 0.12), (0, 0.12)], 6, "iron")
    if flame:
        b.lathe(x, y - 0.3, z + 0.52, [(0.09, 0), (0.12, 0.14), (0.05, 0.34), (0, 0.5)], 5, "flame")


def banner_hang(b, x, y, z_top, w=0.9, h=1.9, color="cloth_red", trim="gold"):
    b.beam((x - w / 2 - 0.15, y - 0.12, z_top), (x + w / 2 + 0.15, y - 0.12, z_top), 0.07, "iron_dark")
    b.plate([(x - w / 2, z_top), (x + w / 2, z_top), (x + w / 2, z_top - h * 0.8), (x + w * 0.25, z_top - h), (x, z_top - h * 0.85), (x - w * 0.2, z_top - h * 0.97), (x - w / 2, z_top - h * 0.75)], -0.18 + y, -0.12 + y, color)
    b.plate(circle_poly(x, z_top - h * 0.38, w * 0.22, 6), y - 0.2, y - 0.16, trim)


@asset("walls", 30, 900)
def wall_straight():
    b = Builder("wall_straight")
    wall_shell(b)
    return b


@asset("walls", 30, 900)
def wall_straight_buttress():
    b = Builder("wall_straight_buttress")
    wall_shell(b)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.frustum(sx * 1.35, sy * (T / 2 + 0.12), 0.55, 0.5, 0.26, 2.7, "brick_b", 0.4, 0.2)
    b.box(0, -T / 2 - 0.03, 0.9, 1.6, 0.06, 1.8, "brick_dark")
    b.box(0, T / 2 + 0.03, 0.9, 1.6, 0.06, 1.8, "brick_dark")
    return b


@asset("walls", 30, 900)
def wall_torch():
    b = Builder("wall_torch")
    wall_shell(b)
    sconce(b, 0, -T / 2 - 0.03, 1.55)
    return b


@asset("walls", 30, 900)
def wall_banner():
    b = Builder("wall_banner")
    wall_shell(b)
    banner_hang(b, 0, -T / 2 - 0.03, 3.0)
    sconce(b, -1.45, -T / 2 - 0.03, 1.5)
    sconce(b, 1.45, -T / 2 - 0.03, 1.5)
    return b


@asset("walls", 30, 900)
def wall_window_bars():
    b = Builder("wall_window_bars")
    b.box(0, 0, 0, M, T + 0.16, 0.55, "brick_dark")
    b.box(0, 0, 0.55, M, T, 0.85, "brick_a")
    b.box(0, 0, 2.5, M, T, 0.8, "brick_a")
    for sx in (-1, 1):
        b.box(sx * 1.5, 0, 1.4, 1.0, T, 1.1, "brick_a")
    b.box(0, 0, 1.4, 2.0, T + 0.12, 0.12, "trim")
    b.box(0, 0, 2.38, 2.0, T + 0.12, 0.12, "trim")
    for i in range(5):
        b.cyl(-0.8 + i * 0.4, 0, 1.45, 0.045, 0.95, 5, "iron_dark")
    b.box(0, 0, 1.9, 2.0, 0.08, 0.07, "iron_dark")
    b.box(0, 0, 1.55, 2.0, T + 0.16, 0.12, "iron_dark")
    b.box(0, 0, 1.75, M, T + 0.06, 0.1, "brick_light")
    b.box(0, 0, H - 0.3, M, T + 0.2, 0.3, "trim")
    return b


def archway_wall(name, leaf=None, bars=False, boss=False):
    b = Builder(name)
    a = 1.45 if boss else OPEN_A
    zs = 1.55 if boss else OPEN_ZS
    top = H - 0.3
    b.plate(pointed_wall_poly(M, top, [(0, a, zs)]), -T / 2, T / 2, "brick_a")
    for sx in (-1, 1):
        b.box(sx * (a + (M / 2 - a) / 2), 0, 0, M / 2 - a, T + 0.16, 0.55, "brick_dark")
    b.box(0, 0, top, M, T + 0.2, 0.3, "trim")
    b.plate(arch_ring(0, a, a + 0.28, zs), -T / 2 - 0.07, T / 2 + 0.07, "trim")
    b.box(0, -T / 2 - 0.07, zs + arch_rise(a) + 0.02, 0.36, 0.16, 0.34, "trim_dark")
    b.box(0, T / 2 + 0.07 - 0.16, zs + arch_rise(a) + 0.02, 0.36, 0.16, 0.34, "trim_dark")
    for sx in (-1, 1):
        b.box(sx * (a + 0.14), 0, 0, 0.28, T + 0.14, 0.7, "trim_dark")
    if leaf == "wood":
        b.plate(arch_slab(0, a, zs), -0.12, 0.12, "wood")
        for z in (0.5, 1.6, 2.5):
            b.box(0, -0.13, z, a * 2 - 0.1, 0.05, 0.14, "iron_dark")
        b.box(0, 0, 0, 0.06, 0.28, zs + 0.2, "wood_dark")
        b.cyl(0.25, -0.2, 1.3, 0.09, 0.05, 6, "iron")
        b.cyl(-0.25, -0.2, 1.3, 0.09, 0.05, 6, "iron")
    if bars:
        n = int(a * 2 / 0.3)
        for i in range(n + 1):
            x = -a + 0.1 + i * (a * 2 - 0.2) / n
            ztop = zs + max(0.0, math.sqrt(max(0.0, (1.5 * a) ** 2 - (abs(x) + a - 1.5 * a) ** 2)) - 0.05) if abs(x) < a else zs
            b.box(x, 0, 0.0, 0.07, 0.1, ztop - 0.25, "iron_dark")
        for z in (0.35, 1.3, 2.3):
            b.box(0, 0, z, a * 2 - 0.1, 0.09, 0.09, "iron")
        b.box(0, -0.02, 0.25, a * 2 - 0.1, 0.14, 0.14, "iron_dark")
    if boss:
        b.plate(arch_slab(0, a - 0.12, zs - 0.05), -0.14, 0.14, "iron_dark")
        b.box(-0.01, -0.15, 0, 0.03, 0.05, zs + 1.3, "black")
        for z in (0.45, 1.5, 2.4):
            b.box(0, -0.17, z, a * 2 - 0.3, 0.06, 0.16, "iron")
        for sx in (-1, 1):
            b.cyl(sx * 0.55, -0.2, 1.45, 0.22, 0.06, 8, "bronze")
        b.plate(circle_poly(0, 2.75, 0.28, 6), -0.2, -0.15, "bone")
        b.plate([(-0.2, 2.76), (-0.08, 2.76), (-0.08, 2.64), (-0.2, 2.64)], -0.215, -0.19, "rune_red")
        b.plate([(0.2, 2.76), (0.08, 2.76), (0.08, 2.64), (0.2, 2.64)], -0.215, -0.19, "rune_red")
    return b


@asset("walls", 30, 1500)
def wall_arch_open():
    return archway_wall("wall_arch_open")


@asset("walls", 30, 1500)
def wall_arch_door():
    return archway_wall("wall_arch_door", leaf="wood")


@asset("walls", 30, 1500)
def wall_arch_gate():
    return archway_wall("wall_arch_gate", bars=True)


@asset("walls", 30, 1500)
def wall_arch_boss():
    return archway_wall("wall_arch_boss", boss=True)


@asset("walls", 30, 900)
def wall_cracked():
    b = Builder("wall_cracked")
    wall_shell(b)
    for side in (-1, 1):
        y = side * (T / 2 + 0.015)
        for pts in ([(-0.7, 3.1), (-0.5, 2.4), (-0.75, 1.9), (-0.45, 1.2)], [(0.9, 2.9), (0.7, 2.2), (1.0, 1.6)]):
            for p, q in zip(pts, pts[1:]):
                b.beam((p[0], y, p[1]), (q[0], y, q[1]), 0.07, "brick_dark", d=0.03)
        b.box(-0.45, y, 0.7, 0.9, 0.03, 0.25, "moss_dark")
    return b


@asset("walls", 30, 900)
def wall_moss():
    b = Builder("wall_moss")
    wall_shell(b)
    for side in (-1, 1):
        y = side * (T / 2 + 0.02)
        with b.at(0, y, 0, 0):
            b.plate([(-1.8, 0.4), (-0.5, 0.4), (-0.2, 1.1), (-0.7, 1.6), (-1.3, 1.9), (-1.9, 1.3)], -0.03 * side, 0.0, "moss")
            b.plate([(0.5, 0.4), (1.7, 0.4), (1.9, 1.0), (1.4, 1.5), (0.8, 1.2)], -0.04 * side, 0.0, "moss_dark")
    return b


@asset("walls", 30, 900)
def wall_ruined_a():
    b = Builder("wall_ruined_a")
    b.box(0, 0, 0, M, T + 0.16, 0.55, "brick_dark")
    b.box(0, 0, 0.55, M, T, 1.0, "brick_a")
    b.box(-1.0, 0, 1.55, 2.0, T, 1.1, "brick_a")
    b.box(-1.4, 0, 2.65, 1.2, T, 0.5, "brick_b")
    b.box(1.1, 0, 1.55, 1.8, T, 0.5, "brick_a")
    b.box(1.5, 0, 2.05, 0.9, T, 0.4, "brick_b")
    b.rock(0.2, -0.8, 0, 0.7, 0.6, 0.35, "brick_b", 3, sides=6, rings=2)
    b.rock(-0.9, 0.9, 0, 0.5, 0.5, 0.3, "brick_a", 5, sides=6, rings=2)
    b.rock(1.1, -1.0, 0, 0.45, 0.4, 0.28, "brick_light", 7, sides=5, rings=2)
    b.box(0, 0, 1.75 - 0.0, 1.0, T + 0.06, 0.1, "brick_light")
    return b


@asset("walls", 30, 900)
def wall_ruined_b():
    b = Builder("wall_ruined_b")
    b.box(0, 0, 0, M, T + 0.16, 0.55, "brick_dark")
    b.box(0, 0, 0.55, M, T, 2.2, "brick_a")
    b.box(-1.5, 0, 2.75, 1.0, T, 0.55, "brick_b")
    b.box(0.2, 0, 2.75, 0.8, T, 0.3, "brick_a")
    b.box(1.6, 0, 2.75, 0.8, T + 0.2, 0.3, "trim")
    b.box(-1.2, 0, 1.75, 1.0, T + 0.06, 0.1, "brick_light")
    b.rock(-0.3, -0.9, 0, 0.6, 0.5, 0.3, "brick_b", 11, sides=6, rings=2)
    b.rock(1.0, 0.85, 0, 0.5, 0.5, 0.25, "brick_a", 12, sides=5, rings=2)
    b.box(0.4, -0.7, 0, 0.5, 0.35, 0.2, "trim_dark", rot=0.5)
    return b


@asset("walls", 30, 900)
def wall_collapsed():
    b = Builder("wall_collapsed")
    b.box(-1.7, 0, 0, 0.6, T + 0.16, 0.55, "brick_dark")
    b.box(-1.7, 0, 0.55, 0.6, T, 1.8, "brick_a")
    b.box(1.7, 0, 0, 0.6, T + 0.16, 0.55, "brick_dark")
    b.box(1.7, 0, 0.55, 0.6, T, 1.3, "brick_a")
    for i, (x, y, s, c) in enumerate([(-0.9, -0.5, 0.9, "brick_b"), (0.1, 0.4, 1.1, "brick_a"), (0.9, -0.3, 0.8, "brick_light"), (-0.2, -0.9, 0.7, "brick_dark"), (0.3, -0.2, 0.9, "brick_b")]):
        b.rock(x, y, 0, s, s * 0.8, s * 0.5, c, 20 + i, sides=6, rings=2)
    b.box(-0.3, 0.7, 0, 0.9, 0.35, 0.3, "trim", rot=0.4)
    return b


@asset("walls", 30, 600)
def wall_low():
    b = Builder("wall_low")
    b.box(0, 0, 0, M, T + 0.16, 0.4, "brick_dark")
    b.box(0, 0, 0.4, M, T, 0.7, "brick_a")
    b.box(0, 0, 1.1, M, T + 0.2, 0.2, "trim")
    return b


@asset("walls", 30, 600)
def wall_low_ruined():
    b = Builder("wall_low_ruined")
    b.box(0, 0, 0, M, T + 0.16, 0.4, "brick_dark")
    b.box(-1.0, 0, 0.4, 2.0, T, 0.5, "brick_a")
    b.box(1.2, 0, 0.4, 1.6, T, 0.8, "brick_b")
    b.box(1.2, 0, 1.2, 1.6, T + 0.2, 0.2, "trim")
    b.rock(-0.1, -0.7, 0, 0.5, 0.5, 0.28, "brick_a", 31, sides=5, rings=2)
    b.rock(0.2, 0.7, 0, 0.4, 0.4, 0.22, "brick_b", 32, sides=5, rings=2)
    return b


@asset("walls", 30, 1000)
def wall_cell_bars():
    b = Builder("wall_cell_bars")
    b.box(0, 0, 0, M, T, 0.3, "brick_dark")
    b.box(0, 0, H - 0.3, M, T, 0.3, "trim")
    for sx in (-1, 1):
        b.box(sx * (M / 2 - 0.2), 0, 0.3, 0.4, T, H - 0.6, "brick_a")
    n = 9
    for i in range(n):
        b.cyl(-1.5 + i * 3.0 / (n - 1), 0, 0.3, 0.05, H - 0.6, 5, "iron_dark")
    for z in (0.6, 1.8, 2.9):
        b.box(0, 0, z, M - 0.8, 0.1, 0.1, "iron")
    b.box(0.0, -0.06, 0.3, 0.06, 0.1, H - 0.7, "iron")
    b.cyl(0.35, -0.1, 1.4, 0.1, 0.12, 6, "iron")
    return b


@asset("walls", 30, 900)
def wall_block():
    b = Builder("wall_block")
    b.box(0, 0, 0, M, M, 0.55, "brick_dark")
    b.box(0, 0, 0.55, M - 0.2, M - 0.2, H - 0.85, "brick_b")
    b.box(0, 0, H - 0.3, M, M, 0.3, "trim")
    return b


def post_body(b, w, h, body="brick_a", cap="trim"):
    b.box(0, 0, 0, w + 0.2, w + 0.2, 0.55, "brick_dark")
    b.box(0, 0, 0.55, w, w, h - 0.85, body)
    b.box(0, 0, h - 0.3, w + 0.25, w + 0.25, 0.3, cap)


@asset("walls", 30, 600)
def wall_post():
    b = Builder("wall_post")
    post_body(b, 1.0, H + 0.1)
    return b


@asset("walls", 30, 600)
def wall_post_round():
    b = Builder("wall_post_round")
    b.cyl(0, 0, 0, 0.75, 0.55, 8, "brick_dark", r2=0.7)
    b.cyl(0, 0, 0.55, 0.62, H - 0.85, 8, "brick_a", r2=0.58)
    b.cyl(0, 0, H - 0.3, 0.75, 0.3, 8, "trim")
    b.cyl(0, 0, 1.75, 0.66, 0.1, 8, "brick_light")
    return b


@asset("walls", 30, 600)
def wall_post_torch():
    b = Builder("wall_post_torch")
    post_body(b, 1.0, H + 0.1)
    sconce(b, 0, -0.5 - 0.03, 1.6)
    return b


@asset("walls", 30, 900)
def wall_post_tall_gargoyle():
    b = Builder("wall_post_tall_gargoyle")
    post_body(b, 1.0, H + 0.1)
    b.frustum(0, -0.62, 2.1, 0.6, 0.6, 0.5, "stone_dark", 0.5, 0.9)
    b.frustum(0, -0.62, 2.6, 0.5, 0.9, 0.35, "stone", 0.35, 0.7)
    b.box(0, -1.05, 2.5, 0.25, 0.25, 0.25, "stone_dark")
    for sx in (-1, 1):
        b.pyramid(sx * 0.18, -0.9, 2.95, 0.12, 0.12, 0.25, "stone_dark")
        b.box(sx * 0.12, -1.18, 2.58, 0.07, 0.05, 0.07, "ember")
    return b


def floor_base(b, color="flag_dark"):
    b.box(0, 0, -0.34, M, M, 0.25, color)


@asset("floors", 30, 400)
def floor_flagstone():
    b = Builder("floor_flagstone")
    floor_base(b)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 1.0, sy * 1.0, -0.12, 1.94, 1.94, 0.12, "flag_a")
    return b


@asset("floors", 30, 400)
def floor_flagstone_slabs():
    b = Builder("floor_flagstone_slabs")
    floor_base(b)
    cuts_x = [-2.0, -0.6, 2.0]
    rows = [(-2.0, -0.8, [(-2.0, -0.2), (-0.2, 2.0)]), (-0.8, 0.9, [(-2.0, 0.6), (0.6, 2.0)]), (0.9, 2.0, [(-2.0, -0.9), (-0.9, 2.0)])]
    for y0, y1, spans in rows:
        for i, (x0, x1) in enumerate(spans):
            b.box((x0 + x1) / 2, (y0 + y1) / 2, -0.12, x1 - x0 - 0.06, y1 - y0 - 0.06, 0.12, "flag_a" if (i + int(y0)) % 2 == 0 else "flag_warm")
    return b


@asset("floors", 30, 600)
def floor_cracked():
    b = Builder("floor_cracked")
    floor_base(b, "dirt_dark")
    heights = {(-1, -1): 0.0, (1, -1): -0.04, (-1, 1): -0.02}
    for (sx, sy), dz in heights.items():
        b.box(sx * 1.0, sy * 1.0, -0.12 + dz, 1.94, 1.94, 0.12, "flag_a")
    b.box(1.0, 1.0, -0.2, 1.94, 1.94, 0.1, "flag_b", rot=0.0)
    b.rock(1.1, 0.9, -0.05, 1.0, 0.8, 0.14, "rock_b", 41, sides=6, rings=2)
    b.beam((-1.9, -0.1, 0.0), (-0.3, 0.1, 0.0), 0.05, "black", d=0.01)
    return b


@asset("floors", 30, 700)
def floor_checker():
    b = Builder("floor_checker")
    floor_base(b)
    for i in range(4):
        for j in range(4):
            b.box(-1.5 + i, -1.5 + j, -0.1, 0.96, 0.96, 0.1, "flag_a" if (i + j) % 2 == 0 else "flag_b")
    return b


@asset("floors", 30, 900)
def floor_rune():
    b = Builder("floor_rune")
    floor_base(b)
    for i in range(4):
        for j in range(4):
            b.box(-1.5 + i, -1.5 + j, -0.1, 0.96, 0.96, 0.1, "flag_b" if (i + j) % 2 == 0 else "flag_dark")
    n = 12
    for k in range(n):
        a0, a1 = 2 * math.pi * k / n, 2 * math.pi * (k + 1) / n
        b.beam((1.5 * math.cos(a0), 1.5 * math.sin(a0), 0.0), (1.5 * math.cos(a1), 1.5 * math.sin(a1), 0.0), 0.16, "rune_green", d=0.02)
    for k in range(3):
        a0, a1 = 2 * math.pi * k / 3 + 0.5, 2 * math.pi * (k + 1) / 3 + 0.5
        b.beam((1.1 * math.cos(a0), 1.1 * math.sin(a0), 0.0), (1.1 * math.cos(a1), 1.1 * math.sin(a1), 0.0), 0.1, "rune_green", d=0.02)
    return b


@asset("floors", 30, 400)
def floor_brick():
    b = Builder("floor_brick")
    floor_base(b, "brick_dark")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 1.0, sy * 1.0, -0.1, 1.94, 1.94, 0.1, "brick_warm")
    return b


@asset("floors", 30, 500)
def floor_wood():
    b = Builder("floor_wood")
    b.box(0, 0, -0.34, M, M, 0.3, "dirt_dark")
    rnd = random.Random(3)
    for i in range(8):
        dz = rnd.uniform(-0.02, 0.0)
        b.box(-1.75 + i * 0.5, 0, -0.1 + dz, 0.47, M, 0.1, "wood" if i % 3 else "wood_rot")
    for y in (-1.3, 1.3):
        b.box(0, y, -0.2, M, 0.12, 0.08, "wood_dark")
    return b


@asset("floors", 30, 700)
def floor_cave():
    b = Builder("floor_cave")
    rnd = random.Random(8)
    n = 5
    step = M / n
    grid = {}
    for i in range(n + 1):
        for j in range(n + 1):
            edge = i in (0, n) or j in (0, n)
            grid[(i, j)] = (-M / 2 + i * step + (0 if edge else rnd.uniform(-0.2, 0.2)), -M / 2 + j * step + (0 if edge else rnd.uniform(-0.2, 0.2)), 0 if edge else rnd.uniform(-0.1, 0.05))
    pts, faces, colors = [], [], []
    keys = list(grid)
    index = {k: n_ for n_, k in enumerate(keys)}
    pts = [grid[k] for k in keys]
    cols = ["rock_a", "rock_b", "dirt", "rock_c"]
    for i in range(n):
        for j in range(n):
            quad = [index[(i, j)], index[(i + 1, j)], index[(i + 1, j + 1)], index[(i, j + 1)]]
            b.add([pts[q] for q in quad], [[0, 1, 2, 3]], cols[(i * 3 + j * 5) % 4])
    b.box(0, 0, -0.34, M, M, 0.2, "rock_dark")
    return b


@asset("floors", 30, 900)
def floor_rubble():
    b = Builder("floor_rubble")
    floor_base(b)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 1.0, sy * 1.0, -0.12, 1.94, 1.94, 0.12, "flag_a")
    for i, (x, y, s) in enumerate([(-0.9, 0.7, 0.7), (0.6, -0.8, 0.55), (1.2, 1.0, 0.4), (-1.3, -1.2, 0.35), (0.1, 0.2, 0.3)]):
        b.rock(x, y, 0, s, s * 0.85, s * 0.6, ("brick_b", "brick_a", "flag_b", "brick_light", "rock_b")[i], 60 + i, sides=6, rings=2)
    b.beam((0.5, 1.4, 0.04), (1.6, 0.4, 0.05), 0.12, "wood_rot", d=0.08)
    return b


@asset("floors", 30, 700)
def floor_drain():
    b = Builder("floor_drain")
    floor_base(b)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 1.25, sy * 1.25, -0.12, 1.44, 1.44, 0.12, "flag_a")
    b.box(0, 0, -0.3, 1.0, 1.0, 0.2, "black")
    for i in range(5):
        b.box(-0.4 + i * 0.2, 0, -0.1, 0.07, 1.0, 0.1, "iron")
    b.box(0, 0, -0.12, 1.1, 0.07, 0.1, "iron_dark")
    return b


@asset("floors", 30, 700)
def floor_moss():
    b = Builder("floor_moss")
    floor_base(b)
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 1.0, sy * 1.0, -0.12, 1.94, 1.94, 0.12, "flag_a")
    b.slab([(-1.8, -1.0), (-0.4, -1.5), (0.6, -0.8), (0.4, 0.3), (-0.8, 0.8), (-1.7, 0.2)], 0.0, 0.025, "moss")
    b.slab([(0.7, 0.9), (1.6, 0.5), (1.9, 1.5), (1.0, 1.9)], 0.0, 0.025, "moss_dark")
    return b


@asset("structure", 30, 1200)
def pillar_square():
    b = Builder("pillar_square")
    b.box(0, 0, 0, 1.6, 1.6, 0.4, "stone_dark")
    b.frustum(0, 0, 0.4, 1.4, 1.4, 0.25, "trim", 1.1, 1.1)
    b.frustum(0, 0, 0.65, 1.1, 1.1, 2.4, "stone", 0.95, 0.95)
    b.frustum(0, 0, 3.05, 0.95, 0.95, 0.3, "trim", 1.4, 1.4)
    b.box(0, 0, 3.35, 1.6, 1.6, 0.3, "stone_dark")
    return b


@asset("structure", 30, 1200)
def pillar_round():
    b = Builder("pillar_round")
    b.cyl(0, 0, 0, 0.95, 0.35, 8, "stone_dark", r2=0.9)
    b.lathe(0, 0, 0.35, [(0.8, 0), (0.62, 0.2), (0.55, 0.45), (0.5, 2.5), (0.56, 2.7), (0.8, 2.95)], 8, "stone")
    b.cyl(0, 0, 3.3, 0.95, 0.3, 8, "stone_dark")
    for z in (1.5, 2.3):
        b.cyl(0, 0, z, 0.56, 0.1, 8, "trim")
    return b


@asset("structure", 30, 1500)
def pillar_broken():
    b = Builder("pillar_broken")
    b.box(0, 0, 0, 1.6, 1.6, 0.4, "stone_dark")
    b.frustum(0, 0, 0.4, 1.4, 1.4, 0.25, "trim", 1.1, 1.1)
    b.frustum(0, 0, 0.65, 1.1, 1.1, 1.0, "stone", 1.02, 1.02)
    b.frustum(-0.2, 0.0, 1.65, 0.9, 1.0, 0.7, "stone", 0.7, 0.9)
    b.frustum(0.3, -0.1, 1.65, 0.5, 0.8, 0.35, "stone", 0.35, 0.6)
    b.frustum(0.9, 0.9, 0.0, 0.9, 0.8, 0.35, "stone", 0.7, 0.6, rot=0.4)
    b.rock(-1.0, -0.8, 0, 0.7, 0.6, 0.4, "stone", 71, sides=6, rings=2)
    b.rock(1.0, -0.9, 0, 0.5, 0.5, 0.3, "trim_dark", 72, sides=5, rings=2)
    b.rock(-1.2, 0.9, 0, 0.45, 0.4, 0.25, "stone_dark", 73, sides=5, rings=2)
    return b


@asset("structure", 30, 1500)
def pillar_carved():
    b = Builder("pillar_carved")
    b.box(0, 0, 0, 1.7, 1.7, 0.4, "stone_dark")
    b.frustum(0, 0, 0.4, 1.5, 1.5, 0.25, "trim", 1.15, 1.15)
    b.box(0, 0, 0.65, 1.15, 1.15, 2.4, "stone")
    for z in (0.95, 2.75):
        b.box(0, 0, z, 1.3, 1.3, 0.14, "trim")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box(sx * 0.6, sy * 0.6, 0.65, 0.16, 0.16, 2.3, "stone_dark")
    for rot in (0, math.pi / 2, math.pi, -math.pi / 2):
        with b.at(rot=rot):
            b.plate(circle_poly(0, 1.9, 0.24, 6), -0.62, -0.57, "bone")
            b.box(-0.08, -0.62, 1.82, 0.07, 0.05, 0.07, "rune_red")
            b.box(0.08, -0.62, 1.82, 0.07, 0.05, 0.07, "rune_red")
            b.box(0, -0.62, 1.2, 0.08, 0.03, 0.4, "rune_red")
            b.box(0, -0.62, 1.4, 0.3, 0.03, 0.07, "rune_red")
    b.frustum(0, 0, 3.05, 1.15, 1.15, 0.3, "trim", 1.5, 1.5)
    b.box(0, 0, 3.35, 1.7, 1.7, 0.3, "stone_dark")
    return b


@asset("structure", 30, 800)
def pillar_stump():
    b = Builder("pillar_stump")
    b.box(0, 0, 0, 1.6, 1.6, 0.4, "stone_dark")
    b.frustum(0, 0, 0.4, 1.4, 1.4, 0.25, "trim", 1.1, 1.1)
    b.frustum(0, 0, 0.65, 1.1, 1.1, 0.5, "stone", 1.0, 1.0)
    b.frustum(0.1, 0, 1.15, 0.9, 0.8, 0.2, "stone", 0.6, 0.5)
    b.rock(0.9, -0.8, 0, 0.6, 0.5, 0.3, "stone", 74, sides=5, rings=2)
    return b


def arch_free(name, ruined=False, wide=False):
    b = Builder(name)
    a = 1.5 if wide else 1.2
    spring = 1.4
    ring = 0.5
    d = 1.0
    for sx in (-1, 1):
        px = sx * (a + ring / 2 + 0.1)
        w = ring + 0.4
        b.box(px, 0, 0, w, d + 0.2, 0.45, "stone_dark")
        if ruined and sx == 1:
            b.box(px, 0, 0.45, w, d, 0.6, "stone")
            b.frustum(px, 0, 1.05, w, d, 0.4, "stone", w * 0.6, d * 0.6)
            b.rock(px + 0.5, -0.9, 0, 0.8, 0.7, 0.5, "stone", 80, sides=6, rings=2)
            b.rock(px - 0.3, 0.9, 0, 0.6, 0.5, 0.35, "stone_dark", 81, sides=5, rings=2)
        else:
            b.box(px, 0, 0.45, w, d, spring - 0.45, "stone")
            b.box(px, 0, spring, w + 0.1, d + 0.1, 0.2, "trim")
    top = spring + 0.2
    inner = pointed_arc(0, a, top, 4)
    outer = pointed_arc(0, a + ring, top, 4)
    if not ruined:
        poly = [(-a, top)] + inner[1:-1] + [(a, top), (a + ring, top)] + list(reversed(outer))[1:-1] + [(-a - ring, top)]
        b.plate(poly, -d / 2, d / 2, "stone")
        b.box(0, -d / 2 - 0.03, top + arch_rise(a + ring / 2) - 0.2, 0.42, 0.14, 0.5, "trim")
    else:
        cut = -0.15
        li = [p for p in inner if p[0] <= cut]
        lo = [p for p in outer if p[0] <= cut]
        poly = [(-a, top)] + li[1:] + [(cut, li[-1][1]), (cut, lo[-1][1])] + list(reversed(lo))[1:-1] + [(-a - ring, top)]
        b.plate(poly, -d / 2, d / 2, "stone")
    return b


@asset("structure", 30, 1500)
def arch_doorway():
    return arch_free("arch_doorway")


@asset("structure", 30, 1500)
def arch_wide():
    return arch_free("arch_wide", wide=True)


@asset("structure", 30, 1500)
def arch_ruined():
    return arch_free("arch_ruined", ruined=True)


def stringers(b, w, run, rise, color="stone_dark"):
    for sx in (-1, 1):
        extrude_x(b, sx * (w / 2 + 0.2), 0, 0, 0.4, [(-run / 2, 0), (run / 2, 0), (run / 2, rise + 0.45), (run / 2 - 0.4, rise + 0.45), (-run / 2, 0.45)], color)


@asset("stairs", 30, 1200)
def stairs_short():
    b = Builder("stairs_short")
    stair_flight(b, 0, 0, 0, 4.0, 3.0, 0.8, 4, "flag_a")
    stringers(b, 4.0, 3.0, 0.8)
    return b


@asset("stairs", 30, 1500)
def stairs_long():
    b = Builder("stairs_long")
    stair_flight(b, 0, 0, 0, 4.0, 6.0, 2.0, 10, "flag_a")
    stringers(b, 4.0, 6.0, 2.0)
    return b


@asset("stairs", 30, 1200)
def stairs_narrow():
    b = Builder("stairs_narrow")
    stair_flight(b, 0, 0, 0, 2.0, 4.0, 1.2, 6, "flag_a")
    stringers(b, 2.0, 4.0, 1.2)
    return b


@asset("stairs", 30, 900)
def platform_dais():
    b = Builder("platform_dais")
    b.box(0, 0, 0, 4.0, 4.0, 0.4, "stone_dark")
    b.box(0, 0, 0.4, 3.6, 3.6, 0.4, "flag_a")
    b.box(0, 0, 0.8, 3.8, 3.8, 0.12, "trim")
    b.box(0, 0, 0.92, 3.2, 3.2, 0.04, "flag_b")
    return b


@asset("stairs", 30, 900)
def platform_block():
    b = Builder("platform_block")
    b.box(0, 0, 0, 4.0, 4.0, 0.6, "stone_dark")
    b.box(0, 0, 0.6, 4.0, 4.0, 0.2, "trim")
    return b
