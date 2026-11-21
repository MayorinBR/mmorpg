using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Pure helpers shared by the inverse kinematics passes, kept free of Animator calls so they can be unit tested.
    /// </summary>
    public static class IkMath
    {
        /// <summary>
        /// Gets how planted a foot is from how far it is above the lowest foot of the same pose.
        /// </summary>
        /// <param name="heightAboveLowestFoot">Height of the foot above the lowest foot, in metres.</param>
        /// <param name="plantedWithin">Height up to which the foot counts as fully planted.</param>
        /// <param name="liftedAbove">Height from which the foot counts as fully lifted.</param>
        /// <returns>1 when planted, 0 when lifted, linear in between.</returns>
        public static float FootPlantWeight(float heightAboveLowestFoot, float plantedWithin, float liftedAbove)
        {
            return 1f - Mathf.InverseLerp(plantedWithin, liftedAbove, heightAboveLowestFoot);
        }

        /// <summary>
        /// Gets how strongly the head may follow a target from the angle between the facing direction and the target.
        /// </summary>
        /// <param name="angleDegrees">Horizontal angle between the facing direction and the target, in degrees.</param>
        /// <param name="fullBelow">Angle up to which the look is at full strength.</param>
        /// <param name="zeroAbove">Angle from which the look is switched off.</param>
        /// <returns>1 inside the comfortable angle, 0 beyond the limit, linear in between.</returns>
        public static float LookAngleWeight(float angleDegrees, float fullBelow, float zeroAbove)
        {
            return 1f - Mathf.InverseLerp(fullBelow, zeroAbove, angleDegrees);
        }
    }
}
