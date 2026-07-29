using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    /// <summary>
    /// Displays the player's currently equipped ammo as an icon and count,
    /// refreshing from <see cref="EquipmentManager.AmmoCountChanged"/>
    /// (consumption) and <see cref="EquipmentManager.EquipmentChanged"/>
    /// (equipping, unequipping, or depleting to zero and auto-unequipping).
    /// Shows nothing when no ammo is equipped, since the Archer's base
    /// shot fallback doesn't need a counter.
    /// </summary>
    public class AmmoCounterUI : MonoBehaviour
    {
        [SerializeField] private Items.EquipmentManager equipment;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text countText;

        private void OnEnable()
        {
            equipment.AmmoCountChanged += Refresh;
            equipment.EquipmentChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            equipment.AmmoCountChanged -= Refresh;
            equipment.EquipmentChanged -= Refresh;
        }

        private void Refresh()
        {
            var equippedAmmo = equipment.GetEquippedItems(Items.EquipmentSlot.Ammo);

            if (equippedAmmo.Count == 0)
            {
                iconImage.enabled = false;
                countText.text = string.Empty;
                return;
            }

            iconImage.enabled = true;
            iconImage.sprite = equippedAmmo[0].Icon;
            countText.text = equipment.EquippedAmmoCount.ToString();
        }
    }
}