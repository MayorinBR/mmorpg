using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Project.World;

namespace Project.EditorTools
{
    /// <summary>
    /// Fits the <see cref="MapThumbnailArea"/> of every map scene to the full extent of its terrain so all
    /// thumbnails frame the same square and the camera always sits above the highest point of the scene.
    /// </summary>
    public static class MapThumbnailAreaFitter
    {
        private const string SceneFolder = "Assets/Scenes/Maps";
        private const float HeightMargin = 20f;
        private const float DepthMargin = 20f;
        private const float CenterTolerance = 0.5f;
        private const int UiLayer = 5;

        /// <summary>Opens each map scene in turn, fits its thumbnail area and saves it, then reopens the previous scene.</summary>
        [MenuItem("Tools/Maps/Fit Thumbnail Areas To Terrain")]
        public static void FitAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScene = EditorSceneManager.GetActiveScene().path;
            foreach (string guid in AssetDatabase.FindAssets("t:SceneAsset", new[] { SceneFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (FitScene())
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrEmpty(previousScene))
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        /// <summary>Fits the thumbnail area of the open scene to its terrain.</summary>
        /// <returns>True when an area was found and updated.</returns>
        public static bool FitScene()
        {
            var area = Object.FindFirstObjectByType<MapThumbnailArea>();
            Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            if (area == null || terrains.Length == 0)
            {
                return false;
            }

            Bounds extent = MeasureTerrains(terrains);
            float top = Mathf.Max(extent.max.y, HighestRenderer(extent));
            Vector3 position = area.transform.position;
            if (Mathf.Abs(position.x - extent.center.x) > CenterTolerance || Mathf.Abs(position.z - extent.center.z) > CenterTolerance)
            {
                Debug.LogWarning($"MapThumbnailAreaFitter: {area.gameObject.scene.name} area is not centered on its terrain.");
            }

            var serialized = new SerializedObject(area);
            serialized.FindProperty("sideLength").floatValue = Mathf.Max(extent.size.x, extent.size.z);
            serialized.FindProperty("cameraHeight").floatValue = top + HeightMargin - position.y;
            serialized.FindProperty("captureDepth").floatValue = top + HeightMargin - extent.min.y + DepthMargin;
            serialized.ApplyModifiedProperties();
            return true;
        }

        private static Bounds MeasureTerrains(Terrain[] terrains)
        {
            Bounds result = TerrainBounds(terrains[0]);
            foreach (Terrain terrain in terrains.Skip(1))
            {
                result.Encapsulate(TerrainBounds(terrain));
            }

            return result;
        }

        private static Bounds TerrainBounds(Terrain terrain)
        {
            Vector3 size = terrain.terrainData.size;
            return new Bounds(terrain.transform.position + (size * 0.5f), size);
        }

        private static float HighestRenderer(Bounds extent)
        {
            float highest = float.MinValue;
            foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                Bounds bounds = renderer.bounds;
                bool inside = bounds.center.x >= extent.min.x && bounds.center.x <= extent.max.x && bounds.center.z >= extent.min.z && bounds.center.z <= extent.max.z;
                if (renderer.gameObject.layer != UiLayer && inside)
                {
                    highest = Mathf.Max(highest, bounds.max.y);
                }
            }

            return highest;
        }
    }
}
