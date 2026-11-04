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

        private string[] shownValues;

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

        private void Update()
        {
            RefreshAll();
        }

        /// <summary>
        /// Rewrites the rows whose text changed. Runs every frame because timed buffs
        /// start and expire without raising <see cref="PlayerStatsController.StatsChanged"/>.
        /// </summary>
        private void RefreshAll()
        {
            var subStats = statsController.CurrentSubStats;
            var def = statsController.EffectiveDef;
            var mdef = statsController.EffectiveMDef;

            if (shownValues == null || shownValues.Length != subStatRows.Length)
            {
                shownValues = new string[subStatRows.Length];
            }

            for (var i = 0; i < subStatRows.Length; i++)
            {
                var text = FormatValue(subStatRows[i].SubStatType, subStats, def, mdef, statsController.EffectiveAspd);

                if (text == shownValues[i])
                {
                    continue;
                }

                shownValues[i] = text;
                subStatRows[i].SetValue(text);
            }
        }

        /// <summary>
        /// Formats a single sub-stat for display. Every value is shown as
        /// its raw rating, matching classic Ragnarok Online's own status
        /// window — Hit, Flee and Aspd briefly showed a derived percentage
        /// or a base-1.0 multiplier instead (see FUTURE_IMPROVEMENTS.md),
        /// reverted to keep every sub-stat's calculation and
        /// presentation consistent with the original game.
        /// Aspd uses a preliminary placeholder formula (see
        /// <see cref="SubStatsCalculator"/>'s remarks and
        /// FUTURE_IMPROVEMENTS.md) — display-only styling aside, it already
        /// drives real auto-attack timing and animation speed, just not yet
        /// the real Ragnarok Online-sourced formula. The displayed Aspd is
        /// the effective one, with the equipped weapon's speed multiplier applied.
        /// </summary>
        /// <param name="type">The sub-stat to format.</param>
        /// <param name="subStats">The current sub-stats.</param>
        /// <param name="def">The Def after buff multipliers.</param>
        /// <param name="mdef">The MDef including buff bonuses.</param>
        /// <param name="effectiveAspd">The Aspd rating after the weapon speed multiplier.</param>
        /// <returns>The text to display.</returns>
        private static string FormatValue(SubStatType type, SubStats subStats, int def, int mdef, int effectiveAspd)
        {
            switch (type)
            {
                case SubStatType.Atk: return subStats.StatusAtk.ToString();
                case SubStatType.Matk: return subStats.StatusMatk.ToString();
                case SubStatType.Def: return def.ToString();
                case SubStatType.MDef: return mdef.ToString();
                case SubStatType.Hit: return subStats.Hit.ToString();
                case SubStatType.Flee: return subStats.Flee.ToString();
                case SubStatType.CriticalRate: return $"{subStats.CriticalRate:0.#}%";
                case SubStatType.Aspd: return effectiveAspd.ToString();
                default: return string.Empty;
            }
        }
    }
}
