using System.Collections.Generic;

namespace Project.Character.Combat
{
    /// <summary>
    /// Maps each <see cref="BodyPart"/> to the name of the body renderer that draws it.
    /// </summary>
    public static class BodyPartNames
    {
        /// <summary>
        /// Name of the renderer that draws the torso, which also contains the neck.
        /// </summary>
        public const string TorsoMeshName = "Body_Torso";

        private static readonly (BodyPart part, string meshName)[] Map =
        {
            (BodyPart.Torso, TorsoMeshName),
            (BodyPart.Pelvis, "Body_Pelvis"),
            (BodyPart.UpperArmLeft, "Body_UpperArm_L"),
            (BodyPart.UpperArmRight, "Body_UpperArm_R"),
            (BodyPart.HandLeft, "Body_Hand_L"),
            (BodyPart.HandRight, "Body_Hand_R"),
            (BodyPart.LegLeft, "Body_Leg_L"),
            (BodyPart.LegRight, "Body_Leg_R"),
            (BodyPart.FootLeft, "Body_Foot_L"),
            (BodyPart.FootRight, "Body_Foot_R")
        };

        /// <summary>
        /// Gets the renderer names of the regions in a mask.
        /// </summary>
        /// <param name="mask">The regions to resolve.</param>
        /// <returns>The body renderer name of every region set in <paramref name="mask"/>.</returns>
        public static IEnumerable<string> Resolve(BodyPart mask)
        {
            foreach (var (part, meshName) in Map)
            {
                if ((mask & part) != 0)
                {
                    yield return meshName;
                }
            }
        }

        /// <summary>
        /// Gets whether a renderer name belongs to one of the body regions.
        /// </summary>
        /// <param name="meshName">The renderer name to test.</param>
        /// <returns>True when <paramref name="meshName"/> is a body region renderer.</returns>
        public static bool IsBodyMesh(string meshName)
        {
            foreach (var (_, name) in Map)
            {
                if (name == meshName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
