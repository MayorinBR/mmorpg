using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Defines a single item type. Instances are authored as assets and
    /// referenced by inventory slots and loot tables, so all systems read
    /// identical item data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Project/Items/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;

        /// <summary>Gets the display name of the item.</summary>
        public string ItemName => itemName;

        /// <summary>Gets the item's description text.</summary>
        public string Description => description;

        /// <summary>Gets the icon shown in inventory UI.</summary>
        public Sprite Icon => icon;

        /// <summary>Gets a value indicating whether multiple units of this item can share a single slot.</summary>
        public bool IsStackable => isStackable;

        /// <summary>Gets the maximum quantity allowed in a single stack.</summary>
        public int MaxStackSize => maxStackSize;
    }
}