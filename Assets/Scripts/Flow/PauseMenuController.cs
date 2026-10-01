using UnityEngine;
using UnityEngine.UI;
using Project.World;

namespace Project.Flow
{
    /// <summary>
    /// Drives the in-game pause panel: one button returns to Character
    /// Creation, the other quits the game outright. The panel itself opens
    /// and closes via its sibling <see cref="Project.UI.WindowPanel"/>,
    /// toggled by <see cref="Project.UI.UIInputRouter"/> on Esc — this
    /// component only wires the two buttons, the same shape every other
    /// content-specific window in the project already follows.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private Button characterCreationButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string characterCreationSceneName = "CharacterCreation";

        private void Awake()
        {
            characterCreationButton.onClick.AddListener(HandleCharacterCreationClicked);
            exitButton.onClick.AddListener(HandleExitClicked);
        }

        private void HandleCharacterCreationClicked()
        {
            MapTransitionService.ReturnToMenu(characterCreationSceneName);
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
