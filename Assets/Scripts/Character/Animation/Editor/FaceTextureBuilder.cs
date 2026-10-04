using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Animation.EditorTools
{
    /// <summary>
    /// Builds the face texture arrays from the individual sprites in the Eyes and Mouths
    /// folders. Run it from Tools > Face > Rebuild Face Textures after adding, removing or
    /// editing a sprite. It writes the arrays and the part catalog, and assigns the arrays to
    /// every toon material under the Character Customization folder.
    /// </summary>
    public static class FaceTextureBuilder
    {
        private const string RootFolder = "Assets/Models/Character Customization";
        private const string EyesFolder = RootFolder + "/Eyes";
        private const string MouthsFolder = RootFolder + "/Mouths";
        private const string OutputFolder = RootFolder + "/Face";
        private const string ToonShaderName = "Project/Characters/ToonCharacter";

        private static readonly Regex Digits = new Regex(@"\d+");

        /// <summary>
        /// Rebuilds the eye, iris and mouth texture arrays and the part catalog.
        /// </summary>
        [MenuItem("Tools/Face/Rebuild Face Textures")]
        public static void Rebuild()
        {
            List<string> eyeFiles = FindSprites(EyesFolder);
            List<string> mouthFiles = FindSprites(MouthsFolder);
            if (eyeFiles.Count == 0 || mouthFiles.Count == 0)
            {
                Debug.LogError($"Face textures not built: found {eyeFiles.Count} eye and {mouthFiles.Count} mouth sprites.");
                return;
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            var eyeColors = new List<byte[]>();
            var irisMasks = new List<byte[]>();
            int eyeWidth = 0, eyeHeight = 0;
            foreach (string file in eyeFiles)
            {
                byte[] pixels = ReadPixels(file, out int w, out int h);
                if (!SameSize(ref eyeWidth, ref eyeHeight, w, h, file))
                {
                    return;
                }

                FaceTextureProcessor.ProcessEye(pixels, w, h, out byte[] color, out byte[] mask);
                eyeColors.Add(color);
                irisMasks.Add(mask);
            }

            var mouthColors = new List<byte[]>();
            int mouthWidth = 0, mouthHeight = 0;
            foreach (string file in mouthFiles)
            {
                byte[] pixels = ReadPixels(file, out int w, out int h);
                if (!SameSize(ref mouthWidth, ref mouthHeight, w, h, file))
                {
                    return;
                }

                mouthColors.Add(FaceTextureProcessor.ProcessMouth(pixels, w, h));
            }

            Texture2DArray eyeArray = SaveArray("EyeLayers", eyeWidth, eyeHeight, TextureFormat.RGBA32, false, eyeColors);
            Texture2DArray irisArray = SaveArray("EyeIrisLayers", eyeWidth, eyeHeight, TextureFormat.R8, true, irisMasks);
            Texture2DArray mouthArray = SaveArray("MouthLayers", mouthWidth, mouthHeight, TextureFormat.RGBA32, false, mouthColors);
            SaveCatalog(eyeFiles, mouthFiles);
            int assigned = AssignToMaterials(eyeArray, irisArray, mouthArray);

            AssetDatabase.SaveAssets();
            Debug.Log($"Face textures rebuilt: {eyeFiles.Count} eyes, {mouthFiles.Count} mouths, {assigned} material(s) updated.");
        }

        private static List<string> FindSprites(string folder)
        {
            var files = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                bool isSource = Path.GetDirectoryName(path).Replace('\\', '/') == folder
                    && path.EndsWith(".png") && !name.StartsWith("_") && !name.Contains("Sheet");
                if (isSource)
                {
                    files.Add(path);
                }
            }

            files.Sort(CompareNatural);
            return files;
        }

        private static int CompareNatural(string a, string b)
        {
            string nameA = Path.GetFileNameWithoutExtension(a), nameB = Path.GetFileNameWithoutExtension(b);
            Match digitsA = Digits.Match(nameA), digitsB = Digits.Match(nameB);
            if (digitsA.Success && digitsB.Success && nameA.Substring(0, digitsA.Index) == nameB.Substring(0, digitsB.Index))
            {
                return int.Parse(digitsA.Value).CompareTo(int.Parse(digitsB.Value));
            }

            return string.CompareOrdinal(nameA, nameB);
        }

        private static byte[] ReadPixels(string assetPath, out int width, out int height)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(assetPath));
            width = texture.width;
            height = texture.height;
            Color32[] colors = texture.GetPixels32();
            Object.DestroyImmediate(texture);

            var bytes = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++)
            {
                bytes[i * 4] = colors[i].r;
                bytes[i * 4 + 1] = colors[i].g;
                bytes[i * 4 + 2] = colors[i].b;
                bytes[i * 4 + 3] = colors[i].a;
            }

            return bytes;
        }

        private static bool SameSize(ref int width, ref int height, int w, int h, string file)
        {
            if (width == 0)
            {
                width = w;
                height = h;
                return true;
            }

            if (w == width && h == height)
            {
                return true;
            }

            Debug.LogError($"Face textures not built: '{file}' is {w}x{h} but the other sprites in its folder are {width}x{height}.");
            return false;
        }

        private static Texture2DArray SaveArray(string name, int width, int height, TextureFormat format, bool linear, List<byte[]> layers)
        {
            var array = new Texture2DArray(width, height, layers.Count, format, false, linear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int i = 0; i < layers.Count; i++)
            {
                array.SetPixelData(layers[i], 0, i);
            }

            array.Apply(false, false);

            string path = $"{OutputFolder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(array, path);
                MakeNotReadable(array);
                return array;
            }

            EditorUtility.CopySerialized(array, existing);
            Object.DestroyImmediate(array);
            MakeNotReadable(existing);
            return existing;
        }

        private static void MakeNotReadable(Object asset)
        {
            var serialized = new SerializedObject(asset);
            SerializedProperty readable = serialized.FindProperty("m_IsReadable");
            if (readable != null)
            {
                readable.boolValue = false;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(asset);
        }

        private static void SaveCatalog(List<string> eyeFiles, List<string> mouthFiles)
        {
            string path = $"{OutputFolder}/FacePartCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<FacePartCatalog>(path);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<FacePartCatalog>();
                AssetDatabase.CreateAsset(catalog, path);
            }

            catalog.SetNames(Names(eyeFiles), Names(mouthFiles));
            EditorUtility.SetDirty(catalog);
        }

        private static IEnumerable<string> Names(List<string> files)
        {
            foreach (string file in files)
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        private static int AssignToMaterials(Texture2DArray eyes, Texture2DArray iris, Texture2DArray mouths)
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { RootFolder }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || material.shader == null || material.shader.name != ToonShaderName)
                {
                    continue;
                }

                material.SetTexture("_EyeArray", eyes);
                material.SetTexture("_EyeIrisArray", iris);
                material.SetTexture("_MouthArray", mouths);
                EditorUtility.SetDirty(material);
                count++;
            }

            return count;
        }
    }
}
