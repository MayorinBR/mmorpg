using System.Linq;
using UnityEngine;
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

        /// <summary>
        /// Calculates the total flat Status ATK bonus from every learned
        /// passive skill that applies to the given equipped weapon subtype.
        /// </summary>
        /// <param name="equippedWeaponSubtype">The main-hand weapon's current subtype, or <see cref="WeaponSubtype.Unarmed"/> if none equipped.</param>
        /// <returns>The combined attack bonus. Zero if no applicable passive skill is learned.</returns>
        public int GetAttackBonus(WeaponSubtype equippedWeaponSubtype)
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

                total += skill.GetPassiveAttackBonus(level);
            }

            return total;
        }

        private static bool MatchesWeaponRequirement(SkillDefinition skill, WeaponSubtype equippedWeaponSubtype)
        {
            var requiredSubtypes = skill.PassiveRequiredWeaponSubtypes;
            return requiredSubtypes.Count == 0 || requiredSubtypes.Contains(equippedWeaponSubtype);
        }
    }
}
