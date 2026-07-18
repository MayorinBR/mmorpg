using System;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Combines a character's invested base stats with the bonuses granted
    /// by currently equipped items, presenting the result as a single
    /// <see cref="IStatProvider"/>. Keeps <see cref="CharacterBaseStats"/>
    /// entirely unaware of equipment.
    /// </summary>
    public class EquippedStatsView : IStatProvider
    {
        private readonly CharacterBaseStats baseStats;
        private readonly EquipmentManager equipment;

        /// <summary>
        /// Initializes a view combining base stats with equipped item bonuses.
        /// </summary>
        /// <param name="baseStats">The character's invested base stats.</param>
        /// <param name="equipment">The character's equipment manager.</param>
        public EquippedStatsView(CharacterBaseStats baseStats, EquipmentManager equipment)
        {
            this.baseStats = baseStats;
            this.equipment = equipment;
        }

        private const int MinimumEffectiveValue = 1;

        /// <inheritdoc />
        public int GetValue(StatType stat)
        {
            var total = baseStats.GetValue(stat) + equipment.GetBonus(stat);
            return Math.Max(total, MinimumEffectiveValue);
        }
    }
}