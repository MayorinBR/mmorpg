using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Reads the armor manifest, the JSON file that lists every armor item the build menu creates.
    /// </summary>
    public static class ArmorManifestStore
    {
        /// <summary>Project relative path of the manifest file.</summary>
        public const string ManifestPath = "Assets/Data/Items/ArmorManifest.json";

        /// <summary>Root object of the manifest JSON, required by JsonUtility.</summary>
        [Serializable]
        public sealed class ManifestFile
        {
            /// <summary>The armor items.</summary>
            public List<ArmorManifestEntry> items = new List<ArmorManifestEntry>();
        }

        /// <summary>
        /// Loads the manifest entries.
        /// </summary>
        /// <returns>The entries, or an empty list when the file is missing or unreadable.</returns>
        public static IReadOnlyList<ArmorManifestEntry> Load()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogWarning($"Equipment: manifest not found at {ManifestPath}.");
                return Array.Empty<ArmorManifestEntry>();
            }

            var file = JsonUtility.FromJson<ManifestFile>(File.ReadAllText(ManifestPath));
            return file?.items ?? new List<ArmorManifestEntry>();
        }
    }
}
