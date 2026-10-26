using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// How a batch of scenery is accepted and instanced.
    /// </summary>
    public sealed class ScatterRule
    {
        /// <summary>Gets or sets the test a ground point must pass; null accepts every point.</summary>
        public Func<Vector3, bool> Accept { get; set; }

        /// <summary>Gets or sets the list of already used points, which also receives the new ones; null disables spacing.</summary>
        public List<Vector3> Placed { get; set; }

        /// <summary>Gets or sets the minimum ground distance to any point in <see cref="Placed"/>.</summary>
        public float Spacing { get; set; }

        /// <summary>Gets or sets the smallest uniform scale.</summary>
        public float MinScale { get; set; } = 0.9f;

        /// <summary>Gets or sets the largest uniform scale.</summary>
        public float MaxScale { get; set; } = 1.2f;

        /// <summary>Gets or sets the layer of the instances; decoration layers keep them out of the NavMesh.</summary>
        public int Layer { get; set; }
    }

    /// <summary>
    /// Instances prefabs at random ground points of a terrain, with a random heading and scale.
    /// </summary>
    public sealed class SceneryScatterer
    {
        private const int FieldAttempts = 8000;
        private const int AroundAttempts = 40;
        private const float EdgeMargin = 2f;

        private readonly TerrainFootprint terrain;
        private readonly Transform root;

        /// <summary>
        /// Creates a scatterer.
        /// </summary>
        /// <param name="terrain">The terrain that supplies ground heights.</param>
        /// <param name="root">Parent for the generated groups, expected at the world origin.</param>
        /// <param name="seed">Seed that makes the layout repeatable.</param>
        public SceneryScatterer(TerrainFootprint terrain, Transform root, int seed)
        {
            this.terrain = terrain;
            this.root = root;
            Random = new System.Random(seed);
        }

        /// <summary>Gets the random source, shared so a layout stays repeatable.</summary>
        public System.Random Random { get; }

        /// <summary>
        /// Creates an empty child group under the root.
        /// </summary>
        /// <param name="name">Group name.</param>
        /// <returns>The group transform.</returns>
        public Transform CreateGroup(string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(root, false);
            return group;
        }

        /// <summary>
        /// Gets a random point on the ground surface, away from the terrain edge.
        /// </summary>
        /// <returns>A surface point.</returns>
        public Vector3 RandomPoint()
        {
            var x = Mathf.Lerp(terrain.Min.x + EdgeMargin, terrain.Max.x - EdgeMargin, (float)Random.NextDouble());
            var z = Mathf.Lerp(terrain.Min.z + EdgeMargin, terrain.Max.z - EdgeMargin, (float)Random.NextDouble());
            return terrain.Surface(x, z);
        }

        /// <summary>
        /// Places up to a number of instances at random accepted points.
        /// </summary>
        /// <param name="group">Parent group.</param>
        /// <param name="prefabs">Candidates, one picked at random per instance.</param>
        /// <param name="count">Instances to place.</param>
        /// <param name="rule">Acceptance and instancing rule.</param>
        public void Field(Transform group, IReadOnlyList<GameObject> prefabs, int count, ScatterRule rule)
        {
            if (prefabs.Count == 0)
            {
                return;
            }

            var made = 0;

            for (var attempt = 0; attempt < FieldAttempts && made < count; attempt++)
            {
                if (TryPlace(group, prefabs, RandomPoint(), rule))
                {
                    made++;
                }
            }
        }

        /// <summary>
        /// Places instances in discs around given centers.
        /// </summary>
        /// <param name="group">Parent group.</param>
        /// <param name="prefabs">Candidates, one picked at random per instance.</param>
        /// <param name="centers">Disc centers.</param>
        /// <param name="perCenter">Instances per center.</param>
        /// <param name="radius">Disc radius in meters.</param>
        /// <param name="rule">Acceptance and instancing rule.</param>
        public void Around(Transform group, IReadOnlyList<GameObject> prefabs, IEnumerable<Vector3> centers, int perCenter, float radius, ScatterRule rule)
        {
            if (prefabs.Count == 0)
            {
                return;
            }

            foreach (var center in centers)
            {
                var made = 0;

                for (var attempt = 0; attempt < AroundAttempts && made < perCenter; attempt++)
                {
                    var angle = (float)Random.NextDouble() * Mathf.PI * 2f;
                    var distance = Mathf.Sqrt((float)Random.NextDouble()) * radius;
                    var point = terrain.Surface(center.x + Mathf.Cos(angle) * distance, center.z + Mathf.Sin(angle) * distance);

                    if (TryPlace(group, prefabs, point, rule))
                    {
                        made++;
                    }
                }
            }
        }

        /// <summary>
        /// Places one instance at a point when the rule accepts it.
        /// </summary>
        /// <param name="group">Parent group.</param>
        /// <param name="prefabs">Candidates, one picked at random.</param>
        /// <param name="point">Ground point.</param>
        /// <param name="rule">Acceptance and instancing rule.</param>
        /// <returns>True when an instance was placed.</returns>
        public bool TryPlace(Transform group, IReadOnlyList<GameObject> prefabs, Vector3 point, ScatterRule rule)
        {
            if ((rule.Accept != null && !rule.Accept(point))
                || (rule.Placed != null && DistanceToNearest(point, rule.Placed) < rule.Spacing))
            {
                return false;
            }

            rule.Placed?.Add(point);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[Random.Next(prefabs.Count)], group);
            instance.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, (float)Random.NextDouble() * 360f, 0f));
            instance.transform.localScale *= Mathf.Lerp(rule.MinScale, rule.MaxScale, (float)Random.NextDouble());

            if (rule.Layer != 0)
            {
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = rule.Layer;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the ground distance from a point to the nearest of several points.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <param name="others">Points to compare against.</param>
        /// <returns>Distance in meters, or a very large value when the list is empty.</returns>
        public static float DistanceToNearest(Vector3 point, List<Vector3> others)
        {
            var nearest = float.MaxValue;

            foreach (var other in others)
            {
                nearest = Mathf.Min(nearest, new Vector2(point.x - other.x, point.z - other.z).magnitude);
            }

            return nearest;
        }
    }
}
