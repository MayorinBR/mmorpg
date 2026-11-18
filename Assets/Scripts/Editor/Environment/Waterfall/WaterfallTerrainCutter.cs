using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Opens the terrain hole mask where the wall surface lies inside the mouth outline. The area around the mouth is
    /// reset first, so running it again with a different outline never leaves stale holes behind.
    /// </summary>
    public static class WaterfallTerrainCutter
    {
        private const float MarginX = 3f;
        private const float MarginZ = 2f;
        private const float TopMargin = 4f;

        /// <summary>
        /// Cuts the mouth into the terrain.
        /// </summary>
        /// <param name="terrain">Terrain that holds the wall.</param>
        /// <param name="profile">Profile with the mouth position and size.</param>
        /// <param name="outline">Outline of the opening.</param>
        /// <param name="wall">Terrain probe.</param>
        /// <returns>The number of terrain cells that were cut.</returns>
        public static int Cut(Terrain terrain, WaterfallProfile profile, MouthOutline outline, WaterfallWall wall)
        {
            var data = terrain.terrainData;
            var origin = terrain.transform.position;
            var resolution = data.holesResolution;

            var xMin = profile.CenterX - profile.MouthHalfWidth - profile.RimMaxWidth - MarginX;
            var xMax = profile.CenterX + profile.MouthHalfWidth + profile.RimMaxWidth + MarginX;
            var zMin = wall.SurfaceZ(profile.CenterX, profile.SillHeight) - MarginZ;
            var zMax = wall.SurfaceZ(profile.CenterX, profile.TopHeight + TopMargin) + MarginZ;

            var firstX = Mathf.Clamp(Mathf.FloorToInt((xMin - origin.x) / data.size.x * resolution), 0, resolution - 1);
            var lastX = Mathf.Clamp(Mathf.CeilToInt((xMax - origin.x) / data.size.x * resolution), 0, resolution - 1);
            var firstZ = Mathf.Clamp(Mathf.FloorToInt((zMin - origin.z) / data.size.z * resolution), 0, resolution - 1);
            var lastZ = Mathf.Clamp(Mathf.CeilToInt((zMax - origin.z) / data.size.z * resolution), 0, resolution - 1);

            var width = lastX - firstX + 1;
            var height = lastZ - firstZ + 1;
            var holes = new bool[height, width];
            var cut = 0;

            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    var worldX = origin.x + (firstX + col + 0.5f) / resolution * data.size.x;
                    var worldZ = origin.z + (firstZ + row + 0.5f) / resolution * data.size.z;
                    var inside = outline.Contains(new Vector2(worldX, wall.HeightAt(worldX, worldZ)));
                    holes[row, col] = !inside;
                    if (inside)
                    {
                        cut++;
                    }
                }
            }

            data.SetHoles(firstX, firstZ, holes);
            return cut;
        }
    }
}
