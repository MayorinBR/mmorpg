using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Pure display rules for the active effects bar: ordering, the expiring
    /// warning and the remaining time text.
    /// </summary>
    public static class ActiveEffectRules
    {
        /// <summary>Seconds left under which a timed effect is considered about to expire.</summary>
        public const float ExpiringThresholdSeconds = 3f;

        /// <summary>
        /// Checks whether a timed effect is about to expire.
        /// </summary>
        /// <param name="effect">The effect to check.</param>
        /// <returns>True when the effect has a timer and little time left.</returns>
        public static bool IsExpiring(ActiveEffect effect)
        {
            return effect.HasTimer && effect.RemainingSeconds <= ExpiringThresholdSeconds;
        }

        /// <summary>
        /// Sorts effects for display: harmful first (debuffs, then statuses), then buffs, toggles and passives.
        /// Inside a category, timed effects come first, shortest remaining time first, then by name.
        /// </summary>
        /// <param name="effects">The list to sort in place.</param>
        public static void SortForDisplay(List<ActiveEffect> effects)
        {
            var sorted = effects
                .OrderBy(effect => CategoryOrder(effect.Descriptor.Category))
                .ThenBy(effect => effect.HasTimer ? 0 : 1)
                .ThenBy(effect => effect.HasTimer ? effect.RemainingSeconds : 0f)
                .ThenBy(effect => effect.Descriptor.DisplayName, System.StringComparer.Ordinal)
                .ToList();

            effects.Clear();
            effects.AddRange(sorted);
        }

        /// <summary>
        /// Formats a remaining time for the tooltip: 45s, 2m 05s or 1h 03m.
        /// </summary>
        /// <param name="seconds">The remaining seconds.</param>
        /// <returns>The formatted text.</returns>
        public static string FormatRemaining(float seconds)
        {
            var total = Mathf.Max(0, Mathf.CeilToInt(seconds));

            if (total < 60)
            {
                return $"{total}s";
            }

            if (total < 3600)
            {
                return $"{total / 60}m {total % 60:00}s";
            }

            return $"{total / 3600}h {total % 3600 / 60:00}m";
        }

        private static int CategoryOrder(EffectCategory category)
        {
            switch (category)
            {
                case EffectCategory.Debuff:
                    return 0;
                case EffectCategory.Status:
                    return 1;
                case EffectCategory.Buff:
                    return 2;
                case EffectCategory.Toggle:
                    return 3;
                default:
                    return 4;
            }
        }
    }
}
