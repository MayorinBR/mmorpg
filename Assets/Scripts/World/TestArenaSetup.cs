using System.Collections;
using UnityEngine;
using Project.Character.Combat;
using Project.Character.Stats;

namespace Project.World
{
    /// <summary>
    /// Lives in the test arena scene and applies the character choices made in its Inspector to the persisted
    /// player once the player exists, so class specific tests start from the scene. Each choice is applied only
    /// when its toggle is on. The player keeps its own save: a class or gender applied here is part of the
    /// character from then on, like any other change made in play.
    /// </summary>
    public class TestArenaSetup : MonoBehaviour
    {
        private const float SettleSeconds = 1f;

        [Header("Class")]
        [Tooltip("Replaces the player's class when the arena loads. The change is saved with the character.")]
        [SerializeField] private bool applyClass;
        [SerializeField] private CharacterClass playerClass = CharacterClass.Novice;

        [Header("Gender")]
        [Tooltip("Replaces the player's gender when the arena loads. The change is saved with the character.")]
        [SerializeField] private bool applyGender;
        [SerializeField] private CharacterGender playerGender = CharacterGender.Male;

        private IEnumerator Start()
        {
            while (PersistentPlayerAnchor.Instance == null)
            {
                yield return null;
            }

            // Waits for the player's save to be restored, which would otherwise overwrite these choices.
            yield return new WaitForSeconds(SettleSeconds);

            Apply(PersistentPlayerAnchor.Instance);
        }

        private void Apply(PersistentPlayerAnchor player)
        {
            if (applyClass)
            {
                var classController = player.GetComponentInChildren<PlayerClassController>(true);
                if (classController != null && classController.CurrentClass != playerClass)
                {
                    classController.ChangeClass(playerClass);
                }
            }

            if (applyGender)
            {
                var genderController = player.GetComponentInChildren<PlayerGenderController>(true);
                if (genderController != null && genderController.CurrentGender != playerGender)
                {
                    genderController.SetGender(playerGender);
                }
            }
        }
    }
}
