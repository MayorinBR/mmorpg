using UnityEngine;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Owns the player's base stats and exposes calculated sub-stats for
    /// combat. Acts as the composition root connecting the plain C# stat
    /// classes to the Unity component world. When an
    /// <see cref="EquipmentManager"/> is assigned, equipped item bonuses are
    /// included in the sub-stats calculation.
    /// </summary>
    public class PlayerStatsController : MonoBehaviour
    {
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int startingStatPoints = 10;
        [SerializeField] private EquipmentManager equipment;

        private CharacterBaseStats baseStats;
        private ISubStatsCalculator subStatsCalculator;
        private IStatProvider effectiveStats;

        /// <summary>Gets the player's base stat block (STR, AGI, VIT, INT, DEX, LUK).</summary>
        public CharacterBaseStats BaseStats => baseStats;

        /// <summary>Gets or sets the player's current base level, driven by the experience system.</summary>
        public int BaseLevel { get; set; }

        /// <summary>Gets the sub-stats calculated from the current effective stats and level.</summary>
        public SubStats CurrentSubStats => subStatsCalculator.Calculate(effectiveStats, BaseLevel);

        private void Awake()
        {
            BaseLevel = startingLevel;
            baseStats = new CharacterBaseStats(new FlatStatPointCostStrategy());
            baseStats.GrantPoints(startingStatPoints);
            subStatsCalculator = new SubStatsCalculator();
            effectiveStats = equipment != null ? new EquippedStatsView(baseStats, equipment) : baseStats;
        }
    }
}