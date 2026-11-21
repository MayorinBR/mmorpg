using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Turns the head, and optionally the spine, toward the point supplied by an <see cref="ILookTargetSource"/>.
    /// The weight fades in and out, and drops to zero when the target is too far to the side or behind the character.
    /// </summary>
    public sealed class LookAtIkPass : IAnimatorIkPass
    {
        private readonly LookAtIkSettings settings;
        private readonly ILookTargetSource source;
        private float weight;
        private Vector3 lookPosition;

        /// <summary>
        /// Creates the pass.
        /// </summary>
        /// <param name="settings">Tuning of the look.</param>
        /// <param name="source">Where the character should look.</param>
        public LookAtIkPass(LookAtIkSettings settings, ILookTargetSource source)
        {
            this.settings = settings;
            this.source = source;
        }

        /// <inheritdoc />
        public void Apply(Animator animator, float deltaTime, float masterWeight)
        {
            var targetWeight = 0f;
            var aimsWithBody = false;

            if (masterWeight > 0f && source.TryGetLookTarget(out var target))
            {
                var flat = target.Position - animator.transform.position;
                flat.y = 0f;
                var angle = Vector3.Angle(animator.transform.forward, flat);

                targetWeight = IkMath.LookAngleWeight(angle, settings.FullBelowAngle, settings.ZeroAboveAngle) * masterWeight;
                aimsWithBody = target.AimsWithBody;

                lookPosition = weight <= 0.001f
                    ? target.Position
                    : Vector3.Lerp(lookPosition, target.Position, 1f - Mathf.Exp(-settings.FollowSharpness * deltaTime));
            }

            weight = Mathf.MoveTowards(weight, targetWeight, settings.FadeSpeed * deltaTime);

            if (weight <= 0f)
            {
                animator.SetLookAtWeight(0f);
                return;
            }

            var body = aimsWithBody ? settings.AimBodyWeight : settings.BodyWeight;
            animator.SetLookAtWeight(weight, body, settings.HeadWeight, 0f, settings.ClampWeight);
            animator.SetLookAtPosition(lookPosition);
        }
    }
}
