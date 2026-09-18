namespace Project.Character.Stats
{
    /// <summary>
    /// A monster's race classification, matching Ragnarok Online's own ten
    /// races and their real in-game numeric IDs — read by combat code that
    /// cares whether a target/attacker is Demon or Undead (e.g. Demon Bane,
    /// Divine Protection, Decrease AGI/Signum Crucis, Heal's Undead-damage
    /// interaction, and the Boss/Insect/Demon exception on Hiding
    /// detection). Also present (via <see cref="CharacterStatsDefinition.Race"/>)
    /// on the player's own stats, where it's simply never read by anything,
    /// mirroring how <see cref="MonsterSize"/> is unused there too.
    /// <see cref="Formless"/> is first (matching real RO's own race ID 0)
    /// so a pre-existing <see cref="CharacterStatsDefinition"/> asset
    /// authored before this field existed defaults to it — Unity falls
    /// back to a new field's C# default (0) when deserializing an asset
    /// that predates it, and no race-conditional bonus in this project
    /// currently keys off Formless. New values are always appended at the
    /// end — existing values are saved as raw numbers in every authored
    /// asset, so reordering would silently reassign them.
    /// </summary>
    public enum MonsterRace
    {
        Formless,
        Undead,
        Brute,
        Plant,
        Insect,
        Fish,
        Demon,
        DemiHuman,
        Angel,
        Dragon
    }
}
