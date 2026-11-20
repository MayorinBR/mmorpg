namespace Project.Ambience
{
    /// <summary>
    /// A light source that the day and night cycle can switch on and off.
    /// </summary>
    public interface ILightSwitchable
    {
        /// <summary>
        /// Gets a value indicating whether the source is currently lit.
        /// </summary>
        bool IsLit { get; }

        /// <summary>
        /// Switches the source on or off.
        /// </summary>
        /// <param name="lit">True to light the source, false to turn it off.</param>
        void SetLit(bool lit);

        /// <summary>
        /// Switches the source on or off as part of a dusk or dawn transition. Sources may spread the change
        /// over time; by default the switch is immediate.
        /// </summary>
        /// <param name="lit">True to light the source, false to turn it off.</param>
        void TransitionTo(bool lit)
        {
            SetLit(lit);
        }
    }
}
