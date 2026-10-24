using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>
    /// Keeps the environment model folders tidy: shared trees live under Common and obsolete assets are removed.
    /// </summary>
    public static class EnvFolderOrganizer
    {
        private const string EnvironmentRoot = "Assets/Models/Environment";
        private const string LegacyTrees = "Assets/Models/Trees";
        private const string LegacySoftTrees = LegacyTrees + "/Soft";
        private const string CommonTrees = EnvironmentRoot + "/Common/Trees";
        private const string ObsoleteMarker = "StairDescent";

        /// <summary>Moves the shared soft trees under Environment/Common and deletes obsolete environment assets.</summary>
        [MenuItem("Tools/Environment/Organize Folders")]
        public static void Organize()
        {
            MoveSoftTrees();
            DeleteObsoleteAssets();
            DeleteIfEmpty(LegacyTrees);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void MoveSoftTrees()
        {
            if (!AssetDatabase.IsValidFolder(LegacySoftTrees))
            {
                return;
            }

            EnsureFolder(CommonTrees);
            string error = AssetDatabase.MoveAsset(LegacySoftTrees, CommonTrees + "/Soft");
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("EnvFolderOrganizer: " + error);
            }
        }

        private static void DeleteObsoleteAssets()
        {
            string[] paths = AssetDatabase.FindAssets(ObsoleteMarker, new[] { EnvironmentRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !AssetDatabase.IsValidFolder(path))
                .ToArray();
            foreach (string path in paths)
            {
                AssetDatabase.DeleteAsset(path);
            }

            Debug.Log("EnvFolderOrganizer: removed " + paths.Length + " obsolete assets.");
        }

        private static void DeleteIfEmpty(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder) && AssetDatabase.FindAssets(string.Empty, new[] { folder }).Length == 0)
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
