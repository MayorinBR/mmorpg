using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Converts the Aspd sub-stat into an actual auto-attack interval, using
    /// classic Ragnarok Online's own ASPD-to-delay conversion (source: iRO
    /// Wiki Classic — Attack Speed, consulted September 2026):
    /// <c>delay(ms) = (200 - Aspd) * 10</c>. This conversion is the real,
    /// sourced game mechanic and is independent of how Aspd itself is
    /// calculated — unlike <see cref="Character.Stats.SubStatsCalculator"/>'s
    /// preliminary AGI/DEX-to-Aspd formula, this does not need to change
    /// when that formula is replaced with the real one. How this interval
    /// turns into an actual swing-animation playback speed is
    /// <see cref="Animation.PlayerAnimatorController.SetAttackDuration"/>'s
    /// job, not this class's — it needs the real
    /// <see cref="UnityEngine.AnimationClip.length"/> of whichever swing is
    /// about to play, which this purely numeric class has no access to.
    /// </summary>
    public static class AttackSpeedCalculator
    {
        private const int AspdDelayBase = 200;
        private const float MillisecondsPerAspdPoint = 10f;
        private const float MinIntervalSeconds = 0.1f;
        private const float MaxIntervalSeconds = 2f;

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
    }
}
