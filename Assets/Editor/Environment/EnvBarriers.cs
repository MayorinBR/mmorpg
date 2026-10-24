using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Builders for large level-boundary pieces: cliff walls, giant boulders, pits and blockers.</summary>
    public static class EnvBarriers
    {
        private static readonly Quaternion Identity = Quaternion.identity;

        /// <summary>Builds a cliff wall following a polyline from tapered faceted rock columns.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Cliff submesh.</param>
        /// <param name="snow">Snow cap submesh, or -1 for none.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="points">Wall path on the XZ plane.</param>
        /// <param name="height">Wall height.</param>
        /// <param name="thickness">Wall thickness.</param>
        public static void CliffPath(EnvMesh m, int rock, int snow, int seed, IList<Vector3> points, float height, float thickness)
        {
            var rng = new Rng(seed);
            for (int s = 0; s < points.Count - 1; s++)
            {
                Vector3 a = points[s];
                Vector3 b = points[s + 1];
                Vector3 dir = b - a;
                float length = dir.magnitude;
                dir /= length;
                Vector3 normal = Vector3.Cross(Vector3.up, dir);
                float yaw = Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg;
                int columns = Mathf.Max(1, Mathf.RoundToInt(length / Mathf.Max(2.5f, height * 0.45f)));
                float step = length / columns;
                for (int i = 0; i < columns; i++)
                {
                    Vector3 pos = a + dir * ((i + 0.5f) * step) + normal * (rng.Range(-0.1f, 0.1f) * thickness);
                    float hv = height * rng.Range(0.8f, 1.12f);
                    Crag(m, rock, snow, pos, step * rng.Range(1.0f, 1.25f), thickness * rng.Range(0.85f, 1.05f), hv, yaw + rng.Range(-8f, 8f), rng);
                    Vector3 side = dir * (step * 0.5f * (i % 2 == 0 ? 1f : -1f));
                    Vector3 shoulder = pos + side + normal * (rng.Range(-0.2f, 0.2f) * thickness);
                    Crag(m, rock, snow, shoulder, step * 0.75f, thickness * 0.7f, hv * rng.Range(0.5f, 0.75f), yaw + rng.Range(-20f, 20f), rng);
                    Vector3 foot = pos + normal * (thickness * 0.55f) + dir * (rng.Range(-0.4f, 0.4f) * step);
                    EnvShapes.GroundedRock(m, rock, foot, new Vector3(step * 0.28f, hv * 0.1f, step * 0.26f), seed + i * 29 + s, 0.2f, 3f, 1);
                }
            }
        }

        /// <summary>Adds a tapered, leaning, flat-shaded rock column with a peaked top.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="snow">Snow submesh used for the upper tier, or -1 for none.</param>
        /// <param name="baseCenter">Center of the base.</param>
        /// <param name="width">Base width along the local X axis.</param>
        /// <param name="depth">Base depth along the local Z axis.</param>
        /// <param name="height">Column height.</param>
        /// <param name="yawDegrees">Rotation around the vertical axis.</param>
        /// <param name="rng">Random source.</param>
        private static void Crag(EnvMesh m, int rock, int snow, Vector3 baseCenter, float width, float depth, float height, float yawDegrees, Rng rng)
        {
            const int sides = 6;
            const int tiers = 4;
            const float uvScale = 5f;
            Quaternion yaw = Quaternion.Euler(0f, yawDegrees, 0f);
            Vector3 lean = new Vector3(rng.Range(-0.1f, 0.1f), 0f, rng.Range(-0.1f, 0.1f)) * height;
            var rings = new Vector3[tiers + 1][];
            for (int t = 0; t <= tiers; t++)
            {
                float k = t / (float)tiers;
                float taper = Mathf.Lerp(1f, 0.5f, k * k);
                rings[t] = new Vector3[sides];
                for (int s = 0; s < sides; s++)
                {
                    float angle = (s + rng.Range(-0.2f, 0.2f)) / sides * Mathf.PI * 2f;
                    float reach = taper * rng.Range(0.82f, 1.12f);
                    float y = height * k * rng.Range(0.94f, 1.06f);
                    Vector3 local = new Vector3(Mathf.Cos(angle) * width * 0.5f * reach, y, Mathf.Sin(angle) * depth * 0.5f * reach) + lean * (k * k);
                    rings[t][s] = baseCenter + yaw * local;
                }
            }

            Vector3 apex = baseCenter + yaw * (lean + Vector3.up * (height * rng.Range(1.04f, 1.14f)));
            Vector3 hint = baseCenter + Vector3.up * (height * 0.5f);
            for (int t = 0; t < tiers; t++)
            {
                int sub = snow >= 0 && t == tiers - 1 ? snow : rock;
                for (int s = 0; s < sides; s++)
                {
                    int n = (s + 1) % sides;
                    m.AddFlatQuad(sub, rings[t][s], rings[t][n], rings[t + 1][n], rings[t + 1][s], hint, false, uvScale);
                }
            }

            int capSub = snow >= 0 ? snow : rock;
            for (int s = 0; s < sides; s++)
            {
                m.AddFlatTriangle(capSub, rings[tiers][s], rings[tiers][(s + 1) % sides], apex, hint, false, uvScale);
            }
        }

        /// <summary>Returns the points of a straight path centered on the origin.</summary>
        /// <param name="length">Path length.</param>
        /// <returns>Two end points.</returns>
        public static Vector3[] Line(float length)
        {
            return new[] { new Vector3(-length * 0.5f, 0f, 0f), new Vector3(length * 0.5f, 0f, 0f) };
        }

        /// <summary>Returns the points of a right-angle corner path.</summary>
        /// <param name="arm">Length of each arm.</param>
        /// <returns>Three path points.</returns>
        public static Vector3[] Corner(float arm)
        {
            return new[] { new Vector3(-arm, 0f, 0f), Vector3.zero, new Vector3(0f, 0f, -arm) };
        }

        /// <summary>Returns the points of a circular arc path.</summary>
        /// <param name="radius">Arc radius.</param>
        /// <param name="degrees">Arc angle in degrees.</param>
        /// <param name="segments">Segment count.</param>
        /// <returns>Arc points.</returns>
        public static Vector3[] Arc(float radius, float degrees, int segments)
        {
            var pts = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.Deg2Rad * (degrees * i / segments - degrees * 0.5f);
                pts[i] = new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius - radius);
            }

            return pts;
        }

        /// <summary>Builds a giant rounded boulder with smaller rocks at its foot.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="size">Main boulder radius.</param>
        public static void GiantBoulder(EnvMesh m, int rock, int seed, float size)
        {
            var rng = new Rng(seed);
            EnvShapes.GroundedRock(m, rock, Vector3.zero, new Vector3(size, size * 0.85f, size * 0.9f), seed, 0.16f, 3f, 2);
            for (int i = 0; i < 4; i++)
            {
                float a = rng.Range(0f, Mathf.PI * 2f);
                float s = size * rng.Range(0.22f, 0.38f);
                Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (size * rng.Range(0.8f, 1.05f));
                EnvShapes.GroundedRock(m, rock, p, new Vector3(s, s * 0.8f, s), seed + i * 7, 0.2f, 3f, 1);
            }
        }

        /// <summary>Builds a tight row of large boulders blocking a path.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="count">Boulder count.</param>
        /// <param name="size">Average boulder radius.</param>
        public static void BoulderLine(EnvMesh m, int rock, int seed, int count, float size)
        {
            var rng = new Rng(seed);
            float step = size * 1.5f;
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * step;
                float s = size * rng.Range(0.85f, 1.2f);
                EnvShapes.GroundedRock(m, rock, new Vector3(x, 0f, rng.Range(-0.4f, 0.4f)), new Vector3(s, s * rng.Range(0.8f, 1.1f), s * 0.9f), seed + i * 11, 0.18f, 3f, 2);
            }

            for (int i = 0; i < count - 1; i++)
            {
                float x = (i - (count - 1) * 0.5f + 0.5f) * step;
                float s = size * 0.55f;
                EnvShapes.GroundedRock(m, rock, new Vector3(x, size * 0.55f, rng.Range(-0.3f, 0.3f)), new Vector3(s, s * 0.8f, s), seed + 90 + i, 0.18f, 3f, 1);
            }
        }

        /// <summary>Builds a rock slide: a sloped heap of rocks of many sizes.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="width">Heap width.</param>
        /// <param name="height">Heap height.</param>
        public static void RockSlide(EnvMesh m, int rock, int seed, float width, float height)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < 26; i++)
            {
                float x = rng.Range(-0.5f, 0.5f) * width;
                float z = rng.Range(-0.5f, 0.5f) * height * 1.4f;
                float edge = 1f - Mathf.Clamp01(Mathf.Sqrt((x / (width * 0.5f)) * (x / (width * 0.5f)) + (z / (height * 0.7f)) * (z / (height * 0.7f))));
                float y = height * 0.75f * edge * edge;
                float s = Mathf.Lerp(0.35f, height * 0.45f, rng.Range(0f, 1f) * (0.4f + edge));
                EnvShapes.GroundedRock(m, rock, new Vector3(x, y * 0.6f, z), new Vector3(s, s * 0.8f, s * 0.9f), seed + i * 5, 0.22f, 2.5f, 1);
            }
        }

        /// <summary>Builds a straight or curved chasm with rock lips on both sides.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="dark">Void submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="points">Chasm center line.</param>
        /// <param name="width">Chasm width.</param>
        public static void Chasm(EnvMesh m, int rock, int dark, int seed, IList<Vector3> points, float width)
        {
            var rng = new Rng(seed);
            for (int s = 0; s < points.Count - 1; s++)
            {
                Vector3 a = points[s];
                Vector3 b = points[s + 1];
                Vector3 dir = b - a;
                float length = dir.magnitude;
                dir /= length;
                Vector3 normal = Vector3.Cross(Vector3.up, dir);
                float yaw = Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg;
                float taper = Mathf.Sin(Mathf.PI * (s + 0.5f) / (points.Count - 1));
                float w = width * Mathf.Lerp(0.55f, 1f, taper);
                EnvShapes.Box(m, dark, (a + b) * 0.5f + Vector3.up * 0.02f, new Vector3(length + 0.2f, 0.04f, w), Quaternion.Euler(0f, yaw, 0f), 2f, false);
                int rocks = Mathf.Max(2, Mathf.RoundToInt(length / 1.6f));
                for (int i = 0; i < rocks; i++)
                {
                    Vector3 c = a + dir * ((i + 0.5f) * length / rocks);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        float sz = rng.Range(0.6f, 1.1f);
                        Vector3 p = c + normal * (side * (w * 0.5f + sz * 0.5f));
                        EnvShapes.GroundedRock(m, rock, p, new Vector3(sz * 1.3f, sz * rng.Range(0.6f, 1f), sz), seed + s * 31 + i * 3 + (int)side, 0.2f, 2.5f, 1);
                    }
                }
            }

            foreach (int end in new[] { 0, points.Count - 1 })
            {
                float s = width * 0.45f;
                Vector3 outward = (points[end] - points[end == 0 ? 1 : end - 1]).normalized * (end == 0 ? -1f : 1f);
                EnvShapes.GroundedRock(m, rock, points[end] + outward * (s * 0.3f), new Vector3(s, s * 0.9f, s), seed + 700 + end, 0.2f, 2.5f, 1);
            }
        }

        /// <summary>Builds a wooden palisade with sharpened posts.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="length">Wall length.</param>
        /// <param name="height">Wall height.</param>
        public static void Palisade(EnvMesh m, int wood, int seed, float length, float height)
        {
            var rng = new Rng(seed);
            int posts = Mathf.RoundToInt(length / 0.42f);
            for (int i = 0; i < posts; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / posts;
                float h = height * rng.Range(0.88f, 1.08f);
                float z = rng.Range(-0.05f, 0.05f);
                EnvShapes.Tube(m, wood, new Vector3(x, 0f, z), new Vector3(x, h, z), 0.17f, 0.16f, 6, 1.5f, false);
                EnvShapes.Tube(m, wood, new Vector3(x, h, z), new Vector3(x, h + 0.5f, z), 0.16f, 0.01f, 6, 1.5f, false);
            }

            foreach (float y in new[] { height * 0.3f, height * 0.7f })
            {
                EnvShapes.Box(m, wood, new Vector3(0f, y, 0.2f), new Vector3(length, 0.14f, 0.1f), Identity, 1.5f, false);
            }
        }

        /// <summary>Builds a dense thicket wall from leaf clusters.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="leaf">Leaf submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="length">Wall length.</param>
        /// <param name="height">Wall height.</param>
        public static void Thicket(EnvMesh m, int leaf, int seed, float length, float height)
        {
            var rng = new Rng(seed);
            int clusters = Mathf.RoundToInt(length / (height * 0.55f));
            for (int i = 0; i < clusters; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / clusters;
                for (int layer = 0; layer < 2; layer++)
                {
                    float r = height * rng.Range(0.42f, 0.55f);
                    Vector3 c = new Vector3(x + rng.Range(-0.3f, 0.3f), r * (0.8f + layer * 0.9f), rng.Range(-0.3f, 0.3f));
                    EnvShapes.LeafCluster(m, leaf, c, new Vector3(r * 1.1f, r * 0.9f, r), 14, r * 1.1f, r * 1.5f, new Vector3(x, r * 0.2f, 0f), 0.3f, rng);
                }
            }
        }

        /// <summary>Builds a broken stone wall made of blocks.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Block submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="length">Wall length.</param>
        /// <param name="height">Maximum height.</param>
        public static void RuinWall(EnvMesh m, int stone, int seed, float length, float height)
        {
            var rng = new Rng(seed);
            float x = -length * 0.5f;
            int index = 0;
            while (x < length * 0.5f - 0.3f)
            {
                float w = Mathf.Min(rng.Range(0.9f, 1.5f), length * 0.5f - x);
                float edge = Mathf.Abs(x / (length * 0.5f));
                float h = height * Mathf.Lerp(1f, rng.Range(0.35f, 0.7f), edge * edge) * rng.Range(0.8f, 1f);
                EnvShapes.Box(m, stone, new Vector3(x + w * 0.5f, h * 0.5f, rng.Range(-0.05f, 0.05f)), new Vector3(w * 0.98f, h, 0.9f), Quaternion.Euler(0f, rng.Range(-3f, 3f), 0f), 1.5f, false);
                if (rng.Chance(0.5f))
                {
                    EnvShapes.Box(m, stone, new Vector3(x + w * 0.5f, h + 0.1f, 0f), new Vector3(w * 0.6f, 0.2f, 0.7f), Quaternion.Euler(0f, rng.Range(-15f, 15f), rng.Range(-8f, 8f)), 1.5f, false);
                }

                x += w;
                index++;
            }

            for (int i = 0; i < 4; i++)
            {
                float s = rng.Range(0.3f, 0.5f);
                EnvShapes.GroundedRock(m, stone, new Vector3(rng.Range(-0.5f, 0.5f) * length, 0f, rng.Range(0.7f, 1.3f)), new Vector3(s, s * 0.7f, s), seed + i, 0.1f, 1.5f, 1);
            }
        }

        /// <summary>Builds a pile of fallen logs.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="bark">Bark submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="length">Log length.</param>
        public static void LogPile(EnvMesh m, int bark, int seed, float length)
        {
            var rng = new Rng(seed);
            int logs = 6;
            for (int i = 0; i < logs; i++)
            {
                int row = i < 3 ? 0 : i < 5 ? 1 : 2;
                float r = rng.Range(0.3f, 0.42f);
                float yaw = rng.Range(-14f, 14f) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
                float z = (row == 0 ? i - 1f : row == 1 ? (i - 3f) - 0.5f : 0f) * 0.75f;
                float y = r + row * 0.62f;
                Vector3 c = new Vector3(rng.Range(-0.3f, 0.3f), y, z);
                float len = length * rng.Range(0.85f, 1.1f);
                EnvShapes.PathTube(m, bark, new[] { c - dir * (len * 0.5f), c, c + dir * (len * 0.5f) }, new[] { r, r * 1.03f, r * 0.96f }, 8, 2f, true, true);
            }
        }

        /// <summary>Builds a row of crossed-timber tank traps.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="wood">Wood submesh.</param>
        /// <param name="count">Trap count.</param>
        public static void Hedgehogs(EnvMesh m, int wood, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 c = new Vector3((i - (count - 1) * 0.5f) * 2.4f, 1.1f, 0f);
                EnvShapes.Box(m, wood, c, new Vector3(0.22f, 3f, 0.22f), Quaternion.Euler(0f, 0f, 35f), 1f, false);
                EnvShapes.Box(m, wood, c, new Vector3(0.22f, 3f, 0.22f), Quaternion.Euler(35f, 0f, -35f), 1f, false);
                EnvShapes.Box(m, wood, c, new Vector3(0.22f, 3f, 0.22f), Quaternion.Euler(-35f, 0f, 0f), 1f, false);
            }
        }

        /// <summary>Builds a wall of ice crystals over snow.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="ice">Ice submesh.</param>
        /// <param name="snow">Snow submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="length">Wall length.</param>
        /// <param name="height">Tallest crystal height.</param>
        public static void IceWall(EnvMesh m, int ice, int snow, int seed, float length, float height)
        {
            var rng = new Rng(seed);
            int count = Mathf.RoundToInt(length / (height * 0.28f));
            for (int i = 0; i < count; i++)
            {
                float x = -length * 0.5f + (i + 0.5f) * length / count;
                float h = height * rng.Range(0.55f, 1f);
                float r = h * 0.2f;
                var profile = new List<Vector2> { new Vector2(0f, -0.1f), new Vector2(r, 0f), new Vector2(r * 1.05f, h * 0.72f), new Vector2(0f, h) };
                EnvShapes.Lathe(m, ice, new Vector3(x, 0f, rng.Range(-0.4f, 0.4f)), profile, 6, 2f, true);
            }

            for (int i = 0; i < count + 1; i++)
            {
                float x = -length * 0.5f + i * length / count;
                EnvShapes.GroundedRock(m, snow, new Vector3(x, 0f, rng.Range(-0.3f, 0.3f)), new Vector3(height * 0.32f, height * 0.16f, height * 0.3f), seed + i, 0.08f, 2f, 1);
            }
        }

        /// <summary>Builds a smooth rounded stone.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radii">Stone radii.</param>
        public static void SmoothStone(EnvMesh m, int rock, int seed, Vector3 radii)
        {
            EnvShapes.GroundedRock(m, rock, Vector3.zero, radii, seed, 0.05f, 2.5f, 2);
        }

        /// <summary>Builds a scatter of small smooth pebbles.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="count">Pebble count.</param>
        /// <param name="radius">Scatter radius.</param>
        public static void Pebbles(EnvMesh m, int rock, int seed, int count, float radius)
        {
            var rng = new Rng(seed);
            for (int i = 0; i < count; i++)
            {
                float a = rng.Range(0f, Mathf.PI * 2f);
                float d = radius * Mathf.Sqrt(rng.Range(0f, 1f));
                float s = rng.Range(0.12f, 0.3f);
                EnvShapes.GroundedRock(m, rock, new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d), new Vector3(s * rng.Range(1f, 1.4f), s * 0.7f, s), seed + i, 0.05f, 1.5f, 1);
            }
        }
    }
}
