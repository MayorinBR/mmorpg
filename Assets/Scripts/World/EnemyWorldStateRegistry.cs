using System.Collections.Generic;
using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Snapshot of one enemy's state, captured the moment its map scene
    /// unloads: where it was standing, whether it was alive, its current
    /// health, whether it was actively engaged with the player, and — if
    /// dead — the absolute <see cref="Time.time"/> at which it should
    /// respawn. Using an absolute timestamp instead of a remaining
    /// duration means a respawn timer keeps counting down in real time
    /// even while the map is unloaded, matching how a persistent MMORPG
    /// world behaves rather than pausing offscreen.
    /// </summary>
    public readonly struct EnemyWorldState
    {
        public EnemyWorldState(Vector3 position, bool isAlive, float respawnAtTime, int currentHealth, bool wasEngagingPlayer)
        {
            Position = position;
            IsAlive = isAlive;
            RespawnAtTime = respawnAtTime;
            CurrentHealth = currentHealth;
            WasEngagingPlayer = wasEngagingPlayer;
        }

        public Vector3 Position { get; }
        public bool IsAlive { get; }
        public float RespawnAtTime { get; }
        public int CurrentHealth { get; }
        public bool WasEngagingPlayer { get; }
    }

    /// <summary>
    /// In-memory, session-lifetime registry of enemy state keyed by scene
    /// name and enemy id. An enemy saves its own state here right before
    /// its map scene unloads (see <c>EnemyDeathHandler.OnDestroy</c>) and
    /// restores it right after that scene loads again, so enemies keep
    /// their position and respawn countdown across map switches instead
    /// of resetting to their scene-authored defaults every time. This is
    /// deliberately not persisted to disk: it only needs to survive scene
    /// reloads within the current play session, the same scope as the
    /// existing <see cref="MapTransitionService.PendingSpawnPointId"/>.
    /// </summary>
    public static class EnemyWorldStateRegistry
    {
        private static readonly Dictionary<string, EnemyWorldState> States = new Dictionary<string, EnemyWorldState>();

        /// <summary>Saves or overwrites the state recorded for the given enemy.</summary>
        /// <param name="sceneName">Name of the scene the enemy belongs to.</param>
        /// <param name="enemyId">Stable identifier for the enemy within that scene.</param>
        /// <param name="state">The state to record.</param>
        public static void Save(string sceneName, string enemyId, EnemyWorldState state)
        {
            States[BuildKey(sceneName, enemyId)] = state;
        }

        /// <summary>
        /// Attempts to retrieve previously saved state for the given enemy.
        /// </summary>
        /// <param name="sceneName">Name of the scene the enemy belongs to.</param>
        /// <param name="enemyId">Stable identifier for the enemy within that scene.</param>
        /// <param name="state">The saved state, if any was found.</param>
        /// <returns>True if state was found for this enemy.</returns>
        public static bool TryGet(string sceneName, string enemyId, out EnemyWorldState state)
        {
            return States.TryGetValue(BuildKey(sceneName, enemyId), out state);
        }

        private static string BuildKey(string sceneName, string enemyId)
        {
            return $"{sceneName}:{enemyId}";
        }
    }
}
