using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Project.Ambience;
using Project.World;

namespace Project.AI
{
    /// <summary>
    /// Keeps up to a fixed number of enemies alive on hand-placed spawn points. Every direct child of this
    /// object is a spawn point; there can be more points than <c>maxActive</c>, and each enemy occupies one
    /// point at a time. When an enemy dies its point is freed, and once its respawn delay (set per monster
    /// in the spawn table) elapses it returns on a random free point, possibly the one it just left.
    /// Each table entry can be limited to the day or to the night. A slot keeps its species while that
    /// species is allowed; when the period changes, an enemy that no longer fits is replaced gradually:
    /// a dead one once its respawn delay has elapsed, a living one once it is out of combat and away from
    /// the player. Enemies are named after the area and their slot index (plus the period for period-bound
    /// species), so the per-enemy world state saved by <see cref="EnemyDeathHandler"/> keeps working across
    /// map switches. Only <see cref="EnemyController"/> prefabs can be assigned, so non-enemy targets such
    /// as the training dummy cannot be spawned here.
    /// </summary>
    public class EnemySpawnArea : MonoBehaviour
    {
        /// <summary>One weighted candidate of the spawn table.</summary>
        [Serializable]
        public class Entry
        {
            [SerializeField] private EnemyController prefab;
            [SerializeField, Min(0f)] private float weight = 1f;
            [Tooltip("Overrides the monster's own respawn delay. A negative value uses the delay set on its MonsterDefinition.")]
            [SerializeField] private float respawnDelaySeconds = -1f;
            [Tooltip("When of the day and night cycle this monster can appear.")]
            [SerializeField] private SpawnPeriod period = SpawnPeriod.Always;

            /// <summary>Gets the enemy prefab to spawn.</summary>
            public EnemyController Prefab => prefab;

            /// <summary>Gets the relative chance of this entry.</summary>
            public float Weight => weight;

            /// <summary>Gets when of the day and night cycle this monster can appear.</summary>
            public SpawnPeriod Period => period;

            /// <summary>
            /// Gets how long this monster stays dead before respawning: the entry's override when set,
            /// otherwise the delay on the monster's definition.
            /// </summary>
            public float RespawnDelaySeconds
            {
                get
                {
                    if (respawnDelaySeconds >= 0f)
                    {
                        return respawnDelaySeconds;
                    }

                    return prefab != null && prefab.Stats is MonsterDefinition monster ? monster.RespawnDelaySeconds : 30f;
                }
            }
        }

        /// <summary>The enemy currently held by one slot and what is needed to judge it.</summary>
        private sealed class Slot
        {
            public int Index;
            public EnemyController Enemy;
            public EnemyDeathHandler Handler;
            public SpawnPeriod Period = SpawnPeriod.Always;
        }

        private const float NavMeshSampleDistance = 2f;
        private const float RefreshIntervalSeconds = 5f;
        private const float ReplaceDistanceFromPlayer = 25f;

        private static readonly Dictionary<string, int> SessionSeeds = new Dictionary<string, int>();

        [Tooltip("Regular enemies this area can spawn, picked by weight. Each has its own respawn delay and period.")]
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
        private readonly List<Slot> slots = new List<Slot>();
        private System.Random random;
        private int sessionSeed;
        private int bossesSpawned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionSeeds()
        {
            SessionSeeds.Clear();
        }

        private void Start()
        {
            CollectPoints();
            sessionSeed = GetSessionSeed();
            random = new System.Random(sessionSeed);

            if (maxActive > pointPositions.Count)
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}' has {pointPositions.Count} usable spawn points for maxActive {maxActive}; spawning {pointPositions.Count}.", this);
            }

            if (entries == null || entries.All(entry => entry == null || entry.Prefab == null))
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}' has no valid prefab to spawn.", this);
            }

            var count = Mathf.Min(maxActive, pointPositions.Count);

            for (var index = 0; index < count; index++)
            {
                var slot = new Slot { Index = index };
                slots.Add(slot);
                SpawnSlot(slot);
            }

            StartCoroutine(RefreshRoutine());
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

        private IEnumerator RefreshRoutine()
        {
            var wait = new WaitForSeconds(RefreshIntervalSeconds);

            while (true)
            {
                yield return wait;
                RefreshSlots();
            }
        }

        /// <summary>
        /// Fills empty slots and replaces enemies whose species no longer fits the current period,
        /// when it is safe to do so.
        /// </summary>
        private void RefreshSlots()
        {
            var isNight = WorldClock.IsNight;

            foreach (var slot in slots)
            {
                if (slot.Enemy == null)
                {
                    SpawnSlot(slot);
                }
                else if (!SpawnRules.IsAllowed(slot.Period, isNight) && CanReplace(slot))
                {
                    Retire(slot);
                    SpawnSlot(slot);
                }
            }
        }

        private bool CanReplace(Slot slot)
        {
            if (slot.Handler != null && !slot.Handler.IsAlive)
            {
                return Time.time >= slot.Handler.RespawnAtTime;
            }

            if (slot.Enemy.PlayerTarget != null)
            {
                return false;
            }

            var player = PersistentPlayerAnchor.Instance;

            return player == null
                || Vector3.Distance(player.transform.position, slot.Enemy.transform.position) > ReplaceDistanceFromPlayer;
        }

        private void Retire(Slot slot)
        {
            if (slot.Handler != null)
            {
                claims.Remove(slot.Handler);
            }

            Destroy(slot.Enemy.gameObject);
            slot.Enemy = null;
            slot.Handler = null;
        }

        private void SpawnSlot(Slot slot)
        {
            if (!TryPick(slot.Index, WorldClock.IsNight, out var prefab, out var respawnDelay, out var period))
            {
                return;
            }

            var index = PickFreeIndex();

            if (index < 0)
            {
                return;
            }

            var rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
            var enemy = Instantiate(prefab, pointPositions[index], rotation);
            enemy.name = period == SpawnPeriod.Always ? $"{name}_{slot.Index}" : $"{name}_{slot.Index}_{period}";
            slot.Enemy = enemy;
            slot.Period = period;
            Manage(slot, index, respawnDelay);
        }

        private void Manage(Slot slot, int pointIndex, float respawnDelay)
        {
            var handler = slot.Enemy.GetComponentInChildren<EnemyDeathHandler>(true);

            if (handler == null)
            {
                Debug.LogWarning($"{nameof(EnemySpawnArea)} '{name}': '{slot.Enemy.name}' has no {nameof(EnemyDeathHandler)}, so it will not respawn on spawn points.", slot.Enemy);
                return;
            }

            slot.Handler = handler;
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

        /// <summary>
        /// Picks the species of a slot for a period. The roll comes from a generator seeded by the area,
        /// the slot and the period, so a slot gets the same species every time it is filled in the same
        /// period, which keeps the saved world state of that species valid after a map switch.
        /// </summary>
        private bool TryPick(int slotIndex, bool isNight, out EnemyController prefab, out float respawnDelay, out SpawnPeriod period)
        {
            var slotRandom = new System.Random(sessionSeed ^ (slotIndex * 7919) ^ (isNight ? 104729 : 0));
            period = SpawnPeriod.Always;

            if (bossPrefab != null && SpawnRules.ShouldSpawnBoss(bossChance, (float)slotRandom.NextDouble(), bossesSpawned, maxBosses))
            {
                bossesSpawned++;
                prefab = bossPrefab;
                respawnDelay = bossRespawnDelaySeconds;
                return true;
            }

            var candidates = entries ?? Array.Empty<Entry>();
            var weights = candidates
                .Select(entry => entry != null && entry.Prefab != null && SpawnRules.IsAllowed(entry.Period, isNight) ? entry.Weight : 0f)
                .ToList();
            var index = SpawnRules.PickWeightedIndex(weights, (float)slotRandom.NextDouble());

            prefab = index < 0 ? null : candidates[index].Prefab;
            respawnDelay = index < 0 ? 0f : candidates[index].RespawnDelaySeconds;
            period = index < 0 ? SpawnPeriod.Always : candidates[index].Period;
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
