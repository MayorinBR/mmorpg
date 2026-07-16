using UnityEngine;

namespace Project.AI
{
    /// <summary>
    /// The enemy paths toward <see cref="EnemyController.PlayerTarget"/>.
    /// Gives up and returns to <see cref="EnemyIdleState"/> if the target is
    /// lost or if the chase strays beyond <see cref="EnemyController.LeashRange"/>
    /// from the spawn point. Transitions to <see cref="EnemyAttackState"/>
    /// once within attack range.
    /// </summary>
    public class EnemyChaseState : IEnemyState
    {
        /// <inheritdoc />
        public void Enter(EnemyController enemy)
        {
        }

        /// <inheritdoc />
        public void Tick(EnemyController enemy)
        {
            if (enemy.PlayerTarget == null || HasExceededLeashRange(enemy))
            {
                enemy.PlayerTarget = null;
                enemy.ChangeState(new EnemyIdleState());
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.position);

            if (distanceToTarget <= enemy.AttackRange)
            {
                enemy.ChangeState(new EnemyAttackState());
                return;
            }

            enemy.Agent.SetDestination(enemy.PlayerTarget.position);
        }

        /// <inheritdoc />
        public void Exit(EnemyController enemy)
        {
        }

        private bool HasExceededLeashRange(EnemyController enemy)
        {
            var distanceFromSpawn = Vector3.Distance(enemy.transform.position, enemy.SpawnPosition);
            return distanceFromSpawn > enemy.LeashRange;
        }
    }
}