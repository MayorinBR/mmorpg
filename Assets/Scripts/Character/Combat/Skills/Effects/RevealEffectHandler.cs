using System.Collections.Generic;
using UnityEngine;
using Project.Skills;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Reveal skills, such as Sight and Ruwach: reveals every hidden target around the caster and,
    /// when the skill has a damage roll, damages only the targets that were actually hidden.
    /// </summary>
    public sealed class RevealEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (!context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                return false;
            }

            var subStats = context.Stats.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            var hitColliders = Physics.OverlapSphere(context.Transform.position, skill.AreaRadius, context.EnemyLayer);
            var alreadyRevealed = new HashSet<StatusEffectController>();

            foreach (var hitCollider in hitColliders)
            {
                var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();

                if (targetStatus == null || !targetStatus.IsHidden || !alreadyRevealed.Add(targetStatus))
                {
                    continue;
                }

                targetStatus.Reveal();

                var target = hitCollider.GetComponentInParent<IDamageable>();
                var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);

                if (damage <= 0 || target == null || target.IsDead || !HitChanceCalculator.RollHit(accuracy, target.FleeRating))
                {
                    continue;
                }

                damage = WeaponSizeModifiers.Apply(damage, category, context.GetMainHandWeaponSubtype(), target.Size);
                target.TakeDamage(damage, skill.Element, category, attacker: context.Transform);
            }

            return true;
        }
    }
}
