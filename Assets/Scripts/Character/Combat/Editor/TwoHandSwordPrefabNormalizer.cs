using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.Editor
{
    /// <summary>
    /// Rewrites a two-handed sword prefab so its geometry follows the project convention that the shared
    /// two-hand hold poses in the weapon animation catalog rely on: the blade points along +Z, the guard is
    /// widest along Y, the flat face of the blade has its normal on X, and the root origin sits at the centre
    /// of the grip. The root transform is left untouched; the rigid correction is applied to its direct children.
    /// </summary>
    public static class TwoHandSwordPrefabNormalizer
    {
        private const string MenuPath = "Tools/Weapons/Normalize Selected Two-Hand Sword Prefabs";
        private const int ProfileBins = 32;
        private const int PowerIterations = 64;

        /// <summary>Normalizes every prefab asset in the current selection.</summary>
        [MenuItem(MenuPath)]
        public static void NormalizeSelection()
        {
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);

                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                {
                    continue;
                }

                NormalizePrefab(path);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>Normalizes the prefab at the given asset path.</summary>
        /// <param name="assetPath">The prefab asset path.</param>
        public static void NormalizePrefab(string assetPath)
        {
            var root = PrefabUtility.LoadPrefabContents(assetPath);

            try
            {
                var points = CollectPoints(root.transform);

                if (points.Count == 0)
                {
                    Debug.LogWarning($"{assetPath}: no mesh found.");
                    return;
                }

                Analyze(points, out var blade, out var width, out var origin);
                var correction = Quaternion.Inverse(Quaternion.LookRotation(blade, width));

                foreach (Transform child in root.transform)
                {
                    child.localPosition = correction * (child.localPosition - origin);
                    child.localRotation = correction * child.localRotation;
                }

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
        /// Finds the blade direction (principal axis, pointing away from the pommel), the guard width direction,
        /// and the grip centre. The pommel end is the one nearer the widest cross-section (the guard).
        /// </summary>
        private static void Analyze(List<Vector3> points, out Vector3 blade, out Vector3 width, out Vector3 origin)
        {
            var mean = Vector3.zero;
            points.ForEach(p => mean += p);
            mean /= points.Count;

            var covariance = new float[3, 3];

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

            blade = PrincipalAxis(covariance, Vector3.right);
            Deflate(covariance, blade);
            width = PrincipalAxis(covariance, Vector3.up);
            width = Vector3.ProjectOnPlane(width, blade).normalized;

            var low = float.MaxValue;
            var high = float.MinValue;

            foreach (var p in points)
            {
                var t = Vector3.Dot(p - mean, blade);
                low = Mathf.Min(low, t);
                high = Mathf.Max(high, t);
            }

            var guardIndex = WidestBin(points, mean, blade, width, low, high);
            var guardPosition = low + ((guardIndex + 0.5f) / ProfileBins) * (high - low);

            if (guardPosition - low > high - guardPosition)
            {
                blade = -blade;
                var flippedLow = -high;
                high = -low;
                low = flippedLow;
                guardPosition = -guardPosition;
            }

            var gripCentre = (low + guardPosition) * 0.5f;
            var lateral = Vector3.zero;
            var count = 0;

            foreach (var p in points)
            {
                var t = Vector3.Dot(p - mean, blade);

                if (t >= low && t <= guardPosition)
                {
                    lateral += Vector3.ProjectOnPlane(p - mean, blade);
                    count++;
                }
            }

            lateral = count > 0 ? lateral / count : Vector3.zero;
            origin = mean + lateral + blade * gripCentre;
        }

        private static int WidestBin(List<Vector3> points, Vector3 mean, Vector3 blade, Vector3 width, float low, float high)
        {
            var min = new float[ProfileBins];
            var max = new float[ProfileBins];

            for (var i = 0; i < ProfileBins; i++)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            foreach (var p in points)
            {
                var d = p - mean;
                var bin = Mathf.Clamp((int)((Vector3.Dot(d, blade) - low) / (high - low) * ProfileBins), 0, ProfileBins - 1);
                var w = Vector3.Dot(d, width);
                min[bin] = Mathf.Min(min[bin], w);
                max[bin] = Mathf.Max(max[bin], w);
            }

            var best = 0;
            var bestWidth = float.MinValue;

            for (var i = 0; i < ProfileBins; i++)
            {
                if (max[i] - min[i] > bestWidth)
                {
                    bestWidth = max[i] - min[i];
                    best = i;
                }
            }

            return best;
        }

        private static Vector3 PrincipalAxis(float[,] matrix, Vector3 seed)
        {
            var v = seed;

            for (var n = 0; n < PowerIterations; n++)
            {
                var next = new Vector3(
                    matrix[0, 0] * v.x + matrix[0, 1] * v.y + matrix[0, 2] * v.z,
                    matrix[1, 0] * v.x + matrix[1, 1] * v.y + matrix[1, 2] * v.z,
                    matrix[2, 0] * v.x + matrix[2, 1] * v.y + matrix[2, 2] * v.z);
                v = next.sqrMagnitude > 0f ? next.normalized : v;
            }

            return v;
        }

        private static void Deflate(float[,] matrix, Vector3 axis)
        {
            var eigenvalue = 0f;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    eigenvalue += axis[i] * matrix[i, j] * axis[j];
                }
            }

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    matrix[i, j] -= eigenvalue * axis[i] * axis[j];
                }
            }
        }
    }
}
