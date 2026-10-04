using System;
using System.Collections.Generic;

namespace Project.Character.Animation.EditorTools
{
    /// <summary>
    /// Converts raw face sprites into the layers the toon shader expects. Works on plain RGBA
    /// byte buffers (row 0 at the bottom, like Unity textures) so it has no dependency on the
    /// Unity API.
    /// </summary>
    public static class FaceTextureProcessor
    {
        private const float SolidAlpha = 0.5f;
        private const int MinFilledArea = 350;
        private const int OpenEyeReach = 10;
        private const int MinOpenEyeMass = 100;
        private const int MinOpenEyeHeight = 14;
        private const float MaxOpenEyeAspect = 1.6f;
        private const int BleedIterations = 8;

        /// <summary>
        /// Splits an eye sprite into a color layer (white sclera, black line art, grayscale iris)
        /// and an iris mask layer. The mask is 255 where the iris takes the chosen color.
        /// </summary>
        /// <param name="rgba">Source pixels, 4 bytes each.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="color">Output RGBA color layer.</param>
        /// <param name="irisMask">Output single-channel iris mask, one byte per pixel.</param>
        public static void ProcessEye(byte[] rgba, int width, int height, out byte[] color, out byte[] irisMask)
        {
            int count = width * height;
            var alpha = new float[count];
            var solid = new bool[count];
            for (int i = 0; i < count; i++)
            {
                alpha[i] = rgba[i * 4 + 3] / 255f;
                solid[i] = alpha[i] > SolidAlpha;
            }

            bool[] inside = FindEyeInterior(solid, width, height);
            bool[] core = Erode(inside, width, height, 1);

            var irisWeight = new float[count];
            for (int i = 0; i < count; i++)
            {
                irisWeight[i] = core[i] && alpha[i] < 0.97f ? Clamp01(alpha[i] / 0.15f) : 0f;
            }

            irisWeight = Blur(irisWeight, width, height);
            for (int i = 0; i < count; i++)
            {
                if (!core[i])
                {
                    irisWeight[i] = 0f;
                }
            }

            var rgb = new float[count * 3];
            var coverage = new float[count];
            for (int i = 0; i < count; i++)
            {
                float a = alpha[i];
                float lineAmount = Clamp01((a - 0.6f) / 0.4f);
                float shade = irisWeight[i] > 0.01f ? Clamp01(1f - 0.7f * a) : 1f;
                for (int c = 0; c < 3; c++)
                {
                    float line = a > 0.3f ? rgba[i * 4 + c] / 255f : 0.05f;
                    rgb[i * 3 + c] = inside[i] ? shade * (1f - lineAmount) + line * lineAmount : line;
                }

                coverage[i] = inside[i] ? 1f : a;
            }

            color = Pack(rgb, coverage, width, height);
            irisMask = new byte[count];
            for (int i = 0; i < count; i++)
            {
                irisMask[i] = (byte)(irisWeight[i] * 255f + 0.5f);
            }
        }

        /// <summary>
        /// Prepares a mouth sprite: keeps its colors and spreads them into transparent pixels so
        /// texture filtering never blends in a dark fringe.
        /// </summary>
        /// <param name="rgba">Source pixels, 4 bytes each.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>The prepared RGBA pixels.</returns>
        public static byte[] ProcessMouth(byte[] rgba, int width, int height)
        {
            int count = width * height;
            var rgb = new float[count * 3];
            var coverage = new float[count];
            for (int i = 0; i < count; i++)
            {
                for (int c = 0; c < 3; c++)
                {
                    rgb[i * 3 + c] = rgba[i * 4 + c] / 255f;
                }

                coverage[i] = rgba[i * 4 + 3] / 255f;
            }

            return Pack(rgb, coverage, width, height);
        }

        private static bool[] FindEyeInterior(bool[] solid, int w, int h)
        {
            int count = w * h;
            bool[] closed = Close(solid, w, h, 2);
            int[] labels = Label(closed, w, h, out int n);
            var inside = new bool[count];

            var minXs = new int[n + 1]; var maxXs = new int[n + 1];
            var minYs = new int[n + 1]; var maxYs = new int[n + 1];
            var masses = new int[n + 1];
            for (int k = 1; k <= n; k++)
            {
                minXs[k] = w; maxXs[k] = -1; minYs[k] = h; maxYs[k] = -1;
            }

            for (int i = 0; i < count; i++)
            {
                int k = labels[i];
                if (k == 0)
                {
                    continue;
                }

                int x = i % w, y = i / w;
                masses[k]++;
                minXs[k] = Math.Min(minXs[k], x); maxXs[k] = Math.Max(maxXs[k], x);
                minYs[k] = Math.Min(minYs[k], y); maxYs[k] = Math.Max(maxYs[k], y);
            }

            for (int k = 1; k <= n; k++)
            {
                var component = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    component[i] = labels[i] == k;
                }

                bool[] filled = FillHoles(component, w, h);
                if (Count(filled) >= MinFilledArea)
                {
                    Or(inside, filled);
                    continue;
                }

                int boxW = maxXs[k] - minXs[k] + 1, boxH = maxYs[k] - minYs[k] + 1;
                bool eyeShaped = masses[k] >= MinOpenEyeMass && boxH >= MinOpenEyeHeight && (float)boxW / boxH <= MaxOpenEyeAspect;
                if (!eyeShaped)
                {
                    continue;
                }

                // Open eye outline: wrap it, together with the pieces that touch its
                // neighbourhood, in a convex shape.
                var points = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    int other = labels[i];
                    if (other == 0)
                    {
                        continue;
                    }

                    bool near = maxXs[other] >= minXs[k] - OpenEyeReach && minXs[other] <= maxXs[k] + OpenEyeReach
                        && maxYs[other] >= minYs[k] - OpenEyeReach && minYs[other] <= maxYs[k] + OpenEyeReach;
                    if (near)
                    {
                        points.Add(i);
                    }
                }

                Or(inside, ConvexHullMask(points, w, h));
            }

            return inside;
        }

        private static bool[] ConvexHullMask(List<int> pixels, int w, int h)
        {
            var pts = new List<long[]>();
            foreach (int i in pixels)
            {
                pts.Add(new[] { (long)(i % w), (long)(i / w) });
            }

            pts.Sort((a, b) => a[0] != b[0] ? a[0].CompareTo(b[0]) : a[1].CompareTo(b[1]));
            var hull = new List<long[]>();
            for (int pass = 0; pass < 2; pass++)
            {
                int start = hull.Count;
                for (int i = 0; i < pts.Count; i++)
                {
                    while (hull.Count >= start + 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], pts[i]) <= 0)
                    {
                        hull.RemoveAt(hull.Count - 1);
                    }

                    hull.Add(pts[i]);
                }

                hull.RemoveAt(hull.Count - 1);
                pts.Reverse();
            }

            var mask = new bool[w * h];
            if (hull.Count < 3)
            {
                return mask;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool insideHull = true;
                    for (int e = 0; e < hull.Count && insideHull; e++)
                    {
                        long[] a = hull[e], b = hull[(e + 1) % hull.Count];
                        insideHull = (b[0] - a[0]) * (y - a[1]) - (b[1] - a[1]) * (x - a[0]) >= 0;
                    }

                    mask[y * w + x] = insideHull;
                }
            }

            return mask;
        }

        private static long Cross(long[] o, long[] a, long[] b)
        {
            return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0]);
        }

        private static byte[] Pack(float[] rgb, float[] coverage, int w, int h)
        {
            int count = w * h;
            var filled = new bool[count];
            for (int i = 0; i < count; i++)
            {
                filled[i] = coverage[i] >= 0.01f;
            }

            for (int pass = 0; pass < BleedIterations; pass++)
            {
                var next = (bool[])filled.Clone();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        if (filled[i])
                        {
                            continue;
                        }

                        float r = 0, g = 0, b = 0;
                        int n = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= w || ny >= h || !filled[ny * w + nx])
                                {
                                    continue;
                                }

                                int j = ny * w + nx;
                                r += rgb[j * 3]; g += rgb[j * 3 + 1]; b += rgb[j * 3 + 2];
                                n++;
                            }
                        }

                        if (n > 0)
                        {
                            rgb[i * 3] = r / n; rgb[i * 3 + 1] = g / n; rgb[i * 3 + 2] = b / n;
                            next[i] = true;
                        }
                    }
                }

                filled = next;
            }

            var result = new byte[count * 4];
            for (int i = 0; i < count; i++)
            {
                for (int c = 0; c < 3; c++)
                {
                    result[i * 4 + c] = (byte)(Clamp01(rgb[i * 3 + c]) * 255f + 0.5f);
                }

                result[i * 4 + 3] = (byte)(Clamp01(coverage[i]) * 255f + 0.5f);
            }

            return result;
        }

        private static float Clamp01(float v)
        {
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }

        private static int Count(bool[] mask)
        {
            int n = 0;
            foreach (bool b in mask)
            {
                if (b)
                {
                    n++;
                }
            }

            return n;
        }

        private static void Or(bool[] target, bool[] other)
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i] |= other[i];
            }
        }

        private static bool[] Dilate(bool[] src, int w, int h, int iterations)
        {
            bool[] current = src;
            for (int it = 0; it < iterations; it++)
            {
                var next = new bool[src.Length];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        next[i] = current[i]
                            || (x > 0 && current[i - 1]) || (x < w - 1 && current[i + 1])
                            || (y > 0 && current[i - w]) || (y < h - 1 && current[i + w]);
                    }
                }

                current = next;
            }

            return current;
        }

        private static bool[] Erode(bool[] src, int w, int h, int iterations)
        {
            bool[] current = src;
            for (int it = 0; it < iterations; it++)
            {
                var next = new bool[src.Length];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        next[i] = current[i]
                            && x > 0 && current[i - 1] && x < w - 1 && current[i + 1]
                            && y > 0 && current[i - w] && y < h - 1 && current[i + w];
                    }
                }

                current = next;
            }

            return current;
        }

        private static bool[] Close(bool[] src, int w, int h, int iterations)
        {
            return Erode(Dilate(src, w, h, iterations), w, h, iterations);
        }

        private static int[] Label(bool[] mask, int w, int h, out int count)
        {
            var labels = new int[mask.Length];
            count = 0;
            var stack = new Stack<int>();
            for (int start = 0; start < mask.Length; start++)
            {
                if (!mask[start] || labels[start] != 0)
                {
                    continue;
                }

                count++;
                labels[start] = count;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    int i = stack.Pop();
                    int x = i % w, y = i / w;
                    TryVisit(mask, labels, stack, count, x - 1, y, w, h);
                    TryVisit(mask, labels, stack, count, x + 1, y, w, h);
                    TryVisit(mask, labels, stack, count, x, y - 1, w, h);
                    TryVisit(mask, labels, stack, count, x, y + 1, w, h);
                }
            }

            return labels;
        }

        private static void TryVisit(bool[] mask, int[] labels, Stack<int> stack, int label, int x, int y, int w, int h)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            int i = y * w + x;
            if (mask[i] && labels[i] == 0)
            {
                labels[i] = label;
                stack.Push(i);
            }
        }

        private static bool[] FillHoles(bool[] mask, int w, int h)
        {
            var outside = new bool[mask.Length];
            var stack = new Stack<int>();
            for (int x = 0; x < w; x++)
            {
                Seed(mask, outside, stack, x, 0, w);
                Seed(mask, outside, stack, x, h - 1, w);
            }

            for (int y = 0; y < h; y++)
            {
                Seed(mask, outside, stack, 0, y, w);
                Seed(mask, outside, stack, w - 1, y, w);
            }

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                int x = i % w, y = i / w;
                if (x > 0) { Seed(mask, outside, stack, x - 1, y, w); }
                if (x < w - 1) { Seed(mask, outside, stack, x + 1, y, w); }
                if (y > 0) { Seed(mask, outside, stack, x, y - 1, w); }
                if (y < h - 1) { Seed(mask, outside, stack, x, y + 1, w); }
            }

            var filled = new bool[mask.Length];
            for (int i = 0; i < mask.Length; i++)
            {
                filled[i] = !outside[i];
            }

            return filled;
        }

        private static void Seed(bool[] mask, bool[] outside, Stack<int> stack, int x, int y, int w)
        {
            int i = y * w + x;
            if (!mask[i] && !outside[i])
            {
                outside[i] = true;
                stack.Push(i);
            }
        }

        private static float[] Blur(float[] src, int w, int h)
        {
            float[] kernel = { 0.0545f, 0.2442f, 0.4026f, 0.2442f, 0.0545f };
            var tmp = new float[src.Length];
            var dst = new float[src.Length];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float sum = 0;
                    for (int k = -2; k <= 2; k++)
                    {
                        sum += src[y * w + Math.Min(w - 1, Math.Max(0, x + k))] * kernel[k + 2];
                    }

                    tmp[y * w + x] = sum;
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float sum = 0;
                    for (int k = -2; k <= 2; k++)
                    {
                        sum += tmp[Math.Min(h - 1, Math.Max(0, y + k)) * w + x] * kernel[k + 2];
                    }

                    dst[y * w + x] = sum;
                }
            }

            return dst;
        }
    }
}
