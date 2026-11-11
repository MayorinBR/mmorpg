using UnityEngine;
using Project.Skills;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Heal: restores the caster's health, or, when the selected target is a living Undead in range,
    /// damages it with Holy damage instead, matching Ragnarok Online. Ally and Self both resolve to the caster
    /// until ally selection exists.
    /// </summary>
    public sealed class HealEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                return false;
            }

            var healAmount = skill.CalculateHeal(context.Stats.BaseLevel, context.Stats.GetEffectiveStat(StatType.Intelligence), level, context.Stats.CurrentSubStats.StatusMatk);
            var undeadTarget = GetUndeadTargetInRange(context, skill);

            if (undeadTarget != null)
            {
                var damage = Mathf.Max(1, healAmount / 2);
                undeadTarget.TakeDamage(damage, Element.Holy, DamageCategory.Magical, attacker: context.Transform);
                return true;
            }

            context.OwnHealth.Heal(healAmount);
            return true;
        }

        private static IDamageable GetUndeadTargetInRange(SkillCastContext context, SkillDefinition skill)
        {
            var selector = context.TargetSelector;
            var target = selector.CurrentDamageable;

            if (target == null || target.IsDead || target.Race != MonsterRace.Undead)
            {
                return null;
            }

            return CombatRangeMath.HorizontalDistance(context.Transform.position, selector.CurrentTarget.position) <= skill.Range ? target : null;
        }
    }
}
