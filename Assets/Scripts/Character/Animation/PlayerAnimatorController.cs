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
        private static readonly int AttackSpeedMultiplierParameter = Animator.StringToHash("AttackSpeedMultiplier");

        [SerializeField] private Animator animator;

        [Tooltip("The exact clip bound to the Attack state's Motion field in the Animator Controller — read only for its authored AnimationClip.length, so SetAttackDuration can scale it to match the real Aspd interval regardless of that length.")]
        [SerializeField] private AnimationClip meleeAttackClip;

        [Tooltip("The exact clip bound to the AttackRanged state's Motion field, for the same reason as meleeAttackClip.")]
        [SerializeField] private AnimationClip rangedAttackClip;

        /// <summary>
        /// Retargets this controller at a different <see cref="Animator"/> —
        /// used when the player's model changes at runtime (a gender swap:
        /// see <see cref="Combat.PlayerGenderController"/>), since a single
        /// Animator component can only ever point at one Avatar, so each
        /// gendered model needs its own. Every method below simply acts on
        /// whichever Animator was set most recently.
        /// </summary>
        /// <param name="newAnimator">The Animator component now driving the player's visible model.</param>
        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        /// <summary>
        /// Updates the Speed parameter that blends between the Idle and Run states.
        /// </summary>
        /// <param name="normalizedSpeed">0 when idle, 1 when moving at full speed.</param>
        public void SetMovementSpeed(float normalizedSpeed)
        {
            animator.SetFloat(SpeedParameter, normalizedSpeed);
        }

        /// <summary>
        /// Sets the auto-attack swing's playback speed so its real duration
        /// matches <paramref name="targetSeconds"/> — the same Aspd-derived
        /// interval the next auto-attack actually waits for (see
        /// <see cref="Combat.AttackSpeedCalculator.GetAttackIntervalSeconds"/>)
        /// — regardless of the clip's own authored length or the Attack/
        /// AttackRanged state's base Speed in the Animator Controller (both
        /// bound to this via the AttackSpeedMultiplier parameter, so this
        /// has no effect on Idle, Run, Cast or Death). Call before
        /// <see cref="TriggerAttack"/>/<see cref="TriggerRangedAttack"/> so
        /// the upcoming swing plays at the right speed.
        /// </summary>
        /// <param name="targetSeconds">How long the swing should actually take to play, in seconds.</param>
        /// <param name="isRanged">True to scale <see cref="rangedAttackClip"/>; false for <see cref="meleeAttackClip"/>.</param>
        public void SetAttackDuration(float targetSeconds, bool isRanged)
        {
            var clip = isRanged ? rangedAttackClip : meleeAttackClip;
            var multiplier = clip != null && targetSeconds > 0f ? clip.length / targetSeconds : 1f;
            animator.SetFloat(AttackSpeedMultiplierParameter, multiplier);
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
