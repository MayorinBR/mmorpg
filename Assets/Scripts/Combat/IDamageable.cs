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

        /// <summary>
        /// Applies damage, reducing current health down to a minimum of zero.
        /// Has no effect if the entity is already dead.
        /// </summary>
        /// <param name="amount">The amount of damage to apply. Non-positive values are ignored.</param>
        void TakeDamage(int amount);
    }
}