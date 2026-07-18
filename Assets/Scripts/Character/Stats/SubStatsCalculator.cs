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
        public SubStats Calculate(IStatProvider stats, int baseLevel)
        {
            var str = stats.GetValue(StatType.Strength);
            var agi = stats.GetValue(StatType.Agility);
            var vit = stats.GetValue(StatType.Vitality);
            var intel = stats.GetValue(StatType.Intelligence);
            var dex = stats.GetValue(StatType.Dexterity);
            var luk = stats.GetValue(StatType.Luck);

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