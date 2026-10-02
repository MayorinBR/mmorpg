using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// The full-screen black panel shown while the Bootstrap scene is the only
    /// one loaded, so the persisted camera and HUD are never visible during
    /// loading. The panel lives under a persisted canvas, so it is not removed
    /// with the Bootstrap scene; <see cref="MapBootstrap"/> hides it once a map
    /// has taken over, and
    /// <see cref="MapTransitionService"/> shows it again whenever the player warps
    /// to another map.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private static LoadingScreen instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>Shows the loading panel, if one exists.</summary>
        public static void Show()
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(true);
            }
        }

        /// <summary>Hides the loading panel, if one exists.</summary>
        public static void Hide()
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
            }
        }
    }
}
