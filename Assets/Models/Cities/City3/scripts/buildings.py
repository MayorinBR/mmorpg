import math

from lib import *


def column(b, x, y, z0, r, h, style="plain", seg=8):
    prof = [(r * 1.35, 0), (r * 1.35, 0.2), (r, 0.3), (r, h - 0.5)]
    if style == "papyrus":
        prof += [(r * 0.85, h - 0.4), (r * 1.5, h - 0.15), (r * 1.1, h)]
    else:
        prof += [(r * 1.2, h - 0.35), (r * 1.45, h - 0.15), (r * 1.45, h)]
    b.lathe(x, y, z0, prof + [(0, h)], seg, "trim")


def shell(b, w, d, h, floors=1, plinth=True, crenel=True, side_windows=True, fh=3.6):
    block_body(b, w, d, h, plinth=plinth, band_z=fh if floors > 1 else None)
    parapet(b, w, d, h + 0.25, 0.3, 0.55)
    if crenel:
        crenels(b, w, d, h + 0.8, 1.7, 0.4, 0.4)
    if side_windows:
        for side in ("right", "left", "back"):
            rot, ys = wall_frame(side, w, d)
            span = d if side != "back" else w
            n = max(int(span / 3.0), 1)
            with b.at(rot=rot):
                for f in range(floors):
                    for i in range(n):
                        window(b, -span / 2 + span * (i + 0.5) / n, 1.7 + f * fh, 0.8, 1.2, ys)
    vigas(b, w, d, h - 0.5)
    quoins(b, w, d, h, step=0.7)
    for i in range(4):
        pot(b, -w / 2 + 1.0 + i * (w - 2.0) / 3, d / 2 - 0.8, h + 0.25, 0.28)


def statue_seated(b, x, y, z0, s=1.0, rot=0.0, color="sand_light"):
    with b.at(x, y, z0, rot):
        b.box(0, 0, 0, 2.2 * s, 2.6 * s, 0.4 * s, "stone")
        b.box(0, 0, 0.4 * s, 1.9 * s, 2.3 * s, 0.4 * s, "stone_dark")
        b.box(0, 0.25 * s, 0.8 * s, 1.6 * s, 2.0 * s, 0.9 * s, color)
        b.box(0, 1.0 * s, 1.7 * s, 1.5 * s, 0.5 * s, 2.4 * s, "sand")
        b.box(0, 0.35 * s, 1.7 * s, 1.1 * s, 0.8 * s, 1.5 * s, color)
        b.frustum(0, 0.35 * s, 3.2 * s, 1.1 * s, 0.8 * s, 0.35 * s, color, 0.9 * s, 0.7 * s)
        b.box(0, 0.3 * s, 3.55 * s, 0.7 * s, 0.7 * s, 0.7 * s, color)
        for sx in (-1, 1):
            b.box(sx * 0.62 * s, 0.3 * s, 3.5 * s, 0.32 * s, 0.7 * s, 1.0 * s, "cloth_blue")
            b.box(sx * 0.62 * s, -0.25 * s, 1.7 * s, 0.4 * s, 0.4 * s, 0.4 * s, color)
            b.box(sx * 0.6 * s, -0.65 * s, 1.7 * s, 0.4 * s, 1.0 * s, 0.35 * s, color)
            b.box(sx * 0.55 * s, -1.05 * s, 0.8 * s, 0.4 * s, 0.4 * s, 0.9 * s, color)
            b.box(sx * 0.55 * s, -1.2 * s, 0.4 * s, 0.5 * s, 0.7 * s, 0.4 * s, color)
            b.box(sx * 0.6 * s, -0.25 * s, 2.15 * s, 0.35 * s, 0.35 * s, 0.9 * s, color)
        b.lathe(0, 0.3 * s, 4.25 * s, [(0.28 * s, 0), (0.36 * s, 0.3 * s), (0.2 * s, 0.7 * s), (0, 0.85 * s)], 6, "gold")
        b.box(0, -0.1 * s, 3.65 * s, 0.35 * s, 0.1 * s, 0.14 * s, "black")


def sphinx(b, x, y, z0, s=1.0, rot=0.0):
    with b.at(x, y, z0, rot):
        b.box(0, 0, 0, 2.4 * s, 5.2 * s, 0.3 * s, "stone")
        b.box(0, 0, 0.3 * s, 2.0 * s, 4.8 * s, 0.3 * s, "stone_dark")
        b.box(0, 0.8 * s, 0.6 * s, 1.5 * s, 2.6 * s, 1.0 * s, "sand_light")
        b.frustum(0, 1.9 * s, 0.6 * s, 1.5 * s, 1.2 * s, 1.5 * s, "sand_light", 1.2 * s, 0.8 * s)
        b.box(0, -0.2 * s, 1.6 * s, 1.3 * s, 1.4 * s, 0.4 * s, "sand_light")
        for sx in (-1, 1):
            b.box(sx * 0.5 * s, -1.5 * s, 0.6 * s, 0.4 * s, 1.5 * s, 0.4 * s, "sand_light")
            for k in (-1, 0, 1):
                b.box(sx * 0.5 * s + k * 0.12 * s, -2.3 * s, 0.6 * s, 0.1 * s, 0.25 * s, 0.25 * s, "trim")
            b.box(sx * 0.62 * s, 2.0 * s, 0.6 * s, 0.35 * s, 0.9 * s, 0.7 * s, "sand")
            b.box(sx * 0.8 * s, -0.55 * s, 1.95 * s, 0.35 * s, 0.4 * s, 1.1 * s, "cloth_blue")
        b.box(0, -0.7 * s, 2.0 * s, 0.9 * s, 0.9 * s, 0.9 * s, "sand_light")
        b.frustum(0, -0.7 * s, 2.9 * s, 1.4 * s, 1.3 * s, 0.55 * s, "cloth_blue", 0.9 * s, 0.9 * s)
        b.box(0, -1.2 * s, 2.1 * s, 0.22 * s, 0.2 * s, 0.4 * s, "sand")
        b.box(0, -1.2 * s, 1.55 * s, 0.22 * s, 0.25 * s, 0.5 * s, "gold")
        b.box(0, -1.15 * s, 3.45 * s, 0.14 * s, 0.14 * s, 0.35 * s, "gold")
        for sx in (-1, 1):
            b.box(sx * 0.2 * s, -1.17 * s, 2.45 * s, 0.14 * s, 0.05 * s, 0.1 * s, "black")
        b.box(0, 3.1 * s, 0.6 * s, 0.3 * s, 0.9 * s, 0.3 * s, "sand")
        b.box(0, 3.4 * s, 0.6 * s, 0.25 * s, 0.6 * s, 0.7 * s, "sand")


def obelisk(b, x, y, z0, s=1.0, glyphs=True):
    b.box(x, y, z0, 2.0 * s, 2.0 * s, 0.35 * s, "stone")
    b.box(x, y, z0 + 0.35 * s, 1.7 * s, 1.7 * s, 0.3 * s, "stone_dark")
    b.frustum(x, y, z0 + 0.65 * s, 1.2 * s, 1.2 * s, 5.0 * s, "sand_light", 0.7 * s, 0.7 * s)
    b.pyramid(x, y, z0 + 5.65 * s, 0.7 * s, 0.7 * s, 0.7 * s, "gold")
    if glyphs:
        for side in range(4):
            with b.at(x, y, z0, side * math.pi / 2):
                for i in range(7):
                    t = i / 7
                    wd = (1.2 - 0.5 * t * 0.9) * s
                    for j in (-1, 1):
                        b.plate(rect_poly(j * 0.22 * wd, 0.9 * s + i * 0.55 * s, 0.14 * s * (1.2 - t * 0.4), 0.3 * s), -(wd / 2) - 0.02 * s + t * 0.0, -(wd / 2) + 0.05 * s, "stone_dark")







def banner(b, x, y, z0, h=4.0, color="cloth_red", rot=0.0):
    with b.at(x, y, z0, rot):
        b.cyl(0, 0, 0, 0.07, h, 4, "wood_dark")
        b.box(0, 0, h - 0.05, 1.2, 0.08, 0.08, "wood_dark")
        b.plate([(-0.5, h - 1.9), (0.5, h - 1.9), (0.5, h - 0.1), (-0.5, h - 0.1)], -0.04, 0.04, color)
        b.plate([(-0.5, h - 1.9), (0, h - 2.3), (0.5, h - 1.9)], -0.04, 0.04, color)
        b.plate(circle_poly(0, h - 1.0, 0.22, 6), -0.06, 0.06, "gold")


def table(b, x, y, z0=0.0, r=0.8):
    b.cyl(x, y, z0, 0.12, 0.8, 4, "wood_dark")
    b.cyl(x, y, z0 + 0.8, r, 0.1, 8, "wood")


def stool(b, x, y, z0=0.0):
    b.cyl(x, y, z0, 0.2, 0.45, 6, "wood_dark")
    b.cyl(x, y, z0 + 0.45, 0.3, 0.08, 6, "wood")


@asset("buildings", 2000, 5000)
def shop_tools():
    b = Builder("shop_tools")
    w, d, h = 9.0, 7.0, 4.6
    shell(b, w, d, h)
    y = -d / 2
    door(b, -1.8, 0.45, 1.6, 2.6, y)
    window(b, 2.4, 1.2, 2.2, 1.6, y, arched=False)
    b.box(2.4, y - 0.5, 0, 2.4, 0.7, 1.1, "wood")
    awning(b, 0, y, 3.5, w - 1.2, 1.6, "cloth_cream", "cloth_ochre")
    hanging_sign(b, w / 2 - 0.5, 1.3, y, "pick")
    for sx in (-1, 1):
        b.cyl(-4.9 + sx * 0.0, y - 1.2, 0, 0.1, 2.2, 4, "wood_dark") if sx == -1 else None
    b.box(-3.6, y - 1.4, 1.5, 1.8, 0.12, 0.12, "wood_dark")
    for sx in (-4.5, -2.7):
        b.cyl(sx, y - 1.4, 0, 0.1, 2.1, 4, "wood_dark")
    for i, k in enumerate(("hammer", "pick", "hammer")):
        icon(b, k, -4.1 + i * 0.55, 0.65, y - 1.46, 0.75)
    barrel(b, 4.4, y - 1.2, 0)
    crate(b, 3.5, y - 1.3, 0, 0.8)
    pot(b, 0.6, y - 0.8, 0)
    slate_dome(b, 2.5, 1.5, h + 0.8, 1.4, 1.4)
    b.box(-2.5, 1.5, h + 0.8, 0.9, 0.9, 2.0, "sand_light")
    return b


@asset("buildings", 2000, 5000)
def shop_ammo():
    b = Builder("shop_ammo")
    w, d, h = 8.5, 7.0, 4.4
    shell(b, w, d, h)
    y = -d / 2
    door(b, 1.8, 0.45, 1.6, 2.6, y)
    window(b, -2.0, 1.3, 2.0, 1.5, y, arched=False)
    b.box(-2.0, y - 0.5, 0, 2.2, 0.7, 1.1, "wood")
    awning(b, 0, y, 3.4, w - 1.2, 1.5, "cloth_cream", "cloth_green")
    hanging_sign(b, -w / 2 + 0.5, 1.3, y, "arrow")
    b.cyl(3.9, y - 1.3, 0, 0.55, 1.1, 6, "wood")
    for i in range(5):
        a = i * 1.26
        b.cyl(3.9 + 0.2 * math.cos(a), y - 1.3 + 0.2 * math.sin(a), 1.1, 0.03, 0.8, 4, "wood_dark")
    with b.at(-4.9, y - 2.2, 0):
        b.cyl(0, 0, 0, 0.08, 1.7, 4, "wood_dark")
        b.plate(circle_poly(0, 1.6, 0.8, 10), -0.12, 0.0, "cloth_cream")
        b.plate(circle_poly(0, 1.6, 0.5, 10), -0.16, 0.0, "cloth_red")
        b.plate(circle_poly(0, 1.6, 0.2, 8), -0.2, 0.0, "gold")
    crate(b, 0.3, y - 1.2, 0, 0.8)
    crate(b, 1.0, y - 1.3, 0, 0.6, 0.4)
    barrel(b, -4.0, y - 1.0, 0, 0.35, 0.8)
    slate_dome(b, -2.5, 1.5, h + 0.8, 1.3, 1.3)
    return b


@asset("buildings", 2000, 5000)
def shop_weapons():
    b = Builder("shop_weapons")
    w, d, h = 9.5, 7.5, 4.8
    shell(b, w, d, h)
    y = -d / 2
    door(b, 0, 0.45, 1.8, 2.8, y)
    for s in (-1, 1):
        window(b, s * 3.1, 1.4, 1.4, 1.5, y, arched=False)
        icon(b, "shield", s * 3.1, 3.4, y, 0.8)
    icon(b, "swords", 0, 3.7, y, 0.7)
    awning(b, 0, y, 3.2, w - 1.5, 1.4, "cloth_cream", "cloth_red")
    hanging_sign(b, w / 2 - 0.4, 1.3, y, "sword")
    with b.at(-3.6, y - 1.6, 0):
        b.box(0, 0, 0, 2.6, 0.2, 0.15, "wood_dark")
        b.box(0, 0, 1.6, 2.6, 0.14, 0.14, "wood_dark")
        for sx in (-1, 1):
            b.cyl(sx * 1.25, 0, 0, 0.09, 1.9, 4, "wood_dark")
        for i in range(4):
            icon(b, "sword", -0.9 + i * 0.6, 0.3, -0.1, 0.8)
    with b.at(3.8, y - 1.5, 0):
        b.cyl(0, 0, 0, 0.35, 0.9, 6, "metal")
        b.cyl(0, 0, 0.9, 0.5, 0.7, 6, "metal", r2=0.3)
        b.lathe(0, 0, 1.6, [(0.28, 0), (0.3, 0.3), (0, 0.4)], 6, "metal_light")
        b.box(0, 0, 0.9, 1.1, 0.25, 0.2, "metal")
    barrel(b, 2.0, y - 1.1, 0, 0.35, 0.8)
    slate_dome(b, 0, 1.5, h + 0.8, 1.6, 1.7)
    return b


@asset("buildings", 2000, 5000)
def blacksmith():
    b = Builder("blacksmith")
    w, d, h = 10.0, 8.0, 4.8
    shell(b, w, d, h)
    y = -d / 2
    b.plate(arch_poly(-1.6, 0.45, 4.2, 3.6, 6), y - 0.12, y + 0.02, "black")
    b.plate(arch_poly(-1.6, 0.45, 3.5, 3.2, 6), y - 0.16, y + 0.02, "door")
    b.plate(rect_poly(-1.6, 0.45, 1.2, 1.0), y - 0.2, y + 0.02, "fire")
    door(b, 3.2, 0.45, 1.4, 2.5, y)
    awning(b, -1.6, y, 3.7, 5.0, 1.8, "cloth_cream", "cloth_ochre")
    hanging_sign(b, w / 2 - 0.5, 1.3, y, "anvil")
    cx, cy = w / 2 - 2.0, d / 2 - 1.8
    b.frustum(cx, cy, h + 0.25, 1.8, 1.8, 3.6, "stone", 1.2, 1.2)
    b.box(cx, cy, h + 3.85, 1.5, 1.5, 0.25, "stone_dark")
    b.box(cx, cy, h + 4.1, 0.9, 0.9, 0.3, "black")
    with b.at(-1.6, y - 1.6, 0):
        b.cyl(0, 0, 0, 0.4, 0.55, 6, "wood")
        b.box(0, 0, 0.55, 0.5, 1.2, 0.25, "metal")
        b.box(0, 0, 0.8, 0.25, 0.6, 0.25, "metal_light")
        b.frustum(0, 0, 0.55, 0.2, 0.3, 0.12, "metal", 0.15, 0.15, 0)
        b.frustum(0.0, 0.7, 0.8, 0.2, 0.5, 0.3, "metal_light", 0.1, 0.1)
    with b.at(-4.2, y - 1.0, 0):
        b.box(0, 0, 0, 0.5, 0.9, 0.9, "wood_dark")
        b.plate(circle_poly(0, 1.1, 0.55, 10), -0.12, 0.12, "stone")
        b.plate(circle_poly(0, 1.1, 0.12, 6), -0.14, 0.14, "metal")
    barrel(b, 3.6, y - 1.4, 0)
    barrel(b, 4.3, y - 1.3, 0, 0.35, 0.8)
    b.cyl(-5.4, 1.0, 0, 0.5, 0.9, 6, "metal")
    for i in range(4):
        b.box(-5.4, 1.0, 0.9 + i * 0.12, 0.8 - i * 0.12, 0.8 - i * 0.12, 0.12, "metal_light")
    return b


@asset("buildings", 2000, 5000)
def library():
    b = Builder("library")
    w, d, h = 12.0, 10.0, 6.4
    block_body(b, w, d, h, band_z=3.3)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    y = -d / 2
    stairs(b, 0, y - 1.6, 0, 7.0, 3.0, 1.0, 5)
    b.box(0, y - 1.5, 1.0, 7.4, 3.2, 0.25, "trim")
    for sx in (-3.0, -1.0, 1.0, 3.0):
        column(b, sx, y - 2.5, 1.25, 0.4, 4.6)
    b.box(0, y - 2.5, 5.85, 8.0, 3.4, 0.5, "trim")
    b.gable(0, y - 2.5, 6.35, 8.4, 3.6, 1.3, "sand_light")
    door(b, 0, 1.25, 2.0, 3.2, y)
    for sx in (-4.2, 4.2):
        window(b, sx, 1.8, 0.9, 1.6, y)
        window(b, sx, 4.5, 0.9, 1.3, y)
    for sx in (-3.0, 0, 3.0):
        window(b, sx, 4.6, 0.9, 1.2, y)
    for side in ("right", "left"):
        rot, ys = wall_frame(side, w, d)
        with b.at(rot=rot):
            for i in range(3):
                window(b, -3.0 + i * 3.0, 1.8, 0.9, 1.5, ys)
                window(b, -3.0 + i * 3.0, 4.6, 0.9, 1.3, ys)
    icon(b, "book", 0, 8.1, y - 4.2, 0.9)
    b.cyl(0, 0, h + 0.25, 3.0, 1.2, 8, "sand_light")
    slate_dome(b, 0, 0, h + 1.45, 3.1, 3.3)
    for sx in (-4.2, 4.2):
        for sy in (-3.4, 3.4):
            b.cyl(sx, sy, h + 0.25, 0.9, 1.0, 6, "sand_light")
            slate_dome(b, sx, sy, h + 1.25, 0.95, 1.1, 6)
    banner(b, -5.2, y - 3.0, 0, 3.6, "cloth_blue")
    banner(b, 5.2, y - 3.0, 0, 3.6, "cloth_blue")
    return b


@asset("buildings", 2000, 5000)
def temple():
    b = Builder("temple")
    W, D = 18.0, 22.0
    for i, (s, hh) in enumerate(((1.0, 0.5), (0.94, 0.5), (0.88, 0.5))):
        b.box(0, 0, i * 0.5, W * s + 2, D * s + 2, 0.5, "stone")
    base = 1.5
    stairs(b, 0, -D / 2 - 1.2, 0, 7.0, 3.0, 1.5, 6)
    fw = 4.4
    for sx in (-1, 1):
        b.frustum(sx * 6.4, -D / 2 + 1.5, base, 5.0, 3.6, 10.0, "sand_light", 3.8, 2.6)
        b.box(sx * 6.4, -D / 2 + 1.5, base + 9.9, 4.4, 3.2, 0.5, "trim")
        b.frustum(sx * 6.4, -D / 2 + 1.5, base + 10.4, 4.4, 3.2, 0.5, "gold", 3.6, 2.4)
        for k in (-1, 0, 1):
            b.plate(rect_poly(sx * 6.4 + k * 1.1, base + 2.0, 0.5, 6.0), -D / 2 - 0.4, -D / 2 - 0.2, "sand")
        b.cyl(sx * 9.8, -D / 2 + 1.5, base, 0.08, 9.0, 4, "wood_dark")
        banner(b, sx * 3.4, -D / 2 - 0.4, base + 0.0, 8.0, "cloth_blue" if sx < 0 else "cloth_red")
    b.plate(wall_poly(5.0, 7.0, [(0, 1.5, 3.8)], 6), -D / 2 + 0.6, -D / 2 + 2.4, "sand", x=0, z=base)
    b.box(0, -D / 2 + 1.5, base + 7.0, 5.4, 2.0, 0.5, "trim")
    for sx in (-1, 1):
        for k in range(4):
            column(b, sx * 3.6, -D / 2 + 4.5 + k * 3.2, base, 0.45, 5.0, "papyrus")
    b.box(0, 3.5, base, 11.0, 11.0, 5.0, "sand")
    b.box(0, 3.5, base + 4.7, 11.6, 11.6, 0.5, "trim")
    b.plate(arch_poly(0, base, 2.4, 3.6, 6), -D / 2 + 9.0, -D / 2 + 9.3, "door")
    for sx in (-3.6, 3.6):
        window(b, sx, base + 1.8, 0.9, 1.6, -2.2)
    b.cyl(0, 3.5, base + 5.2, 3.6, 1.8, 8, "sand_light")
    slate_dome(b, 0, 3.5, base + 7.0, 3.8, 4.2)
    for sx in (-4.3, 4.3):
        for sy in (-0.8, 7.8):
            b.cyl(sx, sy, base + 5.2, 0.9, 1.2, 6, "sand_light")
            slate_dome(b, sx, sy, base + 6.4, 0.95, 1.2, 6)
    for sx in (-1, 1):
        obelisk(b, sx * 8.2, -D / 2 - 2.6, 0, 0.9)
        statue_seated(b, sx * 3.0, -D / 2 - 3.8, 0, 0.8, math.pi)
    return b


@asset("buildings", 2000, 5000)
def tavern():
    b = Builder("tavern")
    w, d, h = 11.0, 8.0, 7.4
    block_body(b, w, d, h, band_z=3.7)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    crenels(b, w, d, h + 0.75, 1.8, 0.4, 0.35)
    y = -d / 2
    door(b, 0, 0.45, 1.9, 2.7, y)
    for s in (-1, 1):
        window(b, s * 3.4, 1.4, 1.1, 1.5, y, shutters=True)
        for k in (-1, 1):
            window(b, s * 3.4 + k * 1.0 * 0, 4.6, 0.9, 1.4, y)
    door(b, 0, 4.0, 1.2, 2.3, y, color="window")
    balcony(b, 0, y, 3.8, 6.5, 1.5)
    for sx in (-2.4, 2.4):
        window(b, sx, 4.4, 0.9, 1.5, y)
    for side in ("right", "left"):
        rot, ys = wall_frame(side, w, d)
        with b.at(rot=rot):
            for i in range(2):
                window(b, -2.0 + i * 4.0, 1.7, 0.9, 1.3, ys)
                window(b, -2.0 + i * 4.0, 5.2, 0.9, 1.3, ys)
    vigas(b, w, d, h - 0.5)
    hanging_sign(b, w / 2 - 0.6, 1.6, y, "mug")
    b.cyl(-w / 2 + 1.8, d / 2 - 1.8, h + 0.25, 1.4, 1.3, 8, "sand_light")
    slate_dome(b, -w / 2 + 1.8, d / 2 - 1.8, h + 1.55, 1.5, 1.8)
    b.box(3.0, 1.5, h + 0.25, 3.0, 3.0, 0.15, "wood")
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cyl(3.0 + sx * 1.3, 1.5 + sy * 1.3, h + 0.4, 0.06, 2.0, 4, "wood_dark")
    b.box(3.0, 1.5, h + 2.4, 3.2, 3.2, 0.1, "cloth_red")
    for tx, ty in ((-4.0, y - 2.2), (4.2, y - 2.4)):
        table(b, tx, ty)
        stool(b, tx - 1.1, ty)
        stool(b, tx + 1.1, ty)
    barrel(b, -2.2, y - 1.0, 0)
    barrel(b, -1.5, y - 1.1, 0, 0.35, 0.8)
    for sx in (-1.8, 1.8):
        b.cyl(sx, y - 0.4, 2.7, 0.12, 0.35, 4, "gold")
    return b


@asset("landmarks", 5000, 10000)
def pyramid():
    b = Builder("pyramid")
    S = 38.0
    for i, s in enumerate((1.18, 1.1, 1.04)):
        b.box(0, 0, i * 0.5, S * s, S * s, 0.5, "stone" if i % 2 == 0 else "stone_dark")
    n = 26
    z = 1.5
    ch = 0.9
    for i in range(n):
        w0 = S * (1 - i / n * 0.93)
        w1 = S * (1 - (i + 1) / n * 0.93)
        b.frustum(0, 0, z, w0, w0, ch, "sand_light" if i % 2 == 0 else "sand", w1, w1)
        z += ch
    top = S * (1 - 0.93)
    b.pyramid(0, 0, z, top, top, 2.8, "gold")
    for i in range(n):
        w0 = S * (1 - (i + 0.5) / n * 0.93)
        b.box(0, -w0 / 2 + 0.1, 1.5 + i * ch, 4.8, 0.8, ch, "trim" if i % 2 == 0 else "sand_light")
    for sx in (-1, 1):
        for i in range(n):
            w0 = S * (1 - (i + 0.5) / n * 0.93)
            b.box(sx * 2.7, -w0 / 2 + 0.1, 1.5 + i * ch, 0.6, 0.7, ch + 0.4, "sand_dark")
    gz = 1.5
    b.plate(wall_poly(7.0, 6.0, [(0, 1.7, 3.0)], 6), -S / 2 + 0.5, -S / 2 + 2.5, "sand", x=0, z=gz)
    stairs(b, 0, -S / 2 - 2.6, 0, 9.0, 5.2, 1.5, 6)
    for sx in (-3.0, -1.0, 1.0, 3.0):
        column(b, sx, -S / 2 - 1.4, gz, 0.5, 5.2, "papyrus")
    b.box(0, -S / 2 - 1.4, gz + 5.2, 8.6, 2.8, 0.6, "trim")
    b.gable(0, -S / 2 - 1.4, gz + 5.8, 8.8, 3.0, 1.0, "gold")
    b.plate(arch_poly(0, gz, 2.6, 3.6, 6), -S / 2 + 0.4, -S / 2 + 0.6, "door")
    for k in range(4):
        sy = -S / 2 - 9.0 - k * 7.5
        for sx in (-1, 1):
            sphinx(b, sx * 9.5, sy, 0, 1.0, -sx * math.pi / 2)
            b.cyl(sx * 4.2, sy, 0, 0.3, 1.2, 6, "stone")
            b.lathe(sx * 4.2, sy, 1.2, [(0.55, 0), (0.7, 0.1), (0.4, 0.3), (0.5, 0.7), (0.0, 0.9)], 6, "metal")
            b.lathe(sx * 4.2, sy, 2.1, [(0.4, 0), (0.25, 0.4), (0.1, 0.8), (0, 1.0)], 6, "fire")
    for sx in (-1, 1):
        obelisk(b, sx * 16.0, -S / 2 - 4.0, 0, 1.2)
        statue_seated(b, sx * 11.5, -S / 2 - 3.2, 0, 0.9, math.pi)
    return b


@asset("stairs", 200, 2500)
def stairs_small():
    b = Builder("stairs_small")
    n = 7
    stairs(b, 0, 0, 0, 4.0, 5.6, 2.8, n, "stone")
    for s in (-1, 1):
        b.frustum(s * 2.45, 0, 0, 0.9, 6.0, 3.2, "sand_light", 0.9, 4.6)
        b.box(s * 2.45, 0, 3.2, 1.1, 4.8, 0.2, "trim")
    b.box(0, -2.8, 0, 4.0 + 1.9, 0.2, 0.1, "stone_dark")
    for k in range(n):
        b.box(0, -2.8 + (k + 0.5) * 5.6 / n, 2.8 / n * (k + 1), 4.0, 0.14, 0.06, "trim")
    return b


@asset("stairs", 200, 2500)
def stairs_wide():
    b = Builder("stairs_wide")
    n = 12
    stairs(b, 0, 0, 0, 8.0, 9.6, 4.8, n, "stone")
    for s in (-1, 1):
        b.frustum(s * 4.6, 0, 0, 1.2, 10.0, 5.4, "sand_light", 1.2, 8.0)
        b.box(s * 4.6, 0, 5.4, 1.5, 8.2, 0.25, "trim")
        b.box(s * 4.6, -4.2, 0, 1.6, 1.4, 1.5, "sand_light")
        b.cyl(s * 4.6, -4.6, 1.5, 0.35, 0.3, 8, "trim")
        b.lathe(s * 4.6, -4.6, 1.8, [(0.3, 0), (0.45, 0.4), (0, 0.9)], 8, "gold")
    for k in range(n):
        b.box(0, -4.8 + (k + 0.5) * 9.6 / n, 4.8 / n * (k + 1), 8.0, 0.14, 0.06, "trim")
    for k in (-2, -1, 0, 1, 2):
        b.cyl(k * 1.6, 4.4, 4.8, 0.2, 0.9, 6, "trim")
    return b


@asset("stairs", 200, 2500)
def stairs_double_landing():
    b = Builder("stairs_double_landing")
    stairs(b, 0, 0, 0, 5.0, 4.0, 2.0, 5, "stone", rot=0.0)
    b.box(0, 4.5, 0, 9.0, 5.0, 2.0, "stone")
    b.box(0, 4.5, 2.0, 9.4, 5.4, 0.2, "trim")
    for s in (-1, 1):
        with b.at(s * 4.4, 4.5, 2.0):
            stairs(b, s * 1.5, 3.6, 0, 3.0, 5.0, 2.0, 6, "stone", rot=0.0)
    b.box(0, 7.4, 2.2, 9.4, 0.3, 0.9, "sand_light")
    for sx in (-1, 1):
        b.box(sx * 4.6, 4.5, 2.2, 0.3, 5.4, 0.9, "sand_light")
    for k in range(5):
        b.box(0, -1.8 + (k + 0.5) * 0.8, 0.4 * (k + 1), 5.0, 0.12, 0.06, "trim")
    return b


@asset("buildings", 2000, 5000)
def mercenary_guild():
    b = Builder("mercenary_guild")
    w, d, h = 13.0, 10.0, 6.6
    block_body(b, w, d, h, wall="sand_dark", band_z=3.5)
    parapet(b, w, d, h + 0.25, 0.4, 0.6, "sand")
    crenels(b, w, d, h + 0.85, 1.4, 0.5, 0.5, "sand")
    y = -d / 2
    b.plate(arch_poly(0, 0.45, 3.4, 4.2, 6), y - 0.12, y + 0.02, "trim")
    b.plate(arch_poly(0, 0.45, 2.9, 3.8, 6), y - 0.2, y + 0.02, "door")
    for s in (-1, 1):
        b.box(s * 0.75, y - 0.22, 0.45, 1.4, 0.05, 3.0, "wood_dark")
        for r in range(3):
            for c in range(2):
                b.box(s * (0.25 + c * 0.9), y - 0.26, 0.9 + r * 0.9, 0.18, 0.06, 0.18, "gold")
    icon(b, "swords", 0, 5.3, y, 1.4)
    for sx in (-1, 1):
        tx, ty = sx * (w / 2 - 0.2), y + 0.2
        tower_round(b, tx, ty, 0, 1.9, h - 0.4, 8, None, "sand_dark", 2)
        b.cyl(tx, ty, h + 0.4, 2.2, 0.3, 8, "trim")
        for k in range(8):
            a = 2 * math.pi * k / 8
            b.box(tx + 2.0 * math.cos(a), ty + 2.0 * math.sin(a), h + 0.7, 0.55, 0.55, 0.5, "sand", a)
        banner(b, sx * 3.6, y - 0.8, 0, 5.4, "cloth_red")
    for sx in (-4.0, 4.0):
        window(b, sx, 1.8, 0.9, 1.5, y)
        window(b, sx, 4.6, 0.9, 1.3, y)
    for side in ("right", "left", "back"):
        rot, ys = wall_frame(side, w, d)
        with b.at(rot=rot):
            for i in range(3):
                window(b, -3.0 + i * 3.0, 4.2, 0.8, 1.3, ys)
    b.cyl(0, 1.0, h + 0.25, 2.2, 1.2, 8, "sand")
    slate_dome(b, 0, 1.0, h + 1.45, 2.3, 2.6)
    stairs(b, 0, y - 1.0, 0, 4.2, 1.6, 0.45, 2)
    return b


@asset("buildings", 2000, 5000)
def arsenal():
    b = Builder("arsenal")
    w, d, h = 15.0, 11.0, 6.0
    block_body(b, w, d, h, wall="sand", band_z=3.0)
    parapet(b, w, d, h + 0.25, 0.4, 0.6)
    crenels(b, w, d, h + 0.85, 1.5, 0.5, 0.5)
    y = -d / 2
    b.plate(arch_poly(0, 0.45, 4.4, 4.8, 6), y - 0.12, y + 0.02, "trim")
    for s in (-1, 1):
        b.plate(arch_poly(s * 0.95, 0.45, 1.9, 4.4, 4), y - 0.2, y + 0.02, "wood_dark")
    stairs(b, 0, y - 2.0, 0, 5.0, 3.6, 0.45, 3)
    for sx in (-5.0, 5.0):
        window(b, sx, 2.0, 1.0, 1.8, y, arched=False)
        window(b, sx, 4.5, 0.9, 1.0, y, arched=False)
    for side in ("right", "left", "back"):
        rot, ys = wall_frame(side, w, d)
        with b.at(rot=rot):
            n = 4 if side == "back" else 3
            span = w if side == "back" else d
            for i in range(n):
                window(b, -span / 2 + span * (i + 0.5) / n, 3.6, 0.8, 1.1, ys)
    for sx in (-1, 1):
        with b.at(sx * 6.6, y - 1.5, 0):
            b.box(0, 0, 0, 3.0, 0.2, 0.15, "wood_dark")
            b.box(0, 0, 1.7, 3.0, 0.14, 0.14, "wood_dark")
            for p in (-1.4, 1.4):
                b.cyl(p, 0, 0, 0.09, 2.0, 4, "wood_dark")
            for i in range(5):
                b.cyl(-1.1 + i * 0.55, 0, 0.1, 0.04, 2.4, 4, "wood")
                b.pyramid(-1.1 + i * 0.55, 0, 2.5, 0.16, 0.16, 0.3, "metal_light")
        barrel(b, sx * 4.2, y - 1.0, 0)
        crate(b, sx * 4.9, y - 1.2, 0, 0.8)
        banner(b, sx * 7.2, y + 0.4, 0, 5.2, "cloth_red")
    for sx in (-1, 1):
        tx = sx * (w / 2 - 0.6)
        tower_round(b, tx, d / 2 - 0.6, 0, 1.7, h - 0.4, 8, None, "sand", 1)
        b.cyl(tx, d / 2 - 0.6, h + 0.4, 2.0, 0.3, 8, "trim")
    b.cyl(0, 1.0, h + 0.25, 2.0, 1.0, 8, "sand_light")
    slate_dome(b, 0, 1.0, h + 1.25, 2.1, 2.2)
    return b
