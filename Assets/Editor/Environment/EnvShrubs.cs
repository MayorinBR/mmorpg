using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Describes the silhouette of a low shrub built from overlapping leaf-card lobes.</summary>
    public sealed class ShrubSpec
    {
        /// <summary>Creates a shrub description.</summary>
        /// <param name="seed">Seed for deterministic output.</param>
        /// <param name="radius">Horizontal radius of a single lobe.</param>
        /// <param name="height">Total height of a lobe.</param>
        /// <param name="lobes">Number of overlapping lobes.</param>
        /// <param name="spread">Distance of the outer lobes from the center, relative to the radius.</param>
        /// <param name="cardsPerLobe">Number of leaf cards in each lobe.</param>
        public ShrubSpec(int seed, float radius, float height, int lobes, float spread, int cardsPerLobe = 16)
        {
            Seed = seed;
            Radius = radius;
            Height = height;
            Lobes = lobes;
            Spread = spread;
            CardsPerLobe = cardsPerLobe;
        }

        /// <summary>Gets the random seed.</summary>
        public int Seed { get; }

        /// <summary>Gets the horizontal radius of a single lobe.</summary>
        public float Radius { get; }

        /// <summary>Gets the total height of a lobe.</summary>
        public float Height { get; }

        /// <summary>Gets the number of overlapping lobes.</summary>
        public int Lobes { get; }

        /// <summary>Gets the distance of the outer lobes from the center, relative to the radius.</summary>
        public float Spread { get; }

        /// <summary>Gets the number of leaf cards in each lobe.</summary>
        public int CardsPerLobe { get; }
    }

    /// <summary>Geometry builders for low shrubs and bushes.</summary>
    public static class EnvShrubs
    {
        private const float CardMinFactor = 0.6f;
        private const float CardMaxFactor = 0.85f;
        private const float UpBias = 0.35f;

        /// <summary>Builds a shrub as a central lobe surrounded by smaller ones.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="leaf">Leaf card submesh.</param>
        /// <param name="spec">Shrub silhouette.</param>
        public static void Shrub(EnvMesh m, int leaf, ShrubSpec spec)
        {
            var rng = new Rng(spec.Seed);
            for (int i = 0; i < spec.Lobes; i++)
            {
                bool center = i == 0;
                float scale = center ? 1f : rng.Range(0.6f, 0.85f);
                float angle = (i - 1) * Mathf.PI * 2f / Mathf.Max(1, spec.Lobes - 1) + rng.Range(-0.4f, 0.4f);
                float distance = center ? 0f : spec.Radius * spec.Spread * rng.Range(0.85f, 1.15f);
                float rx = spec.Radius * scale;
                float ry = spec.Height * 0.5f * scale;
                Vector3 position = new Vector3(Mathf.Cos(angle) * distance, ry * 0.9f, Mathf.Sin(angle) * distance);
                Vector3 shading = new Vector3(0f, -spec.Height * 0.4f, 0f);
                EnvShapes.LeafCluster(m, leaf, position, new Vector3(rx, ry, rx), spec.CardsPerLobe, rx * CardMinFactor, rx * CardMaxFactor, shading, UpBias, rng);
            }
        }
    }
}
