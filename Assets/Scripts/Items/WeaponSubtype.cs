namespace Project.Items
{
    /// <summary>
    /// The specific category of weapon within a broader <see cref="WeaponType"/>,
    /// covering classic Ragnarok Online's weapon categories for the six
    /// base classes currently in this project (Dagger/One-Hand Sword/
    /// Two-Hand Sword for Swordman, Axe/Mace for Merchant and Acolyte,
    /// Bow for Archer, Book for Mage). Used by weapon-mastery passive
    /// skills to check whether their bonus applies to the currently
    /// equipped main-hand weapon — see
    /// <see cref="Character.Combat.PlayerPassiveSkillController"/> and
    /// <see cref="Skills.SkillDefinition.PassiveRequiredWeaponSubtypes"/>.
    /// Only meaningful for items whose <see cref="ItemDefinition.ItemType"/>
    /// is Equipment and that occupy a hand slot; unarmed (or a non-weapon
    /// hand item) is <see cref="Unarmed"/>.
    /// </summary>
    public enum WeaponSubtype
    {
        Unarmed,
        Dagger,
        OneHandSword,
        TwoHandSword,
        OneHandAxe,
        TwoHandAxe,
        Mace,
        Spear,
        Bow,
        Book
    }
}
