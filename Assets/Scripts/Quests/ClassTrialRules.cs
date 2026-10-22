namespace Project.Quests
{
    /// <summary>
    /// Pure decision rules for the class trial conversation, kept free of scene
    /// and UI types so they can be unit tested.
    /// </summary>
    public static class ClassTrialRules
    {
        /// <summary>
        /// Decides what the trial master NPC should offer.
        /// </summary>
        /// <param name="isNovice">Whether the player is still a Novice.</param>
        /// <param name="hasCompletedTrial">Whether any class trial quest is already completed.</param>
        /// <param name="hasActiveTrial">Whether any class trial quest is currently in progress.</param>
        /// <param name="canEvolve">Whether the player meets the Job Level and skill point requirements.</param>
        /// <returns>The stage that drives the NPC's dialogue.</returns>
        public static ClassTrialStage Resolve(bool isNovice, bool hasCompletedTrial, bool hasActiveTrial, bool canEvolve)
        {
            if (!isNovice)
            {
                return ClassTrialStage.AlreadyPromoted;
            }

            if (hasCompletedTrial)
            {
                return ClassTrialStage.ClaimPromotion;
            }

            if (hasActiveTrial)
            {
                return ClassTrialStage.TrialActive;
            }

            return canEvolve ? ClassTrialStage.ChooseTrial : ClassTrialStage.NotReady;
        }
    }
}
