using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Drives the Character Creation screen: pick one of the 6 classes and a
    /// gender, type a name, and create a brand-new character with that
    /// combination under the signed-in account (see
    /// <see cref="AccountSessionService"/>). Cancel returns to Character
    /// Selection with no side effects, since nothing has been submitted yet.
    /// </summary>
    public class CharacterCreationController : MonoBehaviour
    {
        [SerializeField] private ClassSelectionButton[] classOptions;
        [SerializeField] private GenderSelectionButton[] genderOptions;
        [SerializeField] private TMP_InputField characterNameInput;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private Button createButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private string gameplaySceneName = "Prototype_Map01";
        [SerializeField] private string characterSelectionSceneName = "CharacterSelection";

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
            cancelButton.onClick.AddListener(HandleCancelClicked);

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
                ShowWarning("Enter a name for the character.");
                return;
            }

            if (CharacterSaveLookup.Exists(characterName))
            {
                ShowWarning("A character with that name already exists.");
                return;
            }

            if (selectedClassOption == null || selectedGenderOption == null)
            {
                ShowWarning("Choose a class and a gender.");
                return;
            }

            AccountRepository.AddCharacter(AccountSessionService.CurrentAccountLogin, characterName);
            GameSessionService.BeginNewCharacter(characterName, selectedClassOption.CharacterClass, selectedGenderOption.Gender);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void HandleCancelClicked()
        {
            SceneManager.LoadScene(characterSelectionSceneName);
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
