using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Drives the Character Selection screen: lists the signed-in account's
    /// characters (see <see cref="AccountSessionService"/>) 6 at a time, in
    /// creation order, with search-by-name and pagination. Selecting a slot
    /// and pressing Start loads the gameplay scene with that character; New
    /// Character goes to Character Creation instead.
    /// </summary>
    public class CharacterSelectionController : MonoBehaviour
    {
        private const int CharactersPerPage = 6;

        [SerializeField] private CharacterSlotView[] characterSlots;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button newCharacterButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string characterCreationSceneName = "CharacterCreation";
        [SerializeField] private string gameplaySceneName = "Prototype_Map01";
        [SerializeField] private string loginSceneName = "Login";

        private List<string> ownedCharacterNames = new List<string>();
        private string selectedCharacterName;
        private int currentPage;

        private void Awake()
        {
            newCharacterButton.onClick.AddListener(() => SceneManager.LoadScene(characterCreationSceneName));
            startButton.onClick.AddListener(HandleStartClicked);
            previousPageButton.onClick.AddListener(() => ChangePage(-1));
            nextPageButton.onClick.AddListener(() => ChangePage(1));
            backButton.onClick.AddListener(() => SceneManager.LoadScene(loginSceneName));
            searchInput.onValueChanged.AddListener(_ => RefreshSlots(resetPage: true));

            foreach (var slot in characterSlots)
            {
                var capturedSlot = slot;
                capturedSlot.Button.onClick.AddListener(() => SelectSlot(capturedSlot));
            }
        }

        private void Start()
        {
            var login = AccountSessionService.CurrentAccountLogin;

            if (!string.IsNullOrEmpty(login) && AccountRepository.Exists(login))
            {
                ownedCharacterNames = AccountRepository.Load(login).characterNames;
            }

            RefreshSlots(resetPage: true);
        }

        private void ChangePage(int delta)
        {
            currentPage += delta;
            RefreshSlots(resetPage: false);
        }

        private void SelectSlot(CharacterSlotView slot)
        {
            if (slot.CharacterName == null)
            {
                return;
            }

            selectedCharacterName = slot.CharacterName;

            foreach (var characterSlot in characterSlots)
            {
                characterSlot.SetSelected(characterSlot.CharacterName == selectedCharacterName);
            }

            startButton.interactable = true;
        }

        private void RefreshSlots(bool resetPage)
        {
            if (resetPage)
            {
                currentPage = 0;
            }

            var searchTerm = searchInput.text?.Trim() ?? "";
            var filteredNames = ownedCharacterNames
                .Where(name => name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var pageCount = Mathf.Max(1, Mathf.CeilToInt(filteredNames.Count / (float)CharactersPerPage));
            currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);

            var pageNames = filteredNames.Skip(currentPage * CharactersPerPage).Take(CharactersPerPage).ToList();

            for (var i = 0; i < characterSlots.Length; i++)
            {
                if (i < pageNames.Count && TryLoadSummary(pageNames[i], out var level, out var characterClass))
                {
                    characterSlots[i].SetCharacter(pageNames[i], level, characterClass);
                }
                else
                {
                    characterSlots[i].SetEmpty();
                }

                characterSlots[i].SetSelected(characterSlots[i].CharacterName == selectedCharacterName);
            }

            previousPageButton.interactable = currentPage > 0;
            nextPageButton.interactable = currentPage < pageCount - 1;
            startButton.interactable = pageNames.Contains(selectedCharacterName);
        }

        private static bool TryLoadSummary(string characterName, out int level, out CharacterClass characterClass)
        {
            var repository = new JsonFileSaveRepository(CharacterSaveLookup.SaveFilePath(characterName));

            if (repository.TryLoad(out var data))
            {
                level = data.baseLevel;
                characterClass = (CharacterClass)data.characterClassIndex;
                return true;
            }

            level = 0;
            characterClass = default;
            return false;
        }

        private void HandleStartClicked()
        {
            if (selectedCharacterName == null)
            {
                return;
            }

            GameSessionService.BeginExistingCharacter(selectedCharacterName);
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
