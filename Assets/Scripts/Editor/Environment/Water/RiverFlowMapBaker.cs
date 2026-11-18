using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Bakes a flow map from the river mesh of the active scene and assigns a RiverWater material that uses it.
    /// The map stores the downhill direction, the speed and a water mask for every vertex of the river mesh.
    /// Run it again after the world rebuild tools regenerate the river mesh.
    /// </summary>
    public static class RiverFlowMapBaker
    {
        private const string RiversObjectPath = "WorldWater/Rivers";
        private const string ShaderName = "Project/Environment/RiverWater";
        private const string TemplateMaterialPath = "Assets/Materials/Environment/RiverWater.mat";
        private const string WaterMeshSuffix = "_Water";
        private const int BlurRadius = 6;
        private const float SlopeForMaxSpeed = 0.12f;

        /// <summary>
        /// Bakes the flow map for the river of the active scene.
        /// </summary>
        [MenuItem("Tools/Environment/Water/Bake River Flow Map (Active Scene)")]
        public static void BakeActiveScene()
        {
            try
            {
                Debug.Log(Bake());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Bakes the flow map, creates or updates the per map material and assigns it to the river renderer.
        /// </summary>
        /// <returns>A one line report of what was written.</returns>
        public static string Bake()
        {
            var rivers = GameObject.Find(RiversObjectPath);
            if (rivers == null)
            {
                throw new InvalidOperationException($"Object '{RiversObjectPath}' not found in the active scene.");
            }

            var mesh = rivers.GetComponent<MeshFilter>().sharedMesh;
            var meshPath = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(meshPath))
            {
                throw new InvalidOperationException("The river mesh must be saved as an asset.");
            }

            var folder = Path.GetDirectoryName(meshPath).Replace('\\', '/');
            var mapName = Path.GetFileNameWithoutExtension(meshPath).Replace(WaterMeshSuffix, string.Empty);

            var grid = WaterSurfaceGrid.FromMesh(mesh, rivers.transform);
            var field = RiverFlowField.Compute(grid, BlurRadius, SlopeForMaxSpeed);
            var texture = WriteFlowMap(field, grid, $"{folder}/{mapName}_FlowMap.png");

            var material = CreateMapMaterial($"{folder}/{mapName}_RiverWater.mat", texture, grid);
            var renderer = rivers.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(rivers.scene);
            EditorSceneManager.SaveScene(rivers.scene);

            return $"River flow map baked for {mapName}: {AssetDatabase.GetAssetPath(texture)} and {AssetDatabase.GetAssetPath(material)}";
        }

        private static Texture2D WriteFlowMap(RiverFlowField field, WaterSurfaceGrid grid, string path)
        {
            var size = field.Size;
            var pixels = new Color32[size * size];
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var i = row * size + col;
                    pixels[i] = new Color32(
                        EncodeSigned(field.Flow[i].x),
                        EncodeSigned(field.Flow[i].y),
                        (byte)Mathf.RoundToInt(field.Speed[i] * 255f),
                        grid.IsWater(col, row) ? (byte)255 : (byte)0);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            texture.SetPixels32(pixels);
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            File.WriteAllBytes(Path.Combine(projectRoot, path), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static byte EncodeSigned(float value)
        {
            return (byte)Mathf.RoundToInt((value * 0.5f + 0.5f) * 255f);
        }

        private static Material CreateMapMaterial(string path, Texture2D flowMap, WaterSurfaceGrid grid)
        {
            var template = EditorAssetUtility.EnsureMaterial(TemplateMaterialPath, ShaderName, _ => { });
            var half = grid.CellSize * 0.5f;
            var span = grid.Size * grid.CellSize;
            var area = new Vector4(grid.Origin.x - half, grid.Origin.y - half, span, span);

            return EditorAssetUtility.EnsureMaterial(path, ShaderName, material =>
            {
                material.parent = template;
                material.SetTexture("_FlowMap", flowMap);
                material.SetVector("_FlowArea", area);
            });
        }
    }
}
