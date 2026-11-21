using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Adds an invisible walkable collider on the Ground layer to every City4 stair prefab, so foot IK and click-to-move can hit the steps.
    /// The pass is idempotent and must be run again after the kit is regenerated.
    /// </summary>
    public static class StairWalkColliderTool
    {
        private const string StairsFolder = "Assets/Models/Cities/City4/Stairs";
        private const string ColliderName = "WalkCollider";
        private const string GroundLayerName = "Ground";
        private static readonly string[] NonWalkableMarkers = { "Parapet", "Post", "Support" };

        /// <summary>
        /// Adds the walk collider to every walkable stair prefab that does not have one yet.
        /// </summary>
        [MenuItem("Tools/Optimization/Colliders/Add Stair Walk Colliders")]
        public static void AddAll()
        {
            int groundLayer = LayerMask.NameToLayer(GroundLayerName);
            if (groundLayer < 0)
            {
                Debug.LogError("Layer '" + GroundLayerName + "' does not exist.");
                return;
            }

            int added = 0;
            int skipped = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { StairsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsNonWalkable(path))
                {
                    continue;
                }

                if (TryAdd(path, groundLayer))
                {
                    added++;
                }
                else
                {
                    skipped++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Stair walk colliders: added " + added + ", skipped " + skipped + ".");
        }

        private static bool IsNonWalkable(string path)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            foreach (string marker in NonWalkableMarkers)
            {
                if (fileName.Contains(marker))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryAdd(string path, int groundLayer)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                MeshFilter filter = root.GetComponentInChildren<MeshFilter>(true);
                if (filter == null || filter.sharedMesh == null || filter.transform.Find(ColliderName) != null)
                {
                    return false;
                }

                var child = new GameObject(ColliderName) { layer = groundLayer };
                child.transform.SetParent(filter.transform, false);
                child.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
