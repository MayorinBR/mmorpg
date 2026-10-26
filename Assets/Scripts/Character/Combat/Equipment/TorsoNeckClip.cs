using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Keeps only the part of a body renderer above a plane that follows the neck, so a covered torso still shows the neck.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class TorsoNeckClip : MonoBehaviour
    {
        private static readonly int ClipPlaneId = Shader.PropertyToID("_ClipPlane");

        private Renderer target;
        private MaterialPropertyBlock block;
        private Transform lowerBone;
        private Transform upperBone;
        private float height;

        /// <summary>
        /// Starts clipping the renderer along the line between two bones.
        /// </summary>
        /// <param name="lower">Bone at the bottom of the cut line.</param>
        /// <param name="upper">Bone at the top of the cut line.</param>
        /// <param name="heightFraction">Position of the cut from <paramref name="lower"/> (0) to <paramref name="upper"/> (1).</param>
        public void Enable(Transform lower, Transform upper, float heightFraction)
        {
            lowerBone = lower;
            upperBone = upper;
            height = heightFraction;
            enabled = true;
            Apply();
        }

        /// <summary>
        /// Stops clipping and shows the whole renderer again.
        /// </summary>
        public void Disable()
        {
            enabled = false;
            Write(Vector4.zero);
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            if (lowerBone == null || upperBone == null)
            {
                return;
            }

            var span = upperBone.position - lowerBone.position;

            if (span.sqrMagnitude < 1e-8f)
            {
                return;
            }

            var normal = span.normalized;
            var point = lowerBone.position + span * height;
            Write(new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point)));
        }

        private void Write(Vector4 plane)
        {
            if (target == null)
            {
                target = GetComponent<Renderer>();
            }

            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }

            target.GetPropertyBlock(block);
            block.SetVector(ClipPlaneId, plane);
            target.SetPropertyBlock(block);
        }
    }
}
