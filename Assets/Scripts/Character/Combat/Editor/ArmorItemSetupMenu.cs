using Project.Items;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Editor menu that builds the skinned armor items listed in the armor manifest: one prefab
    /// per item holding its skinned modules, the item asset, its registration in the item database
    /// and its drop in the Poring monster drops. Idempotent: prefabs are rebuilt in place and existing
    /// item assets keep the values authored in the inspector.
    /// </summary>
    public static class ArmorItemSetupMenu
    {
        private const string ModelsFolder = "Assets/Models/Items/Equipaments";
        private const string PrefabsFolder = "Assets/Prefabs/Items";
        private const string DataFolder = "Assets/Data/Items";
        private const string ItemDatabasePath = "Assets/Data/ItemDatabase.asset";
        private const string PoringLootPath = "Assets/Data/Enemies/PorigonStats.asset";

        /// <summary>
        /// Links the materials, builds the prefabs and item assets of every manifest entry,
        /// registers them in the item database and adds them to the Poring monster drops.
        /// </summary>
        [MenuItem("Tools/Equipment/Build Armor Items")]
        public static void Build()
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
            var loot = AssetDatabase.LoadAssetAtPath<ScriptableObject>(PoringLootPath);

            foreach (var entry in ArmorManifestStore.Load())
            {
                if (!entry.Validate(out var error))
                {
                    Debug.LogWarning($"Equipment: skipping '{entry.assetName}': {error}.");
                    continue;
                }

                foreach (var model in entry.models)
                {
                    ArmorMaterialLinker.Link($"{ModelsFolder}/{model}.fbx", entry.material);
                }

                var prefab = BuildPrefab(entry);

                if (prefab == null)
                {
                    continue;
                }

                var item = LoadOrCreateItem(entry);
                Assign(item, "equippedModel", prefab);
                RegisterInDatabase(database, item);
                AddDrop(loot, item, entry.dropChance);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Equipment: armor items built.");
        }

        private static GameObject BuildPrefab(ArmorManifestEntry spec)
        {
            var folder = $"{PrefabsFolder}/{spec.category}";
            EnsureFolder(folder);

            var root = new GameObject(spec.assetName);
            var covered = new SerializedObject(root.AddComponent<SkinnedEquipment>());
            covered.FindProperty("coveredParts").intValue = (int)spec.ParseCoveredParts();
            covered.ApplyModifiedPropertiesWithoutUndo();

            foreach (var model in spec.models)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsFolder}/{model}.fbx");

                if (source == null)
                {
                    Debug.LogWarning($"Equipment: missing model {ModelsFolder}/{model}.fbx");
                    Object.DestroyImmediate(root);
                    return null;
                }

                PrefabUtility.InstantiatePrefab(source, root.transform);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{folder}/{spec.assetName}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static ItemDefinition LoadOrCreateItem(ArmorManifestEntry spec)
        {
            var folder = $"{DataFolder}/{spec.category}";
            EnsureFolder(folder);

            var path = $"{folder}/{spec.assetName}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

            if (item != null)
            {
                return item;
            }

            item = ScriptableObject.CreateInstance<ItemDefinition>();
            AssetDatabase.CreateAsset(item, path);

            var serialized = new SerializedObject(item);
            serialized.FindProperty("itemName").stringValue = spec.displayName;
            serialized.FindProperty("description").stringValue = spec.description;
            serialized.FindProperty("isStackable").boolValue = false;
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("itemType").intValue = (int)ItemType.Equipment;
            serialized.FindProperty("weight").floatValue = spec.weight;
            serialized.FindProperty("buyPrice").intValue = spec.buyPrice;
            serialized.FindProperty("sellPrice").intValue = spec.sellPrice;
            serialized.FindProperty("statBonuses.vitality").intValue = spec.vitality;
            serialized.FindProperty("statBonuses.agility").intValue = spec.agility;

            var slots = serialized.FindProperty("requiredSlots");
            slots.arraySize = 1;
            slots.GetArrayElementAtIndex(0).intValue = (int)spec.ParseSlot();

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static void RegisterInDatabase(ItemDatabase database, ItemDefinition item)
        {
            if (database == null)
            {
                return;
            }

            var serialized = new SerializedObject(database);
            var all = serialized.FindProperty("allItems");

            if (ContainsItem(all, "", item))
            {
                return;
            }

            all.arraySize++;
            all.GetArrayElementAtIndex(all.arraySize - 1).objectReferenceValue = item;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void AddDrop(ScriptableObject loot, ItemDefinition item, float chance)
        {
            if (loot == null)
            {
                return;
            }

            var serialized = new SerializedObject(loot);
            var entries = serialized.FindProperty("drops");

            if (ContainsItem(entries, "item", item))
            {
                return;
            }

            entries.arraySize++;
            var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            entry.FindPropertyRelative("item").objectReferenceValue = item;
            entry.FindPropertyRelative("dropChance").floatValue = chance;
            entry.FindPropertyRelative("minQuantity").intValue = 1;
            entry.FindPropertyRelative("maxQuantity").intValue = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loot);
        }

        private static bool ContainsItem(SerializedProperty array, string field, Object item)
        {
            for (var i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var reference = field.Length == 0 ? element : element.FindPropertyRelative(field);

                if (reference.objectReferenceValue == item)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
