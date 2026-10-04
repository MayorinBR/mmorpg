using UnityEngine;
using Project.Character.Animation;
using Project.Character.Appearance;
using Project.Character.Hair;
using Project.Character.Head;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Appearance panel of the Character Creation screen. Fills each selector row from the catalogs,
    /// keeps the chosen look in a <see cref="CharacterAppearanceData"/> and mirrors every change on the
    /// <see cref="CharacterPreview"/>. A new character starts with the first entry of every list.
    /// </summary>
    public sealed class CharacterAppearanceSelectionController : MonoBehaviour
    {
        [SerializeField] private HeadCatalog headCatalog;
        [SerializeField] private HairCatalog hairCatalog;
        [SerializeField] private FacePartCatalog faceCatalog;
        [SerializeField] private CharacterPreview preview;

        [Header("Selectors")]
        [SerializeField] private AppearanceOptionSelector skinToneSelector;
        [SerializeField] private AppearanceOptionSelector headModelSelector;
        [SerializeField] private AppearanceOptionSelector hairStyleSelector;
        [SerializeField] private AppearanceOptionSelector hairColorSelector;
        [SerializeField] private AppearanceOptionSelector eyeStyleSelector;
        [SerializeField] private AppearanceOptionSelector eyeColorSelector;
        [SerializeField] private AppearanceOptionSelector mouthStyleSelector;

        [Header("Reserved for future models (shown disabled)")]
        [SerializeField] private AppearanceOptionSelector bodyModelSelector;

        private AppearanceOptions options;
        private CharacterGender gender;

        /// <summary>Gets the appearance currently chosen on the screen.</summary>
        public CharacterAppearanceData Current { get; private set; }

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Shows the base model of a gender in the preview with the current appearance.
        /// </summary>
        /// <param name="newGender">The gender chosen on the screen.</param>
        public void SetGender(CharacterGender newGender)
        {
            EnsureInitialized();
            gender = newGender;
            preview.Show(gender, Current, options);
        }

        private void EnsureInitialized()
        {
            if (options != null)
            {
                return;
            }

            options = new AppearanceOptions(hairCatalog, faceCatalog, headCatalog);
            Current = options.CreateDefault();

            skinToneSelector?.Configure(options.SkinToneLabels, 0, i => Change(() => Current.skinToneIndex = i));
            headModelSelector?.Configure(options.HeadLabels, 0, i => Change(() => Current.headModelId = options.HeadIds[i]));
            hairStyleSelector.Configure(options.HairStyleLabels, 0, i => Change(() => Current.hairStyleId = options.HairStyleIds[i]));
            hairColorSelector.Configure(options.HairColorLabels, 0, i => Change(() => Current.hairColorIndex = i));
            eyeStyleSelector.Configure(options.EyeIds, 0, i => Change(() => Current.eyeStyleId = options.EyeIds[i]));
            eyeColorSelector.Configure(options.EyeColorLabels, 0, i => Change(() => Current.eyeColorIndex = i));
            mouthStyleSelector.Configure(options.MouthIds, 0, i => Change(() => Current.mouthStyleId = options.MouthIds[i]));

            bodyModelSelector?.Configure(null, -1, null);
        }

        private void Change(System.Action edit)
        {
            edit();
            preview.Refresh();
        }
    }
}
