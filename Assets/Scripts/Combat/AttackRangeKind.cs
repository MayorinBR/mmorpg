namespace Project.Combat
{
    /// <summary>
    /// Whether an attack is melee or ranged, checked by a blocking zone
    /// (see <see cref="SkillZoneController.InitializeBlock"/>, e.g. Safety
    /// Wall blocking melee, Pneuma blocking ranged) via
    /// <see cref="SkillZoneController.BlocksAttack"/>. A separate enum from
    /// <see cref="Items.WeaponType"/> rather than reusing it directly:
    /// Project.Combat can't reference Project.Items without creating an
    /// assembly cycle (Project.Items already references Project.Combat).
    /// </summary>
    public enum AttackRangeKind
    {
        Melee,
        Ranged
    }
}
