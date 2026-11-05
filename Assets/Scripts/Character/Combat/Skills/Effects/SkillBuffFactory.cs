using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Builds the buff payloads and effect descriptors skills apply, so every handler that applies a buff
    /// builds it the same way.
    /// </summary>
    public static class SkillBuffFactory
    {
        /// <summary>
        /// Builds the payload a Buff skill grants at the given level, from every channel the skill supports.
        /// </summary>
        /// <param name="skill">The buff or debuff skill.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>The payload.</returns>
        public static BuffPayload BuildPayload(SkillDefinition skill, int level)
        {
            var statBonus = skill.GetBuffStatBonus(level);
            return new BuffPayload(
                skill.GetBuffAtkPercent(level),
                skill.GetBuffDefPercent(level),
                skill.GetBuffMdefBonus(level),
                skill.GetBuffAspdPercent(level),
                skill.GetBuffMaxHealthBonus(level),
                statBonus.Strength,
                statBonus.Agility,
                statBonus.Vitality,
                statBonus.Intelligence,
                statBonus.Dexterity,
                statBonus.Luck,
                preventsFlinch: skill.BuffPreventsFlinch);
        }

        /// <summary>
        /// Builds the description the active effects bar shows for a buff or debuff applied by a skill.
        /// </summary>
        /// <param name="skill">The skill that applies the effect.</param>
        /// <param name="category">Whether the effect is a buff or a debuff for whoever receives it.</param>
        /// <param name="idSuffix">Optional suffix, for a second effect of the same skill that must not refresh the first.</param>
        /// <returns>The effect descriptor.</returns>
        public static EffectDescriptor BuildDescriptor(SkillDefinition skill, EffectCategory category, string idSuffix = "")
        {
            return new EffectDescriptor("skill." + skill.name + idSuffix, skill.SkillName, skill.Description, skill.Icon, category);
        }
    }
}
