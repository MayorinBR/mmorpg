using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools.Maps
{
    /// <summary>Small scene queries shared by the map builders.</summary>
    public static class SceneHelper
    {
        private const string TerrainName = "Terrain";

        /// <summary>Finds the terrain among the root objects of a scene.</summary>
        /// <param name="scene">The scene to search.</param>
        /// <returns>The terrain component, or null.</returns>
        public static Terrain FindTerrain(Scene scene)
        {
            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (candidate.name == TerrainName)
                {
                    return candidate.GetComponent<Terrain>();
                }
            }

            return null;
        }

        /// <summary>Destroys the root objects that have one of the given names.</summary>
        /// <param name="scene">The scene to clean.</param>
        /// <param name="names">Names of the roots to destroy.</param>
        public static void DeleteRoots(Scene scene, params string[] names)
        {
            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (System.Array.IndexOf(names, candidate.name) >= 0)
                {
                    Object.DestroyImmediate(candidate);
                }
            }
        }

        /// <summary>Returns the child group with a name, creating it when missing.</summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="name">Group name.</param>
        /// <returns>The group transform.</returns>
        public static Transform Group(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }
    }
}
