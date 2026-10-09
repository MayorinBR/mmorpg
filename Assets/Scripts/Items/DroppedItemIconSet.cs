using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Maps each <see cref="DropIconCategory"/> to the small sprite shown
    /// when an item of that category lies on the ground. Keeps the world
    /// icon per item type rather than per item, so a new sword needs no
    /// extra art. Falls back to the item's detailed icon when a category
    /// has no sprite assigned.
    /// </summary>
    [CreateAssetMenu(fileName = "DroppedItemIconSet", menuName = "Project/Items/Dropped Item Icon Set")]
    public class DroppedItemIconSet : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public DropIconCategory category;
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<DropIconCategory, Sprite> lookup;

        /// <summary>
        /// Gets the ground icon for the given item.
        /// </summary>
        /// <param name="item">The dropped item.</param>
        /// <returns>The sprite for the item's category, then the Generic sprite, then the item's own icon; null when item is null.</returns>
        public Sprite GetIcon(ItemDefinition item)
        {
            if (item == null)
            {
                return null;
            }

            EnsureLookup();

            if (lookup.TryGetValue(item.DropCategory, out var sprite) && sprite != null)
            {
                return sprite;
            }

            if (lookup.TryGetValue(DropIconCategory.Generic, out var generic) && generic != null)
            {
                return generic;
            }

            return item.Icon;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<DropIconCategory, Sprite>();

            foreach (var entry in entries)
            {
                lookup[entry.category] = entry.sprite;
            }
        }

        private void OnValidate()
        {
            lookup = null;
        }
    }
}
