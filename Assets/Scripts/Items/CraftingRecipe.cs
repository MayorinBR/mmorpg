using System.Collections.Generic;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Defines a crafting recipe: a result item/quantity produced from a
    /// set of required material items/quantities, consumed from the
    /// crafter's own inventory via <see cref="Inventory.TryCraft"/>.
    /// Instances are authored as assets, mirroring how skills and items are
    /// already defined as data in this project. Shared by every
    /// crafting-type skill (<see cref="Skills.SkillEffectType.Craft"/> —
    /// e.g. Arrow Crafting, Aqua Benedicta) rather than each carrying its
    /// own recipe fields, so the same crafting logic works for all of them.
    /// An empty <see cref="Materials"/> list is valid — e.g. Aqua
    /// Benedicta, whose only real cost is being cast near water rather
    /// than consuming an ingredient.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Project/Items/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [SerializeField] private ItemDefinition resultItem;
        [SerializeField] private int resultQuantity = 1;
        [SerializeField] private CraftingMaterial[] materials;

        /// <summary>Gets the item produced by this recipe.</summary>
        public ItemDefinition ResultItem => resultItem;

        /// <summary>Gets how many units of <see cref="ResultItem"/> this recipe produces per craft.</summary>
        public int ResultQuantity => resultQuantity;

        /// <summary>Gets the materials this recipe consumes per craft. May be empty.</summary>
        public IReadOnlyList<CraftingMaterial> Materials => materials;
    }
}
