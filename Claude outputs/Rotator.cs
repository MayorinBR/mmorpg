using UnityEngine;

namespace MMORPG.Misc
{
    /// <summary>
    /// Rotates the GameObject continuously around a single local axis at a configurable angular speed.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        private enum Axis
        {
            X,
            Y,
            Z
        }

        [SerializeField]
        private Axis rotationAxis = Axis.Y;

        [SerializeField]
        private float angularSpeed = 90f;

        private Vector3 RotationVector => rotationAxis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => Vector3.up
        };

        private void Update()
        {
            transform.Rotate(RotationVector, angularSpeed * Time.deltaTime, Space.Self);
        }
    }
}
