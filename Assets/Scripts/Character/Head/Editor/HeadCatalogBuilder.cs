using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Head.EditorTools
{
    /// <summary>
    /// Builds the <see cref="HeadCatalog"/> asset from the head models in the Female and Male
    /// Heads folders. Models are named <c>Head_{Gender}_{Name}</c> and the two genders of the same
    /// name form one entry.
    /// </summary>
    public static class HeadCatalogBuilder
    {
        private const string Root = "Assets/Models/Character Customization";
        private const string CatalogPath = Root + "/HeadCatalog.asset";
        private const string DefaultId = "Default";

        /// <summary>
        /// Creates or updates the catalog. The first entry keeps the head of the body, followed by
        /// one entry per head name sorted alphabetically. Existing style indices change when
        /// models are added or removed.
        /// </summary>
        /// <returns>The catalog asset.</returns>
        [MenuItem("Tools/Head/Rebuild Head Catalog")]
        public static HeadCatalog Build()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HeadCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<HeadCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var female = CollectModels($"{Root}/Female/Heads");
            var male = CollectModels($"{Root}/Male/Heads");

            var ids = new SortedSet<string>(female.Keys, System.StringComparer.OrdinalIgnoreCase);
            ids.UnionWith(male.Keys);

            var entries = new List<HeadStyleEntry> { new HeadStyleEntry(DefaultId, DefaultId, null, null) };
            foreach (string id in ids)
            {
                female.TryGetValue(id, out GameObject femaleModel);
                male.TryGetValue(id, out GameObject maleModel);
                entries.Add(new HeadStyleEntry(id, ObjectNames.NicifyVariableName(id), femaleModel, maleModel));
            }

            catalog.SetStyles(entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static Dictionary<string, GameObject> CollectModels(string folder)
        {
            var models = new Dictionary<string, GameObject>();
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return models;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (model != null)
                {
                    models[model.name.Substring(model.name.LastIndexOf('_') + 1)] = model;
                }
            }

            return models;
        }
    }
}
