using System.Collections.Generic;
using System.IO;
using Project.Ambience;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.EditorTools
{
    /// <summary>
    /// Finds the chimneys of the City4 building prefabs and gives each one a smoke emitter, plus a
    /// <see cref="ChimneySmoke"/> on the building root to switch the smoke level. A chimney is recognised by its
    /// snow cap, the 0.95 x 0.16 x 0.95 m block of the light stone submesh that the city generator puts on top of
    /// every chimney. The operation is idempotent: prefabs that already have a <see cref="ChimneySmoke"/> are skipped.
    /// Run it again after regenerating the buildings, because regenerating rewrites the prefabs.
    /// </summary>
    public static class ChimneySmokeBaker
    {
        private const string SearchFolder = "Assets/Models/Cities/City4";
        private const string EffectsParent = "Assets/Models/Shared";
        private const string EffectsFolderName = "Effects";
        private const string EffectsFolder = EffectsParent + "/" + EffectsFolderName;
        private const string TexturePath = EffectsFolder + "/T_SmokePuff.png";
        private const string MaterialPath = EffectsFolder + "/M_ChimneySmoke.mat";
        private const string ProfilePath = EffectsFolder + "/ChimneySmokeProfile.asset";
        private const string EmitterPrefix = "Smoke_";
        private const string CapMaterialKeyword = "stone_light";
        private const float CapTolerance = 0.04f;
        private const float WeldPrecision = 500f;
        private const float DuplicateDistance = 0.1f;
        private const int TextureSize = 128;

        private static readonly Vector3 CapSize = new Vector3(0.95f, 0.16f, 0.95f);

        /// <summary>
        /// Adds smoke emitters to every City4 building prefab that has chimneys.
        /// </summary>
        [MenuItem("Tools/Environment/Bake Chimney Smoke")]
        public static void BakeCity4()
        {
            var count = BakeFolders(SearchFolder);
            Debug.Log($"Chimney smoke bake finished: {count} emitters created.");
        }

        /// <summary>
        /// Adds smoke emitters to every prefab found in the given folders.
        /// </summary>
        /// <param name="folders">Asset folders to search for prefabs.</param>
        /// <returns>The number of emitters created.</returns>
        public static int BakeFolders(params string[] folders)
        {
            EnsureEffectsFolder();
            var profile = LoadOrCreateProfile();
            var material = LoadOrCreateMaterial();

            var created = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                created += BakePrefab(AssetDatabase.GUIDToAssetPath(guid), profile, material);
            }

            AssetDatabase.SaveAssets();
            return created;
        }

        /// <summary>
        /// Adds the emitters and the controller to one prefab.
        /// </summary>
        private static int BakePrefab(string path, ChimneySmokeProfile profile, Material material)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset.GetComponent<ChimneySmoke>() != null)
            {
                return 0;
            }

            var tops = FindChimneyTops(asset);
            if (tops.Count == 0)
            {
                return 0;
            }

            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var emitters = new ParticleSystem[tops.Count];
                for (var i = 0; i < tops.Count; i++)
                {
                    emitters[i] = CreateEmitter(contents.transform, i, tops[i], material);
                }

                var smoke = contents.AddComponent<ChimneySmoke>();
                AssignReferences(smoke, profile, emitters);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            return tops.Count;
        }

        /// <summary>
        /// Returns the top centre of every chimney cap of a prefab, in the local space of its root.
        /// </summary>
        private static List<Vector3> FindChimneyTops(GameObject asset)
        {
            var tops = new List<Vector3>();
            foreach (var renderer in asset.GetComponentsInChildren<MeshRenderer>(true))
            {
                var filter = renderer.GetComponent<MeshFilter>();
                var mesh = filter != null ? filter.sharedMesh : null;
                if (mesh == null || !mesh.isReadable)
                {
                    continue;
                }

                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length && i < mesh.subMeshCount; i++)
                {
                    if (materials[i] == null || !materials[i].name.ToLowerInvariant().Contains(CapMaterialKeyword))
                    {
                        continue;
                    }

                    foreach (var bounds in ClusterBounds(mesh, i))
                    {
                        if (!IsCap(bounds))
                        {
                            continue;
                        }

                        var world = renderer.transform.TransformPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
                        AddUnique(tops, asset.transform.InverseTransformPoint(world));
                    }
                }
            }

            return tops;
        }

        /// <summary>
        /// Adds a point unless another one is already within the duplicate distance.
        /// </summary>
        private static void AddUnique(List<Vector3> points, Vector3 point)
        {
            foreach (var existing in points)
            {
                if (Vector3.Distance(existing, point) < DuplicateDistance)
                {
                    return;
                }
            }

            points.Add(point);
        }

        /// <summary>
        /// True when the bounds match the size of a chimney snow cap.
        /// </summary>
        private static bool IsCap(Bounds bounds)
        {
            return Mathf.Abs(bounds.size.x - CapSize.x) < CapTolerance
                && Mathf.Abs(bounds.size.y - CapSize.y) < CapTolerance
                && Mathf.Abs(bounds.size.z - CapSize.z) < CapTolerance;
        }

        /// <summary>
        /// Splits a submesh into connected pieces (vertices at the same spot are welded) and returns their bounds.
        /// </summary>
        private static List<Bounds> ClusterBounds(Mesh mesh, int subMesh)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.GetTriangles(subMesh);
            var triangleCount = triangles.Length / 3;

            var parent = new int[triangleCount];
            for (var t = 0; t < triangleCount; t++)
            {
                parent[t] = t;
            }

            var firstTriangleAt = new Dictionary<Vector3Int, int>();
            for (var t = 0; t < triangleCount; t++)
            {
                for (var corner = 0; corner < 3; corner++)
                {
                    var key = WeldKey(vertices[triangles[t * 3 + corner]]);
                    if (firstTriangleAt.TryGetValue(key, out var other))
                    {
                        parent[FindRoot(parent, t)] = FindRoot(parent, other);
                    }
                    else
                    {
                        firstTriangleAt[key] = t;
                    }
                }
            }

            var boundsByRoot = new Dictionary<int, Bounds>();
            for (var t = 0; t < triangleCount; t++)
            {
                var root = FindRoot(parent, t);
                for (var corner = 0; corner < 3; corner++)
                {
                    var vertex = vertices[triangles[t * 3 + corner]];
                    if (boundsByRoot.TryGetValue(root, out var bounds))
                    {
                        bounds.Encapsulate(vertex);
                        boundsByRoot[root] = bounds;
                    }
                    else
                    {
                        boundsByRoot[root] = new Bounds(vertex, Vector3.zero);
                    }
                }
            }

            return new List<Bounds>(boundsByRoot.Values);
        }

        /// <summary>
        /// Quantises a vertex position so vertices at the same spot share a key.
        /// </summary>
        private static Vector3Int WeldKey(Vector3 position)
        {
            return new Vector3Int(
                Mathf.RoundToInt(position.x * WeldPrecision),
                Mathf.RoundToInt(position.y * WeldPrecision),
                Mathf.RoundToInt(position.z * WeldPrecision));
        }

        /// <summary>
        /// Union-find root lookup with path compression.
        /// </summary>
        private static int FindRoot(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }

            return index;
        }

        /// <summary>
        /// Creates a smoke emitter above a chimney. The emitter points up and simulates in world space,
        /// the level values are applied at runtime by <see cref="ChimneySmoke"/>.
        /// </summary>
        private static ParticleSystem CreateEmitter(Transform root, int index, Vector3 top, Material material)
        {
            var emitterObject = new GameObject($"{EmitterPrefix}{index + 1:00}");
            emitterObject.transform.SetParent(root, false);
            emitterObject.transform.localPosition = top;
            emitterObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var system = emitterObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.prewarm = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;
            main.gravityModifier = -0.02f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 6f;
            shape.radius = 0.12f;

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.7f));

            var colour = system.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.12f),
                    new GradientAlphaKey(0.55f, 0.7f),
                    new GradientAlphaKey(0f, 1f),
                });
            colour.color = gradient;

            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-20f * Mathf.Deg2Rad, 20f * Mathf.Deg2Rad);

            var noise = system.noise;
            noise.enabled = true;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.2f;
            noise.damping = true;
            noise.quality = ParticleSystemNoiseQuality.Low;

            var particleRenderer = emitterObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.sharedMaterial = material;
            particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
            particleRenderer.receiveShadows = false;

            return system;
        }

        /// <summary>
        /// Stores the profile and the emitters in the private fields of the controller.
        /// </summary>
        private static void AssignReferences(ChimneySmoke smoke, ChimneySmokeProfile profile, ParticleSystem[] emitters)
        {
            var serialized = new SerializedObject(smoke);
            serialized.FindProperty("profile").objectReferenceValue = profile;
            var list = serialized.FindProperty("emitters");
            list.arraySize = emitters.Length;
            for (var i = 0; i < emitters.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = emitters[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Creates the effects folder when it does not exist.
        /// </summary>
        private static void EnsureEffectsFolder()
        {
            if (!AssetDatabase.IsValidFolder(EffectsFolder))
            {
                AssetDatabase.CreateFolder(EffectsParent, EffectsFolderName);
            }
        }

        /// <summary>
        /// Loads the shared smoke profile, creating it with the default values on first use.
        /// </summary>
        private static ChimneySmokeProfile LoadOrCreateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<ChimneySmokeProfile>(ProfilePath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<ChimneySmokeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            return profile;
        }

        /// <summary>
        /// Loads the transparent particle material, creating it on first use.
        /// </summary>
        private static Material LoadOrCreateMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetTexture("_BaseMap", LoadOrCreateTexture());
            material.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        /// <summary>
        /// Loads the soft puff texture, drawing it on first use: a round falloff broken up by noise.
        /// </summary>
        private static Texture2D LoadOrCreateTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            return existing != null ? existing : RebuildTexture();
        }

        /// <summary>
        /// Draws the puff texture and writes it over the existing file: a soft round blob with a mild noise variation.
        /// </summary>
        /// <returns>The imported texture.</returns>
        public static Texture2D RebuildTexture()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[TextureSize * TextureSize];
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    var u = (x + 0.5f) / TextureSize * 2f - 1f;
                    var v = (y + 0.5f) / TextureSize * 2f - 1f;
                    var falloff = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - Mathf.Sqrt(u * u + v * v)) * 1.6f));
                    var noise = Mathf.PerlinNoise(x * 0.05f + 7f, y * 0.05f + 7f) * 0.6f
                        + Mathf.PerlinNoise(x * 0.12f + 3f, y * 0.12f + 3f) * 0.4f;
                    var alpha = Mathf.Clamp01(falloff * (0.7f + 0.6f * noise));
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), TexturePath), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(TexturePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        }
    }
}
