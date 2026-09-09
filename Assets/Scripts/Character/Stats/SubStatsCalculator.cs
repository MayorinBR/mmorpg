using System;

namespace Project.Character.Stats
{
    /// <summary>
    /// Computes sub-stats using the classic (pre-renewal) Ragnarok Online
    /// formulas for the status-only component of each value (source: iRO
    /// Wiki Classic — Stats, consulted September 2026, the same page
    /// already cited by <see cref="RagnarokStatPointCostStrategy"/> and
    /// <see cref="RagnarokStatPointsPerLevelStrategy"/>). Weapon-based bonus
    /// ATK/MATK and item-based DEF are added on top of this by the
    /// equipment system, not modeled here.
    /// </summary>
    /// <remarks>
    /// Three simplifications are made relative to the source, called out
    /// here rather than silently applied:
    /// <list type="bullet">
    /// <item>MATK is modeled as a single value. Real Ragnarok Online rolls
    /// a random value in an INT-derived [min, max] range on every magical
    /// hit; this project doesn't have a randomized-damage-range concept
    /// anywhere yet (basic attacks and skills both deal a fixed number
    /// before the critical-hit roll), so <see cref="StatType.Intelligence"/>
    /// resolves to the midpoint of that range instead.</item>
    /// <item>Status DEF uses the source's own "approximately 0.8 VIT-based
    /// DEF" figure, which the source states holds precisely only up to 50
    /// VIT and grows slightly faster above that without giving an exact
    /// formula. This applies the same 0.8 ratio across the full 1-99 range
    /// <see cref="CharacterBaseStats"/> allows, rather than fabricating an
    /// unsourced curve for the upper half.</item>
    /// <item><b>Aspd is a preliminary placeholder, not sourced from iRO
    /// Wiki like the rest of this class.</b> Real Ragnarok Online ASPD
    /// depends on the equipped weapon's base delay and has diminishing
    /// returns from AGI/DEX past certain thresholds — not yet verified
    /// enough to commit to code (see FUTURE_IMPROVEMENTS.md). Until that formula is
    /// researched, this uses a simple linear approximation (the caller-
    /// supplied base Aspd — see <see cref="Calculate"/>'s
    /// <c>baseAspd</c> parameter — plus a flat AGI/DEX contribution,
    /// clamped to the real game's 0-190 display range) purely so the
    /// status window has a number to show, matching the original game's
    /// own presentation (Victor asked for every sub-stat's calculation and
    /// display to stay consistent with classic Ragnarok Online's status
    /// window, after briefly trying a base-1.0 multiplier representation
    /// instead — see FUTURE_IMPROVEMENTS.md for that reverted attempt).
    /// This value already drives real auto-attack timing and animation
    /// speed via <see cref="Combat.AttackSpeedCalculator"/> — replace this
    /// whole calculation once the real per-weapon formula is verified.</item>
    /// </list>
    /// </remarks>
    public class SubStatsCalculator : ISubStatsCalculator
    {
        private const int MaxAspd = 190;
        private const float AgiAspdWeight = 0.25f;
        private const float DexAspdWeight = 0.1f;

        /// <inheritdoc />
        /// <remarks>
        /// StatusATK comes from STR when melee, or DEX when wielding a
        /// ranged weapon (<paramref name="weaponIsRanged"/>) — the real
        /// Ragnarok Online mechanic behind "Archers deal more damage with
        /// DEX, everyone else with STR": it's the weapon type that decides,
        /// not the job class, so any class wielding a bow scales off DEX
        /// the same way. LUK adds a small universal bonus on top,
        /// regardless of weapon.
        /// </remarks>
        public SubStats Calculate(IStatProvider stats, int baseLevel, bool weaponIsRanged, int baseAspd)
        {
            var str = stats.GetValue(StatType.Strength);
            var agi = stats.GetValue(StatType.Agility);
            var vit = stats.GetValue(StatType.Vitality);
            var intel = stats.GetValue(StatType.Intelligence);
            var dex = stats.GetValue(StatType.Dexterity);
            var luk = stats.GetValue(StatType.Luck);

            var mainAtkStat = weaponIsRanged ? dex : str;
            var statusAtk = mainAtkStat + (mainAtkStat / 10) * (mainAtkStat / 10) + (luk / 5);

            var minMatk = intel + (intel / 7) * (intel / 7);
            var maxMatk = intel + (intel / 5) * (intel / 5);
            var statusMatk = (minMatk + maxMatk) / 2;

            var statusDef = (int)(vit * 0.8f);
            var statusMDef = intel + (vit / 5) + (dex / 5) + (baseLevel / 4);
            var hit = baseLevel + dex;
            var flee = agi + baseLevel;
            var criticalRate = (luk * 0.3f) + 1f;
            var aspd = CalculatePreliminaryAspd(baseAspd, agi, dex);

            return new SubStats(statusAtk, statusMatk, statusDef, statusMDef, hit, flee, criticalRate, aspd);
        }

        /// <summary>
        /// Preliminary Aspd approximation — see the placeholder note in this
        /// class's remarks. Linear in AGI and DEX on top of the caller-
        /// supplied base, clamped to the real game's 0-190 display range.
        /// </summary>
        private static int CalculatePreliminaryAspd(int baseAspd, int agi, int dex)
        {
            var raw = baseAspd + (int)(agi * AgiAspdWeight) + (int)(dex * DexAspdWeight);
            return Math.Clamp(raw, 0, MaxAspd);
        }
    }
}