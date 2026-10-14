using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Stage 2 of the equipment pipeline. Finds every equipment model that already has a painted
    /// <c>&lt;Name&gt;_BaseColor.png</c> next to it, limits the texture import size, creates the toon material
    /// and links it to the model.
    /// </summary>
    public static class EquipmentMaterialApplier
    {
        private const string ModelsRoot = "Assets/Models/Items/Equipaments";
        private const string MaterialsFolder = "Assets/Materials/Equipament";
        private const string TemplateMaterialPath = "Assets/Materials/Equipament/BlacksmithPants_Toon.mat";
        private const string TextureSuffix = "_BaseColor.png";
        private const string MaterialSuffix = "_Toon.mat";
        private const string BaseMapProperty = "_BaseMap";
        private const int MaxTextureSize = 1024;

        /// <summary>Applies materials to every equipment model that has a painted texture.</summary>
        [MenuItem("Tools/Equipment/Apply Item Materials")]
        public static void ApplyAll()
        {
            var template = AssetDatabase.LoadAssetAtPath<Material>(TemplateMaterialPath);
            if (template == null)
            {
                Debug.LogError($"Template material not found at {TemplateMaterialPath}");
                return;
            }

            var builder = new EquipmentMaterialBuilder(template, MaterialsFolder, MaterialSuffix, BaseMapProperty);
            var linker = new EquipmentMaterialLinker();
            var textureConfigurator = new EquipmentTextureConfigurator(MaxTextureSize);
            int applied = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ModelsRoot }))
            {
                string modelPath = AssetDatabase.GUIDToAssetPath(guid);
                string itemName = Path.GetFileNameWithoutExtension(modelPath);
                string texturePath = Path.Combine(Path.GetDirectoryName(modelPath), itemName + TextureSuffix).Replace('\\', '/');

                if (!textureConfigurator.Configure(texturePath))
                {
                    continue;
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                Material material = builder.CreateOrUpdate(itemName, texture);
                if (linker.Link(modelPath, itemName, material))
                {
                    applied++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Equipment materials applied: {applied}");
        }
    }

    /// <summary>Limits the import resolution of a painted texture without changing the source file.</summary>
    public sealed class EquipmentTextureConfigurator
    {
        private readonly int maxSize;

        /// <summary>Creates a configurator.</summary>
        /// <param name="maxSize">Largest allowed texture size, in pixels, on both axes.</param>
        public EquipmentTextureConfigurator(int maxSize)
        {
            this.maxSize = maxSize;
        }

        /// <summary>Sets the importer max size and reimports the texture when it changed.</summary>
        /// <param name="texturePath">Asset path of the texture.</param>
        /// <returns>True when the texture exists and now respects the size limit.</returns>
        public bool Configure(string texturePath)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            if (importer.maxTextureSize != maxSize)
            {
                importer.maxTextureSize = maxSize;
                importer.SaveAndReimport();
            }

            return true;
        }
    }

    /// <summary>Creates the toon material of an item from a template and assigns its base texture.</summary>
    public sealed class EquipmentMaterialBuilder
    {
        private readonly Material template;
        private readonly string folder;
        private readonly string suffix;
        private readonly string baseMapProperty;

        /// <summary>Creates a builder.</summary>
        /// <param name="template">Material whose shader and toon settings are copied.</param>
        /// <param name="folder">Asset folder that receives the materials.</param>
        /// <param name="suffix">File suffix of the generated materials, including the extension.</param>
        /// <param name="baseMapProperty">Shader property that receives the painted texture.</param>
        public EquipmentMaterialBuilder(Material template, string folder, string suffix, string baseMapProperty)
        {
            this.template = template;
            this.folder = folder;
            this.suffix = suffix;
            this.baseMapProperty = baseMapProperty;
        }

        /// <summary>Creates the item material, or updates its texture when it already exists.</summary>
        /// <param name="itemName">Item name, also the material name inside the model.</param>
        /// <param name="texture">Painted base color texture.</param>
        /// <returns>The material asset.</returns>
        public Material CreateOrUpdate(string itemName, Texture2D texture)
        {
            string path = $"{folder}/{itemName}{suffix}";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(template) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetTexture(baseMapProperty, texture);
            EditorUtility.SetDirty(material);
            return material;
        }
    }

    /// <summary>Maps the material embedded in a model to a project material.</summary>
    public sealed class EquipmentMaterialLinker
    {
        /// <summary>Links the material to the model importer and reimports the model when the link changed.</summary>
        /// <param name="modelPath">Asset path of the model.</param>
        /// <param name="embeddedMaterialName">Name of the material stored inside the FBX.</param>
        /// <param name="material">Project material to use.</param>
        /// <returns>True when the model now uses the material.</returns>
        public bool Link(string modelPath, string embeddedMaterialName, Material material)
        {
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
            {
                return false;
            }

            var identifier = new AssetImporter.SourceAssetIdentifier(typeof(Material), embeddedMaterialName);
            if (importer.GetExternalObjectMap().TryGetValue(identifier, out UnityEngine.Object current) && current == material)
            {
                return true;
            }

            importer.AddRemap(identifier, material);
            importer.SaveAndReimport();
            return true;
        }
    }
}
