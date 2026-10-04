using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Hair.EditorTools
{
    /// <summary>
    /// Inspector for <see cref="HairCustomization"/>: shows the hairstyle as a dropdown that lists
    /// the names from the <see cref="HairCatalog"/> and previews every change on the character.
    /// </summary>
    [CustomEditor(typeof(HairCustomization))]
    public sealed class HairCustomizationEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty catalogProperty = serializedObject.FindProperty("catalog");
            if (catalogProperty.objectReferenceValue == null)
            {
                catalogProperty.objectReferenceValue = HairCatalogBuilder.Build();
            }

            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, "m_Script", "styleIndex");
            DrawStylePopup(serializedObject.FindProperty("styleIndex"), catalogProperty.objectReferenceValue as HairCatalog);

            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Rebuild Catalog From Hair Folder"))
            {
                catalogProperty.objectReferenceValue = HairCatalogBuilder.Build();
                changed = true;
            }

            var customization = (HairCustomization)target;
            if (changed)
            {
                customization.Rebuild();
            }

            DrawPlacementButtons(customization);
        }

        private static void DrawPlacementButtons(HairCustomization customization)
        {
            Transform hair = customization.FindInstance();
            bool hasStyle = customization.Catalog != null
                && customization.Catalog.TryGet(customization.StyleIndex, out _);

            using (new EditorGUI.DisabledScope(hair == null || !hasStyle))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Move the Hair object under the socket, then save its position, rotation and scale for this style and body type.", MessageType.None);

                if (GUILayout.Button("Save Placement To Style"))
                {
                    StorePlacement(customization, HairPlacement.Capture(hair));
                }

                if (GUILayout.Button("Reset Placement"))
                {
                    StorePlacement(customization, new HairPlacement());
                    customization.Rebuild();
                }
            }
        }

        private static void StorePlacement(HairCustomization customization, HairPlacement placement)
        {
            HairCatalog catalog = customization.Catalog;
            catalog.TryGet(customization.StyleIndex, out HairStyleEntry entry);

            Undo.RecordObject(catalog, "Save Hair Placement");
            entry.SetPlacement(customization.BodyType, placement);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static void DrawStylePopup(SerializedProperty index, HairCatalog catalog)
        {
            if (catalog == null || catalog.Styles.Count == 0)
            {
                EditorGUILayout.PropertyField(index, new GUIContent("Style Index"));
                return;
            }

            var options = new List<string> { "None" };
            foreach (HairStyleEntry entry in catalog.Styles)
            {
                options.Add(entry.DisplayName);
            }

            int selected = EditorGUILayout.Popup("Hair Style", Mathf.Clamp(index.intValue + 1, 0, options.Count - 1), options.ToArray());
            index.intValue = selected - 1;
        }
    }
}
