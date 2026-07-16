using System.Collections.Generic;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Defines the possible drops for an enemy type. Each entry rolls
    /// independently, so an enemy can drop multiple items from a single death.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "Project/Items/Loot Table")]
    public class LootTableDefinition : ScriptableObject
    {
        [SerializeField] private LootEntry[] entries;

        /// <summary>
        /// Rolls each entry independently against its drop chance.
        /// </summary>
        /// <returns>The items and quantities that dropped from this roll.</returns>
        public IReadOnlyList<(ItemDefinition item, int quantity)> RollDrops()
        {
            var drops = new List<(ItemDefinition, int)>();

            foreach (var entry in entries)
            {
                if (entry.Item == null || Random.value > entry.DropChance)
                {
                    continue;
                }

                var quantity = Random.Range(entry.MinQuantity, entry.MaxQuantity + 1);

                if (quantity > 0)
                {
                    drops.Add((entry.Item, quantity));
                }
            }

            return drops;
        }
    }
}