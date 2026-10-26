using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace City2.Editor
{
    /// <summary>
    /// Makes the cloth-like parts of the City2 models (banners, awnings, canopies) visible from both sides.
    /// The mesh itself receives a reversed copy of each cloth triangle, and the material has back-face culling disabled.
    /// </summary>
    public sealed class City2ClothMaterialPostprocessor : AssetPostprocessor
    {
        private const string ModelFolder = "Assets/Models/Cities/City2";
        private const string CullProperty = "_Cull";
        private static readonly string[] ClothPrefixes = { "Banner", "Awning", "Canopy" };

        /// <summary>Changing this value makes Unity reimport every model handled by this postprocessor.</summary>
        /// <returns>The postprocessor version.</returns>
        public override uint GetVersion()
        {
            return 3;
        }

        /// <summary>Reimports every City2 model, forcing the cloth fix to run again.</summary>
        [MenuItem("Tools/City2/Apply Double-Sided Cloth")]
        public static void ReimportModels()
        {
            var paths = AssetDatabase.FindAssets("t:Model", new[] { ModelFolder }).Select(AssetDatabase.GUIDToAssetPath);
            foreach (var path in paths)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        private static bool IsCloth(Material material)
        {
            return material != null && ClothPrefixes.Any(prefix => material.name.StartsWith(prefix));
        }

        private void OnPostprocessMaterial(Material material)
        {
            if (assetPath.StartsWith(ModelFolder) && IsCloth(material) && material.HasProperty(CullProperty))
            {
                material.SetFloat(CullProperty, 0f);
            }
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(ModelFolder))
            {
                return;
            }

            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer != null && filter.sharedMesh != null)
                {
                    DuplicateClothBackFaces(filter.sharedMesh, renderer.sharedMaterials);
                }
            }
        }

        private static void DuplicateClothBackFaces(Mesh mesh, Material[] materials)
        {
            var clothSubMeshes = Enumerable.Range(0, Mathf.Min(mesh.subMeshCount, materials.Length))
                .Where(index => IsCloth(materials[index]))
                .ToArray();
            if (clothSubMeshes.Length == 0)
            {
                return;
            }

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            mesh.GetVertices(vertices);
            mesh.GetNormals(normals);
            mesh.GetUVs(0, uvs);
            var originalCount = vertices.Count;
            var hasUvs = uvs.Count == originalCount;

            var triangles = Enumerable.Range(0, mesh.subMeshCount).Select(index => mesh.GetTriangles(index).ToList()).ToArray();
            foreach (var index in clothSubMeshes)
            {
                var source = triangles[index];
                var reversed = new List<int>(source.Count);
                for (var i = 0; i < source.Count; i += 3)
                {
                    reversed.Add(source[i] + originalCount);
                    reversed.Add(source[i + 2] + originalCount);
                    reversed.Add(source[i + 1] + originalCount);
                }

                source.AddRange(reversed);
            }

            vertices.AddRange(vertices.Take(originalCount).ToArray());
            normals.AddRange(normals.Take(originalCount).Select(normal => -normal).ToArray());
            if (hasUvs)
            {
                uvs.AddRange(uvs.Take(originalCount).ToArray());
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            if (hasUvs)
            {
                mesh.SetUVs(0, uvs);
            }

            for (var index = 0; index < triangles.Length; index++)
            {
                mesh.SetTriangles(triangles[index], index);
            }
        }
    }
}
