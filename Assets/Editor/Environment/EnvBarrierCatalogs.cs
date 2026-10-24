using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Large reusable pieces that close off paths: cliffs, boulders, pits and blockers.</summary>
    public sealed class BarrierCatalog : EnvCatalogBase
    {
        private const string Area = "Barriers";

        /// <inheritdoc />
        protected override void Register()
        {
            RegisterCliffs();
            RegisterBoulders();
            RegisterPits();
            RegisterBlockers();
        }

        private void RegisterCliffs()
        {
            string[] styles = { "Grey", "Red", "Dark" };
            string[] materials = { "rock_cliff_grey", "rock_cliff_red", "rock_cliff_dark" };
            for (int k = 0; k < styles.Length; k++)
            {
                int i = k;
                string mat = materials[k];
                string name = "Cliff" + styles[k];
                int seed = 1000 + i * 100;
                Add(Area, "Cliffs", name + "_Straight", new[] { mat }, m => EnvBarriers.CliffPath(m, 0, -1, seed, EnvBarriers.Line(12f), 8f, 4f), true, true);
                Add(Area, "Cliffs", name + "_Tall", new[] { mat }, m => EnvBarriers.CliffPath(m, 0, -1, seed + 1, EnvBarriers.Line(12f), 14f, 5f), true, true);
                Add(Area, "Cliffs", name + "_Low", new[] { mat }, m => EnvBarriers.CliffPath(m, 0, -1, seed + 2, EnvBarriers.Line(10f), 4.5f, 3f), true, true);
                Add(Area, "Cliffs", name + "_Corner", new[] { mat }, m => EnvBarriers.CliffPath(m, 0, -1, seed + 3, EnvBarriers.Corner(9f), 9f, 4f), true, true);
                Add(Area, "Cliffs", name + "_Curve", new[] { mat }, m => EnvBarriers.CliffPath(m, 0, -1, seed + 4, EnvBarriers.Arc(11f, 80f, 6), 9f, 4f), true, true);
            }

            Add(Area, "Cliffs", "CliffSnow_Straight", new[] { "rock_cliff_grey", "snow" }, m => EnvBarriers.CliffPath(m, 0, 1, 1500, EnvBarriers.Line(12f), 9f, 4f), true, true);
            Add(Area, "Cliffs", "CliffSnow_Tall", new[] { "rock_cliff_dark", "snow" }, m => EnvBarriers.CliffPath(m, 0, 1, 1501, EnvBarriers.Line(12f), 15f, 5f), true, true);
            Add(Area, "Cliffs", "CliffSnow_Corner", new[] { "rock_cliff_grey", "snow" }, m => EnvBarriers.CliffPath(m, 0, 1, 1502, EnvBarriers.Corner(9f), 10f, 4f), true, true);
            Add(Area, "Cliffs", "CliffSand_Straight", new[] { "rock_sandstone" }, m => EnvBarriers.CliffPath(m, 0, -1, 1600, EnvBarriers.Line(12f), 8f, 4f), true, true);
            Add(Area, "Cliffs", "CliffSand_Tall", new[] { "rock_sandstone" }, m => EnvBarriers.CliffPath(m, 0, -1, 1601, EnvBarriers.Line(12f), 13f, 5f), true, true);
            Add(Area, "Cliffs", "CliffMossy_Straight", new[] { "rock_mossy" }, m => EnvBarriers.CliffPath(m, 0, -1, 1700, EnvBarriers.Line(12f), 7f, 4f), true, true);
            Add(Area, "Cliffs", "CliffTower_Grey", new[] { "rock_cliff_grey" }, m => EnvRocks.Spires(m, 0, 1800, 12f, 4), true, true);
            Add(Area, "Cliffs", "CliffTower_Red", new[] { "rock_cliff_red" }, m => EnvRocks.Spires(m, 0, 1801, 14f, 5), true, true);
        }

        private void RegisterBoulders()
        {
            float[] sizes = { 3f, 4.5f, 6f };
            string[] mats = { "rock_smooth_grey", "rock_smooth_warm", "rock_cliff_grey" };
            string[] names = { "GiantSmooth", "GiantWarm", "GiantCliff" };
            for (int a = 0; a < mats.Length; a++)
            {
                for (int b = 0; b < sizes.Length; b++)
                {
                    string mat = mats[a];
                    float size = sizes[b];
                    int seed = 2000 + a * 40 + b * 7;
                    Add(Area, "Boulders", names[a] + "_0" + (b + 1), new[] { mat }, m => EnvBarriers.GiantBoulder(m, 0, seed, size), true, true);
                }
            }

            Add(Area, "Boulders", "BoulderLine_Smooth", new[] { "rock_smooth_grey" }, m => EnvBarriers.BoulderLine(m, 0, 2200, 5, 2.2f), true, true);
            Add(Area, "Boulders", "BoulderLine_Granite", new[] { "rock_granite" }, m => EnvBarriers.BoulderLine(m, 0, 2201, 5, 2.4f), true, true);
            Add(Area, "Boulders", "BoulderLine_Sand", new[] { "rock_sandstone" }, m => EnvBarriers.BoulderLine(m, 0, 2202, 6, 2.0f), true, true);
            Add(Area, "Boulders", "BoulderLine_Mossy", new[] { "rock_mossy" }, m => EnvBarriers.BoulderLine(m, 0, 2203, 4, 2.8f), true, true);
            Add(Area, "Boulders", "RockSlide_Grey", new[] { "rock_granite" }, m => EnvBarriers.RockSlide(m, 0, 2300, 10f, 4f), true, true);
            Add(Area, "Boulders", "RockSlide_Dark", new[] { "rock_slate" }, m => EnvBarriers.RockSlide(m, 0, 2301, 12f, 5f), true, true);
            Add(Area, "Boulders", "RockSlide_Sand", new[] { "rock_sandstone" }, m => EnvBarriers.RockSlide(m, 0, 2302, 10f, 4f), true, true);
            Add(Area, "Boulders", "Megalith_01", new[] { "rock_cliff_grey" }, m => EnvRocks.Standing(m, 0, 2400, 8f, 2f), true, true);
            Add(Area, "Boulders", "Megalith_02", new[] { "rock_cliff_red" }, m => EnvRocks.Standing(m, 0, 2401, 10f, 2.4f), true, true);
            Add(Area, "Boulders", "RockArch_Grey", new[] { "rock_cliff_grey" }, m => EnvRocks.Arch(m, 0, 2500, 6f, 5.5f), true, true);
        }

        private void RegisterPits()
        {
            string[] mats = { "rock_granite", "rock_mossy", "rock_sandstone", "rock_slate" };
            string[] names = { "Granite", "Mossy", "Sand", "Slate" };
            float[] radii = { 2.2f, 3.5f, 5f };
            string[] sizes = { "Small", "Medium", "Large" };
            for (int a = 0; a < mats.Length; a++)
            {
                for (int b = 0; b < radii.Length; b++)
                {
                    string mat = mats[a];
                    float r = radii[b];
                    int seed = 3000 + a * 30 + b * 5;
                    Add(Area, "Pits", "Pit" + names[a] + "_" + sizes[b], new[] { mat, "dark" }, m => EnvEntrances.Sinkhole(m, 0, 1, seed, r), true, false, 6f);
                }
            }

            Add(Area, "Pits", "Chasm_Short", new[] { "rock_slate", "dark" }, m => EnvBarriers.Chasm(m, 0, 1, 3100, new[] { new Vector3(-4f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(4f, 0f, 0f) }, 3f), true, false, 6f);
            Add(Area, "Pits", "Chasm_Long", new[] { "rock_granite", "dark" }, m => EnvBarriers.Chasm(m, 0, 1, 3101, new[] { new Vector3(-8f, 0f, 0f), new Vector3(-4f, 0f, 0.3f), new Vector3(0f, 0f, 0f), new Vector3(4f, 0f, -0.3f), new Vector3(8f, 0f, 0f) }, 3.6f), true, false, 6f);
            Add(Area, "Pits", "Chasm_Wide", new[] { "rock_sandstone", "dark" }, m => EnvBarriers.Chasm(m, 0, 1, 3102, new[] { new Vector3(-7f, 0f, 0f), new Vector3(-3.5f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(3.5f, 0f, 0f), new Vector3(7f, 0f, 0f) }, 6f), true, false, 6f);
            Add(Area, "Pits", "Chasm_Curved", new[] { "rock_slate", "dark" }, m => EnvBarriers.Chasm(m, 0, 1, 3103, EnvBarriers.Arc(9f, 90f, 6), 3.2f), true, false, 6f);
            Add(Area, "Pits", "Chasm_Mossy", new[] { "rock_mossy", "dark" }, m => EnvBarriers.Chasm(m, 0, 1, 3104, new[] { new Vector3(-6f, 0f, 0f), new Vector3(-2f, 0f, 0.5f), new Vector3(2f, 0f, -0.5f), new Vector3(6f, 0f, 0f) }, 3f), true, false, 6f);
        }

        private void RegisterBlockers()
        {
            Add(Area, "Blockers", "Palisade_Wood", new[] { "wood_planks" }, m => EnvBarriers.Palisade(m, 0, 4000, 8f, 3.2f), true, true);
            Add(Area, "Blockers", "Palisade_Dark", new[] { "wood_dark" }, m => EnvBarriers.Palisade(m, 0, 4001, 8f, 3.8f), true, true);
            Add(Area, "Blockers", "Palisade_Short", new[] { "wood_planks" }, m => EnvBarriers.Palisade(m, 0, 4002, 4f, 2.4f), true, true);
            Add(Area, "Blockers", "Thicket_Green", new[] { "leaf_bush" }, m => EnvBarriers.Thicket(m, 0, 4100, 8f, 3f), true, false, 3f);
            Add(Area, "Blockers", "Thicket_Tall", new[] { "leaf_oak_dark" }, m => EnvBarriers.Thicket(m, 0, 4101, 10f, 4.5f), true, false, 4.5f);
            Add(Area, "Blockers", "Thicket_Dry", new[] { "leaf_dry_bush" }, m => EnvBarriers.Thicket(m, 0, 4102, 8f, 3f), true, false, 3f);
            Add(Area, "Blockers", "Thicket_Olive", new[] { "leaf_olive" }, m => EnvBarriers.Thicket(m, 0, 4103, 8f, 3f), true, false, 3f);
            Add(Area, "Blockers", "RuinWall_Grey", new[] { "ruin_blocks" }, m => EnvBarriers.RuinWall(m, 0, 4200, 8f, 3.6f), true, true);
            Add(Area, "Blockers", "RuinWall_Sand", new[] { "sandstone_blocks" }, m => EnvBarriers.RuinWall(m, 0, 4201, 8f, 3.6f), true, true);
            Add(Area, "Blockers", "RuinWall_Slate", new[] { "slate_blocks" }, m => EnvBarriers.RuinWall(m, 0, 4202, 8f, 3.6f), true, true);
            Add(Area, "Blockers", "RuinWall_Long", new[] { "ruin_blocks" }, m => EnvBarriers.RuinWall(m, 0, 4203, 14f, 4.2f), true, true);
            Add(Area, "Blockers", "LogPile_01", new[] { "bark_oak" }, m => EnvBarriers.LogPile(m, 0, 4300, 6f), true, true);
            Add(Area, "Blockers", "LogPile_02", new[] { "bark_dead" }, m => EnvBarriers.LogPile(m, 0, 4301, 8f), true, true);
            Add(Area, "Blockers", "Hedgehogs_01", new[] { "wood_dark" }, m => EnvBarriers.Hedgehogs(m, 0, 3), true, true);
            Add(Area, "Blockers", "Hedgehogs_02", new[] { "wood_planks" }, m => EnvBarriers.Hedgehogs(m, 0, 5), true, true);
            Add(Area, "Blockers", "IceWall_01", new[] { "ice", "snow" }, m => EnvBarriers.IceWall(m, 0, 1, 4400, 8f, 5f), true, true);
            Add(Area, "Blockers", "IceWall_02", new[] { "ice", "snow" }, m => EnvBarriers.IceWall(m, 0, 1, 4401, 12f, 8f), true, true);
        }
    }

    /// <summary>Smooth, rounded stones and pebbles for every biome.</summary>
    public sealed class SmoothStoneCatalog : EnvCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            RegisterFor("Field", "rock_smooth_grey");
            RegisterFor("Coast", "rock_smooth_river");
            RegisterFor("Desert", "rock_smooth_warm");
            RegisterFor("Mountain", "rock_smooth_grey");
        }

        private void RegisterFor(string area, string rock)
        {
            Vector3[] shapes =
            {
                new Vector3(0.5f, 0.4f, 0.45f), new Vector3(0.9f, 0.7f, 0.8f), new Vector3(1.4f, 0.5f, 1.1f), new Vector3(0.7f, 1.1f, 0.65f),
                new Vector3(1.8f, 1.3f, 1.5f), new Vector3(2.6f, 0.8f, 1.8f)
            };
            string[] labels = { "Small", "Round", "Slab", "Egg", "Large", "Flat" };
            for (int k = 0; k < shapes.Length; k++)
            {
                Vector3 radii = shapes[k];
                int seed = 5000 + k * 11 + area.Length;
                Add(area, "Stones", "Smooth" + labels[k] + "_01", new[] { rock }, m => EnvBarriers.SmoothStone(m, 0, seed, radii), true, true);
            }

            Add(area, "Stones", "SmoothPebbles_01", new[] { rock }, m => EnvBarriers.Pebbles(m, 0, 5100 + area.Length, 9, 0.9f), true, false);
            Add(area, "Stones", "SmoothPebbles_02", new[] { rock }, m => EnvBarriers.Pebbles(m, 0, 5200 + area.Length, 16, 1.6f), true, false);
        }
    }
}
