using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Marks a position a player can arrive at after warping into this map.
    /// A map can have several of these, one per warp portal that leads to
    /// it, distinguished by <see cref="SpawnPointId"/>.
    /// </summary>
    public class MapSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnPointId = "Default";

        /// <summary>Identifier a <see cref="WarpPortal"/> targets to arrive here.</summary>
        public string SpawnPointId => spawnPointId;

        /// <summary>World-space position the player is warped to.</summary>
        public Vector3 Position => transform.position;
    }
}
