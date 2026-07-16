namespace Project.Character.Stats
{
    /// <summary>
    /// Computes sub-stats using the classic Ragnarok Online formulas for the
    /// status-only component of each value. Weapon-based ATK/MATK and
    /// item-based DEF are added on top of this by the equipment system.
    /// </summary>
    public class SubStatsCalculator : ISubStatsCalculator
    {
        /// <inheritdoc />
        public SubStats Calculate(CharacterBaseStats baseStats, int baseLevel)
        {
            var str = baseStats.GetValue(StatType.Strength);
            var agi = baseStats.GetValue(StatType.Agility);
            var vit = baseStats.GetValue(StatType.Vitality);
            var intel = baseStats.GetValue(StatType.Intelligence);
            var dex = baseStats.GetValue(StatType.Dexterity);
            var luk = baseStats.GetValue(StatType.Luck);

            var statusAtk = str;
            var statusMatk = (int)(intel * 1.5f);
            var statusDef = vit / 2;
            var statusMDef = intel + (vit / 5) + (dex / 5) + (baseLevel / 4);
            var hit = 175 + dex + (luk / 3) + baseLevel;
            var flee = agi + baseLevel;
            var criticalRate = luk * 0.3f;

            return new SubStats(statusAtk, statusMatk, statusDef, statusMDef, hit, flee, criticalRate);
        }
    }
}