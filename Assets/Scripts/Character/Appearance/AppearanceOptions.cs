using System;
using System.Collections.Generic;
using System.Text;
using Project.Character.Animation;
using Project.Character.Hair;
using Project.Character.Head;
using Project.Persistence;

namespace Project.Character.Appearance
{
    /// <summary>
    /// The selectable values of every appearance setting, built from the hairstyle catalog, the
    /// face part catalog and the color enums. Shared by the character creation screen, which lists
    /// them, and by <see cref="AppearanceApplier"/>, which turns stored names back into indices.
    /// </summary>
    public sealed class AppearanceOptions
    {
        private readonly List<string> headIds = new List<string>();
        private readonly List<string> headLabels = new List<string>();
        private readonly List<string> skinToneLabels = new List<string>();
        private readonly List<string> hairStyleIds = new List<string>();
        private readonly List<string> hairStyleLabels = new List<string>();
        private readonly List<string> hairColorLabels = new List<string>();
        private readonly List<string> eyeColorLabels = new List<string>();
        private readonly List<string> eyeIds = new List<string>();
        private readonly List<string> mouthIds = new List<string>();

        /// <summary>
        /// Creates the option lists.
        /// </summary>
        /// <param name="hairCatalog">The hairstyle catalog, or null when there is none.</param>
        /// <param name="faceCatalog">The eye and mouth catalog, or null when there is none.</param>
        /// <param name="headCatalog">The head shape catalog, or null when there is none.</param>
        public AppearanceOptions(HairCatalog hairCatalog, FacePartCatalog faceCatalog, HeadCatalog headCatalog)
        {
            HairCatalog = hairCatalog;
            HeadCatalog = headCatalog;
            FaceCatalog = faceCatalog;

            if (headCatalog != null)
            {
                foreach (HeadStyleEntry entry in headCatalog.Styles)
                {
                    headIds.Add(entry.Id);
                    headLabels.Add(entry.DisplayName);
                }
            }

            if (hairCatalog != null)
            {
                foreach (HairStyleEntry entry in hairCatalog.Styles)
                {
                    hairStyleIds.Add(entry.Id);
                    hairStyleLabels.Add(entry.DisplayName);
                }
            }

            if (faceCatalog != null)
            {
                eyeIds.AddRange(faceCatalog.EyeNames);
                mouthIds.AddRange(faceCatalog.MouthNames);
            }

            AddEnumLabels<SkinTone>(skinToneLabels);
            AddEnumLabels<HairColor>(hairColorLabels);
            AddEnumLabels<EyeColor>(eyeColorLabels);
        }

        /// <summary>The hairstyle catalog these options were built from.</summary>
        public HairCatalog HairCatalog { get; }

        /// <summary>The head shape catalog these options were built from.</summary>
        public HeadCatalog HeadCatalog { get; }

        /// <summary>Stable identifiers of the head shapes, in selection order.</summary>
        public IReadOnlyList<string> HeadIds => headIds;

        /// <summary>Display names of the head shapes, in selection order.</summary>
        public IReadOnlyList<string> HeadLabels => headLabels;

        /// <summary>The eye and mouth catalog these options were built from.</summary>
        public FacePartCatalog FaceCatalog { get; }

        /// <summary>Display names of the skin tones, from the lightest to the darkest.</summary>
        public IReadOnlyList<string> SkinToneLabels => skinToneLabels;

        /// <summary>Stable identifiers of the hairstyles, in selection order.</summary>
        public IReadOnlyList<string> HairStyleIds => hairStyleIds;

        /// <summary>Display names of the hairstyles, in selection order.</summary>
        public IReadOnlyList<string> HairStyleLabels => hairStyleLabels;

        /// <summary>Display names of the hair colors, in enum order.</summary>
        public IReadOnlyList<string> HairColorLabels => hairColorLabels;

        /// <summary>Identifiers of the eye drawings, in selection order.</summary>
        public IReadOnlyList<string> EyeIds => eyeIds;

        /// <summary>Display names of the iris colors, in enum order.</summary>
        public IReadOnlyList<string> EyeColorLabels => eyeColorLabels;

        /// <summary>Identifiers of the mouth drawings, in selection order.</summary>
        public IReadOnlyList<string> MouthIds => mouthIds;

        /// <summary>
        /// Creates the appearance a new character starts with: the first entry of every list.
        /// </summary>
        /// <returns>The default appearance, already marked as chosen.</returns>
        public CharacterAppearanceData CreateDefault()
        {
            return new CharacterAppearanceData
            {
                isSet = true,
                skinToneIndex = 0,
                headModelId = FirstOrEmpty(headIds),
                hairStyleId = FirstOrEmpty(hairStyleIds),
                hairColorIndex = 0,
                eyeStyleId = FirstOrEmpty(eyeIds),
                eyeColorIndex = 0,
                mouthStyleId = FirstOrEmpty(mouthIds)
            };
        }

        /// <summary>
        /// Finds the position of an identifier in one of the option lists.
        /// </summary>
        /// <param name="ids">The list to search.</param>
        /// <param name="id">The identifier to find.</param>
        /// <returns>The index, or -1 when the identifier is not in the list.</returns>
        public static int IndexOf(IReadOnlyList<string> ids, string id)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string FirstOrEmpty(List<string> ids)
        {
            return ids.Count > 0 ? ids[0] : "";
        }

        private static void AddEnumLabels<TEnum>(List<string> labels) where TEnum : struct, Enum
        {
            foreach (string name in Enum.GetNames(typeof(TEnum)))
            {
                labels.Add(SplitWords(name));
            }
        }

        private static string SplitWords(string name)
        {
            var builder = new StringBuilder(name.Length + 2);

            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                {
                    builder.Append(' ');
                }

                builder.Append(name[i]);
            }

            return builder.ToString();
        }
    }
}
