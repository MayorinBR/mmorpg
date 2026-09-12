using UnityEngine;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Contract for anything that can receive damage. Attackers depend only
    /// on this interface, not on a concrete health implementation, so the
    /// same attack code works against players, mobs, or destructible objects.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Gets a value indicating whether this entity has run out of health.</summary>
        bool IsDead { get; }

        /// <summary>Gets the dodge rating an attacker's Hit is checked against to resolve hit chance.</summary>
        int FleeRating { get; }

        /// <summary>Gets this entity's size class, read by the attacker to scale incoming physical damage by weapon type (e.g. a dagger vs. a Large monster).</summary>
        MonsterSize Size { get; }

        /// <summary>
        /// Applies damage, reducing current health down to a minimum of zero.
        /// Has no effect if the entity is already dead.
        /// </summary>
        /// <param name="amount">The amount of damage to apply. Non-positive values are ignored.</param>
        /// <param name="element">The element this damage carries. Defaults to <see cref="Element.Neutral"/> for ordinary, non-elemental damage.</param>
        /// <param name="category">Whether this damage is mitigated by physical or magical defense. Defaults to <see cref="DamageCategory.Physical"/>.</param>
        /// <param name="isCritical">Whether this hit is a critical hit, purely for cosmetic feedback (e.g. floating damage numbers). Defaults to false.</param>
        /// <param name="attacker">
        /// Who dealt this damage, if known. Lets the entity being hit react
        /// to exactly who attacked it — e.g. <see cref="Project.AI.EnemyController"/>
        /// targets this transform immediately, regardless of its own
        /// detection range — instead of having to guess. Null when the
        /// source doesn't need that (e.g. an enemy hitting the player).
        /// </param>
        void TakeDamage(int amount, Element element = Element.Neutral, DamageCategory category = DamageCategory.Physical, bool isCritical = false, Transform attacker = null);

        /// <summary>
        /// Notifies this entity that an incoming attack missed it, purely
        /// for cosmetic dodge feedback (e.g. a floating "dodge" popup).
        /// Attackers call this instead of <see cref="TakeDamage"/> when
        /// their hit-chance roll fails.
        /// </summary>
        void NotifyDodged();
    }
}