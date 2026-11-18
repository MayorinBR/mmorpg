using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds the foam patch at the foot of the fall by copying the part of the river mesh around the landing point
    /// and lifting it slightly, so the foam follows the slope of the water and never spills onto the banks.
    /// </summary>
    public static class WaterfallFoamMeshBuilder
    {
        /// <summary>
        /// Builds the foam patch in world space with a UV that is centered on the landing point.
        /// </summary>
        /// <param name="river">River mesh to copy from.</param>
        /// <param name="riverTransform">Transform that places the river mesh in the world.</param>
        /// <param name="center">World X and Z of the landing point.</param>
        /// <param name="radius">Radius of the patch, in meters.</param>
        /// <param name="lift">How far above the water the patch floats, in meters.</param>
        /// <returns>The foam mesh, or an empty mesh when the river has no triangle in range.</returns>
        public static Mesh Build(Mesh river, Transform riverTransform, Vector2 center, float radius, float lift)
        {
            var source = river.vertices;
            var triangles = river.triangles;
            var remap = new Dictionary<int, int>();
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var kept = new List<int>();

            for (var i = 0; i < triangles.Length; i += 3)
            {
                if (!IsInRange(source, triangles, i, riverTransform, center, radius))
                {
                    continue;
                }

                for (var k = 0; k < 3; k++)
                {
                    var index = triangles[i + k];
                    if (!remap.TryGetValue(index, out var mapped))
                    {
                        var world = riverTransform.TransformPoint(source[index]);
                        mapped = vertices.Count;
                        remap[index] = mapped;
                        vertices.Add(world + Vector3.up * lift);
                        uvs.Add(new Vector2((world.x - center.x) / radius * 0.5f + 0.5f, (world.z - center.y) / radius * 0.5f + 0.5f));
                    }

                    kept.Add(mapped);
                }
            }

            var mesh = new Mesh { name = "Waterfall_FoamPatch" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(kept, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static bool IsInRange(Vector3[] source, int[] triangles, int start, Transform transform, Vector2 center, float radius)
        {
            for (var k = 0; k < 3; k++)
            {
                var world = transform.TransformPoint(source[triangles[start + k]]);
                if (new Vector2(world.x - center.x, world.z - center.y).sqrMagnitude > radius * radius)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
