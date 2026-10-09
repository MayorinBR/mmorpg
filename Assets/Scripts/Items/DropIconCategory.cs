namespace Project.Items
{
    /// <summary>
    /// The small world-icon group an item belongs to when dropped on the
    /// ground. Items of the same category share one icon (all one-handed
    /// swords look alike on the floor), while the detailed
    /// <see cref="ItemDefinition.Icon"/> stays unique per item. New values
    /// are always appended at the end so serialized values never shift.
    /// </summary>
    public enum DropIconCategory
    {
        /// <summary>Resolve the category from the item's type, slot and weapon subtype.</summary>
        Auto,
        Dagger,
        OneHandSword,
        TwoHandSword,
        Axe,
        Mace,
        Spear,
        Bow,
        Rod,
        Book,
        Instrument,
        Whip,
        Knuckle,
        Katar,
        Gun,
        Shuriken,
        Shield,
        Headgear,
        Armor,
        Legs,
        Boots,
        Gloves,
        Accessory,
        Ammo,
        Potion,
        Food,
        Material,
        KeyItem,
        Generic
    }
}
