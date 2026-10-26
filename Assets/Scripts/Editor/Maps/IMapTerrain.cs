namespace Project.EditorTools.Maps
{
    /// <summary>Describes the relief and painting of one 160 m map terrain.</summary>
    public interface IMapTerrain
    {
        /// <summary>Gets the names of the terrain layers, in splat order, found in the painted layers folder.</summary>
        string[] LayerNames { get; }

        /// <summary>Gets the world height of the terrain origin.</summary>
        float OriginY { get; }

        /// <summary>Gets the terrain height range in meters.</summary>
        float MaxHeight { get; }

        /// <summary>Gets the height above the terrain origin at a world position.</summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>Height in meters above the terrain origin.</returns>
        float HeightAt(float x, float z);

        /// <summary>Gets the normalized splat weights at a world position.</summary>
        /// <param name="world">Position on the XZ plane.</param>
        /// <param name="slope">Rise over run of the surface.</param>
        /// <returns>One weight per layer, summing to one.</returns>
        float[] LayerWeights(UnityEngine.Vector2 world, float slope);
    }
}
