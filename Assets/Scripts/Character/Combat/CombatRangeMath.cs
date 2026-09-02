using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Shared distance calculation for combat range checks (basic attack
    /// range, skill range). Ignores height so a target's vertical
    /// animation — a hop, a jump arc — can't push it in or out of range
    /// on its own; only horizontal movement should matter for reach.
    /// </summary>
    internal static class CombatRangeMath
    {
        /// <summary>
        /// Measures the distance between two positions on the horizontal
        /// (XZ) plane, ignoring any difference in height.
        /// </summary>
        /// <param name="from">First position.</param>
        /// <param name="to">Second position.</param>
        /// <returns>The horizontal distance between the two positions.</returns>
        internal static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            var delta = from - to;
            delta.y = 0f;
            return delta.magnitude;
        }
    }
}
