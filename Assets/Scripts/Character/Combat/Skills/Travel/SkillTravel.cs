namespace Project.Character.Combat
{
    /// <summary>
    /// Entry point travel skills use to reach the services that live outside the combat assembly.
    /// Each service is registered by the system that implements it.
    /// </summary>
    public static class SkillTravel
    {
        /// <summary>Gets or sets the service that moves the player.</summary>
        public static ISkillTravelService Service { get; set; }

        /// <summary>Gets or sets the menu used to ask the player for a destination.</summary>
        public static IChoicePrompt Prompt { get; set; }
    }
}
