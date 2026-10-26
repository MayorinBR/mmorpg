using UnityEngine;
using UnityEditor;

namespace Project.EditorTools
{
    /// <summary>
    /// Measures and positions prefab instances by their rendered bounds, for prefabs whose size and pivot are not known in advance.
    /// </summary>
    public static class PrefabFit
    {
        /// <summary>
        /// Gets the world-space bounds that enclose every renderer of an object.
        /// </summary>
        /// <param name="target">The object to measure.</param>
        /// <returns>The combined bounds, or a unit box at the object when it has no renderer.</returns>
        public static Bounds CombinedBounds(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(target.transform.position, Vector3.one);

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Measures the size a prefab has when instanced unrotated at its own scale.
        /// </summary>
        /// <param name="prefab">The prefab to measure.</param>
        /// <param name="parent">Temporary parent for the probe instance.</param>
        /// <returns>The size of its rendered bounds.</returns>
        public static Vector3 MeasureSize(GameObject prefab, Transform parent)
        {
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            probe.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var size = CombinedBounds(probe).size;
            Object.DestroyImmediate(probe);
            return size;
        }

        /// <summary>
        /// Instances a prefab so the horizontal center of its bounds sits on a point and the bottom of its bounds sits at a height.
        /// </summary>
        /// <param name="prefab">The prefab to instance.</param>
        /// <param name="parent">Parent transform, expected at the world origin.</param>
        /// <param name="center">World X and Z the bounds are centered on.</param>
        /// <param name="bottomY">World Y of the lowest point of the bounds.</param>
        /// <param name="rotation">World rotation.</param>
        /// <param name="scale">Scale multiplied onto the prefab's own scale.</param>
        /// <returns>The new instance.</returns>
        public static GameObject Place(GameObject prefab, Transform parent, Vector3 center, float bottomY, Quaternion rotation, Vector3 scale)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.SetPositionAndRotation(center, rotation);
            instance.transform.localScale = Vector3.Scale(prefab.transform.localScale, scale);

            var bounds = CombinedBounds(instance);
            instance.transform.position += new Vector3(center.x - bounds.center.x, bottomY - bounds.min.y, center.z - bounds.center.z);
            return instance;
        }

        /// <summary>
        /// Removes every collider from an instance, for visuals that physics and ground clicks must ignore.
        /// </summary>
        /// <param name="instance">The instance to strip.</param>
        public static void RemoveColliders(GameObject instance)
        {
            foreach (var collider in instance.GetComponentsInChildren<Collider>())
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
