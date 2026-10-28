using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Project.World;

namespace Project.EditorTools
{
    /// <summary>
    /// Renders a top-down picture of map scenes and assigns it as the map's World Map thumbnail. A scene with a
    /// <see cref="MapThumbnailArea"/> is shot over that area; any other scene is shot over everything in it.
    /// </summary>
    public static class MapThumbnailCapture
    {
        private const string SceneFolder = "Assets/Scenes/Maps";
        private const string OutputFolder = "Assets/Art/MapThumbnails";
        private const int Resolution = 512;
        private const int UiLayer = 5;
        private const float CameraClearance = 20f;

        private static readonly Color BackgroundColor = new Color(0.1f, 0.12f, 0.16f, 1f);

        /// <summary>
        /// Opens each map scene in turn, captures it from above and stores the picture on its map definition,
        /// then reopens the scene that was open before.
        /// </summary>
        [MenuItem("Tools/Maps/Capture All Thumbnails")]
        public static void CaptureAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var previousScene = EditorSceneManager.GetActiveScene().path;
            Directory.CreateDirectory(OutputFolder);

            foreach (var guid in AssetDatabase.FindAssets("t:MapDefinition"))
            {
                var definition = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
                var mapId = new SerializedObject(definition).FindProperty("mapId").stringValue;
                var scenePath = $"{SceneFolder}/{mapId}.unity";

                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning($"MapThumbnailCapture: no scene at {scenePath}.");
                    continue;
                }

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                CaptureOpenScene();
            }

            if (!string.IsNullOrEmpty(previousScene))
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        /// <summary>
        /// Captures the open scene and stores the picture on the map definition whose id matches the scene name.
        /// </summary>
        [MenuItem("Tools/Maps/Capture Open Scene Thumbnail")]
        public static void CaptureOpenScene()
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            var definition = FindDefinition(sceneName);

            if (definition == null)
            {
                Debug.LogWarning($"MapThumbnailCapture: no map definition with id {sceneName}.");
                return;
            }

            if (!TryCapture(out var png, out var center, out var sideLength))
            {
                Debug.LogWarning($"MapThumbnailCapture: nothing to capture in {sceneName}.");
                return;
            }

            Directory.CreateDirectory(OutputFolder);
            Assign(definition, sceneName, png, center, sideLength);
            AssetDatabase.SaveAssets();
        }

        private static ScriptableObject FindDefinition(string mapId)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MapDefinition"))
            {
                var definition = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));

                if (new SerializedObject(definition).FindProperty("mapId").stringValue == mapId)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Renders the open scene from above and reports the world area the picture covers.
        /// </summary>
        /// <param name="png">The encoded picture.</param>
        /// <param name="center">World X and Z of the center of the captured area.</param>
        /// <param name="sideLength">Side length in meters of the captured square.</param>
        /// <returns>False when the scene has nothing to capture.</returns>
        private static bool TryCapture(out byte[] png, out Vector2 center, out float sideLength)
        {
            var area = Object.FindFirstObjectByType<MapThumbnailArea>();

            if (area != null)
            {
                var position = area.CameraPosition;
                center = new Vector2(position.x, position.z);
                sideLength = area.SideLength;
                png = Render(position, sideLength * 0.5f, area.CaptureDepth);
                return true;
            }

            if (!TryGetSceneBounds(out var bounds))
            {
                png = null;
                center = default;
                sideLength = 0f;
                return false;
            }

            var halfSide = Mathf.Max(bounds.extents.x, bounds.extents.z);
            center = new Vector2(bounds.center.x, bounds.center.z);
            sideLength = halfSide * 2f;
            png = Render(new Vector3(bounds.center.x, bounds.max.y + CameraClearance, bounds.center.z), halfSide, bounds.size.y + CameraClearance * 2f);
            return true;
        }

        private static byte[] Render(Vector3 cameraPosition, float halfSide, float depth)
        {
            var cameraObject = new GameObject("ThumbnailCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = halfSide;
            camera.transform.position = cameraPosition;
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = depth;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.cullingMask = ~(1 << UiLayer);

            var target = RenderTexture.GetTemporary(Resolution, Resolution, 24);
            camera.targetTexture = target;
            camera.Render();

            var previousActive = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(Resolution, Resolution, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, Resolution, Resolution), 0, 0);
            texture.Apply();
            RenderTexture.active = previousActive;

            var png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(cameraObject);
            return png;
        }

        private static bool TryGetSceneBounds(out Bounds bounds)
        {
            bounds = default;
            var found = false;

            foreach (var meshRenderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (meshRenderer.gameObject.layer != UiLayer)
                {
                    Encapsulate(ref bounds, ref found, meshRenderer.bounds);
                }
            }

            foreach (var terrain in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                var data = terrain.terrainData;
                Encapsulate(ref bounds, ref found, new Bounds(terrain.transform.position + data.size * 0.5f, data.size));
            }

            return found;
        }

        private static void Encapsulate(ref Bounds bounds, ref bool found, Bounds addition)
        {
            if (found)
            {
                bounds.Encapsulate(addition);
                return;
            }

            bounds = addition;
            found = true;
        }

        private static void Assign(ScriptableObject definition, string mapId, byte[] png, Vector2 center, float sideLength)
        {
            var path = $"{OutputFolder}/{mapId}.png";
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            var serialized = new SerializedObject(definition);
            serialized.FindProperty("thumbnail").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            serialized.FindProperty("thumbnailCenter").vector2Value = center;
            serialized.FindProperty("thumbnailSideLength").floatValue = sideLength;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }
    }
}
