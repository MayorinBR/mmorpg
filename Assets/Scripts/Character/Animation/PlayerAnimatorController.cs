using System;
using System.Collections;
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
        private static readonly int AttackAlternateParameter = Animator.StringToHash("AttackAlt");
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

        [Tooltip("The exact clip bound to the AttackAlt state's Motion field in the Animator Controller. It is the key that SetAlternateAttackClip overrides at runtime.")]
        [SerializeField] private AnimationClip attackAlternatePlaceholderClip;

        [Tooltip("The exact clip bound to the Idle state's Motion field in the Animator Controller. It is the key that SetIdleClip overrides at runtime.")]
        [SerializeField] private AnimationClip idlePlaceholderClip;

        [Tooltip("The exact clip bound to the Run state's Motion field in the Animator Controller. It is the key that SetRunClip overrides at runtime.")]
        [SerializeField] private AnimationClip runPlaceholderClip;

        [Tooltip("The exact clip bound to the Channel state's Motion field in the Animator Controller. It is the key that SetCastClip overrides at runtime.")]
        [SerializeField] private AnimationClip channelPlaceholderClip;

        [Tooltip("The exact clip bound to the Cast state's Motion field in the Animator Controller. It is the key that SetCastClip overrides at runtime.")]
        [SerializeField] private AnimationClip castPlaceholderClip;

        /// <summary>
        /// Raised whenever a basic attack or a spell cast animation is triggered, so systems that only
        /// care that the player just acted in combat (see <see cref="Combat.PlayerCombatState"/>) do not
        /// need to know which script triggered it.
        /// </summary>
        public event Action CombatActionTriggered;

        private AnimatorOverrideController overrideController;
        private AnimationClip activeAttackClip;
        private AnimationClip activeAlternateAttackClip;
        private Coroutine chainRoutine;
        private AnimationClip activeIdleClip;
        private AnimationClip activeRunClip;
        private AnimationClip activeCastClip;
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
            ApplyClipOverrides();
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
            ApplyClipOverrides();
        }

        /// <summary>
        /// Chooses which clip the AttackAlt state plays, the same way <see cref="SetAttackClip"/> does for the Attack state.
        /// </summary>
        /// <param name="clip">The second swing clip for the equipped weapon; null when the weapon has a single swing.</param>
        public void SetAlternateAttackClip(AnimationClip clip)
        {
            activeAlternateAttackClip = clip;
            ApplyClipOverrides();
        }

        /// <summary>Gets whether the equipped weapon has a second swing to alternate or chain with.</summary>
        public bool HasAlternateAttack => activeAlternateAttackClip != null;

        /// <summary>
        /// Chooses which clip the Idle state plays, the same way <see cref="SetAttackClip"/> does for the
        /// Attack state. The choice is remembered, so it survives <see cref="SetAnimator"/> being called again.
        /// </summary>
        /// <param name="clip">The idle clip for the equipped weapon; null restores the default idle clip.</param>
        public void SetIdleClip(AnimationClip clip)
        {
            activeIdleClip = clip;
            ApplyClipOverrides();
        }

        /// <summary>
        /// Chooses which clip the Run state plays, the same way <see cref="SetAttackClip"/> does for the Attack state.
        /// </summary>
        /// <param name="clip">The run clip for the equipped weapon; null restores the default run clip.</param>
        public void SetRunClip(AnimationClip clip)
        {
            activeRunClip = clip;
            ApplyClipOverrides();
        }

        /// <summary>
        /// Makes one clip serve as both the channeling and the release animation of spells, the same way
        /// <see cref="SetAttackClip"/> does for the Attack state. While set, a timed cast plays the clip once,
        /// stretched to the cast time, and no separate release animation follows it.
        /// </summary>
        /// <param name="clip">The single cast clip for the equipped weapon; null restores the default channel and cast clips.</param>
        public void SetCastClip(AnimationClip clip)
        {
            activeCastClip = clip;
            ApplyClipOverrides();
        }

        /// <summary>
        /// Sets where the attack blow lands in the swing for the equipped weapon.
        /// </summary>
        /// <param name="normalizedTime">The impact moment as a fraction of the attack clip; 0 for a blow that lands as the attack starts.</param>
        public void SetAttackImpact(float normalizedTime)
        {
            AttackImpactNormalizedTime = normalizedTime;
        }

        /// <summary>
        /// Where the attack blow lands in the swing of the equipped weapon, as a fraction of the attack clip.
        /// </summary>
        public float AttackImpactNormalizedTime { get; private set; }

        private void ApplyClipOverrides()
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

            if (attackAlternatePlaceholderClip != null)
            {
                overrideController[attackAlternatePlaceholderClip] = activeAlternateAttackClip != null ? activeAlternateAttackClip : attackAlternatePlaceholderClip;
            }

            if (idlePlaceholderClip != null)
            {
                overrideController[idlePlaceholderClip] = activeIdleClip != null ? activeIdleClip : idlePlaceholderClip;
            }

            if (runPlaceholderClip != null)
            {
                overrideController[runPlaceholderClip] = activeRunClip != null ? activeRunClip : runPlaceholderClip;
            }

            if (channelPlaceholderClip != null)
            {
                overrideController[channelPlaceholderClip] = activeCastClip != null ? activeCastClip : channelPlaceholderClip;
            }

            if (castPlaceholderClip != null)
            {
                overrideController[castPlaceholderClip] = activeCastClip != null ? activeCastClip : castPlaceholderClip;
            }
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
            CancelChain();
            animator.SetTrigger(AttackParameter);
            CombatActionTriggered?.Invoke();
        }

        /// <summary>
        /// Plays the second swing of the equipped weapon (see <see cref="SetAlternateAttackClip"/>), scaled to last the given time.
        /// </summary>
        /// <param name="durationSeconds">How long the swing should actually take to play, in seconds.</param>
        public void TriggerAlternateAttack(float durationSeconds)
        {
            var multiplier = HasAlternateAttack && durationSeconds > 0f ? activeAlternateAttackClip.length / durationSeconds : 1f;
            animator.SetFloat(AttackSpeedMultiplierParameter, multiplier);
            animator.SetTrigger(AttackAlternateParameter);
            CombatActionTriggered?.Invoke();
        }

        /// <summary>
        /// Plays the first swing and, right after it, the second swing, each at double speed so the pair takes the
        /// time of a single attack. Used when Double Attack triggers.
        /// </summary>
        /// <param name="totalSeconds">How long both swings together should take, in seconds.</param>
        /// <returns>The seconds after which the second swing starts.</returns>
        public float TriggerAttackChain(float totalSeconds)
        {
            var half = totalSeconds * 0.5f;
            SetAttackDuration(half);
            TriggerAttack();
            chainRoutine = StartCoroutine(PlayAlternateAfter(half));
            return half;
        }

        /// <summary>Fraction of the first swing played before the second one starts, so the Attack state's exit does not show an idle frame in between.</summary>
        private const float ChainOverlap = 0.9f;

        private IEnumerator PlayAlternateAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds * ChainOverlap);
            chainRoutine = null;
            TriggerAlternateAttack(seconds);
        }

        private void CancelChain()
        {
            if (chainRoutine != null)
            {
                StopCoroutine(chainRoutine);
                chainRoutine = null;
            }
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
        /// Plays the spell release animation, the moment a spell is actually cast. When a timed cast has just
        /// finished playing its cast clip in the Channel state, the release is skipped so the clip is not repeated.
        /// </summary>
        public void TriggerCast()
        {
            var channelClipAlreadyPlayed = animator.GetCurrentAnimatorStateInfo(0).IsName(ChannelStateName);

            if (!channelClipAlreadyPlayed)
            {
                animator.SetTrigger(CastParameter);
            }

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
