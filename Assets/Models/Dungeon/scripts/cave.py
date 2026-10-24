import math

from lib import *


def rock_wall(b, w=4.0, d=1.8, h=3.6, seed=1, color="rock_a", top_color="rock_c", sides=14, rings=3, jitter=0.18, x=0.0, y=0.0, z0=0.0):
    """Cave wall segment whose end faces stay flat so neighbouring pieces meet without gaps."""
    rnd = random.Random(seed)
    exp = 0.3
    pts, ring_idx = [], []
    for k in range(rings + 1):
        t = k / rings
        zk = h * t + (rnd.uniform(-0.1, 0.1) * h * 0.4 if 0 < k < rings else 0.0)
        idx = []
        for s in range(sides):
            a = 2 * math.pi * (s + 0.5) / sides
            ca, sa = math.cos(a), math.sin(a)
            px = math.copysign(abs(ca) ** exp, ca) * w / 2
            py = math.copysign(abs(sa) ** exp, sa) * d / 2
            narrow = 1.0 - 0.28 * t ** 1.5
            j = 1.0 + rnd.uniform(-jitter, jitter) * (0.5 if k == 0 else 1.0)
            idx.append(len(pts))
            pts.append((px, py * narrow * j, zk))
        ring_idx.append(idx)
    faces = [list(reversed(ring_idx[0])), ring_idx[-1]]
    cols = [color, "rock_b", color, "rock_a"]
    with b.at(x, y, z0):
        for r, (a, c) in enumerate(zip(ring_idx, ring_idx[1:])):
            for s in range(sides):
                s1 = (s + 1) % sides
                b.add(pts, [[a[s], a[s1], c[s1], c[s]]], cols[(r + s) % 4] if r < rings - 1 else color)
        b.add(pts, [ring_idx[-1]], top_color)
        b.add(pts, [list(reversed(ring_idx[0]))], "rock_dark")
    return b


@asset("cave", 30, 600)
def cave_wall_a():
    b = Builder("cave_wall_a")
    rock_wall(b, seed=1)
    return b


@asset("cave", 30, 600)
def cave_wall_b():
    b = Builder("cave_wall_b")
    rock_wall(b, seed=2, h=3.2, d=2.1, color="rock_b", top_color="rock_a")
    b.rock(-1.0, -1.2, 0, 0.8, 0.7, 0.5, "rock_a", 5, sides=6, rings=2)
    return b


@asset("cave", 30, 700)
def cave_wall_c():
    b = Builder("cave_wall_c")
    rock_wall(b, seed=3, h=4.0, d=1.6, color="rock_warm", top_color="rock_c")
    b.rock(1.1, -1.0, 0, 0.6, 0.6, 0.45, "rock_a", 6, sides=6, rings=2)
    return b


@asset("cave", 30, 700)
def cave_wall_mossy():
    b = Builder("cave_wall_mossy")
    rock_wall(b, seed=4, h=3.4, d=1.8, color="rock_a")
    b.box(-0.6, -0.95, 0.0, 1.4, 0.08, 0.5, "moss")
    b.box(0.9, -0.85, 0.0, 0.9, 0.08, 0.35, "moss_dark")
    return b


@asset("cave", 30, 800)
def cave_block():
    b = Builder("cave_block")
    rock_wall(b, w=4.0, d=4.0, h=3.6, seed=7, color="rock_a", top_color="rock_c", sides=16, jitter=0.1)
    return b


@asset("cave", 30, 800)
def cave_wall_crystals():
    b = Builder("cave_wall_crystals")
    rock_wall(b, seed=8, h=3.6, d=1.8, color="rock_b")
    for i, (x, z, lean) in enumerate([(-0.8, 1.2, -0.3), (-0.4, 1.6, 0.2), (0.5, 1.0, 0.0), (0.9, 1.7, 0.3)]):
        b.spike(x, -0.85, z, 0.12, 0.7 + 0.1 * i, 6, "crystal_blue", seed=i, lean=lean)
    return b


@asset("cave", 30, 700)
def stalagmite_a():
    b = Builder("stalagmite_a")
    b.spike(0, 0, 0, 0.7, 2.2, 7, "rock_a", seed=1)
    b.spike(0.7, 0.3, 0, 0.4, 1.1, 6, "rock_c", seed=2, lean=0.1)
    b.spike(-0.5, -0.4, 0, 0.35, 0.8, 6, "rock_b", seed=3)
    return b


@asset("cave", 30, 700)
def stalagmite_b():
    b = Builder("stalagmite_b")
    b.spike(0, 0, 0, 0.55, 3.0, 6, "rock_c", seed=4, lean=0.05)
    b.rock(0.5, 0.3, 0, 0.8, 0.7, 0.5, "rock_a", 9, sides=6, rings=2)
    return b


@asset("cave", 30, 700)
def stalagmite_cluster():
    b = Builder("stalagmite_cluster")
    for i, (x, y, r, h) in enumerate([(0, 0, 0.5, 1.8), (0.8, 0.4, 0.4, 1.2), (-0.7, 0.5, 0.35, 0.9), (0.2, -0.8, 0.45, 1.4), (-0.6, -0.6, 0.3, 0.7)]):
        b.spike(x, y, 0, r, h, 6, ("rock_a", "rock_c", "rock_b", "rock_warm", "rock_a")[i], seed=10 + i, lean=(i - 2) * 0.05)
    return b


@asset("cave", 30, 700)
def stalactite_a():
    b = Builder("stalactite_a")
    b.spike(0, 0, 0, 0.6, 2.4, 7, "rock_c", seed=21, up=False)
    b.spike(0.6, 0.2, 0.9, 0.3, 1.5, 6, "rock_a", seed=22, up=False)
    return b


@asset("cave", 30, 700)
def stalactite_b():
    b = Builder("stalactite_b")
    b.spike(0, 0, 0, 0.45, 1.6, 6, "rock_a", seed=23, up=False)
    b.spike(-0.5, -0.2, 0.4, 0.3, 1.2, 6, "rock_b", seed=24, up=False)
    b.spike(0.45, 0.35, 0.5, 0.25, 1.0, 5, "rock_c", seed=25, up=False)
    return b


@asset("cave", 30, 800)
def cave_column():
    b = Builder("cave_column")
    b.spike(0, 0, 0, 1.0, 1.8, 8, "rock_a", seed=31, lean=0.0)
    b.spike(0, 0, 1.8, 0.95, 2.0, 8, "rock_c", seed=32, up=False)
    b.cyl(0, 0, 1.2, 0.34, 1.4, 8, "rock_b", r2=0.3)
    return b


@asset("cave", 30, 700)
def rock_small_a():
    b = Builder("rock_small_a")
    b.rock(0, 0, 0, 0.9, 0.8, 0.6, "rock_a", 41, sides=7, rings=2)
    return b


@asset("cave", 30, 700)
def rock_small_b():
    b = Builder("rock_small_b")
    b.rock(0, 0, 0, 1.0, 0.9, 0.5, "rock_b", 42, sides=7, rings=2)
    b.rock(0.7, 0.5, 0, 0.5, 0.45, 0.35, "rock_c", 43, sides=6, rings=2)
    return b


@asset("cave", 30, 700)
def rock_large_a():
    b = Builder("rock_large_a")
    b.rock(0, 0, 0, 2.4, 2.0, 1.8, "rock_a", 44, sides=9, rings=3)
    b.rock(1.4, -0.6, 0, 1.0, 0.9, 0.7, "rock_c", 45, sides=7, rings=2)
    return b


@asset("cave", 30, 700)
def rock_large_b():
    b = Builder("rock_large_b")
    b.rock(0, 0, 0, 2.0, 2.6, 2.4, "rock_b", 46, sides=9, rings=3, top=0.45)
    b.rock(-1.0, 0.9, 0, 0.9, 0.8, 0.6, "rock_a", 47, sides=6, rings=2)
    return b


@asset("cave", 30, 800)
def rock_pile():
    b = Builder("rock_pile")
    for i, (x, y, s, c) in enumerate([(0, 0, 1.2, "rock_a"), (0.9, 0.5, 0.8, "rock_c"), (-0.8, 0.4, 0.9, "rock_b"), (0.2, -0.8, 0.7, "rock_warm"), (-0.3, 0.9, 0.5, "rock_a"), (0.5, 0.0, 0.5, "rock_b")]):
        b.rock(x, y, 0, s, s * 0.9, s * 0.7, c, 50 + i, sides=6, rings=2)
    return b


@asset("cave", 30, 700)
def rock_ledge():
    b = Builder("rock_ledge")
    rock_wall(b, w=4.0, d=4.0, h=0.9, seed=60, color="rock_a", top_color="rock_c", sides=16, rings=2, jitter=0.08)
    return b


@asset("cave", 30, 800)
def cave_arch():
    b = Builder("cave_arch")
    rock_wall(b, w=1.4, d=2.4, h=3.6, seed=61, x=-1.8, color="rock_a")
    rock_wall(b, w=1.4, d=2.4, h=3.6, seed=62, x=1.8, color="rock_b")
    top = [(-2.5, 2.6), (-1.0, 2.7), (0.0, 2.55), (1.0, 2.7), (2.5, 2.6), (2.3, 3.7), (0.8, 4.0), (-0.9, 3.95), (-2.3, 3.7)]
    b.plate([(x, z) for x, z in top], -1.1, 1.1, "rock_c")
    b.rock(0, -1.2, 0, 1.0, 0.9, 0.6, "rock_a", 63, sides=6, rings=2)
    return b


def crystals(b, seed, color, count=7, spread=0.9, base="rock_b"):
    rnd = random.Random(seed)
    b.rock(0, 0, 0, spread * 2.0, spread * 1.8, 0.5, base, seed + 100, sides=7, rings=2)
    for i in range(count):
        a = rnd.uniform(0, 2 * math.pi)
        r = rnd.uniform(0, spread * 0.8)
        h = rnd.uniform(0.8, 2.0) * (1.2 if i == 0 else 1.0)
        rad = rnd.uniform(0.14, 0.28)
        b.spike(math.cos(a) * r * (0.2 if i == 0 else 1.0), math.sin(a) * r * (0.2 if i == 0 else 1.0), 0.25, rad, h, 6, color, seed=seed + i, lean=rnd.uniform(-0.25, 0.25))


@asset("cave", 30, 700)
def crystal_cluster_blue():
    b = Builder("crystal_cluster_blue")
    crystals(b, 70, "crystal_blue")
    return b


@asset("cave", 30, 700)
def crystal_cluster_purple():
    b = Builder("crystal_cluster_purple")
    crystals(b, 80, "crystal_purple", count=8, spread=1.0)
    return b


@asset("cave", 30, 700)
def crystal_small():
    b = Builder("crystal_small")
    crystals(b, 90, "crystal_blue", count=4, spread=0.5)
    return b


@asset("cave", 30, 600)
def mushroom_glow():
    b = Builder("mushroom_glow")
    for i, (x, y, r, h) in enumerate([(0, 0, 0.45, 0.8), (0.7, 0.3, 0.3, 0.5), (-0.5, 0.5, 0.25, 0.4), (0.2, -0.6, 0.2, 0.3)]):
        b.cyl(x, y, 0, r * 0.18, h, 5, "bone_dark")
        b.lathe(x, y, h, [(r, 0), (r * 0.85, r * 0.3), (r * 0.4, r * 0.55), (0, r * 0.6)], 7, "slime")
    return b
