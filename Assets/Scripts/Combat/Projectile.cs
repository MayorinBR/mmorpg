using System;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// A visual-only projectile: travels from its spawn position toward a
    /// target transform (following it if it moves) and invokes a callback
    /// on arrival. Hit/miss and damage are already decided by the time one
    /// of these is spawned (see <see cref="Character.Combat.PlayerCombatController.DealHit"/>) —
    /// this exists purely so an arrow or spell bolt visually travels before
    /// its already-resolved outcome is applied, instead of landing
    /// instantly. Keeps flying toward the target's last known position if
    /// the target is destroyed mid-flight, rather than stopping short.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const float ArrivalDistance = 0.15f;

        private Transform target;
        private Vector3 lastKnownPosition;
        private float speed;
        private Action onArrival;

        /// <summary>
        /// Spawns a new projectile at <paramref name="origin"/>, using
        /// <paramref name="prefab"/> for its visuals if one is assigned (it
        /// must carry its own <see cref="Projectile"/>, or one is added),
        /// or a bare invisible GameObject otherwise.
        /// </summary>
        /// <param name="prefab">Optional visual prefab, or null for a logic-only projectile.</param>
        /// <param name="origin">World position to spawn the projectile at.</param>
        /// <param name="target">Transform the projectile travels toward.</param>
        /// <param name="speed">Travel speed, in meters per second.</param>
        /// <param name="onArrival">Invoked once the projectile reaches its target, then the projectile destroys itself.</param>
        public static void Spawn(GameObject prefab, Vector3 origin, Transform target, float speed, Action onArrival)
        {
            var instance = prefab != null ? Instantiate(prefab, origin, Quaternion.identity) : new GameObject("Projectile");
            instance.transform.position = origin;

            var projectile = instance.GetComponent<Projectile>();
            if (projectile == null)
            {
                projectile = instance.AddComponent<Projectile>();
            }

            projectile.target = target;
            projectile.lastKnownPosition = target != null ? target.position : origin;
            projectile.speed = speed;
            projectile.onArrival = onArrival;
        }

        private void Update()
        {
            var destination = target != null ? target.position : lastKnownPosition;
            lastKnownPosition = destination;

            var toDestination = destination - transform.position;

            if (toDestination.sqrMagnitude <= ArrivalDistance * ArrivalDistance)
            {
                onArrival?.Invoke();
                Destroy(gameObject);
                return;
            }

            var direction = toDestination.normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.forward = direction;
        }
    }
}
