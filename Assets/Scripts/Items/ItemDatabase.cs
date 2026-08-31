using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Resolves a stable string id to an <see cref="ItemDefinition"/> asset
    /// and back, using the asset's own name. Exists so the save system
    /// (which cannot serialize a direct ScriptableObject reference through
    /// JSON) can record and later look up "which item" without depending
    /// on any particular UI component's own list of known items. Mirrors
    /// <c>Project.Skills.SkillDatabase</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Project/Items/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] allItems;

        /// <summary>
        /// Gets the stable id for an item, currently its asset name.
        /// </summary>
        /// <param name="item">The item to get an id for.</param>
        /// <returns>The item's id, or an empty string if <paramref name="item"/> is null.</returns>
        public string GetId(ItemDefinition item)
        {
            return item != null ? item.name : string.Empty;
        }

        /// <summary>
        /// Finds the item asset with the given id.
        /// </summary>
        /// <param name="id">The id to look up, as returned by <see cref="GetId"/>.</param>
        /// <returns>The matching item, or null if not found or <paramref name="id"/> is empty.</returns>
        public ItemDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (var item in allItems)
            {
                if (item != null && item.name == id)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
