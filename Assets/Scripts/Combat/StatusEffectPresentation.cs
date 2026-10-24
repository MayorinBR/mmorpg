using System;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Optional asset that overrides how the built-in conditions are shown
    /// (name, description, icon). Any id without an entry falls back to the
    /// built-in text, so the bar works with no asset at all and icons can be
    /// added later without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffectPresentation", menuName = "Project/Combat/Status Effect Presentation")]
    public class StatusEffectPresentation : ScriptableObject
    {
        [SerializeField] private Entry[] entries;

        /// <summary>
        /// Resolves the descriptor of a built-in condition, preferring the authored entry.
        /// </summary>
        /// <param name="asset">The presentation asset, or null to use only the built-in text.</param>
        /// <param name="id">One of the <see cref="StatusEffectIds"/> values.</param>
        /// <returns>The descriptor to show.</returns>
        public static EffectDescriptor Resolve(StatusEffectPresentation asset, string id)
        {
            var fallback = GetDefault(id);

            if (asset == null || asset.entries == null)
            {
                return fallback;
            }

            foreach (var entry in asset.entries)
            {
                if (entry.Id == id)
                {
                    return new EffectDescriptor(
                        id,
                        string.IsNullOrEmpty(entry.DisplayName) ? fallback.DisplayName : entry.DisplayName,
                        string.IsNullOrEmpty(entry.Description) ? fallback.Description : entry.Description,
                        entry.Icon,
                        fallback.Category);
                }
            }

            return fallback;
        }

        /// <summary>
        /// Gets the built-in descriptor of a condition.
        /// </summary>
        /// <param name="id">One of the <see cref="StatusEffectIds"/> values.</param>
        /// <returns>The built-in descriptor. An unknown id yields a descriptor named after the id.</returns>
        public static EffectDescriptor GetDefault(string id)
        {
            switch (id)
            {
                case StatusEffectIds.Stun:
                    return Status(id, "Stun", "You cannot move, attack or cast skills.");
                case StatusEffectIds.Poison:
                    return Status(id, "Poison", "You take damage over time.");
                case StatusEffectIds.Silence:
                    return Status(id, "Silence", "You cannot cast skills. Movement and basic attacks still work.");
                case StatusEffectIds.Blind:
                    return Status(id, "Blind", "Your vision is impaired.");
                case StatusEffectIds.Freeze:
                    return Status(id, "Freeze", "Frozen solid: you cannot move, attack or cast skills. Your element becomes Water.");
                case StatusEffectIds.Petrify:
                    return Status(id, "Petrify", "Turned to stone: you cannot move, attack or cast skills. Your element becomes Earth.");
                case StatusEffectIds.Hiding:
                    return new EffectDescriptor(id, "Hiding", "Enemies cannot see you. Cast Hiding again to reveal yourself.", null, EffectCategory.Toggle);
                case StatusEffectIds.OverweightMinor:
                    return Status(id, "Overweight", "You carry too much: HP and SP no longer regenerate.");
                case StatusEffectIds.OverweightMajor:
                    return Status(id, "Heavily overweight", "You carry far too much: you cannot attack or cast skills.");
                default:
                    return Status(id, id, string.Empty);
            }
        }

        private static EffectDescriptor Status(string id, string displayName, string description)
        {
            return new EffectDescriptor(id, displayName, description, null, EffectCategory.Status);
        }

        [Serializable]
        private class Entry
        {
            [Tooltip("One of the StatusEffectIds values, for example status.poison.")]
            [SerializeField] private string id;

            [Tooltip("Leave empty to keep the built-in name.")]
            [SerializeField] private string displayName;

            [Tooltip("Leave empty to keep the built-in description.")]
            [SerializeField, TextArea] private string description;

            [SerializeField] private Sprite icon;

            public string Id => id;

            public string DisplayName => displayName;

            public string Description => description;

            public Sprite Icon => icon;
        }
    }
}
