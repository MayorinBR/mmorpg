using System.Collections.Generic;
using UnityEngine;
using Project.Skills;
using Project.Character.Movement;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Casts skills: checks the skill is known, off cooldown, and
    /// affordable, then either damages the current enemy target (in range)
    /// or heals the caster, depending on the skill's effect and target type.
    /// </summary>
    public class PlayerSkillCaster : MonoBehaviour
    {
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent ownHealth;
        [SerializeField] private PlayerTargetSelector targetSelector;

        private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();

        /// <summary>
        /// Attempts to cast the given skill.
        /// </summary>
        /// <param name="skill">The skill to cast.</param>
        /// <returns>True if the skill was successfully cast; false if any requirement wasn't met.</returns>
        public bool TryCastSkill(SkillDefinition skill)
        {
            var level = skillBook.GetLevel(skill);

            if (level <= 0 || Time.time < GetCooldownEndTime(skill))
            {
                return false;
            }

            var cast = skill.EffectType == SkillEffectType.Heal
                ? TryCastHeal(skill)
                : TryCastDamage(skill, level);

            if (cast)
            {
                cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
            }

            return cast;
        }

        /// <summary>
        /// Checks whether the given skill could be cast right now, without
        /// spending any resources or triggering its cooldown. Intended for UI
        /// (hotbar icon state) rather than the actual cast flow.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>The skill's current availability.</returns>
        public SkillAvailability GetAvailability(SkillDefinition skill)
        {
            if (skill == null || skillBook.GetLevel(skill) <= 0)
            {
                return SkillAvailability.NotLearned;
            }

            if (Time.time < GetCooldownEndTime(skill))
            {
                return SkillAvailability.OnCooldown;
            }

            if (mana.CurrentMana < skill.ManaCost)
            {
                return SkillAvailability.InsufficientMana;
            }

            if (skill.EffectType == SkillEffectType.Damage && !HasValidDamageTarget(skill))
            {
                return SkillAvailability.NoValidTarget;
            }

            return SkillAvailability.Ready;
        }

        /// <summary>
        /// Gets the remaining cooldown time for a skill, in seconds.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>Seconds remaining before the skill is off cooldown, or 0 if it's already ready.</returns>
        public float GetCooldownRemaining(SkillDefinition skill)
        {
            return Mathf.Max(0f, GetCooldownEndTime(skill) - Time.time);
        }

        private bool HasValidDamageTarget(SkillDefinition skill)
        {
            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            return Vector3.Distance(transform.position, targetSelector.CurrentTarget.position) <= skill.Range;
        }

        private bool TryCastHeal(SkillDefinition skill)
        {
            // Self and Ally both currently resolve to the caster — ally
            // target selection (targeting another player) isn't implemented yet.
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            ownHealth.Heal(skill.CalculateHeal());
            return true;
        }

        private bool TryCastDamage(SkillDefinition skill, int level)
        {
            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            var distance = Vector3.Distance(transform.position, targetSelector.CurrentTarget.position);

            if (distance > skill.Range || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var subStats = statsController.CurrentSubStats;
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            targetSelector.CurrentDamageable.TakeDamage(damage);
            return true;
        }

        private float GetCooldownEndTime(SkillDefinition skill)
        {
            return cooldownEndTimes.TryGetValue(skill, out var endTime) ? endTime : 0f;
        }
    }
}