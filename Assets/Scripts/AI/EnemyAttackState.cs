using UnityEngine;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// The enemy stands still and attacks its target on a fixed cooldown.
    /// Falls back to <see cref="EnemyChaseState"/> if the target moves out
    /// of attack range.
    /// </summary>
    public class EnemyAttackState : IEnemyState
    {
        private const float AttackCooldownSeconds = 1.5f;

        private float cooldownRemaining;

        /// <inheritdoc />
        public void Enter(EnemyController enemy)
        {
            enemy.Agent.ResetPath();
            cooldownRemaining = 0f;
        }

        /// <inheritdoc />
        public void Tick(EnemyController enemy)
        {
            if (enemy.PlayerTarget == null)
            {
                enemy.ChangeState(new EnemyIdleState());
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.position);

            if (distanceToTarget > enemy.AttackRange)
            {
                enemy.ChangeState(new EnemyChaseState());
                return;
            }

            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0f)
            {
                PerformAttack(enemy);
                cooldownRemaining = AttackCooldownSeconds;
            }
        }

        /// <inheritdoc />
        public void Exit(EnemyController enemy)
        {
        }

        private void PerformAttack(EnemyController enemy)
        {
            var damageable = enemy.PlayerTarget.GetComponent<IDamageable>();

            damageable?.TakeDamage(enemy.Stats.AttackPower);
        }
    }
}