using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.Trees
{
    /// <summary>
    /// Accumulates vertices, normals, UVs and triangles for a tree mesh with two submeshes:
    /// trunk (0) and foliage (1).
    /// </summary>
    public sealed class TreeMeshData
    {
        /// <summary>Submesh index used for bark geometry.</summary>
        public const int TrunkSubmesh = 0;

        /// <summary>Submesh index used for leaf card geometry.</summary>
        public const int FoliageSubmesh = 1;

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int>[] triangles = { new List<int>(), new List<int>() };

        /// <summary>Adds a vertex and returns its index.</summary>
        /// <param name="position">Local-space position.</param>
        /// <param name="normal">Local-space normal.</param>
        /// <param name="uv">Texture coordinate.</param>
        /// <returns>The index of the new vertex.</returns>
        public int AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            vertices.Add(position);
            normals.Add(normal.normalized);
            uvs.Add(uv);
            return vertices.Count - 1;
        }

        /// <summary>Adds one triangle to a submesh.</summary>
        /// <param name="submesh">Target submesh index.</param>
        /// <param name="a">First vertex index.</param>
        /// <param name="b">Second vertex index.</param>
        /// <param name="c">Third vertex index.</param>
        public void AddTriangle(int submesh, int a, int b, int c)
        {
            triangles[submesh].Add(a);
            triangles[submesh].Add(b);
            triangles[submesh].Add(c);
        }

        /// <summary>Adds a quad made of two triangles to a submesh.</summary>
        /// <param name="submesh">Target submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        /// <param name="d">Fourth corner.</param>
        public void AddQuad(int submesh, int a, int b, int c, int d)
        {
            AddTriangle(submesh, a, b, c);
            AddTriangle(submesh, a, c, d);
        }

        /// <summary>Builds a Unity mesh from the accumulated data.</summary>
        /// <param name="name">Name assigned to the mesh.</param>
        /// <returns>A new mesh with two submeshes.</returns>
        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(triangles[TrunkSubmesh], TrunkSubmesh);
            mesh.SetTriangles(triangles[FoliageSubmesh], FoliageSubmesh);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }

    /// <summary>Generates the geometry of a single tree.</summary>
    public interface ITreeGenerator
    {
        /// <summary>Creates the tree geometry.</summary>
        /// <returns>The accumulated mesh data.</returns>
        TreeMeshData Generate();
    }

    /// <summary>Reusable primitives shared by the tree generators.</summary>
    public static class TreePrimitives
    {
        /// <summary>Adds a tapered tube to the trunk submesh.</summary>
        /// <param name="data">Destination mesh data.</param>
        /// <param name="from">Tube start.</param>
        /// <param name="to">Tube end.</param>
        /// <param name="startRadius">Radius at the start.</param>
        /// <param name="endRadius">Radius at the end.</param>
        /// <param name="sides">Number of radial sides.</param>
        /// <param name="rings">Number of ring segments along the axis.</param>
        /// <param name="uvTileLength">World length covered by one vertical texture repeat.</param>
        public static void AddTube(TreeMeshData data, Vector3 from, Vector3 to, float startRadius, float endRadius, int sides, int rings, float uvTileLength)
        {
            Vector3 axis = to - from;
            float length = axis.magnitude;
            Vector3 up = axis / length;
            Vector3 reference = Mathf.Abs(Vector3.Dot(up, Vector3.right)) > 0.9f ? Vector3.forward : Vector3.right;
            Vector3 tangent = Vector3.Cross(up, reference).normalized;
            Vector3 bitangent = Vector3.Cross(up, tangent);

            var grid = new int[rings + 1, sides + 1];
            for (int r = 0; r <= rings; r++)
            {
                float t = r / (float)rings;
                Vector3 center = from + axis * t;
                float radius = Mathf.Lerp(startRadius, endRadius, t);
                for (int s = 0; s <= sides; s++)
                {
                    float angle = s / (float)sides * Mathf.PI * 2f;
                    Vector3 radial = tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle);
                    Vector3 normal = (radial + up * ((startRadius - endRadius) / length)).normalized;
                    grid[r, s] = data.AddVertex(center + radial * radius, normal, new Vector2(s / (float)sides * 2f, t * length / uvTileLength));
                }
            }

            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < sides; s++)
                {
                    data.AddQuad(TreeMeshData.TrunkSubmesh, grid[r, s], grid[r + 1, s], grid[r + 1, s + 1], grid[r, s + 1]);
                }
            }
        }

        /// <summary>
        /// Adds a curved leaf card that extends from an origin along a direction. Normals point away
        /// from a shading center so the foliage reads as a soft, rounded mass.
        /// </summary>
        /// <param name="data">Destination mesh data.</param>
        /// <param name="origin">Card base position.</param>
        /// <param name="direction">Unit direction the card extends along.</param>
        /// <param name="side">Unit vector across the card width.</param>
        /// <param name="length">Card length.</param>
        /// <param name="width">Card width.</param>
        /// <param name="droop">Downward bend at the tip, relative to length.</param>
        /// <param name="segments">Number of segments along the length.</param>
        /// <param name="shadingCenter">Point the normals radiate from.</param>
        /// <param name="upBias">Amount of upward bias applied to the normals.</param>
        public static void AddCurvedCard(TreeMeshData data, Vector3 origin, Vector3 direction, Vector3 side, float length, float width, float droop, int segments, Vector3 shadingCenter, float upBias)
        {
            var left = new int[segments + 1];
            var right = new int[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 center = origin + direction * (length * t) + Vector3.down * (droop * length * t * t);
                Vector3 normal = (center - shadingCenter).normalized + Vector3.up * upBias;
                left[i] = data.AddVertex(center - side * (width * 0.5f), normal, new Vector2(0f, t));
                right[i] = data.AddVertex(center + side * (width * 0.5f), normal, new Vector2(1f, t));
            }

            for (int i = 0; i < segments; i++)
            {
                data.AddQuad(TreeMeshData.FoliageSubmesh, left[i], right[i], right[i + 1], left[i + 1]);
            }
        }

        /// <summary>Adds a flat square leaf card facing outward from a shading center.</summary>
        /// <param name="data">Destination mesh data.</param>
        /// <param name="center">Card center.</param>
        /// <param name="facing">Unit direction the card faces.</param>
        /// <param name="size">Edge length of the card.</param>
        /// <param name="roll">Rotation around the facing axis in degrees.</param>
        /// <param name="shadingCenter">Point the normals radiate from.</param>
        /// <param name="upBias">Amount of upward bias applied to the normals.</param>
        public static void AddFacingCard(TreeMeshData data, Vector3 center, Vector3 facing, float size, float roll, Vector3 shadingCenter, float upBias)
        {
            Vector3 reference = Mathf.Abs(Vector3.Dot(facing, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            Vector3 tangent = Vector3.Cross(reference, facing).normalized;
            tangent = Quaternion.AngleAxis(roll, facing) * tangent;
            Vector3 bitangent = Vector3.Cross(facing, tangent);
            float h = size * 0.5f;
            Vector3[] corners =
            {
                center - tangent * h - bitangent * h,
                center + tangent * h - bitangent * h,
                center + tangent * h + bitangent * h,
                center - tangent * h + bitangent * h
            };
            Vector2[] uv = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            var index = new int[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 normal = (corners[i] - shadingCenter).normalized + Vector3.up * upBias;
                index[i] = data.AddVertex(corners[i], normal, uv[i]);
            }

            data.AddQuad(TreeMeshData.FoliageSubmesh, index[0], index[1], index[2], index[3]);
        }
    }

    /// <summary>Generates a layered conifer made of drooping leaf-card fronds.</summary>
    public sealed class SoftPineGenerator : ITreeGenerator
    {
        private readonly System.Random random;
        private readonly float height;
        private readonly int tiers;

        /// <summary>Creates a pine generator.</summary>
        /// <param name="seed">Seed for deterministic variation.</param>
        /// <param name="height">Total tree height in meters.</param>
        /// <param name="tiers">Number of foliage tiers.</param>
        public SoftPineGenerator(int seed, float height, int tiers)
        {
            random = new System.Random(seed);
            this.height = height;
            this.tiers = tiers;
        }

        /// <inheritdoc />
        public TreeMeshData Generate()
        {
            var data = new TreeMeshData();
            TreePrimitives.AddTube(data, Vector3.zero, new Vector3(0f, height * 0.97f, 0f), height * 0.032f, height * 0.006f, 8, 6, height * 0.35f);

            for (int i = 0; i < tiers; i++)
            {
                float t = i / (float)(tiers - 1);
                float y = height * Mathf.Lerp(0.15f, 0.9f, t);
                float radius = height * 0.30f * Mathf.Pow(1f - t, 0.85f) + height * 0.045f;
                int count = Mathf.RoundToInt(Mathf.Lerp(9f, 5f, t));
                float pitch = Mathf.Lerp(0.38f, 0.04f, t);
                Vector3 shadingCenter = new Vector3(0f, y + radius * 0.35f, 0f);

                for (int j = 0; j < count; j++)
                {
                    float yaw = ((j + 0.5f * (i % 2)) / count * 360f + Range(-12f, 12f)) * Mathf.Deg2Rad;
                    float p = pitch + Range(-0.05f, 0.05f);
                    Vector3 direction = new Vector3(Mathf.Cos(yaw) * Mathf.Cos(p), -Mathf.Sin(p), Mathf.Sin(yaw) * Mathf.Cos(p)).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
                    float length = radius * Range(1.1f, 1.3f);
                    float width = length * 0.95f;
                    foreach (float roll in new[] { -24f, 24f })
                    {
                        Vector3 rolledSide = Quaternion.AngleAxis(roll, direction) * side;
                        TreePrimitives.AddCurvedCard(data, new Vector3(0f, y, 0f), direction, rolledSide, length, width, 0.22f, 3, shadingCenter, 0.55f);
                    }
                }
            }

            AddCrown(data);
            return data;
        }

        private void AddCrown(TreeMeshData data)
        {
            float baseY = height * 0.84f;
            float length = height * 0.18f;
            Vector3 shadingCenter = new Vector3(0f, baseY, 0f);
            for (int k = 0; k < 3; k++)
            {
                float yaw = k * 60f * Mathf.Deg2Rad;
                Vector3 side = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
                TreePrimitives.AddCurvedCard(data, new Vector3(0f, baseY, 0f), Vector3.up, side, length, length * 0.8f, 0f, 2, shadingCenter, 0.2f);
            }
        }

        private float Range(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }

    /// <summary>Generates a rounded broadleaf tree with clustered leaf cards.</summary>
    public sealed class SoftBroadleafGenerator : ITreeGenerator
    {
        private readonly System.Random random;
        private readonly float height;
        private readonly int clusters;

        /// <summary>Creates a broadleaf generator.</summary>
        /// <param name="seed">Seed for deterministic variation.</param>
        /// <param name="height">Total tree height in meters.</param>
        /// <param name="clusters">Number of leaf clusters in the crown.</param>
        public SoftBroadleafGenerator(int seed, float height, int clusters)
        {
            random = new System.Random(seed);
            this.height = height;
            this.clusters = clusters;
        }

        /// <inheritdoc />
        public TreeMeshData Generate()
        {
            var data = new TreeMeshData();
            Vector3 crownCenter = new Vector3(0f, height * 0.66f, 0f);
            float crownRadius = height * 0.3f;
            Vector3 trunkTop = new Vector3(0f, height * 0.5f, 0f);
            TreePrimitives.AddTube(data, Vector3.zero, trunkTop, height * 0.05f, height * 0.034f, 8, 4, height * 0.4f);

            for (int c = 0; c < clusters; c++)
            {
                Vector3 offset = c == 0
                    ? new Vector3(0f, crownRadius * 0.55f, 0f)
                    : Quaternion.Euler(0f, c / (float)(clusters - 1) * 360f + Range(-20f, 20f), 0f) * new Vector3(crownRadius * Range(0.6f, 0.8f), Range(-0.15f, 0.2f) * crownRadius, 0f);
                Vector3 clusterCenter = crownCenter + offset;
                float clusterRadius = crownRadius * Range(0.55f, 0.7f);
                TreePrimitives.AddTube(data, trunkTop, clusterCenter - offset.normalized * clusterRadius * 0.3f, height * 0.022f, height * 0.01f, 5, 2, height * 0.4f);
                AddCluster(data, clusterCenter, clusterRadius, crownCenter);
            }

            return data;
        }

        private void AddCluster(TreeMeshData data, Vector3 center, float radius, Vector3 crownCenter)
        {
            const int cardsPerCluster = 16;
            for (int i = 0; i < cardsPerCluster; i++)
            {
                Vector3 facing = RandomUnit();
                if (facing.y < -0.35f)
                {
                    facing.y = -facing.y * 0.4f;
                    facing.Normalize();
                }

                Vector3 position = center + facing * radius * Range(0.45f, 0.8f);
                TreePrimitives.AddFacingCard(data, position, facing, radius * Range(1.25f, 1.6f), Range(0f, 360f), crownCenter, 0.25f);
            }
        }

        private Vector3 RandomUnit()
        {
            float z = Range(-1f, 1f);
            float a = Range(0f, Mathf.PI * 2f);
            float r = Mathf.Sqrt(1f - z * z);
            return new Vector3(r * Mathf.Cos(a), z, r * Mathf.Sin(a));
        }

        private float Range(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }
}
