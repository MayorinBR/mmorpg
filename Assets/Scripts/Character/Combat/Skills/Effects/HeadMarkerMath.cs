namespace Project.Character.Combat
{
    /// <summary>
    /// Math for placing a floating mark, such as Provoke's, above the top of a target's model.
    /// </summary>
    public static class HeadMarkerMath
    {
        /// <summary>The gap, in meters, between the top of the target and the authored point of the mark.</summary>
        public const float MarginMeters = 0.3f;

        /// <summary>
        /// Gets how far to move a spawned effect vertically so the point it was authored at sits the margin
        /// above the top of the target.
        /// </summary>
        /// <param name="targetTop">The world Y of the top of the target's model.</param>
        /// <param name="instanceY">The world Y the effect spawned at.</param>
        /// <param name="authoredHeight">The height of the authored point above the effect's origin.</param>
        /// <returns>The vertical shift to apply.</returns>
        public static float ComputeShift(float targetTop, float instanceY, float authoredHeight)
        {
            return targetTop + MarginMeters - (instanceY + authoredHeight);
        }
    }
}
