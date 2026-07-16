using UnityEngine;
using Project.Character.Movement;
using Project.Character.Stats;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives player auto-attack: while a target is selected, walks into
    /// attack range if needed, then attacks on a fixed cooldown using the
    /// player's calculated sub-stats. Damage is applied through
    /// <see cref="Project.Combat.IDamageable"/>, the same contract enemies use.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        private const float CriticalDamageMultiplier = 1.4f;

        [SerializeField] private PlayerStatsController playerStats;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldownSeconds = 1f;

        private float cooldownRemaining;

        private void Update()
        {
            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return;
            }

            if (targetSelector.CurrentDamageable.IsDead)
            {
                targetSelector.ClearTarget();
                return;
            }

            var distanceToTarget = Vector3.Distance(transform.position, targetSelector.CurrentTarget.position);

            if (distanceToTarget > attackRange)
            {
                movementController.SetClickDestination(targetSelector.CurrentTarget.position);
                return;
            }

            movementController.StopMovement();
            transform.forward = (targetSelector.CurrentTarget.position - transform.position).normalized;

            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0f)
            {
                PerformAttack();
                cooldownRemaining = attackCooldownSeconds;
            }
        }

        private void PerformAttack()
        {
            var subStats = playerStats.CurrentSubStats;
            var isCriticalHit = Random.value * 100f < subStats.CriticalRate;
            var damage = isCriticalHit
                ? Mathf.RoundToInt(subStats.StatusAtk * CriticalDamageMultiplier)
                : subStats.StatusAtk;

            targetSelector.CurrentDamageable.TakeDamage(damage);
        }
    }
}