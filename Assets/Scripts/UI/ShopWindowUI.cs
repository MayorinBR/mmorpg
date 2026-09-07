using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Character.Combat;
using Project.Character.Movement;
using Project.Items;
using Project.NPC;

namespace Project.UI
{
    /// <summary>
    /// Shows a merchant NPC's buy list next to the player's sell tray.
    /// <see cref="Open"/> and <see cref="Close"/> are called directly by
    /// <see cref="PlayerUIController"/> rather than this component
    /// subscribing to <see cref="PlayerNpcInteractionController.ShopOpened"/>
    /// itself: this window's GameObject starts inactive, and Unity never
    /// runs Awake on an inactive GameObject, so it could never register
    /// that subscription on its own. Closes itself automatically as soon
    /// as the player starts moving again, via
    /// <see cref="CharacterMovementController.MovementStarted"/> — that
    /// subscription lives in <see cref="Awake"/>/<see cref="OnDestroy"/>
    /// rather than OnEnable/OnDisable because this component shares its
    /// GameObject with the <see cref="WindowPanel"/> that toggles that
    /// GameObject's active state (OnEnable would drop it for good the
    /// first time the window closes), and it's safe here because by the
    /// time the window can close, it has already opened once, so Awake has
    /// already run. The sell tray starts empty on every shop visit; the
    /// player fills its slots by dragging items from their inventory, then
    /// sells everything queued in one click.
    /// </summary>
    public class ShopWindowUI : MonoBehaviour
    {
        private const int SellTraySlotCount = 8;

        [SerializeField] private WindowPanel windowPanel;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerCurrency playerCurrency;
        [SerializeField] private ShopBuyEntryUI buyEntryPrefab;
        [SerializeField] private Transform buyListParent;
        [SerializeField] private ScrollRect buyListScrollRect;

        [SerializeField] private ShopSellEntryUI sellEntryPrefab;
        [SerializeField] private Transform sellListParent;
        [SerializeField] private Button sellAllButton;

        private readonly List<ShopBuyEntryUI> buyEntryViews = new List<ShopBuyEntryUI>();
        private readonly ShopSellEntryUI[] sellTraySlots = new ShopSellEntryUI[SellTraySlotCount];

        private NpcShopKeeper currentShop;
        private bool sellTrayBuilt;

        /// <summary>Raised whenever this window closes, for any reason. Forwards <see cref="WindowPanel.Closed"/> without exposing the panel itself.</summary>
        public event Action Closed
        {
            add => windowPanel.Closed += value;
            remove => windowPanel.Closed -= value;
        }

        private void Awake()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
                movementController.MovementStarted += HandleMovementStarted;
            }

            if (sellAllButton != null)
            {
                sellAllButton.onClick.AddListener(HandleSellAllClicked);
            }
        }

        private void OnDestroy()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
            }
        }

        /// <summary>
        /// Opens this window for the given shop. Called directly by
        /// <see cref="PlayerUIController"/> — see the class summary for why
        /// this can't be a self-registered event subscription instead.
        /// </summary>
        /// <param name="shop">The merchant NPC whose stock and prices to show.</param>
        public void Open(NpcShopKeeper shop)
        {
            currentShop = shop;

            windowPanel.Open();
            windowPanel.SetTitle(shop.ShopName);

            BuildBuyList();

            if (buyListScrollRect != null)
            {
                buyListScrollRect.verticalNormalizedPosition = 1f;
            }

            EnsureSellTrayBuilt();
            ClearSellTray();
        }

        /// <summary>Closes this window, if open.</summary>
        public void Close()
        {
            windowPanel.Close();
        }

        private void BuildBuyList()
        {
            foreach (var view in buyEntryViews)
            {
                Destroy(view.gameObject);
            }

            buyEntryViews.Clear();

            foreach (var item in currentShop.ItemsForSale)
            {
                var view = Instantiate(buyEntryPrefab, buyListParent);
                view.Setup(item, HandleBuyClicked);
                buyEntryViews.Add(view);
            }
        }

        private void EnsureSellTrayBuilt()
        {
            if (sellTrayBuilt)
            {
                return;
            }

            for (var i = 0; i < SellTraySlotCount; i++)
            {
                var view = Instantiate(sellEntryPrefab, sellListParent);
                view.Init(HandleItemDroppedOnTraySlot);
                sellTraySlots[i] = view;
            }

            sellTrayBuilt = true;
        }

        private void ClearSellTray()
        {
            foreach (var traySlot in sellTraySlots)
            {
                traySlot.Clear();
            }
        }

        private void HandleItemDroppedOnTraySlot(ShopSellEntryUI traySlot, ItemDefinition item)
        {
            if (traySlot.HasItem || IsAlreadyQueued(item))
            {
                return;
            }

            var totalOwned = GetTotalQuantityOwned(item);

            if (totalOwned <= 0)
            {
                return;
            }

            traySlot.Assign(item, totalOwned);
        }

        private bool IsAlreadyQueued(ItemDefinition item)
        {
            foreach (var traySlot in sellTraySlots)
            {
                if (traySlot.HasItem && traySlot.Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sums how many units of an item the player holds across every
        /// inventory slot, so a non-stackable item that occupies one slot
        /// per unit can still be sold in bulk from a single tray slot.
        /// </summary>
        /// <param name="item">The item to total up.</param>
        /// <returns>The combined quantity across every matching slot.</returns>
        private int GetTotalQuantityOwned(ItemDefinition item)
        {
            var total = 0;

            for (var i = 0; i < playerInventory.Items.SlotCount; i++)
            {
                var slot = playerInventory.Items.GetSlot(i);

                if (!slot.IsEmpty && slot.Item == item)
                {
                    total += slot.Quantity;
                }
            }

            return total;
        }

        private void HandleMovementStarted()
        {
            if (windowPanel.IsOpen)
            {
                Close();
            }
        }

        private void HandleBuyClicked(ItemDefinition item, int quantity)
        {
            currentShop.TryBuy(item, quantity, playerInventory, playerCurrency);
        }

        private void HandleSellAllClicked()
        {
            if (currentShop == null)
            {
                return;
            }

            foreach (var traySlot in sellTraySlots)
            {
                if (!traySlot.HasItem)
                {
                    continue;
                }

                SellFromInventory(traySlot.Item, traySlot.Quantity);
                traySlot.Clear();
            }
        }

        /// <summary>
        /// Sells up to the requested quantity of an item, drawing from
        /// however many inventory slots hold it — one sale per slot, since
        /// <see cref="NpcShopKeeper.TrySell"/> only knows how to remove from
        /// a single slot at a time. Re-reads the live inventory rather than
        /// trusting what was queued at drag time, so anything the player
        /// already spent, equipped or sold elsewhere between then and now
        /// is simply skipped instead of oversold.
        /// </summary>
        /// <param name="item">The item type to sell.</param>
        /// <param name="quantity">The total quantity requested, across all of the player's slots.</param>
        private void SellFromInventory(ItemDefinition item, int quantity)
        {
            var remaining = quantity;

            for (var i = 0; i < playerInventory.Items.SlotCount && remaining > 0; i++)
            {
                var slot = playerInventory.Items.GetSlot(i);

                if (slot.IsEmpty || slot.Item != item)
                {
                    continue;
                }

                var amountFromSlot = Mathf.Min(remaining, slot.Quantity);

                if (currentShop.TrySell(i, amountFromSlot, playerInventory, playerCurrency))
                {
                    remaining -= amountFromSlot;
                }
            }
        }
    }
}
