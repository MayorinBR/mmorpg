using UnityEditor;
using UnityEngine;
using Project.World;

namespace Project.EditorTools
{
    /// <summary>
    /// Adds a button to <see cref="MapThumbnailArea"/> that captures the open scene's thumbnail.
    /// </summary>
    [CustomEditor(typeof(MapThumbnailArea))]
    public class MapThumbnailAreaEditor : Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Capture Thumbnail"))
            {
                MapThumbnailCapture.CaptureOpenScene();
            }
        }
    }
}
