using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// A point the character should look at, and whether the upper body should join the head in aiming at it.
    /// </summary>
    public readonly struct LookTarget
    {
        /// <summary>
        /// Creates a look target.
        /// </summary>
        /// <param name="position">World position to look at.</param>
        /// <param name="aimsWithBody">True when the spine should lean toward the target, as when aiming a ranged weapon.</param>
        public LookTarget(Vector3 position, bool aimsWithBody)
        {
            Position = position;
            AimsWithBody = aimsWithBody;
        }

        /// <summary>World position to look at.</summary>
        public Vector3 Position { get; }

        /// <summary>True when the spine should lean toward the target.</summary>
        public bool AimsWithBody { get; }
    }

    /// <summary>
    /// Supplies the point a character looks at, so the look-at pass does not depend on the combat or targeting code.
    /// </summary>
    public interface ILookTargetSource
    {
        /// <summary>
        /// Gets the current look target.
        /// </summary>
        /// <param name="target">The point to look at, valid only when the method returns true.</param>
        /// <returns>True when the character should look at something, false when it should look ahead.</returns>
        bool TryGetLookTarget(out LookTarget target);
    }
}
