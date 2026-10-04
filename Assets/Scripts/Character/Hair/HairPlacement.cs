using System;
using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// Local offset of a hair model relative to the head socket. The default value is no
    /// adjustment, so a model authored with its origin on the socket needs no placement.
    /// </summary>
    [Serializable]
    public sealed class HairPlacement
    {
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Vector3 eulerAngles = Vector3.zero;
        [SerializeField] private Vector3 scale = Vector3.one;

        /// <summary>
        /// Local position relative to the socket.
        /// </summary>
        public Vector3 Position => position;

        /// <summary>
        /// Local rotation relative to the socket, in euler angles.
        /// </summary>
        public Vector3 EulerAngles => eulerAngles;

        /// <summary>
        /// Local scale relative to the socket.
        /// </summary>
        public Vector3 Scale => scale;

        /// <summary>
        /// Captures the current local transform of a hair model.
        /// </summary>
        /// <param name="hair">The hair model, already parented to the socket.</param>
        /// <returns>A placement that reproduces the local transform of the model.</returns>
        public static HairPlacement Capture(Transform hair)
        {
            return new HairPlacement
            {
                position = hair.localPosition,
                eulerAngles = hair.localEulerAngles,
                scale = hair.localScale
            };
        }

        /// <summary>
        /// Writes this placement to the local transform of a hair model.
        /// </summary>
        /// <param name="hair">The hair model, already parented to the socket.</param>
        public void ApplyTo(Transform hair)
        {
            hair.localPosition = position;
            hair.localRotation = Quaternion.Euler(eulerAngles);
            hair.localScale = scale;
        }
    }
}
