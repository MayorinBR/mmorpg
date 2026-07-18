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
        Equipment
    }
}