using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Hair.EditorTools
{
    /// <summary>
    /// Builds the <see cref="HairCatalog"/> asset from the hair models found in the Hair folder.
    /// </summary>
    public static class HairCatalogBuilder
    {
        private const string HairFolder = "Assets/Models/Character Customization/Hair";
        private const string CatalogPath = HairFolder + "/HairCatalog.asset";

        /// <summary>
        /// Creates or updates the catalog so it lists every model in the Hair folder, sorted by
        /// file name. Stored placements are kept for models that were already listed. Existing
        /// style indices change when models are added or removed.
        /// </summary>
        /// <returns>The catalog asset.</returns>
        [MenuItem("Tools/Hair/Rebuild Hair Catalog")]
        public static HairCatalog Build()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HairCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<HairCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            List<HairStyleEntry> entries = CollectEntries();
            KeepPlacements(catalog, entries);
            catalog.SetStyles(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static void KeepPlacements(HairCatalog catalog, List<HairStyleEntry> entries)
        {
            var previous = new Dictionary<GameObject, HairStyleEntry>();
            foreach (HairStyleEntry entry in catalog.Styles)
            {
                if (entry.Prefab != null)
                {
                    previous[entry.Prefab] = entry;
                }
            }

            foreach (HairStyleEntry entry in entries)
            {
                if (previous.TryGetValue(entry.Prefab, out HairStyleEntry old))
                {
                    entry.CopyPlacementsFrom(old);
                }
            }
        }

        private static List<HairStyleEntry> CollectEntries()
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { HairFolder }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(System.StringComparer.OrdinalIgnoreCase);

            var entries = new List<HairStyleEntry>(paths.Count);
            foreach (string path in paths)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null)
                {
                    entries.Add(new HairStyleEntry(ObjectNames.NicifyVariableName(model.name), model));
                }
            }

            return entries;
        }
    }
}
