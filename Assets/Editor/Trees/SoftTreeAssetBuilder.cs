using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mmo.EditorTools.Trees
{
    /// <summary>Describes one tree variant to be baked into a mesh asset and a prefab.</summary>
    public readonly struct SoftTreeVariant
    {
        /// <summary>Creates a variant description.</summary>
        /// <param name="name">Asset base name.</param>
        /// <param name="generator">Geometry generator.</param>
        /// <param name="leafMaterial">Leaf material file name without extension.</param>
        public SoftTreeVariant(string name, ITreeGenerator generator, string leafMaterial)
        {
            Name = name;
            Generator = generator;
            LeafMaterial = leafMaterial;
        }

        /// <summary>Asset base name.</summary>
        public string Name { get; }

        /// <summary>Geometry generator.</summary>
        public ITreeGenerator Generator { get; }

        /// <summary>Leaf material file name without extension.</summary>
        public string LeafMaterial { get; }
    }

    /// <summary>Editor tool that creates the soft tree materials, meshes and prefabs.</summary>
    public static class SoftTreeAssetBuilder
    {
        private const string Root = "Assets/Models/Environment/Common/Trees/Soft";
        private const string TextureFolder = Root + "/Textures";
        private const string MaterialFolder = Root + "/Materials";
        private const string MeshFolder = Root + "/Meshes";
        private const string PrefabFolder = Root + "/Prefabs";

        /// <summary>Builds all soft tree assets.</summary>
        [MenuItem("Tools/Trees/Build Soft Trees")]
        public static void BuildAll()
        {
            foreach (string folder in new[] { MaterialFolder, MeshFolder, PrefabFolder })
            {
                Directory.CreateDirectory(folder);
            }

            AssetDatabase.Refresh();
            ConfigureTextures();

            Material bark = CreateBarkMaterial();
            CreateLeafMaterial("Mat_SoftPine", "leaf_pine");
            CreateLeafMaterial("Mat_SoftPineCool", "leaf_pine_cool");
            CreateLeafMaterial("Mat_SoftRound", "leaf_round");

            var variants = new[]
            {
                new SoftTreeVariant("SoftPine_A", new SoftPineGenerator(11, 7.5f, 13), "Mat_SoftPine"),
                new SoftTreeVariant("SoftPine_B", new SoftPineGenerator(23, 9.5f, 14), "Mat_SoftPineCool"),
                new SoftTreeVariant("SoftPine_Small", new SoftPineGenerator(37, 5f, 10), "Mat_SoftPine"),
                new SoftTreeVariant("SoftBroadleaf_A", new SoftBroadleafGenerator(5, 7f, 6), "Mat_SoftRound"),
            };

            foreach (SoftTreeVariant variant in variants)
            {
                Material leaf = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{variant.LeafMaterial}.mat");
                BuildVariant(variant, bark, leaf);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ConfigureTextures()
        {
            foreach (string leaf in new[] { "leaf_pine", "leaf_pine_cool", "leaf_round" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath($"{TextureFolder}/{leaf}.png");
                importer.alphaIsTransparency = true;
                importer.mipMapsPreserveCoverage = true;
                importer.alphaTestReferenceValue = 0.5f;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            var normal = (TextureImporter)AssetImporter.GetAtPath($"{TextureFolder}/bark_normal.png");
            normal.textureType = TextureImporterType.NormalMap;
            normal.SaveAndReimport();
        }

        private static Material CreateBarkMaterial()
        {
            Material material = LoadOrCreateMaterial("Mat_SoftBark");
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/bark_albedo.png"));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/bark_normal.png"));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Smoothness", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateLeafMaterial(string materialName, string textureName)
        {
            Material material = LoadOrCreateMaterial(materialName);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/{textureName}.png"));
            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_Cull", 0f);
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateMaterial(string materialName)
        {
            string path = $"{MaterialFolder}/{materialName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void BuildVariant(SoftTreeVariant variant, Material bark, Material leaf)
        {
            Mesh mesh = variant.Generator.Generate().ToMesh(variant.Name);
            string meshPath = $"{MeshFolder}/{variant.Name}.asset";
            AssetDatabase.DeleteAsset(meshPath);
            AssetDatabase.CreateAsset(mesh, meshPath);

            var root = new GameObject($"Tree_{variant.Name}");
            root.AddComponent<MeshFilter>().sharedMesh = mesh;
            root.AddComponent<MeshRenderer>().sharedMaterials = new[] { bark, leaf };
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/Tree_{variant.Name}.prefab");
            Object.DestroyImmediate(root);
        }
    }
}
