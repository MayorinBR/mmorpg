using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Project.Character.Head;
using Project.Character.Head.EditorTools;
using Project.Items;

namespace Project.Character.Npc.EditorTools
{
    /// <summary>
    /// Creates the test NPC prefab variants from the Male_Body and Female_Body prefabs. Each variant
    /// carries a configured <see cref="NpcAppearance"/>. Running it again recreates the variants.
    /// </summary>
    public static class NpcSetupMenu
    {
        private const string OutputFolder = "Assets/Prefabs/Npcs";
        private const string MaleBodyPath = "Assets/Prefabs/Characters/Male_Body.prefab";
        private const string FemaleBodyPath = "Assets/Prefabs/Characters/Female_Body.prefab";
        private const string ItemsRoot = "Assets/Data/Items";

        private static readonly NpcSpec[] Specs =
        {
            new NpcSpec("NPC_Male", false, "Square", "Armor/PlateArmor", "Legs/PlateLegs", "Boots/LowLeatherBoots"),
            new NpcSpec("NPC_Female", true, "Slim", "Armor/RangerArmor", "Legs/BasicPants", "Boots/RangerBoots")
        };

        /// <summary>
        /// Creates or replaces the test NPC variants in the NPC prefab folder.
        /// </summary>
        [MenuItem("Tools/NPC/Build Test NPCs")]
        public static void Build()
        {
            EnsureFolder();
            HeadCatalog catalog = HeadCatalogBuilder.Build();

            foreach (NpcSpec spec in Specs)
            {
                CreateVariant(spec, catalog);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateVariant(NpcSpec spec, HeadCatalog catalog)
        {
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(spec.Female ? FemaleBodyPath : MaleBodyPath);
            if (body == null)
            {
                Debug.LogError($"Body prefab for {spec.Name} was not found.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(body);
            instance.name = spec.Name;

            var appearance = instance.AddComponent<NpcAppearance>();
            var serialized = new SerializedObject(appearance);
            serialized.FindProperty("gender").enumValueIndex = spec.Female ? 1 : 0;
            serialized.FindProperty("headCatalog").objectReferenceValue = catalog;
            serialized.FindProperty("headStyle").intValue = FindHeadIndex(catalog, spec.HeadId);
            WriteOutfit(serialized.FindProperty("outfit"), spec.ItemPaths);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            string path = $"{OutputFolder}/{spec.Name}.prefab";
            AssetDatabase.DeleteAsset(path);
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static void WriteOutfit(SerializedProperty outfit, IReadOnlyList<string> itemPaths)
        {
            outfit.arraySize = itemPaths.Count;

            for (int i = 0; i < itemPaths.Count; i++)
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsRoot}/{itemPaths[i]}.asset");
                if (item == null)
                {
                    Debug.LogWarning($"Item '{itemPaths[i]}' was not found and is left empty.");
                }

                outfit.GetArrayElementAtIndex(i).objectReferenceValue = item;
            }
        }

        private static int FindHeadIndex(HeadCatalog catalog, string headId)
        {
            for (int i = 0; i < catalog.Styles.Count; i++)
            {
                if (catalog.Styles[i].Id == headId)
                {
                    return i;
                }
            }

            return 0;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Npcs");
            }
        }

        private readonly struct NpcSpec
        {
            public NpcSpec(string name, bool female, string headId, params string[] itemPaths)
            {
                Name = name;
                Female = female;
                HeadId = headId;
                ItemPaths = itemPaths;
            }

            public string Name { get; }
            public bool Female { get; }
            public string HeadId { get; }
            public string[] ItemPaths { get; }
        }
    }
}
