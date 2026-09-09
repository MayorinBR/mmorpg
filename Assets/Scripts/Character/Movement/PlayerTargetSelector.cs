using System;
using UnityEngine;
using Project.Combat;

namespace Project.Character.Movement
{
    /// <summary>
    /// Holds the player's currently selected combat target. Selection input
    /// (click, tab-target, etc.) is handled elsewhere and calls
    /// <see cref="SelectTarget"/>; this class only tracks the result.
    /// </summary>
    public class PlayerTargetSelector : MonoBehaviour
    {
        /// <summary>Raised whenever the selected target changes, including selection to null.</summary>
        public event Action<Transform> TargetChanged;

        /// <summary>Gets the transform of the currently selected target, or null if none is selected.</summary>
        public Transform CurrentTarget { get; private set; }

        /// <summary>Gets the damageable interface of the currently selected target, or null if none is selected.</summary>
        public IDamageable CurrentDamageable { get; private set; }

        /// <summary>
        /// Selects a new target from a hit collider.
        /// </summary>
        /// <remarks>
        /// <see cref="CurrentTarget"/> is resolved to the GameObject that
        /// owns <see cref="CurrentDamageable"/> — typically an ancestor of
        /// <paramref name="hitCollider"/> — rather than the collider's own
        /// transform. An enemy's collider commonly sits on a separate,
        /// purely cosmetic geometry child (e.g. one that bounces via its
        /// own animation), while the damageable/AI root stays put; anything
        /// that follows <see cref="CurrentTarget"/> (such as
        /// <see cref="Project.UI.GroundRingFollower"/>) would otherwise
        /// drift away from the enemy's actual position as that animation
        /// plays. Falls back to the collider's own transform if it isn't
        /// parented under an <see cref="IDamageable"/> at all.
        /// </remarks>
        /// <param name="hitCollider">The collider that was clicked or otherwise targeted.</param>
        public void SelectTarget(Collider hitCollider)
        {
            CurrentDamageable = hitCollider.GetComponentInParent<IDamageable>();
            CurrentTarget = (CurrentDamageable as Component)?.transform ?? hitCollider.transform;
            TargetChanged?.Invoke(CurrentTarget);
        }

        /// <summary>
        /// Clears the current target, typically called when it dies or moves out of relevance.
        /// </summary>
        public void ClearTarget()
        {
            CurrentTarget = null;
            CurrentDamageable = null;
            TargetChanged?.Invoke(null);
        }
    }
}