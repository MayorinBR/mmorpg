namespace Project.Character.Movement
{
    /// <summary>
    /// Represents a single frame's movement intent, independent of the input source
    /// that produced it (keyboard, gamepad, or click-to-move pathing).
    /// </summary>
    public interface IMovementIntent
    {
        /// <summary>
        /// Gets a value indicating whether this intent should override any
        /// in-progress path-following movement.
        /// </summary>
        bool CancelsPathing { get; }

        /// <summary>
        /// Gets the desired movement direction in world space, normalized.
        /// Returns Vector3.zero when no directional input is active.
        /// </summary>
        UnityEngine.Vector3 Direction { get; }
    }
}