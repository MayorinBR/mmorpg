using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>Runs the world scene builder one map per editor tick so the editor stays responsive.</summary>
    public static class WorldBuildRunner
    {
        private const string ArchivePath = "Tools/WorldTerrain/world_build.zip";
        private const string WorkFolder = "Temp/WorldBuild";
        private const float MapSide = 200f;

        private static Queue<WorldCellInfo> queue;
        private static WorldSceneBuilder builder;
        private static int failures;

        /// <summary>Builds every map of the world.</summary>
        [MenuItem("Tools/Maps/World/Build All Scenes")]
        public static void BuildAll()
        {
            Start(null, true);
        }

        /// <summary>Starts a build.</summary>
        /// <param name="names">Scene names to build, or null for all.</param>
        /// <param name="extract">True to extract the archive first.</param>
        public static void Start(string[] names, bool extract)
        {
            MapMath.Use(MapSide);
            string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            WorldBuildData data = extract
                ? WorldBuildData.Open(Path.Combine(project, ArchivePath), Path.Combine(project, WorkFolder))
                : WorldBuildData.OpenExtracted(Path.Combine(project, WorkFolder));
            EditorSceneManager.SaveOpenScenes();
            IEnumerable<WorldCellInfo> cells = data.Manifest.cells;
            if (names != null)
            {
                cells = cells.Where(cell => names.Contains(cell.name));
            }

            queue = new Queue<WorldCellInfo>(cells);
            builder = new WorldSceneBuilder(data);
            failures = 0;
            EditorApplication.update += Tick;
            Debug.Log($"WORLD_BUILD started: {queue.Count} maps");
        }

        private static void Tick()
        {
            if (queue.Count == 0)
            {
                EditorApplication.update -= Tick;
                AssetDatabase.SaveAssets();
                Debug.Log($"WORLD_BUILD_DONE failures={failures}");
                return;
            }

            WorldCellInfo info = queue.Dequeue();
            try
            {
                Debug.Log("WORLD_BUILD ok " + builder.Build(info));
            }
            catch (Exception exception)
            {
                failures++;
                Debug.LogError($"WORLD_BUILD failed {info.name}: {exception}");
            }
        }
    }
}
