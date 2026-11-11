using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>
    /// Rewrites the relief of a map that already exists so its borders continue into the neighbors,
    /// keeping the original relief and painting in the interior.
    /// </summary>
    public sealed class ExistingTerrainUpdater
    {
        private static readonly string[][] LayerKeywords =
        {
            new[] { "meadow", "grass", "lawn" },
            new[] { "dry", "grass" },
            new[] { "path", "road", "dirt", "cobble" },
            new[] { "rock", "cliff", "stone" },
            new[] { "beach", "sand" },
            new[] { "desert", "sand" },
            new[] { "dark", "sand" },
            new[] { "soil", "dirt" },
        };

        /// <summary>Applies the baked relief and blends the painting of the border zone.</summary>
        /// <param name="terrain">The terrain of the existing map.</param>
        /// <param name="data">The baked map.</param>
        /// <param name="baked">The baked terrain adapter.</param>
        /// <returns>The area, in local XZ meters, that the terrain covered before the update.</returns>
        public Rect Apply(Terrain terrain, WorldCellData data, BakedMapTerrain baked)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 oldPosition = terrain.transform.position;
            Vector3 oldSize = terrainData.size;
            var oldArea = new Rect(oldPosition.x, oldPosition.z, oldSize.x, oldSize.z);
            int res = terrainData.alphamapResolution;
            float[,,] source = terrainData.GetAlphamaps(0, 0, res, res);
            TreeInstance[] trees = terrainData.treeInstances;
            WriteHeights(terrain, terrainData, data);
            KeepTreesInPlace(terrainData, trees, oldArea);
            BlendSplat(terrainData, data, baked, source, oldArea);
            terrain.Flush();
            EditorUtility.SetDirty(terrainData);
            return oldArea;
        }

        private static void KeepTreesInPlace(TerrainData terrainData, TreeInstance[] trees, Rect oldArea)
        {
            if (trees.Length == 0)
            {
                return;
            }

            for (int i = 0; i < trees.Length; i++)
            {
                Vector3 position = trees[i].position;
                position.x = (oldArea.x + (position.x * oldArea.width) + MapMath.Half) / MapMath.Size;
                position.z = (oldArea.y + (position.z * oldArea.height) + MapMath.Half) / MapMath.Size;
                trees[i].position = position;
            }

            terrainData.SetTreeInstances(trees, true);
        }

        private static void WriteHeights(Terrain terrain, TerrainData terrainData, WorldCellData data)
        {
            int n = terrainData.heightmapResolution;
            var heights = new float[n, n];
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    heights[z, x] = data.Heights[(z * n) + x] / 65535f;
                }
            }

            terrainData.size = new Vector3(MapMath.Size, data.Info.size, MapMath.Size);
            terrainData.SetHeights(0, 0, heights);
            terrain.transform.position = new Vector3(-MapMath.Half, data.Info.lo, -MapMath.Half);
        }

        private static void BlendSplat(TerrainData terrainData, WorldCellData data, BakedMapTerrain baked, float[,,] source, Rect oldArea)
        {
            TerrainLayer[] layers = terrainData.terrainLayers;
            int res = terrainData.alphamapResolution;
            var alpha = new float[res, res, layers.Length];
            int[] map = new int[LayerKeywords.Length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = FindLayer(layers, LayerKeywords[i]);
            }

            float scale = (WorldCellData.SplatSamples - 1f) / (res - 1f);
            var mapped = new float[layers.Length];
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    float keep = KeepAt(data, x * scale, z * scale);
                    float[] weights = baked.SampleSplat(x * scale, z * scale);
                    System.Array.Clear(mapped, 0, mapped.Length);
                    for (int i = 0; i < map.Length; i++)
                    {
                        mapped[map[i]] += weights[i];
                    }

                    float localX = -MapMath.Half + (x / (res - 1f) * MapMath.Size);
                    float localZ = -MapMath.Half + (z / (res - 1f) * MapMath.Size);
                    float sourceX = Mathf.Clamp((localX - oldArea.x) / oldArea.width * (res - 1f), 0f, res - 1f);
                    float sourceZ = Mathf.Clamp((localZ - oldArea.y) / oldArea.height * (res - 1f), 0f, res - 1f);
                    for (int layer = 0; layer < layers.Length; layer++)
                    {
                        float original = Bilinear(source, sourceX, sourceZ, layer, res);
                        alpha[z, x, layer] = (keep * original) + ((1f - keep) * mapped[layer]);
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, alpha);
        }

        private static float Bilinear(float[,,] source, float fx, float fz, int layer, int res)
        {
            int x0 = Mathf.Min((int)fx, res - 2);
            int z0 = Mathf.Min((int)fz, res - 2);
            float tx = fx - x0;
            float tz = fz - z0;
            float a = source[z0, x0, layer];
            float b = source[z0, x0 + 1, layer];
            float c = source[z0 + 1, x0, layer];
            float d = source[z0 + 1, x0 + 1, layer];
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz);
        }

        private static float KeepAt(WorldCellData data, float fx, float fz)
        {
            int n = WorldCellData.SplatSamples;
            int x0 = Mathf.Min((int)fx, n - 2);
            int z0 = Mathf.Min((int)fz, n - 2);
            float tx = fx - x0;
            float tz = fz - z0;
            float a = data.Keep[(z0 * n) + x0];
            float b = data.Keep[(z0 * n) + x0 + 1];
            float c = data.Keep[((z0 + 1) * n) + x0];
            float d = data.Keep[((z0 + 1) * n) + x0 + 1];
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz) / 255f;
        }

        private static int FindLayer(TerrainLayer[] layers, string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i] != null && layers[i].name.ToLowerInvariant().Contains(keyword))
                    {
                        return i;
                    }
                }
            }

            return 0;
        }
    }
}
