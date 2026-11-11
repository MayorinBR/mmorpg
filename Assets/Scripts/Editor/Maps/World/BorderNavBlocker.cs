using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>Marks a strip of the walkable map borders as not walkable, leaving gaps at the portals.</summary>
    public sealed class BorderNavBlocker
    {
        private const float Strip = 6f;
        private const float BarrierStrip = 16f;
        private const float GateClearance = 15f;
        private const int SamplesPerSide = 80;
        private const float Margin = 1f;
        private const int NotWalkableArea = 1;

        private static readonly Type VolumeType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Unity.AI.Navigation.NavMeshModifierVolume"))
            .FirstOrDefault(type => type != null);

        private readonly float floor;
        private readonly float ceiling;

        /// <summary>Initializes a new instance of the <see cref="BorderNavBlocker"/> class.</summary>
        /// <param name="floor">Lowest world height of the volumes.</param>
        /// <param name="ceiling">Highest world height of the volumes.</param>
        public BorderNavBlocker(float floor, float ceiling)
        {
            this.floor = floor;
            this.ceiling = ceiling;
        }

        /// <summary>Creates the blocking volumes.</summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="walk">Border walkability.</param>
        /// <param name="gates">Local XZ positions of the portals that must stay reachable.</param>
        /// <param name="northBarrier">True when the north border is a forest wall.</param>
        /// <returns>Number of volumes created.</returns>
        public int Build(Transform parent, WorldWalkData walk, IReadOnlyList<Vector2> gates, bool northBarrier)
        {
            if (VolumeType == null)
            {
                Debug.LogError("BorderNavBlocker: NavMeshModifierVolume is not available.");
                return 0;
            }

            int created = Side(parent, walk.N, 'N', gates, northBarrier ? BarrierStrip : Strip);
            created += Side(parent, walk.E, 'E', gates, Strip);
            created += Side(parent, walk.S, 'S', gates, Strip);
            created += Side(parent, walk.W, 'W', gates, Strip);
            return created;
        }

        private static float Step => MapMath.Size / SamplesPerSide;

        private static Vector2 Point(char side, int index)
        {
            float t = index * Step;
            switch (side)
            {
                case 'N':
                    return new Vector2(-MapMath.Half + t, MapMath.Half);
                case 'S':
                    return new Vector2(-MapMath.Half + t, -MapMath.Half);
                case 'W':
                    return new Vector2(-MapMath.Half, MapMath.Half - t);
                default:
                    return new Vector2(MapMath.Half, MapMath.Half - t);
            }
        }

        private int Side(Transform parent, string walk, char side, IReadOnlyList<Vector2> gates, float strip)
        {
            int created = 0;
            int start = -1;
            for (int i = 0; i <= walk.Length; i++)
            {
                bool blocked = i < walk.Length && walk[i] == '1' && !gates.Any(gate => Vector2.Distance(gate, Point(side, i)) < GateClearance);
                if (blocked && start < 0)
                {
                    start = i;
                }
                else if (!blocked && start >= 0)
                {
                    Create(parent, side, start, i - 1, strip);
                    created++;
                    start = -1;
                }
            }

            return created;
        }

        private void Create(Transform parent, char side, int from, int to, float strip)
        {
            Vector2 a = Point(side, from);
            Vector2 b = Point(side, to);
            Vector2 inward = side == 'N' ? Vector2.down : side == 'S' ? Vector2.up : side == 'W' ? Vector2.right : Vector2.left;
            Vector2 center = ((a + b) * 0.5f) + (inward * ((strip * 0.5f) - Margin));
            float length = Vector2.Distance(a, b) + (Step * 2f);
            bool horizontal = side == 'N' || side == 'S';
            var volume = new GameObject("BorderBlock_" + side);
            volume.transform.SetParent(parent, false);
            volume.transform.position = new Vector3(center.x, (floor + ceiling) * 0.5f, center.y);
            Component component = volume.AddComponent(VolumeType);
            Vector3 size = horizontal
                ? new Vector3(length, ceiling - floor, strip + (Margin * 2f))
                : new Vector3(strip + (Margin * 2f), ceiling - floor, length);
            VolumeType.GetProperty("size").SetValue(component, size);
            VolumeType.GetProperty("area").SetValue(component, NotWalkableArea);
        }
    }
}
