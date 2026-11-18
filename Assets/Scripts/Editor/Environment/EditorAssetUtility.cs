using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.EditorTools
{
    /// <summary>
    /// Small helpers shared by the environment bakers to create or refresh assets in an idempotent way.
    /// </summary>
    public static class EditorAssetUtility
    {
        /// <summary>
        /// Creates every missing folder of an asset path.
        /// </summary>
        /// <param name="folder">Folder path relative to the project, for example Assets/Materials/Water.</param>
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        /// <summary>
        /// Saves a mesh as an asset, overwriting the previous mesh at that path in place so scene references survive.
        /// </summary>
        /// <param name="mesh">Mesh to store. It is destroyed when an asset already existed.</param>
        /// <param name="path">Asset path of the mesh.</param>
        /// <returns>The persistent mesh asset.</returns>
        public static Mesh SaveMesh(Mesh mesh, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        /// <summary>
        /// Loads the material at a path or creates it, then applies the configuration.
        /// </summary>
        /// <param name="path">Asset path of the material.</param>
        /// <param name="shaderName">Name of the shader the material must use.</param>
        /// <param name="configure">Callback that sets the material properties.</param>
        /// <returns>The persistent material asset.</returns>
        /// <exception cref="InvalidOperationException">The shader cannot be found.</exception>
        public static Material EnsureMaterial(string path, string shaderName, Action<Material> configure)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader not found: {shaderName}");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            configure(material);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
