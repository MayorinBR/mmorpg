using UnityEngine;
using Project.Character.Combat;
using Project.Character.Stats;

namespace Project.UI
{
    /// <summary>
    /// Displays the player's status-derived sub-stats (Atk, Matk, Def, MDef,
    /// Hit, Flee, Critical and Aspd), reading from
    /// <see cref="PlayerStatsController.CurrentSubStats"/>. Refreshes on
    /// <see cref="PlayerStatsController.StatsChanged"/> — raised whenever a
    /// stat point is spent, stats are reset, a save is restored, or
    /// equipment changes — and on level up, since <see cref="PlayerExperience"/>
    /// grants stat points and raises the character's base level outside of
    /// <see cref="PlayerStatsController"/>'s own methods.
    /// </summary>
    public class SubStatsPanelUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private PlayerExperience experience;
        [SerializeField] private SubStatRowUI[] subStatRows;

        private void OnEnable()
        {
            statsController.StatsChanged += RefreshAll;

            if (experience != null)
            {
                experience.LeveledUp += HandleLeveledUp;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            statsController.StatsChanged -= RefreshAll;

            if (experience != null)
            {
                experience.LeveledUp -= HandleLeveledUp;
            }
        }

        private void HandleLeveledUp(int newLevel)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            var subStats = statsController.CurrentSubStats;

            foreach (var row in subStatRows)
            {
                row.SetValue(FormatValue(row.SubStatType, subStats));
            }
        }

        /// <summary>
        /// Formats a single sub-stat for display. Every value is shown as
        /// its raw rating, matching classic Ragnarok Online's own status
        /// window — Hit, Flee and Aspd briefly showed a derived percentage
        /// or a base-1.0 multiplier instead (see FUTURE_IMPROVEMENTS.md),
        /// reverted at Victor's request to keep every sub-stat's
        /// calculation and presentation consistent with the original game.
        /// Aspd uses a preliminary placeholder formula (see
        /// <see cref="SubStatsCalculator"/>'s remarks and
        /// FUTURE_IMPROVEMENTS.md) — display-only styling aside, it already
        /// drives real auto-attack timing and animation speed, just not yet
        /// the real Ragnarok Online-sourced formula.
        /// </summary>
        private static string FormatValue(SubStatType type, SubStats subStats)
        {
            switch (type)
            {
                case SubStatType.Atk: return subStats.StatusAtk.ToString();
                case SubStatType.Matk: return subStats.StatusMatk.ToString();
                case SubStatType.Def: return subStats.StatusDef.ToString();
                case SubStatType.MDef: return subStats.StatusMDef.ToString();
                case SubStatType.Hit: return subStats.Hit.ToString();
                case SubStatType.Flee: return subStats.Flee.ToString();
                case SubStatType.CriticalRate: return $"{subStats.CriticalRate:0.#}%";
                case SubStatType.Aspd: return subStats.Aspd.ToString();
                default: return string.Empty;
            }
        }
    }
}
