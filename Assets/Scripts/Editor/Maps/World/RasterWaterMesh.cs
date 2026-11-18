using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>Builds the surface of rivers and lakes from a water level raster.</summary>
    public static class RasterWaterMesh
    {
        private const string WaterMaterialPath = "Assets/Materials/Environment/Plains_Water.mat";
        private const float DryDrop = 0.5f;

        /// <summary>Creates the water object, or returns null when the raster holds no water.</summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="name">Object name.</param>
        /// <param name="levels">Water level in centimeters, 257 x 257, short.MinValue where dry.</param>
        /// <param name="meshPath">Asset path the mesh is saved to.</param>
        /// <param name="layer">Layer of the object.</param>
        /// <returns>The water object or null.</returns>
        public static GameObject Create(Transform parent, string name, short[] levels, string meshPath, int layer)
        {
            int n = WorldCellData.WaterSamples;
            float step = MapMath.Size / (n - 1);
            var heights = new float[n * n];
            var wet = new bool[n * n];
            bool any = false;
            for (int i = 0; i < levels.Length; i++)
            {
                wet[i] = levels[i] != short.MinValue;
                any |= wet[i];
                heights[i] = levels[i] / 100f;
            }

            if (!any)
            {
                return null;
            }

            float[] filled = FillDryLevels(heights, wet, n);
            var vertices = new Vector3[n * n];
            var triangles = new System.Collections.Generic.List<int>();
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    float level = wet[(z * n) + x] ? heights[(z * n) + x] : filled[(z * n) + x] - DryDrop;
                    vertices[(z * n) + x] = new Vector3((x * step) - MapMath.Half, level, (z * step) - MapMath.Half);
                }
            }

            for (int z = 0; z < n - 1; z++)
            {
                for (int x = 0; x < n - 1; x++)
                {
                    int a = (z * n) + x;
                    int b = a + 1;
                    int c = a + n;
                    int d = c + 1;
                    if (!(wet[a] || wet[b] || wet[c] || wet[d]))
                    {
                        continue;
                    }

                    triangles.AddRange(new[] { a, c, d, a, d, b });
                }
            }

            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32, vertices = vertices };
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, meshPath);

            var water = new GameObject(name) { layer = layer };
            water.transform.SetParent(parent, false);
            water.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = water.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return water;
        }

        /// <summary>Gets the water level at a local position, or null when dry.</summary>
        /// <param name="levels">The water raster.</param>
        /// <param name="point">Local XZ position.</param>
        /// <returns>Level in meters or null.</returns>
        public static float? LevelAt(short[] levels, Vector2 point)
        {
            int n = WorldCellData.WaterSamples;
            int x = Mathf.Clamp(Mathf.RoundToInt((point.x + MapMath.Half) / MapMath.Size * (n - 1)), 0, n - 1);
            int z = Mathf.Clamp(Mathf.RoundToInt((point.y + MapMath.Half) / MapMath.Size * (n - 1)), 0, n - 1);
            short value = levels[(z * n) + x];
            return value == short.MinValue ? (float?)null : value / 100f;
        }

        /// <summary>Gives every dry sample the lowest level of the wet samples closest to it, growing outward one ring at a time.</summary>
        /// <param name="heights">Level of each wet sample.</param>
        /// <param name="wet">Wet flag of each sample.</param>
        /// <param name="n">Samples per side.</param>
        /// <returns>The level of each sample, with dry samples filled from their nearest wet neighbors.</returns>
        private static float[] FillDryLevels(float[] heights, bool[] wet, int n)
        {
            var result = (float[])heights.Clone();
            var reached = (bool[])wet.Clone();
            var frontier = new System.Collections.Generic.List<int>();
            for (int i = 0; i < wet.Length; i++)
            {
                if (wet[i])
                {
                    frontier.Add(i);
                }
            }

            while (frontier.Count > 0)
            {
                var next = new System.Collections.Generic.List<int>();
                var pending = new System.Collections.Generic.Dictionary<int, float>();
                foreach (int index in frontier)
                {
                    int x = index % n;
                    int z = index / n;
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = x + dx;
                            int zz = z + dz;
                            int neighbor = (zz * n) + xx;
                            if (xx < 0 || zz < 0 || xx >= n || zz >= n || reached[neighbor])
                            {
                                continue;
                            }

                            pending[neighbor] = pending.TryGetValue(neighbor, out float known) ? Mathf.Min(known, result[index]) : result[index];
                        }
                    }
                }

                foreach (System.Collections.Generic.KeyValuePair<int, float> entry in pending)
                {
                    reached[entry.Key] = true;
                    result[entry.Key] = entry.Value;
                    next.Add(entry.Key);
                }

                frontier = next;
            }

            return result;
        }
    }
}
