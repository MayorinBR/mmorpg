namespace Project.EditorTools.Dungeon
{
    /// <summary>Asset locations shared by the dungeon kit importer and the dungeon scene builders.</summary>
    public static class DungeonKitPaths
    {
        /// <summary>Root folder of the dungeon kit models.</summary>
        public const string Root = "Assets/Models/Dungeon";

        /// <summary>Folder that holds the palette, category map, pattern array and material.</summary>
        public const string Painted = Root + "/Painted";

        /// <summary>Folder that holds the generated prefabs, one subfolder per category.</summary>
        public const string Prefabs = Root + "/Prefabs";

        /// <summary>Path of the palette texture.</summary>
        public const string PalettePng = Painted + "/DungeonPalette.png";

        /// <summary>Path of the category map texture that selects the surface pattern and emission per palette cell.</summary>
        public const string CategoryPng = Painted + "/DungeonCategory.png";

        /// <summary>Path of the pattern texture array.</summary>
        public const string PatternArray = Painted + "/DungeonPatterns.asset";

        /// <summary>Path of the shared material.</summary>
        public const string Material = Painted + "/DungeonPainted.mat";

        /// <summary>Name of the shader used by the shared material.</summary>
        public const string ShaderName = "Project/Environment/DungeonPainted";

        /// <summary>Name of the material that the exported models reference.</summary>
        public const string SourceMaterialName = "DungeonPalette";

        /// <summary>Categories of the kit; each one is a subfolder of <see cref="Root"/> that contains FBX files.</summary>
        public static readonly string[] Categories = { "walls", "floors", "structure", "stairs", "cave", "decor", "props" };

        /// <summary>Gets the prefab path for a model.</summary>
        /// <param name="category">The category folder name.</param>
        /// <param name="name">The model name without extension.</param>
        /// <returns>The prefab asset path.</returns>
        public static string PrefabPath(string category, string name) => $"{Prefabs}/{category}/{name}.prefab";
    }
}
