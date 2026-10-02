using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Project.Persistence;
using Project.UI;
using Project.World;

namespace Project.Flow
{
    /// <summary>
    /// Drives the in-game pause panel: Resume closes it, Character Selection
    /// returns to the selection screen, and Exit Game quits outright. The
    /// panel itself is a full-screen dimmed <see cref="WindowPanel"/> toggled
    /// by <see cref="UIInputRouter"/> on Esc and by a HUD button; clicking the
    /// dimmed area outside the inner window also closes it.
    /// </summary>
    [RequireComponent(typeof(WindowPanel))]
    public class PauseMenuController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button characterSelectionButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string characterSelectionSceneName = "CharacterSelection";

        private WindowPanel window;

        private void Awake()
        {
            window = GetComponent<WindowPanel>();
            resumeButton.onClick.AddListener(window.Close);
            characterSelectionButton.onClick.AddListener(HandleCharacterSelectionClicked);
            exitButton.onClick.AddListener(HandleExitClicked);
        }

        /// <summary>
        /// Closes the panel when the click lands on the panel's own backdrop
        /// rather than on the inner window or one of its buttons.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == gameObject)
            {
                window.Close();
            }
        }

        private void HandleCharacterSelectionClicked()
        {
            FindFirstObjectByType<PlayerSaveController>()?.Save();
            MapTransitionService.ReturnToMenu(characterSelectionSceneName);
        }

        /// <summary>
        /// Quits the built game, or stops Play Mode when running in the
        /// Editor, where <see cref="Application.Quit"/> alone has no effect.
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
