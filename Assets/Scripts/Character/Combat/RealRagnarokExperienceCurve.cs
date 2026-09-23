namespace Project.Character.Combat
{
    /// <summary>
    /// Real Ragnarok Online Classic base experience requirements per level
    /// (source: iRO Wiki Classic — Base EXP Chart, consulted September 2026).
    /// The real game has no closed-form formula for this curve, unlike the
    /// stat-point curves elsewhere in <c>Project.Character.Stats</c> — it's
    /// an authored table per level — so this is a lookup array rather than
    /// a calculation.
    /// </summary>
    public class RealRagnarokExperienceCurve : IExperienceCurve
    {
        // Index 0 is unused. Index N is the experience required to advance
        // from level N to level N + 1. Sourced verbatim from the wiki chart.
        private static readonly int[] RequiredExperience =
        {
            0,
            9, 16, 25, 36, 77, 112, 153, 200, 253, 320,
            385, 490, 585, 700, 830, 970, 1120, 1260, 1420, 1620,
            1860, 1990, 2240, 2504, 2950, 3426, 3934, 4474, 6889, 7995,
            9174, 10425, 11748, 13967, 15775, 17678, 19677, 21773, 30543, 34212,
            38065, 42102, 46323, 53026, 58419, 64041, 69892, 75973, 102468, 115254,
            128692, 142784, 157528, 178184, 196300, 215198, 234879, 255341, 330188, 365914,
            403224, 442116, 482590, 536948, 585191, 635278, 687211, 740988, 925400, 1473746,
            1594058, 1718928, 1848355, 1982340, 2230113, 2386162, 2547417, 2713878, 3206160, 3681024,
            4022472, 4377024, 4744680, 5125440, 5767272, 6204000, 6655464, 7121664, 7602600, 9738720,
            11649960, 13643520, 18339300, 23836800, 35658000, 48687000, 58135000, 99999998,
        };

        /// <summary>
        /// Gets the experience required to advance from <paramref name="currentLevel"/>
        /// to the next level. Returns <see cref="int.MaxValue"/> at level 99
        /// or above, since 99 is this project's current level cap (no
        /// rebirth/transcendence exists here, matching the scope already
        /// established by <c>RagnarokStatPointCostStrategy</c>) — the wiki's
        /// own chart likewise has no entry past level 98.
        /// </summary>
        /// <param name="currentLevel">The character's current level.</param>
        /// <returns>The experience amount required for the next level up.</returns>
        public int GetRequiredExperience(int currentLevel)
        {
            if (currentLevel < 1)
            {
                currentLevel = 1;
            }

            return currentLevel >= RequiredExperience.Length - 1
                ? int.MaxValue
                : RequiredExperience[currentLevel];
        }
    }
}
