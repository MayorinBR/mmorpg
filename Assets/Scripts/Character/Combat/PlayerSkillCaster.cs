using System.Collections.Generic;
using UnityEngine;
using Project.Skills;
using Project.Character.Animation;
using Project.Character.Movement;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Casts skills: checks the skill is known, off cooldown, and
    /// affordable, then damages the current enemy target (in range), heals
    /// the caster, or applies a timed buff/debuff (see
    /// <see cref="ownBuffs"/>) to the caster or the current enemy target,
    /// depending on the skill's effect and target type.
    /// </summary>
    public class PlayerSkillCaster : MonoBehaviour
    {
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent ownHealth;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private PlayerAnimatorController animatorController;

        [Tooltip("Optional. Source of the equipped main-hand weapon's subtype, used to scale a Physical damage skill's damage against the target's size (see WeaponSizeModifiers). Left empty, Physical skills deal full damage regardless of target size.")]
        [SerializeField] private EquipmentManager equipment;

        [Tooltip("The caster's own buff modifiers, for a Self-targeted Buff skill (e.g. Endure). Left empty, a Self-targeted Buff skill can't be cast.")]
        [SerializeField] private BuffController ownBuffs;

        [Tooltip("Layer containing enemy colliders, used by an area-of-effect skill (see SkillDefinition.IsAreaOfEffect, e.g. Magnum Break) to find every target within range of the caster. Should be set to the same layer as SkillTargetingController's own enemyLayer.")]
        [SerializeField] private LayerMask enemyLayer;

        private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();

        /// <summary>
        /// Attempts to cast the given skill. For a Damage skill with no
        /// valid current target, this hands off to
        /// <see cref="SkillTargetingController"/> to let the player pick
        /// one (showing a targeting ring, per its own docs) instead of
        /// just failing — so this can return false either because the
        /// skill genuinely can't be cast (not learned, on cooldown) or
        /// because it just started waiting for a target pick.
        /// </summary>
        /// <param name="skill">The skill to cast.</param>
        /// <returns>True if the skill was successfully cast; false if any requirement wasn't met, or a target pick was started instead.</returns>
        public bool TryCastSkill(SkillDefinition skill)
        {
            if (ownHealth.IsDead)
            {
                return false;
            }

            var level = skillBook.GetLevel(skill);

            if (level <= 0 || Time.time < GetCooldownEndTime(skill))
            {
                return false;
            }

            if (skill.EffectType == SkillEffectType.Passive)
            {
                // Passive skills apply their bonus automatically while
                // learned (see PlayerPassiveSkillController) and are
                // never actually cast.
                return false;
            }

            if (NeedsEnemyTarget(skill) && !HasValidDamageTarget(skill))
            {
                SkillTargetingController.Instance?.BeginPicking(skill);
                return false;
            }

            var cast = skill.EffectType switch
            {
                SkillEffectType.Heal => TryCastHeal(skill),
                SkillEffectType.Buff => TryCastBuff(skill, level),
                _ => TryCastDamage(skill, level)
            };

            if (cast)
            {
                animatorController?.TriggerCast();
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

            if (NeedsEnemyTarget(skill) && !HasValidDamageTarget(skill))
            {
                return SkillAvailability.NoValidTarget;
            }

            return SkillAvailability.Ready;
        }

        /// <summary>
        /// Checks whether a skill needs a valid current enemy target before
        /// it can be cast — every Damage skill, plus an Enemy-targeted Buff
        /// skill (e.g. Provoke). A Self-targeted Buff skill (e.g. Endure)
        /// and Heal need no such target.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill requires a targeted enemy in range.</returns>
        private static bool NeedsEnemyTarget(SkillDefinition skill)
        {
            return skill.EffectType == SkillEffectType.Damage
                || (skill.EffectType == SkillEffectType.Buff && skill.TargetType == SkillTargetType.Enemy);
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

        /// <summary>
        /// Checks whether the current combat target is a usable target
        /// for this skill (selected, alive, and within range) — used for
        /// every Damage skill and for an Enemy-targeted Buff skill (see
        /// <see cref="NeedsEnemyTarget"/>), e.g. Provoke. Public so
        /// <see cref="SkillTargetingController"/> can re-check it right
        /// after the player confirms a picked target. An area-of-effect
        /// skill (see <see cref="SkillDefinition.IsAreaOfEffect"/>) needs
        /// no pre-selected target at all, so this is always true for one —
        /// it can never fall into <see cref="SkillTargetingController"/>'s
        /// picking flow.
        /// </summary>
        /// <param name="skill">The skill to check range against.</param>
        /// <returns>True if the current target can be hit by this skill right now.</returns>
        public bool HasValidDamageTarget(SkillDefinition skill)
        {
            if (skill.IsAreaOfEffect)
            {
                return true;
            }

            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            return CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position) <= skill.Range;
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

        /// <summary>
        /// Casts a buff/debuff skill (e.g. Provoke, Endure): applies a
        /// timed stat modifier via <see cref="BuffController"/> to the
        /// caster itself (<see cref="SkillTargetType.Self"/>, using
        /// <see cref="ownBuffs"/>) or to the current enemy target
        /// (<see cref="SkillTargetType.Enemy"/>), found through a
        /// <see cref="BuffController"/> on the same
        /// <see cref="IDamageable"/> hierarchy <see cref="targetSelector"/>
        /// already resolved — the same lookup <see cref="HealthComponent"/>
        /// itself reads from. Spends mana as soon as the cast is committed,
        /// the same as a Damage skill; fails without spending anything if
        /// the resolved target has no <see cref="BuffController"/> wired.
        /// </summary>
        /// <param name="skill">The buff/debuff skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True if the buff was applied.</returns>
        private bool TryCastBuff(SkillDefinition skill, int level)
        {
            var buffs = skill.TargetType == SkillTargetType.Self
                ? ownBuffs
                : targetSelector.CurrentTarget?.GetComponentInParent<BuffController>();

            if (buffs == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            buffs.ApplyBuff(skill.GetBuffAtkPercent(level), skill.GetBuffDefPercent(level), skill.GetBuffMdefBonus(level), skill.GetBuffDuration(level));
            return true;
        }

        /// <summary>
        /// Spends mana and puts the skill on cooldown as soon as the cast is
        /// committed (in range, affordable), regardless of whether the hit
        /// actually lands — matching Ragnarok Online, where a missed skill
        /// still consumes its resources. Delegates to
        /// <see cref="TryCastAreaDamage"/> for an area-of-effect skill (see
        /// <see cref="SkillDefinition.IsAreaOfEffect"/>), since that needs
        /// no single pre-selected target at all. Otherwise the hit is
        /// resolved against the target's Flee via
        /// <see cref="HitChanceCalculator"/>, applying damage as
        /// <see cref="DamageCategory.Physical"/> or
        /// <see cref="DamageCategory.Magical"/> depending on the skill's
        /// <see cref="SkillDefinition.DamageType"/> — a Physical skill is
        /// also scaled by <see cref="WeaponSizeModifiers"/> for the
        /// equipped weapon against the target's size, the same as a basic
        /// attack.
        /// </summary>
        private bool TryCastDamage(SkillDefinition skill, int level)
        {
            if (skill.IsAreaOfEffect)
            {
                return TryCastAreaDamage(skill, level);
            }

            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            var distance = CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position);

            if (distance > skill.Range || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var target = targetSelector.CurrentDamageable;
            var subStats = statsController.CurrentSubStats;

            if (HitChanceCalculator.RollHit(subStats.Hit + skill.GetAccuracyBonus(level), target.FleeRating))
            {
                var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
                var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
                damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                target.TakeDamage(damage, skill.Element, category, attacker: transform);
            }
            else
            {
                target.NotifyDodged();
            }

            return true;
        }

        /// <summary>
        /// Casts an area-of-effect damage skill (e.g. Magnum Break): spends
        /// mana as soon as the cast is committed, the same as
        /// <see cref="TryCastDamage"/>, then rolls a separate hit check
        /// against every distinct, living <see cref="IDamageable"/> found
        /// within <see cref="SkillDefinition.AreaRadius"/> of the caster's
        /// own position via <see cref="enemyLayer"/> — unlike a
        /// single-target skill, this needs no pre-selected target and
        /// can't fail for lack of one, matching how the real Magnum Break
        /// always fires (and consumes its cost) whether or not anything
        /// was actually standing in range.
        /// </summary>
        /// <param name="skill">The area-of-effect skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True once the cast is committed (mana spent), regardless of how many targets were hit.</returns>
        private bool TryCastAreaDamage(SkillDefinition skill, int level)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var subStats = statsController.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            var hitColliders = Physics.OverlapSphere(transform.position, skill.AreaRadius, enemyLayer);
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
                    var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
                    damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                    target.TakeDamage(damage, skill.Element, category, attacker: transform);
                }
                else
                {
                    target.NotifyDodged();
                }
            }

            return true;
        }

        private float GetCooldownEndTime(SkillDefinition skill)
        {
            return cooldownEndTimes.TryGetValue(skill, out var endTime) ? endTime : 0f;
        }

        /// <summary>
        /// Gets the equipped main-hand weapon's subtype, or
        /// <see cref="WeaponSubtype.Unarmed"/> if <see cref="equipment"/>
        /// isn't wired — <see cref="WeaponSizeModifiers"/> already treats
        /// Unarmed as no size penalty, so this degrades safely.
        /// </summary>
        private WeaponSubtype GetMainHandWeaponSubtype()
        {
            return equipment != null ? equipment.GetMainHandWeaponSubtype() : WeaponSubtype.Unarmed;
        }
    }
}