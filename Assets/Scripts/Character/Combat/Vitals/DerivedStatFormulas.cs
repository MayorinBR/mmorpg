using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Pure formulas that turn an effective base stat into a derived value, using the real Ragnarok Online rules.
    /// Kept free of component state so they can be unit tested directly.
    /// </summary>
    public static class DerivedStatFormulas
    {
        // iRO Wiki Classic, Stats (consulted September 2026): max HP and SP grow 1% per point of VIT and INT.
        private const float MaxHealthBonusPerVit = 0.01f;
        private const float MaxManaBonusPerInt = 0.01f;
        private const float CastTimeReductionDivisor = 530f;

        // ragnarokjobguides.blogspot.com "How stats affect statuses" (consulted September 2026): VIT + 3 percent
        // chance to resist a status effect. Applied to every status type as a deliberate simplification.
        private const int VitStatusResistOffset = 3;

        /// <summary>Calculates the extra max HP that VIT grants.</summary>
        /// <param name="baseMaxHealth">The max health before the VIT bonus.</param>
        /// <param name="vitality">The effective VIT.</param>
        /// <returns>The bonus, rounded to the nearest whole point.</returns>
        public static int MaxHealthBonus(int baseMaxHealth, int vitality)
        {
            return Mathf.RoundToInt(baseMaxHealth * vitality * MaxHealthBonusPerVit);
        }

        /// <summary>Calculates the extra max SP that INT grants.</summary>
        /// <param name="baseMaxMana">The max mana before the INT bonus.</param>
        /// <param name="intelligence">The effective INT.</param>
        /// <returns>The bonus, rounded to the nearest whole point.</returns>
        public static int MaxManaBonus(int baseMaxMana, int intelligence)
        {
            return Mathf.RoundToInt(baseMaxMana * intelligence * MaxManaBonusPerInt);
        }

        /// <summary>Calculates the factor applied to a skill's variable cast time: (DEX x 2 + INT) / 530 is shaved off.</summary>
        /// <param name="dexterity">The effective DEX.</param>
        /// <param name="intelligence">The effective INT.</param>
        /// <returns>A multiplier between 0 (instant) and 1 (no reduction).</returns>
        public static float CastTimeMultiplier(int dexterity, int intelligence)
        {
            var reduction = (dexterity * 2 + intelligence) / CastTimeReductionDivisor;
            return Mathf.Clamp01(1f - reduction);
        }

        /// <summary>Calculates the chance to fully resist an incoming status effect.</summary>
        /// <param name="vitality">The effective VIT.</param>
        /// <returns>The resist chance from 0 to 1.</returns>
        public static float StatusResistChance(int vitality)
        {
            return Mathf.Clamp01((vitality + VitStatusResistOffset) / 100f);
        }
    }
}
