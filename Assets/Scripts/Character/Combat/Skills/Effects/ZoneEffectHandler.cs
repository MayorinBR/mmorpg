using UnityEngine;
using Project.Skills;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Zone skills, such as Fire Wall and Safety Wall, by spawning a persistent zone at a ground
    /// position the player picked. A zone either blocks attacks of one range kind or damages every enemy
    /// standing inside it on its own tick. Damage and accuracy are fixed at cast time.
    /// Not an <see cref="ISkillEffectHandler"/>, because it needs the picked position.
    /// </summary>
    public sealed class ZoneEffectHandler
    {
        /// <summary>
        /// Spends mana and spawns the zone.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The zone skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="position">The world position the zone spawns at.</param>
        /// <returns>True if the zone was spawned.</returns>
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level, Vector3 position)
        {
            if (!context.Mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var zone = SkillZoneController.Spawn(skill.ZonePrefab, position, skill.AreaRadius, skill.GetZoneDuration(level), skill.ZoneTickIntervalSeconds);

            if (skill.BlocksAttacks)
            {
                zone.InitializeBlock(skill.BlockedWeaponType == WeaponType.Melee ? AttackRangeKind.Melee : AttackRangeKind.Ranged);
                return true;
            }

            var subStats = context.Stats.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            zone.Initialize(context.EnemyLayer, accuracy, damage, skill.Element, category, context.Transform, skill.GetZoneMaxHits(level));
            return true;
        }
    }
}
