namespace Project.Ambience
{
    /// <summary>
    /// Smoke intensity of the chimneys of one building.
    /// </summary>
    public enum ChimneySmokeLevel
    {
        /// <summary>No smoke, the hearth is cold.</summary>
        Off = 0,

        /// <summary>A thin, pale wisp, like steam or a banked fire.</summary>
        Light = 1,

        /// <summary>A thick, darker column, like a roaring fire.</summary>
        Heavy = 2,
    }
}
