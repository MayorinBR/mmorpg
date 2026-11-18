using System;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Gives a <see cref="TrainingDummy"/> a hostile behavior for skill testing: on a fixed interval it attacks a
    /// player standing within range, either instantly or through a visual projectile, and inflicts the configured
    /// status effects on a landed attack. A blocking zone covering the player (Safety Wall for melee, Pneuma for
    /// ranged) cancels the attack, matching what <c>EnemyAttackState</c> does for real enemies.
    /// </summary>
    public class TrainingDummyAttacker : MonoBehaviour
    {
        [Serializable]
        private struct StatusToInflict
        {
            public StatusEffectType type;
            public float durationSeconds;
            public int poisonDamagePerTick;
        }

        [Tooltip("Layers searched for a player to attack.")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Distance within which a player is attacked.")]
        [SerializeField] private float range = 8f;

        [Tooltip("Seconds between attacks.")]
        [SerializeField] private float intervalSeconds = 4f;

        [Tooltip("When false, a player hidden by Hiding is ignored, like a regular enemy. When true, this dummy still sees hidden players, e.g. to test Play Dead being hit.")]
        [SerializeField] private bool detectsHidden;

        [Tooltip("When false, a player lying from Play Dead is ignored. When true, this dummy still attacks them, to test that taking damage makes them stand up.")]
        [SerializeField] private bool hitsPlayingDead;

        [Tooltip("Whether the attack counts as melee or ranged when checked against blocking zones.")]
        [SerializeField] private AttackRangeKind attackKind = AttackRangeKind.Melee;

        [Tooltip("Damage dealt by a landed attack. Zero deals no damage and only inflicts statuses.")]
        [SerializeField] private int damage;

        [Tooltip("Optional visual projectile. When set, the attack resolves on arrival instead of instantly.")]
        [SerializeField] private GameObject projectilePrefab;

        [SerializeField] private float projectileSpeed = 12f;

        [Tooltip("Status effects inflicted on every landed attack.")]
        [SerializeField] private StatusToInflict[] statuses;

        private float cooldownRemaining;

        private void Update()
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining > 0f)
            {
                return;
            }

            var target = FindTarget();

            if (target == null)
            {
                return;
            }

            cooldownRemaining = intervalSeconds;
            Launch(target);
        }

        private Transform FindTarget()
        {
            foreach (var hit in Physics.OverlapSphere(transform.position, range, playerLayer))
            {
                var effects = hit.transform.root.GetComponentInChildren<StatusEffectController>();

                var visible = effects == null || !effects.IsHidden || detectsHidden;
                var alive = effects == null || !effects.IsPlayingDead || hitsPlayingDead;

                if (visible && alive)
                {
                    return hit.transform;
                }
            }

            return null;
        }

        private void Launch(Transform target)
        {
            if (projectilePrefab == null)
            {
                Resolve(target);
                return;
            }

            Projectile.Spawn(projectilePrefab, transform.position + Vector3.up, target, projectileSpeed, () => Resolve(target));
        }

        private void Resolve(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (SkillZoneController.BlocksAttack(target.position, attackKind))
            {
                CombatLog.Write($"{name}: {attackKind} attack blocked by a zone.");
                return;
            }

            var damageable = target.root.GetComponentInChildren<IDamageable>();

            if (damageable == null || damageable.IsDead)
            {
                return;
            }

            if (damage > 0)
            {
                damageable.TakeDamage(damage, attacker: transform);
            }

            InflictStatuses(target);
            CombatLog.Write($"{name}: {attackKind} attack landed ({damage} damage, {statuses.Length} status effect(s)).");
        }

        private void InflictStatuses(Transform target)
        {
            var effects = target.root.GetComponentInChildren<StatusEffectController>();

            if (effects == null)
            {
                return;
            }

            foreach (var status in statuses)
            {
                effects.Apply(status.type, status.durationSeconds, status.poisonDamagePerTick);
            }
        }
    }
}
