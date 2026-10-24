namespace Project.Character.Stats
{
    /// <summary>
    /// A monster's element, matching Ragnarok Online's ten elemental properties. Stored on
    /// <see cref="CharacterStatsDefinition.Element"/> so elemental damage tables can read it.
    /// <see cref="Neutral"/> is first so assets authored before this field existed default to it.
    /// New values are always appended at the end, because authored assets save them as raw numbers.
    /// </summary>
    public enum MonsterElement
    {
        Neutral,
        Water,
        Earth,
        Fire,
        Wind,
        Poison,
        Holy,
        Shadow,
        Ghost,
        Undead
    }
}
