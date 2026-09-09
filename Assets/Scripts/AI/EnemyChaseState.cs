using UnityEngine;

namespace Project.AI
{
    /// <summary>
    /// The enemy paths toward <see cref="EnemyController.PlayerTarget"/>,
    /// free to follow it anywhere on the map. Gives up only when the target
    /// is lost, or gets farther than <see cref="EnemyController.ChaseGiveUpRange"/>
    /// from the enemy's own current position AND hasn't landed a hit on this
    /// enemy within <see cref="EnemyController.ChaseGiveUpGraceSeconds"/> —
    /// the second condition keeps a ranged or spell attacker sniping from
    /// beyond that range from being hit for free: as long as their attacks
    /// keep landing, the enemy keeps trying to close the distance instead of
    /// re-engaging and immediately giving up again every tick. Giving up
    /// switches to <see cref="EnemyWanderState"/> centered on wherever the
    /// chase ended — while still remembering the target via
    /// <see cref="EnemyController.RememberedAggressor"/> for a later
    /// re-engage. Transitions to <see cref="EnemyAttackState"/> once within
    /// attack range.
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
            if (enemy.PlayerTarget == null)
            {
                enemy.ChangeState(new EnemyWanderState(enemy.transform.position));
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.position);
            var timeSinceLastHit = Time.time - enemy.LastAttackedTime;

            if (distanceToTarget > enemy.ChaseGiveUpRange && timeSinceLastHit > enemy.ChaseGiveUpGraceSeconds)
            {
                enemy.PlayerTarget = null;
                enemy.ChangeState(new EnemyWanderState(enemy.transform.position));
                return;
            }

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
    }
}
