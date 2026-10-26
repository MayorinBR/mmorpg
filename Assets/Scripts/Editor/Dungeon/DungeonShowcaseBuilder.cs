using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Lays every prefab of the dungeon kit out on a grid in a scene, one block of rows per category.</summary>
    public static class DungeonShowcaseBuilder
    {
        private const string ScenePath = DungeonKitPaths.Root + "/DungeonKitShowcase.unity";
        private const float RowWidth = 80f;
        private const float Gap = 1.8f;
        private const float CategoryGap = 8f;

        /// <summary>Builds and saves the showcase scene.</summary>
        [MenuItem("Tools/Dungeon/Build Showcase Scene")]
        public static void Build()
        {
            if (File.Exists(ScenePath) && !OverwriteGuard.Confirm(ScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("Kit").transform;
            float z = 0f;
            foreach (string category in DungeonKitPaths.Categories)
            {
                z = PlaceCategory(root, category, z) + CategoryGap;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static float PlaceCategory(Transform root, string category, float startZ)
        {
            var parent = new GameObject(category).transform;
            parent.SetParent(root, false);
            string folder = $"{DungeonKitPaths.Prefabs}/{category}";
            if (!Directory.Exists(folder))
            {
                return startZ;
            }

            var items = new List<(GameObject Instance, Bounds Bounds)>();
            foreach (string file in Directory.GetFiles(folder, "*.prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace('\\', '/'));
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                items.Add((instance, RendererBounds(instance)));
            }

            float x = 0f;
            float z = startZ;
            float rowDepth = 0f;
            foreach ((GameObject instance, Bounds bounds) in items)
            {
                if (x > 0f && x + bounds.size.x > RowWidth)
                {
                    x = 0f;
                    z += rowDepth + Gap;
                    rowDepth = 0f;
                }

                instance.transform.position = new Vector3(x - bounds.min.x, 0f, z - bounds.min.z);
                x += bounds.size.x + Gap;
                rowDepth = Mathf.Max(rowDepth, bounds.size.z);
            }

            return z + rowDepth;
        }

        private static Bounds RendererBounds(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}
