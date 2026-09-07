using UnityEngine;

namespace Project.NPC
{
    /// <summary>
    /// Drives an NPC's Animator between an always-looping Idle state and a
    /// one-shot Interact state, played when the player opens or closes this
    /// NPC's shop. <see cref="PlayInteract"/> only fires the trigger while
    /// the Animator is already in Idle, so a request that arrives mid-bow is
    /// simply dropped rather than restarting or queuing the animation — the
    /// Interact clip always plays to completion before another can start.
    /// </summary>
    public class NpcAnimationController : MonoBehaviour
    {
        private static readonly int InteractTrigger = Animator.StringToHash("Interact");
        private const string IdleStateName = "Idle";

        [SerializeField] private Animator animator;

        /// <summary>
        /// Requests the Interact animation, ignored unless the NPC is
        /// currently idle and not mid-transition.
        /// </summary>
        public void PlayInteract()
        {
            if (animator == null || animator.IsInTransition(0))
            {
                return;
            }

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(IdleStateName))
            {
                return;
            }

            animator.SetTrigger(InteractTrigger);
        }
    }
}
