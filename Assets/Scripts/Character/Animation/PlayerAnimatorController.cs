using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        private static readonly int CastParameter = Animator.StringToHash("Cast");
        private static readonly int IsChannelingParameter = Animator.StringToHash("IsChanneling");
        private static readonly int HitParameter = Animator.StringToHash("Hit");
        private static readonly int ChannelSpeedParameter = Animator.StringToHash("ChannelSpeed");
        private const string ChannelStateName = "Channel";
        private static readonly int IsDeadParameter = Animator.StringToHash("IsDead");
        private static readonly int AttackSpeedMultiplierParameter = Animator.StringToHash("AttackSpeedMultiplier");

        [SerializeField] private Animator animator;

        [Tooltip("The exact clip bound to the Attack state's Motion field in the Animator Controller. It is the key that SetAttackClip overrides at runtime, and the clip used for the swing duration when no weapon clip has been set.")]
        [FormerlySerializedAs("meleeAttackClip")]
        [SerializeField] private AnimationClip attackPlaceholderClip;

        /// <summary>
        /// Raised whenever a basic attack or a spell cast animation is triggered, so systems that only
        /// care that the player just acted in combat (see <see cref="Combat.PlayerCombatState"/>) do not
        /// need to know which script triggered it.
        /// </summary>
        public event Action CombatActionTriggered;

        private AnimatorOverrideController overrideController;
        private AnimationClip activeAttackClip;
        private float pendingChannelSeconds;

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
            overrideController = null;
            ApplyAttackClip();
        }

        /// <summary>
        /// Chooses which clip the single Attack state plays, by overriding
        /// <see cref="attackPlaceholderClip"/> on a runtime
        /// <see cref="AnimatorOverrideController"/> wrapped around the
        /// Animator's controller. The choice is remembered, so it survives
        /// <see cref="SetAnimator"/> being called again for a new model.
        /// </summary>
        /// <param name="clip">The attack clip for the equipped weapon; null restores the placeholder clip.</param>
        public void SetAttackClip(AnimationClip clip)
        {
            activeAttackClip = clip;
            ApplyAttackClip();
        }

        private void ApplyAttackClip()
        {
            if (animator == null || attackPlaceholderClip == null)
            {
                return;
            }

            if (overrideController == null)
            {
                var baseController = animator.runtimeAnimatorController;

                if (baseController is AnimatorOverrideController existingOverride)
                {
                    baseController = existingOverride.runtimeAnimatorController;
                }

                if (baseController == null)
                {
                    return;
                }

                overrideController = new AnimatorOverrideController(baseController);
                animator.runtimeAnimatorController = overrideController;
            }

            overrideController[attackPlaceholderClip] = activeAttackClip != null ? activeAttackClip : attackPlaceholderClip;
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
        /// — regardless of the clip's own authored length or the Attack
        /// state's base Speed in the Animator Controller (bound to this via
        /// the AttackSpeedMultiplier parameter, so this has no effect on
        /// Idle, Run, Cast or Death). Call before <see cref="TriggerAttack"/>
        /// so the upcoming swing plays at the right speed.
        /// </summary>
        /// <param name="targetSeconds">How long the swing should actually take to play, in seconds.</param>
        public void SetAttackDuration(float targetSeconds)
        {
            var clip = activeAttackClip != null ? activeAttackClip : attackPlaceholderClip;
            var multiplier = clip != null && targetSeconds > 0f ? clip.length / targetSeconds : 1f;
            animator.SetFloat(AttackSpeedMultiplierParameter, multiplier);
        }

        /// <summary>
        /// Plays the basic-attack animation. Which clip plays depends on the
        /// equipped weapon — see <see cref="SetAttackClip"/>.
        /// </summary>
        public void TriggerAttack()
        {
            animator.SetTrigger(AttackParameter);
            CombatActionTriggered?.Invoke();
        }

        /// <summary>
        /// Enters or leaves the channeling pose held while a timed cast charges up. Set to true when the
        /// cast starts and back to false when it completes or is interrupted.
        /// </summary>
        /// <param name="isChanneling">Whether the player is currently channeling a spell.</param>
        /// <param name="durationSeconds">How long the channeling animation should take, so its playback speed is scaled to the cast time. Ignored when leaving the pose.</param>
        public void SetChanneling(bool isChanneling, float durationSeconds = 0f)
        {
            animator.SetBool(IsChannelingParameter, isChanneling);
            pendingChannelSeconds = isChanneling ? durationSeconds : 0f;

            if (!isChanneling)
            {
                animator.SetFloat(ChannelSpeedParameter, 1f);
            }
        }

        /// <summary>
        /// Once the Animator has entered the Channel state, scales its playback speed so the clip lasts exactly
        /// the requested cast time.
        /// </summary>
        private void Update()
        {
            if (pendingChannelSeconds <= 0f || animator == null || !animator.GetCurrentAnimatorStateInfo(0).IsName(ChannelStateName))
            {
                return;
            }

            var clips = animator.GetCurrentAnimatorClipInfo(0);

            if (clips.Length == 0)
            {
                return;
            }

            animator.SetFloat(ChannelSpeedParameter, clips[0].clip.length / pendingChannelSeconds);
            pendingChannelSeconds = 0f;
        }

        /// <summary>
        /// Plays the hit reaction, used when a cast is knocked out of the player's hands.
        /// </summary>
        public void TriggerHit()
        {
            animator.SetTrigger(HitParameter);
        }

        /// <summary>
        /// Plays the spell release animation, the moment a spell is actually cast.
        /// </summary>
        public void TriggerCast()
        {
            animator.SetTrigger(CastParameter);
            CombatActionTriggered?.Invoke();
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
