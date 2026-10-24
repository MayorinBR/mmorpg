namespace Project.Combat
{
    /// <summary>
    /// Stable identifiers of the built-in conditions shown in the active
    /// effects bar. Used as <see cref="EffectDescriptor.Id"/> and as the lookup key
    /// of <see cref="StatusEffectPresentation"/>.
    /// </summary>
    public static class StatusEffectIds
    {
        /// <summary>Stun.</summary>
        public const string Stun = "status.stun";

        /// <summary>Poison.</summary>
        public const string Poison = "status.poison";

        /// <summary>Silence.</summary>
        public const string Silence = "status.silence";

        /// <summary>Blind.</summary>
        public const string Blind = "status.blind";

        /// <summary>Freeze.</summary>
        public const string Freeze = "status.freeze";

        /// <summary>Petrify.</summary>
        public const string Petrify = "status.petrify";

        /// <summary>Hiding.</summary>
        public const string Hiding = "status.hiding";

        /// <summary>Carrying at least the minor overweight ratio of the weight limit.</summary>
        public const string OverweightMinor = "status.overweight.minor";

        /// <summary>Carrying at least the major overweight ratio of the weight limit.</summary>
        public const string OverweightMajor = "status.overweight.major";
    }
}
