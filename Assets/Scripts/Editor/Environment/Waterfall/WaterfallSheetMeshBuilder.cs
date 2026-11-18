using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds the ribbon of falling water along a <see cref="WaterfallTrajectory"/>. The rows are spaced evenly in
    /// time, so they spread out as the water speeds up. The V coordinate is the time in seconds since the cave
    /// entrance, which lets the shader scroll features at the real speed of the water.
    /// </summary>
    public static class WaterfallSheetMeshBuilder
    {
        private const int Columns = 12;
        private const int CaveRows = 8;
        private const int FallRows = 48;
        private const float CaveStartWidthFactor = 0.85f;
        private const float SpreadExponent = 0.8f;
        private const float FadeInEnd = 0.6f;
        private const float FadeOutStart = 0.92f;

        /// <summary>
        /// Builds the sheet mesh in world space.
        /// </summary>
        /// <param name="profile">Profile with the sheet widths.</param>
        /// <param name="trajectory">Path the water follows.</param>
        /// <returns>The sheet mesh.</returns>
        public static Mesh Build(WaterfallProfile profile, WaterfallTrajectory trajectory)
        {
            var times = BuildTimes(trajectory);
            var vertices = new List<Vector3>(times.Count * (Columns + 1));
            var uvs = new List<Vector2>(vertices.Capacity);
            var colors = new List<Color>(vertices.Capacity);

            foreach (var time in times)
            {
                var center = trajectory.PositionAt(time);
                var width = WidthAt(profile, trajectory, time);
                var aeration = Mathf.Clamp01(time / trajectory.FallTime);
                var fade = FadeAt(trajectory, time);

                for (var column = 0; column <= Columns; column++)
                {
                    var u = column / (float)Columns;
                    vertices.Add(center + Vector3.right * ((u - 0.5f) * width));
                    uvs.Add(new Vector2(u, time + trajectory.CaveTime));
                    colors.Add(new Color(1f, aeration, 0f, fade));
                }
            }

            var triangles = new List<int>();
            for (var row = 0; row < times.Count - 1; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    var a = row * (Columns + 1) + column;
                    var b = a + 1;
                    var c = a + Columns + 1;
                    var d = c + 1;
                    triangles.AddRange(new[] { a, c, b, b, c, d });
                }
            }

            var mesh = new Mesh { name = "Waterfall_Sheet" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static List<float> BuildTimes(WaterfallTrajectory trajectory)
        {
            var times = new List<float>(CaveRows + FallRows + 1);
            for (var i = 0; i < CaveRows; i++)
            {
                times.Add(-trajectory.CaveTime * (1f - i / (float)CaveRows));
            }

            for (var i = 0; i <= FallRows; i++)
            {
                times.Add(trajectory.FallTime * i / FallRows);
            }

            return times;
        }

        private static float WidthAt(WaterfallProfile profile, WaterfallTrajectory trajectory, float time)
        {
            if (time < 0f)
            {
                var along = 1f + time / trajectory.CaveTime;
                return Mathf.Lerp(profile.SheetWidthAtLip * CaveStartWidthFactor, profile.SheetWidthAtLip, along);
            }

            var progress = Mathf.Pow(time / trajectory.FallTime, SpreadExponent);
            return Mathf.Lerp(profile.SheetWidthAtLip, profile.SheetWidthAtBase, progress);
        }

        private static float FadeAt(WaterfallTrajectory trajectory, float time)
        {
            if (time < 0f)
            {
                return Mathf.SmoothStep(0f, 1f, (time + trajectory.CaveTime) / trajectory.CaveTime / FadeInEnd);
            }

            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FadeOutStart, 1f, time / trajectory.FallTime));
        }
    }
}
