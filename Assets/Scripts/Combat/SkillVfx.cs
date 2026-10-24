using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Spawns fire-and-forget skill visual effects that destroy themselves once their particles have finished.
    /// </summary>
    public static class SkillVfx
    {
        private const float MinimumLifetimeSeconds = 1f;

        /// <summary>
        /// Instantiates an effect at a world position and schedules its destruction.
        /// </summary>
        /// <param name="prefab">The effect to spawn; nothing happens when null.</param>
        /// <param name="position">World position of the effect.</param>
        /// <returns>The spawned instance, or null when there was no prefab.</returns>
        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            Object.Destroy(instance, GetLifetimeSeconds(instance));
            return instance;
        }

        /// <summary>
        /// Gets how long the effect must live so its longest particle system can finish.
        /// </summary>
        private static float GetLifetimeSeconds(GameObject instance)
        {
            var lifetime = MinimumLifetimeSeconds;

            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>())
            {
                var main = system.main;
                lifetime = Mathf.Max(lifetime, main.duration + main.startLifetime.constantMax);
            }

            return lifetime;
        }
    }
}
