using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Adds the target ring marker to enemy prefabs created by hand, so every
    /// enemy shows the white hover ring and the red current-target ring.
    /// </summary>
    public static class TargetRingMarkerMenu
    {
        private const string MarkerTypeName = "Project.UI.TargetRingMarker";

        /// <summary>
        /// Adds a <c>TargetRingMarker</c> with the default ring materials to the root of
        /// every selected prefab that does not already have one.
        /// </summary>
        [MenuItem("Tools/Enemies/Add Target Ring Marker To Selected Prefabs")]
        public static void AddToSelectedPrefabs()
        {
            var markerType = EditorReflection.FindType(MarkerTypeName);
            var added = 0;

            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    if (root.GetComponent(markerType) != null)
                    {
                        continue;
                    }

                    root.AddComponent(markerType);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    added++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            Debug.Log($"TargetRingMarkerMenu: added the marker to {added} prefab(s). Tune radius and ground offset in the Inspector.");
        }
    }
}
