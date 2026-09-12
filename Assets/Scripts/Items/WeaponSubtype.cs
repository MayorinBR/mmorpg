namespace Project.Items
{
    /// <summary>
    /// The specific category of weapon within a broader <see cref="WeaponType"/>,
    /// covering every weapon type on the iRO Wiki (https://irowiki.org/wiki/Weapons),
    /// plus <see cref="Shield"/> for the one piece of off-hand equipment
    /// that behaves like a weapon slot-wise without dealing damage. Used
    /// by weapon-mastery passive skills to check whether their bonus
    /// applies to the currently equipped main-hand weapon — see
    /// <see cref="Character.Combat.PlayerPassiveSkillController"/> and
    /// <see cref="Skills.SkillDefinition.PassiveRequiredWeaponSubtypes"/>.
    /// Only meaningful for items whose <see cref="ItemDefinition.ItemType"/>
    /// is Equipment and that occupy a hand slot; unarmed (or a non-weapon
    /// hand item) is <see cref="Unarmed"/>. New values are always appended
    /// at the end — existing values are saved as raw numbers in every
    /// authored item and skill asset, so reordering would silently
    /// reassign them.
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
        Book,
        Instrument,
        Whip,
        Rod,
        Knuckle,
        Katar,
        Gun,
        HuumaShuriken,

        /// <summary>
        /// Off-hand equipment with no attack of its own. Authored with
        /// <see cref="ItemDefinition.RequiredSlots"/> set to just
        /// <see cref="EquipmentSlot.RightHand"/> — no special-case code
        /// needed: <c>EquipmentManager</c> already evicts whatever
        /// currently occupies a slot being claimed, so a two-handed
        /// weapon displaces an equipped shield and vice versa, and a
        /// shield displaces an off-hand dagger the same way one off-hand
        /// weapon replaces another.
        /// </summary>
        Shield
    }
}
