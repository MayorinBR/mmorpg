namespace Project.Combat
{
    /// <summary>
    /// The minimal currency operations a transaction needs: spend and pay.
    /// Implemented by <see cref="Character.Combat.PlayerCurrency"/>. Exists
    /// so a gameplay assembly that needs to charge or pay currency (e.g.
    /// <c>Project.NPC</c>) can depend on this small leaf interface instead
    /// of the whole <c>Project.Character.Combat</c> assembly — the same
    /// dependency-inversion reason <see cref="PlayerFeedbackChannel"/>
    /// lives here rather than in a gameplay assembly.
    /// </summary>
    public interface ICurrencyWallet
    {
        /// <summary>Attempts to spend an amount, failing if there isn't enough.</summary>
        /// <param name="amount">The amount to spend. Must be positive.</param>
        /// <returns>True if there was enough and it was spent; false otherwise.</returns>
        bool TrySpend(int amount);

        /// <summary>Adds an amount, typically as payment received.</summary>
        /// <param name="amount">The amount to add. Non-positive values are ignored.</param>
        void Add(int amount);
    }
}
