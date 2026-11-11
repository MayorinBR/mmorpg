using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Moves the player around the world for travel skills. Implemented where the map and scene
    /// systems live, so the skill code never depends on them.
    /// </summary>
    public interface ISkillTravelService
    {
        /// <summary>Gets the name of the scene the player is currently in.</summary>
        string CurrentSceneName { get; }

        /// <summary>
        /// Moves the player to a random walkable point of the current map.
        /// </summary>
        /// <returns>True if the player was moved.</returns>
        bool TryTeleportRandomly();

        /// <summary>
        /// Moves the player to a destination, loading its scene first when it is another map.
        /// </summary>
        /// <param name="destination">Where to go.</param>
        void TravelTo(WarpDestination destination);

        /// <summary>
        /// Turns an already spawned object into a portal that sends the player to a destination
        /// when they step into it.
        /// </summary>
        /// <param name="portalObject">The spawned object that represents the portal.</param>
        /// <param name="destination">Where the portal leads.</param>
        /// <param name="activationRadius">How close, in meters, the player must be to use it.</param>
        void ConfigurePortal(GameObject portalObject, WarpDestination destination, float activationRadius);
    }
}
