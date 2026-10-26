using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Read-only view of a terrain's extent and surface height, in world space.
    /// </summary>
    public readonly struct TerrainFootprint
    {
        private readonly Terrain terrain;

        /// <summary>
        /// Creates a footprint for a terrain.
        /// </summary>
        /// <param name="terrain">The terrain to measure.</param>
        public TerrainFootprint(Terrain terrain)
        {
            this.terrain = terrain;
        }

        /// <summary>Gets the lowest corner of the terrain in world space.</summary>
        public Vector3 Min => terrain.transform.position;

        /// <summary>Gets the highest corner of the terrain in world space.</summary>
        public Vector3 Max => Min + terrain.terrainData.size;

        /// <summary>Gets the shorter side of the terrain on the ground plane.</summary>
        public float ShortSide => Mathf.Min(Max.x - Min.x, Max.z - Min.z);

        /// <summary>
        /// Gets the world-space height of the terrain surface at a ground position.
        /// </summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>World Y of the surface.</returns>
        public float SurfaceY(float x, float z)
        {
            return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
        }

        /// <summary>
        /// Gets the point on the terrain surface at a ground position.
        /// </summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>The surface point.</returns>
        public Vector3 Surface(float x, float z)
        {
            return new Vector3(x, SurfaceY(x, z), z);
        }

        /// <summary>
        /// Gets the distance from a ground position to the nearest terrain edge.
        /// </summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>Distance in meters, negative outside the terrain.</returns>
        public float DistanceToEdge(float x, float z)
        {
            return Mathf.Min(x - Min.x, Max.x - x, z - Min.z, Max.z - z);
        }
    }
}
