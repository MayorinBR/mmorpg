using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Replaces the contents of Prototype_Cave01 with the dungeon layout while keeping its gameplay objects and enemies.</summary>
    public static class CaveDungeonSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Prototypes/Prototype_Cave01.unity";
        private const string RootName = "Generated_Dungeon";
        private const string LegacyRootName = "Generated_Cave";
        private const string SpawnAreaName = "EnemySpawn";
        private const string BossAreaName = "EnemySpawn_Boss";
        private const string RespawnName = "Map04RespawnPoint";
        private const string NavMeshAssetPath = "Assets/Scenes/Prototypes/Prototype_Cave01/NavMesh-Terrain.asset";
        private const string VoidMaterialPath = DungeonKitPaths.Painted + "/DungeonVoid.mat";
        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";
        private const string BaseColorProperty = "_BaseColor";
        private const int Seed = 1707;
        private const int DecorationLayer = 2;
        private const float SnapDistance = 1.5f;
        private const float VoidDrop = 1f;
        private const float VoidMargin = 60f;

        /// <summary>Rebuilds Prototype_Cave01 as a dungeon from the menu.</summary>
        [MenuItem("Tools/Dungeon/Rebuild Prototype Cave01")]
        public static void RebuildFromMenu()
        {
            if (!OverwriteGuard.Confirm(ScenePath))
            {
                return;
            }

            Debug.Log(Rebuild());
        }

        /// <summary>Rebuilds Prototype_Cave01 as a dungeon and saves the scene.</summary>
        /// <returns>A report of what was built and whether every spawn point can be reached from the respawn point.</returns>
        public static string Rebuild()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var terrain = Object.FindFirstObjectByType<Terrain>();
            DungeonLayout layout = CaveDungeonLayout.Create();
            SceneObjects.DestroyAllNamed(LegacyRootName);
            SceneObjects.DestroyAllNamed(RootName);

            Transform root = new GameObject(RootName).transform;
            var library = new DungeonPrefabLibrary();
            var shell = new DungeonShellBuilder(layout, library, Seed);
            shell.BuildFloors(Child(root, "Floors"), terrain.gameObject.layer);
            shell.BuildWalls(Child(root, "Walls"));
            shell.BuildPosts(Child(root, "Posts"));
            int props = new DungeonDresser(layout, library).Dress(Child(root, "Props"));
            BuildVoid(root, layout);

            DisableTerrain(terrain);
            PlaceSpawnPoints(layout);
            ApplyAtmosphere();
            bool baked = NavMeshBaker.Bake(terrain.gameObject, NavMeshAssetPath);
            string report = Verify(layout, props, baked);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return report;
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void BuildVoid(Transform root, DungeonLayout layout)
        {
            float width = (layout.Grid.Columns * DungeonGrid.CellSize) + VoidMargin;
            float depth = (layout.Grid.Rows * DungeonGrid.CellSize) + VoidMargin;
            Vector2 middle = layout.Grid.CellCenter(new Vector2Int(layout.Grid.Columns / 2, layout.Grid.Rows / 2));
            Material material = VoidMaterial();
            GameObject voidPlane = GreyboxKit.Create(PrimitiveType.Cube, "Void", root, new Vector3(middle.x, -VoidDrop, middle.y), new Vector3(width, 1f, depth), material);
            Object.DestroyImmediate(voidPlane.GetComponent<Collider>());
            voidPlane.layer = DecorationLayer;
        }

        private static Material VoidMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(VoidMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find(UnlitShaderName));
                material.SetColor(BaseColorProperty, new Color(0.01f, 0.01f, 0.014f));
                AssetDatabase.CreateAsset(material, VoidMaterialPath);
            }

            return material;
        }

        private static void DisableTerrain(Terrain terrain)
        {
            terrain.enabled = false;
            if (terrain.TryGetComponent(out TerrainCollider collider))
            {
                collider.enabled = false;
            }
        }

        private static void PlaceSpawnPoints(DungeonLayout layout)
        {
            GameObject area = GameObject.Find(SpawnAreaName);
            ClearChildren(area);
            for (int index = 0; index < layout.Spawns.Count; index++)
            {
                SpawnSpec spawn = layout.Spawns[index];
                AddPoint(area.transform, $"Point_{index:00}", layout, spawn);
            }

            GameObject boss = GameObject.Find(BossAreaName);
            ClearChildren(boss);
            AddPoint(boss.transform, "Point_00", layout, layout.BossSpawn);
        }

        private static void AddPoint(Transform parent, string name, DungeonLayout layout, SpawnSpec spawn)
        {
            Vector2 position = layout.ToWorld(spawn.Room, spawn.U, spawn.V);
            var point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.position = new Vector3(position.x, 0f, position.y);
        }

        private static void ClearChildren(GameObject area)
        {
            for (int index = area.transform.childCount - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(area.transform.GetChild(index).gameObject);
            }
        }

        private static void ApplyAtmosphere()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.33f, 0.33f, 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.01f;
            RenderSettings.fogColor = new Color(0.04f, 0.04f, 0.06f);

            GameObject sun = GameObject.Find("Directional Light");
            if (sun != null && sun.TryGetComponent(out Light light))
            {
                light.intensity = 0.4f;
                light.color = new Color(0.65f, 0.7f, 0.95f);
            }
        }

        private static string Verify(DungeonLayout layout, int props, bool baked)
        {
            var report = new StringBuilder();
            report.AppendLine($"Dungeon built: {props} props, navmesh baked: {baked}");
            GameObject respawn = GameObject.Find(RespawnName);
            if (respawn == null || !NavMesh.SamplePosition(respawn.transform.position, out NavMeshHit start, SnapDistance, NavMesh.AllAreas))
            {
                report.AppendLine("Respawn point is not on the NavMesh.");
                return report.ToString();
            }

            foreach (string areaName in new[] { SpawnAreaName, BossAreaName })
            {
                foreach (Transform point in GameObject.Find(areaName).transform)
                {
                    var path = new NavMeshPath();
                    bool onMesh = NavMesh.SamplePosition(point.position, out NavMeshHit end, SnapDistance, NavMesh.AllAreas);
                    bool reachable = onMesh && NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
                    report.AppendLine($"{areaName}/{point.name} {point.position}: onMesh={onMesh} reachable={reachable}");
                }
            }

            return report.ToString();
        }
    }
}
