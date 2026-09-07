using System;
using System.Collections.Generic;
using UnityEngine;
using Project.Combat;
using Project.Items;

namespace Project.NPC
{
    /// <summary>
    /// A merchant NPC that sells a fixed assortment of items for Zeny and
    /// buys any item back from the player at that item's own configured
    /// sell price. The first concrete NPC behavior in the project — other
    /// NPC roles (quest giver, dialogue-only, etc.) are expected to become
    /// separate components placed alongside this one rather than growing
    /// this class to cover every kind of NPC.
    /// </summary>
    public class NpcShopKeeper : MonoBehaviour
    {
        [SerializeField] private string shopName = "Shop";
        [SerializeField] private ItemDefinition[] itemsForSale;
        [SerializeField] private NpcAnimationController animationController;

        /// <summary>Gets the name shown as this shop's window title.</summary>
        public string ShopName => shopName;

        /// <summary>Gets the items this NPC currently has for sale.</summary>
        public IReadOnlyList<ItemDefinition> ItemsForSale => itemsForSale;

        /// <summary>Gets the controller for this NPC's Idle/Interact animations, if assigned.</summary>
        public NpcAnimationController AnimationController => animationController;

        /// <summary>
        /// Attempts to buy a quantity of an item this shop sells, spending
        /// Zeny from the buyer and adding the items to their inventory.
        /// Verifies both the price and the inventory's carry-weight
        /// capacity before spending anything, so a purchase either fully
        /// succeeds or leaves the buyer untouched.
        /// </summary>
        /// <param name="item">The item to buy. Must be one this shop sells.</param>
        /// <param name="quantity">How many units to buy. Must be positive.</param>
        /// <param name="buyerInventory">The buyer's inventory.</param>
        /// <param name="buyerCurrency">The buyer's currency wallet.</param>
        /// <returns>True if the purchase succeeded.</returns>
        public bool TryBuy(ItemDefinition item, int quantity, PlayerInventory buyerInventory, ICurrencyWallet buyerCurrency)
        {
            if (item == null || quantity <= 0 || Array.IndexOf(itemsForSale, item) < 0)
            {
                return false;
            }

            if (buyerInventory.Items.GetAddableQuantity(item, quantity) < quantity)
            {
                PlayerFeedbackChannel.Publish($"Not enough carry weight for {quantity}x {item.ItemName}.");
                return false;
            }

            var totalPrice = item.BuyPrice * quantity;

            if (!buyerCurrency.TrySpend(totalPrice))
            {
                PlayerFeedbackChannel.Publish($"Not enough Zeny for {quantity}x {item.ItemName}.");
                return false;
            }

            buyerInventory.Items.TryAddItem(item, quantity);
            return true;
        }

        /// <summary>
        /// Attempts to sell a quantity of the item held at a seller's
        /// inventory slot, removing it and paying the seller Zeny based on
        /// the item's own sell price.
        /// </summary>
        /// <param name="sellerInventorySlotIndex">The seller's inventory slot to sell from.</param>
        /// <param name="quantity">How many units to sell.</param>
        /// <param name="sellerInventory">The seller's inventory.</param>
        /// <param name="sellerCurrency">The seller's currency wallet.</param>
        /// <returns>True if the sale succeeded.</returns>
        public bool TrySell(int sellerInventorySlotIndex, int quantity, PlayerInventory sellerInventory, ICurrencyWallet sellerCurrency)
        {
            var slot = sellerInventory.Items.GetSlot(sellerInventorySlotIndex);

            if (slot.IsEmpty || quantity <= 0 || quantity > slot.Quantity)
            {
                return false;
            }

            var totalPrice = slot.Item.SellPrice * quantity;

            if (!sellerInventory.Items.TryRemoveFromSlot(sellerInventorySlotIndex, quantity))
            {
                return false;
            }

            sellerCurrency.Add(totalPrice);
            return true;
        }
    }
}
