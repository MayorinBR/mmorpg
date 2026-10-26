using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Sides of the square map.</summary>
    public enum MapSide
    {
        /// <summary>The side at positive Z.</summary>
        North,

        /// <summary>The side at positive X.</summary>
        East,

        /// <summary>The side at negative Z.</summary>
        South,

        /// <summary>The side at negative X.</summary>
        West,
    }

    /// <summary>Closes the foot of a map edge with a continuous, irregular line of boulders and thickets, leaving gaps at the gates.</summary>
    public sealed class PerimeterLineBuilder
    {
        private const float StepFactor = 0.8f;
        private const float LateralJitter = 1.6f;
        private const float YawJitter = 25f;
        private const float MinStep = 2.5f;
        private const float PieceSink = 0.3f;
        private const float PieceFootprint = 3.5f;

        private readonly PrefabLibrary library;
        private readonly PropPlacer placer;
        private readonly Occupancy occupancy;
        private readonly System.Random random;
        private readonly string[] pool;

        /// <summary>Initializes a new instance of the <see cref="PerimeterLineBuilder"/> class.</summary>
        /// <param name="library">Prefab source.</param>
        /// <param name="placer">Instantiation helper.</param>
        /// <param name="occupancy">Footprints shared with the scatter pass.</param>
        /// <param name="random">Deterministic random source.</param>
        /// <param name="pool">Names of the prefabs the line is made of.</param>
        public PerimeterLineBuilder(PrefabLibrary library, PropPlacer placer, Occupancy occupancy, System.Random random, string[] pool)
        {
            this.library = library;
            this.placer = placer;
            this.occupancy = occupancy;
            this.random = random;
            this.pool = pool;
        }

        /// <summary>Builds the line along the chosen sides.</summary>
        /// <param name="parent">Parent of the pieces.</param>
        /// <param name="inset">Distance of the line from the map edge.</param>
        /// <param name="sides">The sides that receive a line.</param>
        /// <param name="gates">Points the line keeps clear of.</param>
        /// <param name="gateRadius">Radius kept clear around each gate.</param>
        /// <returns>The number of pieces placed.</returns>
        public int Build(Transform parent, float inset, IReadOnlyCollection<MapSide> sides, IReadOnlyList<Vector2> gates, float gateRadius)
        {
            float c = MapMath.Half - inset;
            int placed = 0;
            foreach (MapSide side in sides)
            {
                switch (side)
                {
                    case MapSide.North:
                        placed += PlaceLine(new Vector2(-c, c), new Vector2(c, c), 0f, parent, gates, gateRadius);
                        break;
                    case MapSide.East:
                        placed += PlaceLine(new Vector2(c, c), new Vector2(c, -c), 90f, parent, gates, gateRadius);
                        break;
                    case MapSide.South:
                        placed += PlaceLine(new Vector2(c, -c), new Vector2(-c, -c), 180f, parent, gates, gateRadius);
                        break;
                    default:
                        placed += PlaceLine(new Vector2(-c, -c), new Vector2(-c, c), 270f, parent, gates, gateRadius);
                        break;
                }
            }

            return placed;
        }

        private static float Width(GameObject prefab)
        {
            return prefab.GetComponentInChildren<MeshFilter>().sharedMesh.bounds.size.x;
        }

        private int PlaceLine(Vector2 start, Vector2 end, float yaw, Transform parent, IReadOnlyList<Vector2> gates, float gateRadius)
        {
            Vector2 direction = (end - start).normalized;
            var side = new Vector2(-direction.y, direction.x);
            float length = Vector2.Distance(start, end);
            float cursor = 0f;
            int placed = 0;
            while (cursor < length)
            {
                string name = pool[random.Next(pool.Length)];
                GameObject prefab = library.Get(name);
                float step = Mathf.Max(MinStep, Width(prefab) * StepFactor);
                Vector2 point = start + (direction * (cursor + (step * 0.5f))) + (side * Range(-LateralJitter, LateralJitter));
                float jitter = Range(-YawJitter, YawJitter);
                float scale = Range(0.95f, 1.25f);
                cursor += step;
                if (gates.Any(gate => Vector2.Distance(gate, point) < gateRadius))
                {
                    continue;
                }

                GameObject piece = placer.Place(prefab, parent, point, Quaternion.Euler(0f, yaw + jitter, 0f), Vector3.one * scale, PieceSink, 0);
                piece.name = name;
                occupancy.Add(point, PieceFootprint);
                placed++;
            }

            return placed;
        }

        private float Range(float min, float max)
        {
            return min + ((float)random.NextDouble() * (max - min));
        }
    }
}
