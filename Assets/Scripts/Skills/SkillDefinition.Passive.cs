using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Skills
{
    /// <summary>
    /// Passive-skill scaling formulas of <see cref="SkillDefinition"/>.
    /// </summary>
    public partial class SkillDefinition
    {
        /// <summary>
        /// Calculates the flat Status ATK bonus this passive skill grants
        /// at the given level, before <see cref="PassiveRequiredWeaponSubtypes"/>
        /// is checked against the equipped weapon. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated attack bonus, zero for passives without one.</returns>
        public int GetPassiveAttackBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveAttackBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat DEX bonus this passive skill grants at the
        /// given level, before <see cref="PassiveRequiredWeaponSubtypes"/>
        /// is checked against the equipped weapon. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated DEX bonus, zero for passives without one.</returns>
        public int GetPassiveDexBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveDexBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat Hit rating bonus this passive skill grants
        /// at the given level, before
        /// <see cref="PassiveRequiredWeaponSubtypes"/> is checked against
        /// the equipped weapon. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated Hit bonus, zero for passives without one.</returns>
        public int GetPassiveHitBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveHitBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat attack range bonus, in meters, this passive
        /// skill grants at the given level, before
        /// <see cref="PassiveRequiredWeaponSubtypes"/> is checked against
        /// the equipped weapon. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated range bonus, zero for passives without one.</returns>
        public float GetPassiveRangeBonus(int skillLevel)
        {
            return passiveRangeBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat Flee rating bonus this passive skill grants
        /// at the given level. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated Flee bonus, zero for passives without one.</returns>
        public int GetPassiveFleeBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveFleeBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Gets the chance, from 0 to 1, that a basic attack deals double damage at the given skill level. Only meaningful
        /// for a passive such as Double Attack.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The double damage chance.</returns>
        public float GetPassiveDoubleAttackChance(int skillLevel)
        {
            return Mathf.Clamp01(passiveDoubleAttackChancePerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat max carry weight bonus this passive skill
        /// grants at the given level (e.g. Enlarge Weight Limit). Never
        /// weapon-gated. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus, zero for passives without one.</returns>
        public float GetPassiveWeightLimitBonus(int skillLevel)
        {
            return passiveWeightLimitBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat physical damage bonus this passive skill
        /// grants at the given level against Demon/Undead race targets
        /// (e.g. Demon Bane). Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated damage bonus, zero for passives without one.</returns>
        public int GetPassiveRaceDamageBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveRaceDamageBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat physical defense bonus this passive skill
        /// grants at the given level against Demon/Undead race attackers
        /// (e.g. Divine Protection). Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated defense bonus, zero for passives without one.</returns>
        public int GetPassiveRaceDefenseBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveRaceDefenseBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the HP this passive restores on each skill-regen tick
        /// at the given level: a flat amount plus a fraction of max HP, both
        /// scaling with level (e.g. Increase HP Recovery). Summed across
        /// every learned regen passive by the player's passive-skill
        /// controller. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <param name="maxHealth">The player's current max HP.</param>
        /// <returns>The HP restored per tick. Zero for a passive without a regen component.</returns>
        public float GetPassiveRegenAmount(int skillLevel, int maxHealth)
        {
            return (passiveRegenFlatPerLevel + passiveRegenPercentOfMaxHealthPerLevel * maxHealth) * skillLevel;
        }

        /// <summary>
        /// Calculates this passive's bonus to the HP restored by healing
        /// items at the given level (e.g. 0.3 for Increase HP Recovery at
        /// level 3, a +30% bonus). Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a passive without one.</returns>
        public float GetPassiveHealItemBonus(int skillLevel)
        {
            return passiveHealItemBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates this passive's bonus to natural SP regen at the given
        /// level (e.g. 0.4 for Increase SP Recovery at level 2, a +40%
        /// bonus) applied to the natural SP regen tick. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a passive without an SP regen component.</returns>
        public float GetPassiveSpRegenBonus(int skillLevel)
        {
            return passiveSpRegenMultiplierPerLevel * skillLevel;
        }
    }
}
