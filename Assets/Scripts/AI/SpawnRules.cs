using System.Collections.Generic;

namespace Project.AI
{
    /// <summary>
    /// Pure decision rules behind <see cref="EnemySpawnArea"/>, kept free of Unity types so they can be
    /// unit tested without a scene.
    /// </summary>
    public static class SpawnRules
    {
        /// <summary>
        /// Picks an index from a weight list. An entry's chance is its weight divided by the sum of all
        /// positive weights; entries with a non-positive weight are never picked.
        /// </summary>
        /// <param name="weights">The weight of each candidate.</param>
        /// <param name="roll">A random value in the range [0, 1).</param>
        /// <returns>The picked index, or -1 when no candidate has a positive weight.</returns>
        public static int PickWeightedIndex(IReadOnlyList<float> weights, float roll)
        {
            var total = 0f;
            var lastPositive = -1;

            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] > 0f)
                {
                    total += weights[i];
                    lastPositive = i;
                }
            }

            if (lastPositive < 0)
            {
                return -1;
            }

            var target = roll * total;
            var cumulative = 0f;

            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                cumulative += weights[i];

                if (target < cumulative)
                {
                    return i;
                }
            }

            return lastPositive;
        }

        /// <summary>
        /// Decides whether the next spawn should be the boss instead of a regular enemy.
        /// </summary>
        /// <param name="chance">The chance, from 0 to 1, that a spawn becomes the boss.</param>
        /// <param name="roll">A random value in the range [0, 1).</param>
        /// <param name="bossesSpawned">How many bosses this area has already spawned.</param>
        /// <param name="maxBosses">The most bosses this area may spawn.</param>
        /// <returns>True when the boss should spawn.</returns>
        public static bool ShouldSpawnBoss(float chance, float roll, int bossesSpawned, int maxBosses)
        {
            return bossesSpawned < maxBosses && roll < chance;
        }

        /// <summary>
        /// Computes a hash of a string that is stable across runs and platforms (FNV-1a), unlike
        /// <see cref="string.GetHashCode"/>.
        /// </summary>
        /// <param name="value">The text to hash.</param>
        /// <returns>The hash value.</returns>
        public static int StableHash(string value)
        {
            unchecked
            {
                var hash = (int)2166136261;

                foreach (var character in value)
                {
                    hash = (hash ^ character) * 16777619;
                }

                return hash;
            }
        }
    }
}
