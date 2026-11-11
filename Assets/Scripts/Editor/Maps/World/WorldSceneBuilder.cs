using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools.Maps.World
{
    /// <summary>Creates or reshapes the scene of one map from its baked world data.</summary>
    public sealed class WorldSceneBuilder
    {
        private const string TemplateScene = "Assets/Scenes/Maps/Map_6_4_DrySteppe.unity";
        private const string SceneFolder = "Assets/Scenes/Maps";
        private const string DataFolder = "Assets/Terrains/Maps/World";
        private const string EnvironmentFolder = "Assets/Models/Environment";
        private const string PropPrefix = "W_";
        private const string WaterRoot = "WorldWater";
        private const string NavBlockRoot = "WorldNavBlocks";
        private const int DecorationLayer = 2;
        private const int WaterLayer = 4;
        private const float PortalLift = 0.2f;
        private const float ArrivalOffset = 3f;
        private const float BandDistance = 20f;
        private const float FloatTolerance = 0.8f;
        private const float OldDecorThinning = 0.6f;
        private const float WetBlockDepth = 0.7f;
        private const float SeaLevel = -0.5f;

        private static readonly string[] DecorGroups = { "Trees", "Rocks", "Plants", "Shrubs", "Flowers", "Boundary", "Bridges" };

        private readonly WorldBuildData world;
        private readonly PrefabLibrary library;

        /// <summary>Initializes a new instance of the <see cref="WorldSceneBuilder"/> class.</summary>
        /// <param name="world">The baked world data.</param>
        public WorldSceneBuilder(WorldBuildData world)
        {
            this.world = world;
            library = new PrefabLibrary(EnvironmentFolder);
        }

        /// <summary>Builds or updates the scene of one map and saves it.</summary>
        /// <param name="info">The manifest entry of the map.</param>
        /// <returns>A short report line.</returns>
        public string Build(WorldCellInfo info)
        {
            WorldCellData cell = world.Load(info);
            string path = info.existing ? FindScenePath(info.name) : PrepareNewScene(info);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Terrain terrain = SceneHelper.FindTerrain(scene);
            var baked = new BakedMapTerrain(cell, world.Manifest.layers);
            List<Vector2> gates = info.portals.Select(portal => new Vector2(portal.x, portal.z)).ToList();
            gates.AddRange(FindPortalPositions(scene));

            Rect previousArea = new Rect(-MapMath.Half, -MapMath.Half, MapMath.Size, MapMath.Size);
            if (info.existing)
            {
                previousArea = new ExistingTerrainUpdater().Apply(terrain, cell, baked);
            }
            else
            {
                PrepareNewRoots(scene, info);
                UseOwnTerrainData(terrain, info);
                new MapTerrainWriter().Write(terrain, baked);
            }

            BuildWater(scene, cell, info);

            int props = PlaceProps(scene, terrain, cell, info, previousArea);
            int portals = PlacePortals(scene, terrain, info);
            BuildNavBlocks(scene, cell, gates);
            if (!info.existing)
            {
                PlaceRespawn(scene, terrain, info);
                Register(info, path);
            }

            bool baking = BakeNavMesh(terrain, info);
            EditorSceneManager.SaveScene(scene);
            return $"{info.name} props={props} portals={portals} nav={baking}";
        }

        private static string FindScenePath(string sceneName)
        {
            string guid = AssetDatabase.FindAssets(sceneName + " t:Scene").FirstOrDefault(
                candidate => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(candidate)) == sceneName);
            return guid == null ? throw new InvalidOperationException("Scene not found: " + sceneName) : AssetDatabase.GUIDToAssetPath(guid);
        }

        private static string PrepareNewScene(WorldCellInfo info)
        {
            string path = $"{SceneFolder}/{info.name}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null && !AssetDatabase.CopyAsset(TemplateScene, path))
            {
                throw new InvalidOperationException("Could not copy the template scene to " + path);
            }

            return path;
        }

        private static IEnumerable<Vector2> FindPortalPositions(Scene scene)
        {
            return scene.GetRootGameObjects()
                .Where(root => root.name.StartsWith("WarpPortal_To"))
                .Select(root => new Vector2(root.transform.position.x, root.transform.position.z));
        }

        private static void PrepareNewRoots(Scene scene, WorldCellInfo info)
        {
            SceneHelper.DeleteRoots(scene, "EnemySpawn", WaterRoot, NavBlockRoot);
            string container = $"Map_{info.r}_{info.c}";
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name.EndsWith("RespawnPoint"))
                {
                    root.name = container + "RespawnPoint";
                }
                else if (root.name.StartsWith("Map_") && root.name != info.name && root.name.Count(character => character == '_') == 2)
                {
                    root.name = container;
                    foreach (Transform group in root.transform)
                    {
                        for (int i = group.childCount - 1; i >= 0; i--)
                        {
                            UnityEngine.Object.DestroyImmediate(group.GetChild(i).gameObject);
                        }
                    }
                }
            }
        }

        private static void UseOwnTerrainData(Terrain terrain, WorldCellInfo info)
        {
            string dataPath = $"{DataFolder}/{info.name}_Terrain.asset";
            if (AssetDatabase.GetAssetPath(terrain.terrainData) == dataPath)
            {
                return;
            }

            AssetFolders.Ensure(DataFolder);
            var data = new TerrainData();
            AssetDatabase.CreateAsset(data, dataPath);
            terrain.terrainData = data;
            var collider = terrain.GetComponent<TerrainCollider>();
            if (collider != null)
            {
                collider.terrainData = data;
            }

            ClearNavMeshReference(terrain.gameObject);
        }

        private static void ClearNavMeshReference(GameObject owner)
        {
            Component surface = owner.GetComponents<Component>().FirstOrDefault(component => component != null && component.GetType().Name == "NavMeshSurface");
            if (surface == null)
            {
                return;
            }

            var serialized = new SerializedObject(surface);
            serialized.FindProperty("m_NavMeshData").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildWater(Scene scene, WorldCellData cell, WorldCellInfo info)
        {
            SceneHelper.DeleteRoots(scene, WaterRoot);
            var root = new GameObject(WaterRoot);
            SceneManager.MoveGameObjectToScene(root, scene);
            if (!info.existing && cell.Info.lo < SeaLevel)
            {
                MapWater.Create(root.transform, "Sea", new Vector3(0f, SeaLevel, 0f), MapMath.Size, MapMath.Size, WaterLayer);
            }

            AssetFolders.Ensure(DataFolder);
            RasterWaterMesh.Create(root.transform, "Rivers", cell.Water, $"{DataFolder}/{info.name}_Water.asset", WaterLayer);
        }

        private int PlaceProps(Scene scene, Terrain terrain, WorldCellData cell, WorldCellInfo info, Rect previousArea)
        {
            Transform container = FindContainer(scene, info);
            foreach (string name in DecorGroups)
            {
                Transform group = container.Find(name);
                if (group == null)
                {
                    continue;
                }

                for (int i = group.childCount - 1; i >= 0; i--)
                {
                    Transform child = group.GetChild(i);
                    Vector2 point = new Vector2(child.position.x, child.position.z);
                    bool generated = child.name.StartsWith(PropPrefix);
                    bool outside = MapMath.EdgeDistance(point) < 0f;
                    bool inBand = name != "Boundary" && Mathf.Min(MapMath.EdgeDistance(point), EdgeDistance(previousArea, point)) < BandDistance;
                    if (generated || outside || (info.existing && inBand) || (info.existing && IsStranded(terrain, child)) || (info.existing && IsThinned(info, name, point)))
                    {
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    }
                }
            }

            var placer = new PropPlacer(terrain);
            int placed = 0;
            int missing = 0;
            foreach (WorldProp prop in cell.Props)
            {
                GameObject prefab;
                try
                {
                    prefab = library.Get(prop.Prefab);
                }
                catch (KeyNotFoundException)
                {
                    missing++;
                    continue;
                }

                bool shrub = prop.Prefab.Contains("Bush") || prop.Prefab.Contains("Cactus");
                bool rock = prop.Prefab.Contains("Boulder") || prop.Prefab.Contains("Rock") || prop.Prefab.Contains("Cliff");
                Transform group = SceneHelper.Group(container, prop.Prefab.Contains("Bridge") ? "Bridges" : shrub ? "Shrubs" : rock ? "Rocks" : "Trees");
                Quaternion rotation = Quaternion.Euler(0f, prop.Yaw, 0f);
                GameObject instance = prop.Height.HasValue
                    ? placer.Place(prefab, group, new Vector3(prop.Position.x, prop.Height.Value, prop.Position.y), rotation, Vector3.one * prop.Scale, 0)
                    : placer.Place(prefab, group, prop.Position, rotation, Vector3.one * prop.Scale, 0.05f, shrub ? DecorationLayer : 0);
                instance.name = PropPrefix + prop.Prefab;
                placed++;
            }

            if (missing > 0)
            {
                Debug.LogWarning($"{info.name}: {missing} props skipped because their prefab was not found.");
            }

            return placed;
        }

        /// <summary>Tells whether a legacy prop floats above or sits under the rebuilt terrain.</summary>
        /// <param name="terrain">The rebuilt terrain.</param>
        /// <param name="prop">The prop to test.</param>
        /// <returns>True when the prop has no ground under it.</returns>
        private static bool IsStranded(Terrain terrain, Transform prop)
        {
            Vector3 position = prop.position;
            float ground = terrain.SampleHeight(position) + terrain.GetPosition().y;
            Renderer[] renderers = prop.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return Mathf.Abs(position.y - ground) > FloatTolerance * 2f;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.min.y - ground > FloatTolerance || bounds.max.y < ground + 0.1f;
        }

        /// <summary>Deterministically removes a share of the legacy decoration so the walkable space grows.</summary>
        /// <param name="info">The map being rebuilt.</param>
        /// <param name="group">The decoration group name.</param>
        /// <param name="point">The prop position on the ground plane.</param>
        /// <returns>True when the prop must be removed.</returns>
        private static bool IsThinned(WorldCellInfo info, string group, Vector2 point)
        {
            if (group == "Boundary" || info.name.Contains("_City"))
            {
                return false;
            }

            float hash = Mathf.Abs(Mathf.Sin(point.x * 12.9898f + point.y * 78.233f) * 43758.5453f) % 1f;
            return hash < OldDecorThinning;
        }

        private static float EdgeDistance(Rect area, Vector2 point)
        {
            return Mathf.Min(Mathf.Min(point.x - area.xMin, area.xMax - point.x), Mathf.Min(point.y - area.yMin, area.yMax - point.y));
        }

        private static Transform FindContainer(Scene scene, WorldCellInfo info)
        {
            string name = $"Map_{info.r}_{info.c}";
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate => candidate.name == name);
            if (root == null)
            {
                root = new GameObject(name);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            return root.transform;
        }

        private static int PlacePortals(Scene scene, Terrain terrain, WorldCellInfo info)
        {
            GameObject[] existing = scene.GetRootGameObjects().Where(root => root.name.StartsWith("WarpPortal_To")).ToArray();
            if (existing.Length == 0)
            {
                return 0;
            }

            GameObject template = existing[0];
            var created = new List<GameObject>();
            foreach (WorldPortalData portal in info.portals.Where(entry => !entry.bothExisting))
            {
                string name = "WarpPortal_To" + portal.dest;
                if (info.existing && scene.GetRootGameObjects().Any(root => root.name == name))
                {
                    continue;
                }

                float ground = terrain.SampleHeight(new Vector3(portal.x, 0f, portal.z)) + terrain.transform.position.y;
                GameObject clone = PortalFactory.Clone(template, name, new Vector3(portal.x, ground + PortalLift, portal.z), portal.dest, string.Empty, portal.dest, ArrivalOffset);
                clone.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(-portal.inz, portal.inx) * Mathf.Rad2Deg, 0f);
                Transform arrival = clone.transform.Find("SpawnPoint_" + portal.dest);
                if (arrival != null)
                {
                    arrival.localPosition = new Vector3(ArrivalOffset, 0f, 0f);
                }

                SceneManager.MoveGameObjectToScene(clone, scene);
                created.Add(clone);
            }

            if (!info.existing)
            {
                foreach (GameObject old in existing)
                {
                    UnityEngine.Object.DestroyImmediate(old);
                }
            }

            return created.Count;
        }

        private void BuildNavBlocks(Scene scene, WorldCellData cell, IReadOnlyList<Vector2> gates)
        {
            SceneHelper.DeleteRoots(scene, NavBlockRoot);
            var root = new GameObject(NavBlockRoot);
            SceneManager.MoveGameObjectToScene(root, scene);
            float floor = cell.Info.lo - 2f;
            float ceiling = cell.Info.lo + cell.Info.size + 6f;
            new BorderNavBlocker(floor, ceiling).Build(root.transform, cell.Info.walk, gates, cell.Info.northBarrier);
            if (cell.Info.existing && cell.Info.name.Contains("_City"))
            {
                return;
            }

            var baked = new BakedMapTerrain(cell, world.Manifest.layers);
            Func<Vector2, float> groundAt = point =>
            {
                float? level = RasterWaterMesh.LevelAt(cell.Water, point);
                float limit = level.HasValue ? Mathf.Max(SeaLevel + 0.25f, level.Value - WetBlockDepth) : SeaLevel + 0.25f;
                return baked.WorldHeight(point) - limit;
            };
            new WaterNavBlocker(groundAt, 0f, ceiling).Build(root.transform);
        }

        private static void PlaceRespawn(Scene scene, Terrain terrain, WorldCellInfo info)
        {
            GameObject respawn = scene.GetRootGameObjects().FirstOrDefault(root => root.name.EndsWith("RespawnPoint"));
            if (respawn == null)
            {
                return;
            }

            float x = info.hub[0];
            float z = info.hub[1];
            float ground = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
            respawn.transform.position = new Vector3(x, ground + 0.5f, z);
        }

        private static bool BakeNavMesh(Terrain terrain, WorldCellInfo info)
        {
            return NavMeshBaker.Bake(terrain.gameObject, $"{DataFolder}/Nav_{info.name}.asset");
        }

        private void Register(WorldCellInfo info, string scenePath)
        {
            Type definitionType = FindType("Project.Maps.MapDefinition");
            Type databaseType = FindType("Project.Maps.MapDatabase");
            string existingPath = AssetDatabase.FindAssets("t:" + definitionType.Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => !path.StartsWith("Assets/_clear/"));
            string folder = existingPath != null ? Path.GetDirectoryName(existingPath).Replace('\\', '/') : "Assets/Data/Maps";
            string assetPath = $"{folder}/{info.name}.asset";
            UnityEngine.Object definition = AssetDatabase.LoadAssetAtPath(assetPath, definitionType);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance(definitionType);
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            var serialized = new SerializedObject(definition);
            serialized.FindProperty("mapId").stringValue = info.name;
            serialized.FindProperty("displayName").stringValue = $"{info.kind} ({info.r},{info.c})";
            serialized.FindProperty("mapPosition").vector2Value = new Vector2(info.c * 60f, -info.r * 60f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            string databaseGuid = AssetDatabase.FindAssets("t:" + databaseType.Name).FirstOrDefault();
            if (databaseGuid != null)
            {
                var database = new SerializedObject(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(databaseGuid), databaseType));
                SerializedProperty list = database.FindProperty("allMaps");
                bool listed = false;
                for (int i = 0; i < list.arraySize; i++)
                {
                    listed |= list.GetArrayElementAtIndex(i).objectReferenceValue == definition;
                }

                if (!listed)
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = definition;
                    database.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.All(entry => entry.path != scenePath))
            {
                EditorBuildSettings.scenes = scenes.Concat(new[] { new EditorBuildSettingsScene(scenePath, true) }).ToArray();
            }
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(fullName)).FirstOrDefault(type => type != null)
                ?? throw new InvalidOperationException("Type not found: " + fullName);
        }
    }
}
