using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Reusable geometry primitives used by every procedural environment asset.</summary>
    public static class EnvShapes
    {
        /// <summary>Adds a smooth tube that follows a path of points with a radius per point.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="points">Path points.</param>
        /// <param name="radii">Radius at each path point.</param>
        /// <param name="sides">Radial segment count.</param>
        /// <param name="uvTile">World length covered by one vertical texture repeat.</param>
        /// <param name="capStart">Close the first end.</param>
        /// <param name="capEnd">Close the last end.</param>
        public static void PathTube(EnvMesh m, int sub, IList<Vector3> points, IList<float> radii, int sides, float uvTile, bool capStart, bool capEnd)
        {
            int n = points.Count;
            var tangents = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                Vector3 t = i == 0 ? points[1] - points[0] : i == n - 1 ? points[n - 1] - points[n - 2] : points[i + 1] - points[i - 1];
                tangents[i] = t.normalized;
            }

            Vector3 reference = Mathf.Abs(tangents[0].y) > 0.9f ? Vector3.right : Vector3.up;
            Vector3 frame = Vector3.Cross(tangents[0], reference).normalized;
            var rings = new int[n, sides + 1];
            float distance = 0f;
            for (int i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    distance += (points[i] - points[i - 1]).magnitude;
                    frame = (frame - tangents[i] * Vector3.Dot(frame, tangents[i])).normalized;
                }

                Vector3 binormal = Vector3.Cross(tangents[i], frame);
                for (int s = 0; s <= sides; s++)
                {
                    float angle = s / (float)sides * Mathf.PI * 2f;
                    Vector3 radial = frame * Mathf.Cos(angle) + binormal * Mathf.Sin(angle);
                    rings[i, s] = m.AddVertex(points[i] + radial * radii[i], radial, new Vector2(s / (float)sides * 2f, distance / uvTile));
                }
            }

            for (int i = 0; i < n - 1; i++)
            {
                for (int s = 0; s < sides; s++)
                {
                    m.AddOrientedQuad(sub, rings[i, s], rings[i + 1, s], rings[i + 1, s + 1], rings[i, s + 1]);
                }
            }

            if (capStart)
            {
                AddCap(m, sub, points[0], -tangents[0], rings, 0, sides, radii[0], uvTile);
            }

            if (capEnd)
            {
                AddCap(m, sub, points[n - 1], tangents[n - 1], rings, n - 1, sides, radii[n - 1], uvTile);
            }
        }

        /// <summary>Adds a straight tapered tube.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="from">Start point.</param>
        /// <param name="to">End point.</param>
        /// <param name="r0">Start radius.</param>
        /// <param name="r1">End radius.</param>
        /// <param name="sides">Radial segment count.</param>
        /// <param name="uvTile">World length covered by one vertical texture repeat.</param>
        /// <param name="capEnd">Close the far end.</param>
        public static void Tube(EnvMesh m, int sub, Vector3 from, Vector3 to, float r0, float r1, int sides, float uvTile, bool capEnd)
        {
            PathTube(m, sub, new[] { from, to }, new[] { r0, r1 }, sides, uvTile, false, capEnd);
        }

        /// <summary>Adds an axis-aligned or rotated box with flat shading.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Box center.</param>
        /// <param name="size">Box size.</param>
        /// <param name="rotation">Box rotation.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        /// <param name="inward">Face normals toward the center, for tunnels.</param>
        public static void Box(EnvMesh m, int sub, Vector3 center, Vector3 size, Quaternion rotation, float uvScale, bool inward)
        {
            var c = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 local = new Vector3((i & 4) != 0 ? 0.5f : -0.5f, (i & 2) != 0 ? 0.5f : -0.5f, (i & 1) != 0 ? 0.5f : -0.5f);
                c[i] = center + rotation * Vector3.Scale(local, size);
            }

            int[][] faces = { new[] { 4, 5, 7, 6 }, new[] { 0, 1, 3, 2 }, new[] { 2, 3, 7, 6 }, new[] { 0, 1, 5, 4 }, new[] { 1, 3, 7, 5 }, new[] { 0, 2, 6, 4 } };
            foreach (int[] f in faces)
            {
                m.AddFlatQuad(sub, c[f[0]], c[f[1]], c[f[2]], c[f[3]], center, inward, uvScale);
            }
        }

        /// <summary>Adds a box without its front (+Z) and bottom faces, facing inward: a tunnel interior.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Tunnel center.</param>
        /// <param name="size">Tunnel size.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        public static void Tunnel(EnvMesh m, int sub, Vector3 center, Vector3 size, float uvScale)
        {
            Vector3 h = size * 0.5f;
            Vector3 a = center + new Vector3(-h.x, -h.y, -h.z);
            Vector3 b = center + new Vector3(h.x, -h.y, -h.z);
            Vector3 c = center + new Vector3(h.x, h.y, -h.z);
            Vector3 d = center + new Vector3(-h.x, h.y, -h.z);
            Vector3 e = center + new Vector3(-h.x, -h.y, h.z);
            Vector3 f = center + new Vector3(h.x, -h.y, h.z);
            Vector3 g = center + new Vector3(h.x, h.y, h.z);
            Vector3 i = center + new Vector3(-h.x, h.y, h.z);
            m.AddFlatQuad(sub, a, b, c, d, center, true, uvScale);
            m.AddFlatQuad(sub, a, e, i, d, center, true, uvScale);
            m.AddFlatQuad(sub, b, f, g, c, center, true, uvScale);
            m.AddFlatQuad(sub, d, c, g, i, center, true, uvScale);
            m.AddFlatQuad(sub, a, b, f, e, center, true, uvScale);
        }

        /// <summary>Adds a surface of revolution around the Y axis.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="baseCenter">Center of the profile base.</param>
        /// <param name="profile">Profile points as (radius, height), ordered bottom to top.</param>
        /// <param name="sides">Radial segment count.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        /// <param name="hardEdges">Duplicate rings per segment so profile corners stay sharp.</param>
        public static void Lathe(EnvMesh m, int sub, Vector3 baseCenter, IList<Vector2> profile, int sides, float uvScale, bool hardEdges)
        {
            float maxR = 0.01f;
            foreach (Vector2 p in profile)
            {
                maxR = Mathf.Max(maxR, p.x);
            }

            float circumference = Mathf.PI * 2f * maxR;
            if (hardEdges)
            {
                float v0 = 0f;
                for (int j = 0; j < profile.Count - 1; j++)
                {
                    Vector2 p0 = profile[j];
                    Vector2 p1 = profile[j + 1];
                    Vector2 t = p1 - p0;
                    float len = t.magnitude;
                    if (len < 1e-5f)
                    {
                        continue;
                    }

                    Vector2 n2 = new Vector2(t.y, -t.x) / len;
                    int[] r0 = LatheRing(m, baseCenter, p0, n2, sides, circumference / uvScale, v0 / uvScale);
                    int[] r1 = LatheRing(m, baseCenter, p1, n2, sides, circumference / uvScale, (v0 + len) / uvScale);
                    for (int s = 0; s < sides; s++)
                    {
                        m.AddOrientedQuad(sub, r0[s], r1[s], r1[s + 1], r0[s + 1]);
                    }

                    v0 += len;
                }

                return;
            }

            var rings = new int[profile.Count][];
            float v = 0f;
            for (int j = 0; j < profile.Count; j++)
            {
                Vector2 prev = profile[Mathf.Max(0, j - 1)];
                Vector2 next = profile[Mathf.Min(profile.Count - 1, j + 1)];
                Vector2 t = next - prev;
                Vector2 n2 = t.sqrMagnitude < 1e-10f ? Vector2.right : new Vector2(t.y, -t.x).normalized;
                if (j > 0)
                {
                    v += (profile[j] - profile[j - 1]).magnitude;
                }

                rings[j] = LatheRing(m, baseCenter, profile[j], n2, sides, circumference / uvScale, v / uvScale);
            }

            for (int j = 0; j < profile.Count - 1; j++)
            {
                for (int s = 0; s < sides; s++)
                {
                    m.AddOrientedQuad(sub, rings[j][s], rings[j + 1][s], rings[j + 1][s + 1], rings[j][s + 1]);
                }
            }
        }

        /// <summary>Adds a capped cylinder with hard edges.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="baseCenter">Center of the bottom face.</param>
        /// <param name="radius">Cylinder radius.</param>
        /// <param name="height">Cylinder height.</param>
        /// <param name="sides">Radial segment count.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        public static void Cylinder(EnvMesh m, int sub, Vector3 baseCenter, float radius, float height, int sides, float uvScale)
        {
            Lathe(m, sub, baseCenter, new[] { new Vector2(0f, 0f), new Vector2(radius, 0f), new Vector2(radius, height), new Vector2(0f, height) }, sides, uvScale, true);
        }

        /// <summary>Adds a faceted low-poly rock made from a displaced icosphere.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Rock center.</param>
        /// <param name="radii">Radii along each axis.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="roughness">Radial displacement amount.</param>
        /// <param name="bottomCut">Fraction of the lower half removed to create a flat base (0 to 0.9).</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        /// <param name="subdivisions">Icosphere subdivision level.</param>
        /// <param name="euler">Rock rotation in degrees.</param>
        public static void Rock(EnvMesh m, int sub, Vector3 center, Vector3 radii, int seed, float roughness, float bottomCut, float uvScale, int subdivisions, Vector3 euler)
        {
            var rng = new Rng(seed);
            List<Vector3> dirs;
            List<int[]> faces;
            Icosphere(subdivisions, out dirs, out faces);
            Quaternion rotation = Quaternion.Euler(euler);
            var points = new Vector3[dirs.Count];
            for (int i = 0; i < dirs.Count; i++)
            {
                Vector3 d = dirs[i];
                Vector3 p = Vector3.Scale(d, radii) * (1f + rng.Range(-1f, 1f) * roughness);
                float floor = -radii.y * (1f - bottomCut);
                if (p.y < floor)
                {
                    p.y = floor;
                }

                points[i] = rotation * p;
            }

            foreach (int[] f in faces)
            {
                m.AddFlatTriangle(sub, center + points[f[0]], center + points[f[1]], center + points[f[2]], center, false, uvScale);
            }
        }

        /// <summary>Adds a rock resting on the ground at the given base position.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="ground">Point on the ground below the rock.</param>
        /// <param name="radii">Radii along each axis.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="roughness">Radial displacement amount.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        /// <param name="subdivisions">Icosphere subdivision level.</param>
        public static void GroundedRock(EnvMesh m, int sub, Vector3 ground, Vector3 radii, int seed, float roughness, float uvScale, int subdivisions)
        {
            var rng = new Rng(seed + 991);
            Vector3 euler = new Vector3(rng.Range(-6f, 6f), rng.Range(0f, 360f), rng.Range(-6f, 6f));
            Rock(m, sub, ground + Vector3.up * (radii.y * 0.62f), radii, seed, roughness, 0.38f, uvScale, subdivisions, euler);
        }

        /// <summary>Adds a leaf card that extends along a direction with a downward droop.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="origin">Card base position.</param>
        /// <param name="direction">Unit direction the card extends along.</param>
        /// <param name="side">Unit vector across the card width.</param>
        /// <param name="length">Card length.</param>
        /// <param name="width">Card width.</param>
        /// <param name="droop">Downward bend at the tip relative to length.</param>
        /// <param name="segments">Segments along the length.</param>
        /// <param name="shadingCenter">Point the normals radiate from.</param>
        /// <param name="upBias">Upward bias for normals.</param>
        public static void CurvedCard(EnvMesh m, int sub, Vector3 origin, Vector3 direction, Vector3 side, float length, float width, float droop, int segments, Vector3 shadingCenter, float upBias)
        {
            var left = new int[segments + 1];
            var right = new int[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 center = origin + direction * (length * t) + Vector3.down * (droop * length * t * t);
                Vector3 normal = (center - shadingCenter).normalized + Vector3.up * upBias;
                left[i] = m.AddVertex(center - side * (width * 0.5f), normal, new Vector2(0f, t));
                right[i] = m.AddVertex(center + side * (width * 0.5f), normal, new Vector2(1f, t));
            }

            for (int i = 0; i < segments; i++)
            {
                m.AddQuad(sub, left[i], right[i], right[i + 1], left[i + 1]);
            }
        }

        /// <summary>Adds a flat square leaf card facing outward from a shading center.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Card center.</param>
        /// <param name="facing">Unit direction the card faces.</param>
        /// <param name="size">Edge length.</param>
        /// <param name="roll">Roll around the facing axis in degrees.</param>
        /// <param name="shadingCenter">Point the normals radiate from.</param>
        /// <param name="upBias">Upward bias for normals.</param>
        public static void FacingCard(EnvMesh m, int sub, Vector3 center, Vector3 facing, float size, float roll, Vector3 shadingCenter, float upBias)
        {
            Vector3 reference = Mathf.Abs(Vector3.Dot(facing, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            Vector3 tangent = Quaternion.AngleAxis(roll, facing) * Vector3.Cross(reference, facing).normalized;
            Vector3 bitangent = Vector3.Cross(facing, tangent);
            float h = size * 0.5f;
            Vector3[] corners = { center - tangent * h - bitangent * h, center + tangent * h - bitangent * h, center + tangent * h + bitangent * h, center - tangent * h + bitangent * h };
            Vector2[] uv = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            var index = new int[4];
            for (int i = 0; i < 4; i++)
            {
                index[i] = m.AddVertex(corners[i], (corners[i] - shadingCenter).normalized + Vector3.up * upBias, uv[i]);
            }

            m.AddQuad(sub, index[0], index[1], index[2], index[3]);
        }

        /// <summary>Scatters facing cards inside an ellipsoid to build a soft leaf mass.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Ellipsoid center.</param>
        /// <param name="radii">Ellipsoid radii.</param>
        /// <param name="count">Number of cards.</param>
        /// <param name="sizeMin">Minimum card size.</param>
        /// <param name="sizeMax">Maximum card size.</param>
        /// <param name="shadingCenter">Point the normals radiate from.</param>
        /// <param name="upBias">Upward bias for normals.</param>
        /// <param name="rng">Random source.</param>
        public static void LeafCluster(EnvMesh m, int sub, Vector3 center, Vector3 radii, int count, float sizeMin, float sizeMax, Vector3 shadingCenter, float upBias, Rng rng)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 facing = rng.Unit();
                if (facing.y < -0.35f)
                {
                    facing.y = -facing.y * 0.4f;
                    facing.Normalize();
                }

                Vector3 position = center + Vector3.Scale(facing, radii) * rng.Range(0.5f, 0.85f);
                FacingCard(m, sub, position, facing, rng.Range(sizeMin, sizeMax), rng.Range(0f, 360f), shadingCenter, upBias);
            }
        }

        /// <summary>Adds an upright card that grows from the ground, used for grass and flowers.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="basePosition">Ground position of the card base.</param>
        /// <param name="yaw">Yaw in degrees.</param>
        /// <param name="width">Card width.</param>
        /// <param name="height">Card height.</param>
        /// <param name="lean">Horizontal lean of the tip along the card normal.</param>
        public static void UprightCard(EnvMesh m, int sub, Vector3 basePosition, float yaw, float width, float height, float lean)
        {
            Quaternion q = Quaternion.Euler(0f, yaw, 0f);
            Vector3 side = q * Vector3.right;
            Vector3 forward = q * Vector3.forward;
            Vector3 normal = (Vector3.up * 0.8f + forward * 0.3f).normalized;
            int a = m.AddVertex(basePosition - side * (width * 0.5f), normal, new Vector2(0, 0));
            int b = m.AddVertex(basePosition + side * (width * 0.5f), normal, new Vector2(1, 0));
            Vector3 mid = basePosition + Vector3.up * (height * 0.5f) + forward * (lean * 0.35f);
            int c = m.AddVertex(mid - side * (width * 0.5f), normal, new Vector2(0, 0.5f));
            int d = m.AddVertex(mid + side * (width * 0.5f), normal, new Vector2(1, 0.5f));
            Vector3 tip = basePosition + Vector3.up * height + forward * lean;
            int e = m.AddVertex(tip - side * (width * 0.5f), normal, new Vector2(0, 1));
            int f = m.AddVertex(tip + side * (width * 0.5f), normal, new Vector2(1, 1));
            m.AddQuad(sub, a, b, d, c);
            m.AddQuad(sub, c, d, f, e);
        }

        /// <summary>Adds a clump of crossing upright cards.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="position">Clump ground position.</param>
        /// <param name="width">Card width.</param>
        /// <param name="height">Card height.</param>
        /// <param name="cards">Number of crossing cards.</param>
        /// <param name="rng">Random source.</param>
        public static void Clump(EnvMesh m, int sub, Vector3 position, float width, float height, int cards, Rng rng)
        {
            float offset = rng.Range(0f, 180f);
            for (int i = 0; i < cards; i++)
            {
                UprightCard(m, sub, position, offset + i * 180f / cards + rng.Range(-12f, 12f), width, height * rng.Range(0.85f, 1.1f), rng.Range(-0.1f, 0.1f) * height);
            }
        }

        /// <summary>Scatters clumps over a disc.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Disc center.</param>
        /// <param name="radius">Disc radius.</param>
        /// <param name="clumps">Number of clumps.</param>
        /// <param name="width">Card width.</param>
        /// <param name="height">Card height.</param>
        /// <param name="cards">Cards per clump.</param>
        /// <param name="rng">Random source.</param>
        public static void Patch(EnvMesh m, int sub, Vector3 center, float radius, int clumps, float width, float height, int cards, Rng rng)
        {
            for (int i = 0; i < clumps; i++)
            {
                float a = rng.Range(0f, Mathf.PI * 2f);
                float r = radius * Mathf.Sqrt(rng.Range(0f, 1f));
                Clump(m, sub, center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), width, height * rng.Range(0.8f, 1.15f), cards, rng);
            }
        }

        /// <summary>Adds a closed circular tube lying in the XZ plane, such as a coil of rope.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="sub">Submesh index.</param>
        /// <param name="center">Ring center.</param>
        /// <param name="ringRadius">Radius of the ring.</param>
        /// <param name="tubeRadius">Radius of the tube.</param>
        /// <param name="segments">Segments around the ring.</param>
        /// <param name="sides">Sides of the tube.</param>
        /// <param name="uvTile">World length per texture repeat.</param>
        public static void Torus(EnvMesh m, int sub, Vector3 center, float ringRadius, float tubeRadius, int segments, int sides, float uvTile)
        {
            var points = new List<Vector3>();
            var radii = new List<float>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                points.Add(center + new Vector3(Mathf.Cos(a) * ringRadius, 0f, Mathf.Sin(a) * ringRadius));
                radii.Add(tubeRadius);
            }

            PathTube(m, sub, points, radii, sides, uvTile, false, false);
        }

        private static int[] LatheRing(EnvMesh m, Vector3 baseCenter, Vector2 profilePoint, Vector2 normal2, int sides, float uScale, float v)
        {
            var ring = new int[sides + 1];
            for (int s = 0; s <= sides; s++)
            {
                float a = s / (float)sides * Mathf.PI * 2f;
                float cos = Mathf.Cos(a);
                float sin = Mathf.Sin(a);
                Vector3 position = baseCenter + new Vector3(profilePoint.x * cos, profilePoint.y, profilePoint.x * sin);
                Vector3 normal = new Vector3(normal2.x * cos, normal2.y, normal2.x * sin);
                ring[s] = m.AddVertex(position, normal, new Vector2(s / (float)sides * uScale, v));
            }

            return ring;
        }

        private static void AddCap(EnvMesh m, int sub, Vector3 center, Vector3 normal, int[,] rings, int ringIndex, int sides, float radius, float uvTile)
        {
            int c = m.AddVertex(center, normal, new Vector2(0.5f, 0.5f));
            var edge = new int[sides + 1];
            Vector3 reference = Mathf.Abs(normal.y) > 0.9f ? Vector3.right : Vector3.up;
            Vector3 u = Vector3.Cross(normal, reference).normalized;
            Vector3 v = Vector3.Cross(normal, u);
            for (int s = 0; s <= sides; s++)
            {
                float a = s / (float)sides * Mathf.PI * 2f;
                Vector3 p = center + (u * Mathf.Cos(a) + v * Mathf.Sin(a)) * radius;
                edge[s] = m.AddVertex(p, normal, new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
            }

            for (int s = 0; s < sides; s++)
            {
                m.AddOrientedTriangle(sub, c, edge[s], edge[s + 1]);
            }
        }

        private static void Icosphere(int subdivisions, out List<Vector3> vertices, out List<int[]> faces)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            vertices = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized;
            }

            faces = new List<int[]>
            {
                new[] { 0, 11, 5 }, new[] { 0, 5, 1 }, new[] { 0, 1, 7 }, new[] { 0, 7, 10 }, new[] { 0, 10, 11 },
                new[] { 1, 5, 9 }, new[] { 5, 11, 4 }, new[] { 11, 10, 2 }, new[] { 10, 7, 6 }, new[] { 7, 1, 8 },
                new[] { 3, 9, 4 }, new[] { 3, 4, 2 }, new[] { 3, 2, 6 }, new[] { 3, 6, 8 }, new[] { 3, 8, 9 },
                new[] { 4, 9, 5 }, new[] { 2, 4, 11 }, new[] { 6, 2, 10 }, new[] { 8, 6, 7 }, new[] { 9, 8, 1 }
            };

            for (int level = 0; level < subdivisions; level++)
            {
                var cache = new Dictionary<long, int>();
                var next = new List<int[]>();
                foreach (int[] f in faces)
                {
                    int ab = Midpoint(vertices, cache, f[0], f[1]);
                    int bc = Midpoint(vertices, cache, f[1], f[2]);
                    int ca = Midpoint(vertices, cache, f[2], f[0]);
                    next.Add(new[] { f[0], ab, ca });
                    next.Add(new[] { f[1], bc, ab });
                    next.Add(new[] { f[2], ca, bc });
                    next.Add(new[] { ab, bc, ca });
                }

                faces = next;
            }
        }

        private static int Midpoint(List<Vector3> vertices, Dictionary<long, int> cache, int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            int index;
            if (cache.TryGetValue(key, out index))
            {
                return index;
            }

            vertices.Add(((vertices[a] + vertices[b]) * 0.5f).normalized);
            index = vertices.Count - 1;
            cache[key] = index;
            return index;
        }
    }
}
