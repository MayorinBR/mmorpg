using System.Collections;
using UnityEngine;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Applies a consumable item's effect when the player uses it from
    /// their inventory — restoring health/mana instantly, or gradually
    /// over time — then removes one unit of the item. Self-guards on death
    /// the same way <see cref="PlayerSkillCaster"/> and
    /// <see cref="Items.EquipmentManager"/> do, since this is reachable
    /// directly from a UI click rather than only from <c>Update</c>.
    /// </summary>
    public class PlayerConsumableUser : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private PlayerInventory inventory;

        /// <summary>
        /// Attempts to use the consumable item held at the given inventory
        /// slot. Does nothing if the slot is empty, the item isn't a
        /// consumable, or the player is dead.
        /// </summary>
        /// <param name="inventoryIndex">The inventory slot to consume from.</param>
        /// <returns>True if the item was consumed and its effect applied.</returns>
        public bool TryConsume(int inventoryIndex)
        {
            if (health != null && health.IsDead)
            {
                return false;
            }

            var slot = inventory.Items.GetSlot(inventoryIndex);

            if (slot.IsEmpty || slot.Item.ItemType != ItemType.Consumable)
            {
                return false;
            }

            var item = slot.Item;

            if (!inventory.Items.TryRemoveFromSlot(inventoryIndex, 1))
            {
                return false;
            }

            ApplyEffect(item);
            return true;
        }

        private void ApplyEffect(ItemDefinition item)
        {
            PlayerFeedbackChannel.Publish($"Used {item.ItemName}.");

            if (item.ConsumableEffectType == ConsumableEffectType.Instant)
            {
                RestoreOnce(item.HealthRestore, item.ManaRestore);
                return;
            }

            StartCoroutine(ApplyOverTime(item));
        }

        private IEnumerator ApplyOverTime(ItemDefinition item)
        {
            var elapsed = 0f;

            while (elapsed < item.EffectDurationSeconds)
            {
                yield return new WaitForSeconds(item.TickIntervalSeconds);
                elapsed += item.TickIntervalSeconds;

                if (health != null && health.IsDead)
                {
                    yield break;
                }

                RestoreOnce(item.HealthRestore, item.ManaRestore);
            }
        }

        private void RestoreOnce(int healthAmount, int manaAmount)
        {
            if (healthAmount > 0)
            {
                health?.Heal(healthAmount);
            }

            if (manaAmount > 0)
            {
                mana?.RestoreMana(manaAmount);
            }
        }
    }
}
