using System;
using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// One selectable hairstyle: a display name, the model that is attached to the head, and the
    /// fine adjustment of that model for each body type.
    /// </summary>
    [Serializable]
    public sealed class HairStyleEntry
    {
        [SerializeField] private string displayName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private HairPlacement malePlacement = new HairPlacement();
        [SerializeField] private HairPlacement femalePlacement = new HairPlacement();

        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="displayName">Name shown in selection lists.</param>
        /// <param name="prefab">Model attached to the head socket.</param>
        public HairStyleEntry(string displayName, GameObject prefab)
        {
            this.displayName = displayName;
            this.prefab = prefab;
        }

        /// <summary>
        /// Name shown in selection lists.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Model attached to the head socket.
        /// </summary>
        public GameObject Prefab => prefab;

        /// <summary>
        /// Stable identifier of this hairstyle, the name of its model. Unlike the position in the
        /// catalog, it does not change when other hairstyles are added or removed.
        /// </summary>
        public string Id => prefab != null ? prefab.name : displayName;

        /// <summary>
        /// Returns the adjustment of this hairstyle for a body type.
        /// </summary>
        /// <param name="bodyType">Body the hair is worn on.</param>
        /// <returns>The stored placement, or no adjustment when none was stored.</returns>
        public HairPlacement GetPlacement(HairBodyType bodyType)
        {
            HairPlacement stored = bodyType == HairBodyType.Female ? femalePlacement : malePlacement;
            return stored ?? new HairPlacement();
        }

        /// <summary>
        /// Stores the adjustment of this hairstyle for a body type.
        /// </summary>
        /// <param name="bodyType">Body the hair is worn on.</param>
        /// <param name="placement">The adjustment to store.</param>
        public void SetPlacement(HairBodyType bodyType, HairPlacement placement)
        {
            if (bodyType == HairBodyType.Female)
            {
                femalePlacement = placement;
            }
            else
            {
                malePlacement = placement;
            }
        }

        /// <summary>
        /// Copies the adjustments of another entry, used to keep them when the catalog is rebuilt.
        /// </summary>
        /// <param name="source">The entry to copy from.</param>
        public void CopyPlacementsFrom(HairStyleEntry source)
        {
            malePlacement = source.GetPlacement(HairBodyType.Male);
            femalePlacement = source.GetPlacement(HairBodyType.Female);
        }
    }
}
