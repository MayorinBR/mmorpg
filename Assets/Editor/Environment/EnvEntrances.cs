using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Builders for cave and dungeon entrances. All entrances face +Z and rest on y = 0.</summary>
    public static class EnvEntrances
    {
        private static readonly Quaternion Identity = Quaternion.identity;

        /// <summary>Builds a natural cave mouth framed by piled rocks.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="dark">Interior submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="width">Opening width.</param>
        /// <param name="height">Opening height.</param>
        /// <param name="depth">Tunnel depth.</param>
        public static void CaveMouth(EnvMesh m, int rock, int dark, int seed, float width, float height, float depth)
        {
            var rng = new Rng(seed);
            float tunnelDepth = depth * 0.6f;
            EnvShapes.Tunnel(m, dark, new Vector3(0f, height * 0.5f, -tunnelDepth * 0.5f), new Vector3(width, height, tunnelDepth), 2f);
            foreach (float side in new[] { -1f, 1f })
            {
                int stack = 3;
                for (int k = 0; k < stack; k++)
                {
                    float w = width * (0.42f - k * 0.07f);
                    float x = side * (width * 0.5f + w * 0.85f);
                    float y = k * height * 0.34f;
                    EnvShapes.GroundedRock(m, rock, new Vector3(x, y, rng.Range(-0.5f, 0.1f)), new Vector3(w, height * 0.27f, w * 1.15f), seed + k * 7 + (int)side * 3, 0.22f, 1.8f, 1);
                }
            }

            for (int k = 0; k < 3; k++)
            {
                float x = (k - 1) * width * 0.38f;
                EnvShapes.Rock(m, rock, new Vector3(x, height * 1.2f - Mathf.Abs(k - 1) * height * 0.05f, -0.2f), new Vector3(width * 0.36f, height * 0.26f, width * 0.45f), seed + 30 + k, 0.2f, 0.1f, 1.8f, 1, new Vector3(0f, rng.Range(0f, 360f), 0f));
            }

            EnvShapes.Rock(m, rock, new Vector3(0f, height * 0.05f, -tunnelDepth - depth * 0.45f), new Vector3(width * 1.4f, height * 1.4f, depth * 0.55f), seed + 60, 0.14f, 0.95f, 2.2f, 1, Vector3.zero);
            for (int i = 0; i < 3; i++)
            {
                float x = rng.Range(-1f, 1f) * width * 0.7f;
                EnvShapes.GroundedRock(m, rock, new Vector3(x, 0f, rng.Range(0.6f, 1.4f)), Vector3.one * rng.Range(0.25f, 0.5f), seed + 70 + i, 0.25f, 1.6f, 1);
            }
        }

        /// <summary>Builds a timber-framed mine entrance with rails.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="dark">Interior submesh.</param>
        /// <param name="wood">Timber submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="width">Opening width.</param>
        /// <param name="height">Opening height.</param>
        public static void MineEntrance(EnvMesh m, int rock, int dark, int wood, int seed, float width, float height)
        {
            CaveMouth(m, rock, dark, seed, width + 0.8f, height + 0.4f, 6f);
            EnvDecor.TimberFrame(m, wood, width, height, 0.1f);
            EnvDecor.TimberFrame(m, wood, width, height, -2.2f);
            for (int i = 0; i < 6; i++)
            {
                EnvShapes.Box(m, wood, new Vector3(0f, 0.04f, 1.8f - i * 0.8f), new Vector3(width * 0.8f, 0.07f, 0.18f), Identity, 1f, false);
            }

            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, wood, new Vector3(s * width * 0.25f, 0.1f, 0.2f), new Vector3(0.07f, 0.06f, 5.4f), Identity, 1f, false);
            }
        }

        /// <summary>Builds a stone gate with pillars, lintel and steps leading into a dark doorway.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Block submesh.</param>
        /// <param name="dark">Interior submesh.</param>
        /// <param name="width">Doorway width.</param>
        /// <param name="height">Doorway height.</param>
        /// <param name="ruined">Break the lintel and one pillar.</param>
        public static void StoneGate(EnvMesh m, int stone, int dark, float width, float height, bool ruined)
        {
            EnvShapes.Tunnel(m, dark, new Vector3(0f, height * 0.5f, -2.5f), new Vector3(width, height, 5f), 2f);
            float p = 0.9f;
            foreach (float s in new[] { -1f, 1f })
            {
                float h = ruined && s > 0f ? height * 0.62f : height + 0.3f;
                EnvShapes.Box(m, stone, new Vector3(s * (width * 0.5f + p * 0.5f), h * 0.5f, 0f), new Vector3(p, h, 1.1f), Identity, 1.5f, false);
                EnvShapes.Box(m, stone, new Vector3(s * (width * 0.5f + p * 0.5f), 0.15f, 0.1f), new Vector3(p + 0.3f, 0.3f, 1.4f), Identity, 1.5f, false);
            }

            if (ruined)
            {
                EnvShapes.Box(m, stone, new Vector3(-width * 0.2f, height + 0.35f, 0f), new Vector3(width * 0.8f, 0.7f, 1.2f), Identity, 1.5f, false);
                EnvShapes.GroundedRock(m, stone, new Vector3(width * 0.7f, 0f, 1.4f), new Vector3(0.6f, 0.45f, 0.55f), 3, 0.2f, 1.5f, 1);
            }
            else
            {
                EnvShapes.Box(m, stone, new Vector3(0f, height + 0.35f, 0f), new Vector3(width + p * 2f + 0.3f, 0.7f, 1.3f), Identity, 1.5f, false);
                EnvShapes.Box(m, stone, new Vector3(0f, height + 0.85f, 0f), new Vector3(width + p * 1.4f, 0.3f, 1.0f), Identity, 1.5f, false);
            }

            for (int i = 0; i < 3; i++)
            {
                EnvShapes.Box(m, stone, new Vector3(0f, 0.08f + i * 0.0f, 0.9f + i * 0.45f), new Vector3(width + p * 2f + i * 0.6f, 0.16f + (2 - i) * 0.0f, 0.6f), Identity, 1.5f, false);
            }
        }

        /// <summary>Builds a small mausoleum with a pitched roof and a dark door.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Wall submesh.</param>
        /// <param name="roof">Roof submesh.</param>
        /// <param name="dark">Door submesh.</param>
        public static void Mausoleum(EnvMesh m, int stone, int roof, int dark)
        {
            EnvShapes.Box(m, stone, new Vector3(0f, 0.2f, 0f), new Vector3(5.2f, 0.4f, 5.2f), Identity, 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(-1.9f, 1.8f, -0.2f), new Vector3(1.3f, 2.8f, 4.4f), Identity, 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(1.9f, 1.8f, -0.2f), new Vector3(1.3f, 2.8f, 4.4f), Identity, 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(0f, 3.0f, -0.2f), new Vector3(2.5f, 0.8f, 4.4f), Identity, 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(0f, 1.8f, -1.9f), new Vector3(2.5f, 2.8f, 0.6f), Identity, 1.5f, false);
            EnvShapes.Box(m, dark, new Vector3(0f, 1.3f, -1.5f), new Vector3(1.9f, 1.8f, 0.2f), Identity, 1f, false);
            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, roof, new Vector3(s * 1.45f, 4.1f, -0.2f), new Vector3(3.3f, 0.25f, 4.8f), Quaternion.Euler(0f, 0f, -s * 28f), 1.5f, false);
            }

        }

        /// <summary>Builds a sinkhole: a dark opening ringed by piled rocks.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="dark">Opening submesh.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="radius">Opening radius.</param>
        public static void Sinkhole(EnvMesh m, int rock, int dark, int seed, float radius)
        {
            var rng = new Rng(seed);
            EnvShapes.Cylinder(m, dark, new Vector3(0f, 0.02f, 0f), radius, 0.02f, 12, 2f);
            const int count = 11;
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f + rng.Range(-0.12f, 0.12f);
                float s = radius * rng.Range(0.45f, 0.75f);
                Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (radius + s * 0.55f);
                EnvShapes.GroundedRock(m, rock, p, new Vector3(s, s * rng.Range(0.7f, 1.1f), s), seed + i * 3, 0.22f, 1.8f, 1);
            }
        }

        /// <summary>Builds a sealed circular door set in a stone wall.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Wall submesh.</param>
        /// <param name="metal">Door submesh.</param>
        public static void SealedDoor(EnvMesh m, int stone, int metal)
        {
            EnvShapes.Box(m, stone, new Vector3(0f, 2.2f, 0f), new Vector3(6f, 4.4f, 1.2f), Identity, 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(0f, 0.15f, 0.8f), new Vector3(6.6f, 0.3f, 1.2f), Identity, 1.5f, false);
            EnvShapes.Box(m, metal, new Vector3(0f, 2.0f, 0.66f), new Vector3(2.8f, 2.8f, 0.14f), Identity, 1.5f, false);
            EnvShapes.Box(m, metal, new Vector3(0f, 2.0f, 0.66f), new Vector3(2.8f, 2.8f, 0.14f), Quaternion.Euler(0f, 0f, 45f), 1.5f, false);
            EnvShapes.Box(m, stone, new Vector3(0f, 2.0f, 0.78f), new Vector3(0.7f, 0.7f, 0.12f), Quaternion.Euler(0f, 0f, 45f), 1.5f, false);
        }

        /// <summary>Builds an arch portal with two pillars and a curved lintel.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="stone">Frame submesh.</param>
        /// <param name="glow">Portal surface submesh.</param>
        /// <param name="width">Inner width.</param>
        /// <param name="height">Inner height.</param>
        public static void ArchPortal(EnvMesh m, int stone, int glow, float width, float height)
        {
            foreach (float s in new[] { -1f, 1f })
            {
                EnvShapes.Box(m, stone, new Vector3(s * (width * 0.5f + 0.45f), height * 0.5f, 0f), new Vector3(0.9f, height, 0.9f), Identity, 1.5f, false);
                EnvShapes.Box(m, stone, new Vector3(s * (width * 0.5f + 0.45f), 0.2f, 0f), new Vector3(1.3f, 0.4f, 1.3f), Identity, 1.5f, false);
            }

            const int segments = 7;
            for (int i = 0; i < segments; i++)
            {
                float t = (i + 0.5f) / segments;
                float a = Mathf.PI * t;
                float r = width * 0.5f + 0.45f;
                Vector3 c = new Vector3(Mathf.Cos(a) * r, height + Mathf.Sin(a) * r * 0.55f, 0f);
                EnvShapes.Box(m, stone, c, new Vector3(0.95f, 0.85f, 0.95f), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg - 90f), 1.5f, false);
            }

            EnvShapes.Box(m, glow, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, 0.08f), Identity, 2f, false);
        }

        /// <summary>Builds an ice cave mouth made from crystals and snow-capped rocks.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="rock">Rock submesh.</param>
        /// <param name="snow">Snow submesh.</param>
        /// <param name="ice">Ice submesh.</param>
        /// <param name="dark">Interior submesh.</param>
        /// <param name="seed">Random seed.</param>
        public static void IceCave(EnvMesh m, int rock, int snow, int ice, int dark, int seed)
        {
            CaveMouth(m, rock, dark, seed, 4.2f, 3.6f, 6f);
            var rng = new Rng(seed);
            EnvShapes.Rock(m, snow, new Vector3(0f, 4.4f, -3.2f), new Vector3(3.6f, 1.0f, 2.8f), seed + 5, 0.1f, 0.3f, 2f, 1, Vector3.zero);
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 1.3f;
                float h = rng.Range(0.7f, 1.4f);
                var profile = new List<Vector2> { new Vector2(0f, 0f), new Vector2(0.12f, 0f), new Vector2(0.03f, -h) };
                EnvShapes.Lathe(m, ice, new Vector3(x, 3.55f, 0.35f), profile, 5, 1f, true);
            }

            foreach (float s in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float h = rng.Range(1.0f, 2.2f);
                    var crystal = new List<Vector2> { new Vector2(0f, -0.1f), new Vector2(h * 0.17f, 0f), new Vector2(h * 0.18f, h * 0.7f), new Vector2(0f, h) };
                    EnvShapes.Lathe(m, ice, new Vector3(s * (3.4f + i * 0.5f), 0f, 0.6f + i * 0.3f), crystal, 6, 2f, true);
                }
            }
        }
    }
}
