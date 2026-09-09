using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Flow
{
    /// <summary>
    /// The game's first screen. Start advances to the (placeholder) Login
    /// screen; Exit closes the game. The logo image itself is just an
    /// <see cref="UnityEngine.UI.Image"/> placed in the Editor — nothing
    /// here reads or sets it, since it's static art with no logic attached.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string loginSceneName = "Login";

        private void Awake()
        {
            startButton.onClick.AddListener(HandleStartClicked);
            exitButton.onClick.AddListener(HandleExitClicked);
        }

        private void HandleStartClicked()
        {
            SceneManager.LoadScene(loginSceneName);
        }

        /// <summary>
        /// Quits the built game, or stops Play Mode when running in the
        /// Editor — <see cref="Application.Quit"/> alone has no effect
        /// there, and this way Exit behaves the same way to test it without
        /// a full build.
        /// </summary>
        private void HandleExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
