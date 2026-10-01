using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Temporary local login screen: no real authentication or server exists
    /// yet, so signing in with a login that has never been used creates the
    /// account on the spot (see <see cref="AccountRepository.TryLogin"/>).
    /// Once signed in, advances to Character Selection, which reads the
    /// signed-in account's characters via <see cref="AccountSessionService"/>.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField loginInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string characterSelectionSceneName = "CharacterSelection";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            loginButton.onClick.AddListener(HandleLoginClicked);
            backButton.onClick.AddListener(HandleBackClicked);
            HideWarning();
        }

        private void HandleLoginClicked()
        {
            var login = loginInput.text?.Trim();
            var password = passwordInput.text;

            if (!AccountRepository.TryLogin(login, password, out _, out var error))
            {
                ShowWarning(error);
                return;
            }

            AccountSessionService.SetCurrentAccount(login);
            SceneManager.LoadScene(characterSelectionSceneName);
        }

        private void HandleBackClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void ShowWarning(string message)
        {
            if (warningText == null)
            {
                return;
            }

            warningText.text = message;
            warningText.gameObject.SetActive(true);
        }

        private void HideWarning()
        {
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            }
        }
    }
}
