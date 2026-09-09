using UnityEngine;
using Project.Flow;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Applies the character chosen on the Character Selection screen (via
    /// <see cref="GameSessionService"/>) to the Player as soon as this
    /// gameplay scene loads. Runs in <c>Awake</c> — Unity guarantees every
    /// component's <c>Awake</c> finishes before any component's <c>Start</c>
    /// fires, scene-wide — so <see cref="PlayerSaveController"/>'s own
    /// <c>Start</c>, which decides the save file path and does the actual
    /// load/save, always sees the character choice already applied,
    /// regardless of which GameObject's <c>Awake</c> happens to run first.
    /// </summary>
    /// <remarks>
    /// If nothing is pending — e.g. Play was pressed directly in this scene
    /// during development, skipping the menu flow entirely —
    /// <see cref="GameSessionService.TryConsumePendingCharacter"/> returns
    /// false and this does nothing at all, leaving
    /// <see cref="PlayerSaveController"/> to fall back to its own
    /// Inspector-configured default save file exactly as before this
    /// feature existed, so that existing workflow keeps working unchanged.
    /// </remarks>
    /// <remarks>
    /// <see cref="PlayerClassController"/> and <see cref="PlayerGenderController"/>
    /// each apply their own Inspector-configured starting value in their own
    /// <c>Awake</c>. Unity guarantees every <c>Awake</c> in the scene finishes
    /// before any <c>Start</c>, but does not guarantee the order between two
    /// different GameObjects' <c>Awake</c> calls — so without this attribute,
    /// this component's <see cref="classController"/>/<see cref="genderController"/>
    /// calls below could run before those defaults are applied and then get
    /// silently overwritten by them. <see cref="DefaultExecutionOrderAttribute"/>
    /// forces this component's <c>Awake</c> (and every other Unity message)
    /// to run after any component left at the default order (0), guaranteeing
    /// this always applies last.
    /// </remarks>
    [DefaultExecutionOrder(100)]
    public class CharacterSessionBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerSaveController saveController;
        [SerializeField] private PlayerNameProvider nameProvider;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerGenderController genderController;

        private void Awake()
        {
            if (!GameSessionService.TryConsumePendingCharacter(
                    out var characterName, out var isNewCharacter, out var characterClass, out var gender))
            {
                return;
            }

            saveController.ConfigureCharacter(characterName, isNewCharacter);

            if (isNewCharacter)
            {
                // An existing character's name/class/gender are instead
                // restored from its save file by PlayerSaveController.Load()
                // (called from its own Start), via each component's own
                // ISaveParticipant.RestoreState.
                nameProvider.SetName(characterName);
                classController.ChangeClass(characterClass);
                genderController.SetGender(gender);
            }
        }
    }
}
