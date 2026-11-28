using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools
{
    /// <summary>
    /// Turns the trees painted with the terrain tree brush into prefab instances, so they block the NavMesh
    /// like the scenery placed as objects, then rebakes the NavMesh of the scene. The NavMesh bake ignores
    /// painted tree instances, so they would otherwise stay walkable.
    /// </summary>
    public static class TerrainTreeConverter
    {
        private const string GroupName = "Trees";
        private const string FallbackGroupName = "TerrainTrees";
        private const string SurfaceTypeName = "NavMeshSurface";

        /// <summary>
        /// Converts the painted trees of every terrain in the active scene, keeping their position, heading
        /// and scale, clears them from the terrain and rebakes the scene NavMesh.
        /// </summary>
        [MenuItem("Tools/Maps/Convert Painted Trees To Objects")]
        public static void ConvertActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var parent = FindTreeGroup(roots);
            var converted = roots.SelectMany(root => root.GetComponentsInChildren<Terrain>(true)).Sum(terrain => Convert(terrain, parent));

            if (converted == 0)
            {
                Debug.Log("TerrainTreeConverter: no painted trees in the active scene.");
                return;
            }

            var surfaceOwner = roots
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .FirstOrDefault(component => component != null && component.GetType().Name == SurfaceTypeName)?.gameObject;

            if (surfaceOwner != null)
            {
                var fallbackPath = Path.Combine(Path.GetDirectoryName(scene.path), scene.name, "NavMesh-Terrain.asset").Replace('\\', '/');
                NavMeshBaker.Bake(surfaceOwner, fallbackPath);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"TerrainTreeConverter: {converted} painted trees converted to objects under '{parent.name}'" + (surfaceOwner != null ? " and the NavMesh was rebaked." : ". No NavMeshSurface found, bake it by hand."));
        }

        /// <summary>
        /// Instances every painted tree of <paramref name="terrain"/> under <paramref name="parent"/> and removes
        /// the painted instances. Tree prototypes stay on the terrain so the brush still works.
        /// </summary>
        /// <param name="terrain">Terrain whose painted trees are converted.</param>
        /// <param name="parent">Group that receives the new objects.</param>
        /// <returns>The number of trees converted.</returns>
        private static int Convert(Terrain terrain, Transform parent)
        {
            var data = terrain.terrainData;
            var prototypes = data.treePrototypes;
            var origin = terrain.transform.position;
            var converted = 0;

            foreach (var tree in data.treeInstances)
            {
                var prefab = prototypes[tree.prototypeIndex].prefab;

                if (prefab == null)
                {
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(instance, "Convert painted trees");
                instance.transform.SetPositionAndRotation(origin + Vector3.Scale(tree.position, data.size), Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f));
                instance.transform.localScale = Vector3.Scale(prefab.transform.localScale, new Vector3(tree.widthScale, tree.heightScale, tree.widthScale));
                converted++;
            }

            if (converted > 0)
            {
                Undo.RegisterCompleteObjectUndo(data, "Convert painted trees");
                data.treeInstances = new TreeInstance[0];
                EditorUtility.SetDirty(data);
            }

            return converted;
        }

        /// <summary>
        /// Returns the scene's existing "Trees" scenery group (a direct child of a root object), or a new root
        /// group when the scene has none.
        /// </summary>
        /// <param name="roots">Root objects of the active scene.</param>
        /// <returns>The group that receives the converted trees.</returns>
        private static Transform FindTreeGroup(GameObject[] roots)
        {
            var existing = roots.Select(root => root.transform.Find(GroupName)).FirstOrDefault(group => group != null);

            if (existing != null)
            {
                return existing;
            }

            var created = new GameObject(FallbackGroupName);
            Undo.RegisterCreatedObjectUndo(created, "Convert painted trees");
            return created.transform;
        }
    }
}
