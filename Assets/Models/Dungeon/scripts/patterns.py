"""Generates the eight tileable grayscale surface patterns used by the DungeonPainted shader.

Slice order: 0 smooth stone, 1 bricks, 2 planks, 3 flagstone, 4 cloth, 5 metal, 6 rough rock, 7 flat.
"""
import os
import sys

import numpy as np
from PIL import Image

SIZE = 512
NAMES = ["0_smooth", "1_bricks", "2_planks", "3_flagstone", "4_cloth", "5_metal", "6_rock", "7_flat"]


def periodic_noise(rnd, power=1.6, stretch=(1.0, 1.0), size=SIZE):
    """Tileable noise: white noise shaped by a 1/f^power spectrum, normalised to 0..1."""
    white = rnd.standard_normal((size, size))
    fy = np.fft.fftfreq(size)[:, None] * stretch[0]
    fx = np.fft.fftfreq(size)[None, :] * stretch[1]
    radius = np.sqrt(fx * fx + fy * fy)
    radius[0, 0] = 1.0
    spectrum = np.fft.fft2(white) / radius ** power
    spectrum[0, 0] = 0.0
    out = np.real(np.fft.ifft2(spectrum))
    out -= out.min()
    return out / out.max()


def fbm(rnd, octaves=((1.2, 1.0), (1.6, 0.5), (2.0, 0.25))):
    total = np.zeros((SIZE, SIZE))
    weight = 0.0
    for power, w in octaves:
        total += periodic_noise(rnd, power) * w
        weight += w
    return total / weight


def speckle(rnd, strength=0.05):
    return (rnd.standard_normal((SIZE, SIZE)) * strength)


def wrap_voronoi(rnd, count):
    """Returns (F1, F2, cell id) for a tileable voronoi diagram, distances in pixels."""
    pts = rnd.uniform(0, SIZE, (count, 2))
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    best = np.full((SIZE, SIZE), 1e9)
    second = np.full((SIZE, SIZE), 1e9)
    ident = np.zeros((SIZE, SIZE), dtype=int)
    for i, (py, px) in enumerate(pts):
        dy = np.abs(yy - py)
        dx = np.abs(xx - px)
        dy = np.minimum(dy, SIZE - dy)
        dx = np.minimum(dx, SIZE - dx)
        d = np.sqrt(dx * dx + dy * dy)
        closer = d < best
        second = np.where(closer, best, np.minimum(second, d))
        ident = np.where(closer, i, ident)
        best = np.where(closer, d, best)
    return best, second, ident


def smooth_step(x, a, b):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def finish(img, mean=0.86):
    img = np.clip(img, 0.0, 1.0)
    return img


def smooth_stone(rnd):
    base = 0.88 + (fbm(rnd) - 0.5) * 0.22
    stains = smooth_step(fbm(rnd, ((1.4, 1.0), (2.0, 0.6))), 0.62, 0.85)
    cracks_n = periodic_noise(rnd, 1.1)
    cracks = 1.0 - smooth_step(np.abs(cracks_n - 0.5), 0.004, 0.012) * 0 - (1.0 - smooth_step(np.abs(cracks_n - 0.5), 0.0, 0.006)) * 0.25
    return finish(base - stains * 0.12 + speckle(rnd, 0.025)) * np.clip(cracks, 0.7, 1.0)


def bricks(rnd):
    rows, cols = 8, 4
    bh, bw = SIZE // rows, SIZE // cols
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    warp = (periodic_noise(rnd, 1.4) - 0.5) * 7
    warp2 = (periodic_noise(rnd, 1.4) - 0.5) * 7
    y = (yy + warp) % SIZE
    row = (y // bh).astype(int)
    xs = (xx + warp2 + (row % 2) * (bw // 2)) % SIZE
    col = (xs // bw).astype(int)
    ly = y - row * bh
    lx = xs - col * bw
    edge = np.minimum(np.minimum(ly, bh - 1 - ly), np.minimum(lx, bw - 1 - lx))
    mortar = 1.0 - smooth_step(edge + (periodic_noise(rnd, 1.2) - 0.5) * 5, 1.5, 5.0)
    tone = rnd.uniform(0.78, 1.0, (rows, cols))
    brick_tone = tone[row % rows, col % cols]
    grain = fbm(rnd, ((1.1, 1.0), (1.8, 0.7))) - 0.5
    chips = smooth_step(periodic_noise(rnd, 1.0), 0.78, 0.9)
    erosion = 1.0 - smooth_step(edge + (periodic_noise(rnd, 1.2) - 0.5) * 8, 4.0, 12.0)
    img = brick_tone + grain * 0.22 - erosion * (0.12 + 0.1 * chips) + speckle(rnd, 0.035)
    img = img * (1.0 - mortar * 0.5)
    crack_n = periodic_noise(rnd, 1.05)
    img -= (1.0 - smooth_step(np.abs(crack_n - 0.5), 0.0, 0.007)) * 0.3
    return finish(img)


def planks(rnd):
    count = 8
    pw = SIZE // count
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    warp = (periodic_noise(rnd, 1.5) - 0.5) * 6
    xw = (xx + warp) % SIZE
    plank = (xw // pw).astype(int)
    lx = xw - plank * pw
    gap = 1.0 - smooth_step(np.minimum(lx, pw - 1 - lx), 1.5, 4.5)
    tone = rnd.uniform(0.70, 0.88, count)[plank]
    grain = periodic_noise(rnd, 1.5, stretch=(16.0, 1.0)) - 0.5
    fine = periodic_noise(rnd, 1.0, stretch=(24.0, 1.0)) - 0.5
    img = tone + grain * 0.3 + fine * 0.1 + speckle(rnd, 0.02)
    for _ in range(5):
        cx, cy = rnd.integers(0, SIZE, 2)
        d = np.sqrt(np.minimum(np.abs(xx - cx), SIZE - np.abs(xx - cx)) ** 2 + (np.minimum(np.abs(yy - cy), SIZE - np.abs(yy - cy)) * 0.45) ** 2)
        img -= (1.0 - smooth_step(d, 3, 12)) * 0.28
    img *= 1.0 - gap * 0.55
    seams = np.zeros_like(img)
    for k in range(3):
        sy = rnd.integers(0, SIZE)
        seams += (1.0 - smooth_step(np.abs((yy - sy + SIZE // 2) % SIZE - SIZE // 2), 0.0, 2.0)) * (rnd.random((1, count))[0][plank] > 0.5)
    return finish(img - np.clip(seams, 0, 1) * 0.25)


def flagstone(rnd):
    f1, f2, ident = wrap_voronoi(rnd, 28)
    edge = (f2 - f1)
    wob = (periodic_noise(rnd, 1.3) - 0.5) * 5
    seam = 1.0 - smooth_step(edge + wob, 1.5, 5.0)
    tone = rnd.uniform(0.80, 1.0, 28)[ident]
    grain = fbm(rnd, ((1.1, 1.0), (1.8, 0.8))) - 0.5
    wear = smooth_step(f1, 0, 60) * 0.0
    erosion = 1.0 - smooth_step(edge + wob, 3.0, 10.0)
    img = tone + grain * 0.2 - erosion * 0.08 + speckle(rnd, 0.035)
    img *= 1.0 - seam * 0.55
    img -= (1.0 - smooth_step(np.abs(periodic_noise(rnd, 1.05) - 0.5), 0.0, 0.006)) * 0.25
    img = np.where(seam < 0.2, np.maximum(img, 0.62), img)
    return finish(img + wear)


def cloth(rnd):
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    weave = 0.5 + 0.5 * np.sin(xx * 2 * np.pi / 8) * np.sin(yy * 2 * np.pi / 8)
    folds = periodic_noise(rnd, 1.6, stretch=(0.5, 1.0))
    tatter = smooth_step(fbm(rnd), 0.62, 0.8)
    img = 0.88 + (weave - 0.5) * 0.08 + (folds - 0.5) * 0.18 - tatter * 0.12 + speckle(rnd, 0.02)
    return finish(img)


def metal(rnd):
    streak = periodic_noise(rnd, 1.2, stretch=(1.0, 16.0)) - 0.5
    blot = periodic_noise(rnd, 1.7)
    rust = smooth_step(blot, 0.55, 0.78)
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    scratches = np.zeros((SIZE, SIZE))
    for _ in range(26):
        y0 = rnd.integers(0, SIZE)
        x0 = rnd.integers(0, SIZE)
        length = rnd.integers(40, 160)
        slope = rnd.uniform(-0.3, 0.3)
        t = np.arange(length)
        py = (y0 + t * slope).astype(int) % SIZE
        px = (x0 + t) % SIZE
        scratches[py, px] = 1.0
    img = 0.88 + streak * 0.14 - rust * 0.22 - scratches * 0.14 + speckle(rnd, 0.02)
    return finish(img)


def rock(rnd):
    base = fbm(rnd, ((1.0, 1.0), (1.5, 0.7), (2.1, 0.4)))
    f1, f2, ident = wrap_voronoi(rnd, 30)
    facets = rnd.uniform(0.82, 1.0, 30)[ident]
    seam = 1.0 - smooth_step((f2 - f1) + (periodic_noise(rnd, 1.3) - 0.5) * 8, 1.0, 5.0)
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    strata = 0.5 + 0.5 * np.sin((yy + xx * 0.25 + (periodic_noise(rnd, 1.4) - 0.5) * 60) * 2 * np.pi / 64)
    img = 0.84 + (base - 0.5) * 0.35 + (facets - 0.91) * 0.5 + (strata - 0.5) * 0.08 + speckle(rnd, 0.04)
    img -= seam * 0.3
    img -= (1.0 - smooth_step(np.abs(periodic_noise(rnd, 1.05) - 0.5), 0.0, 0.006)) * 0.3
    return finish(img)


def flat(rnd):
    return finish(0.92 + (fbm(rnd) - 0.5) * 0.08 + speckle(rnd, 0.02))


GENERATORS = [smooth_stone, bricks, planks, flagstone, cloth, metal, rock, flat]


def generate(out_dir):
    os.makedirs(out_dir, exist_ok=True)
    images = []
    for i, (name, fn) in enumerate(zip(NAMES, GENERATORS)):
        rnd = np.random.default_rng(1000 + i)
        img = fn(rnd)
        images.append(img)
        Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8), "L").convert("RGB").save(os.path.join(out_dir, name + ".png"))
    return images


if __name__ == "__main__":
    out = sys.argv[1]
    imgs = generate(out)
    sheet = np.zeros((SIZE * 2, SIZE * 4))
    for i, img in enumerate(imgs):
        sheet[(i // 4) * SIZE:(i // 4 + 1) * SIZE, (i % 4) * SIZE:(i % 4 + 1) * SIZE] = img
    Image.fromarray((sheet * 255).astype(np.uint8), "L").resize((1600, 800)).save(os.path.join(out, "_sheet.png"))
    for n, im in zip(NAMES, imgs):
        print(n, round(float(im.mean()), 3), round(float(im.std()), 3))
