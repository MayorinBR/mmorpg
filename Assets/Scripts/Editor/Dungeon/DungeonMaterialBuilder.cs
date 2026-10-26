using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Creates the pattern texture array and the shared material of the dungeon kit.</summary>
    public static class DungeonMaterialBuilder
    {
        private const int PatternSize = 512;
        private const int PatternCount = 8;
        private const int Anisotropy = 4;
        private const string PatternFolder = DungeonKitPaths.Painted + "/Patterns";

        /// <summary>Configures the palette and category textures, builds the pattern array and writes the material.</summary>
        /// <returns>The shared dungeon material, or null when the shader is missing.</returns>
        public static Material Build()
        {
            ConfigurePointTexture(DungeonKitPaths.PalettePng, true);
            ConfigurePointTexture(DungeonKitPaths.CategoryPng, false);
            Texture2DArray patterns = BuildPatternArray();
            return BuildMaterial(patterns);
        }

        private static void ConfigurePointTexture(string path, bool srgb)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                Debug.LogError("DungeonMaterialBuilder: missing texture " + path);
                return;
            }

            importer.sRGBTexture = srgb;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        private static Texture2DArray BuildPatternArray()
        {
            var files = new List<string>(Directory.GetFiles(PatternFolder, "*.png"));
            files.Sort(System.StringComparer.Ordinal);
            if (files.Count != PatternCount)
            {
                Debug.LogError($"DungeonMaterialBuilder: expected {PatternCount} patterns in {PatternFolder}, found {files.Count}.");
                return null;
            }

            var array = new Texture2DArray(PatternSize, PatternSize, PatternCount, TextureFormat.R8, true, true)
            {
                filterMode = FilterMode.Trilinear,
                wrapMode = TextureWrapMode.Repeat,
                anisoLevel = Anisotropy,
            };

            for (int slice = 0; slice < PatternCount; slice++)
            {
                var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                source.LoadImage(File.ReadAllBytes(files[slice]));
                Color32[] pixels = source.GetPixels32();
                var bytes = new byte[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    bytes[i] = pixels[i].r;
                }

                array.SetPixelData(bytes, 0, slice);
                Object.DestroyImmediate(source);
            }

            array.Apply(true, true);
            AssetDatabase.DeleteAsset(DungeonKitPaths.PatternArray);
            AssetDatabase.CreateAsset(array, DungeonKitPaths.PatternArray);
            return array;
        }

        private static Material BuildMaterial(Texture2DArray patterns)
        {
            Shader shader = Shader.Find(DungeonKitPaths.ShaderName);
            if (shader == null)
            {
                Debug.LogError("DungeonMaterialBuilder: shader not found: " + DungeonKitPaths.ShaderName);
                return null;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(DungeonKitPaths.Material);
            if (material == null)
            {
                material = new Material(shader) { name = "DungeonPainted" };
                AssetDatabase.CreateAsset(material, DungeonKitPaths.Material);
            }

            material.shader = shader;
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DungeonKitPaths.PalettePng));
            material.SetTexture("_CategoryMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DungeonKitPaths.CategoryPng));
            material.SetTexture("_PatternArray", patterns);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
