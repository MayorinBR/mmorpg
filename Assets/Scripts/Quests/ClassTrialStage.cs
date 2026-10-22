namespace Project.Quests
{
    /// <summary>
    /// The state of a player's class trial as seen by the trial master NPC,
    /// which decides what the NPC offers.
    /// </summary>
    public enum ClassTrialStage
    {
        /// <summary>The player is no longer a Novice, so the trial is behind them.</summary>
        AlreadyPromoted,

        /// <summary>A trial is finished but the player has not turned it in at the NPC yet.</summary>
        ClaimPromotion,

        /// <summary>A trial is in progress and can be abandoned.</summary>
        TrialActive,

        /// <summary>The Novice has not met the requirements to start a trial.</summary>
        NotReady,

        /// <summary>The Novice may pick one of the class trials.</summary>
        ChooseTrial
    }
}
