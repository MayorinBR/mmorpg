using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// One step of the Animator inverse kinematics pass, such as a look-at or a foot placement.
    /// </summary>
    public interface IAnimatorIkPass
    {
        /// <summary>
        /// Writes the IK goals of this pass to the Animator. Called from the Animator IK callback, once per frame.
        /// </summary>
        /// <param name="animator">The Animator being evaluated.</param>
        /// <param name="deltaTime">Seconds since the previous frame.</param>
        /// <param name="masterWeight">Multiplier from 0 to 1 applied to every weight of the pass; 0 switches the pass off.</param>
        void Apply(Animator animator, float deltaTime, float masterWeight);
    }
}
