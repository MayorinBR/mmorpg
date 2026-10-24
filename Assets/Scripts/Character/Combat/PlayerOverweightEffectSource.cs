using System.Collections.Generic;
using UnityEngine;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Reports the overweight condition of the player's inventory as a status
    /// effect: the minor level stops HP and SP regeneration, the major level
    /// also blocks attacking and casting.
    /// </summary>
    public class PlayerOverweightEffectSource : MonoBehaviour, IActiveEffectSource
    {
        [Tooltip("The player's inventory. Resolved from the player hierarchy when left empty.")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Overrides the name, description and icon of the overweight conditions.")]
        [SerializeField] private StatusEffectPresentation presentation;

        private void Awake()
        {
            if (inventory == null)
            {
                inventory = transform.root.GetComponentInChildren<PlayerInventory>(true);
            }
        }

        /// <inheritdoc />
        public void CollectActiveEffects(List<ActiveEffect> into)
        {
            if (inventory == null || !inventory.Items.IsMinorOverweight)
            {
                return;
            }

            var id = inventory.Items.IsMajorOverweight ? StatusEffectIds.OverweightMajor : StatusEffectIds.OverweightMinor;
            var detail = $"Carrying {Mathf.RoundToInt(inventory.Items.WeightRatio * 100f)}% of your weight limit";
            into.Add(new ActiveEffect(StatusEffectPresentation.Resolve(presentation, id), 0f, 0f, detail));
        }
    }
}
