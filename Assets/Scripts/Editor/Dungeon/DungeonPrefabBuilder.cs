using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Applies the shared material to the kit FBX files and creates one prefab per piece.</summary>
    public static class DungeonPrefabBuilder
    {
        private const int DecorationLayer = 2;
        private const string LightObjectName = "Torch";

        /// <summary>Remaps the material of every kit model and rebuilds all prefabs.</summary>
        /// <param name="material">The shared dungeon material.</param>
        /// <returns>The number of prefabs written.</returns>
        public static int Build(Material material)
        {
            int count = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string category in DungeonKitPaths.Categories)
                {
                    foreach (string fbx in ModelsOf(category))
                    {
                        ConfigureModel(fbx, material);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            foreach (string category in DungeonKitPaths.Categories)
            {
                foreach (string fbx in ModelsOf(category))
                {
                    count += BuildPrefab(category, fbx) ? 1 : 0;
                }
            }

            AssetDatabase.SaveAssets();
            return count;
        }

        private static string[] ModelsOf(string category)
        {
            string folder = $"{DungeonKitPaths.Root}/{category}";
            return Directory.Exists(folder) ? Directory.GetFiles(folder, "*.fbx") : new string[0];
        }

        private static void ConfigureModel(string path, Material material)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            if (importer == null)
            {
                return;
            }

            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), DungeonKitPaths.SourceMaterialName), material);
            importer.SaveAndReimport();
        }

        private static bool BuildPrefab(string category, string fbxPath)
        {
            string name = Path.GetFileNameWithoutExtension(fbxPath);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath.Replace('\\', '/'));
            if (model == null)
            {
                return false;
            }

            string prefabPath = DungeonKitPaths.PrefabPath(category, name);
            AssetFolders.Ensure(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try
            {
                instance.name = name;
                var meshFilter = instance.GetComponentInChildren<MeshFilter>();
                AddCollider(instance, meshFilter, category, name);
                AddLights(instance, name);
                SetFlags(instance, name);
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            return true;
        }

        private static void AddCollider(GameObject root, MeshFilter meshFilter, string category, string name)
        {
            if (meshFilter == null)
            {
                return;
            }

            Bounds bounds = meshFilter.sharedMesh.bounds;
            switch (DungeonPieceRules.ColliderFor(category, name, bounds))
            {
                case ColliderMode.Box:
                    var box = meshFilter.gameObject.AddComponent<BoxCollider>();
                    box.center = bounds.center;
                    box.size = bounds.size;
                    break;
                case ColliderMode.Mesh:
                    meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
                    break;
            }
        }

        private static void AddLights(GameObject root, string name)
        {
            int index = 0;
            foreach (PieceLight spec in DungeonPieceRules.LightsFor(name))
            {
                var holder = new GameObject(index == 0 ? LightObjectName : LightObjectName + index);
                holder.transform.SetParent(root.transform, false);
                holder.transform.localPosition = new Vector3(spec.Position.x, spec.Position.z, -spec.Position.y);
                var light = holder.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = spec.Color;
                light.intensity = spec.Intensity;
                light.range = spec.Range;
                light.shadows = LightShadows.None;
                index++;
            }
        }

        private static void SetFlags(GameObject root, string name)
        {
            if (DungeonPieceRules.IsDecoration(name))
            {
                root.layer = DecorationLayer;
                foreach (Transform child in root.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = DecorationLayer;
                }
            }

            GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
        }
    }
}
