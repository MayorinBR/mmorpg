namespace Project.Character.Stats
{
    /// <summary>
    /// Calculates the maximum carry weight following the Ragnarok Online
    /// formula (source: iRO Wiki, Weight Limit): 2000 + 30 * base STR + a
    /// job modifier. Skill and item modifiers are applied separately by
    /// the inventory.
    /// </summary>
    public static class WeightCapacityCalculator
    {
        private const float BaseWeight = 2000f;
        private const float WeightPerStrength = 30f;

        /// <summary>
        /// Calculates the unscaled maximum carry weight.
        /// </summary>
        /// <param name="baseStrength">The character's base STR, without equipment or buffs.</param>
        /// <param name="jobModifier">The class modifier, see <see cref="GetJobModifier"/>.</param>
        /// <returns>The maximum carry weight in Ragnarok Online units.</returns>
        public static float Calculate(int baseStrength, float jobModifier)
        {
            return BaseWeight + WeightPerStrength * baseStrength + jobModifier;
        }

        /// <summary>
        /// Gets the carry weight bonus granted by a class.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The class modifier in Ragnarok Online units.</returns>
        public static float GetJobModifier(CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Swordman:
                case CharacterClass.Merchant:
                    return 800f;
                case CharacterClass.Archer:
                    return 600f;
                case CharacterClass.Acolyte:
                case CharacterClass.Thief:
                    return 400f;
                case CharacterClass.Mage:
                    return 200f;
                default:
                    return 0f;
            }
        }
    }
}
