using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Shows the player's inventory one fixed-size page at a time (matching
    /// <see cref="PageSize"/>), with Next/Back navigation between pages.
    /// New pages become reachable automatically as <see cref="Inventory"/>
    /// grows internally; this component never changes how many slot views
    /// exist, only which underlying inventory indices they display.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        private const int PageSize = 20;

        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private TMP_Text weightText;
        [SerializeField] private TMP_Text pageIndicatorText;

        private readonly InventorySlotUI[] slotViews = new InventorySlotUI[PageSize];
        private int currentPage;
        private bool isBuilt;

        private void OnEnable()
        {
            if (playerInventory != null && playerInventory.Items != null)
            {
                playerInventory.Items.InventoryChanged -= RefreshAll;
                playerInventory.Items.InventoryChanged += RefreshAll;

                if (isBuilt)
                {
                    RefreshAll();
                }
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null && playerInventory.Items != null)
            {
                playerInventory.Items.InventoryChanged -= RefreshAll;
            }
        }

        private void Start()
        {
            BuildSlotViews();
            WireNavigationButtons();

            playerInventory.Items.InventoryChanged -= RefreshAll;
            playerInventory.Items.InventoryChanged += RefreshAll;

            isBuilt = true;
            RefreshAll();
        }

        private void BuildSlotViews()
        {
            for (var i = 0; i < PageSize; i++)
            {
                var slotView = Instantiate(slotPrefab, slotsParent);
                slotView.Clicked += HandleSlotClicked;
                slotViews[i] = slotView;
            }
        }

        private void WireNavigationButtons()
        {
            if (nextPageButton != null)
            {
                nextPageButton.onClick.AddListener(GoToNextPage);
            }

            if (previousPageButton != null)
            {
                previousPageButton.onClick.AddListener(GoToPreviousPage);
            }
        }

        private void GoToNextPage()
        {
            currentPage++;
            RefreshAll();
        }

        private void GoToPreviousPage()
        {
            currentPage--;
            RefreshAll();
        }

        private void HandleSlotClicked(int inventoryIndex)
        {
            if (equipment != null)
            {
                equipment.TryEquipFromInventory(inventoryIndex);
            }
        }

        private void RefreshAll()
        {
            var totalPages = Mathf.Max(1, Mathf.CeilToInt((float)playerInventory.Items.SlotCount / PageSize));
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

            for (var i = 0; i < PageSize; i++)
            {
                var inventoryIndex = (currentPage * PageSize) + i;
                var slotData = inventoryIndex < playerInventory.Items.SlotCount
                    ? playerInventory.Items.GetSlot(inventoryIndex)
                    : InventorySlot.Empty;

                slotViews[i].SetIndex(inventoryIndex);
                slotViews[i].SetSlot(slotData);
            }

            if (previousPageButton != null)
            {
                previousPageButton.interactable = currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = currentPage < totalPages - 1;
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"{currentPage + 1}/{totalPages}";
            }

            if (weightText != null)
            {
                weightText.text = $"{playerInventory.Items.CurrentWeight:F1} / {playerInventory.Items.MaxCarryWeight:F1}";
            }
        }
    }
}