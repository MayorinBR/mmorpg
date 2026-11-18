using System.Collections.Generic;
using Project.Ambience;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.EditorTools
{
    /// <summary>
    /// Splits the glass submesh of merged building meshes into one pane object per window and wires each pane to a
    /// <see cref="WindowLight"/>, so every window can later be lit by the day and night cycle. The operation is
    /// idempotent: prefabs whose glass has already been split are skipped.
    /// </summary>
    public static class WindowLightBaker
    {
        private const string OffMaterialPath = "Assets/Models/Shared/Materials/M_Window_Off.mat";
        private const string LitMaterialPath = "Assets/Models/Shared/Materials/M_Window_Lit.mat";
        private const string WindowsContainerName = "Windows";
        private const string WindowNamePrefix = "Window_";
        private const float WeldPrecision = 500f;
        private const float PaneMergeDistance = 0.12f;

        private static readonly string[] DefaultFolders =
        {
            "Assets/Models/Cities/City1/buildings",
            "Assets/Models/Cities/City1/shops",
            "Assets/Models/Cities/City2/Buildings",
            "Assets/Models/Cities/City2/Houses",
            "Assets/Models/Cities/City4/Houses",
            "Assets/Models/Cities/City4/Guild",
        };

        /// <summary>
        /// Splits the windows of every building prefab in the default city folders.
        /// </summary>
        [MenuItem("Tools/Environment/Bake Window Lights")]
        public static void BakeDefaultFolders()
        {
            var count = BakeFolders(DefaultFolders);
            Debug.Log($"Window light bake finished: {count} windows created.");
        }

        /// <summary>
        /// Splits the windows of every prefab found in the given folders.
        /// </summary>
        /// <param name="folders">Asset folders to search for prefabs.</param>
        /// <returns>The number of window panes created.</returns>
        public static int BakeFolders(params string[] folders)
        {
            var offMaterial = LoadOrCreateOffMaterial();
            var litMaterial = LoadOrCreateLitMaterial();

            var created = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                created += BakePrefab(AssetDatabase.GUIDToAssetPath(guid), offMaterial, litMaterial);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return created;
        }

        /// <summary>
        /// Adds a <see cref="LightPoint"/> to every prefab in the folders that owns real lights but no window group.
        /// </summary>
        /// <param name="folders">Asset folders to search for prefabs.</param>
        /// <returns>The number of prefabs that received a light point.</returns>
        public static int AddLightPoints(params string[] folders)
        {
            var changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", folders))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var hasLights = asset.GetComponentInChildren<Light>(true) != null;
                var alreadyControlled = asset.GetComponent<LightPoint>() != null
                    || asset.GetComponentInChildren<WindowLight>(true) != null;
                if (!hasLights || alreadyControlled)
                {
                    continue;
                }

                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    contents.AddComponent<LightPoint>();
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    changed++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            AssetDatabase.SaveAssets();
            return changed;
        }

        private static int BakePrefab(string path, Material offMaterial, Material litMaterial)
        {
            var jobs = CollectJobs(path);
            if (jobs.Count == 0)
            {
                return 0;
            }

            foreach (var job in jobs)
            {
                foreach (var pane in job.Panes)
                {
                    AssetDatabase.AddObjectToAsset(pane.Mesh, path);
                }
            }

            AssetDatabase.SaveAssets();

            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var job in jobs)
                {
                    BuildPaneObjects(contents, job, offMaterial, litMaterial);
                }

                AssignGroup(contents);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var job in jobs)
            {
                var node = FindNode(saved.transform, job.NodePath);
                RemoveSubmesh(node.GetComponent<MeshFilter>().sharedMesh, job.GlassIndex);
            }

            AssetDatabase.SaveAssets();

            var total = 0;
            foreach (var job in jobs)
            {
                total += job.Panes.Count;
            }

            return total;
        }

        private static List<BakeJob> CollectJobs(string path)
        {
            var jobs = new List<BakeJob>();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            foreach (var renderer in asset.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.GetComponentInParent<WindowLight>(true) != null)
                {
                    continue;
                }

                var filter = renderer.GetComponent<MeshFilter>();
                var mesh = filter != null ? filter.sharedMesh : null;
                var glassIndex = FindGlassIndex(renderer.sharedMaterials);
                if (mesh == null || !mesh.isReadable || glassIndex < 0 || glassIndex >= mesh.subMeshCount)
                {
                    continue;
                }

                var panes = SplitPanes(mesh, glassIndex);
                if (panes.Count == 0)
                {
                    continue;
                }

                jobs.Add(new BakeJob
                {
                    NodePath = AnimationUtility.CalculateTransformPath(renderer.transform, asset.transform),
                    GlassIndex = glassIndex,
                    Panes = panes,
                });
            }

            return jobs;
        }

        private static int FindGlassIndex(Material[] materials)
        {
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].name.ToLowerInvariant().Contains("glass"))
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<Pane> SplitPanes(Mesh mesh, int subMesh)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            var triangles = mesh.GetTriangles(subMesh);
            var triangleCount = triangles.Length / 3;

            var groups = GroupConnectedTriangles(vertices, triangles, triangleCount);
            var bounds = new List<Bounds>();
            foreach (var group in groups)
            {
                bounds.Add(ComputeBounds(vertices, triangles, group));
            }

            MergeNearbyGroups(groups, bounds);

            var panes = new List<Pane>();
            for (var i = 0; i < groups.Count; i++)
            {
                var center = bounds[i].center;
                var paneMesh = BuildPaneMesh(vertices, normals, uvs, triangles, groups[i], center);
                paneMesh.name = $"{mesh.name}_{WindowNamePrefix}{i:00}";
                panes.Add(new Pane { Mesh = paneMesh, Center = center });
            }

            return panes;
        }

        private static List<List<int>> GroupConnectedTriangles(Vector3[] vertices, int[] triangles, int triangleCount)
        {
            var parent = new int[triangleCount];
            for (var i = 0; i < triangleCount; i++)
            {
                parent[i] = i;
            }

            var firstTriangleAt = new Dictionary<Vector3Int, int>();
            for (var t = 0; t < triangleCount; t++)
            {
                for (var corner = 0; corner < 3; corner++)
                {
                    var key = Quantize(vertices[triangles[t * 3 + corner]]);
                    if (firstTriangleAt.TryGetValue(key, out var other))
                    {
                        parent[Root(parent, t)] = Root(parent, other);
                    }
                    else
                    {
                        firstTriangleAt[key] = t;
                    }
                }
            }

            var byRoot = new Dictionary<int, List<int>>();
            var ordered = new List<List<int>>();
            for (var t = 0; t < triangleCount; t++)
            {
                var root = Root(parent, t);
                if (!byRoot.TryGetValue(root, out var list))
                {
                    list = new List<int>();
                    byRoot[root] = list;
                    ordered.Add(list);
                }

                list.Add(t);
            }

            return ordered;
        }

        private static void MergeNearbyGroups(List<List<int>> groups, List<Bounds> bounds)
        {
            var merged = true;
            while (merged)
            {
                merged = false;
                for (var a = 0; a < groups.Count && !merged; a++)
                {
                    for (var b = a + 1; b < groups.Count && !merged; b++)
                    {
                        var expanded = bounds[a];
                        expanded.Expand(PaneMergeDistance * 2f);
                        if (!expanded.Intersects(bounds[b]))
                        {
                            continue;
                        }

                        groups[a].AddRange(groups[b]);
                        var union = bounds[a];
                        union.Encapsulate(bounds[b]);
                        bounds[a] = union;
                        groups.RemoveAt(b);
                        bounds.RemoveAt(b);
                        merged = true;
                    }
                }
            }
        }

        private static Bounds ComputeBounds(Vector3[] vertices, int[] triangles, List<int> group)
        {
            var bounds = new Bounds(vertices[triangles[group[0] * 3]], Vector3.zero);
            foreach (var t in group)
            {
                for (var corner = 0; corner < 3; corner++)
                {
                    bounds.Encapsulate(vertices[triangles[t * 3 + corner]]);
                }
            }

            return bounds;
        }

        private static Mesh BuildPaneMesh(
            Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles, List<int> group, Vector3 center)
        {
            var remap = new Dictionary<int, int>();
            var newVertices = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newUvs = new List<Vector2>();
            var newTriangles = new List<int>();

            foreach (var t in group)
            {
                for (var corner = 0; corner < 3; corner++)
                {
                    var source = triangles[t * 3 + corner];
                    if (!remap.TryGetValue(source, out var index))
                    {
                        index = newVertices.Count;
                        remap[source] = index;
                        newVertices.Add(vertices[source] - center);
                        if (normals.Length > source)
                        {
                            newNormals.Add(normals[source]);
                        }

                        if (uvs.Length > source)
                        {
                            newUvs.Add(uvs[source]);
                        }
                    }

                    newTriangles.Add(index);
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(newVertices);
            mesh.SetTriangles(newTriangles, 0);
            if (newNormals.Count == newVertices.Count)
            {
                mesh.SetNormals(newNormals);
            }
            else
            {
                mesh.RecalculateNormals();
            }

            if (newUvs.Count == newVertices.Count)
            {
                mesh.SetUVs(0, newUvs);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildPaneObjects(GameObject root, BakeJob job, Material offMaterial, Material litMaterial)
        {
            var node = FindNode(root.transform, job.NodePath);
            var renderer = node.GetComponent<MeshRenderer>();

            var materials = new List<Material>(renderer.sharedMaterials);
            materials.RemoveAt(job.GlassIndex);
            renderer.sharedMaterials = materials.ToArray();

            var container = new GameObject(WindowsContainerName) { layer = node.gameObject.layer };
            container.transform.SetParent(node, false);

            for (var i = 0; i < job.Panes.Count; i++)
            {
                var window = new GameObject($"{WindowNamePrefix}{i:00}") { layer = node.gameObject.layer };
                window.transform.SetParent(container.transform, false);
                window.transform.localPosition = job.Panes[i].Center;

                window.AddComponent<MeshFilter>().sharedMesh = job.Panes[i].Mesh;
                var paneRenderer = window.AddComponent<MeshRenderer>();
                paneRenderer.sharedMaterial = offMaterial;
                paneRenderer.shadowCastingMode = ShadowCastingMode.Off;

                var light = window.AddComponent<WindowLight>();
                var serialized = new SerializedObject(light);
                serialized.FindProperty("pane").objectReferenceValue = paneRenderer;
                serialized.FindProperty("offMaterial").objectReferenceValue = offMaterial;
                serialized.FindProperty("litMaterial").objectReferenceValue = litMaterial;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void AssignGroup(GameObject root)
        {
            var group = root.GetComponent<BuildingWindowGroup>();
            if (group == null)
            {
                group = root.AddComponent<BuildingWindowGroup>();
            }

            var windows = root.GetComponentsInChildren<WindowLight>(true);
            var serialized = new SerializedObject(group);
            var property = serialized.FindProperty("windows");
            property.arraySize = windows.Length;
            for (var i = 0; i < windows.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = windows[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindNode(Transform root, string relativePath)
        {
            return string.IsNullOrEmpty(relativePath) ? root : root.Find(relativePath);
        }

        private static void RemoveSubmesh(Mesh mesh, int index)
        {
            var kept = new List<int[]>();
            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                if (i != index)
                {
                    kept.Add(mesh.GetTriangles(i));
                }
            }

            mesh.subMeshCount = kept.Count;
            for (var i = 0; i < kept.Count; i++)
            {
                mesh.SetTriangles(kept[i], i);
            }

            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
        }

        private static Material LoadOrCreateOffMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(OffMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_Window_Off" };
            material.SetColor("_BaseColor", new Color(0.13f, 0.19f, 0.24f, 1f));
            material.SetFloat("_Smoothness", 0.85f);
            material.SetFloat("_Metallic", 0.1f);
            AssetDatabase.CreateAsset(material, OffMaterialPath);
            return material;
        }

        private static Material LoadOrCreateLitMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(LitMaterialPath);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_Window_Lit" };
            material.SetColor("_BaseColor", new Color(0.25f, 0.2f, 0.12f, 1f));
            material.SetFloat("_Smoothness", 0.4f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.35f) * 2.2f);
            AssetDatabase.CreateAsset(material, LitMaterialPath);
            return material;
        }

        private static Vector3Int Quantize(Vector3 v)
        {
            return new Vector3Int(
                Mathf.RoundToInt(v.x * WeldPrecision),
                Mathf.RoundToInt(v.y * WeldPrecision),
                Mathf.RoundToInt(v.z * WeldPrecision));
        }

        private static int Root(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]];
                index = parent[index];
            }

            return index;
        }

        private sealed class Pane
        {
            public Mesh Mesh;
            public Vector3 Center;
        }

        private sealed class BakeJob
        {
            public string NodePath;
            public int GlassIndex;
            public List<Pane> Panes;
        }
    }
}
