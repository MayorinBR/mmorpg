namespace Project.AI
{
    /// <summary>
    /// A mob's engagement behavior, matching Ragnarok Online's monster
    /// "Aggressive" vs "Passive" mode distinction.
    /// </summary>
    public enum EnemyBehaviorMode
    {
        /// <summary>Automatically detects and chases the player within aggro range.</summary>
        Aggressive,

        /// <summary>Stays idle regardless of proximity; only engages after taking damage.</summary>
        Passive
    }
}