namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component modify incoming damage before
    /// <see cref="HealthComponent"/> applies it — e.g. blocking, damage
    /// reduction, or elemental resistance.
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>
        /// Modifies an incoming damage amount before it's applied.
        /// </summary>
        /// <param name="amount">The raw incoming damage amount.</param>
        /// <param name="element">The element the incoming attack carries. <see cref="Element.Neutral"/> for non-elemental attacks (every basic attack except the Mage's, and any skill without one set).</param>
        /// <returns>The damage amount to actually apply, after any modification.</returns>
        int ModifyIncomingDamage(int amount, Element element);
    }
}