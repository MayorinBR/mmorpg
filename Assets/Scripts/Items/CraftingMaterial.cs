using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// A single material requirement within a <see cref="CraftingRecipe"/>:
    /// an item and how many units of it a craft consumes.
    /// </summary>
    [System.Serializable]
    public struct CraftingMaterial
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity;

        /// <summary>Gets the required material item.</summary>
        public ItemDefinition Item => item;

        /// <summary>Gets how many units of <see cref="Item"/> are required.</summary>
        public int Quantity => quantity;
    }
}
