using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tuning for <see cref="BowHandIkPass"/>: where the free hand rests relative to the bow and how fast it moves there.
    /// </summary>
    [Serializable]
    public sealed class BowHandIkSettings
    {
        [Tooltip("Hand target in metres, in the bow's own axes. The string side is the negative Z axis.")]
        [SerializeField] private Vector3 gripOffset = new Vector3(0f, 0f, -0.15f);

        [Tooltip("IK weight of the free hand while the bow is held at rest.")]
        [SerializeField, Range(0f, 1f)] private float holdWeight = 1f;

        [Tooltip("Weight units per second when the hand blends in or out.")]
        [SerializeField, Min(0.01f)] private float fadeSpeed = 4f;

        /// <summary>
        /// Gets the hand target in the bow's local axes, in metres.
        /// </summary>
        public Vector3 GripOffset => gripOffset;

        /// <summary>
        /// Gets the IK weight of the free hand while the bow is held at rest.
        /// </summary>
        public float HoldWeight => holdWeight;

        /// <summary>
        /// Gets the blend speed in weight units per second.
        /// </summary>
        public float FadeSpeed => fadeSpeed;
    }
}
