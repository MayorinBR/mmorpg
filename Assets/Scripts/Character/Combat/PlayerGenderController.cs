using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Character.Animation;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the player's chosen <see cref="CharacterGender"/> and swaps
    /// which 3D model represents it — purely cosmetic, since Ragnarok
    /// Online itself gives gender no mechanical effect. The player keeps a
    /// single geometry instance at a time: switching gender destroys the
    /// current instance under <see cref="geometryRoot"/> and instantiates
    /// <see cref="malePrefab"/> or <see cref="femalePrefab"/> in its place.
    /// Since the prefabs are raw imported models with no
    /// <see cref="RuntimeAnimatorController"/> of their own,
    /// <see cref="sharedAnimatorController"/> is assigned to the new
    /// instance's <see cref="Animator"/> directly — Humanoid retargeting
    /// means the same controller plays correctly regardless of which
    /// gender's avatar it's driving — before retargeting
    /// <see cref="animatorController"/> at that <see cref="Animator"/> so
    /// every existing trigger/parameter call
    /// (<see cref="PlayerAnimatorController"/>) keeps working unchanged.
    /// Also adds a <see cref="PlayerAnimationEventRelay"/> to the new
    /// instance's <see cref="Animator"/> GameObject, forwarding its
    /// Animation Events to <see cref="combatController"/> — Unity calls an
    /// Animation Event's function on whatever GameObject owns the
    /// Animator, which is this swappable clone rather than the player
    /// root.
    /// </summary>
    /// <remarks>
    /// Deliberately independent of any specific scene: nothing here assumes
    /// it's running in the gameplay scene specifically, so the exact same
    /// component can later drive a live 3D preview on the Character
    /// Selection screen itself (a planned follow-up) by dropping it onto a
    /// preview rig with its own geometry root wired, with no changes needed
    /// here.
    /// </remarks>
    public class PlayerGenderController : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private CharacterGender startingGender = CharacterGender.Male;
        [SerializeField] private Transform geometryRoot;
        [SerializeField] private GameObject malePrefab;
        [SerializeField] private GameObject femalePrefab;
        [SerializeField] private RuntimeAnimatorController sharedAnimatorController;
        [SerializeField] private PlayerAnimatorController animatorController;

        [Tooltip("Optional. The player's combat controller — wired to a " +
            "PlayerAnimationEventRelay added to each new model instance, so " +
            "its Animation Events (e.g. ReleaseProjectile) reach it. Left " +
            "empty (e.g. a Character Selection preview rig with no combat " +
            "controller), no relay is added and Animation Events on that " +
            "rig simply have no receiver.")]
        [SerializeField] private PlayerCombatController combatController;

        private GameObject currentGeometry;

        /// <summary>Raised whenever the player's gender changes.</summary>
        public event Action<CharacterGender> GenderChanged;

        /// <summary>Gets the player's current gender.</summary>
        public CharacterGender CurrentGender { get; private set; }

        private void Awake()
        {
            SetGender(startingGender);
        }

        /// <summary>
        /// Sets the player's gender, replacing the current geometry instance
        /// with the matching prefab and retargeting
        /// <see cref="animatorController"/> at its <see cref="Animator"/>.
        /// </summary>
        /// <param name="newGender">The gender to switch to.</param>
        public void SetGender(CharacterGender newGender)
        {
            CurrentGender = newGender;

            if (geometryRoot == null)
            {
                return;
            }

            if (currentGeometry != null)
            {
                Destroy(currentGeometry);
                currentGeometry = null;
            }

            var prefab = newGender == CharacterGender.Male ? malePrefab : femalePrefab;

            if (prefab == null)
            {
                GenderChanged?.Invoke(newGender);
                return;
            }

            currentGeometry = Instantiate(prefab, geometryRoot);

            var activeAnimator = currentGeometry.GetComponentInChildren<Animator>();

            if (activeAnimator != null)
            {
                // The instantiated model is a raw imported FBX with no
                // RuntimeAnimatorController of its own — Humanoid
                // retargeting means the same controller plays correctly on
                // any Humanoid avatar, so it's assigned explicitly here
                // instead of relying on it being pre-configured per model.
                if (sharedAnimatorController != null)
                {
                    activeAnimator.runtimeAnimatorController = sharedAnimatorController;
                }

                // Unity imports a Humanoid FBX with root motion enabled by
                // default. Left on, Mecanim extracts motion (including
                // rotation) straight from the clip's hip movement and
                // applies it to this clone every frame — independently of,
                // and fighting with, the player root's own
                // transform.forward (see CharacterMovementController and
                // PlayerCombatController), which is what actually caused
                // the character to visibly spin/turn during attacks.
                // Facing must be owned entirely by the player root, so the
                // clone's own motion extraction is disabled here.
                activeAnimator.applyRootMotion = false;

                if (animatorController != null)
                {
                    animatorController.SetAnimator(activeAnimator);
                }

                // Animation Events call a function on whatever GameObject
                // owns the Animator — this freshly instantiated clone, not
                // the player root — so the receiver has to be (re)added
                // here every swap instead of wired once in the Editor.
                if (combatController != null)
                {
                    var relay = activeAnimator.gameObject.AddComponent<PlayerAnimationEventRelay>();
                    relay.Initialize(combatController);
                }
            }

            GenderChanged?.Invoke(newGender);
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.characterGenderIndex = (int)CurrentGender;
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            SetGender((CharacterGender)data.characterGenderIndex);
        }
    }
}
