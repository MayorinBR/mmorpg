using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace City2.Editor
{
    /// <summary>
    /// Raw terrain data baked for the City2 island: normalized heights and one layer index per alphamap texel.
    /// </summary>
    public sealed class City2TerrainSource
    {
        private const string DataFolder = "Assets/Terrains/City2";

        /// <summary>Gets the heightmap resolution in samples.</summary>
        public const int HeightResolution = 513;

        /// <summary>Gets the alphamap resolution in texels.</summary>
        public const int AlphamapResolution = 512;

        /// <summary>Gets a value indicating whether the baked data files exist.</summary>
        public static bool Exists => File.Exists(Path.Combine(DataFolder, "height.bytes"));

        /// <summary>Reads the normalized heights as a [z, x] array.</summary>
        /// <returns>The heights ready for <see cref="TerrainData.SetHeights"/>.</returns>
        public float[,] ReadHeights()
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(DataFolder, "height.bytes"));
            var heights = new float[HeightResolution, HeightResolution];
            System.Buffer.BlockCopy(bytes, 0, heights, 0, bytes.Length);
            return heights;
        }

        /// <summary>Reads the layer index of every alphamap texel as a [z, x] array.</summary>
        /// <returns>The layer indices.</returns>
        public byte[,] ReadLayerIndices()
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(DataFolder, "splat.bytes"));
            var indices = new byte[AlphamapResolution, AlphamapResolution];
            System.Buffer.BlockCopy(bytes, 0, indices, 0, bytes.Length);
            return indices;
        }
    }

    /// <summary>
    /// Applies the baked City2 data to a <see cref="TerrainData"/>: size, heights, terrain layers and splatmap.
    /// </summary>
    public sealed class City2TerrainBuilder
    {
        private const string LayerFolder = "Assets/Terrains/City2/Layers/";
        private static readonly string[] LayerNames = { "Grass", "Cobble", "Road", "Sand", "Seabed" };
        private const string SharedGroundSource = "Assets/Terrains/T_Prototype_Map03.asset";
        private const int GrassIndex = 0;
        private const int RoadIndex = 2;
        private static readonly Vector3 TerrainSize = new Vector3(200f, 6f, 200f);

        private readonly City2TerrainSource source;

        /// <summary>Initializes a new instance of the <see cref="City2TerrainBuilder"/> class.</summary>
        /// <param name="source">The baked data source.</param>
        public City2TerrainBuilder(City2TerrainSource source)
        {
            this.source = source;
        }

        /// <summary>Rebuilds heights, layers and splatmap on the given terrain data.</summary>
        /// <param name="data">The terrain data to modify.</param>
        public void Build(TerrainData data)
        {
            data.heightmapResolution = City2TerrainSource.HeightResolution;
            data.size = TerrainSize;
            data.SetHeights(0, 0, source.ReadHeights());
            data.terrainLayers = LoadLayers();
            data.alphamapResolution = City2TerrainSource.AlphamapResolution;
            data.SetAlphamaps(0, 0, BuildAlphamaps(source.ReadLayerIndices(), LayerNames.Length));
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static TerrainLayer[] LoadLayers()
        {
            var layers = new TerrainLayer[LayerNames.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"{LayerFolder}City2_{LayerNames[i]}.terrainlayer");
            }

            var shared = LoadSharedGroundLayers();
            if (shared != null)
            {
                layers[GrassIndex] = shared[0];
                layers[RoadIndex] = shared[1];
            }

            return layers;
        }

        /// <summary>Gets the grass and dirt layers shared with the plains maps, or null when they are unavailable.</summary>
        /// <returns>A two element array with grass first and dirt second.</returns>
        public static TerrainLayer[] LoadSharedGroundLayers()
        {
            var plains = AssetDatabase.LoadAssetAtPath<TerrainData>(SharedGroundSource);
            var layers = plains != null ? plains.terrainLayers.Where(layer => layer != null).ToArray() : new TerrainLayer[0];
            return layers.Length >= 2 ? new[] { layers[0], layers[1] } : null;
        }

        private static float[,,] BuildAlphamaps(byte[,] indices, int layerCount)
        {
            int size = indices.GetLength(0);
            var maps = new float[size, size, layerCount];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    maps[z, x, indices[z, x]] = 1f;
                }
            }

            return maps;
        }
    }

    /// <summary>
    /// Editor entry points that build the City2 terrain on demand and automatically whenever its ground layers are outdated.
    /// </summary>
    public static class City2TerrainMenu
    {
        private const string TerrainAssetPath = "Assets/Terrains/City2.asset";
        private const string SceneName = "Prototype_City2";
        private const string TerrainObjectName = "City2";
        private const string NavMeshFallbackPath = "Assets/Scenes/Prototype_City2/NavMesh-City2.asset";
        private const string PendingBakeKey = "City2.PendingNavMeshBake";

        /// <summary>Rebuilds the City2 terrain from the baked data.</summary>
        [MenuItem("Tools/City2/Build Terrain")]
        public static void BuildTerrain()
        {
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainAssetPath);
            if (data == null || !City2TerrainSource.Exists)
            {
                Debug.LogError($"City2 terrain build skipped: missing {TerrainAssetPath} or baked data.");
                return;
            }

            new City2TerrainBuilder(new City2TerrainSource()).Build(data);
            EditorPrefs.SetBool(PendingBakeKey, true);
            Debug.Log("City2 terrain built.");
            BakePendingNavMesh();
        }

        /// <summary>Rebuilds the NavMesh of the City2 terrain object without touching its layer mask.</summary>
        [MenuItem("Tools/City2/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            var terrain = GameObject.Find(TerrainObjectName);
            var surface = terrain != null ? terrain.GetComponents<Component>().FirstOrDefault(c => c != null && c.GetType().Name == "NavMeshSurface") : null;
            var type = surface != null ? surface.GetType() : null;
            var build = type?.GetMethod("BuildNavMesh", System.Type.EmptyTypes);
            var dataProperty = type?.GetProperty("navMeshData");
            if (build == null || dataProperty == null)
            {
                Debug.LogWarning($"City2 NavMesh bake skipped: open the {SceneName} scene so the NavMeshSurface on '{TerrainObjectName}' can be found.");
                return;
            }

            var previous = dataProperty.GetValue(surface) as Object;
            var path = previous != null ? AssetDatabase.GetAssetPath(previous) : NavMeshFallbackPath;
            build.Invoke(surface, null);
            if (dataProperty.GetValue(surface) is Object built && !AssetDatabase.Contains(built))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(built, path);
            }

            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(terrain.scene);
            EditorPrefs.DeleteKey(PendingBakeKey);
            Debug.Log("City2 NavMesh baked.");
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorSceneManager.sceneOpened += (scene, _) => BakePendingNavMesh();
            EditorApplication.delayCall += () =>
            {
                if (NeedsBuild())
                {
                    BuildTerrain();
                }
                else
                {
                    BakePendingNavMesh();
                }
            };
        }

        private static bool NeedsBuild()
        {
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainAssetPath);
            if (data == null || !City2TerrainSource.Exists || AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Terrains/City2/Layers/City2_Grass.terrainlayer") == null)
            {
                return false;
            }

            var shared = City2TerrainBuilder.LoadSharedGroundLayers();
            var current = data.terrainLayers;
            return current.Length == 0 || (shared != null && current[0] != shared[0]);
        }

        private static void BakePendingNavMesh()
        {
            if (EditorPrefs.GetBool(PendingBakeKey, false) && EditorSceneManager.GetActiveScene().name == SceneName
                && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                BakeNavMesh();
            }
        }
    }
}
