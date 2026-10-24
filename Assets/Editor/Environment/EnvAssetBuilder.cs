using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Describes one environment asset to bake into a mesh asset and a prefab.</summary>
    public sealed class EnvAssetDefinition
    {
        /// <summary>Creates a definition.</summary>
        /// <param name="area">Area folder such as Field, Coast, Desert, Mountain or Dungeon.</param>
        /// <param name="type">Type folder such as Trees, Stones, Flowers, Plants, Decor or Entrances.</param>
        /// <param name="name">Asset name without the area prefix.</param>
        /// <param name="materials">Material keys, one per submesh.</param>
        /// <param name="build">Geometry builder that fills the mesh.</param>
        /// <param name="alignToGround">Move the lowest vertex to y = 0.</param>
        /// <param name="collider">Add a mesh collider to the prefab.</param>
        /// <param name="blockHeight">Height of an invisible box collider over the footprint, or 0.</param>
        public EnvAssetDefinition(string area, string type, string name, string[] materials, Action<EnvMesh> build, bool alignToGround, bool collider, float blockHeight = 0f)
        {
            Area = area;
            Type = type;
            Name = name;
            Materials = materials;
            Build = build;
            AlignToGround = alignToGround;
            Collider = collider;
            BlockHeight = blockHeight;
        }

        /// <summary>Area folder name.</summary>
        public string Area { get; }

        /// <summary>Type folder name.</summary>
        public string Type { get; }

        /// <summary>Asset name without the area prefix.</summary>
        public string Name { get; }

        /// <summary>Material keys, one per submesh.</summary>
        public string[] Materials { get; }

        /// <summary>Geometry builder.</summary>
        public Action<EnvMesh> Build { get; }

        /// <summary>Whether the mesh is lifted so its lowest vertex sits at y = 0.</summary>
        public bool AlignToGround { get; }

        /// <summary>Whether the prefab receives a mesh collider.</summary>
        public bool Collider { get; }

        /// <summary>Gets the height of an invisible box collider covering the footprint, or 0 for none.</summary>
        public float BlockHeight { get; }

        /// <summary>Gets the prefab and mesh base name.</summary>
        public string FullName
        {
            get { return Area + "_" + Name; }
        }
    }

    /// <summary>Provides the asset definitions of one area.</summary>
    public interface IEnvCatalog
    {
        /// <summary>Gets the definitions of this catalog.</summary>
        /// <returns>All asset definitions.</returns>
        IEnumerable<EnvAssetDefinition> Definitions();
    }

    /// <summary>Base class with helpers to register definitions.</summary>
    public abstract class EnvCatalogBase : IEnvCatalog
    {
        private readonly List<EnvAssetDefinition> items = new List<EnvAssetDefinition>();

        /// <inheritdoc />
        public IEnumerable<EnvAssetDefinition> Definitions()
        {
            items.Clear();
            Register();
            return items;
        }

        /// <summary>Registers all definitions of the catalog.</summary>
        protected abstract void Register();

        /// <summary>Registers a definition.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="type">Type folder.</param>
        /// <param name="name">Asset name.</param>
        /// <param name="materials">Material keys.</param>
        /// <param name="build">Geometry builder.</param>
        /// <param name="align">Lift the lowest vertex to y = 0.</param>
        /// <param name="collider">Add a mesh collider.</param>
        /// <param name="blockHeight">Height of an invisible box collider over the footprint, or 0.</param>
        protected void Add(string area, string type, string name, string[] materials, Action<EnvMesh> build, bool align = true, bool collider = false, float blockHeight = 0f)
        {
            items.Add(new EnvAssetDefinition(area, type, name, materials, build, align, collider, blockHeight));
        }
    }

    /// <summary>Creates and caches the materials used by the environment assets.</summary>
    public static class EnvMaterials
    {
        /// <summary>Root folder of all environment assets.</summary>
        public const string Root = "Assets/Models/Environment";

        private static readonly Dictionary<string, string> ExternalTextures = new Dictionary<string, string>
        {
            { "leaf_pine", "Assets/Models/Environment/Common/Trees/Soft/Textures/leaf_pine.png" },
            { "leaf_pine_cool", "Assets/Models/Environment/Common/Trees/Soft/Textures/leaf_pine_cool.png" },
            { "leaf_round", "Assets/Models/Environment/Common/Trees/Soft/Textures/leaf_round.png" }
        };

        private static readonly Dictionary<string, float> Smoothness = new Dictionary<string, float>
        {
            { "ice", 0.6f }, { "snow", 0.25f }, { "metal", 0.35f }, { "pottery", 0.15f },
            { "rock_smooth_grey", 0.18f }, { "rock_smooth_warm", 0.12f }, { "rock_smooth_river", 0.3f }
        };

        /// <summary>Tells whether a key refers to an alpha-clipped card material.</summary>
        /// <param name="key">Material key.</param>
        /// <returns>True for leaf, grass and flower cards.</returns>
        public static bool IsCard(string key)
        {
            return key.StartsWith("leaf_") || key.StartsWith("grass_") || key.StartsWith("flower_");
        }

        /// <summary>Applies import settings to every environment texture.</summary>
        public static void ConfigureTextures()
        {
            foreach (string path in Directory.GetFiles(Root + "/Textures", "*.png", SearchOption.AllDirectories))
            {
                string assetPath = path.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                if (assetPath.EndsWith("_normal.png"))
                {
                    importer.textureType = TextureImporterType.NormalMap;
                }
                else if (assetPath.Contains("/Cards/"))
                {
                    importer.alphaIsTransparency = true;
                    importer.mipMapsPreserveCoverage = true;
                    importer.alphaTestReferenceValue = 0.5f;
                    importer.wrapMode = TextureWrapMode.Clamp;
                }

                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
        }

        /// <summary>Loads or creates the material for a key.</summary>
        /// <param name="key">Material key.</param>
        /// <returns>The material asset.</returns>
        public static Material Get(string key)
        {
            string folder = Root + "/Materials";
            Directory.CreateDirectory(folder);
            string path = folder + "/M_" + key + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            if (IsCard(key))
            {
                ConfigureCard(material, key);
            }
            else
            {
                ConfigureSolid(material, key);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureCard(Material material, string key)
        {
            string texturePath;
            if (!ExternalTextures.TryGetValue(key, out texturePath))
            {
                texturePath = Root + "/Textures/Cards/" + key + ".png";
            }

            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_Cull", 0f);
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        }

        private static void ConfigureSolid(Material material, string key)
        {
            string folder = Root + "/Textures/Solid/";
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(folder + key + "_albedo.png"));
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + key + "_normal.png");
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }

            float smoothness;
            material.SetFloat("_Smoothness", Smoothness.TryGetValue(key, out smoothness) ? smoothness : 0f);
            if (key == "ice")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.04f, 0.16f, 0.24f));
            }
        }
    }

    /// <summary>Editor tool that bakes every environment catalog into meshes, materials and prefabs.</summary>
    public static class EnvAssetBuilder
    {
        /// <summary>Builds all environment assets.</summary>
        [MenuItem("Tools/Environment/Build All Assets")]
        public static void BuildAll()
        {
            AssetDatabase.Refresh();
            EnvMaterials.ConfigureTextures();
            int count = 0;
            foreach (IEnvCatalog catalog in Catalogs())
            {
                foreach (EnvAssetDefinition definition in catalog.Definitions())
                {
                    BuildOne(definition);
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Environment assets built: " + count);
        }

        /// <summary>Lists all catalogs.</summary>
        /// <returns>The catalogs in build order.</returns>
        public static IEnumerable<IEnvCatalog> Catalogs()
        {
            yield return new FieldCatalog();
            yield return new CoastCatalog();
            yield return new DesertCatalog();
            yield return new MountainCatalog();
            yield return new DungeonCatalog();
            yield return new BarrierCatalog();
            yield return new SmoothStoneCatalog();
            yield return new ShrubCatalog();
            yield return new BridgeCatalog();
        }

        private static void BuildOne(EnvAssetDefinition definition)
        {
            string folder = EnvMaterials.Root + "/" + definition.Area + "/" + definition.Type;
            Directory.CreateDirectory(folder + "/Meshes");

            var data = new EnvMesh(definition.Materials.Length);
            definition.Build(data);
            Mesh generated = data.ToMesh(definition.FullName, definition.AlignToGround);

            string meshPath = folder + "/Meshes/" + definition.FullName + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                AssetDatabase.CreateAsset(generated, meshPath);
                mesh = generated;
            }
            else
            {
                mesh.Clear();
                EditorUtility.CopySerialized(generated, mesh);
                UnityEngine.Object.DestroyImmediate(generated);
                EditorUtility.SetDirty(mesh);
            }

            var materials = new Material[definition.Materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = EnvMaterials.Get(definition.Materials[i]);
            }

            var root = new GameObject(definition.FullName);
            root.AddComponent<MeshFilter>().sharedMesh = mesh;
            root.AddComponent<MeshRenderer>().sharedMaterials = materials;
            if (definition.Collider)
            {
                root.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            if (definition.BlockHeight > 0f)
            {
                var block = root.AddComponent<BoxCollider>();
                block.center = new Vector3(mesh.bounds.center.x, definition.BlockHeight * 0.5f, mesh.bounds.center.z);
                block.size = new Vector3(mesh.bounds.size.x, definition.BlockHeight, mesh.bounds.size.z);
            }

            PrefabUtility.SaveAsPrefabAsset(root, folder + "/" + definition.FullName + ".prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
