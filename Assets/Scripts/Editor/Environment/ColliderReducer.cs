using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools
{
    /// <summary>
    /// Removes colliders that no runtime system reads, from prefab assets and from the build scenes.
    /// Enabled build scenes that bake their NavMesh from physics colliders, and every prefab they depend on, are left untouched.
    /// Both passes can run as a dry run that only reports what would be removed.
    /// </summary>
    public static class ColliderReducer
    {
        private const string NavMeshFromCollidersMarker = "m_UseGeometry: 1";
        private const string GameplayPrefabFolder = "Assets/Prefabs/";

        /// <summary>
        /// Reports what both passes would remove, without changing any asset.
        /// </summary>
        [MenuItem("Tools/Optimization/Colliders/Report (dry run)")]
        public static void ReportAll()
        {
            Debug.Log(ReducePrefabs(false) + "\n" + ReduceScenes(false, GetBuildScenePaths()));
        }

        /// <summary>
        /// Removes unused colliders from prefabs, then from the build scenes, and saves them.
        /// </summary>
        [MenuItem("Tools/Optimization/Colliders/Reduce")]
        public static void ReduceAll()
        {
            if (!EditorUtility.DisplayDialog("Reduce colliders", "Remove unused colliders from prefabs and build scenes? Commit or back up first.", "Reduce", "Cancel"))
            {
                return;
            }

            Debug.Log(ReducePrefabs(true) + "\n" + ReduceScenes(true, GetBuildScenePaths()));
        }

        /// <summary>
        /// Lists the build scene paths.
        /// </summary>
        /// <returns>Scene asset paths in build order.</returns>
        public static List<string> GetBuildScenePaths()
        {
            var paths = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    paths.Add(scene.path);
                }
            }

            return paths;
        }

        /// <summary>
        /// Removes unused colliders owned by prefab assets under a folder.
        /// Colliders that come from nested prefabs are handled when their own asset is processed.
        /// </summary>
        /// <param name="apply">True to modify and save the prefabs, false for a dry run.</param>
        /// <param name="rootFolder">Asset folder to scan.</param>
        /// <returns>Text summary of the pass.</returns>
        public static string ReducePrefabs(bool apply, string rootFolder = "Assets")
        {
            var guarded = FindPrefabsUsedByColliderNavMeshScenes();
            var report = new ReductionReport("prefabs");

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { rootFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    continue;
                }

                int removable = CountOwnedRemovable(asset);
                if (removable == 0)
                {
                    continue;
                }

                if (path.StartsWith(GameplayPrefabFolder) || guarded.Contains(path))
                {
                    report.AddSkipped(path, removable);
                    continue;
                }

                if (apply)
                {
                    RemoveFromPrefab(path);
                }

                report.AddRemoved(path, removable);
            }

            return report.Finish(apply);
        }

        /// <summary>
        /// Removes unused colliders from the given scenes and saves them.
        /// </summary>
        /// <param name="apply">True to modify and save the scenes, false for a dry run.</param>
        /// <param name="scenePaths">Scene asset paths to process.</param>
        /// <returns>Text summary of the pass.</returns>
        public static string ReduceScenes(bool apply, IEnumerable<string> scenePaths)
        {
            var guardedScenes = FindColliderNavMeshScenes();
            var report = new ReductionReport("scenes");

            foreach (string path in scenePaths)
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool wasLoaded = scene.isLoaded;
                if (!wasLoaded)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }

                var removable = CollectRemovable(scene);
                if (removable.Count > 0 && guardedScenes.Contains(path))
                {
                    report.AddSkipped(path, removable.Count);
                }
                else if (removable.Count > 0)
                {
                    if (apply)
                    {
                        foreach (var collider in removable)
                        {
                            DestroyInScene(collider);
                        }

                        EditorSceneManager.SaveScene(scene);
                    }

                    report.AddRemoved(path, removable.Count);
                }

                if (!wasLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return report.Finish(apply);
        }

        private static int CountOwnedRemovable(GameObject prefabAsset)
        {
            int count = 0;
            foreach (var collider in prefabAsset.GetComponentsInChildren<Collider>(true))
            {
                if (ColliderReductionRules.IsRemovable(collider) && !PrefabUtility.IsPartOfPrefabInstance(collider))
                {
                    count++;
                }
            }

            return count;
        }

        private static void RemoveFromPrefab(string path)
        {
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var collider in contents.GetComponentsInChildren<Collider>(true))
                {
                    if (!ColliderReductionRules.IsRemovable(collider) || PrefabUtility.IsPartOfPrefabInstance(collider))
                    {
                        continue;
                    }

                    var holder = collider.gameObject;
                    Object.DestroyImmediate(collider);
                    if (ColliderReductionRules.IsEmptyColliderHolder(holder))
                    {
                        Object.DestroyImmediate(holder);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static List<Collider> CollectRemovable(Scene scene)
        {
            var found = new List<Collider>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (ColliderReductionRules.IsRemovable(collider))
                    {
                        found.Add(collider);
                    }
                }
            }

            return found;
        }

        private static void DestroyInScene(Collider collider)
        {
            var holder = collider.gameObject;
            Undo.DestroyObjectImmediate(collider);
            if (!PrefabUtility.IsPartOfPrefabInstance(holder) && ColliderReductionRules.IsEmptyColliderHolder(holder))
            {
                Undo.DestroyObjectImmediate(holder);
            }
        }

        private static HashSet<string> FindColliderNavMeshScenes()
        {
            var scenes = new HashSet<string>();
            foreach (string path in GetBuildScenePaths())
            {
                if (File.ReadAllText(path).Contains(NavMeshFromCollidersMarker))
                {
                    scenes.Add(path);
                }
            }

            return scenes;
        }

        private static HashSet<string> FindPrefabsUsedByColliderNavMeshScenes()
        {
            var prefabs = new HashSet<string>();
            foreach (string scenePath in FindColliderNavMeshScenes())
            {
                foreach (string dependency in AssetDatabase.GetDependencies(scenePath, true))
                {
                    if (dependency.EndsWith(".prefab"))
                    {
                        prefabs.Add(dependency);
                    }
                }
            }

            return prefabs;
        }

        private sealed class ReductionReport
        {
            private readonly string kind;
            private readonly StringBuilder lines = new StringBuilder();
            private int removed;
            private int skipped;
            private int assets;

            public ReductionReport(string kind)
            {
                this.kind = kind;
            }

            public void AddRemoved(string path, int count)
            {
                removed += count;
                assets++;
                lines.AppendLine(path + "\t" + count + "\tremoved");
            }

            public void AddSkipped(string path, int count)
            {
                skipped += count;
                lines.AppendLine(path + "\t" + count + "\tskipped");
            }

            public string Finish(bool applied)
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string logFolder = Path.Combine(projectRoot, "Logs");
                Directory.CreateDirectory(logFolder);
                File.WriteAllText(Path.Combine(logFolder, "ColliderReduction_" + kind + ".tsv"), lines.ToString());

                string mode = applied ? "removed" : "would remove";
                return "Colliders " + kind + ": " + mode + " " + removed + " in " + assets + " assets, skipped " + skipped + " (guarded). Details in Logs/ColliderReduction_" + kind + ".tsv";
            }
        }
    }
}
