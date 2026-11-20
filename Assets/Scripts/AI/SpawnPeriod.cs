namespace Project.AI
{
    /// <summary>
    /// When of the day and night cycle a spawn table entry may appear.
    /// </summary>
    public enum SpawnPeriod
    {
        /// <summary>The entry can spawn at any time.</summary>
        Always,

        /// <summary>The entry can only spawn during the day.</summary>
        Day,

        /// <summary>The entry can only spawn during the night.</summary>
        Night
    }
}
