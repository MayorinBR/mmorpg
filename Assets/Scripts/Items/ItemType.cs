namespace Project.Items
{
    /// <summary>
    /// Broad category of an item, used to decide how it can be used
    /// (equipped, consumed, or only held for crafting/quests).
    /// </summary>
    public enum ItemType
    {
        Consumable,
        Material,
        Equipment,

        /// <summary>
        /// A unique quest item (e.g. a letter or token an NPC hands over).
        /// Cannot be equipped (excluded from <see cref="Equipment"/> equip
        /// checks) or sold (see <see cref="Project.NPC.NpcShopKeeper.TrySell"/>).
        /// Appended last so its serialized enum index never shifts the
        /// existing values on already-authored item assets.
        /// </summary>
        KeyItem
    }
}