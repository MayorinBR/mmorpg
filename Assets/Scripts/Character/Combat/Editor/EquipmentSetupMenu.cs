using System.Linq;
using Project.Items;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Editor menu that builds the weapon item prefabs and item assets. The
    /// step is idempotent, so running the menu twice never duplicates assets.
    /// </summary>
    public static class EquipmentSetupMenu
    {
        private const string ItemModelsRoot = "Assets/Models/Items/Weapons";
        private const string ItemPrefabsFolder = "Assets/Prefabs/Items/Weapons";
        private const string ItemAssetsFolder = "Assets/Data/Items/Weapon";
        private const string ItemDatabasePath = "Assets/Data/ItemDatabase.asset";
        private const float ReferenceExtent = 2f;
        private const string ToonShaderName = "Project/Items/ToonItem";
        private const string SizeReferenceItem = "IronSword";
        private const float OneHandLengthFactor = 1f;
        private const float TwoHandLengthFactor = 1.3f;
        private const float RodLengthFactor = 1.21f;

        private static readonly EquipmentSlot[] MainHand = { EquipmentSlot.LeftHand };
        private static readonly EquipmentSlot[] BothHands = { EquipmentSlot.LeftHand, EquipmentSlot.RightHand };

        private static readonly ItemSpec[] ItemSpecs =
        {
            new ItemSpec("Swords", "IronSword", new[] { "TestWeapon", "IronSword" }, null, WeaponSubtype.OneHandSword, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 4f, 1.5f),
            new ItemSpec("Swords", "Katana", new[] { "Katana" }, "Katana", WeaponSubtype.OneHandSword, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 3f, 1.5f),
            new ItemSpec("Swords", "Machete", new[] { "Machete" }, "Machete", WeaponSubtype.OneHandSword, WeaponType.Melee, MainHand, 4f, 1.5f, OneHandLengthFactor),
            new ItemSpec("Swords", "Rapier", new[] { "Rapier" }, "Rapier", WeaponSubtype.OneHandSword, WeaponType.Melee, MainHand, 3f, 1.5f, OneHandLengthFactor),
            new ItemSpec("TwoHandSwords", "GiantBlade", new[] { "GiantBlade" }, "Giant Blade", WeaponSubtype.TwoHandSword, WeaponType.Melee, BothHands, 10f, 1.5f, TwoHandLengthFactor),
            new ItemSpec("TwoHandSwords", "Zweihander", new[] { "Zweihander" }, "Zweihander", WeaponSubtype.TwoHandSword, WeaponType.Melee, BothHands, 9f, 1.5f, TwoHandLengthFactor),
            new ItemSpec("Dagger", "IronDagger", new[] { "TestWeaponOff", "IronDagger" }, null, WeaponSubtype.Dagger, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 2f, 1.5f),
            new ItemSpec("Bow", "OakBow", new[] { "OakBow" }, "Oak Bow", WeaponSubtype.Bow, WeaponType.Ranged, new[] { EquipmentSlot.LeftHand, EquipmentSlot.RightHand }, 3f, 8f),
            new ItemSpec("Axe", "NordicAxe", new[] { "NordicAxe" }, "Nordic Axe", WeaponSubtype.OneHandAxe, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 5f, 1.5f),
            new ItemSpec("Book", "AprenticeBook", new[] { "AprenticeBook" }, "Apprentice Book", WeaponSubtype.Book, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 2f, 1.5f),
            new ItemSpec("Staff", "ArcaneStaff", new[] { "ArcaneStaff" }, "Arcane Staff", WeaponSubtype.Rod, WeaponType.Melee, new[] { EquipmentSlot.LeftHand }, 3f, 1.5f),
            new ItemSpec("Staff", "ApprenticeRod", new[] { "ApprenticeRod" }, "Apprentice Rod", WeaponSubtype.Rod, WeaponType.Melee, MainHand, 2f, 1.5f, RodLengthFactor),
        };

        /// <summary>
        /// Creates a toon material and a prefab for every item model, links
        /// the prefab to its item asset (creating the asset when the item does
        /// not exist yet) and registers new items in the item database.
        /// </summary>
        [MenuItem("Tools/Equipment/Build Item Models")]
        public static void BuildItemModels()
        {
            EnsureFolder(ItemPrefabsFolder);
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);

            foreach (var spec in ItemSpecs)
            {
                var prefab = BuildPrefab(spec);
                if (prefab == null)
                {
                    continue;
                }

                foreach (var assetName in spec.AssetNames)
                {
                    var item = LoadOrCreateItem(spec, assetName);
                    if (item == null)
                    {
                        continue;
                    }

                    var serialized = new SerializedObject(item);
                    serialized.FindProperty("equippedModel").objectReferenceValue = prefab;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(item);

                    RegisterInDatabase(database, item);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Equipment: item models built.");
        }

        private static float MeasureExtent(Transform model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return 0f;
            }

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        }

        private static bool NormalizeScale(Transform model)
        {
            var extent = MeasureExtent(model);
            if (extent <= 0f)
            {
                return false;
            }

            var exponent = Mathf.RoundToInt(Mathf.Log10(extent / ReferenceExtent));
            if (exponent == 0)
            {
                return false;
            }

            model.localScale *= Mathf.Pow(10f, -exponent);
            return true;
        }

        /// <summary>
        /// Scales a model so its largest dimension is the length of the size
        /// reference item (the iron sword) times the spec's length factor, so
        /// one-handed swords match the iron sword and two-handed swords are
        /// proportionally larger whatever size they were modelled at.
        /// </summary>
        /// <param name="model">The model instance to scale.</param>
        /// <param name="spec">The item spec carrying the length factor; a factor of zero leaves the model untouched.</param>
        private static void ApplyLengthFactor(Transform model, ItemSpec spec)
        {
            if (spec.LengthFactor <= 0f)
            {
                return;
            }

            var referenceLength = MeasureReferenceLength();
            var length = MeasureExtent(model);
            if (referenceLength <= 0f || length <= 0f)
            {
                Debug.LogWarning($"Equipment: could not size {spec.ModelName} against {SizeReferenceItem}.");
                return;
            }

            model.localScale *= referenceLength * spec.LengthFactor / length;
        }

        private static float MeasureReferenceLength()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ItemPrefabsFolder}/{SizeReferenceItem}.prefab");
            if (prefab == null)
            {
                return 0f;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                return MeasureExtent(instance.transform);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>
        /// Moves a model dropped at the root of the weapons folder into the
        /// folder of its category, keeping its import settings and GUID.
        /// </summary>
        /// <param name="spec">The item spec naming the model and its category folder.</param>
        private static void RelocateFromRoot(ItemSpec spec)
        {
            var rootPath = $"{ItemModelsRoot}/{spec.ModelName}.fbx";
            var targetPath = $"{ItemModelsRoot}/{spec.ModelFolder}/{spec.ModelName}.fbx";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(rootPath) == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) != null)
            {
                return;
            }

            EnsureFolder($"{ItemModelsRoot}/{spec.ModelFolder}");
            var error = AssetDatabase.MoveAsset(rootPath, targetPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning($"Equipment: could not move {rootPath}: {error}");
            }
        }

        private static GameObject BuildPrefab(ItemSpec spec)
        {
            RelocateFromRoot(spec);
            var folder = $"{ItemModelsRoot}/{spec.ModelFolder}";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/{spec.ModelName}.fbx");
            if (model == null)
            {
                Debug.LogWarning($"Equipment: missing model {folder}/{spec.ModelName}.fbx");
                return null;
            }

            var material = LoadOrCreateMaterial(folder, spec.ModelName);
            var prefabPath = $"{ItemPrefabsFolder}/{spec.ModelName}.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            var root = new GameObject(spec.ModelName);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            if (material != null)
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => material).ToArray();
                }
            }

            NormalizeScale(instance.transform);
            ApplyLengthFactor(instance.transform, spec);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Material LoadOrCreateMaterial(string folder, string modelName)
        {
            var path = $"{folder}/{modelName}_Toon.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find(ToonShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"Equipment: shader {ToonShaderName} not found.");
                return existing;
            }

            if (existing != null)
            {
                if (existing.shader != shader)
                {
                    existing.shader = shader;
                    EditorUtility.SetDirty(existing);
                }

                return existing;
            }

            var material = new Material(shader)
            {
                mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/{modelName}_BaseColor.png")
            };
            material.SetTexture("_BaseMap", material.mainTexture);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static ItemDefinition LoadOrCreateItem(ItemSpec spec, string assetName)
        {
            var path = $"{ItemAssetsFolder}/{assetName}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null || spec.DisplayName == null)
            {
                if (item == null)
                {
                    Debug.LogWarning($"Equipment: item asset not found at {path}");
                }

                return item;
            }

            item = ScriptableObject.CreateInstance<ItemDefinition>();
            AssetDatabase.CreateAsset(item, path);

            var serialized = new SerializedObject(item);
            serialized.FindProperty("itemName").stringValue = spec.DisplayName;
            serialized.FindProperty("description").stringValue = $"{spec.DisplayName} placeholder.";
            serialized.FindProperty("isStackable").boolValue = false;
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("itemType").intValue = (int)ItemType.Equipment;
            serialized.FindProperty("weight").floatValue = spec.Weight;
            serialized.FindProperty("weaponType").intValue = (int)spec.WeaponType;
            serialized.FindProperty("weaponSubtype").intValue = (int)spec.Subtype;
            serialized.FindProperty("attackRange").floatValue = spec.AttackRange;

            var slots = serialized.FindProperty("requiredSlots");
            slots.arraySize = spec.Slots.Length;
            for (var i = 0; i < spec.Slots.Length; i++)
            {
                slots.GetArrayElementAtIndex(i).intValue = (int)spec.Slots[i];
            }

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
            for (var i = 0; i < all.arraySize; i++)
            {
                if (all.GetArrayElementAtIndex(i).objectReferenceValue == item)
                {
                    return;
                }
            }

            all.arraySize++;
            all.GetArrayElementAtIndex(all.arraySize - 1).objectReferenceValue = item;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
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

        private readonly struct ItemSpec
        {
            public readonly string ModelFolder;
            public readonly string ModelName;
            public readonly string[] AssetNames;
            public readonly string DisplayName;
            public readonly WeaponSubtype Subtype;
            public readonly WeaponType WeaponType;
            public readonly EquipmentSlot[] Slots;
            public readonly float Weight;
            public readonly float AttackRange;
            public readonly float LengthFactor;

            public ItemSpec(string modelFolder, string modelName, string[] assetNames, string displayName,
                WeaponSubtype subtype, WeaponType weaponType, EquipmentSlot[] slots, float weight, float attackRange,
                float lengthFactor = 0f)
            {
                ModelFolder = modelFolder;
                ModelName = modelName;
                AssetNames = assetNames;
                DisplayName = displayName;
                Subtype = subtype;
                WeaponType = weaponType;
                Slots = slots;
                Weight = weight;
                AttackRange = attackRange;
                LengthFactor = lengthFactor;
            }
        }
    }
}
