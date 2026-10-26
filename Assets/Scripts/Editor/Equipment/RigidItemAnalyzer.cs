using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Measures rigid item models from their mesh vertices in world space, so mounting code does not depend on the
    /// axes, units or node rotations a model was exported with.
    /// </summary>
    internal static class RigidItemAnalyzer
    {
        private const int SliceCount = 24;

        /// <summary>
        /// Describes a bladed weapon measured in world space.
        /// </summary>
        public readonly struct BladeMeasure
        {
            /// <summary>
            /// Creates a blade measurement.
            /// </summary>
            /// <param name="tipDirection">Unit direction from the grip towards the tip.</param>
            /// <param name="guardDirection">Unit direction along the widest part of the cross guard.</param>
            /// <param name="gripCenter">World position of the middle of the grip.</param>
            /// <param name="length">Total length along the blade axis.</param>
            public BladeMeasure(Vector3 tipDirection, Vector3 guardDirection, Vector3 gripCenter, float length)
            {
                TipDirection = tipDirection;
                GuardDirection = guardDirection;
                GripCenter = gripCenter;
                Length = length;
            }

            /// <summary>Gets the unit direction from the grip towards the tip.</summary>
            public Vector3 TipDirection { get; }

            /// <summary>Gets the unit direction along the widest part of the cross guard.</summary>
            public Vector3 GuardDirection { get; }

            /// <summary>Gets the world position of the middle of the grip.</summary>
            public Vector3 GripCenter { get; }

            /// <summary>Gets the total length along the blade axis.</summary>
            public float Length { get; }
        }

        /// <summary>
        /// Describes a flat, round item such as a shield, measured in world space.
        /// </summary>
        public readonly struct FaceMeasure
        {
            /// <summary>
            /// Creates a face measurement.
            /// </summary>
            /// <param name="faceDirection">Unit direction the decorated front side points to.</param>
            /// <param name="center">World center of the bounds.</param>
            /// <param name="diameter">Largest size across the face.</param>
            /// <param name="thickness">Size along the face direction.</param>
            public FaceMeasure(Vector3 faceDirection, Vector3 center, float diameter, float thickness)
            {
                FaceDirection = faceDirection;
                Center = center;
                Diameter = diameter;
                Thickness = thickness;
            }

            /// <summary>Gets the unit direction the decorated front side points to.</summary>
            public Vector3 FaceDirection { get; }

            /// <summary>Gets the world center of the bounds.</summary>
            public Vector3 Center { get; }

            /// <summary>Gets the largest size across the face.</summary>
            public float Diameter { get; }

            /// <summary>Gets the size along the face direction.</summary>
            public float Thickness { get; }
        }

        /// <summary>
        /// Gets the combined renderer bounds of an object.
        /// </summary>
        /// <param name="target">The object to measure.</param>
        /// <returns>The world-space bounds, or an empty box at the object when it has no renderer.</returns>
        public static Bounds RendererBounds(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                return new Bounds(target.transform.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Measures a bladed weapon: the blade axis is the longest bounds axis, the cross guard is the widest slice
        /// along it, the tip is the end farthest from the guard and the grip sits between the guard and the pommel.
        /// </summary>
        /// <param name="instance">The weapon instance.</param>
        /// <param name="measure">The result when the method returns true.</param>
        /// <returns>True when the instance has mesh data to measure.</returns>
        public static bool TryMeasureBlade(GameObject instance, out BladeMeasure measure)
        {
            measure = default;
            var points = WorldVertices(instance);

            if (points.Count == 0)
            {
                return false;
            }

            var bounds = BoundsOf(points);
            var axisIndex = LongestAxis(bounds.size);
            var length = bounds.size[axisIndex];

            if (length <= 0f)
            {
                return false;
            }

            var min = bounds.min[axisIndex];
            var max = bounds.max[axisIndex];
            var widestSquared = new float[SliceCount];
            var widest = new Vector3[SliceCount];

            foreach (var point in points)
            {
                var slice = Mathf.Clamp((int)((point[axisIndex] - min) / length * SliceCount), 0, SliceCount - 1);
                var lateral = point - bounds.center;
                lateral[axisIndex] = 0f;

                if (lateral.sqrMagnitude > widestSquared[slice])
                {
                    widestSquared[slice] = lateral.sqrMagnitude;
                    widest[slice] = lateral;
                }
            }

            var guardSlice = 0;

            for (var slice = 1; slice < SliceCount; slice++)
            {
                if (widestSquared[slice] > widestSquared[guardSlice])
                {
                    guardSlice = slice;
                }
            }

            var guardPosition = min + (guardSlice + 0.5f) / SliceCount * length;
            var tipAtMax = guardPosition - min < max - guardPosition;
            var pommelEnd = tipAtMax ? min : max;
            var gripCenter = bounds.center;
            gripCenter[axisIndex] = (guardPosition + pommelEnd) * 0.5f;

            var axis = AxisVector(axisIndex);
            var guardDirection = widest[guardSlice].sqrMagnitude > 0f ? widest[guardSlice].normalized : Vector3.up;
            measure = new BladeMeasure(tipAtMax ? axis : -axis, guardDirection, gripCenter, length);
            return true;
        }

        /// <summary>
        /// Measures a shield-like item: the thinnest bounds axis is its thickness and the front is the side whose
        /// surface reaches farthest from the median, which is where the boss or ornament sits.
        /// </summary>
        /// <param name="instance">The item instance.</param>
        /// <param name="measure">The result when the method returns true.</param>
        /// <returns>True when the instance has mesh data to measure.</returns>
        public static bool TryMeasureFace(GameObject instance, out FaceMeasure measure)
        {
            measure = default;
            var points = WorldVertices(instance);

            if (points.Count == 0)
            {
                return false;
            }

            var bounds = BoundsOf(points);
            var thinIndex = ShortestAxis(bounds.size);
            var values = new List<float>(points.Count);

            foreach (var point in points)
            {
                values.Add(point[thinIndex]);
            }

            values.Sort();
            var median = values[values.Count / 2];
            var frontIsMax = bounds.max[thinIndex] - median > median - bounds.min[thinIndex];
            var face = AxisVector(thinIndex) * (frontIsMax ? 1f : -1f);
            var diameter = Mathf.Max(bounds.size[(thinIndex + 1) % 3], bounds.size[(thinIndex + 2) % 3]);
            measure = new FaceMeasure(face, bounds.center, diameter, bounds.size[thinIndex]);
            return true;
        }

        private static List<Vector3> WorldVertices(GameObject instance)
        {
            var points = new List<Vector3>();
            var buffer = new List<Vector3>();

            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                filter.sharedMesh.GetVertices(buffer);
                var matrix = filter.transform.localToWorldMatrix;

                foreach (var vertex in buffer)
                {
                    points.Add(matrix.MultiplyPoint3x4(vertex));
                }
            }

            return points;
        }

        private static Bounds BoundsOf(List<Vector3> points)
        {
            var bounds = new Bounds(points[0], Vector3.zero);

            foreach (var point in points)
            {
                bounds.Encapsulate(point);
            }

            return bounds;
        }

        private static int LongestAxis(Vector3 size)
        {
            return size.x >= size.y && size.x >= size.z ? 0 : size.y >= size.z ? 1 : 2;
        }

        private static int ShortestAxis(Vector3 size)
        {
            return size.x <= size.y && size.x <= size.z ? 0 : size.y <= size.z ? 1 : 2;
        }

        private static Vector3 AxisVector(int index)
        {
            return index == 0 ? Vector3.right : index == 1 ? Vector3.up : Vector3.forward;
        }
    }
}
