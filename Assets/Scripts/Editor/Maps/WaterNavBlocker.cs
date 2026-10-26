using System;
using System.Linq;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>
    /// Makes water impassable for the NavMesh by covering every wet stretch of the terrain with not walkable modifier volumes.
    /// Volumes end below the deck of bridges and piers, so those stay walkable.
    /// </summary>
    public sealed class WaterNavBlocker
    {
        private const float RowDepth = 3f;
        private const float ScanStep = 1f;
        private const float Margin = 0.6f;
        private const float Floor = -9f;
        private const int NotWalkableArea = 1;

        private static readonly Type VolumeType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Unity.AI.Navigation.NavMeshModifierVolume"))
            .FirstOrDefault(type => type != null);

        private readonly Func<Vector2, float> groundAt;
        private readonly float waterThreshold;
        private readonly float ceiling;

        /// <summary>Initializes a new instance of the <see cref="WaterNavBlocker"/> class.</summary>
        /// <param name="groundAt">Gives the world height of the terrain at a point.</param>
        /// <param name="waterThreshold">Terrain below this world height counts as wet.</param>
        /// <param name="ceiling">World height of the top of every volume.</param>
        public WaterNavBlocker(Func<Vector2, float> groundAt, float waterThreshold, float ceiling)
        {
            this.groundAt = groundAt;
            this.waterThreshold = waterThreshold;
            this.ceiling = ceiling;
        }

        /// <summary>Creates the volumes under a parent.</summary>
        /// <param name="parent">Parent of the volumes.</param>
        /// <returns>The number of volumes created.</returns>
        public int Build(Transform parent)
        {
            if (VolumeType == null)
            {
                Debug.LogError("WaterNavBlocker: NavMeshModifierVolume is not available, water stays walkable.");
                return 0;
            }

            int created = 0;
            for (float z = -MapMath.Half + (RowDepth * 0.5f); z < MapMath.Half; z += RowDepth)
            {
                created += BuildRow(parent, z);
            }

            return created;
        }

        private int BuildRow(Transform parent, float z)
        {
            int created = 0;
            float runStart = float.NaN;
            for (float x = -MapMath.Half; x <= MapMath.Half + ScanStep; x += ScanStep)
            {
                bool wet = groundAt(new Vector2(x, z)) < waterThreshold;
                if (wet && float.IsNaN(runStart))
                {
                    runStart = x;
                }
                else if (!wet && !float.IsNaN(runStart))
                {
                    CreateVolume(parent, runStart, x, z);
                    created++;
                    runStart = float.NaN;
                }
            }

            if (!float.IsNaN(runStart))
            {
                CreateVolume(parent, runStart, MapMath.Half + Margin, z);
                created++;
            }

            return created;
        }

        private void CreateVolume(Transform parent, float from, float to, float z)
        {
            float start = from - Margin;
            float end = to + Margin;
            var volume = new GameObject("WaterBlock");
            volume.transform.SetParent(parent, false);
            volume.transform.position = new Vector3((start + end) * 0.5f, (ceiling + Floor) * 0.5f, z);
            Component component = volume.AddComponent(VolumeType);
            VolumeType.GetProperty("size").SetValue(component, new Vector3(end - start, ceiling - Floor, RowDepth + 0.2f));
            VolumeType.GetProperty("area").SetValue(component, NotWalkableArea);
        }
    }
}
