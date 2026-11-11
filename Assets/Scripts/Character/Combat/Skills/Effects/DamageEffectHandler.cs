using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves damaging skills: a single target in range, a burst around the selected target, or a burst
    /// around the caster that needs no target at all. Mana, Zeny and health are spent as soon as the cast is
    /// committed, whether or not the hit lands, matching Ragnarok Online.
    /// </summary>
    public sealed class DamageEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (skill.IsAreaOfEffect && skill.TargetType == SkillTargetType.AreaAroundCaster)
            {
                return TryCastAreaAroundCaster(context, skill, level);
            }

            var selector = context.TargetSelector;

            if (selector.CurrentTarget == null || selector.CurrentDamageable == null)
            {
                CombatLog.Write($"{skill.SkillName}: not cast, the target was lost.");
                return false;
            }

            if (skill.ZenyCost > 0 && (context.Currency == null || context.Currency.CurrentZeny < skill.ZenyCost))
            {
                // Checked before the mana is spent, so a skill that cannot be afforded never partially spends.
                CombatLog.Write($"{skill.SkillName}: not cast, not enough Zeny ({skill.ZenyCost} needed).");
                return false;
            }

            var distance = CombatRangeMath.HorizontalDistance(context.Transform.position, selector.CurrentTarget.position);

            var range = context.GetEffectiveRange(skill);

            if (distance > range)
            {
                CombatLog.Write($"{skill.SkillName}: not cast, the target is out of range ({distance:0.#} m, {range:0.#} max).");
                return false;
            }

            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                CombatLog.Write($"{skill.SkillName}: not cast, not enough mana ({skill.GetManaCost(level)} needed).");
                return false;
            }

            if (skill.ZenyCost > 0)
            {
                context.Currency.TrySpend(skill.ZenyCost);
            }

            context.Visuals.PlayProjectileVisual(skill, selector.CurrentTarget);

            if (skill.IsAreaOfEffect)
            {
                var areaCenter = selector.CurrentTarget.position;
                context.RunAfterImpactDelay(skill, () => SkillDamageResolver.ApplyAreaDamage(context, skill, level, areaCenter));
                return true;
            }

            var target = selector.CurrentDamageable;
            var targetTransform = selector.CurrentTarget;
            context.RunAfterImpactDelay(skill, () => SkillDamageResolver.ResolveSingleTargetHit(context, skill, level, target, targetTransform));
            return true;
        }

        private static bool TryCastAreaAroundCaster(SkillCastContext context, SkillDefinition skill, int level)
        {
            var healthCost = skill.GetHealthCost(level);

            if (healthCost > 0 && context.OwnHealth != null && context.OwnHealth.CurrentHealth <= healthCost)
            {
                CombatLog.Write($"{skill.SkillName}: not cast, not enough health ({healthCost} needed).");
                return false;
            }

            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                CombatLog.Write($"{skill.SkillName}: not cast, not enough mana ({skill.GetManaCost(level)} needed).");
                return false;
            }

            if (healthCost > 0 && context.OwnHealth != null)
            {
                context.OwnHealth.TrySpendHealth(healthCost);
            }

            context.RunAfterImpactDelay(skill, () =>
            {
                SkillDamageResolver.ApplyAreaDamage(context, skill, level, context.Transform.position);
                ApplyFireDamageBuff(context, skill, level);
            });

            return true;
        }

        /// <summary>
        /// Gives the caster the skill's after-effect of extra Fire damage on every attack, such as Magnum
        /// Break's. Does nothing for a skill without one.
        /// </summary>
        private static void ApplyFireDamageBuff(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (skill.SelfFireDamageBonusPercent <= 0f || context.OwnBuffs == null)
            {
                return;
            }

            context.OwnBuffs.ApplyBuff(
                new BuffPayload(fireDamagePercent: skill.SelfFireDamageBonusPercent),
                skill.GetBuffDuration(level),
                SkillBuffFactory.BuildDescriptor(skill, EffectCategory.Buff, ".afterEffect"));
        }
    }
}
