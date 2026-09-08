using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Converts the Aspd sub-stat into an actual auto-attack interval and
    /// animation speed, using classic Ragnarok Online's own ASPD-to-delay
    /// conversion (source: iRO Wiki Classic — Attack Speed, consulted
    /// September 2026): <c>delay(ms) = (200 - Aspd) * 10</c>. This
    /// conversion is the real, sourced game mechanic and is independent of
    /// how Aspd itself is calculated — unlike
    /// <see cref="Character.Stats.SubStatsCalculator"/>'s preliminary
    /// AGI/DEX-to-Aspd formula, this does not need to change when that
    /// formula is replaced with the real one.
    /// </summary>
    public static class AttackSpeedCalculator
    {
        private const int AspdDelayBase = 200;
        private const float MillisecondsPerAspdPoint = 10f;
        private const float MinIntervalSeconds = 0.1f;
        private const float MaxIntervalSeconds = 2f;

        // The interval below which the auto-attack animation is considered
        // to play at its authored (1x) speed. Set to the interval produced
        // by the lowest Aspd value this project can currently reach (140,
        // see SubStatsCalculator.BaseAspd) rather than hard-coding that Aspd
        // number here, so a future rebalance of the Aspd formula doesn't
        // silently change what "normal" animation speed means.
        private const float ReferenceIntervalSeconds = 0.6f;

        /// <summary>
        /// Converts an Aspd rating into the interval, in seconds, between
        /// auto-attacks, clamped to a sane range so an out-of-bounds Aspd
        /// can never produce a zero, negative or absurdly long interval.
        /// </summary>
        /// <param name="aspd">The Aspd sub-stat rating.</param>
        /// <returns>The delay, in seconds, before the next auto-attack.</returns>
        public static float GetAttackIntervalSeconds(int aspd)
        {
            var delaySeconds = (AspdDelayBase - aspd) * MillisecondsPerAspdPoint / 1000f;
            return Mathf.Clamp(delaySeconds, MinIntervalSeconds, MaxIntervalSeconds);
        }

        /// <summary>
        /// Converts an Aspd rating into an Animator playback speed
        /// multiplier for the auto-attack swing, so the animation speeds up
        /// proportionally to how much faster the character is actually
        /// attacking (1x at <see cref="ReferenceIntervalSeconds"/>, above 1x
        /// as Aspd climbs above that baseline).
        /// </summary>
        /// <param name="aspd">The Aspd sub-stat rating.</param>
        /// <returns>The animation speed multiplier to apply to the attack state.</returns>
        public static float GetAttackAnimationSpeedMultiplier(int aspd)
        {
            return ReferenceIntervalSeconds / GetAttackIntervalSeconds(aspd);
        }
    }
}
