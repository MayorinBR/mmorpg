using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Project.Skills;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Rolls hits and applies a damaging skill's damage, knockback and status procs to its targets.
    /// Shared by every handler that damages: single target, area and any future damaging effect.
    /// </summary>
    public static class SkillDamageResolver
    {
        /// <summary>
        /// Writes the skill's base damage to the combat log, before any target-specific modifier (race, size,
        /// element, flee). Called when the cast is committed, so the value is logged even if the hit later
        /// misses or no enemy is inside the area.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill that was cast.</param>
        /// <param name="level">The skill's current level.</param>
        public static void LogBaseDamage(SkillCastContext context, SkillDefinition skill, int level)
        {
            var subStats = context.Stats.CurrentSubStats;
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            CombatLog.Write($"{skill.SkillName} lv{level}: base damage {damage} ({skill.Element}).");
        }

        /// <summary>
        /// Rolls the hit against a single target and applies the skill's damage, knockback and status procs.
        /// Skipped if the target was destroyed or died while the impact delay was running.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill that was cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="target">The damageable target selected at cast time.</param>
        /// <param name="targetTransform">The transform of that target.</param>
        public static void ResolveSingleTargetHit(SkillCastContext context, SkillDefinition skill, int level, IDamageable target, Transform targetTransform)
        {
            if (targetTransform == null || target == null || target.IsDead)
            {
                return;
            }

            var subStats = context.Stats.CurrentSubStats;

            if (HitChanceCalculator.RollHit(subStats.Hit + skill.GetAccuracyBonus(level), target.FleeRating))
            {
                ApplyHit(context, skill, level, target, targetTransform);
            }
            else
            {
                target.NotifyDodged();
            }
        }

        /// <summary>
        /// Rolls a separate hit check against every distinct, living target found within the skill's area
        /// radius of the center.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The area-of-effect skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="center">The world position the burst is centered on.</param>
        public static void ApplyAreaDamage(SkillCastContext context, SkillDefinition skill, int level, Vector3 center)
        {
            var subStats = context.Stats.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var hitColliders = Physics.OverlapSphere(center, skill.AreaRadius, context.EnemyLayer);
            var alreadyHit = new HashSet<IDamageable>();

            foreach (var hitCollider in hitColliders)
            {
                var target = hitCollider.GetComponentInParent<IDamageable>();

                if (target == null || target.IsDead || !alreadyHit.Add(target))
                {
                    continue;
                }

                if (HitChanceCalculator.RollHit(accuracy, target.FleeRating))
                {
                    ApplyHit(context, skill, level, target, hitCollider);
                }
                else
                {
                    target.NotifyDodged();
                }
            }

            CombatLog.Write($"{skill.SkillName}: {alreadyHit.Count} enemy(ies) inside the area.");
        }

        private static void ApplyHit(SkillCastContext context, SkillDefinition skill, int level, IDamageable target, Component targetComponent)
        {
            var subStats = context.Stats.CurrentSubStats;
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            damage = Mathf.RoundToInt(damage * skill.GetRaceDamageMultiplier(target.Race));
            damage = WeaponSizeModifiers.Apply(damage, category, context.GetMainHandWeaponSubtype(), target.Size);
            damage += context.GetRaceDamageBonus(category, target.Race);
            target.TakeDamage(damage, skill.Element, category, attacker: context.Transform);
            ElementalBonusDamage.Apply(target, damage, context.Stats.FireDamagePercent, Element.Fire, context.Transform);

            var targetStatus = targetComponent.GetComponentInParent<StatusEffectController>();
            TryProcStunAugment(context, skill, level, targetStatus);
            TryProcInflictedStatus(skill, level, targetStatus);

            if (skill.KnockbackDistance > 0f)
            {
                KnockbackUtility.Slide(targetComponent.GetComponentInParent<NavMeshAgent>(), context.Transform.position, skill.KnockbackDistance);
            }
        }

        /// <summary>
        /// Rolls every learned passive that augments the cast skill with a stun chance and applies a stun to the
        /// target on a successful proc. Does nothing if the target has no status effects.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="castSkill">The skill that was just cast and landed a hit.</param>
        /// <param name="castSkillLevel">The cast skill's current level.</param>
        /// <param name="targetStatus">The hit target's status effects, or null.</param>
        private static void TryProcStunAugment(SkillCastContext context, SkillDefinition castSkill, int castSkillLevel, StatusEffectController targetStatus)
        {
            if (targetStatus == null)
            {
                return;
            }

            foreach (var entry in context.SkillBook.LearnedSkills)
            {
                var passive = entry.Key;

                if (passive.EffectType != SkillEffectType.Passive || entry.Value <= 0 || passive.AugmentsSkill != castSkill)
                {
                    continue;
                }

                if (Random.value < passive.GetStunChance(castSkillLevel))
                {
                    targetStatus.ApplyStun(passive.StunDurationSeconds);
                }
            }
        }

        /// <summary>
        /// Rolls the cast skill's own inflicted status and applies it to the target on a successful proc.
        /// </summary>
        /// <param name="skill">The skill that was just cast and landed a hit.</param>
        /// <param name="level">The cast skill's current level.</param>
        /// <param name="targetStatus">The hit target's status effects, or null.</param>
        private static void TryProcInflictedStatus(SkillDefinition skill, int level, StatusEffectController targetStatus)
        {
            if (targetStatus == null || skill.InflictedStatus == StatusEffectType.None)
            {
                return;
            }

            if (Random.value < skill.GetSuccessChance(level))
            {
                targetStatus.Apply(skill.InflictedStatus, skill.GetInflictedStatusDuration(level), skill.GetPoisonDamagePerTick(level));
            }
        }
    }
}
