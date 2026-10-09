using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Project.AI
{
    /// <summary>
    /// Keeps up to a fixed number of enemies alive on hand-placed spawn points. Every direct child of this
    /// object is a spawn point; there can be more points than <c>maxActive</c>, and each enemy occupies one
    /// point at a time. When an enemy dies its point is freed, and once its respawn delay (set per monster
    /// in the spawn table) elapses it returns on a random free point, possibly the one it just left. Each
    /// slot keeps its species for the whole session. Enemies are named after the area and their slot index,
    /// so the per-enemy world state saved by <see cref="EnemyDeathHandler"/> keeps working across map
    /// switches. Only <see cref="EnemyController"/> prefabs can be assigned, so non-enemy targets such as
    /// the training dummy cannot be spawned here.
    /// </summary>
    public class EnemySpawnArea : MonoBehaviour
    {
        /// <summary>One weighted candidate of the spawn table.</summary>
        [Serializable]
        public class Entry
        {
            [SerializeField] private EnemyController prefab;
            [SerializeField, Min(0f)] private float weight = 1f;
            [SerializeField, Min(0f)] private float respawnDelaySeconds = 30f;

            /// <summary>Gets the enemy prefab to spawn.</summary>
            public EnemyController Prefab => prefab;

            /// <summary>Gets the relative chance of this entry.</summary>
            public float Weight => weight;

            /// <summary>Gets how long this monster stays dead before respawning.</summary>
            public float RespawnDelaySeconds => respawnDelaySeconds;
        }

        private const float NavMeshSampleDistance = 2f;

        private static readonly Dictionary<string, int> SessionSeeds = new Dictionary<string, int>();

        [Tooltip("Regular enemies this area can spawn, picked by weight. Each has its own respawn delay.")]
        [SerializeField] private Entry[] entries;

        [Tooltip("Optional boss that can take the place of a regular spawn.")]
        [SerializeField] private EnemyController bossPrefab;

        [Tooltip("Seconds the boss stays dead before respawning.")]
        [SerializeField, Min(0f)] private float bossRespawnDelaySeconds = 30f;

        [Tooltip("Chance, from 0 to 1, that each spawn becomes the boss.")]
        [SerializeField, Range(0f, 1f)] private float bossChance;

        [Tooltip("Most bosses this area may spawn.")]
        [SerializeField, Min(0)] private int maxBosses = 1;

        [Tooltip("Most enemies alive at the same time. Limited by the number of spawn points (child objects).")]
        [SerializeField, Min(1)] private int maxActive = 5;

        private readonly List<Vector3> pointPositions = new List<Vector3>();
        private readonly Dictionary<EnemyDeathHandler, int> claims = new Dictionary<EnemyDeathHandler, int>();
        private System.Random random;
        private int bossesSpawned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionSeeds()
        {
            SessionSeeds.Clear();
        }

        private void Start()
        {
            CollectPoints();
            random = new System.Random(GetSessionSeed());

            if (maxActive > pointPositions.Count)
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}' has {pointPositions.Count} usable spawn points for maxActive {maxActive}; spawning {pointPositions.Count}.", this);
            }

            var count = Mathf.Min(maxActive, pointPositions.Count);

            for (var slot = 0; slot < count; slot++)
            {
                SpawnSlot(slot);
            }
        }

        private void CollectPoints()
        {
            foreach (Transform point in transform)
            {
                if (NavMesh.SamplePosition(point.position, out var hit, NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    pointPositions.Add(hit.position);
                }
                else
                {
                    Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}': spawn point '{point.name}' is not on the NavMesh and was ignored.", point);
                }
            }
        }

        private int GetSessionSeed()
        {
            var key = gameObject.scene.name + "/" + name;

            if (!SessionSeeds.TryGetValue(key, out var seed))
            {
                seed = SpawnRules.StableHash(key) ^ Environment.TickCount;
                SessionSeeds[key] = seed;
            }

            return seed;
        }

        private void SpawnSlot(int slot)
        {
            if (!TryPick(out var prefab, out var respawnDelay))
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}' has no valid prefab to spawn.", this);
                return;
            }

            var index = PickFreeIndex();

            if (index < 0)
            {
                return;
            }

            var rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
            var enemy = Instantiate(prefab, pointPositions[index], rotation);
            enemy.name = $"{name}_{slot}";
            Manage(enemy, index, respawnDelay);
        }

        private void Manage(EnemyController enemy, int pointIndex, float respawnDelay)
        {
            var handler = enemy.GetComponentInChildren<EnemyDeathHandler>(true);

            if (handler == null)
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}': '{enemy.name}' has no {nameof(EnemyDeathHandler)}, so it will not respawn on spawn points.", enemy);
                return;
            }

            claims[handler] = pointIndex;
            handler.RespawnDelaySeconds = respawnDelay;
            handler.RespawnScheduled += Release;
            handler.RespawnPointProvider = () => Claim(handler);
        }

        private void Release(EnemyDeathHandler owner)
        {
            claims.Remove(owner);
        }

        private Vector3? Claim(EnemyDeathHandler owner)
        {
            var index = PickFreeIndex();

            if (index < 0)
            {
                return null;
            }

            claims[owner] = index;
            return pointPositions[index];
        }

        private int PickFreeIndex()
        {
            var free = Enumerable.Range(0, pointPositions.Count).Where(i => !claims.ContainsValue(i)).ToList();
            return free.Count == 0 ? -1 : free[random.Next(free.Count)];
        }

        private bool TryPick(out EnemyController prefab, out float respawnDelay)
        {
            if (bossPrefab != null && SpawnRules.ShouldSpawnBoss(bossChance, (float)random.NextDouble(), bossesSpawned, maxBosses))
            {
                bossesSpawned++;
                prefab = bossPrefab;
                respawnDelay = bossRespawnDelaySeconds;
                return true;
            }

            var candidates = entries ?? Array.Empty<Entry>();
            var weights = candidates.Select(entry => entry != null && entry.Prefab != null ? entry.Weight : 0f).ToList();
            var index = SpawnRules.PickWeightedIndex(weights, (float)random.NextDouble());

            prefab = index < 0 ? null : candidates[index].Prefab;
            respawnDelay = index < 0 ? 0f : candidates[index].RespawnDelaySeconds;
            return index >= 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);

            foreach (Transform point in transform)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}
