using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Finds prefabs by name inside a set of project folders.</summary>
    public sealed class PrefabLibrary
    {
        private readonly Dictionary<string, GameObject> byName = new Dictionary<string, GameObject>();

        /// <summary>Initializes a new instance of the <see cref="PrefabLibrary"/> class.</summary>
        /// <param name="folders">Project folders searched recursively.</param>
        public PrefabLibrary(params string[] folders)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    byName[Path.GetFileNameWithoutExtension(path)] = prefab;
                }
            }
        }

        /// <summary>Gets a prefab by exact name.</summary>
        /// <param name="name">The prefab name without extension.</param>
        /// <returns>The prefab.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no prefab has that name.</exception>
        public GameObject Get(string name)
        {
            if (!byName.TryGetValue(name, out GameObject prefab))
            {
                throw new KeyNotFoundException("Prefab not found: " + name);
            }

            return prefab;
        }

        /// <summary>Gets every prefab whose name starts with one of the given prefixes, ordered by name.</summary>
        /// <param name="prefixes">Name prefixes.</param>
        /// <returns>The matching prefabs.</returns>
        public IReadOnlyList<GameObject> Match(params string[] prefixes)
        {
            return byName
                .Where(pair => prefixes.Any(prefix => pair.Key.StartsWith(prefix)))
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value)
                .ToList();
        }
    }
}
