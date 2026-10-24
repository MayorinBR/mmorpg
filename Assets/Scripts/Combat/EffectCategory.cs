namespace Project.Combat
{
    /// <summary>
    /// Groups the conditions shown in the active effects bar, deciding the
    /// slot border color and the display order (see <see cref="ActiveEffectRules"/>).
    /// </summary>
    public enum EffectCategory
    {
        /// <summary>A beneficial, timed effect, such as Blessing or Endure.</summary>
        Buff,

        /// <summary>A harmful, timed effect that changes stats, such as Provoke.</summary>
        Debuff,

        /// <summary>A harmful condition, such as Poison, Stun, Silence, Blind, Freeze or Petrify.</summary>
        Status,

        /// <summary>An effect the player switches on and off, such as Hiding or Energy Coat.</summary>
        Toggle,

        /// <summary>A permanent bonus from a learned passive skill. Hidden by default in the bar.</summary>
        Passive
    }
}
