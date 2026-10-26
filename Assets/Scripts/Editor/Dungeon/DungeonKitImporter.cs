using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Menu entry that turns the exported dungeon kit files into a ready-to-use set of materials and prefabs.</summary>
    public static class DungeonKitImporter
    {
        /// <summary>Builds the material, remaps the models and writes the prefabs.</summary>
        [MenuItem("Tools/Dungeon/Import Kit")]
        public static void Import()
        {
            if (!OverwriteGuard.Confirm(DungeonKitPaths.Prefabs))
            {
                return;
            }

            AssetDatabase.Refresh();
            Material material = DungeonMaterialBuilder.Build();
            if (material == null)
            {
                return;
            }

            int count = DungeonPrefabBuilder.Build(material);
            Debug.Log($"DungeonKitImporter: {count} prefabs written to {DungeonKitPaths.Prefabs}.");
        }
    }
}
