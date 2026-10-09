using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Project.Items;

namespace Project.EditorTools
{
    /// <summary>
    /// Editor-only Play Mode window that adds any <see cref="ItemDefinition"/>
    /// in the project to the player's inventory, in any quantity. Lives in
    /// <c>Project.EditorTools</c>, so it never ships in a player build.
    /// Quantities are still limited by the carry weight, like any pickup.
    /// </summary>
    public sealed class InventoryDebugWindow : EditorWindow
    {
        private readonly List<ItemDefinition> items = new List<ItemDefinition>();
        private string filter = string.Empty;
        private int quantity = 1;
        private Vector2 scroll;

        [MenuItem("Tools/Debug/Add Items To Inventory")]
        private static void Open()
        {
            GetWindow<InventoryDebugWindow>("Inventory Debug");
        }

        private void OnEnable()
        {
            LoadItems();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            filter = EditorGUILayout.TextField("Filter", filter);

            if (GUILayout.Button("Reload", GUILayout.Width(60f)))
            {
                LoadItems();
            }

            EditorGUILayout.EndHorizontal();
            quantity = Mathf.Max(1, EditorGUILayout.IntField("Quantity", quantity));

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to add items.", MessageType.Info);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                foreach (var item in items)
                {
                    if (!Matches(item))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{item.name}  (weight {item.Weight})");

                    if (GUILayout.Button("Add", GUILayout.Width(60f)))
                    {
                        Add(item, quantity);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private bool Matches(ItemDefinition item)
        {
            return string.IsNullOrEmpty(filter) || item.name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LoadItems()
        {
            items.Clear();

            foreach (var guid in AssetDatabase.FindAssets("t:ItemDefinition"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));

                if (item != null)
                {
                    items.Add(item);
                }
            }

            items.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        }

        private static void Add(ItemDefinition item, int amount)
        {
            var inventory = Object.FindFirstObjectByType<PlayerInventory>();

            if (inventory == null)
            {
                Debug.LogWarning("InventoryDebugWindow: no PlayerInventory found in the current scene.");
                return;
            }

            var addable = inventory.Items.GetAddableQuantity(item, amount);
            inventory.Items.TryAddItem(item, amount);
            Debug.Log($"InventoryDebugWindow: added {addable} of {amount} x {item.name}.");
        }
    }
}
