using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Resolves whether an attack lands, using the classic Ragnarok Online
    /// accuracy formula (source: iRO Wiki Classic — Accuracy and Flee,
    /// consulted September 2026): <c>HitChance% = 100 + (AttackerHit -
    /// TargetFlee)</c>, clamped so a fight is never a guaranteed hit or a
    /// guaranteed miss. Critical hits are not resolved here — Ragnarok
    /// Online lets a critical bypass the accuracy check entirely, so callers
    /// should only consult this for a non-critical hit.
    /// </summary>
    public static class HitChanceCalculator
    {
        private const float MinHitChancePercent = 5f;
        private const float MaxHitChancePercent = 95f;

        /// <summary>
        /// Rolls whether an attack lands.
        /// </summary>
        /// <param name="attackerHit">The attacker's accuracy rating.</param>
        /// <param name="targetFlee">The target's dodge rating.</param>
        /// <returns>True if the attack hits; false if it misses.</returns>
        public static bool RollHit(int attackerHit, int targetFlee)
        {
            var hitChance = Mathf.Clamp(100 + attackerHit - targetFlee, MinHitChancePercent, MaxHitChancePercent);
            return Random.value * 100f < hitChance;
        }
    }
}
