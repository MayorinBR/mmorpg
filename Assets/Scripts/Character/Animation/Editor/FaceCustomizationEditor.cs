using UnityEditor;
using UnityEngine;

namespace Project.Character.Animation.EditorTools
{
    /// <summary>
    /// Inspector for <see cref="FaceCustomization"/>: shows the eye and mouth drawings as
    /// dropdowns that list the sprite names from the <see cref="FacePartCatalog"/>.
    /// </summary>
    [CustomEditor(typeof(FaceCustomization))]
    public sealed class FaceCustomizationEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty catalogProperty = serializedObject.FindProperty("catalog");
            if (catalogProperty.objectReferenceValue == null)
            {
                catalogProperty.objectReferenceValue = FindCatalog();
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "eyeIndex", "mouthIndex");

            var catalog = catalogProperty.objectReferenceValue as FacePartCatalog;
            DrawPartPopup("Eye Style", serializedObject.FindProperty("eyeIndex"), catalog != null ? catalog.EyeNames : null);
            DrawPartPopup("Mouth Style", serializedObject.FindProperty("mouthIndex"), catalog != null ? catalog.MouthNames : null);

            if (catalog == null)
            {
                EditorGUILayout.HelpBox("No Face Part Catalog found. Run Tools > Face > Rebuild Face Textures.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawPartPopup(string label, SerializedProperty index, System.Collections.Generic.IReadOnlyList<string> names)
        {
            if (names == null || names.Count == 0)
            {
                EditorGUILayout.PropertyField(index, new GUIContent(label));
                return;
            }

            var options = new string[names.Count];
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = names[i];
            }

            index.intValue = EditorGUILayout.Popup(label, Mathf.Clamp(index.intValue, 0, options.Length - 1), options);
        }

        private static FacePartCatalog FindCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:FacePartCatalog");
            return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<FacePartCatalog>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }
    }
}
