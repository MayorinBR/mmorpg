using UnityEngine;
using Project.Character;
using Project.Character.Movement;
using Project.UI;

namespace Project.World
{
    /// <summary>
    /// Keeps the player alive across scene loads and exposes it as a single
    /// static instance so a newly loaded map can find and re-target it. Also
    /// guards against duplicates: if the map that originally held the player
    /// is loaded again, the freshly spawned Player in that scene is
    /// destroyed instead of replacing the persisted one. Persistence and the
    /// static reference are kept in one component deliberately, so a
    /// duplicate is always destroyed before any other script could read a
    /// stale or half-set <see cref="Instance"/>.
    /// </summary>
    public class PersistentPlayerAnchor : MonoBehaviour
    {
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerDeathHandler deathHandler;
        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private WorldItemHoverDetector hoverDetector;
        [SerializeField] private WorldSpaceHealthBarFollower statsCanvasFollower;

        /// <summary>
        /// The single persisted player instance. Null until the player's
        /// first Awake has run.
        /// </summary>
        public static PersistentPlayerAnchor Instance { get; private set; }

        /// <summary>The persisted player's movement controller.</summary>
        public CharacterMovementController MovementController => movementController;

        /// <summary>The persisted player's death/respawn handler.</summary>
        public PlayerDeathHandler DeathHandler => deathHandler;

        /// <summary>The persisted player's click-to-move and targeting input handler.</summary>
        public PlayerInputRouter InputRouter => inputRouter;

        /// <summary>The persisted player's world-space item hover detector.</summary>
        public WorldItemHoverDetector HoverDetector => hoverDetector;

        /// <summary>The follower positioning the player's own floating HP/MP bar canvas.</summary>
        public WorldSpaceHealthBarFollower StatsCanvasFollower => statsCanvasFollower;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
