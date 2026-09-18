using System;
using System.Linq;
using UnityEngine;
using Project.Character.Stats;
using Project.Items;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Sums the flat Status ATK bonus granted by every learned passive
    /// skill (<see cref="SkillEffectType.Passive"/>) whose weapon
    /// requirement, if any, matches the currently equipped main-hand
    /// weapon — e.g. Sword Mastery, Two-Handed Sword Mastery. Queried on
    /// demand by <see cref="PlayerStatsController"/> rather than cached,
    /// the same on-demand approach <see cref="PlayerStatsController.CurrentSubStats"/>
    /// itself already uses for equipment, so a skill level-up or a
    /// weapon swap is reflected immediately without either side needing
    /// to notify this component.
    /// </summary>
    public class PlayerPassiveSkillController : MonoBehaviour
    {
        [SerializeField] private PlayerSkillBook skillBook;

        /// <summary>Gets the skill book this controller reads learned passives from.</summary>
        public PlayerSkillBook SkillBook => skillBook;

        /// <summary>
        /// Calculates the total flat Status ATK bonus from every learned
        /// passive skill that applies to the given equipped weapon subtype.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined attack bonus. Zero if no applicable passive skill is learned.</returns>
        public int GetAttackBonus(WeaponSubtype equippedWeaponSubtype) =>
            SumPassiveBonus(equippedWeaponSubtype, (skill, level) => skill.GetPassiveAttackBonus(level));

        /// <summary>
        /// Calculates the total flat DEX bonus from every learned passive
        /// skill that applies to the given equipped weapon subtype — e.g.
        /// Owl's Eye.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined DEX bonus. Zero if no applicable passive skill is learned.</returns>
        public int GetDexBonus(WeaponSubtype equippedWeaponSubtype) =>
            SumPassiveBonus(equippedWeaponSubtype, (skill, level) => skill.GetPassiveDexBonus(level));

        /// <summary>
        /// Calculates the total flat Hit bonus from every learned passive
        /// skill that applies to the given equipped weapon subtype — e.g.
        /// Vulture's Eye.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined Hit bonus. Zero if no applicable passive skill is learned.</returns>
        public int GetHitBonus(WeaponSubtype equippedWeaponSubtype) =>
            SumPassiveBonus(equippedWeaponSubtype, (skill, level) => skill.GetPassiveHitBonus(level));

        /// <summary>
        /// Calculates the total flat Flee bonus from every learned passive
        /// skill — e.g. Improve Dodge, which unlike the others here isn't
        /// weapon-gated at all, so it always matches regardless of
        /// <paramref name="equippedWeaponSubtype"/>.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined Flee bonus. Zero if no applicable passive skill is learned.</returns>
        public int GetFleeBonus(WeaponSubtype equippedWeaponSubtype) =>
            SumPassiveBonus(equippedWeaponSubtype, (skill, level) => skill.GetPassiveFleeBonus(level));

        /// <summary>
        /// Calculates the total flat attack range bonus, in meters, from
        /// every learned passive skill that applies to the given equipped
        /// weapon subtype — e.g. Vulture's Eye.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined range bonus. Zero if no applicable passive skill is learned.</returns>
        public float GetRangeBonus(WeaponSubtype equippedWeaponSubtype)
        {
            var total = 0f;

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType != SkillEffectType.Passive || level <= 0)
                {
                    continue;
                }

                if (!MatchesWeaponRequirement(skill, equippedWeaponSubtype))
                {
                    continue;
                }

                total += skill.GetPassiveRangeBonus(level);
            }

            return total;
        }

        /// <summary>
        /// Calculates the total flat physical damage bonus from every
        /// learned "race bane" passive skill (e.g. Demon Bane) against the
        /// given target's race. Never weapon-gated, matching real
        /// Ragnarok Online.
        /// </summary>
        /// <param name="targetRace">The race of the target being hit.</param>
        /// <returns>The combined bonus, or zero if the target isn't Demon/Undead or no such passive is learned.</returns>
        public int GetRaceDamageBonus(MonsterRace targetRace)
        {
            if (targetRace != MonsterRace.Demon && targetRace != MonsterRace.Undead)
            {
                return 0;
            }

            var total = 0;

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType == SkillEffectType.Passive && level > 0)
                {
                    total += skill.GetPassiveRaceDamageBonus(level);
                }
            }

            return total;
        }

        /// <summary>
        /// Calculates the total flat physical defense bonus from every
        /// learned "race bane resistance" passive skill (e.g. Divine
        /// Protection) against the given attacker's race. Never
        /// weapon-gated, matching real Ragnarok Online.
        /// </summary>
        /// <param name="attackerRace">The race of the attacking entity.</param>
        /// <returns>The combined bonus, or zero if the attacker isn't Demon/Undead or no such passive is learned.</returns>
        public int GetRaceDefenseBonus(MonsterRace attackerRace)
        {
            if (attackerRace != MonsterRace.Demon && attackerRace != MonsterRace.Undead)
            {
                return 0;
            }

            var total = 0;

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType == SkillEffectType.Passive && level > 0)
                {
                    total += skill.GetPassiveRaceDefenseBonus(level);
                }
            }

            return total;
        }

        private int SumPassiveBonus(WeaponSubtype equippedWeaponSubtype, Func<SkillDefinition, int, int> getBonus)
        {
            var total = 0;

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType != SkillEffectType.Passive || level <= 0)
                {
                    continue;
                }

                if (!MatchesWeaponRequirement(skill, equippedWeaponSubtype))
                {
                    continue;
                }

                total += getBonus(skill, level);
            }

            return total;
        }

        private static bool MatchesWeaponRequirement(SkillDefinition skill, WeaponSubtype equippedWeaponSubtype)
        {
            var requiredSubtypes = skill.PassiveRequiredWeaponSubtypes;
            return requiredSubtypes.Count == 0 || requiredSubtypes.Contains(equippedWeaponSubtype);
        }

        /// <summary>
        /// Calculates the combined multiplier to apply to natural HP regen
        /// (see <see cref="PlayerHealthRegenController"/>) from every
        /// learned passive with a regen bonus — e.g. Increase HP Recovery.
        /// </summary>
        /// <returns>The combined multiplier, where 1 means no bonus.</returns>
        public float GetRegenMultiplier()
        {
            var total = 1f;

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType == SkillEffectType.Passive && level > 0)
                {
                    total += skill.GetPassiveRegenBonus(level);
                }
            }

            return total;
        }

        /// <summary>
        /// Checks whether any learned passive removes the reduced-regen-
        /// while-moving penalty — e.g. HP Recovery While Moving.
        /// </summary>
        /// <returns>True if such a passive is learned, at any level.</returns>
        public bool IgnoresMovementRegenPenalty()
        {
            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;
                var level = entry.Value;

                if (skill.EffectType == SkillEffectType.Passive && level > 0 && skill.PassiveRemovesMovementRegenPenalty)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
