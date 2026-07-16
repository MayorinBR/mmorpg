using UnityEngine;

namespace Project.AI
{
    /// <summary>
    /// Default state: the enemy stays in place and scans for a player
    /// entering its aggro range. Transitions to <see cref="EnemyChaseState"/>
    /// as soon as a target is found.
    /// </summary>
    public class EnemyIdleState : IEnemyState
    {
        /// <inheritdoc />
        public void Enter(EnemyController enemy)
        {
            enemy.Agent.ResetPath();
        }

        /// <inheritdoc />
        public void Tick(EnemyController enemy)
        {
            var detectedPlayer = enemy.DetectPlayer();

            if (detectedPlayer != null)
            {
                enemy.PlayerTarget = detectedPlayer;
                enemy.ChangeState(new EnemyChaseState());
            }
        }

        /// <inheritdoc />
        public void Exit(EnemyController enemy)
        {
        }
    }
}