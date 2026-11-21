using System;
using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Tuning of <see cref="LookAtIkPass"/>.
    /// </summary>
    [Serializable]
    public sealed class LookAtIkSettings
    {
        [Tooltip("How much the head follows the target.")]
        [SerializeField, Range(0f, 1f)] private float headWeight = 1f;

        [Tooltip("How much the spine follows the target during normal play.")]
        [SerializeField, Range(0f, 1f)] private float bodyWeight = 0.2f;

        [Tooltip("How much the spine follows the target while a ranged weapon is equipped.")]
        [SerializeField, Range(0f, 1f)] private float aimBodyWeight = 0.5f;

        [Tooltip("Limits how far the head may turn from the pose of the clip: 0 is free, 1 locks it to the clip.")]
        [SerializeField, Range(0f, 1f)] private float clampWeight = 0.5f;

        [Tooltip("Horizontal angle to the target, in degrees, up to which the look is at full strength.")]
        [SerializeField, Range(0f, 180f)] private float fullBelowAngle = 60f;

        [Tooltip("Horizontal angle to the target, in degrees, from which the look is switched off.")]
        [SerializeField, Range(0f, 180f)] private float zeroAboveAngle = 120f;

        [Tooltip("Weight change per second when the look starts or stops.")]
        [SerializeField, Min(0.1f)] private float fadeSpeed = 3f;

        [Tooltip("How quickly the looked-at point follows a moving target; higher is snappier.")]
        [SerializeField, Min(0.1f)] private float followSharpness = 8f;

        /// <summary>How much the head follows the target.</summary>
        public float HeadWeight => headWeight;

        /// <summary>How much the spine follows the target during normal play.</summary>
        public float BodyWeight => bodyWeight;

        /// <summary>How much the spine follows the target while a ranged weapon is equipped.</summary>
        public float AimBodyWeight => aimBodyWeight;

        /// <summary>Limits how far the head may turn from the pose of the clip.</summary>
        public float ClampWeight => clampWeight;

        /// <summary>Horizontal angle up to which the look is at full strength.</summary>
        public float FullBelowAngle => fullBelowAngle;

        /// <summary>Horizontal angle from which the look is switched off.</summary>
        public float ZeroAboveAngle => zeroAboveAngle;

        /// <summary>Weight change per second when the look starts or stops.</summary>
        public float FadeSpeed => fadeSpeed;

        /// <summary>How quickly the looked-at point follows a moving target.</summary>
        public float FollowSharpness => followSharpness;
    }
}
