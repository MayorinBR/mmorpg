using System;

namespace Project.Character.Combat
{
    /// <summary>
    /// A region of the character body that a skinned equipment piece can cover. Each region maps
    /// to one body renderer, which is hidden while a piece covering it is equipped.
    /// </summary>
    [Flags]
    public enum BodyPart
    {
        None = 0,
        Torso = 1 << 0,
        Pelvis = 1 << 1,
        UpperArmLeft = 1 << 2,
        UpperArmRight = 1 << 3,
        HandLeft = 1 << 4,
        HandRight = 1 << 5,
        LegLeft = 1 << 6,
        LegRight = 1 << 7,
        FootLeft = 1 << 8,
        FootRight = 1 << 9
    }
}
