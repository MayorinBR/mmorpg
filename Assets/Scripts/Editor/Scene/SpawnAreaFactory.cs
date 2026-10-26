using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// One weighted enemy of a spawn table, described by prefab path.
    /// </summary>
    public readonly struct SpawnEntryPlan
    {
        /// <summary>
        /// Creates a spawn table entry.
        /// </summary>
        /// <param name="prefabPath">Asset path of the enemy prefab.</param>
        /// <param name="weight">Relative chance of this enemy.</param>
        /// <param name="respawnSeconds">Seconds the enemy stays dead before returning.</param>
        public SpawnEntryPlan(string prefabPath, float weight, float respawnSeconds)
        {
            PrefabPath = prefabPath;
            Weight = weight;
            RespawnSeconds = respawnSeconds;
        }

        /// <summary>Gets the asset path of the enemy prefab.</summary>
        public string PrefabPath { get; }

        /// <summary>Gets the relative chance of this enemy.</summary>
        public float Weight { get; }

        /// <summary>Gets the respawn delay in seconds.</summary>
        public float RespawnSeconds { get; }
    }

    /// <summary>
    /// Creates configured enemy spawn areas without a compile-time reference to the AI assembly.
    /// </summary>
    public static class SpawnAreaFactory
    {
        private const string AreaTypeName = "Project.AI.EnemySpawnArea";
        private const string ControllerTypeName = "Project.AI.EnemyController";

        /// <summary>
        /// Creates a spawn area whose children are the given spawn points.
        /// </summary>
        /// <param name="name">Object name; also the prefix of the spawned enemy names.</param>
        /// <param name="points">World positions of the spawn points.</param>
        /// <param name="entries">Regular enemy table.</param>
        /// <param name="maxActive">Most enemies alive at once.</param>
        /// <param name="bossPrefabPath">Optional boss prefab path, or null for none.</param>
        /// <param name="bossRespawnSeconds">Seconds the boss stays dead.</param>
        /// <returns>The spawn area object.</returns>
        public static GameObject Create(
            string name,
            IReadOnlyList<Vector3> points,
            IReadOnlyList<SpawnEntryPlan> entries,
            int maxActive,
            string bossPrefabPath = null,
            float bossRespawnSeconds = 300f)
        {
            var area = new GameObject(name);

            for (var index = 0; index < points.Count; index++)
            {
                var point = new GameObject($"Point_{index:00}");
                point.transform.SetParent(area.transform, false);
                point.transform.position = points[index];
            }

            var component = area.AddComponent(EditorReflection.FindType(AreaTypeName));
            EditorReflection.Set(component, serialized =>
            {
                WriteEntries(serialized.FindProperty("entries"), entries);
                serialized.FindProperty("maxActive").intValue = maxActive;
                WriteBoss(serialized, bossPrefabPath, bossRespawnSeconds);
            });

            return area;
        }

        private static void WriteEntries(SerializedProperty list, IReadOnlyList<SpawnEntryPlan> entries)
        {
            list.arraySize = entries.Count;

            for (var index = 0; index < entries.Count; index++)
            {
                var element = list.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("prefab").objectReferenceValue = LoadController(entries[index].PrefabPath);
                element.FindPropertyRelative("weight").floatValue = entries[index].Weight;
                element.FindPropertyRelative("respawnDelaySeconds").floatValue = entries[index].RespawnSeconds;
            }
        }

        private static void WriteBoss(SerializedObject serialized, string bossPrefabPath, float respawnSeconds)
        {
            if (string.IsNullOrEmpty(bossPrefabPath))
            {
                return;
            }

            serialized.FindProperty("bossPrefab").objectReferenceValue = LoadController(bossPrefabPath);
            serialized.FindProperty("bossChance").floatValue = 1f;
            serialized.FindProperty("maxBosses").intValue = 1;
            serialized.FindProperty("bossRespawnDelaySeconds").floatValue = respawnSeconds;
        }

        private static Object LoadController(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                ?? throw new System.InvalidOperationException($"Enemy prefab not found at {prefabPath}. Build the enemy roster first.");

            return prefab.GetComponentInChildren(EditorReflection.FindType(ControllerTypeName), true);
        }
    }
}
