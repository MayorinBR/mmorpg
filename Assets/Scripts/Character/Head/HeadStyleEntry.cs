using System;
using UnityEngine;

namespace Project.Character.Head
{
    /// <summary>
    /// One selectable head shape. A head shape has one model per body type because the male and
    /// female heads differ. An entry without models stands for the head that comes with the body.
    /// </summary>
    [Serializable]
    public sealed class HeadStyleEntry
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private GameObject femaleModel;
        [SerializeField] private GameObject maleModel;

        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="id">Stable identifier stored in the save data.</param>
        /// <param name="displayName">Name shown on the character creation screen.</param>
        /// <param name="femaleModel">Head model for the female body, or null.</param>
        /// <param name="maleModel">Head model for the male body, or null.</param>
        public HeadStyleEntry(string id, string displayName, GameObject femaleModel, GameObject maleModel)
        {
            this.id = id;
            this.displayName = displayName;
            this.femaleModel = femaleModel;
            this.maleModel = maleModel;
        }

        /// <summary>Stable identifier of the head shape.</summary>
        public string Id => id;

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Gets the model of this head shape for a body type.
        /// </summary>
        /// <param name="female">True for the female body, false for the male body.</param>
        /// <returns>The model, or null when this entry keeps the head of the body.</returns>
        public GameObject GetModel(bool female)
        {
            return female ? femaleModel : maleModel;
        }
    }
}
