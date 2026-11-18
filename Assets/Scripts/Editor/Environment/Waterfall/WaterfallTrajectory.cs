using System;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Path of the water: a slow run along the cave floor, then a ballistic arc from the lip down to the river.
    /// Time zero is the instant the water leaves the lip, so negative times are inside the cave.
    /// </summary>
    public sealed class WaterfallTrajectory
    {
        private const float WaterLift = 0.12f;
        private const float CaveUsedDepth = 0.9f;

        private readonly float lipZ;
        private readonly float caveSpeed;

        private WaterfallTrajectory(float centerX, float lipZ, float lipY, float landingY, float outwardSpeed, float fallTime, float caveTime, float gravity, float caveSpeed)
        {
            CenterX = centerX;
            this.lipZ = lipZ;
            LipY = lipY;
            LandingY = landingY;
            OutwardSpeed = outwardSpeed;
            FallTime = fallTime;
            CaveTime = caveTime;
            Gravity = gravity;
            this.caveSpeed = caveSpeed;
        }

        /// <summary>World X of the fall.</summary>
        public float CenterX { get; }

        /// <summary>World Z of the lip.</summary>
        public float LipZ => lipZ;

        /// <summary>World height of the water surface at the lip.</summary>
        public float LipY { get; }

        /// <summary>World height of the river where the water lands.</summary>
        public float LandingY { get; }

        /// <summary>Horizontal speed of the water leaving the lip, in meters per second.</summary>
        public float OutwardSpeed { get; }

        /// <summary>Time from the lip to the river, in seconds.</summary>
        public float FallTime { get; }

        /// <summary>Time spent inside the cave before the lip, in seconds.</summary>
        public float CaveTime { get; }

        /// <summary>Gravity used for the arc, in meters per second squared.</summary>
        public float Gravity { get; }

        /// <summary>
        /// Creates the trajectory that lands at the profile landing Z.
        /// </summary>
        /// <param name="profile">Profile with the sill height, cave data and landing Z.</param>
        /// <param name="lipZ">World Z of the lip.</param>
        /// <param name="landingY">World height of the river at the landing point.</param>
        /// <param name="gravity">Gravity in meters per second squared.</param>
        /// <returns>The trajectory.</returns>
        /// <exception cref="InvalidOperationException">The lip is not above the river.</exception>
        public static WaterfallTrajectory Create(WaterfallProfile profile, float lipZ, float landingY, float gravity)
        {
            var lipY = profile.SillHeight + WaterLift;
            var drop = lipY - landingY;
            if (drop <= 0f)
            {
                throw new InvalidOperationException("The lip must be above the river.");
            }

            var fallTime = Mathf.Sqrt(2f * drop / gravity);
            var outwardSpeed = (lipZ - profile.LandingZ) / fallTime;
            var caveTime = profile.CaveDepth * CaveUsedDepth / profile.CaveFlowSpeed;
            return new WaterfallTrajectory(profile.CenterX, lipZ, lipY, landingY, outwardSpeed, fallTime, caveTime, gravity, profile.CaveFlowSpeed);
        }

        /// <summary>Returns the position of the water at a time relative to the lip.</summary>
        /// <param name="time">Seconds since the water left the lip; negative inside the cave.</param>
        /// <returns>World position on the centerline of the sheet.</returns>
        public Vector3 PositionAt(float time)
        {
            if (time < 0f)
            {
                return new Vector3(CenterX, LipY, lipZ - time * caveSpeed);
            }

            return new Vector3(CenterX, LipY - 0.5f * Gravity * time * time, lipZ - OutwardSpeed * time);
        }
    }
}
