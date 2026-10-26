using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Finds the generated kit prefabs by name and instantiates them into the active scene.</summary>
    public sealed class DungeonPrefabLibrary
    {
        private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        /// <summary>Initializes a new instance of the <see cref="DungeonPrefabLibrary"/> class and indexes every kit prefab.</summary>
        public DungeonPrefabLibrary()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { DungeonKitPaths.Prefabs }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                prefabs[System.IO.Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        /// <summary>Instantiates a kit prefab as a linked prefab instance.</summary>
        /// <param name="name">The piece name, such as <c>wall_straight</c>.</param>
        /// <param name="parent">The parent transform.</param>
        /// <param name="position">World position.</param>
        /// <param name="rotation">World rotation.</param>
        /// <returns>The new instance.</returns>
        public GameObject Instantiate(string name, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (!prefabs.TryGetValue(name, out GameObject prefab))
            {
                throw new KeyNotFoundException("Dungeon prefab not found: " + name);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }
    }
}
