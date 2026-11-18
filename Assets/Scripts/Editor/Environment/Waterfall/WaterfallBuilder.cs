using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds the Map 0,0 waterfall in the active scene: it cuts the mouth into the terrain wall, then creates the rock
    /// collar, the cave, the falling sheet, the foam patch and the particle systems under a single root object.
    /// The operation is idempotent: running it again replaces the previous result.
    /// </summary>
    public static class WaterfallBuilder
    {
        private const string RootName = "Waterfall_0_0";
        private const string RiversPath = "WorldWater/Rivers";
        private const string ProfilePath = "Assets/Terrains/Maps/World/Map_0_0_Peaks_Waterfall.asset";
        private const string MeshFolder = "Assets/Terrains/Maps/World/Waterfall_0_0";
        private const string MaterialFolder = "Assets/Materials/Environment/Waterfall";
        private const string RimLayerName = "rock_cliff";
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string CaveShaderName = "Project/Environment/CaveDark";
        private const string SheetShaderName = "Project/Environment/WaterfallSheet";
        private const string PoolShaderName = "Project/Environment/PoolWater";
        private const string FoamShaderName = "Project/Environment/WaterFoamRing";
        private const string ParticleShaderName = "Project/Environment/WaterParticle";
        private const float FoamRadius = 17f;
        private const float PoolWidthRatio = 0.55f;
        private const float PoolHalfWidth = 16f;
        private const float PoolBackReach = 14f;
        private const float PoolFrontReach = 2f;
        private const float FlowProbeStep = 1.5f;
        private const float FoamLift = 0.06f;
        private const float MinWallClearance = 1f;
        private const int ClearanceSamples = 40;
        private const float ClearanceStartTime = 0.3f;

        /// <summary>
        /// Builds the waterfall of Map 0,0 in the active scene.
        /// </summary>
        [MenuItem("Tools/Environment/Waterfall/Build Map 0,0 Waterfall")]
        public static void BuildMap00()
        {
            try
            {
                Debug.Log(Build());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Builds the waterfall and saves the scene.
        /// </summary>
        /// <returns>A one line report of the result.</returns>
        public static string Build()
        {
            var profile = LoadOrCreateProfile();
            var terrain = Terrain.activeTerrain ?? throw new InvalidOperationException("No terrain in the active scene.");
            var rivers = GameObject.Find(RiversPath) ?? throw new InvalidOperationException($"Object '{RiversPath}' not found.");
            var riverMesh = rivers.GetComponent<MeshFilter>().sharedMesh;

            var grid = WaterSurfaceGrid.FromMesh(riverMesh, rivers.transform);
            if (!grid.TryGetHeight(profile.CenterX, profile.LandingZ, out var landingY))
            {
                throw new InvalidOperationException("The landing point is not over the river water.");
            }

            var wall = new WaterfallWall(terrain);
            var outline = MouthOutline.Create(profile);
            var lipZ = wall.SurfaceZ(profile.CenterX, profile.SillHeight);
            var trajectory = WaterfallTrajectory.Create(profile, lipZ, landingY, Mathf.Abs(Physics.gravity.y));
            WarnAboutProblems(profile, outline, trajectory, wall);

            var cutCells = WaterfallTerrainCutter.Cut(terrain, profile, outline, wall);
            EditorUtility.SetDirty(terrain.terrainData);

            var rimLayer = FindRimLayer(terrain);
            WaterfallMouthMeshBuilder.Build(profile, outline, wall, 1f / rimLayer.tileSize.x, out var rim, out var cave);
            var sheet = WaterfallSheetMeshBuilder.Build(profile, trajectory);
            var foam = WaterfallFoamMeshBuilder.Build(riverMesh, rivers.transform, new Vector2(profile.CenterX, profile.LandingZ), FoamRadius, FoamLift);

            var poolRegion = new Rect(profile.CenterX - PoolHalfWidth, profile.LandingZ - PoolFrontReach, PoolHalfWidth * 2f, PoolBackReach + PoolFrontReach);
            var pool = WaterfallPoolMeshBuilder.Build(terrain, grid, new Vector2(profile.CenterX, profile.LandingZ), landingY, poolRegion);

            var root = RecreateRoot();
            var landing = new Vector3(profile.CenterX, landingY, profile.LandingZ);
            var downstream = DownstreamAt(grid, new Vector2(profile.CenterX, profile.LandingZ));
            var poolHalfWidth = profile.SheetWidthAtBase * PoolWidthRatio;
            CreateMeshObject("Mouth_Rim", root, EditorAssetUtility.SaveMesh(rim, $"{MeshFolder}/Waterfall_MouthRim.asset"), CreateRimMaterial(rimLayer), true);
            CreateMeshObject("Mouth_Cave", root, EditorAssetUtility.SaveMesh(cave, $"{MeshFolder}/Waterfall_MouthCave.asset"), CreateCaveMaterial(), false);
            CreateMeshObject("Sheet", root, EditorAssetUtility.SaveMesh(sheet, $"{MeshFolder}/Waterfall_Sheet.asset"), CreateMaterial("Waterfall_Sheet", SheetShaderName, _ => { }), false);
            CreateMeshObject("Pool", root, EditorAssetUtility.SaveMesh(pool, $"{MeshFolder}/Waterfall_Pool.asset"), CreateMaterial("Waterfall_Pool", PoolShaderName, material => material.SetVector("_Down", new Vector4(downstream.x, downstream.y, 0f, 0f))), false);
            CreateMeshObject("FoamPatch", root, EditorAssetUtility.SaveMesh(foam, $"{MeshFolder}/Waterfall_FoamPatch.asset"), CreateMaterial("Waterfall_Foam", FoamShaderName, material => ConfigureFoam(material, landing, downstream, poolHalfWidth)), false);

            var particleMaterial = CreateMaterial("Waterfall_Particle", ParticleShaderName, _ => { });
            WaterfallParticles.CreateMist(root.transform, landing, poolHalfWidth * 2f, downstream, particleMaterial);
            WaterfallParticles.CreateSplashPuffs(root.transform, landing, poolHalfWidth * 2f, downstream, particleMaterial);
            WaterfallParticles.CreateImpactSpray(root.transform, landing, poolHalfWidth * 2f, particleMaterial);
            WaterfallParticles.CreateFallSpray(root.transform, trajectory, profile.SheetWidthAtLip, particleMaterial);
            WaterfallParticles.CreateMouthHaze(root.transform, trajectory, particleMaterial);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(rivers.scene);
            EditorSceneManager.SaveScene(rivers.scene);

            return $"Waterfall built: drop {trajectory.LipY - landingY:F1} m, outward speed {trajectory.OutwardSpeed:F2} m/s, "
                   + $"fall time {trajectory.FallTime:F2} s, sill width {outline.SillWidth:F1} m, {cutCells} terrain cells cut.";
        }

        private static WaterfallProfile LoadOrCreateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<WaterfallProfile>(ProfilePath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<WaterfallProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            return profile;
        }

        private static void WarnAboutProblems(WaterfallProfile profile, MouthOutline outline, WaterfallTrajectory trajectory, WaterfallWall wall)
        {
            var clearance = float.MaxValue;
            for (var i = 1; i < ClearanceSamples; i++)
            {
                var time = trajectory.FallTime * i / ClearanceSamples;
                if (time < ClearanceStartTime)
                {
                    continue;
                }

                var point = trajectory.PositionAt(time);
                clearance = Mathf.Min(clearance, wall.SurfaceZ(point.x, point.y) - point.z);
            }

            if (clearance < MinWallClearance)
            {
                Debug.LogWarning($"The water sheet passes {clearance:F2} m from the wall. Move the landing Z farther from the wall.");
            }

            if (outline.SillWidth < profile.SheetWidthAtLip + 0.5f)
            {
                Debug.LogWarning($"The flat sill is {outline.SillWidth:F1} m wide, narrower than the water sheet. Widen the mouth or narrow the sheet.");
            }
        }

        private static GameObject RecreateRoot()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            return new GameObject(RootName);
        }

        private static GameObject CreateMeshObject(string name, GameObject root, Mesh mesh, Material material, bool castsShadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castsShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = castsShadows;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            return go;
        }

        private static TerrainLayer FindRimLayer(Terrain terrain)
        {
            foreach (var layer in terrain.terrainData.terrainLayers)
            {
                if (layer != null && layer.name == RimLayerName)
                {
                    return layer;
                }
            }

            throw new InvalidOperationException($"Terrain layer '{RimLayerName}' not found.");
        }

        private static Material CreateRimMaterial(TerrainLayer layer)
        {
            return CreateMaterial("Waterfall_Rim", LitShaderName, material =>
            {
                material.SetTexture("_BaseMap", layer.diffuseTexture);
                material.SetTexture("_BumpMap", layer.normalMapTexture);
                material.SetFloat("_BumpScale", layer.normalScale);
                material.SetFloat("_Smoothness", layer.smoothness);
                material.SetFloat("_Metallic", layer.metallic);
                material.EnableKeyword("_NORMALMAP");
            });
        }

        private static Vector2 DownstreamAt(WaterSurfaceGrid grid, Vector2 point)
        {
            if (!grid.TryGetHeight(point.x + FlowProbeStep, point.y, out var east)
                || !grid.TryGetHeight(point.x - FlowProbeStep, point.y, out var west)
                || !grid.TryGetHeight(point.x, point.y + FlowProbeStep, out var north)
                || !grid.TryGetHeight(point.x, point.y - FlowProbeStep, out var south))
            {
                return Vector2.down;
            }

            var gradient = new Vector2(east - west, north - south);
            return gradient.sqrMagnitude < 1e-6f ? Vector2.down : -gradient.normalized;
        }

        private static void ConfigureFoam(Material material, Vector3 landing, Vector2 downstream, float halfWidth)
        {
            material.SetVector("_Center", new Vector4(landing.x, landing.z, 0f, 0f));
            material.SetVector("_Down", new Vector4(downstream.x, downstream.y, 0f, 0f));
            material.SetFloat("_CoreWidth", halfWidth);
        }

        private static Material CreateCaveMaterial()
        {
            return CreateMaterial("Waterfall_Cave", CaveShaderName, _ => { });
        }

        private static Material CreateMaterial(string name, string shaderName, Action<Material> configure)
        {
            return EditorAssetUtility.EnsureMaterial($"{MaterialFolder}/{name}.mat", shaderName, configure);
        }
    }
}
