using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.Editor
{
    /// <summary>
    /// Rewrites a rod or staff prefab so it sits in the player's hand exactly like the reference
    /// <c>ArcaneStaff</c> prefab: the shaft runs along the same direction, the flared end points the same way,
    /// and the point of the shaft that rests in the palm lands on the same spot. The root transform is left
    /// untouched; the rigid correction is applied to its direct children, and mesh scale is never changed.
    /// </summary>
    public static class RodPrefabNormalizer
    {
        private const string MenuPath = "Tools/Weapons/Normalize Selected Rod Prefabs";
        private const string ReferencePath = "Assets/Prefabs/Items/Weapons/Rod/ArcaneStaff.prefab";
        private const float EndFraction = 0.15f;
        private const int PowerIterations = 64;

        /// <summary>Normalizes every prefab asset in the current selection to match the reference rod.</summary>
        [MenuItem(MenuPath)]
        public static void NormalizeSelection()
        {
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);

                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab") || path == ReferencePath)
                {
                    continue;
                }

                NormalizePrefab(path);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>Normalizes the prefab at the given asset path so it is held like the reference rod.</summary>
        /// <param name="assetPath">The prefab asset path.</param>
        public static void NormalizePrefab(string assetPath)
        {
            if (!TryAnalyzePrefab(ReferencePath, out var reference))
            {
                Debug.LogWarning($"Reference rod not found or empty: {ReferencePath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(assetPath);

            try
            {
                var points = CollectPoints(root.transform);

                if (points.Count == 0)
                {
                    Debug.LogWarning($"{assetPath}: no mesh found.");
                    return;
                }

                var shape = Analyze(points);
                var gripFraction = (Vector3.Dot(-reference.Mean, reference.Axis) - reference.Low) / reference.Length;
                var referenceGrip = reference.Mean + reference.Axis * Vector3.Dot(-reference.Mean, reference.Axis);
                var targetGrip = shape.Mean + shape.Axis * (shape.Low + gripFraction * shape.Length);
                var rotation = Quaternion.LookRotation(reference.Axis, reference.Radial)
                               * Quaternion.Inverse(Quaternion.LookRotation(shape.Axis, shape.Radial));

                foreach (Transform child in root.transform)
                {
                    child.localPosition = rotation * (child.localPosition - targetGrip) + referenceGrip;
                    child.localRotation = rotation * child.localRotation;
                }

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool TryAnalyzePrefab(string assetPath, out Shape shape)
        {
            shape = default;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (asset == null)
            {
                return false;
            }

            var points = CollectPoints(asset.transform);

            if (points.Count == 0)
            {
                return false;
            }

            shape = Analyze(points);
            return true;
        }

        private static List<Vector3> CollectPoints(Transform root)
        {
            var points = new List<Vector3>();

            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                foreach (var vertex in filter.sharedMesh.vertices)
                {
                    points.Add(root.InverseTransformPoint(filter.transform.TransformPoint(vertex)));
                }
            }

            return points;
        }

        /// <summary>
        /// Finds the shaft direction (principal axis, pointing from the flared end to the thin end), the extent of
        /// the mesh along it, and the direction in which the flared end sticks out sideways, which fixes the roll.
        /// </summary>
        private static Shape Analyze(List<Vector3> points)
        {
            var mean = Vector3.zero;
            points.ForEach(p => mean += p);
            mean /= points.Count;

            var axis = PrincipalAxis(points, mean);
            GetExtent(points, mean, axis, out var low, out var high);

            if (MaxRadius(points, mean, axis, high - (high - low) * EndFraction, high) > MaxRadius(points, mean, axis, low, low + (high - low) * EndFraction))
            {
                axis = -axis;
                GetExtent(points, mean, axis, out low, out high);
            }

            var length = high - low;
            var radial = Vector3.right;
            var widest = -1f;

            foreach (var p in points)
            {
                var d = p - mean;
                var t = Vector3.Dot(d, axis);
                var side = Vector3.ProjectOnPlane(d, axis);

                if (t <= low + length * EndFraction && side.sqrMagnitude > widest)
                {
                    widest = side.sqrMagnitude;
                    radial = side;
                }
            }

            radial = Vector3.ProjectOnPlane(radial, axis).normalized;

            if (radial == Vector3.zero)
            {
                radial = Vector3.ProjectOnPlane(Vector3.up, axis).normalized;
            }

            return new Shape { Mean = mean, Axis = axis, Radial = radial, Low = low, Length = length };
        }

        private static void GetExtent(List<Vector3> points, Vector3 mean, Vector3 axis, out float low, out float high)
        {
            low = float.MaxValue;
            high = float.MinValue;

            foreach (var p in points)
            {
                var t = Vector3.Dot(p - mean, axis);
                low = Mathf.Min(low, t);
                high = Mathf.Max(high, t);
            }
        }

        private static float MaxRadius(List<Vector3> points, Vector3 mean, Vector3 axis, float from, float to)
        {
            var radius = 0f;

            foreach (var p in points)
            {
                var d = p - mean;
                var t = Vector3.Dot(d, axis);

                if (t >= from && t <= to)
                {
                    radius = Mathf.Max(radius, Vector3.ProjectOnPlane(d, axis).magnitude);
                }
            }

            return radius;
        }

        private static Vector3 PrincipalAxis(List<Vector3> points, Vector3 mean)
        {
            var covariance = new double[3, 3];

            foreach (var p in points)
            {
                var d = p - mean;

                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        covariance[i, j] += d[i] * d[j];
                    }
                }
            }

            var v = new Vector3(1f, 0.3f, 0.2f).normalized;

            for (var n = 0; n < PowerIterations; n++)
            {
                var next = new Vector3(
                    (float)(covariance[0, 0] * v.x + covariance[0, 1] * v.y + covariance[0, 2] * v.z),
                    (float)(covariance[1, 0] * v.x + covariance[1, 1] * v.y + covariance[1, 2] * v.z),
                    (float)(covariance[2, 0] * v.x + covariance[2, 1] * v.y + covariance[2, 2] * v.z));
                v = next.sqrMagnitude > 0f ? next.normalized : v;
            }

            return v;
        }

        private struct Shape
        {
            public Vector3 Mean;
            public Vector3 Axis;
            public Vector3 Radial;
            public float Low;
            public float Length;
        }
    }
}
