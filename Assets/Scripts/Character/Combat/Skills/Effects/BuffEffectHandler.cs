using System.Collections.Generic;
using UnityEngine;
using Project.Skills;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Buff skills: a buff on the caster (Self and Ally), a resistible debuff on the selected enemy
    /// (such as Provoke), or a debuff on every Demon and Undead around the caster (such as Signum Crucis).
    /// </summary>
    public sealed class BuffEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (skill.TargetType == SkillTargetType.AreaAroundCaster)
            {
                return TryCastAreaDebuff(context, skill, level);
            }

            // Ally targeting is not implemented yet: an Ally buff resolves to the caster, same as Self.
            var isSelfOrAlly = skill.TargetType == SkillTargetType.Self || skill.TargetType == SkillTargetType.Ally;
            var enemy = context.TargetSelector.CurrentTarget;
            var buffs = isSelfOrAlly ? context.OwnBuffs : enemy?.GetComponentInParent<BuffController>();

            if (buffs == null)
            {
                CombatLog.Write($"{skill.SkillName}: no valid target.");
                return false;
            }

            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                CombatLog.Write($"{skill.SkillName}: not enough mana ({skill.GetManaCost(level)} needed).");
                return false;
            }

            if (!isSelfOrAlly && skill.HasCustomVfx)
            {
                context.Visuals.PlayCustomVfx(skill, enemy);
            }

            if (!isSelfOrAlly && IsResisted(skill, level, enemy))
            {
                // Mana and cooldown are still spent on a resist, same as a missed damage skill.
                CombatLog.Write($"{skill.SkillName} was resisted by {enemy.root.name}.");
                return true;
            }

            var category = isSelfOrAlly ? EffectCategory.Buff : EffectCategory.Debuff;
            buffs.ApplyBuff(SkillBuffFactory.BuildPayload(skill, level), skill.GetBuffDuration(level), SkillBuffFactory.BuildDescriptor(skill, category), skill.BuffFlinchHitLimit);

            CombatLog.Write(isSelfOrAlly
                ? $"{skill.SkillName} lv{level} applied to the caster for {skill.GetBuffDuration(level):0.#}s."
                : $"{skill.SkillName} lv{level} landed on {enemy.root.name} for {skill.GetBuffDuration(level):0.#}s.");

            if (!isSelfOrAlly)
            {
                enemy.GetComponentInParent<HealthComponent>()?.Provoke(context.Transform);
            }

            if (isSelfOrAlly && skill.BuffRevealsHidden)
            {
                RevealHiddenAround(context, skill.AreaRadius);
            }

            return true;
        }

        /// <summary>
        /// Rolls whether an enemy-targeted debuff is resisted: Undead and Boss targets are immune when the skill
        /// says so, and a skill with a success chance can fail. A skill with no success chance always lands.
        /// </summary>
        private static bool IsResisted(SkillDefinition skill, int level, Transform enemy)
        {
            if (skill.ImmuneToUndeadAndBoss && IsUndeadOrBoss(enemy))
            {
                return true;
            }

            var successChance = skill.GetSuccessChance(level);
            return successChance > 0f && Random.value >= successChance;
        }

        private static bool IsUndeadOrBoss(Transform target)
        {
            var stats = target != null ? target.GetComponentInParent<CharacterStatsHolder>()?.Stats : null;
            return stats != null && (stats.IsBoss || stats.Race == MonsterRace.Undead);
        }

        private static void RevealHiddenAround(SkillCastContext context, float radius)
        {
            foreach (var hitCollider in Physics.OverlapSphere(context.Transform.position, radius, context.EnemyLayer))
            {
                var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();

                if (targetStatus != null && targetStatus.IsHidden)
                {
                    targetStatus.Reveal();
                }
            }
        }

        /// <summary>
        /// Rolls the debuff separately against every distinct, living Demon or Undead within the skill's area
        /// radius of the caster.
        /// </summary>
        private static bool TryCastAreaDebuff(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                return false;
            }

            var payload = SkillBuffFactory.BuildPayload(skill, level);
            var duration = skill.GetBuffDuration(level);
            var successChance = skill.GetSuccessChance(level);
            var hitColliders = Physics.OverlapSphere(context.Transform.position, skill.AreaRadius, context.EnemyLayer);
            var alreadyHit = new HashSet<BuffController>();

            foreach (var hitCollider in hitColliders)
            {
                var target = hitCollider.GetComponentInParent<IDamageable>();

                if (target == null || target.IsDead || (target.Race != MonsterRace.Demon && target.Race != MonsterRace.Undead))
                {
                    continue;
                }

                var targetBuffs = hitCollider.GetComponentInParent<BuffController>();

                if (targetBuffs == null || !alreadyHit.Add(targetBuffs))
                {
                    continue;
                }

                if (successChance > 0f && Random.value >= successChance)
                {
                    continue;
                }

                targetBuffs.ApplyBuff(payload, duration, SkillBuffFactory.BuildDescriptor(skill, EffectCategory.Debuff));
            }

            return true;
        }
    }
}
