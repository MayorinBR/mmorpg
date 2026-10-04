using UnityEngine;
using Project.Character.Animation;
using Project.Character.Hair;
using Project.Character.Head;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Character.Appearance
{
    /// <summary>
    /// Holds the look of the player character, applies it to whichever model is currently shown and
    /// saves it with the rest of the character. Whoever swaps the model (the gender controller)
    /// calls <see cref="ApplyTo"/> with the new instance, so the look survives model changes.
    /// </summary>
    public sealed class PlayerAppearanceController : MonoBehaviour, ISaveParticipant
    {
        [Tooltip("Head shape catalog used to resolve the stored head name.")]
        [SerializeField] private HeadCatalog headCatalog;

        [Tooltip("Hairstyle catalog used to resolve the stored hairstyle name.")]
        [SerializeField] private HairCatalog hairCatalog;

        [Tooltip("Eye and mouth catalog used to resolve the stored drawing names.")]
        [SerializeField] private FacePartCatalog faceCatalog;

        private AppearanceOptions options;
        private CharacterAppearanceData current;
        private GameObject model;
        private CharacterGender gender;

        /// <summary>
        /// The current appearance, or null when none was chosen and the models keep their own look.
        /// </summary>
        public CharacterAppearanceData Current => current;

        private AppearanceOptions Options => options ??= new AppearanceOptions(hairCatalog, faceCatalog, headCatalog);

        /// <summary>
        /// Sets the appearance and applies it to the current model, if there is one.
        /// </summary>
        /// <param name="data">The appearance to use. A copy is stored.</param>
        public void SetAppearance(CharacterAppearanceData data)
        {
            current = data?.Clone();
            Reapply();
        }

        /// <summary>
        /// Applies the current appearance to a newly shown model and remembers it, so a later
        /// appearance change reaches it too.
        /// </summary>
        /// <param name="newModel">Root of the instantiated character model.</param>
        /// <param name="newGender">Gender of the model.</param>
        public void ApplyTo(GameObject newModel, CharacterGender newGender)
        {
            model = newModel;
            gender = newGender;
            Reapply();
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            if (current != null)
            {
                data.appearance = current.Clone();
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            if (data.appearance != null && data.appearance.isSet)
            {
                SetAppearance(data.appearance);
            }
        }

        private void Reapply()
        {
            if (model != null && current != null)
            {
                AppearanceApplier.Apply(model, current, gender, Options);
            }
        }
    }
}
