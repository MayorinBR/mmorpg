using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Places rigid equipment instances on a character in world space. Every method expects the item to be already
    /// parented where it should end up, and works on its world pose, so the result is independent of bone axes and
    /// of the scale of the model it hangs from. Items are first reset to the world origin, which is where
    /// the authored geometry sits relative to the body it was modelled for.
    /// </summary>
    internal static class EquipmentMountPlacer
    {
        /// <summary>
        /// Keeps the authored placement of an item, scaled about the world origin.
        /// </summary>
        /// <param name="item">The item root.</param>
        /// <param name="scale">World scale to apply.</param>
        public static void PlaceAuthored(Transform item, float scale)
        {
            ResetToOrigin(item);
            SetWorldScale(item, scale);
        }

        /// <summary>
        /// Fits a helmet around the top of a head.
        /// </summary>
        /// <param name="item">The helmet root.</param>
        /// <param name="head">The head bone.</param>
        /// <param name="targetWidth">World width the helmet should have.</param>
        /// <param name="heightAboveHead">Height of the helmet bottom above the head bone.</param>
        /// <param name="backShift">How far the helmet is moved towards the back of the head.</param>
        public static void PlaceHelmet(Transform item, Transform head, float targetWidth, float heightAboveHead, float backShift)
        {
            ResetToOrigin(item);
            var bounds = RigidItemAnalyzer.RendererBounds(item.gameObject);
            var scale = targetWidth / bounds.size.x;
            var bottom = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var anchor = head.position + Vector3.up * heightAboveHead + Vector3.back * backShift;
            SetWorldScale(item, scale);
            item.position = anchor - bottom * scale;
        }

        /// <summary>
        /// Lays a round shield on the outer side of the left forearm, with its face turned away from the arm.
        /// </summary>
        /// <param name="item">The shield root.</param>
        /// <param name="forearm">The elbow bone.</param>
        /// <param name="hand">The wrist bone, which gives the direction of the forearm.</param>
        /// <param name="diameter">World diameter of the shield.</param>
        /// <param name="clearance">Gap between the forearm axis and the back of the shield.</param>
        /// <returns>True when the shield could be measured.</returns>
        public static bool PlaceShield(Transform item, Transform forearm, Transform hand, float diameter, float clearance)
        {
            ResetToOrigin(item);

            if (!RigidItemAnalyzer.TryMeasureFace(item.gameObject, out var face))
            {
                return false;
            }

            var scale = diameter / face.Diameter;
            var armAxis = (hand.position - forearm.position).normalized;
            var normal = Vector3.Cross(armAxis, Vector3.forward).normalized;
            var rotation = Quaternion.FromToRotation(face.FaceDirection, normal);
            var midpoint = Vector3.Lerp(forearm.position, hand.position, 0.5f);
            var center = midpoint + normal * (clearance + face.Thickness * scale * 0.5f);
            SetWorldScale(item, scale);
            item.SetPositionAndRotation(center - rotation * (face.Center * scale), rotation);
            return true;
        }

        /// <summary>
        /// Holds a sword in a fist with the blade along a given direction and the cross guard vertical.
        /// </summary>
        /// <param name="item">The sword root.</param>
        /// <param name="forearm">The elbow bone.</param>
        /// <param name="hand">The wrist bone.</param>
        /// <param name="bladeDirection">World direction the tip should point to.</param>
        /// <param name="length">World length of the sword.</param>
        /// <param name="gripOffset">Distance from the wrist to the middle of the fist along the forearm.</param>
        /// <returns>True when the sword could be measured.</returns>
        public static bool PlaceSword(Transform item, Transform forearm, Transform hand, Vector3 bladeDirection, float length, float gripOffset)
        {
            ResetToOrigin(item);

            if (!RigidItemAnalyzer.TryMeasureBlade(item.gameObject, out var blade))
            {
                return false;
            }

            var direction = bladeDirection.normalized;
            var scale = length / blade.Length;
            var rotation = Quaternion.FromToRotation(blade.TipDirection, direction);
            var guard = rotation * blade.GuardDirection;
            var up = Vector3.ProjectOnPlane(Vector3.up, direction).normalized;
            var roll = Mathf.Repeat(Vector3.SignedAngle(guard, up, direction) + 90f, 180f) - 90f;
            rotation = Quaternion.AngleAxis(roll, direction) * rotation;

            var armAxis = (hand.position - forearm.position).normalized;
            var grip = hand.position + armAxis * gripOffset;
            SetWorldScale(item, scale);
            item.SetPositionAndRotation(grip - rotation * (blade.GripCenter * scale), rotation);
            return true;
        }

        private static void ResetToOrigin(Transform item)
        {
            item.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            SetWorldScale(item, 1f);
        }

        private static void SetWorldScale(Transform item, float scale)
        {
            var parent = item.parent != null ? item.parent.lossyScale : Vector3.one;
            item.localScale = new Vector3(scale / parent.x, scale / parent.y, scale / parent.z);
        }
    }
}
