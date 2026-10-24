namespace Project.Combat
{
    /// <summary>
    /// A snapshot of one effect currently affecting a character: what it is
    /// (<see cref="Descriptor"/>) and how long it still lasts.
    /// </summary>
    public readonly struct ActiveEffect
    {
        /// <summary>
        /// Creates an active effect snapshot.
        /// </summary>
        /// <param name="descriptor">How the effect is presented.</param>
        /// <param name="remainingSeconds">Seconds left, or zero when untimed.</param>
        /// <param name="totalSeconds">Full duration in seconds, or zero when the effect has no timer.</param>
        /// <param name="detail">Extra tooltip lines, such as the stat changes. May be empty.</param>
        public ActiveEffect(EffectDescriptor descriptor, float remainingSeconds, float totalSeconds, string detail = "")
        {
            Descriptor = descriptor;
            RemainingSeconds = remainingSeconds;
            TotalSeconds = totalSeconds;
            Detail = detail ?? string.Empty;
        }

        /// <summary>Gets how the effect is presented.</summary>
        public EffectDescriptor Descriptor { get; }

        /// <summary>Gets the seconds left. Zero for an untimed effect.</summary>
        public float RemainingSeconds { get; }

        /// <summary>Gets the full duration in seconds. Zero for an untimed effect.</summary>
        public float TotalSeconds { get; }

        /// <summary>Gets extra tooltip lines, such as the stat changes.</summary>
        public string Detail { get; }

        /// <summary>Gets a value indicating whether the effect expires on its own.</summary>
        public bool HasTimer => TotalSeconds > 0f;

        /// <summary>Gets the remaining fraction from 1 (just applied) to 0 (about to expire). Always 1 for an untimed effect.</summary>
        public float RemainingFraction => HasTimer ? UnityEngine.Mathf.Clamp01(RemainingSeconds / TotalSeconds) : 1f;
    }
}
