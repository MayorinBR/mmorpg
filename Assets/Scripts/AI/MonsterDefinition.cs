using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Items;

namespace Project.AI
{
    /// <summary>
    /// A monster species in one asset: its combat stats (inherited from
    /// <see cref="CharacterStatsDefinition"/>, so it plugs into
    /// <see cref="CharacterStatsHolder"/> unchanged) plus the items it can
    /// drop on death. Each drop rolls independently, so a single kill can
    /// yield several items.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMonster", menuName = "Project/AI/Monster")]
    public class MonsterDefinition : CharacterStatsDefinition
    {
        [SerializeField] private LootEntry[] drops;

        /// <summary>Gets the possible drops of this monster.</summary>
        public IReadOnlyList<LootEntry> Drops => drops;

        /// <summary>
        /// Rolls every drop entry independently against its drop chance.
        /// </summary>
        /// <returns>The items and quantities that dropped from this roll.</returns>
        public IReadOnlyList<(ItemDefinition item, int quantity)> RollDrops()
        {
            var result = new List<(ItemDefinition, int)>();

            if (drops == null)
            {
                return result;
            }

            foreach (var entry in drops)
            {
                if (entry.Item == null || Random.value > entry.DropChance)
                {
                    continue;
                }

                var quantity = Random.Range(entry.MinQuantity, entry.MaxQuantity + 1);

                if (quantity > 0)
                {
                    result.Add((entry.Item, quantity));
                }
            }

            return result;
        }
    }
}
