namespace Project.Items
{
    /// <summary>
    /// Whether a weapon is used up close or from a distance. Only
    /// meaningful for items whose <see cref="ItemDefinition.ItemType"/> is
    /// Equipment and that occupy a hand slot. Used by
    /// <see cref="Character.Combat.PlayerCombatController"/> to decide
    /// whether the Archer's ammo mechanic applies to the currently
    /// equipped weapon.
    /// </summary>
    public enum WeaponType
    {
        Melee,
        Ranged
    }
}