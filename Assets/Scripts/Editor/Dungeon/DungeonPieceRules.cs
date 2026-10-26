using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>How the physics of a kit piece is set up.</summary>
    public enum ColliderMode
    {
        /// <summary>The piece has no collider.</summary>
        None,

        /// <summary>A box fitted to the mesh bounds.</summary>
        Box,

        /// <summary>A non-convex mesh collider.</summary>
        Mesh,
    }

    /// <summary>A point light attached to a kit piece.</summary>
    public readonly struct PieceLight
    {
        /// <summary>Initializes a new instance of the <see cref="PieceLight"/> struct.</summary>
        /// <param name="position">Position in model space: x to the right, y toward the back, z up.</param>
        /// <param name="color">Light color.</param>
        /// <param name="intensity">Light intensity.</param>
        /// <param name="range">Light range in meters.</param>
        public PieceLight(Vector3 position, Color color, float intensity, float range)
        {
            Position = position;
            Color = color;
            Intensity = intensity;
            Range = range;
        }

        /// <summary>Gets the position of the light in model space: x to the right, y toward the back, z up.</summary>
        public Vector3 Position { get; }

        /// <summary>Gets the light color.</summary>
        public Color Color { get; }

        /// <summary>Gets the light intensity.</summary>
        public float Intensity { get; }

        /// <summary>Gets the light range in meters.</summary>
        public float Range { get; }
    }

    /// <summary>Per-piece rules for colliders, navigation layer and lights of the dungeon kit.</summary>
    public static class DungeonPieceRules
    {
        private const float SmallPieceLimit = 0.9f;
        private static readonly Color Warm = new Color(1f, 0.62f, 0.3f);
        private static readonly Color Blue = new Color(0.35f, 0.65f, 1f);
        private static readonly Color Purple = new Color(0.65f, 0.4f, 0.95f);
        private static readonly Color Green = new Color(0.35f, 1f, 0.6f);
        private static readonly Color Red = new Color(1f, 0.25f, 0.2f);

        private static readonly HashSet<string> Decoration = new HashSet<string>
        {
            "bones_scatter_a", "bones_scatter_b", "skull_pile", "ribcage", "skeleton_lying", "skeleton_chained", "candles_cluster",
            "chains_hanging", "chandelier_iron", "shackles_wall", "lever_wall", "debris_planks", "pressure_plate", "tomb_slab",
            "summoning_circle", "spike_trap", "stalactite_a", "stalactite_b", "torch_ground", "mining_pickaxe_prop", "banner_pole_tattered",
            "hanging_cage", "gravestone_broken", "urn_small", "bucket_wood", "mushroom_glow", "crystal_small",
        };

        private static readonly Dictionary<string, PieceLight[]> Lights = new Dictionary<string, PieceLight[]>
        {
            { "wall_torch", new[] { Torch(0f, -0.33f, 2.25f) } },
            { "wall_banner", new[] { Torch(-1.45f, -0.33f, 2.2f), Torch(1.45f, -0.33f, 2.2f) } },
            { "wall_post_torch", new[] { Torch(0f, -0.83f, 2.3f) } },
            { "torch_standing", new[] { Torch(0f, 0f, 2.4f) } },
            { "brazier_bowl", new[] { new PieceLight(new Vector3(0f, 0f, 2f), Warm, 12f, 11f) } },
            { "candelabra_floor", new[] { new PieceLight(new Vector3(0f, 0f, 1.7f), Warm, 4f, 6f) } },
            { "candles_cluster", new[] { new PieceLight(new Vector3(0f, 0f, 0.6f), Warm, 2f, 4f) } },
            { "chandelier_iron", new[] { new PieceLight(new Vector3(0f, 0f, 0.4f), Warm, 6f, 8f) } },
            { "torch_ground", new[] { new PieceLight(new Vector3(0.4f, 0.1f, 0.6f), Warm, 4f, 6f) } },
            { "cauldron_green", new[] { new PieceLight(new Vector3(0f, 0f, 1.4f), Green, 5f, 6f) } },
            { "crystal_cluster_blue", new[] { new PieceLight(new Vector3(0f, 0f, 1.4f), Blue, 5f, 7f) } },
            { "crystal_cluster_purple", new[] { new PieceLight(new Vector3(0f, 0f, 1.4f), Purple, 5f, 7f) } },
            { "crystal_wall_blue", new[] { new PieceLight(new Vector3(0f, -1.2f, 1.6f), Blue, 4f, 6f) } },
            { "cave_wall_crystals", new[] { new PieceLight(new Vector3(0f, -1.4f, 1.6f), Blue, 4f, 6f) } },
            { "mushroom_glow", new[] { new PieceLight(new Vector3(0f, 0f, 0.8f), Green, 2f, 4f) } },
            { "summoning_circle", new[] { new PieceLight(new Vector3(0f, 0f, 0.8f), Red, 4f, 7f) } },
            { "altar_ritual", new[] { new PieceLight(new Vector3(0f, 0f, 2f), Red, 3f, 6f) } },
            { "floor_rune", new[] { new PieceLight(new Vector3(0f, 0f, 0.8f), Green, 3f, 6f) } },
            { "throne_dark", new[] { new PieceLight(new Vector3(0f, -0.5f, 2.6f), Red, 4f, 7f) } },
            { "wall_arch_boss", new[] { new PieceLight(new Vector3(0f, -0.9f, 2.7f), Red, 3f, 6f) } },
        };

        /// <summary>Gets the collider mode for a piece.</summary>
        /// <param name="category">Kit category folder.</param>
        /// <param name="name">Piece name.</param>
        /// <param name="bounds">Mesh bounds of the piece.</param>
        /// <returns>The collider mode.</returns>
        public static ColliderMode ColliderFor(string category, string name, Bounds bounds)
        {
            if (Decoration.Contains(name))
            {
                return ColliderMode.None;
            }

            switch (category)
            {
                case "floors":
                    return ColliderMode.Box;
                case "walls":
                case "structure":
                case "stairs":
                    return ColliderMode.Mesh;
                case "cave":
                    return name.StartsWith("cave_") || name == "rock_ledge" ? ColliderMode.Mesh : ColliderMode.Box;
                default:
                    return bounds.size.magnitude >= SmallPieceLimit ? ColliderMode.Box : ColliderMode.None;
            }
        }

        /// <summary>Gets whether the piece is decoration that must not affect the navigation mesh.</summary>
        /// <param name="name">Piece name.</param>
        /// <returns>True for decoration.</returns>
        public static bool IsDecoration(string name) => Decoration.Contains(name);

        /// <summary>Gets the lights of a piece.</summary>
        /// <param name="name">Piece name.</param>
        /// <returns>The lights, or an empty array.</returns>
        public static IReadOnlyList<PieceLight> LightsFor(string name) => Lights.TryGetValue(name, out PieceLight[] lights) ? lights : System.Array.Empty<PieceLight>();

        private static PieceLight Torch(float x, float y, float z) => new PieceLight(new Vector3(x, y, z), Warm, 9f, 9f);
    }
}
