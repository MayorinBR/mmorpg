using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Marks an item model as a skinned piece of equipment and lists the body regions it covers.
    /// Placed on the root of an item prefab whose children are skinned meshes exported with the
    /// character skeleton.
    /// </summary>
    public sealed class SkinnedEquipment : MonoBehaviour
    {
        [Tooltip("Body regions hidden while this piece is equipped.")]
        [SerializeField] private BodyPart coveredParts;

        /// <summary>Gets the body regions this piece replaces.</summary>
        public BodyPart CoveredParts => coveredParts;
    }
}
