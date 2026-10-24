using System.Collections.Generic;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Small deterministic random generator used by the procedural asset builders.</summary>
    public sealed class Rng
    {
        private readonly System.Random random;

        /// <summary>Creates a generator.</summary>
        /// <param name="seed">Seed for deterministic output.</param>
        public Rng(int seed)
        {
            random = new System.Random(seed);
        }

        /// <summary>Returns a float in the given range.</summary>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Exclusive upper bound.</param>
        /// <returns>A random value.</returns>
        public float Range(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        /// <summary>Returns a uniformly distributed unit vector.</summary>
        /// <returns>A random direction.</returns>
        public Vector3 Unit()
        {
            float z = Range(-1f, 1f);
            float a = Range(0f, Mathf.PI * 2f);
            float r = Mathf.Sqrt(1f - z * z);
            return new Vector3(r * Mathf.Cos(a), z, r * Mathf.Sin(a));
        }

        /// <summary>Returns true with the given probability.</summary>
        /// <param name="probability">Chance between 0 and 1.</param>
        /// <returns>True when the roll succeeds.</returns>
        public bool Chance(float probability)
        {
            return random.NextDouble() < probability;
        }
    }

    /// <summary>Accumulates geometry for a mesh with an arbitrary number of submeshes.</summary>
    public sealed class EnvMesh
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int>[] triangles;

        /// <summary>Creates an empty mesh builder.</summary>
        /// <param name="submeshCount">Number of submeshes (one per material slot).</param>
        public EnvMesh(int submeshCount)
        {
            triangles = new List<int>[submeshCount];
            for (int i = 0; i < submeshCount; i++)
            {
                triangles[i] = new List<int>();
            }
        }

        /// <summary>Gets the number of submeshes.</summary>
        public int SubmeshCount
        {
            get { return triangles.Length; }
        }

        /// <summary>Adds a vertex and returns its index.</summary>
        /// <param name="position">Local position.</param>
        /// <param name="normal">Local normal.</param>
        /// <param name="uv">Texture coordinate.</param>
        /// <returns>The new vertex index.</returns>
        public int AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            vertices.Add(position);
            normals.Add(normal.sqrMagnitude < 1e-8f ? Vector3.up : normal.normalized);
            uvs.Add(uv);
            return vertices.Count - 1;
        }

        /// <summary>Adds a triangle exactly as given.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First vertex.</param>
        /// <param name="b">Second vertex.</param>
        /// <param name="c">Third vertex.</param>
        public void AddTriangle(int sub, int a, int b, int c)
        {
            triangles[sub].Add(a);
            triangles[sub].Add(b);
            triangles[sub].Add(c);
        }

        /// <summary>Adds a quad exactly as given.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        /// <param name="d">Fourth corner.</param>
        public void AddQuad(int sub, int a, int b, int c, int d)
        {
            AddTriangle(sub, a, b, c);
            AddTriangle(sub, a, c, d);
        }

        /// <summary>Adds a quad whose winding is flipped when needed so it faces along the vertex normals.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        /// <param name="d">Fourth corner.</param>
        public void AddOrientedQuad(int sub, int a, int b, int c, int d)
        {
            Vector3 n = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
            if (n.sqrMagnitude < 1e-12f)
            {
                n = Vector3.Cross(vertices[c] - vertices[a], vertices[d] - vertices[a]);
            }

            Vector3 reference = normals[a] + normals[b] + normals[c] + normals[d];
            if (Vector3.Dot(n, reference) >= 0f)
            {
                AddQuad(sub, a, b, c, d);
            }
            else
            {
                AddQuad(sub, a, d, c, b);
            }
        }

        /// <summary>Adds a triangle whose winding is flipped when needed so it faces along the vertex normals.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        public void AddOrientedTriangle(int sub, int a, int b, int c)
        {
            Vector3 n = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
            if (Vector3.Dot(n, normals[a] + normals[b] + normals[c]) >= 0f)
            {
                AddTriangle(sub, a, b, c);
            }
            else
            {
                AddTriangle(sub, a, c, b);
            }
        }

        /// <summary>Adds a flat-shaded triangle facing away from (or toward) a hint point, with box-projected UVs.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        /// <param name="hint">Interior reference point.</param>
        /// <param name="inward">When true the face looks toward the hint instead of away.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        public void AddFlatTriangle(int sub, Vector3 a, Vector3 b, Vector3 c, Vector3 hint, bool inward, float uvScale)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            Vector3 center = (a + b + c) / 3f;
            bool facesAway = Vector3.Dot(n, center - hint) > 0f;
            if (facesAway == inward)
            {
                Vector3 t = b;
                b = c;
                c = t;
                n = -n;
            }

            n.Normalize();
            int ia = AddVertex(a, n, ProjectUv(a, n, uvScale));
            int ib = AddVertex(b, n, ProjectUv(b, n, uvScale));
            int ic = AddVertex(c, n, ProjectUv(c, n, uvScale));
            AddTriangle(sub, ia, ib, ic);
        }

        /// <summary>Adds a flat-shaded quad facing away from (or toward) a hint point.</summary>
        /// <param name="sub">Submesh index.</param>
        /// <param name="a">First corner.</param>
        /// <param name="b">Second corner.</param>
        /// <param name="c">Third corner.</param>
        /// <param name="d">Fourth corner.</param>
        /// <param name="hint">Interior reference point.</param>
        /// <param name="inward">When true the face looks toward the hint instead of away.</param>
        /// <param name="uvScale">World size covered by one texture repeat.</param>
        public void AddFlatQuad(int sub, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 hint, bool inward, float uvScale)
        {
            AddFlatTriangle(sub, a, b, c, hint, inward, uvScale);
            AddFlatTriangle(sub, a, c, d, hint, inward, uvScale);
        }

        /// <summary>Bends the mesh by shearing vertices toward a top offset, quadratically with height.</summary>
        /// <param name="height">Reference height.</param>
        /// <param name="topOffset">Offset applied at the reference height.</param>
        public void Bend(float height, Vector3 topOffset)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                float t = Mathf.Clamp01(vertices[i].y / height);
                vertices[i] += topOffset * (t * t);
            }
        }

        /// <summary>Builds the Unity mesh.</summary>
        /// <param name="name">Mesh name.</param>
        /// <param name="alignToGround">When true the lowest vertex is moved to y = 0.</param>
        /// <returns>A new mesh.</returns>
        public Mesh ToMesh(string name, bool alignToGround)
        {
            float shift = 0f;
            if (alignToGround && vertices.Count > 0)
            {
                float min = float.MaxValue;
                foreach (Vector3 v in vertices)
                {
                    min = Mathf.Min(min, v.y);
                }

                shift = -min;
            }

            var shifted = new List<Vector3>(vertices.Count);
            foreach (Vector3 v in vertices)
            {
                shifted.Add(new Vector3(v.x, v.y + shift, v.z));
            }

            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(shifted);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++)
            {
                mesh.SetTriangles(triangles[i], i);
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Vector2 ProjectUv(Vector3 p, Vector3 n, float scale)
        {
            Vector3 an = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
            if (an.y >= an.x && an.y >= an.z)
            {
                return new Vector2(p.x, p.z) / scale;
            }

            return an.x >= an.z ? new Vector2(p.z, p.y) / scale : new Vector2(p.x, p.y) / scale;
        }
    }
}
