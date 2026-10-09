using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Project.Character.Head;
using Project.Character.Head.EditorTools;

namespace Project.Character.Npc.EditorTools
{
    /// <summary>
    /// Inspector for <see cref="NpcAppearance"/>: lists the head shapes of the catalog by name and
    /// previews every change on the character, in the scene or in the prefab being edited.
    /// </summary>
    [CustomEditor(typeof(NpcAppearance))]
    public sealed class NpcAppearanceEditor : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty catalogProperty = serializedObject.FindProperty("headCatalog");
            if (catalogProperty.objectReferenceValue == null)
            {
                catalogProperty.objectReferenceValue = HeadCatalogBuilder.Build();
            }

            EditorGUI.BeginChangeCheck();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gender"));
            }

            EditorGUILayout.HelpBox("Gender follows the base prefab. For a female NPC, create the variant from Female_Body.", MessageType.None);

            DrawPropertiesExcluding(serializedObject, "m_Script", "gender", "headStyle");
            DrawHeadPopup(serializedObject.FindProperty("headStyle"), catalogProperty.objectReferenceValue as HeadCatalog);

            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Apply Preview"))
            {
                changed = true;
            }

            if (changed)
            {
                Preview((NpcAppearance)target);
            }
        }

        private static void Preview(NpcAppearance appearance)
        {
            appearance.Rebuild();

            foreach (SkinnedMeshRenderer renderer in appearance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer.hideFlags == HideFlags.None)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                    EditorUtility.SetDirty(renderer);
                }
            }

            EditorUtility.SetDirty(appearance);
            if (appearance.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(appearance.gameObject.scene);
            }
        }

        private static void DrawHeadPopup(SerializedProperty index, HeadCatalog catalog)
        {
            if (catalog == null || catalog.Styles.Count == 0)
            {
                EditorGUILayout.PropertyField(index, new GUIContent("Head Style Index"));
                return;
            }

            var options = new List<string>();
            foreach (HeadStyleEntry entry in catalog.Styles)
            {
                options.Add(entry.DisplayName);
            }

            index.intValue = EditorGUILayout.Popup("Head Style", Mathf.Clamp(index.intValue, 0, options.Count - 1), options.ToArray());
        }
    }
}
