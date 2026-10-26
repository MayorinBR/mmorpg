using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Small helpers for finding and removing objects of the active scene by name.
    /// </summary>
    public static class SceneObjects
    {
        /// <summary>
        /// Destroys every active root-level or nested object with the given name, so a generator can rebuild its output.
        /// </summary>
        /// <param name="name">Exact object name.</param>
        public static void DestroyAllNamed(string name)
        {
            var found = GameObject.Find(name);

            while (found != null)
            {
                Object.DestroyImmediate(found);
                found = GameObject.Find(name);
            }
        }

        /// <summary>
        /// Finds an active object by name or fails with a message naming what the map is missing.
        /// </summary>
        /// <param name="name">Exact object name.</param>
        /// <returns>The object.</returns>
        public static GameObject Require(string name)
        {
            var found = GameObject.Find(name);
            return found != null ? found : throw new System.InvalidOperationException($"The scene has no active object named '{name}'.");
        }
    }
}
