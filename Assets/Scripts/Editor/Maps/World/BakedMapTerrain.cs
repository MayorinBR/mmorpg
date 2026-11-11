using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>Serves the relief and splat baked for one map to <see cref="MapTerrainWriter"/>.</summary>
    public sealed class BakedMapTerrain : IMapTerrain
    {
        private const int LayerCount = 8;

        private readonly WorldCellData data;

        /// <summary>Initializes a new instance of the <see cref="BakedMapTerrain"/> class.</summary>
        /// <param name="data">The baked map.</param>
        /// <param name="layerNames">Terrain layer asset names in splat order.</param>
        public BakedMapTerrain(WorldCellData data, string[] layerNames)
        {
            this.data = data;
            LayerNames = layerNames;
        }

        /// <inheritdoc/>
        public string[] LayerNames { get; }

        /// <inheritdoc/>
        public float OriginY => data.Info.lo;

        /// <inheritdoc/>
        public float MaxHeight => data.Info.size;

        /// <inheritdoc/>
        public float HeightAt(float x, float z)
        {
            return Bilinear(
                (x + MapMath.Half) / MapMath.Size * (WorldCellData.HeightSamples - 1),
                (z + MapMath.Half) / MapMath.Size * (WorldCellData.HeightSamples - 1)) * MaxHeight;
        }

        /// <summary>Gets the height above sea zero at a local position.</summary>
        /// <param name="point">Local XZ position.</param>
        /// <returns>World height in meters.</returns>
        public float WorldHeight(Vector2 point)
        {
            return OriginY + HeightAt(point.x, point.y);
        }

        /// <inheritdoc/>
        public float[] LayerWeights(Vector2 world, float slope)
        {
            float fx = Mathf.Clamp(((world.x + MapMath.Half) / MapMath.Size * WorldCellData.SplatSamples) - 0.5f, 0f, WorldCellData.SplatSamples - 1f);
            float fz = Mathf.Clamp(((world.y + MapMath.Half) / MapMath.Size * WorldCellData.SplatSamples) - 0.5f, 0f, WorldCellData.SplatSamples - 1f);
            return SampleSplat(fx, fz);
        }

        /// <summary>Samples the splat at fractional texel coordinates.</summary>
        /// <param name="fx">Texel X.</param>
        /// <param name="fz">Texel Z.</param>
        /// <returns>One weight per baked layer.</returns>
        public float[] SampleSplat(float fx, float fz)
        {
            int x0 = Mathf.Min((int)fx, WorldCellData.SplatSamples - 2);
            int z0 = Mathf.Min((int)fz, WorldCellData.SplatSamples - 2);
            float tx = fx - x0;
            float tz = fz - z0;
            var weights = new float[LayerCount];
            float sum = 0f;
            for (int layer = 0; layer < LayerCount; layer++)
            {
                float a = Texel(x0, z0, layer);
                float b = Texel(x0 + 1, z0, layer);
                float c = Texel(x0, z0 + 1, layer);
                float d = Texel(x0 + 1, z0 + 1, layer);
                weights[layer] = Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz);
                sum += weights[layer];
            }

            if (sum > 1e-5f)
            {
                for (int layer = 0; layer < LayerCount; layer++)
                {
                    weights[layer] /= sum;
                }
            }

            return weights;
        }

        private float Texel(int x, int z, int layer)
        {
            return data.Splat[(((z * WorldCellData.SplatSamples) + x) * LayerCount) + layer] / 255f;
        }

        private float Bilinear(float fx, float fz)
        {
            int n = WorldCellData.HeightSamples;
            fx = Mathf.Clamp(fx, 0f, n - 1f);
            fz = Mathf.Clamp(fz, 0f, n - 1f);
            int x0 = Mathf.Min((int)fx, n - 2);
            int z0 = Mathf.Min((int)fz, n - 2);
            float tx = fx - x0;
            float tz = fz - z0;
            float a = data.Heights[(z0 * n) + x0];
            float b = data.Heights[(z0 * n) + x0 + 1];
            float c = data.Heights[((z0 + 1) * n) + x0];
            float d = data.Heights[((z0 + 1) * n) + x0 + 1];
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), tz) / 65535f;
        }
    }
}
