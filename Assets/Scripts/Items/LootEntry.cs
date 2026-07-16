using System;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// A single possible drop: which item, the chance it drops, and the
    /// quantity range if it does.
    /// </summary>
    [Serializable]
    public struct LootEntry
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField][Range(0f, 1f)] private float dropChance;
        [SerializeField] private int minQuantity;
        [SerializeField] private int maxQuantity;

        /// <summary>Gets the item this entry may drop.</summary>
        public ItemDefinition Item => item;

        /// <summary>Gets the probability (0 to 1) that this entry drops.</summary>
        public float DropChance => dropChance;

        /// <summary>Gets the minimum quantity dropped, inclusive.</summary>
        public int MinQuantity => minQuantity;

        /// <summary>Gets the maximum quantity dropped, inclusive.</summary>
        public int MaxQuantity => maxQuantity;
    }
}