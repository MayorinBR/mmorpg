using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Parameters of a layered conifer built from drooping leaf-card fronds.</summary>
    public sealed class ConiferSpec
    {
        /// <summary>Random seed.</summary>
        public int Seed = 1;

        /// <summary>Total height in meters.</summary>
        public float Height = 8f;

        /// <summary>Number of foliage tiers.</summary>
        public int Tiers = 13;

        /// <summary>Crown base radius as a fraction of the height.</summary>
        public float RadiusFactor = 0.32f;

        /// <summary>Minimum tier radius as a fraction of the height.</summary>
        public float MinRadiusFactor = 0.07f;

        /// <summary>Height fraction of the first tier.</summary>
        public float StartY = 0.1f;

        /// <summary>Height fraction of the last tier.</summary>
        public float EndY = 0.88f;

        /// <summary>Downward pitch of the lowest tier in radians.</summary>
        public float Pitch = 0.38f;

        /// <summary>Fronds per tier at the bottom.</summary>
        public int MaxBranches = 9;

        /// <summary>Fronds per tier at the top.</summary>
        public int MinBranches = 6;

        /// <summary>Trunk radius as a fraction of the height.</summary>
        public float TrunkRadiusFactor = 0.032f;

        /// <summary>Top offset that bends the whole tree.</summary>
        public Vector3 Lean = Vector3.zero;

        /// <summary>Submesh index of the trunk.</summary>
        public int TrunkSub = 0;

        /// <summary>Submesh index of the foliage.</summary>
        public int LeafSub = 1;

        /// <summary>Droop of each frond relative to its length.</summary>
        public float Droop = 0.22f;
    }

    /// <summary>Parameters of a rounded broadleaf tree.</summary>
    public sealed class BroadleafSpec
    {
        /// <summary>Random seed.</summary>
        public int Seed = 1;

        /// <summary>Total height in meters.</summary>
        public float Height = 7f;

        /// <summary>Fraction of the height where the trunk ends.</summary>
        public float TrunkFraction = 0.5f;

        /// <summary>Trunk radius as a fraction of the height.</summary>
        public float TrunkRadiusFactor = 0.05f;

        /// <summary>Crown radius as a fraction of the height.</summary>
        public float CrownRadiusFactor = 0.3f;

        /// <summary>Vertical stretch of the crown.</summary>
        public float CrownStretch = 1f;

        /// <summary>Number of leaf clusters.</summary>
        public int Clusters = 6;

        /// <summary>Cards per leaf cluster.</summary>
        public int CardsPerCluster = 16;

        /// <summary>Minimum card size relative to the cluster radius.</summary>
        public float CardMin = 1.25f;

        /// <summary>Maximum card size relative to the cluster radius.</summary>
        public float CardMax = 1.6f;

        /// <summary>Top offset that bends the whole tree.</summary>
        public Vector3 Lean = Vector3.zero;

        /// <summary>Submesh index of the trunk.</summary>
        public int TrunkSub = 0;

        /// <summary>Submesh index of the leaves.</summary>
        public int LeafSub = 1;
    }

    /// <summary>Parameters of a palm tree.</summary>
    public sealed class PalmSpec
    {
        /// <summary>Random seed.</summary>
        public int Seed = 1;

        /// <summary>Trunk height in meters.</summary>
        public float Height = 8f;

        /// <summary>Trunk radius at mid height.</summary>
        public float TrunkRadius = 0.2f;

        /// <summary>Horizontal offset of the trunk top.</summary>
        public Vector3 Lean = new Vector3(1.4f, 0f, 0.4f);

        /// <summary>Number of fronds.</summary>
        public int Fronds = 10;

        /// <summary>Frond length.</summary>
        public float FrondLength = 3.4f;

        /// <summary>Frond width.</summary>
        public float FrondWidth = 1.7f;

        /// <summary>Lowest frond pitch in degrees.</summary>
        public float PitchMin = -15f;

        /// <summary>Highest frond pitch in degrees.</summary>
        public float PitchMax = 55f;

        /// <summary>Frond droop relative to its length.</summary>
        public float Droop = 0.7f;

        /// <summary>Number of coconuts or dates hanging under the crown.</summary>
        public int Nuts = 0;

        /// <summary>Submesh index of the trunk.</summary>
        public int TrunkSub = 0;

        /// <summary>Submesh index of the fronds.</summary>
        public int LeafSub = 1;

        /// <summary>Submesh index of the nuts.</summary>
        public int NutSub = 2;
    }

    /// <summary>Procedural tree builders shared by all environment catalogs.</summary>
    public static class EnvTrees
    {
        /// <summary>Builds a layered conifer.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="s">Conifer parameters.</param>
        public static void Conifer(EnvMesh m, ConiferSpec s)
        {
            var rng = new Rng(s.Seed);
            float h = s.Height;
            EnvShapes.Tube(m, s.TrunkSub, Vector3.zero, new Vector3(0f, h * 0.97f, 0f), h * s.TrunkRadiusFactor, h * 0.006f, 8, h * 0.35f, false);

            for (int i = 0; i < s.Tiers; i++)
            {
                float t = i / (float)(s.Tiers - 1);
                float y = h * Mathf.Lerp(s.StartY, s.EndY, t);
                float radius = h * s.RadiusFactor * Mathf.Pow(1f - t, 0.75f) + h * s.MinRadiusFactor;
                int count = Mathf.RoundToInt(Mathf.Lerp(s.MaxBranches, s.MinBranches, t));
                float pitch = Mathf.Lerp(s.Pitch, 0.04f, t);
                Vector3 shading = new Vector3(0f, y + radius * 0.35f, 0f);
                for (int j = 0; j < count; j++)
                {
                    float yaw = ((j + 0.5f * (i % 2)) / count * 360f + rng.Range(-12f, 12f)) * Mathf.Deg2Rad;
                    float p = pitch + rng.Range(-0.05f, 0.05f);
                    Vector3 dir = new Vector3(Mathf.Cos(yaw) * Mathf.Cos(p), -Mathf.Sin(p), Mathf.Sin(yaw) * Mathf.Cos(p)).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                    float length = radius * rng.Range(1.1f, 1.3f);
                    foreach (float roll in new[] { -24f, 24f })
                    {
                        EnvShapes.CurvedCard(m, s.LeafSub, new Vector3(0f, y, 0f), dir, Quaternion.AngleAxis(roll, dir) * side, length, length * 0.95f, s.Droop, 3, shading, 0.55f);
                    }
                }
            }

            float baseY = h * (s.EndY - 0.04f);
            float crown = h * 0.18f;
            for (int k = 0; k < 3; k++)
            {
                float yaw = k * 60f * Mathf.Deg2Rad;
                EnvShapes.CurvedCard(m, s.LeafSub, new Vector3(0f, baseY, 0f), Vector3.up, new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw)), crown, crown * 0.8f, 0f, 2, new Vector3(0f, baseY, 0f), 0.2f);
            }

            if (s.Lean != Vector3.zero)
            {
                m.Bend(h, s.Lean);
            }
        }

        /// <summary>Builds a rounded broadleaf tree.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="s">Broadleaf parameters.</param>
        public static void Broadleaf(EnvMesh m, BroadleafSpec s)
        {
            var rng = new Rng(s.Seed);
            float h = s.Height;
            float crownR = h * s.CrownRadiusFactor;
            Vector3 trunkTop = new Vector3(rng.Range(-0.15f, 0.15f), h * s.TrunkFraction, rng.Range(-0.15f, 0.15f));
            float r = h * s.TrunkRadiusFactor;
            EnvShapes.PathTube(m, s.TrunkSub, new[] { Vector3.zero, trunkTop * 0.3f + Vector3.up * (h * 0.15f), trunkTop * 0.7f + Vector3.up * (h * 0.33f), trunkTop },
                new[] { r * 1.35f, r, r * 0.82f, r * 0.7f }, 8, h * 0.4f, false, false);

            Vector3 crownCenter = new Vector3(0f, h * (s.TrunkFraction + 0.16f), 0f);
            for (int c = 0; c < s.Clusters; c++)
            {
                Vector3 offset = c == 0
                    ? new Vector3(0f, crownR * 0.55f * s.CrownStretch, 0f)
                    : Quaternion.Euler(0f, c / (float)(s.Clusters - 1) * 360f + rng.Range(-20f, 20f), 0f) * new Vector3(crownR * rng.Range(0.6f, 0.8f), rng.Range(-0.15f, 0.2f) * crownR * s.CrownStretch, 0f);
                Vector3 center = crownCenter + offset;
                float radius = crownR * rng.Range(0.55f, 0.7f);
                Vector3 branchEnd = center - offset.normalized * radius * 0.3f;
                EnvShapes.PathTube(m, s.TrunkSub, new[] { trunkTop, (trunkTop + branchEnd) * 0.5f + Vector3.up * 0.1f, branchEnd }, new[] { r * 0.55f, r * 0.4f, r * 0.2f }, 5, h * 0.4f, false, false);
                EnvShapes.LeafCluster(m, s.LeafSub, center, new Vector3(radius, radius * s.CrownStretch, radius), s.CardsPerCluster, radius * s.CardMin, radius * s.CardMax, crownCenter, 0.25f, rng);
            }

            if (s.Lean != Vector3.zero)
            {
                m.Bend(h, s.Lean);
            }
        }

        /// <summary>Builds a palm tree with a bent trunk and radial fronds.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="s">Palm parameters.</param>
        public static void Palm(EnvMesh m, PalmSpec s)
        {
            var rng = new Rng(s.Seed);
            const int rings = 8;
            var points = new List<Vector3>();
            var radii = new List<float>();
            for (int i = 0; i <= rings; i++)
            {
                float t = i / (float)rings;
                points.Add(new Vector3(s.Lean.x * t * t, s.Height * t, s.Lean.z * t * t));
                radii.Add(s.TrunkRadius * (i == 0 ? 1.5f : Mathf.Lerp(1.15f, 0.8f, t)));
            }

            EnvShapes.PathTube(m, s.TrunkSub, points, radii, 8, s.Height * 0.4f, false, true);
            Vector3 top = points[rings];
            Vector3 shading = top + Vector3.down * 0.4f;
            for (int k = 0; k < s.Fronds; k++)
            {
                float yaw = (k / (float)s.Fronds * 360f + rng.Range(-10f, 10f)) * Mathf.Deg2Rad;
                float pitch = Mathf.Lerp(s.PitchMin, s.PitchMax, rng.Range(0f, 1f)) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(yaw) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Sin(yaw) * Mathf.Cos(pitch)).normalized;
                Vector3 side = Quaternion.AngleAxis(rng.Range(-18f, 18f), dir) * Vector3.Cross(Vector3.up, dir).normalized;
                EnvShapes.CurvedCard(m, s.LeafSub, top, dir, side, s.FrondLength * rng.Range(0.85f, 1.1f), s.FrondWidth, s.Droop, 4, shading, 0.45f);
            }

            for (int n = 0; n < s.Nuts; n++)
            {
                float a = n / (float)Mathf.Max(1, s.Nuts) * Mathf.PI * 2f;
                Vector3 p = top + new Vector3(Mathf.Cos(a) * 0.28f, -0.3f - rng.Range(0f, 0.2f), Mathf.Sin(a) * 0.28f);
                EnvShapes.Rock(m, s.NutSub, p, Vector3.one * 0.17f, s.Seed + n, 0.04f, 0f, 0.4f, 1, Vector3.zero);
            }
        }

        /// <summary>Builds a flat-topped acacia with a forked trunk.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="height">Total height.</param>
        /// <param name="canopyRadius">Canopy radius.</param>
        /// <param name="trunkSub">Trunk submesh.</param>
        /// <param name="leafSub">Leaf submesh.</param>
        public static void Acacia(EnvMesh m, int seed, float height, float canopyRadius, int trunkSub, int leafSub)
        {
            var rng = new Rng(seed);
            float r = height * 0.04f;
            Vector3 fork = new Vector3(rng.Range(-0.3f, 0.3f), height * 0.5f, rng.Range(-0.3f, 0.3f));
            EnvShapes.PathTube(m, trunkSub, new[] { Vector3.zero, fork * 0.4f + Vector3.up * (height * 0.2f), fork }, new[] { r * 1.4f, r * 1.0f, r * 0.8f }, 8, height * 0.4f, false, false);
            int limbs = 3;
            float start = rng.Range(0f, 360f);
            for (int i = 0; i < limbs; i++)
            {
                float a = (start + i * 360f / limbs + rng.Range(-15f, 15f)) * Mathf.Deg2Rad;
                Vector3 end = fork + new Vector3(Mathf.Cos(a) * canopyRadius * 0.62f, height * rng.Range(0.3f, 0.4f), Mathf.Sin(a) * canopyRadius * 0.62f);
                Vector3 mid = fork + (end - fork) * 0.5f + Vector3.up * (height * 0.04f);
                EnvShapes.PathTube(m, trunkSub, new[] { fork, mid, end }, new[] { r * 0.7f, r * 0.5f, r * 0.25f }, 6, height * 0.4f, false, false);
                Vector3 c = end + Vector3.up * (canopyRadius * 0.12f);
                Vector3 radii = new Vector3(canopyRadius * 0.62f, canopyRadius * 0.2f, canopyRadius * 0.62f);
                EnvShapes.LeafCluster(m, leafSub, c, radii, 22, canopyRadius * 0.4f, canopyRadius * 0.6f, c + Vector3.down * (canopyRadius * 0.6f), 0.5f, rng);
            }
        }

        /// <summary>Builds a bare, recursively branching dead tree.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="height">Approximate height.</param>
        /// <param name="depth">Recursion depth.</param>
        /// <param name="trunkSub">Bark submesh.</param>
        /// <param name="twigSub">Twig card submesh, or -1 for none.</param>
        public static void DeadTree(EnvMesh m, int seed, float height, int depth, int trunkSub, int twigSub)
        {
            var rng = new Rng(seed);
            float r = height * 0.04f;
            Vector3 d = (Vector3.up + rng.Unit() * 0.12f).normalized;
            Branch(m, trunkSub, rng, Vector3.zero, d, height * 0.36f, r, depth, twigSub, height * 0.22f);
        }

        private static void Branch(EnvMesh m, int sub, Rng rng, Vector3 start, Vector3 dir, float length, float r0, int depth, int twigSub, float twigSize)
        {
            Vector3 end = start + dir * length;
            Vector3 mid = start + dir * (length * 0.5f) + rng.Unit() * (length * 0.07f);
            float r1 = r0 * 0.62f;
            EnvShapes.PathTube(m, sub, new[] { start, mid, end }, new[] { r0, (r0 + r1) * 0.5f, r1 }, 6, 2f, false, depth == 0);
            if (depth == 0)
            {
                if (twigSub >= 0)
                {
                    EnvShapes.FacingCard(m, twigSub, end + dir * (twigSize * 0.25f), Vector3.Cross(dir, Vector3.up).sqrMagnitude < 0.01f ? Vector3.forward : Vector3.Cross(dir, Vector3.up).normalized, twigSize * rng.Range(0.8f, 1.2f), rng.Range(0f, 360f), end - dir, 0.3f);
                }

                return;
            }

            int kids = depth >= 3 ? 3 : 2;
            for (int k = 0; k < kids; k++)
            {
                Vector3 axis = Vector3.Cross(dir, rng.Unit()).normalized;
                Vector3 next = Quaternion.AngleAxis(rng.Range(25f, 58f), axis) * dir;
                next = (next + Vector3.up * 0.18f).normalized;
                Branch(m, sub, rng, end, next, length * rng.Range(0.62f, 0.8f), r1, depth - 1, twigSub, twigSize);
            }
        }
    }
}
