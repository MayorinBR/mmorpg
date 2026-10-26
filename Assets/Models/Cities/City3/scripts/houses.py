import math

from lib import *


@asset("houses", 500, 1500)
def house_01_small():
    b = Builder("house_01_small")
    w, d, h = 6.0, 6.0, 3.6
    block_body(b, w, d, h)
    y = -d / 2
    door(b, 0, 0.45, 1.3, 2.3, y)
    for s in (-1, 1):
        window(b, s * 2.1, 1.7, 0.8, 1.1, y, shutters=True)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, 0, 1.7, 0.9, 1.1, ys)
    vigas(b, w, d, h - 0.5)
    parapet(b, w, d, h + 0.25, 0.3, 0.55)
    b.box(1.5, 1.2, h + 0.25, 1.4, 1.4, 1.1, "sand_light")
    pot(b, -2.0, -2.2, h + 0.25)
    pot(b, 2.1, -2.2, h + 0.25, 0.28)
    return b


@asset("houses", 500, 1500)
def house_02_dome_corner():
    b = Builder("house_02_dome_corner")
    w, d, h = 7.0, 6.0, 3.8
    block_body(b, w, d, h, band_z=1.9)
    y = -d / 2
    door(b, -1.2, 0.45, 1.3, 2.4, y)
    window(b, 1.6, 1.8, 0.9, 1.2, y, shutters=True)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, -1.0, 1.8, 0.9, 1.2, ys)
        window(b, 1.2, 1.8, 0.9, 1.2, ys)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    b.cyl(-2.0, 1.2, h + 0.25, 1.5, 1.2, 8, "sand")
    slate_dome(b, -2.0, 1.2, h + 1.45, 1.6, 1.6)
    awning(b, -1.2, y, 3.0, 2.0, 1.1, "cloth_cream", "cloth_green")
    return b


@asset("houses", 500, 1500)
def house_03_two_floor_balcony():
    b = Builder("house_03_two_floor_balcony")
    w, d, h = 6.5, 6.0, 6.8
    block_body(b, w, d, h, band_z=3.5)
    y = -d / 2
    door(b, 0, 0.45, 1.3, 2.4, y)
    window(b, -2.1, 1.7, 0.8, 1.2, y, shutters=True)
    window(b, 2.1, 1.7, 0.8, 1.2, y, shutters=True)
    door(b, 0, 3.8, 1.1, 2.2, y, color="window")
    balcony(b, 0, y, 3.7, 4.2)
    for s in (-1, 1):
        window(b, s * 2.4, 4.4, 0.7, 1.2, y)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, 0, 1.7, 0.9, 1.2, ys)
        window(b, 0, 4.4, 0.9, 1.2, ys)
    vigas(b, w, d, h - 0.5)
    parapet(b, w, d, h + 0.25, 0.3, 0.55)
    return b


@asset("houses", 500, 1500)
def house_04_wide_awning():
    b = Builder("house_04_wide_awning")
    w, d, h = 9.0, 6.0, 3.6
    block_body(b, w, d, h)
    y = -d / 2
    door(b, 2.4, 0.45, 1.4, 2.3, y)
    for x in (-2.8, -0.6):
        window(b, x, 1.6, 1.0, 1.3, y, arched=False)
    awning(b, -1.7, y, 3.0, 4.4, 1.3, "cloth_cream", "cloth_blue")
    vigas(b, w, d, h - 0.5)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    crenels(b, w, d, h + 0.75, 1.6, 0.4, 0.35, skip_front=False)
    barrel(b, -4.0, -3.5, 0)
    barrel(b, -3.3, -3.6, 0, 0.35, 0.8)
    crate(b, 4.0, -3.5, 0)
    return b


@asset("houses", 500, 1500)
def house_05_tower_stairs():
    b = Builder("house_05_tower_stairs")
    w, d, h = 5.5, 5.5, 6.4
    block_body(b, w, d, h, band_z=3.3)
    y = -d / 2
    door(b, 0, 0.45, 1.2, 2.3, y)
    window(b, 0, 4.0, 0.9, 1.3, y, shutters=True)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, -1.2, 1.8, 0.8, 1.1, ys)
        window(b, 1.2, 4.0, 0.8, 1.2, ys)
    rot, ys = wall_frame("left", w, d)
    with b.at(rot=rot):
        door(b, 0, 3.5, 1.1, 2.0, ys)
    stairs(b, -w / 2 - 1.0, 0.0, 0, 1.6, 4.2, 3.4, 9, rot=math.pi / 2)
    b.box(-w / 2 - 0.9, 2.2, 3.3, 1.8, 1.6, 0.2, "stone")
    tower_round(b, 0, 0, h, 1.7, 1.4, 8, 2.0)
    return b


@asset("houses", 500, 1500)
def house_06_l_courtyard():
    b = Builder("house_06_l_courtyard")
    w, d, h = 8.0, 5.0, 3.8
    block_body(b, w, d, h)
    quoins(b, w, d, h)
    y = -d / 2
    door(b, 1.5, 0.45, 1.3, 2.4, y)
    window(b, -2.0, 1.7, 0.9, 1.2, y, shutters=True)
    window(b, 3.1, 1.7, 0.9, 1.2, y, shutters=True)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    with b.at(-w / 2 + 2.8, d / 2 + 3.0, 0):
        block_body(b, 5.0, 6.0, 3.2)
        quoins(b, 5.0, 6.0, 3.2)
        rot, ys = wall_frame("right", 5.0, 6.0)
        with b.at(rot=rot):
            window(b, -1.0, 1.6, 0.8, 1.1, ys, shutters=True)
            window(b, 1.2, 1.6, 0.8, 1.1, ys)
        rot, ys = wall_frame("left", 5.0, 6.0)
        with b.at(rot=rot):
            door(b, 0, 0.45, 1.2, 2.2, ys)
            window(b, 1.8, 1.6, 0.8, 1.1, ys)
        parapet(b, 5.0, 6.0, 3.45, 0.3, 0.5)
        b.cyl(0.6, 1.0, 3.45, 0.6, 1.8, 6, "sand_light")
        b.lathe(0.6, 1.0, 5.25, [(0.7, 0), (0.45, 0.25), (0, 0.6)], 6, "slate")
    b.box(2.4, 6.0, 0, 3.2, 0.3, 1.2, "sand_dark")
    for i in range(3):
        b.box(1.2 + i * 1.2, 6.0, 1.2, 0.45, 0.45, 0.25, "sand_light")
    pot(b, 2.0, -1.5, h + 0.25)
    pot(b, 3.5, -3.2, 0)
    barrel(b, 0.0, -3.1, 0)
    return b


@asset("houses", 500, 1500)
def house_07_tiled_hip():
    b = Builder("house_07_tiled_hip")
    w, d, h = 6.5, 6.0, 3.4
    block_body(b, w, d, h, wall="sand_light")
    y = -d / 2
    door(b, 0, 0.45, 1.3, 2.3, y, arched=False, color="wood")
    for s in (-1, 1):
        window(b, s * 2.1, 1.6, 0.8, 1.1, y, arched=False, shutters=True)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, 0, 1.6, 0.9, 1.1, ys, arched=False)
    quoins(b, w, d, h)
    b.frustum(0, 0, h, w + 1.2, d + 1.2, 1.9, "slate", w2=w - d + 0.4, d2=0.4)
    b.box(0, 0, h + 1.85, w - d + 0.6, 0.45, 0.2, "slate_light")
    for sx in (-1, 1):
        b.pyramid(sx * (w - d + 0.4) / 2, 0, h + 1.9, 0.5, 0.5, 0.6, "gold")
    b.box(2.0, 0.5, h + 0.9, 0.7, 0.7, 2.2, "sand")
    b.box(2.0, 0.5, h + 3.0, 0.9, 0.9, 0.25, "trim")
    for sx in (-1, 1):
        b.box(sx * 2.6, -d / 2 - 0.9, 0, 0.25, 0.25, 2.6, "wood_dark")
    b.extrude_x(0, -d / 2, 2.6, 5.4, [(0, 0), (-1.2, -0.55), (-1.2, -0.4), (0, 0.15)], "cloth_cream")
    pot(b, -2.0, -d / 2 - 0.9, 0)
    pot(b, 2.0, -d / 2 - 0.9, 0, 0.3)
    barrel(b, -3.9, -2.2, 0)
    return b


@asset("houses", 500, 1500)
def house_08_domed_arcade():
    b = Builder("house_08_domed_arcade")
    w, d, h = 7.0, 6.5, 6.2
    block_body(b, w, d, h, band_z=3.2)
    y = -d / 2
    door(b, 0, 0.45, 1.4, 2.5, y)
    for s in (-1, 1):
        window(b, s * 2.3, 1.7, 0.8, 1.3, y, shutters=True)
        window(b, s * 2.3, 4.3, 0.8, 1.3, y)
    window(b, 0, 4.3, 1.0, 1.5, y)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, -1.2, 1.8, 0.8, 1.2, ys)
        window(b, 1.2, 1.8, 0.8, 1.2, ys)
        window(b, 0, 4.3, 0.9, 1.4, ys)
    b.cyl(0, 0, h + 0.25, 2.6, 0.9, 8, "sand_light")
    slate_dome(b, 0, 0, h + 1.15, 2.7, 2.6)
    for s in (-1, 1):
        for t in (-1, 1):
            b.cyl(s * 2.9, t * 2.6, h + 0.25, 0.45, 1.3, 6, "trim")
            b.lathe(s * 2.9, t * 2.6, h + 1.55, [(0.55, 0), (0.4, 0.3), (0, 0.9)], 6, "slate")
    return b


@asset("houses", 500, 1500)
def house_09_wind_tower():
    b = Builder("house_09_wind_tower")
    w, d, h = 6.0, 6.0, 3.8
    block_body(b, w, d, h)
    y = -d / 2
    door(b, 0, 0.45, 1.3, 2.4, y)
    for s in (-1, 1):
        window(b, s * 2.1, 1.7, 0.8, 1.2, y)
    rot, ys = wall_frame("right", w, d)
    with b.at(rot=rot):
        window(b, 0, 1.7, 0.9, 1.2, ys)
    parapet(b, w, d, h + 0.25, 0.3, 0.5)
    tx, ty = -1.4, 1.2
    b.box(tx, ty, h + 0.25, 1.8, 1.8, 3.6, "sand_light")
    b.box(tx, ty, h + 3.85, 2.1, 2.1, 0.25, "trim")
    b.pyramid(tx, ty, h + 4.1, 2.1, 2.1, 0.9, "slate")
    for side in ("front", "right", "back", "left"):
        rot, ys = wall_frame(side, 1.8, 1.8)
        with b.at(tx, ty, h + 0.25, rot):
            for dx in (-0.45, 0.45):
                b.plate(rect_poly(dx, 1.4, 0.4, 1.6), ys - 0.06, ys + 0.02, "window")
    b.box(2.0, -1.0, h + 0.25, 1.2, 1.2, 0.9, "sand_light")
    quoins(b, w, d, h)
    awning(b, 0, y, 3.0, 3.0, 1.0, "cloth_cream", "cloth_ochre")
    vigas(b, w, d, h - 0.5)
    pot(b, -2.6, -3.5, 0)
    pot(b, 2.6, -3.5, 0, 0.3)
    return b


@asset("houses", 500, 1500)
def house_10_annex_yard():
    b = Builder("house_10_annex_yard")
    w, d, h = 5.5, 5.5, 3.5
    with b.at(-1.0, 0, 0):
        block_body(b, w, d, h)
        y = -d / 2
        door(b, 0, 0.45, 1.3, 2.3, y)
        window(b, -1.7, 1.7, 0.8, 1.1, y, shutters=True)
        window(b, 1.7, 1.7, 0.8, 1.1, y, shutters=True)
        parapet(b, w, d, h + 0.25, 0.3, 0.5)
        vigas(b, w, d, h - 0.5)
    with b.at(4.2, -0.6, 0):
        block_body(b, 3.2, 4.2, 2.4, plinth=False)
        b.box(0, 0, 2.4, 3.6, 4.6, 0.2, "wood_dark")
        b.box(0, 0, 2.6, 3.6, 4.6, 0.2, "trim")
        door(b, 0, 0.0, 1.2, 1.9, -2.1, arched=False, color="wood")
    b.box(-0.5, -4.0, 0, 8.4, 0.3, 1.5, "sand_dark")
    for i in range(7):
        b.box(-4.4 + i * 1.4, -4.0, 1.5, 0.45, 0.45, 0.25, "sand_light")
    b.box(-0.5, -4.0, 0, 1.6, 0.4, 0.2, "stone")
    pot(b, -3.0, -3.0, 0)
    crate(b, 3.0, -2.2, 0)
    return b
