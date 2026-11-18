using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds the rock collar and the dark cave of the waterfall mouth. The collar is a lumpy ring that is buried in
    /// the wall on its outer edge and sticks out on its inner edge, hiding the stair stepped cut of the terrain. The
    /// cave is a tapering tube that starts at the inner edge of the collar and ends in a closed back.
    /// </summary>
    public static class WaterfallMouthMeshBuilder
    {
        private const int CaveRings = 6;
        private const float EmbedDepth = 0.25f;
        private const float CaveJitter = 0.3f;
        private const float CapDepth = 0.5f;
        private const float EntranceBrightness = 0.12f;
        private const float DarknessCurve = 0.7f;
        private const float MinFacetBrightness = 0.55f;

        /// <summary>
        /// Builds both meshes of the mouth in world space.
        /// </summary>
        /// <param name="profile">Profile with the collar and cave dimensions.</param>
        /// <param name="outline">Outline of the opening.</param>
        /// <param name="wall">Terrain probe used to place the collar on the wall.</param>
        /// <param name="uvScale">Texture coordinates per meter of the collar, so it tiles like the terrain rock.</param>
        /// <param name="rim">The rock collar mesh.</param>
        /// <param name="cave">The cave mesh.</param>
        public static void Build(WaterfallProfile profile, MouthOutline outline, WaterfallWall wall, float uvScale, out Mesh rim, out Mesh cave)
        {
            var random = new System.Random(profile.Seed + 101);
            var count = outline.Points.Length;

            var centerSurfaceZ = wall.SurfaceZ(outline.Center.x, outline.Center.y);
            var normal = wall.NormalAt(outline.Center.x, centerSurfaceZ);
            if (normal.z > 0f)
            {
                normal = -normal;
            }

            var outer = new Vector3[count];
            var middle = new Vector3[count];
            var inner = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                var point = outline.Points[i];
                var direction = (point - outline.Center).normalized;
                var width = Mathf.Lerp(profile.RimMinWidth, profile.RimMaxWidth, Next(random));

                outer[i] = OnWall(wall, point + direction * width, normal, -EmbedDepth);
                middle[i] = OnWall(wall, point + direction * width * 0.5f, normal, profile.RimProtrusion * Mathf.Lerp(0.5f, 1f, Next(random)));
                inner[i] = OnWall(wall, point, normal, outline.OnSill[i] ? 0f : profile.RimProtrusion * Mathf.Lerp(0.6f, 1.2f, Next(random)));
            }

            rim = BuildRim(outline, outer, middle, inner, normal, uvScale);
            cave = BuildCave(profile, outline, inner, random);
        }

        private static Mesh BuildRim(MouthOutline outline, Vector3[] outer, Vector3[] middle, Vector3[] inner, Vector3 normal, float uvScale)
        {
            var count = outer.Length;
            var vertices = new List<Vector3>(count * 3);
            vertices.AddRange(outer);
            vertices.AddRange(middle);
            vertices.AddRange(inner);

            var uvs = new List<Vector2>(vertices.Count);
            foreach (var vertex in vertices)
            {
                uvs.Add(new Vector2(vertex.x * uvScale, vertex.y * uvScale));
            }

            var triangles = new List<int>();
            for (var i = 0; i < count; i++)
            {
                var j = (i + 1) % count;
                if (outline.OnSill[i] && outline.OnSill[j])
                {
                    continue;
                }

                AddQuad(triangles, i, j, count + j, count + i);
                AddQuad(triangles, count + i, count + j, 2 * count + j, 2 * count + i);
            }

            if (FacingScore(vertices, triangles, normal) < 0f)
            {
                triangles.Reverse();
            }

            var mesh = new Mesh { name = "Waterfall_MouthRim" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCave(WaterfallProfile profile, MouthOutline outline, Vector3[] inner, System.Random random)
        {
            var count = inner.Length;
            var vertices = new List<Vector3>((CaveRings + 1) * count + 1);
            var colors = new List<Color>(vertices.Capacity);
            for (var ring = 0; ring <= CaveRings; ring++)
            {
                var t = ring / (float)CaveRings;
                var scale = Mathf.Lerp(1f, profile.CaveTaper, t);
                var brightness = EntranceBrightness * Mathf.Pow(1f - t, DarknessCurve);
                for (var i = 0; i < count; i++)
                {
                    var start = inner[i];
                    var jitterX = (Next(random) - 0.5f) * 2f * CaveJitter * t;
                    var jitterY = outline.OnSill[i] ? 0f : (Next(random) - 0.5f) * 2f * CaveJitter * t;
                    vertices.Add(new Vector3(
                        profile.CenterX + (start.x - profile.CenterX) * scale + jitterX,
                        profile.SillHeight + (start.y - profile.SillHeight) * scale + jitterY,
                        start.z + profile.CaveDepth * t));

                    var facet = brightness * Mathf.Lerp(MinFacetBrightness, 1f, Next(random));
                    colors.Add(new Color(facet, facet * 0.88f, facet * 0.75f, 1f));
                }
            }

            var last = CaveRings * count;
            var back = Vector3.zero;
            for (var i = 0; i < count; i++)
            {
                back += vertices[last + i];
            }

            back /= count;
            back.z += CapDepth;
            var backIndex = vertices.Count;
            vertices.Add(back);
            colors.Add(Color.black);

            var triangles = new List<int>();
            for (var ring = 0; ring < CaveRings; ring++)
            {
                for (var i = 0; i < count; i++)
                {
                    var j = (i + 1) % count;
                    AddQuad(triangles, ring * count + i, ring * count + j, (ring + 1) * count + j, (ring + 1) * count + i);
                }
            }

            for (var i = 0; i < count; i++)
            {
                triangles.Add(last + i);
                triangles.Add(last + (i + 1) % count);
                triangles.Add(backIndex);
            }

            var mesh = new Mesh { name = "Waterfall_MouthCave" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 OnWall(WaterfallWall wall, Vector2 point, Vector3 normal, float lift)
        {
            return new Vector3(point.x, point.y, wall.SurfaceZ(point.x, point.y)) + normal * lift;
        }

        private static float Next(System.Random random)
        {
            return (float)random.NextDouble();
        }

        private static void AddQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private static float FacingScore(List<Vector3> vertices, List<int> triangles, Vector3 normal)
        {
            var score = 0f;
            for (var i = 0; i < triangles.Count; i += 3)
            {
                var a = vertices[triangles[i]];
                var b = vertices[triangles[i + 1]];
                var c = vertices[triangles[i + 2]];
                score += Vector3.Dot(Vector3.Cross(b - a, c - a), normal);
            }

            return score;
        }
    }
}
