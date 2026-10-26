using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Maps each <see cref="CharacterClass"/> to the flat stat bonus real
    /// Ragnarok Online grants automatically at a given Job Level, authored as one cumulative breakpoint per Job Level where any stat rises (e.g.
    /// Swordman reaches +7 STR/+2 AGI/+4 VIT/+3 DEX/+2 LUK at Job 50) — separate
    /// from normal spendable stat points. Read by <see cref="PlayerStatsController"/>
    /// through <see cref="JobBonusStatsView"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassJobLevelBonusLookup", menuName = "Project/Character/Class Job Level Bonus Lookup")]
    public class ClassJobLevelBonusLookup : ScriptableObject
    {
        [SerializeField] private ClassBonusEntry[] bonuses;

        /// <summary>
        /// Gets the stat bonus a class has earned by the given Job Level.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <param name="jobLevel">The character's current Job Level.</param>
        /// <returns>
        /// The cumulative bonus of the highest breakpoint the class has
        /// reached, i.e. the entry with the largest required Job Level not
        /// above <paramref name="jobLevel"/>; zero if none is reached or the
        /// class isn't configured. Each entry holds the total bonus from that
        /// Job Level onward, not a delta.
        /// </returns>
        public StatModifiers GetBonus(CharacterClass characterClass, int jobLevel)
        {
            var best = default(StatModifiers);
            var bestLevel = int.MinValue;

            foreach (var entry in bonuses)
            {
                if (entry.CharacterClass == characterClass && jobLevel >= entry.RequiredJobLevel && entry.RequiredJobLevel > bestLevel)
                {
                    best = entry.Bonus;
                    bestLevel = entry.RequiredJobLevel;
                }
            }

            return best;
        }

        [Serializable]
        private struct ClassBonusEntry
        {
            [SerializeField] private CharacterClass characterClass;
            [SerializeField] private int requiredJobLevel;
            [SerializeField] private StatModifiers bonus;

            public CharacterClass CharacterClass => characterClass;
            public int RequiredJobLevel => requiredJobLevel;
            public StatModifiers Bonus => bonus;
        }
    }
}
