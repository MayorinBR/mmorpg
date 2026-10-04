using System;

namespace Project.Persistence
{
    /// <summary>
    /// Plain snapshot of how a character looks: skin tone, head shape, hairstyle, hair color, eyes, eye color and mouth.
    /// Head, hairstyle, eye and mouth choices are stored by name so that adding, removing or reordering
    /// items in the catalogs never changes the look of characters that were already saved. Colors
    /// are stored as the index of their enum value. Serialized with
    /// <see cref="UnityEngine.JsonUtility"/> inside <see cref="PlayerSaveData"/> and
    /// <see cref="PendingCharacterData"/>.
    /// </summary>
    [Serializable]
    public class CharacterAppearanceData
    {
        /// <summary>
        /// Whether a look was chosen. When false, the model keeps its own default look.
        /// </summary>
        public bool isSet;

        /// <summary>Name of the hairstyle model. Empty means no hair.</summary>
        public string hairStyleId = "";

        /// <summary>The chosen hair color, as a <c>HairColor</c> index.</summary>
        public int hairColorIndex;

        /// <summary>Name of the eye drawing.</summary>
        public string eyeStyleId = "";

        /// <summary>The chosen iris color, as an <c>EyeColor</c> index.</summary>
        public int eyeColorIndex;

        /// <summary>The chosen skin tone, as a <c>SkinTone</c> index.</summary>
        public int skinToneIndex;

        /// <summary>Name of the mouth drawing.</summary>
        public string mouthStyleId = "";

        /// <summary>Identifier of the head shape. Empty means the head that comes with the body.</summary>
        public string headModelId = "";

        /// <summary>Reserved for the body model choice. Not used yet.</summary>
        public string bodyModelId = "";

        /// <summary>
        /// Creates an independent copy of this appearance.
        /// </summary>
        /// <returns>The copy.</returns>
        public CharacterAppearanceData Clone()
        {
            return (CharacterAppearanceData)MemberwiseClone();
        }
    }
}
