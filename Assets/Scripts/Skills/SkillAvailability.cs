namespace Project.Skills
{
    /// <summary>
    /// Describes whether a skill can currently be cast, and why not if it
    /// can't. Used by hotbar UI to decide how to render a slot (normal,
    /// on cooldown, or blocked).
    /// </summary>
    public enum SkillAvailability
    {
        /// <summary>The skill can be cast right now.</summary>
        Ready,

        /// <summary>The skill hasn't been learned yet (level 0).</summary>
        NotLearned,

        /// <summary>The skill was cast recently and is still cooling down.</summary>
        OnCooldown,

        /// <summary>The caster doesn't have enough mana to pay the skill's cost.</summary>
        InsufficientMana,

        /// <summary>The skill needs an enemy target in range, and none is currently valid.</summary>
        NoValidTarget
    }
}