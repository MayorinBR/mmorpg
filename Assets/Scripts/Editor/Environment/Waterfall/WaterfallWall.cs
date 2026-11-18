using System;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Probes the terrain wall behind the waterfall: where its surface is and which way it faces.
    /// The wall rises toward +Z, so the surface at a given height is found by walking north.
    /// </summary>
    public sealed class WaterfallWall
    {
        private const float SearchStartZ = 72f;
        private const float SearchEndZ = 110f;
        private const float SearchStep = 0.05f;
        private const int RefineSteps = 12;

        private readonly Terrain terrain;

        /// <summary>
        /// Creates a probe for the given terrain.
        /// </summary>
        /// <param name="terrain">Terrain that holds the wall.</param>
        public WaterfallWall(Terrain terrain)
        {
            this.terrain = terrain;
        }

        /// <summary>Returns the world height of the terrain surface at a world position.</summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>World height of the surface.</returns>
        public float HeightAt(float x, float z)
        {
            return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
        }

        /// <summary>Finds the world Z at which the wall surface reaches a height.</summary>
        /// <param name="x">World X.</param>
        /// <param name="y">World height to look for.</param>
        /// <returns>World Z of the surface at that height.</returns>
        /// <exception cref="InvalidOperationException">The wall never reaches that height.</exception>
        public float SurfaceZ(float x, float y)
        {
            var z = SearchStartZ;
            while (z < SearchEndZ && HeightAt(x, z) < y)
            {
                z += SearchStep;
            }

            if (z >= SearchEndZ)
            {
                throw new InvalidOperationException($"The terrain never reaches height {y} at x={x}.");
            }

            var low = z - SearchStep;
            var high = z;
            for (var i = 0; i < RefineSteps; i++)
            {
                var middle = (low + high) * 0.5f;
                if (HeightAt(x, middle) < y)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            return high;
        }

        /// <summary>Returns the terrain surface normal at a world position.</summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <returns>Unit normal of the surface.</returns>
        public Vector3 NormalAt(float x, float z)
        {
            var data = terrain.terrainData;
            var origin = terrain.transform.position;
            return data.GetInterpolatedNormal((x - origin.x) / data.size.x, (z - origin.z) / data.size.z);
        }
    }
}
