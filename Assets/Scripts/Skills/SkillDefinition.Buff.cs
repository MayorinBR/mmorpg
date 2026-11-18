using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Skills
{
    /// <summary>
    /// Buff and debuff scaling formulas of <see cref="SkillDefinition"/>.
    /// </summary>
    public partial class SkillDefinition
    {
        /// <summary>
        /// Calculates the attack power multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. 0.32 for Provoke's +32%
        /// ATK). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an ATK component.</returns>
        public float GetBuffAtkPercent(int skillLevel)
        {
            return buffAtkPercentBase + buffAtkPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the physical defense multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. -0.55 for Provoke's -55%
        /// DEF). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without a DEF component.</returns>
        public float GetBuffDefPercent(int skillLevel)
        {
            return buffDefPercentBase + buffDefPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat magical defense bonus this buff/debuff skill
        /// grants at the given level. Only meaningful when
        /// <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an MDEF component.</returns>
        public int GetBuffMdefBonus(int skillLevel)
        {
            return Mathf.RoundToInt(buffMdefBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat STR/AGI/VIT/INT/DEX/LUK bonus this
        /// buff/debuff skill grants at the given level (e.g. Blessing's
        /// flat STR/DEX/INT). Only meaningful when <see cref="EffectType"/>
        /// is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero stats for a buff without a stat component.</returns>
        public StatModifiers GetBuffStatBonus(int skillLevel)
        {
            return buffStatBonusBase + buffStatBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the attack speed multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. Increase AGI's ASPD%
        /// component). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an ASPD component.</returns>
        public float GetBuffAspdPercent(int skillLevel)
        {
            return buffAspdPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat Max HP bonus this buff/debuff skill grants
        /// at the given level (e.g. Angelus's flat Max HP component). Only
        /// meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without a Max HP component.</returns>
        public int GetBuffMaxHealthBonus(int skillLevel)
        {
            return buffMaxHealthPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates how long, in seconds, this buff/debuff lasts once
        /// applied at the given level. Only meaningful when
        /// <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated duration in seconds.</returns>
        public float GetBuffDuration(int skillLevel)
        {
            return buffDurationSeconds + buffDurationPerLevel * skillLevel;
        }
    }
}
