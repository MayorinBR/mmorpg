using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds color-coded primitive geometry used to block out maps.
    /// </summary>
    public static class GreyboxKit
    {
        private const string MaterialFolder = "Assets/Materials/Greybox";
        private const string UrpLitShader = "Universal Render Pipeline/Lit";
        private const string FallbackShader = "Standard";

        /// <summary>
        /// Gets a flat-colored material, creating the asset the first time it is requested.
        /// </summary>
        /// <param name="name">Material asset name.</param>
        /// <param name="color">Base color.</param>
        /// <returns>The material asset.</returns>
        public static Material GetMaterial(string name, Color color)
        {
            var material = LoadOrCreate(name);
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// Gets a glowing material for torches and other light sources.
        /// </summary>
        /// <param name="name">Material asset name.</param>
        /// <param name="color">Base color.</param>
        /// <param name="emission">Emission color.</param>
        /// <returns>The material asset.</returns>
        public static Material GetEmissiveMaterial(string name, Color color, Color emission)
        {
            var material = GetMaterial(name, color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            return material;
        }

        /// <summary>
        /// Gets a translucent material, such as water. The alpha of the color sets the opacity.
        /// </summary>
        /// <param name="name">Material asset name.</param>
        /// <param name="color">Base color with alpha.</param>
        /// <returns>The material asset.</returns>
        public static Material GetTransparentMaterial(string name, Color color)
        {
            var material = GetMaterial(name, color);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Smoothness", 0.9f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        /// <summary>
        /// Creates a primitive and places it in the active scene.
        /// </summary>
        /// <param name="type">Primitive shape.</param>
        /// <param name="name">Object name.</param>
        /// <param name="parent">Parent transform, expected to sit at the world origin without rotation or scale.</param>
        /// <param name="position">World position of the object center.</param>
        /// <param name="scale">Local scale.</param>
        /// <param name="material">Material to render with.</param>
        /// <param name="rotation">Optional world rotation.</param>
        /// <param name="layer">Layer to assign.</param>
        /// <param name="keepCollider">True to keep the primitive collider, for example on floors.</param>
        /// <returns>The new object.</returns>
        public static GameObject Create(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material,
            Quaternion? rotation = null,
            int layer = 0,
            bool keepCollider = false)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.layer = layer;
            shape.transform.SetParent(parent, false);
            shape.transform.SetPositionAndRotation(position, rotation ?? Quaternion.identity);
            shape.transform.localScale = scale;
            shape.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Object.DestroyImmediate(shape.GetComponent<Collider>());
            }

            return shape;
        }

        private static Material LoadOrCreate(string name)
        {
            AssetFolders.Ensure(MaterialFolder);
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find(UrpLitShader) ?? Shader.Find(FallbackShader);
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
