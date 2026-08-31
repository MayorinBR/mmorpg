using UnityEngine;
using Project.Persistence;

namespace Project.Items
{
    /// <summary>
    /// Owns the player's <see cref="Inventory"/>. Acts as the composition
    /// root connecting the plain C# inventory class to the Unity component
    /// world, mirroring how <see cref="Character.Combat.PlayerStatsController"/>
    /// wraps <see cref="Character.Stats.CharacterBaseStats"/>.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private float maxCarryWeight = 50f;
        [SerializeField] private ItemDatabase itemDatabase;

        /// <summary>Gets the player's inventory.</summary>
        public Inventory Items { get; private set; }

        private void Awake()
        {
            Items = new Inventory(maxCarryWeight);
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.inventoryItemIds.Clear();
            data.inventoryQuantities.Clear();

            if (itemDatabase == null)
            {
                return;
            }

            for (var i = 0; i < Items.SlotCount; i++)
            {
                var slot = Items.GetSlot(i);
                data.inventoryItemIds.Add(slot.IsEmpty ? string.Empty : itemDatabase.GetId(slot.Item));
                data.inventoryQuantities.Add(slot.IsEmpty ? 0 : slot.Quantity);
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            if (itemDatabase == null)
            {
                return;
            }

            Items.EnsureSlotCount(data.inventoryItemIds.Count);

            for (var i = 0; i < data.inventoryItemIds.Count; i++)
            {
                var item = itemDatabase.FindById(data.inventoryItemIds[i]);

                if (item != null)
                {
                    Items.SetSlot(i, item, data.inventoryQuantities[i]);
                }
            }
        }
    }
}
