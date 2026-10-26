using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Describes how many instances of a set of prefabs to scatter and where they are allowed to appear.</summary>
    public sealed class ScatterRule
    {
        /// <summary>Gets or sets the names of the prefabs to choose from.</summary>
        public string[] Prefabs { get; set; }

        /// <summary>Gets or sets the number of instances to place.</summary>
        public int Count { get; set; }

        /// <summary>Gets or sets the footprint radius used to keep instances apart.</summary>
        public float Radius { get; set; } = 1f;

        /// <summary>Gets or sets the minimum and maximum uniform scale.</summary>
        public Vector2 ScaleRange { get; set; } = Vector2.one;

        /// <summary>Gets or sets the minimum distance to any trail.</summary>
        public float TrailClearance { get; set; }

        /// <summary>Gets or sets the minimum distance to the terrain edge.</summary>
        public float EdgeMargin { get; set; } = 11f;

        /// <summary>Gets or sets the probability function over the map, from 0 to 1.</summary>
        public Func<Vector2, float> Density { get; set; } = point => 1f;

        /// <summary>Gets or sets the layer given to every placed object.</summary>
        public int Layer { get; set; }

        /// <summary>Gets or sets a value indicating whether instances reserve their footprint for later placements.</summary>
        public bool Solid { get; set; } = true;

        /// <summary>Gets or sets the maximum random tilt in degrees.</summary>
        public float Tilt { get; set; }
    }

    /// <summary>Instantiates prefabs on the terrain surface with consistent layer and static settings.</summary>
    public sealed class PropPlacer
    {
        private readonly Terrain terrain;

        /// <summary>Initializes a new instance of the <see cref="PropPlacer"/> class.</summary>
        /// <param name="terrain">The terrain props are placed on.</param>
        public PropPlacer(Terrain terrain)
        {
            this.terrain = terrain;
        }

        /// <summary>Gets the world height of the terrain surface below a point.</summary>
        /// <param name="point">Point on the XZ plane.</param>
        /// <returns>The world Y of the surface.</returns>
        public float GroundAt(Vector2 point)
        {
            return terrain.SampleHeight(new Vector3(point.x, 0f, point.y)) + terrain.transform.position.y;
        }

        /// <summary>Places a prefab instance on the ground.</summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent transform.</param>
        /// <param name="point">Position on the XZ plane.</param>
        /// <param name="rotation">Rotation of the instance.</param>
        /// <param name="scale">Local scale.</param>
        /// <param name="sink">Distance the instance is buried below the surface.</param>
        /// <param name="layer">Layer of the instance and its children.</param>
        /// <returns>The new instance.</returns>
        public GameObject Place(GameObject prefab, Transform parent, Vector2 point, Quaternion rotation, Vector3 scale, float sink, int layer)
        {
            return Place(prefab, parent, new Vector3(point.x, GroundAt(point) - sink, point.y), rotation, scale, layer);
        }

        /// <summary>Places a prefab instance at an exact world position.</summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent transform.</param>
        /// <param name="position">World position.</param>
        /// <param name="rotation">Rotation of the instance.</param>
        /// <param name="scale">Local scale.</param>
        /// <param name="layer">Layer of the instance and its children.</param>
        /// <returns>The new instance.</returns>
        public GameObject Place(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, int layer)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.localScale = scale;
            foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            }

            return instance;
        }
    }

    /// <summary>Scatters props over the map following <see cref="ScatterRule"/> definitions.</summary>
    public sealed class Scatterer
    {
        private const int AttemptsPerInstance = 250;

        private readonly PrefabLibrary library;
        private readonly PropPlacer placer;
        private readonly TrailNetwork trails;
        private readonly Occupancy occupancy;
        private readonly System.Random random;

        /// <summary>Initializes a new instance of the <see cref="Scatterer"/> class.</summary>
        /// <param name="library">Prefab source.</param>
        /// <param name="placer">Instantiation helper.</param>
        /// <param name="trails">Trails that props keep clear of.</param>
        /// <param name="occupancy">Footprints shared by all placements.</param>
        /// <param name="random">Deterministic random source.</param>
        public Scatterer(PrefabLibrary library, PropPlacer placer, TrailNetwork trails, Occupancy occupancy, System.Random random)
        {
            this.library = library;
            this.placer = placer;
            this.trails = trails;
            this.occupancy = occupancy;
            this.random = random;
        }

        /// <summary>Places the instances of a rule under a parent.</summary>
        /// <param name="rule">The rule to apply.</param>
        /// <param name="parent">The parent transform.</param>
        /// <returns>The number of instances placed.</returns>
        public int Run(ScatterRule rule, Transform parent)
        {
            List<GameObject> prefabs = rule.Prefabs.Select(library.Get).ToList();
            float limit = MapMath.Half - rule.EdgeMargin;
            int placed = 0;
            for (int attempt = 0; attempt < rule.Count * AttemptsPerInstance && placed < rule.Count; attempt++)
            {
                var point = new Vector2(Range(-limit, limit), Range(-limit, limit));
                if (Range(0f, 1f) > rule.Density(point) || trails.DistanceTo(point) < rule.TrailClearance || !occupancy.IsFree(point, rule.Radius))
                {
                    continue;
                }

                float scale = Range(rule.ScaleRange.x, rule.ScaleRange.y);
                Quaternion rotation = Quaternion.Euler(Range(-rule.Tilt, rule.Tilt), Range(0f, 360f), Range(-rule.Tilt, rule.Tilt));
                placer.Place(prefabs[random.Next(prefabs.Count)], parent, point, rotation, Vector3.one * scale, 0.05f, rule.Layer);
                if (rule.Solid)
                {
                    occupancy.Add(point, rule.Radius * scale);
                }

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
