using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Detects when the mouse hovers over a world-space <see cref="ItemPickup"/>
    /// using a single per-frame raycast (new Input System compatible, unlike
    /// the legacy OnMouseEnter/OnMouseExit messages, which don't fire when
    /// Active Input Handling is set to Input System Package only). Lives in
    /// Project.UI (not alongside ItemPickup in Project.Items) so the Items
    /// assembly never needs to depend on the UI assembly.
    /// </summary>
    public class WorldItemHoverDetector : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask itemLayer;

        private ItemPickup currentHoveredItem;
        private bool isHovering;

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (IsPointerOverUI())
            {
                ClearHover();
                return;
            }

            var pointerPosition = Mouse.current.position.ReadValue();
            var ray = worldCamera.ScreenPointToRay(pointerPosition);

            if (Physics.Raycast(ray, out var hit, float.MaxValue, itemLayer) &&
                hit.collider.TryGetComponent(out ItemPickup pickup))
            {
                currentHoveredItem = pickup;
                isHovering = true;
                ItemTooltipUI.Instance?.Show(pickup.Item, pointerPosition);
                return;
            }

            ClearHover();
        }

        private void ClearHover()
        {
            if (!isHovering)
            {
                return;
            }

            isHovering = false;
            currentHoveredItem = null;
            ItemTooltipUI.Instance?.Hide();
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}