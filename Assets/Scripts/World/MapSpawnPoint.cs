using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.World
{
    /// <summary>
    /// Marks a position a player can arrive at after warping into this map.
    /// Its id is the name of the scene the player comes from, so a map has
    /// one of these per portal that leads to it.
    /// </summary>
    public class MapSpawnPoint : MonoBehaviour
    {
#if UNITY_EDITOR
        [Tooltip("Drag the scene the player arrives from. Fills the id automatically.")]
        [SerializeField] private SceneAsset fromScene;
#endif
        [SerializeField] private string spawnPointId = "Default";

        /// <summary>Identifier a <see cref="WarpPortal"/> targets to arrive here.</summary>
        public string SpawnPointId => spawnPointId;

        /// <summary>World-space position the player is warped to.</summary>
        public Vector3 Position => transform.position;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (fromScene != null)
            {
                spawnPointId = fromScene.name;
            }
        }
#endif
    }
}
