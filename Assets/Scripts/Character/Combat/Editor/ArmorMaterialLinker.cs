using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Gives an equipment model its toon material: maps each material found in the model to a
    /// shared material asset (created from an existing one when missing) and fills the base map
    /// when a matching <c>_BaseColor.png</c> sits next to the model. Textures are never created.
    /// </summary>
    public static class ArmorMaterialLinker
    {
        private const string MaterialsFolder = "Assets/Materials/Equipament";
        private const string BaseMapProperty = "_BaseMap";
        private const string BaseColorProperty = "_BaseColor";

        /// <summary>
        /// Links every material of a model to a toon material and applies its texture when present.
        /// </summary>
        /// <param name="modelPath">Project relative path of the FBX.</param>
        /// <param name="materialOverride">Set name that replaces the model material name, or empty.</param>
        public static void Link(string modelPath, string materialOverride)
        {
            if (AssetImporter.GetAtPath(modelPath) is not ModelImporter importer)
            {
                return;
            }

            var changed = false;
            var folder = Path.GetDirectoryName(modelPath).Replace('\\', '/');

            foreach (var sourceName in FindSourceMaterialNames(modelPath, importer))
            {
                var identifier = new AssetImporter.SourceAssetIdentifier(typeof(Material), sourceName);
                var map = importer.GetExternalObjectMap();
                map.TryGetValue(identifier, out var mapped);
                var material = mapped as Material;

                if (material == null)
                {
                    var setName = string.IsNullOrEmpty(materialOverride) ? sourceName : materialOverride;
                    material = LoadOrCreate($"{MaterialsFolder}/{setName}_Toon.mat");

                    if (material == null)
                    {
                        continue;
                    }

                    importer.AddRemap(identifier, material);
                    changed = true;
                }

                ApplyTexture(material, folder, string.IsNullOrEmpty(materialOverride) ? sourceName : materialOverride);
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static IEnumerable<string> FindSourceMaterialNames(string modelPath, ModelImporter importer)
        {
            var embedded = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Material>().Select(m => m.name);
            var remapped = importer.GetExternalObjectMap()
                .Where(pair => pair.Key.type == typeof(Material))
                .Select(pair => pair.Key.name);
            return embedded.Concat(remapped).Distinct().ToList();
        }

        private static Material LoadOrCreate(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (existing != null)
            {
                return existing;
            }

            var template = FindTemplate();

            if (template == null)
            {
                Debug.LogWarning($"Equipment: no template material in {MaterialsFolder}, create {path} by hand.");
                return null;
            }

            var material = new Material(template);
            material.SetTexture(BaseMapProperty, null);
            material.SetColor(BaseColorProperty, Color.white);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material FindTemplate()
        {
            return AssetDatabase.FindAssets("t:Material _Toon", new[] { MaterialsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .FirstOrDefault(m => m != null);
        }

        private static void ApplyTexture(Material material, string folder, string setName)
        {
            if (material.GetTexture(BaseMapProperty) != null)
            {
                return;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/{setName}_BaseColor.png");

            if (texture == null)
            {
                return;
            }

            material.SetTexture(BaseMapProperty, texture);
            material.SetColor(BaseColorProperty, Color.white);
            EditorUtility.SetDirty(material);
        }
    }
}
