using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Keeps the feet on uneven ground, such as steps and slopes. The ground height under each foot is compared
    /// with the ground height under the character, and a planted foot is moved by that difference and tilted to
    /// the surface. A foot that is lifted by the clip is left alone, and the hips drop so the lower leg can reach.
    /// </summary>
    public sealed class FootIkPass : IAnimatorIkPass
    {
        private readonly FootIkSettings settings;
        private readonly FootState left = new FootState(AvatarIKGoal.LeftFoot);
        private readonly FootState right = new FootState(AvatarIKGoal.RightFoot);
        private float hipsDrop;

        /// <summary>
        /// Creates the pass.
        /// </summary>
        /// <param name="settings">Tuning of the foot placement.</param>
        public FootIkPass(FootIkSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc />
        public void Apply(Animator animator, float deltaTime, float masterWeight)
        {
            var rootPosition = animator.transform.position;
            var rootGroundY = TryGround(rootPosition, out var rootHit) ? rootHit.point.y : rootPosition.y;

            var leftGoal = animator.GetIKPosition(left.Goal);
            var rightGoal = animator.GetIKPosition(right.Goal);
            var lowest = Mathf.Min(leftGoal.y, rightGoal.y);

            Place(animator, left, leftGoal, lowest, rootGroundY, deltaTime, masterWeight);
            Place(animator, right, rightGoal, lowest, rootGroundY, deltaTime, masterWeight);

            var wantedDrop = Mathf.Min(0f, left.Delta, right.Delta) * masterWeight;
            hipsDrop = Mathf.MoveTowards(hipsDrop, wantedDrop, settings.HipsSpeed * deltaTime);
            animator.bodyPosition += Vector3.up * hipsDrop;
        }

        private void Place(Animator animator, FootState foot, Vector3 goal, float lowest, float rootGroundY, float deltaTime, float masterWeight)
        {
            var found = TryGround(goal, out var hit);
            var wantedDelta = found ? Mathf.Clamp(hit.point.y - rootGroundY, -settings.MaxAdjust, settings.MaxAdjust) : 0f;
            foot.Delta = Mathf.MoveTowards(foot.Delta, wantedDelta, settings.AdjustSpeed * deltaTime);
            foot.Normal = Vector3.Slerp(foot.Normal, found ? hit.normal : Vector3.up, 1f - Mathf.Exp(-10f * deltaTime));

            var weight = IkMath.FootPlantWeight(goal.y - lowest, settings.PlantedWithin, settings.LiftedAbove) * masterWeight;

            animator.SetIKPositionWeight(foot.Goal, weight);
            animator.SetIKRotationWeight(foot.Goal, weight);
            animator.SetIKPosition(foot.Goal, goal + Vector3.up * foot.Delta);
            animator.SetIKRotation(foot.Goal, Quaternion.FromToRotation(Vector3.up, foot.Normal) * animator.GetIKRotation(foot.Goal));
        }

        private bool TryGround(Vector3 position, out RaycastHit hit)
        {
            var origin = position + Vector3.up * settings.RayStartHeight;
            return Physics.Raycast(origin, Vector3.down, out hit, settings.RayLength, settings.GroundMask, QueryTriggerInteraction.Ignore);
        }

        private sealed class FootState
        {
            public FootState(AvatarIKGoal goal)
            {
                Goal = goal;
            }

            public AvatarIKGoal Goal { get; }

            public float Delta { get; set; }

            public Vector3 Normal { get; set; } = Vector3.up;
        }
    }
}
