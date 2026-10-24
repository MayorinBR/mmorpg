using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Builders for stones, boulders and rock formations.</summary>
    public static class EnvRocks
    {
        /// <summary>Builds a single boulder resting on the ground.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="size">Approximate radius.</param>
        /// <param name="flatten">Vertical scale factor.</param>
        /// <param name="roughness">Surface displacement.</param>
        public static void Boulder(EnvMesh m, int sub, int seed, float size, float flatten, float roughness)
        {
            var rng = new Rng(seed);
            var radii = new Vector3(size * rng.Range(0.9f, 1.15f), size * flatten, size * rng.Range(0.8f, 1.05f));
            EnvShapes.GroundedRock(m, sub, Vector3.zero, radii, seed, roughness, 1.6f, 1);
        }

        /// <summary>Builds a group of rocks of decreasing size.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="count">Number of rocks.</param>
        /// <param name="spread">Group radius.</param>
        /// <param name="size">Radius of the largest rock.</param>
        public static void Cluster(EnvMesh m, int sub, int seed, int count, float spread, float size)
        {
            var rng = new Rng(seed);
            EnvShapes.GroundedRock(m, sub, Vector3.zero, new Vector3(size, size * 0.75f, size * 0.9f), seed, 0.24f, 1.6f, 1);
            for (int i = 1; i < count; i++)
            {
                float a = rng.Range(0f, Mathf.PI * 2f);
                float d = spread * rng.Range(0.5f, 1f);
                float s = size * rng.Range(0.25f, 0.6f);
                EnvShapes.GroundedRock(m, sub, new Vector3(Mathf.Cos(a) * d, -s * 0.15f, Mathf.Sin(a) * d), new Vector3(s, s * 0.8f, s * 0.9f), seed + i * 17, 0.25f, 1.6f, 1);
            }
        }

        /// <summary>Builds a tall standing stone.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="height">Stone height.</param>
        /// <param name="width">Stone half width.</param>
        public static void Standing(EnvMesh m, int sub, int seed, float height, float width)
        {
            var rng = new Rng(seed);
            EnvShapes.Rock(m, sub, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height * 0.5f, width * 0.7f), seed, 0.14f, 0.12f, 1.6f, 1, new Vector3(rng.Range(-4f, 4f), rng.Range(0f, 360f), rng.Range(-5f, 5f)));
            EnvShapes.GroundedRock(m, sub, new Vector3(width * 0.8f, 0f, 0f), new Vector3(width * 0.5f, width * 0.4f, width * 0.5f), seed + 3, 0.2f, 1.6f, 1);
        }

        /// <summary>Builds a rock spire cluster.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="height">Height of the tallest spire.</param>
        /// <param name="spires">Spire count.</param>
        public static void Spires(EnvMesh m, int sub, int seed, float height, int spires)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < spires; i++)
            {
                float a = i / (float)spires * Mathf.PI * 2f + rng.Range(-0.4f, 0.4f);
                float d = i == 0 ? 0f : height * rng.Range(0.18f, 0.32f);
                float h = i == 0 ? height : height * rng.Range(0.45f, 0.8f);
                float w = h * rng.Range(0.18f, 0.26f);
                EnvShapes.Rock(m, sub, new Vector3(Mathf.Cos(a) * d, h * 0.5f - 0.2f, Mathf.Sin(a) * d), new Vector3(w, h * 0.5f, w * 0.9f), seed + i * 11, 0.16f, 0.2f, 1.8f, 1, new Vector3(rng.Range(-6f, 6f), rng.Range(0f, 360f), rng.Range(-6f, 6f)));
            }
        }

        /// <summary>Builds a natural rock arch.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="span">Opening width.</param>
        /// <param name="height">Opening height.</param>
        public static void Arch(EnvMesh m, int sub, int seed, float span, float height)
        {
            var rng = new Rng(seed);
            foreach (float side in new[] { -1f, 1f })
            {
                for (int k = 0; k < 3; k++)
                {
                    float y = k * height * 0.38f;
                    float w = span * (0.34f - k * 0.05f);
                    EnvShapes.GroundedRock(m, sub, new Vector3(side * (span * 0.5f + w * 0.3f - k * span * 0.06f), y, rng.Range(-0.2f, 0.2f)), new Vector3(w, height * 0.28f, w * 1.1f), seed + k * 5 + (int)side, 0.2f, 1.8f, 1);
                }
            }

            for (int k = 0; k < 3; k++)
            {
                float x = (k - 1) * span * 0.34f;
                EnvShapes.Rock(m, sub, new Vector3(x, height * 1.02f - Mathf.Abs(k - 1) * height * 0.08f, 0f), new Vector3(span * 0.28f, height * 0.18f, span * 0.3f), seed + 40 + k, 0.2f, 0.1f, 1.8f, 1, new Vector3(0f, rng.Range(0f, 360f), 0f));
            }
        }

        /// <summary>Builds a stack of small stones.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="levels">Stone count.</param>
        /// <param name="baseSize">Radius of the lowest stone.</param>
        public static void Cairn(EnvMesh m, int sub, int seed, int levels, float baseSize)
        {
            var rng = new Rng(seed);
            float y = 0f;
            for (int i = 0; i < levels; i++)
            {
                float s = baseSize * Mathf.Lerp(1f, 0.38f, i / (float)Mathf.Max(1, levels - 1));
                EnvShapes.Rock(m, sub, new Vector3(rng.Range(-0.05f, 0.05f), y + s * 0.45f, rng.Range(-0.05f, 0.05f)), new Vector3(s, s * 0.5f, s * 0.9f), seed + i, 0.14f, 0.2f, 1.2f, 1, new Vector3(0f, rng.Range(0f, 360f), 0f));
                y += s * 0.85f;
            }
        }
    }

    /// <summary>Builders for bushes, grasses, flowers and desert plants.</summary>
    public static class EnvPlants
    {
        /// <summary>Builds a rounded bush from leaf clusters.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="leafSub">Leaf submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radius">Bush radius.</param>
        /// <param name="clusters">Number of clusters.</param>
        public static void Bush(EnvMesh m, int leafSub, int seed, float radius, int clusters)
        {
            var rng = new Rng(seed);
            Vector3 shading = new Vector3(0f, radius * 0.2f, 0f);
            for (int i = 0; i < clusters; i++)
            {
                float a = i / (float)clusters * Mathf.PI * 2f + rng.Range(-0.3f, 0.3f);
                float d = i == 0 ? 0f : radius * rng.Range(0.45f, 0.7f);
                float r = radius * rng.Range(0.5f, 0.65f);
                Vector3 c = new Vector3(Mathf.Cos(a) * d, r * 0.75f, Mathf.Sin(a) * d);
                EnvShapes.LeafCluster(m, leafSub, c, new Vector3(r, r * 0.8f, r), 12, r * 1.1f, r * 1.5f, shading, 0.3f, rng);
            }
        }

        /// <summary>Builds a fern with radial fronds.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="leafSub">Frond submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="fronds">Frond count.</param>
        /// <param name="length">Frond length.</param>
        /// <param name="pitchMin">Lowest pitch in degrees.</param>
        /// <param name="pitchMax">Highest pitch in degrees.</param>
        public static void Fern(EnvMesh m, int leafSub, int seed, int fronds, float length, float pitchMin, float pitchMax)
        {
            var rng = new Rng(seed);
            Vector3 shading = new Vector3(0f, -length * 0.3f, 0f);
            for (int i = 0; i < fronds; i++)
            {
                float yaw = (i / (float)fronds * 360f + rng.Range(-14f, 14f)) * Mathf.Deg2Rad;
                float pitch = rng.Range(pitchMin, pitchMax) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Sin(yaw) * Mathf.Cos(pitch));
                Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                EnvShapes.CurvedCard(m, leafSub, new Vector3(0f, 0.02f, 0f), dir, side, length * rng.Range(0.8f, 1.1f), length * 0.5f, 0.35f, 3, shading, 0.6f);
            }
        }

        /// <summary>Builds a grass patch.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Grass submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radius">Patch radius.</param>
        /// <param name="clumps">Clump count.</param>
        /// <param name="height">Blade height.</param>
        public static void Grass(EnvMesh m, int sub, int seed, float radius, int clumps, float height)
        {
            EnvShapes.Patch(m, sub, Vector3.zero, radius, clumps, height * 0.9f, height, 3, new Rng(seed));
        }

        /// <summary>Builds a flower patch with grass underneath.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="grassSub">Grass submesh.</param>
        /// <param name="flowerSub">Flower submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radius">Patch radius.</param>
        /// <param name="flowers">Flower clump count.</param>
        public static void Flowers(EnvMesh m, int grassSub, int flowerSub, int seed, float radius, int flowers)
        {
            var rng = new Rng(seed);
            EnvShapes.Patch(m, grassSub, Vector3.zero, radius, flowers, 0.5f, 0.45f, 2, rng);
            EnvShapes.Patch(m, flowerSub, Vector3.zero, radius * 0.95f, flowers, 0.42f, 0.5f, 2, rng);
        }

        /// <summary>Builds a saguaro-style cactus with arms.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Cactus submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="height">Trunk height.</param>
        /// <param name="arms">Arm count.</param>
        public static void Saguaro(EnvMesh m, int sub, int seed, float height, int arms)
        {
            var rng = new Rng(seed);
            float r = height * 0.075f;
            EnvShapes.PathTube(m, sub, new[] { Vector3.zero, new Vector3(0f, height * 0.5f, 0f), new Vector3(0f, height * 0.98f, 0f) }, new[] { r * 1.05f, r, r * 0.85f }, 8, height * 0.4f, false, true);
            for (int i = 0; i < arms; i++)
            {
                float a = (i * 180f + rng.Range(-30f, 30f)) * Mathf.Deg2Rad;
                Vector3 outDir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                float y = height * rng.Range(0.3f, 0.55f);
                float reach = height * rng.Range(0.22f, 0.3f);
                float top = y + height * rng.Range(0.22f, 0.34f);
                float ar = r * 0.62f;
                Vector3 p0 = new Vector3(0f, y, 0f);
                Vector3 p1 = outDir * reach + Vector3.up * y;
                Vector3 p2 = outDir * reach + Vector3.up * (y + (top - y) * 0.5f);
                Vector3 p3 = outDir * reach + Vector3.up * top;
                EnvShapes.PathTube(m, sub, new[] { p0, p1 * 0.5f + p0 * 0.5f, p1, p2, p3 }, new[] { ar, ar, ar, ar * 0.95f, ar * 0.8f }, 6, height * 0.4f, false, true);
            }
        }

        /// <summary>Builds a barrel cactus.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Cactus submesh.</param>
        /// <param name="radius">Base radius.</param>
        /// <param name="height">Height.</param>
        public static void BarrelCactus(EnvMesh m, int sub, float radius, float height)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(radius * 0.7f, 0.02f), new Vector2(radius, height * 0.25f), new Vector2(radius * 0.95f, height * 0.6f),
                new Vector2(radius * 0.6f, height * 0.9f), new Vector2(0f, height * 0.95f)
            };
            EnvShapes.Lathe(m, sub, Vector3.zero, profile, 8, 1f, false);
        }

        /// <summary>Builds a paddle (prickly pear) cactus from flattened lobes.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Cactus submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="lobes">Lobe count.</param>
        public static void PaddleCactus(EnvMesh m, int sub, int seed, int lobes)
        {
            var rng = new Rng(seed);
            Vector3 pos = new Vector3(0f, 0.35f, 0f);
            var queue = new List<Vector3> { pos };
            for (int i = 0; i < lobes; i++)
            {
                Vector3 parent = queue[rng.Chance(0.6f) ? queue.Count - 1 : Mathf.FloorToInt(rng.Range(0f, queue.Count - 0.01f))];
                Vector3 next = parent + new Vector3(rng.Range(-0.35f, 0.35f), rng.Range(0.3f, 0.5f), rng.Range(-0.25f, 0.25f));
                queue.Add(next);
                float tilt = rng.Range(-25f, 25f);
                EnvShapes.Rock(m, sub, next, new Vector3(0.34f, 0.42f, 0.09f), seed + i, 0.05f, 0f, 1f, 1, new Vector3(0f, rng.Range(0f, 360f), tilt));
            }
        }

        /// <summary>Builds an agave rosette of upright leaves.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Leaf submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="leaves">Leaf count.</param>
        /// <param name="length">Leaf length.</param>
        public static void Agave(EnvMesh m, int sub, int seed, int leaves, float length)
        {
            Fern(m, sub, seed, leaves, length, 25f, 75f);
        }

        /// <summary>Builds a clump of tall reeds.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Reed submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radius">Clump radius.</param>
        /// <param name="height">Reed height.</param>
        public static void Reeds(EnvMesh m, int sub, int seed, float radius, float height)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < 9; i++)
            {
                float a = rng.Range(0f, Mathf.PI * 2f);
                float d = radius * Mathf.Sqrt(rng.Range(0f, 1f));
                EnvShapes.UprightCard(m, sub, new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d), rng.Range(0f, 180f), height * 0.22f, height * rng.Range(0.75f, 1.1f), rng.Range(-0.2f, 0.2f) * height);
            }
        }
    }

    /// <summary>Builders for man-made props and set dressing.</summary>
    public static class EnvDecor
    {
        private static readonly Quaternion Identity = Quaternion.identity;

        /// <summary>Builds a tree stump with a cut top.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="bark">Bark submesh.</param>
        /// <param name="radius">Stump radius.</param>
        /// <param name="height">Stump height.</param>
        public static void Stump(EnvMesh m, int bark, float radius, float height)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(radius * 1.35f, 0f), new Vector2(radius * 1.1f, height * 0.3f), new Vector2(radius, height * 0.85f),
                new Vector2(radius * 0.92f, height), new Vector2(0f, height * 1.01f)
            };
            EnvShapes.Lathe(m, bark, Vector3.zero, profile, 9, 1.2f, true);
        }

        /// <summary>Builds a fallen log.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="bark">Bark submesh.</param>
        /// <param name="length">Log length.</param>
        /// <param name="radius">Log radius.</param>
        public static void Log(EnvMesh m, int bark, float length, float radius)
        {
            EnvShapes.PathTube(m, bark, new[] { new Vector3(-length * 0.5f, radius, 0f), new Vector3(0f, radius * 1.02f, 0.05f), new Vector3(length * 0.5f, radius, 0f) }, new[] { radius, radius * 0.95f, radius * 0.9f }, 8, 2f, true, true);
        }

        /// <summary>Builds a fence segment with posts and rails.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="length">Segment length.</param>
        /// <param name="height">Fence height.</param>
        public static void Fence(EnvMesh m, int wood, float length, float height)
        {
            foreach (float x in new[] { -length * 0.5f, 0f, length * 0.5f })
            {
                EnvShapes.Box(m, wood, new Vector3(x, height * 0.5f, 0f), new Vector3(0.14f, height, 0.14f), Identity, 1f, false);
            }

            foreach (float y in new[] { height * 0.45f, height * 0.82f })
            {
                EnvShapes.Box(m, wood, new Vector3(0f, y, 0f), new Vector3(length, 0.1f, 0.07f), Identity, 1f, false);
            }
        }

        /// <summary>Builds a signpost with two boards.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        public static void Signpost(EnvMesh m, int wood)
        {
            EnvShapes.Box(m, wood, new Vector3(0f, 1.1f, 0f), new Vector3(0.14f, 2.2f, 0.14f), Identity, 1f, false);
            EnvShapes.Box(m, wood, new Vector3(0.35f, 1.85f, 0.1f), new Vector3(1f, 0.28f, 0.05f), Quaternion.Euler(0f, 0f, 4f), 1f, false);
            EnvShapes.Box(m, wood, new Vector3(-0.3f, 1.4f, 0.1f), new Vector3(0.9f, 0.26f, 0.05f), Quaternion.Euler(0f, 0f, -5f), 1f, false);
        }

        /// <summary>Builds a haystack dome.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="straw">Straw submesh.</param>
        /// <param name="radius">Base radius.</param>
        public static void Haystack(EnvMesh m, int straw, float radius)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(radius, 0f), new Vector2(radius * 1.05f, radius * 0.35f), new Vector2(radius * 0.85f, radius * 0.85f),
                new Vector2(radius * 0.45f, radius * 1.25f), new Vector2(0f, radius * 1.4f)
            };
            EnvShapes.Lathe(m, straw, Vector3.zero, profile, 10, 1.5f, false);
        }

        /// <summary>Builds a stone well with a wooden roof.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Stone submesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="water">Water or dark submesh.</param>
        public static void Well(EnvMesh m, int stone, int wood, int water)
        {
            var ring = new List<Vector2>
            {
                new Vector2(0.75f, 0f), new Vector2(1.05f, 0f), new Vector2(1.05f, 0.95f), new Vector2(0.75f, 0.95f), new Vector2(0.75f, 0.1f)
            };
            EnvShapes.Lathe(m, stone, Vector3.zero, ring, 10, 1.4f, true);
            EnvShapes.Cylinder(m, water, new Vector3(0f, 0.55f, 0f), 0.76f, 0.01f, 10, 1f);
            foreach (float x in new[] { -0.85f, 0.85f })
            {
                EnvShapes.Box(m, wood, new Vector3(x, 1.7f, 0f), new Vector3(0.14f, 1.5f, 0.14f), Identity, 1f, false);
            }

            EnvShapes.Box(m, wood, new Vector3(0f, 1.2f, 0f), new Vector3(0.12f, 0.12f, 1.6f), Quaternion.Euler(0f, 0f, 90f), 1f, false);
            EnvShapes.Box(m, wood, new Vector3(0f, 2.55f, 0f), new Vector3(2.4f, 0.08f, 1.5f), Quaternion.Euler(0f, 0f, 0f), 1.2f, false);
            EnvShapes.Box(m, wood, new Vector3(-0.55f, 2.7f, 0f), new Vector3(1.4f, 0.08f, 1.6f), Quaternion.Euler(0f, 0f, 32f), 1.2f, false);
            EnvShapes.Box(m, wood, new Vector3(0.55f, 2.7f, 0f), new Vector3(1.4f, 0.08f, 1.6f), Quaternion.Euler(0f, 0f, -32f), 1.2f, false);
        }

        /// <summary>Builds a wooden barrel.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="metal">Metal band submesh.</param>
        /// <param name="radius">Barrel radius.</param>
        /// <param name="height">Barrel height.</param>
        public static void Barrel(EnvMesh m, int wood, int metal, float radius, float height)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(radius * 0.8f, 0f), new Vector2(radius, height * 0.5f), new Vector2(radius * 0.8f, height), new Vector2(0f, height)
            };
            EnvShapes.Lathe(m, wood, Vector3.zero, profile, 10, 1f, false);
            foreach (float t in new[] { 0.2f, 0.8f })
            {
                float r = radius * (t < 0.5f ? 0.93f : 0.93f) + 0.01f;
                EnvShapes.Torus(m, metal, new Vector3(0f, height * t, 0f), r, 0.025f, 14, 4, 1f);
            }
        }

        /// <summary>Builds a wooden crate.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="size">Edge length.</param>
        /// <param name="yaw">Yaw in degrees.</param>
        /// <param name="origin">Bottom center position.</param>
        public static void Crate(EnvMesh m, int wood, float size, float yaw, Vector3 origin)
        {
            EnvShapes.Box(m, wood, origin + Vector3.up * (size * 0.5f), Vector3.one * size, Quaternion.Euler(0f, yaw, 0f), 1f, false);
        }

        /// <summary>Builds a small rowboat.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Hull submesh.</param>
        /// <param name="dark">Interior submesh.</param>
        public static void Rowboat(EnvMesh m, int wood, int dark)
        {
            EnvShapes.Box(m, wood, new Vector3(0f, 0.12f, 0f), new Vector3(3.2f, 0.1f, 1.0f), Identity, 1.2f, false);
            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, wood, new Vector3(0f, 0.38f, s * 0.52f), new Vector3(3.4f, 0.5f, 0.07f), Quaternion.Euler(s * 14f, 0f, 0f), 1.2f, false);
            }

            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, wood, new Vector3(s * 1.7f, 0.4f, 0f), new Vector3(0.45f, 0.5f, 0.9f), Quaternion.Euler(0f, 0f, -s * 28f), 1.2f, false);
            }

            EnvShapes.Box(m, dark, new Vector3(0f, 0.2f, 0f), new Vector3(2.9f, 0.04f, 0.8f), Identity, 1f, false);
            EnvShapes.Box(m, wood, new Vector3(0f, 0.42f, 0f), new Vector3(0.12f, 0.06f, 1.1f), Identity, 1f, false);
        }

        /// <summary>Builds a pier section on posts.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="planks">Plank submesh.</param>
        /// <param name="posts">Post submesh.</param>
        /// <param name="length">Section length.</param>
        public static void Pier(EnvMesh m, int planks, int posts, float length)
        {
            EnvShapes.Box(m, planks, new Vector3(0f, 1.0f, 0f), new Vector3(length, 0.14f, 2.4f), Identity, 1.2f, false);
            for (int i = 0; i < 4; i++)
            {
                float x = -length * 0.5f + 0.3f + i * (length - 0.6f) / 3f;
                foreach (float z in new[] { -1.1f, 1.1f })
                {
                    EnvShapes.Tube(m, posts, new Vector3(x, -0.8f, z), new Vector3(x, 1.5f, z), 0.13f, 0.12f, 6, 1f, true);
                }
            }

            foreach (float z in new[] { -1.1f, 1.1f })
            {
                EnvShapes.Box(m, posts, new Vector3(0f, 1.3f, z), new Vector3(length, 0.07f, 0.07f), Identity, 1f, false);
            }
        }

        /// <summary>Builds a mooring buoy.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="cloth">Striped submesh.</param>
        /// <param name="metal">Metal submesh.</param>
        public static void Buoy(EnvMesh m, int cloth, int metal)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(0.3f, 0.12f), new Vector2(0.48f, 0.4f), new Vector2(0.42f, 0.7f), new Vector2(0.2f, 0.95f), new Vector2(0f, 1f)
            };
            EnvShapes.Lathe(m, cloth, Vector3.zero, profile, 8, 1f, false);
            EnvShapes.Tube(m, metal, new Vector3(0f, 0.95f, 0f), new Vector3(0f, 1.5f, 0f), 0.04f, 0.04f, 5, 1f, true);
            EnvShapes.Torus(m, metal, new Vector3(0f, 1.55f, 0f), 0.09f, 0.025f, 10, 4, 1f);
        }

        /// <summary>Builds a fishing net drying rack.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="net">Net submesh.</param>
        public static void NetRack(EnvMesh m, int wood, int net)
        {
            foreach (float x in new[] { -1.4f, 1.4f })
            {
                EnvShapes.Tube(m, wood, new Vector3(x, 0f, 0f), new Vector3(x, 2.4f, 0f), 0.09f, 0.07f, 6, 1f, true);
            }

            EnvShapes.Box(m, wood, new Vector3(0f, 2.3f, 0f), new Vector3(3.0f, 0.08f, 0.08f), Identity, 1f, false);
            EnvShapes.Box(m, net, new Vector3(0f, 1.55f, 0f), new Vector3(2.6f, 1.4f, 0.03f), Identity, 1.5f, false);
        }

        /// <summary>Builds a pottery vase from a lathe profile.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="pottery">Pottery submesh.</param>
        /// <param name="variant">Profile variant between 0 and 2.</param>
        public static void Vase(EnvMesh m, int pottery, int variant)
        {
            Vector2[] profile;
            if (variant == 0)
            {
                profile = new[] { new Vector2(0f, 0f), new Vector2(0.28f, 0f), new Vector2(0.5f, 0.45f), new Vector2(0.4f, 0.85f), new Vector2(0.2f, 1.05f), new Vector2(0.28f, 1.2f), new Vector2(0.2f, 1.2f), new Vector2(0.17f, 1.0f) };
            }
            else if (variant == 1)
            {
                profile = new[] { new Vector2(0f, 0f), new Vector2(0.22f, 0f), new Vector2(0.55f, 0.35f), new Vector2(0.5f, 0.7f), new Vector2(0.24f, 0.95f), new Vector2(0.3f, 1.05f), new Vector2(0.2f, 1.05f) };
            }
            else
            {
                profile = new[] { new Vector2(0f, 0f), new Vector2(0.18f, 0f), new Vector2(0.24f, 0.3f), new Vector2(0.22f, 1.0f), new Vector2(0.3f, 1.25f), new Vector2(0.2f, 1.25f) };
            }

            EnvShapes.Lathe(m, pottery, Vector3.zero, profile, 10, 1f, false);
        }

        /// <summary>Builds a broken column on a base.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Stone submesh.</param>
        /// <param name="height">Column height.</param>
        /// <param name="radius">Column radius.</param>
        /// <param name="broken">Whether the top is broken off.</param>
        public static void Column(EnvMesh m, int stone, float height, float radius, bool broken)
        {
            EnvShapes.Box(m, stone, new Vector3(0f, 0.2f, 0f), new Vector3(radius * 3.2f, 0.4f, radius * 3.2f), Identity, 1.5f, false);
            EnvShapes.Cylinder(m, stone, new Vector3(0f, 0.4f, 0f), radius, height, 8, 1.5f);
            if (!broken)
            {
                EnvShapes.Box(m, stone, new Vector3(0f, 0.4f + height + 0.15f, 0f), new Vector3(radius * 3f, 0.3f, radius * 3f), Identity, 1.5f, false);
            }
            else
            {
                EnvShapes.GroundedRock(m, stone, new Vector3(radius * 2.4f, 0f, radius * 0.5f), new Vector3(radius * 1.1f, radius * 0.9f, radius * 1.1f), 5, 0.2f, 1.5f, 1);
            }
        }

        /// <summary>Builds an obelisk.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Stone submesh.</param>
        /// <param name="height">Shaft height.</param>
        public static void Obelisk(EnvMesh m, int stone, float height)
        {
            var profile = new List<Vector2>
            {
                new Vector2(0f, 0f), new Vector2(1.5f, 0f), new Vector2(1.5f, 0.4f), new Vector2(1.0f, 0.4f), new Vector2(1.0f, 0.6f), new Vector2(0.7f, 0.6f), new Vector2(0.55f, height),
                new Vector2(0f, height + 0.7f)
            };
            EnvShapes.Lathe(m, stone, Vector3.zero, profile, 4, 1.5f, true);
        }

        /// <summary>Builds a nomad tent.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="cloth">Cloth submesh.</param>
        /// <param name="wood">Pole submesh.</param>
        /// <param name="radius">Base radius.</param>
        public static void Tent(EnvMesh m, int cloth, int wood, float radius)
        {
            var profile = new List<Vector2>
            {
                new Vector2(radius, 0f), new Vector2(radius * 0.95f, radius * 0.45f), new Vector2(radius * 0.5f, radius * 0.95f), new Vector2(0.05f, radius * 1.25f)
            };
            EnvShapes.Lathe(m, cloth, Vector3.zero, profile, 6, 2f, true);
            EnvShapes.Tube(m, wood, new Vector3(0f, radius * 1.1f, 0f), new Vector3(0f, radius * 1.7f, 0f), 0.05f, 0.03f, 5, 1f, true);
        }

        /// <summary>Builds a lantern post.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="metal">Metal submesh.</param>
        /// <param name="wood">Wood submesh.</param>
        public static void LampPost(EnvMesh m, int metal, int wood)
        {
            EnvShapes.Tube(m, wood, Vector3.zero, new Vector3(0f, 3f, 0f), 0.1f, 0.07f, 6, 1f, true);
            EnvShapes.Box(m, wood, new Vector3(0.3f, 2.85f, 0f), new Vector3(0.7f, 0.07f, 0.07f), Identity, 1f, false);
            EnvShapes.Box(m, metal, new Vector3(0.6f, 2.55f, 0f), new Vector3(0.3f, 0.42f, 0.3f), Identity, 1f, false);
        }

        /// <summary>Builds an ice crystal cluster.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="ice">Ice submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="count">Crystal count.</param>
        /// <param name="height">Tallest crystal height.</param>
        public static void IceCrystals(EnvMesh m, int ice, int seed, int count, float height)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < count; i++)
            {
                float h = i == 0 ? height : height * rng.Range(0.35f, 0.75f);
                float r = h * 0.17f;
                float a = rng.Range(0f, Mathf.PI * 2f);
                float d = i == 0 ? 0f : height * rng.Range(0.18f, 0.35f);
                Vector3 baseP = new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d);
                var profile = new List<Vector2> { new Vector2(0f, -0.1f), new Vector2(r, 0f), new Vector2(r * 1.05f, h * 0.7f), new Vector2(0f, h) };
                EnvShapes.Lathe(m, ice, baseP, profile, 6, 2f, true);
            }
        }

        /// <summary>Builds a rectangular mine support frame.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="width">Inner width.</param>
        /// <param name="height">Inner height.</param>
        /// <param name="z">Frame depth position.</param>
        public static void TimberFrame(EnvMesh m, int wood, float width, float height, float z)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, wood, new Vector3(s * (width * 0.5f + 0.15f), height * 0.5f, z), new Vector3(0.3f, height, 0.3f), Identity, 1f, false);
            }

            EnvShapes.Box(m, wood, new Vector3(0f, height + 0.15f, z), new Vector3(width + 0.9f, 0.3f, 0.34f), Identity, 1f, false);
        }
    }
}
