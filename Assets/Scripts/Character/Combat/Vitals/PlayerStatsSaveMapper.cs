using Project.Character.Stats;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Copies the base stat block to and from the save data, keeping the persistence field names out of the stats controller.
    /// </summary>
    public static class PlayerStatsSaveMapper
    {
        /// <summary>Writes the six base stats and the unspent points into the save data.</summary>
        /// <param name="stats">The base stat block to read.</param>
        /// <param name="data">The save data to fill.</param>
        public static void Capture(CharacterBaseStats stats, PlayerSaveData data)
        {
            data.strength = stats.GetValue(StatType.Strength);
            data.agility = stats.GetValue(StatType.Agility);
            data.vitality = stats.GetValue(StatType.Vitality);
            data.intelligence = stats.GetValue(StatType.Intelligence);
            data.dexterity = stats.GetValue(StatType.Dexterity);
            data.luck = stats.GetValue(StatType.Luck);
            data.availableStatPoints = stats.AvailablePoints;
        }

        /// <summary>Loads the six base stats and the unspent points from the save data.</summary>
        /// <param name="stats">The base stat block to overwrite.</param>
        /// <param name="data">The save data to read.</param>
        public static void Restore(CharacterBaseStats stats, PlayerSaveData data)
        {
            stats.SetValue(StatType.Strength, data.strength);
            stats.SetValue(StatType.Agility, data.agility);
            stats.SetValue(StatType.Vitality, data.vitality);
            stats.SetValue(StatType.Intelligence, data.intelligence);
            stats.SetValue(StatType.Dexterity, data.dexterity);
            stats.SetValue(StatType.Luck, data.luck);
            stats.SetAvailablePoints(data.availableStatPoints);
        }
    }
}
