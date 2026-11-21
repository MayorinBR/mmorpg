using System;
using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Tuning of <see cref="FootIkPass"/>.
    /// </summary>
    [Serializable]
    public sealed class FootIkSettings
    {
        [Tooltip("Layers the feet are placed on. Anything that must be walkable, such as terrain, ramps and steps, needs a collider on one of them.")]
        [SerializeField] private LayerMask groundMask;

        [Tooltip("Height above the animated foot where the ground ray starts.")]
        [SerializeField, Min(0.1f)] private float rayStartHeight = 0.5f;

        [Tooltip("Length of the ground ray, measured from its start.")]
        [SerializeField, Min(0.2f)] private float rayLength = 1.3f;

        [Tooltip("A foot within this height of the lowest foot of the pose counts as planted.")]
        [SerializeField, Min(0f)] private float plantedWithin = 0.02f;

        [Tooltip("A foot at least this far above the lowest foot of the pose counts as lifted and follows the clip.")]
        [SerializeField, Min(0.01f)] private float liftedAbove = 0.15f;

        [Tooltip("Largest vertical correction applied to a foot, in metres.")]
        [SerializeField, Min(0f)] private float maxAdjust = 0.4f;

        [Tooltip("Metres per second a foot may move toward its new height; hides the jump at the edge of a step.")]
        [SerializeField, Min(0.1f)] private float adjustSpeed = 3f;

        [Tooltip("Metres per second the hips may move down to keep the lower leg reachable.")]
        [SerializeField, Min(0.1f)] private float hipsSpeed = 2f;

        /// <summary>Layers the feet are placed on.</summary>
        public LayerMask GroundMask { get => groundMask; set => groundMask = value; }

        /// <summary>Height above the animated foot where the ground ray starts.</summary>
        public float RayStartHeight => rayStartHeight;

        /// <summary>Length of the ground ray.</summary>
        public float RayLength => rayLength;

        /// <summary>Height up to which a foot counts as planted.</summary>
        public float PlantedWithin => plantedWithin;

        /// <summary>Height from which a foot counts as lifted.</summary>
        public float LiftedAbove => liftedAbove;

        /// <summary>Largest vertical correction applied to a foot.</summary>
        public float MaxAdjust => maxAdjust;

        /// <summary>Metres per second a foot may move toward its new height.</summary>
        public float AdjustSpeed => adjustSpeed;

        /// <summary>Metres per second the hips may move down.</summary>
        public float HipsSpeed => hipsSpeed;
    }
}
