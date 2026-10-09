namespace Project.Items
{
    /// <summary>
    /// A slot an equipment item can occupy. Only one item can be equipped
    /// per slot at a time.
    /// </summary>
    public enum EquipmentSlot
    {
        HeadTop,
        HeadFace,
        Body,
        LeftHand,
        RightHand,
        Pants,
        Boots,
        Accessory,
        Ammo,

        /// <summary>Hand armor worn over the character's hands.</summary>
        Gloves
    }
}