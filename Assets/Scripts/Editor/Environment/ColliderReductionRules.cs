using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Decides whether a collider has any consumer at runtime.
    /// Gameplay physics queries only use layer masks (ground clicks, enemy, item and NPC picking, area overlaps)
    /// and no script listens to trigger or collision callbacks, so a solid collider outside those layers
    /// can be removed without changing behaviour.
    /// </summary>
    public static class ColliderReductionRules
    {
        private static readonly string[] GameplayLayerNames = { "Ground", "Water", "Player", "Enemy", "Item", "NPC" };

        private static readonly int GameplayMask = LayerMask.GetMask(GameplayLayerNames);

        /// <summary>
        /// Returns true when the collider is solid, is not a terrain or character collider
        /// and sits on a layer that no gameplay query reads.
        /// </summary>
        /// <param name="collider">Collider to evaluate.</param>
        /// <returns>True when the collider can be removed.</returns>
        public static bool IsRemovable(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }

            if (collider.isTrigger || collider is TerrainCollider || collider is CharacterController)
            {
                return false;
            }

            return (GameplayMask & (1 << collider.gameObject.layer)) == 0;
        }

        /// <summary>
        /// Returns true for a child object that existed only to carry a collider and is now empty.
        /// Only objects whose name contains "collider" qualify, so named anchors are never deleted.
        /// </summary>
        /// <param name="holder">Object that carried the removed collider.</param>
        /// <returns>True when the object can be deleted.</returns>
        public static bool IsEmptyColliderHolder(GameObject holder)
        {
            return holder.transform.parent != null
                && holder.transform.childCount == 0
                && holder.GetComponents<Component>().Length == 1
                && holder.name.IndexOf("collider", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
