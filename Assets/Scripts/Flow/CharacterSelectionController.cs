using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Drives the Character Selection screen: pick one of the 6 classes and
    /// a gender, type a name, and either create a brand-new character with
    /// that combination or continue with an existing one that already has a
    /// save under that name. Both actions read the same
    /// <see cref="characterNameInput"/> field but are otherwise completely
    /// separate — Create only ever makes a new save and refuses to overwrite
    /// one that already exists; Continue only ever loads an existing one and
    /// never creates anything. Back returns to Login with no other side
    /// effects, since nothing has been submitted yet at this point in the flow.
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        [SerializeField] private ClassSelectionButton[] classOptions;
        [SerializeField] private GenderSelectionButton[] genderOptions;
        [SerializeField] private TMP_InputField characterNameInput;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private Button createButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string gameplaySceneName = "Prototype_Map01";
        [SerializeField] private string loginSceneName = "Login";

        private ClassSelectionButton selectedClassOption;
        private GenderSelectionButton selectedGenderOption;

        private void Awake()
        {
            foreach (var option in classOptions)
            {
                var capturedOption = option;
                capturedOption.Button.onClick.AddListener(() => SelectClass(capturedOption));
            }

            foreach (var option in genderOptions)
            {
                var capturedOption = option;
                capturedOption.Button.onClick.AddListener(() => SelectGender(capturedOption));
            }

            createButton.onClick.AddListener(HandleCreateClicked);
            continueButton.onClick.AddListener(HandleContinueClicked);
            backButton.onClick.AddListener(HandleBackClicked);

            if (classOptions.Length > 0)
            {
                SelectClass(classOptions[0]);
            }

            if (genderOptions.Length > 0)
            {
                SelectGender(genderOptions[0]);
            }

            HideWarning();
        }

        private void SelectClass(ClassSelectionButton option)
        {
            selectedClassOption?.SetSelected(false);
            selectedClassOption = option;
            selectedClassOption.SetSelected(true);
        }

        private void SelectGender(GenderSelectionButton option)
        {
            selectedGenderOption?.SetSelected(false);
            selectedGenderOption = option;
            selectedGenderOption.SetSelected(true);
        }

        private void HandleCreateClicked()
        {
            var characterName = characterNameInput.text?.Trim();

            if (string.IsNullOrEmpty(characterName))
            {
                ShowWarning("Digite um nome para o personagem.");
                return;
            }

            if (CharacterSaveLookup.Exists(characterName))
            {
                ShowWarning("Já existe um personagem com esse nome.");
                return;
            }

            if (selectedClassOption == null || selectedGenderOption == null)
            {
                ShowWarning("Escolha uma classe e um gênero.");
                return;
            }

            GameSessionService.BeginNewCharacter(characterName, selectedClassOption.CharacterClass, selectedGenderOption.Gender);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void HandleContinueClicked()
        {
            var characterName = characterNameInput.text?.Trim();

            if (string.IsNullOrEmpty(characterName))
            {
                ShowWarning("Digite o nome do personagem.");
                return;
            }

            if (!CharacterSaveLookup.Exists(characterName))
            {
                ShowWarning("Personagem não encontrado.");
                return;
            }

            GameSessionService.BeginExistingCharacter(characterName);
            SceneManager.LoadScene(gameplaySceneName);
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

        private void HandleBackClicked()
        {
            SceneManager.LoadScene(loginSceneName);
        }
    }
}
