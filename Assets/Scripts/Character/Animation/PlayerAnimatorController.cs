using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Translates player gameplay state into Animator parameter updates.
    /// Movement, combat and skill scripts call this instead of holding an
    /// <see cref="UnityEngine.Animator"/> reference directly, so every
    /// animation trigger name lives in one place.
    /// </summary>
    public class PlayerAnimatorController : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int AttackParameter = Animator.StringToHash("Attack");
        private static readonly int AttackRangedParameter = Animator.StringToHash("AttackRanged");
        private static readonly int CastParameter = Animator.StringToHash("Cast");
        private static readonly int IsDeadParameter = Animator.StringToHash("IsDead");

        [SerializeField] private Animator animator;

        /// <summary>
        /// Updates the Speed parameter that blends between the Idle and Run states.
        /// </summary>
        /// <param name="normalizedSpeed">0 when idle, 1 when moving at full speed.</param>
        public void SetMovementSpeed(float normalizedSpeed)
        {
            animator.SetFloat(SpeedParameter, normalizedSpeed);
        }

        /// <summary>
        /// Plays the close-range attack animation.
        /// </summary>
        public void TriggerAttack()
        {
            animator.SetTrigger(AttackParameter);
        }

        /// <summary>
        /// Plays the ranged attack animation (e.g. drawing and firing a bow).
        /// </summary>
        public void TriggerRangedAttack()
        {
            animator.SetTrigger(AttackRangedParameter);
        }

        /// <summary>
        /// Plays the spell cast animation.
        /// </summary>
        public void TriggerCast()
        {
            animator.SetTrigger(CastParameter);
        }

        /// <summary>
        /// Enters or leaves the death animation. Set to true when the
        /// player dies and back to false on respawn, rather than a
        /// one-shot trigger, since the pose needs to hold for the whole
        /// respawn countdown instead of returning to Idle on its own.
        /// </summary>
        /// <param name="isDead">Whether the player is currently dead.</param>
        public void SetIsDead(bool isDead)
        {
            animator.SetBool(IsDeadParameter, isDead);
        }
    }
}
