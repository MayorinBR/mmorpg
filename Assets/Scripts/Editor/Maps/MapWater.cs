using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Creates the flat water surfaces of the maps.</summary>
    public static class MapWater
    {
        private const string WaterMaterialPath = "Assets/Materials/Greybox/Plains_Water.mat";

        /// <summary>Creates a water plane with the shared water material and no collider.</summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="name">Name of the new object.</param>
        /// <param name="center">World center of the surface.</param>
        /// <param name="sizeX">Extent along X.</param>
        /// <param name="sizeZ">Extent along Z.</param>
        /// <param name="layer">Layer of the object.</param>
        /// <returns>The new water object.</returns>
        public static GameObject Create(Transform parent, string name, Vector3 center, float sizeX, float sizeZ, int layer)
        {
            var water = new GameObject(name) { layer = layer };
            water.transform.SetParent(parent, false);
            water.transform.position = center;
            var mesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");
            water.transform.localScale = new Vector3(sizeX / mesh.bounds.size.x, 1f, sizeZ / mesh.bounds.size.z);
            water.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = water.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return water;
        }
    }
}
