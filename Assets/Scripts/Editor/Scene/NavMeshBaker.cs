using System;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Bakes a scene's NavMeshSurface from code and keeps its data stored as an asset next to the scene.
    /// </summary>
    public static class NavMeshBaker
    {
        private const string SurfaceTypeName = "NavMeshSurface";
        private const string DataPropertyName = "navMeshData";
        private const string BuildMethodName = "BuildNavMesh";
        private const string LayerMaskPropertyName = "layerMask";

        /// <summary>Layer for decoration that must not affect the NavMesh, such as grass, flowers and water.</summary>
        public const int DecorationLayer = 2;

        /// <summary>Layer of water surfaces, which never produce walkable NavMesh.</summary>
        public const int WaterLayer = 4;

        /// <summary>
        /// Rebuilds the NavMesh of the surface found on the given object.
        /// </summary>
        /// <param name="owner">The object that carries the NavMeshSurface.</param>
        /// <param name="fallbackAssetPath">Where to store the data when the surface had none.</param>
        /// <returns>True when a NavMesh was built and stored.</returns>
        public static bool Bake(GameObject owner, string fallbackAssetPath)
        {
            var surface = EditorReflection.FindByTypeName(owner, SurfaceTypeName);
            var dataProperty = surface?.GetType().GetProperty(DataPropertyName);
            var build = surface?.GetType().GetMethod(BuildMethodName, Type.EmptyTypes);

            if (surface == null || dataProperty == null || build == null)
            {
                Debug.LogError($"NavMeshBaker: no NavMeshSurface with {BuildMethodName} on '{owner.name}'. Bake it from the Inspector.");
                return false;
            }

            var previous = dataProperty.GetValue(surface) as UnityEngine.Object;
            var path = previous != null ? AssetDatabase.GetAssetPath(previous) : fallbackAssetPath;
            surface.GetType().GetProperty(LayerMaskPropertyName)?.SetValue(surface, (LayerMask)~((1 << DecorationLayer) | (1 << WaterLayer)));
            build.Invoke(surface, null);

            if (!(dataProperty.GetValue(surface) is UnityEngine.Object built))
            {
                Debug.LogError("NavMeshBaker: the bake produced no NavMesh data.");
                return false;
            }

            if (!AssetDatabase.Contains(built))
            {
                AssetFolders.Ensure(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(built, path);
            }

            EditorUtility.SetDirty(surface);
            return true;
        }
    }
}
