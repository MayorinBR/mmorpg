namespace Project.Character.Stats
{
    /// <summary>
    /// A monster's size class, matching Ragnarok Online's own Small/Medium/Large
    /// classification — read by <see cref="Items.WeaponSizeModifiers"/> to scale
    /// physical damage by the attacker's weapon type (e.g. daggers deal full
    /// damage to Small monsters but only half to Large ones). Also present
    /// (via <see cref="CharacterStatsDefinition.Size"/>) on the player's own
    /// stats, where it's simply never read by anything, since players aren't
    /// sized in Ragnarok Online. <see cref="Medium"/> is first so a
    /// pre-existing <see cref="CharacterStatsDefinition"/> asset authored
    /// before this field existed defaults to it — Unity falls back to a new
    /// field's C# default (0) when deserializing an asset that predates it.
    /// New values are always appended at the end — existing values are saved
    /// as raw numbers in every authored asset, so reordering would silently
    /// reassign them.
    /// </summary>
    public enum MonsterSize
    {
        Medium,
        Small,
        Large
    }
}
