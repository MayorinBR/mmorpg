using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Flow
{
    /// <summary>
    /// Placeholder login screen — no real authentication exists yet, per
    /// Victor's explicit request ("vai ser implementada depois"). Continue
    /// currently just advances straight to Character Selection; once a real
    /// login/network flow is designed, only <see cref="HandleContinueClicked"/>'s
    /// body needs to change (e.g. to run after a successful server
    /// response) — the Main Menu and Character Selection screens don't
    /// reference this class at all, so neither needs any change either way.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string characterSelectionSceneName = "CharacterSelection";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }

        private void HandleContinueClicked()
        {
            SceneManager.LoadScene(characterSelectionSceneName);
        }

        private void HandleBackClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
