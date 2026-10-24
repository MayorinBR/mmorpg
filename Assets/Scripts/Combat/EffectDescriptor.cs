using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Describes how one effect is presented to the player. Producers build it
    /// from their own data (a skill, a status presentation entry), so the
    /// combat assembly never needs to reference the producer's assembly.
    /// </summary>
    public readonly struct EffectDescriptor
    {
        /// <summary>
        /// Creates a descriptor.
        /// </summary>
        /// <param name="id">Stable identifier. Applying a buff with the same id refreshes it instead of stacking.</param>
        /// <param name="displayName">Name shown in the tooltip.</param>
        /// <param name="description">Short explanation shown in the tooltip.</param>
        /// <param name="icon">Optional icon. A colored tile is shown when null.</param>
        /// <param name="category">The effect category.</param>
        public EffectDescriptor(string id, string displayName, string description, Sprite icon, EffectCategory category)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            Category = category;
        }

        /// <summary>Gets the stable identifier.</summary>
        public string Id { get; }

        /// <summary>Gets the name shown to the player.</summary>
        public string DisplayName { get; }

        /// <summary>Gets the explanation shown to the player.</summary>
        public string Description { get; }

        /// <summary>Gets the optional icon, or null.</summary>
        public Sprite Icon { get; }

        /// <summary>Gets the effect category.</summary>
        public EffectCategory Category { get; }

        /// <summary>Gets a value indicating whether this descriptor has an id and can be displayed.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Id);
    }
}
