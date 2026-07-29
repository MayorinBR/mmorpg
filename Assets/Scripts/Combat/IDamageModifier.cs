namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component modify incoming damage before
    /// <see cref="HealthComponent"/> applies it — e.g. blocking, damage
    /// reduction, or (in the future) elemental resistance.
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>
        /// Modifies an incoming damage amount before it's applied.
        /// </summary>
        /// <param name="amount">The raw incoming damage amount.</param>
        /// <returns>The damage amount to actually apply, after any modification.</returns>
        int ModifyIncomingDamage(int amount);
    }
}