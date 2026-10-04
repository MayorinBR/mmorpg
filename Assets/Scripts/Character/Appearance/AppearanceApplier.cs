using System;
using UnityEngine;
using Project.Character.Animation;
using Project.Character.Hair;
using Project.Character.Head;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Character.Appearance
{
    /// <summary>
    /// Writes a <see cref="CharacterAppearanceData"/> into a character model. Used for the model in
    /// the gameplay scene and for the preview on the character creation screen, so both always
    /// look the same. The face and hair components are added to the model when it does not have
    /// them yet.
    /// </summary>
    public static class AppearanceApplier
    {
        /// <summary>
        /// Applies the head shape, the skin tone, the face and the hair of an appearance to a model.
        /// </summary>
        /// <param name="model">Root of the instantiated character model.</param>
        /// <param name="data">The appearance to apply. Nothing happens when it was not chosen.</param>
        /// <param name="gender">Gender of the model, which selects the hair placement for its body.</param>
        /// <param name="options">The option lists used to resolve stored names.</param>
        public static void Apply(GameObject model, CharacterAppearanceData data, CharacterGender gender, AppearanceOptions options)
        {
            if (model == null || data == null || !data.isSet || options == null)
            {
                return;
            }

            ApplyHead(model, data, gender, options);
            SkinTint.Apply(model, SkinTonePalette.Resolve((SkinTone)ClampToEnum<SkinTone>(data.skinToneIndex)));
            ApplyFace(model, data, options);
            ApplyHair(model, data, gender, options);
        }

        private static void ApplyHead(GameObject model, CharacterAppearanceData data, CharacterGender gender, AppearanceOptions options)
        {
            HeadCustomization head = model.GetComponentInChildren<HeadCustomization>(true);
            if (head == null)
            {
                head = model.AddComponent<HeadCustomization>();
            }

            head.Configure(options.HeadCatalog, gender == CharacterGender.Female);
            head.Apply(Mathf.Max(0, AppearanceOptions.IndexOf(options.HeadIds, data.headModelId)));
        }

        private static void ApplyFace(GameObject model, CharacterAppearanceData data, AppearanceOptions options)
        {
            FaceCustomization face = model.GetComponentInChildren<FaceCustomization>(true);
            if (face == null)
            {
                face = model.AddComponent<FaceCustomization>();
            }

            int eyes = Mathf.Max(0, AppearanceOptions.IndexOf(options.EyeIds, data.eyeStyleId));
            int mouth = Mathf.Max(0, AppearanceOptions.IndexOf(options.MouthIds, data.mouthStyleId));
            face.Apply((EyeColor)ClampToEnum<EyeColor>(data.eyeColorIndex), eyes, mouth);
        }

        private static void ApplyHair(GameObject model, CharacterAppearanceData data, CharacterGender gender, AppearanceOptions options)
        {
            HairCustomization hair = model.GetComponentInChildren<HairCustomization>(true);
            if (hair == null)
            {
                hair = model.AddComponent<HairCustomization>();
            }

            HairBodyType body = gender == CharacterGender.Female ? HairBodyType.Female : HairBodyType.Male;
            hair.Configure(options.HairCatalog, body);
            hair.Apply(ResolveHairStyle(data.hairStyleId, options), (HairColor)ClampToEnum<HairColor>(data.hairColorIndex));
        }

        private static int ResolveHairStyle(string id, AppearanceOptions options)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HairCustomization.NoHair;
            }

            int index = AppearanceOptions.IndexOf(options.HairStyleIds, id);
            if (index < 0)
            {
                Debug.LogWarning($"Hairstyle '{id}' is not in the hair catalog. The character is shown without hair.");
                return HairCustomization.NoHair;
            }

            return index;
        }

        private static int ClampToEnum<TEnum>(int index) where TEnum : struct, Enum
        {
            return Mathf.Clamp(index, 0, Enum.GetValues(typeof(TEnum)).Length - 1);
        }
    }
}
