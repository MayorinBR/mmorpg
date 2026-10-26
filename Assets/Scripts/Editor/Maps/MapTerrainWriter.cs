using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Resizes a map terrain and writes the heights and splat of an <see cref="IMapTerrain"/> into its terrain data asset.</summary>
    public sealed class MapTerrainWriter
    {
        private const int HeightResolution = 513;
        private const int AlphaResolution = 512;
        private const string LayerFolder = "Assets/Terrains/Painted/Layers/";

        /// <summary>Rewrites the terrain, clearing old trees and details, and centers it on the world origin.</summary>
        /// <param name="terrain">The terrain component of the scene.</param>
        /// <param name="shape">Source of heights and splat weights.</param>
        public void Write(Terrain terrain, IMapTerrain shape)
        {
            TerrainData data = terrain.terrainData;
            data.treeInstances = new TreeInstance[0];
            data.detailPrototypes = new DetailPrototype[0];
            data.heightmapResolution = HeightResolution;
            data.size = new Vector3(MapMath.Size, shape.MaxHeight, MapMath.Size);
            data.alphamapResolution = AlphaResolution;
            data.terrainLayers = LoadLayers(shape.LayerNames);

            float[,] heights = BuildHeights(shape);
            data.SetHeights(0, 0, heights);
            data.SetAlphamaps(0, 0, BuildSplat(shape, heights));

            terrain.transform.position = new Vector3(-MapMath.Half, shape.OriginY, -MapMath.Half);
            terrain.Flush();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static TerrainLayer[] LoadLayers(string[] names)
        {
            var layers = new TerrainLayer[names.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(LayerFolder + names[i] + ".terrainlayer");
            }

            return layers;
        }

        private static float[,] BuildHeights(IMapTerrain shape)
        {
            var heights = new float[HeightResolution, HeightResolution];
            float step = MapMath.Size / (HeightResolution - 1);
            for (int z = 0; z < HeightResolution; z++)
            {
                for (int x = 0; x < HeightResolution; x++)
                {
                    float height = shape.HeightAt((x * step) - MapMath.Half, (z * step) - MapMath.Half);
                    heights[z, x] = Mathf.Clamp01(height / shape.MaxHeight);
                }
            }

            return heights;
        }

        private static float[,,] BuildSplat(IMapTerrain shape, float[,] heights)
        {
            int layerCount = shape.LayerNames.Length;
            var weights = new float[AlphaResolution, AlphaResolution, layerCount];
            float texel = MapMath.Size / AlphaResolution;
            for (int z = 0; z < AlphaResolution; z++)
            {
                for (int x = 0; x < AlphaResolution; x++)
                {
                    var world = new Vector2(((x + 0.5f) * texel) - MapMath.Half, ((z + 0.5f) * texel) - MapMath.Half);
                    float[] layer = shape.LayerWeights(world, SlopeAt(heights, x, z, shape.MaxHeight));
                    for (int i = 0; i < layerCount; i++)
                    {
                        weights[z, x, i] = layer[i];
                    }
                }
            }

            return weights;
        }

        private static float SlopeAt(float[,] heights, int x, int z, float maxHeight)
        {
            int hx = Mathf.Clamp(Mathf.RoundToInt(x * (HeightResolution - 1f) / AlphaResolution), 1, HeightResolution - 2);
            int hz = Mathf.Clamp(Mathf.RoundToInt(z * (HeightResolution - 1f) / AlphaResolution), 1, HeightResolution - 2);
            float step = MapMath.Size / (HeightResolution - 1);
            float dx = (heights[hz, hx + 1] - heights[hz, hx - 1]) * maxHeight / (2f * step);
            float dz = (heights[hz + 1, hx] - heights[hz - 1, hx]) * maxHeight / (2f * step);
            return Mathf.Sqrt((dx * dx) + (dz * dz));
        }
    }
}
