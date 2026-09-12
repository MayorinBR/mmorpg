using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// The enemy stands still and attacks its target on a cooldown derived
    /// from its own <see cref="CharacterStatsDefinition.BaseAttackSpeed"/>
    /// via <see cref="AttackSpeedCalculator.GetAttackIntervalSeconds"/> —
    /// the same Aspd-to-delay conversion used for the player, so a
    /// faster-Aspd enemy type genuinely attacks more often instead of every
    /// mob sharing one flat cadence regardless of its stats. Falls back to
    /// <see cref="EnemyChaseState"/> if the target moves out of attack
    /// range. Each attack is resolved against the target's Flee via
    /// <see cref="HitChanceCalculator"/> and can miss outright. Faces the
    /// target every tick while attacking (see <see cref="FaceTarget"/>) —
    /// <see cref="EnemyController.Agent"/>'s own rotation stops updating
    /// once <see cref="Enter"/> resets its path, so without this the enemy
    /// would keep whatever heading it last had while chasing instead of
    /// turning to actually look at the player it's attacking. Base attack
    /// power is scaled by <see cref="EnemyController.Buffs"/>'s
    /// <see cref="Combat.BuffController.AttackMultiplier"/> when a
    /// <see cref="Combat.BuffController"/> is wired — e.g. a landed Provoke
    /// debuff.
    /// </summary>
    public class EnemyAttackState : IEnemyState
    {
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
                enemy.ChangeState(new EnemyWanderState(enemy.transform.position));
                return;
            }

            var distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.position);

            if (distanceToTarget > enemy.AttackRange)
            {
                enemy.ChangeState(new EnemyChaseState());
                return;
            }

            FaceTarget(enemy);

            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0f)
            {
                PerformAttack(enemy);
                cooldownRemaining = AttackSpeedCalculator.GetAttackIntervalSeconds(enemy.Stats.BaseAttackSpeed);
            }
        }

        /// <inheritdoc />
        public void Exit(EnemyController enemy)
        {
        }

        /// <summary>
        /// Rotates the enemy in place to face its target on the horizontal
        /// plane (ignoring height difference, the same way
        /// <c>PlayerCombatController.Update</c> faces the player's own
        /// current target), so an enemy standing still to attack still
        /// visibly looks at whoever it's hitting instead of staying turned
        /// wherever its last movement left it.
        /// </summary>
        private static void FaceTarget(EnemyController enemy)
        {
            var direction = enemy.PlayerTarget.position - enemy.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                enemy.transform.forward = direction.normalized;
            }
        }

        private void PerformAttack(EnemyController enemy)
        {
            // Searched from the hierarchy root rather than directly on
            // enemy.PlayerTarget: the collider Physics.OverlapSphere finds
            // in EnemyController.DetectPlayer can be on any child of the
            // Player GameObject (e.g. its visual/hitbox geometry), while
            // IDamageable (HealthComponent) lives under the separate
            // "Vitals" child. Searching from the root covers both without
            // assuming which sibling holds which.
            var damageable = enemy.PlayerTarget.root.GetComponentInChildren<IDamageable>();

            if (damageable == null)
            {
                return;
            }

            if (!HitChanceCalculator.RollHit(enemy.Stats.Hit, damageable.FleeRating))
            {
                damageable.NotifyDodged();
                return;
            }

            var attackPower = enemy.Buffs != null
                ? Mathf.RoundToInt(enemy.Stats.AttackPower * enemy.Buffs.AttackMultiplier)
                : enemy.Stats.AttackPower;

            damageable.TakeDamage(attackPower);
        }
    }
}