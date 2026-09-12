using System;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Adds a class's Job Level stat bonus (see <see cref="ClassJobLevelBonusLookup"/>)
    /// on top of another <see cref="IStatProvider"/>, mirroring how
    /// <see cref="EquippedStatsView"/> layers in equipment bonuses. The
    /// bonus is re-read on every <see cref="GetValue"/> call, so it
    /// reacts live to Job Level ups without needing to be rebuilt.
    /// </summary>
    public class JobBonusStatsView : IStatProvider
    {
        private readonly IStatProvider inner;
        private readonly Func<StatModifiers> getBonus;

        /// <summary>
        /// Initializes a view adding a live-read stat bonus on top of another provider.
        /// </summary>
        /// <param name="inner">The provider to add the bonus on top of.</param>
        /// <param name="getBonus">Reads the current bonus; called on every <see cref="GetValue"/>.</param>
        public JobBonusStatsView(IStatProvider inner, Func<StatModifiers> getBonus)
        {
            this.inner = inner;
            this.getBonus = getBonus;
        }

        /// <inheritdoc />
        public int GetValue(StatType stat)
        {
            return inner.GetValue(stat) + ReadStat(getBonus(), stat);
        }

        private static int ReadStat(StatModifiers modifiers, StatType stat)
        {
            switch (stat)
            {
                case StatType.Strength: return modifiers.Strength;
                case StatType.Agility: return modifiers.Agility;
                case StatType.Vitality: return modifiers.Vitality;
                case StatType.Intelligence: return modifiers.Intelligence;
                case StatType.Dexterity: return modifiers.Dexterity;
                case StatType.Luck: return modifiers.Luck;
                default: return 0;
            }
        }
    }
}
