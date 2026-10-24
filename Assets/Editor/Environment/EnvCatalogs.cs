using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Shared helpers for catalogs that register tree families.</summary>
    public abstract class EnvTreeCatalogBase : EnvCatalogBase
    {
        /// <summary>Registers conifer variants of increasing height.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="bark">Bark material key.</param>
        /// <param name="leaf">Leaf material key.</param>
        /// <param name="heights">Heights of the variants.</param>
        /// <param name="lean">Optional lean applied to every variant.</param>
        protected void Conifers(string area, string prefix, string bark, string leaf, float[] heights, Vector3 lean)
        {
            for (int k = 0; k < heights.Length; k++)
            {
                int i = k;
                float h = heights[k];
                Add(area, "Trees", prefix + "_0" + (i + 1), new[] { bark, leaf }, m => EnvTrees.Conifer(m, new ConiferSpec { Seed = 11 + i * 7, Height = h, Lean = lean * h }));
            }
        }

        /// <summary>Registers broadleaf variants.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="bark">Bark material key.</param>
        /// <param name="leaf">Leaf material key.</param>
        /// <param name="heights">Heights of the variants.</param>
        /// <param name="crown">Crown radius factor.</param>
        /// <param name="stretch">Crown vertical stretch.</param>
        /// <param name="trunk">Trunk fraction of the height.</param>
        /// <param name="trunkRadius">Trunk radius as a fraction of the height.</param>
        protected void Broadleafs(string area, string prefix, string bark, string leaf, float[] heights, float crown, float stretch, float trunk, float trunkRadius = 0.05f)
        {
            for (int k = 0; k < heights.Length; k++)
            {
                int i = k;
                float h = heights[k];
                Add(area, "Trees", prefix + "_0" + (i + 1), new[] { bark, leaf }, m => EnvTrees.Broadleaf(m, new BroadleafSpec
                {
                    Seed = 31 + i * 13,
                    Height = h,
                    CrownRadiusFactor = crown,
                    CrownStretch = stretch,
                    TrunkFraction = trunk,
                    TrunkRadiusFactor = trunkRadius,
                    Clusters = 5 + i % 2,
                    Lean = new Vector3(0.25f * (i - 1), 0f, 0.2f * (1 - i))
                }));
            }
        }

        /// <summary>Registers dead tree variants.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="count">Variant count.</param>
        /// <param name="height">Base height.</param>
        /// <param name="withTwigs">Add twig cards to branch tips.</param>
        protected void DeadTrees(string area, int count, float height, bool withTwigs)
        {
            for (int k = 0; k < count; k++)
            {
                int i = k;
                string[] mats = withTwigs ? new[] { "bark_dead", "leaf_dead" } : new[] { "bark_dead" };
                Add(area, "Trees", "DeadTree_0" + (i + 1), mats, m => EnvTrees.DeadTree(m, 5 + i * 9, height * (0.85f + 0.15f * i), 4, 0, withTwigs ? 1 : -1));
            }
        }

        /// <summary>Registers grass patches.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="grass">Grass material key.</param>
        /// <param name="count">Variant count.</param>
        /// <param name="height">Blade height.</param>
        protected void Grasses(string area, string prefix, string grass, int count, float height)
        {
            for (int k = 0; k < count; k++)
            {
                int i = k;
                Add(area, "Plants", prefix + "_0" + (i + 1), new[] { grass }, m => EnvPlants.Grass(m, 0, 70 + i * 5, 0.5f + 0.25f * i, 7 + i * 4, height * (0.85f + 0.2f * i)), false);
            }
        }

        /// <summary>Registers flower patches.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="grass">Grass material key.</param>
        /// <param name="flowers">Flower material keys.</param>
        protected void FlowerPatches(string area, string grass, params string[] flowers)
        {
            foreach (string flower in flowers)
            {
                string key = flower;
                string label = char.ToUpper(key.Substring(7, 1)[0]) + key.Substring(8);
                for (int k = 0; k < 2; k++)
                {
                    int i = k;
                    Add(area, "Flowers", label + "_0" + (i + 1), new[] { grass, key }, m => EnvPlants.Flowers(m, 0, 1, 90 + i * 11, 0.55f + 0.3f * i, 6 + i * 4), false);
                }
            }
        }

        /// <summary>Registers rocks of one material in several sizes.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="rock">Rock material key.</param>
        /// <param name="sizes">Radii of the boulders.</param>
        /// <param name="flatten">Vertical scale.</param>
        protected void Boulders(string area, string prefix, string rock, float[] sizes, float flatten)
        {
            for (int k = 0; k < sizes.Length; k++)
            {
                int i = k;
                float s = sizes[k];
                Add(area, "Stones", prefix + "_0" + (i + 1), new[] { rock }, m => EnvRocks.Boulder(m, 0, 101 + i * 19, s, flatten, 0.26f), true, true);
            }
        }

        /// <summary>Registers rock clusters.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="rock">Rock material key.</param>
        protected void Clusters(string area, string prefix, string rock)
        {
            for (int k = 0; k < 3; k++)
            {
                int i = k;
                Add(area, "Stones", prefix + "Cluster_0" + (i + 1), new[] { rock }, m => EnvRocks.Cluster(m, 0, 201 + i * 23, 4 + i * 2, 1.2f + i * 0.5f, 0.9f + i * 0.3f), true, true);
            }
        }

        /// <summary>Registers bushes.</summary>
        /// <param name="area">Area folder.</param>
        /// <param name="prefix">Name prefix.</param>
        /// <param name="leaf">Leaf material key.</param>
        /// <param name="count">Variant count.</param>
        /// <param name="radius">Base radius.</param>
        protected void Bushes(string area, string prefix, string leaf, int count, float radius)
        {
            for (int k = 0; k < count; k++)
            {
                int i = k;
                Add(area, "Plants", prefix + "_0" + (i + 1), new[] { leaf }, m => EnvPlants.Bush(m, 0, 301 + i * 9, radius * (0.8f + 0.3f * i), 3 + i), false);
            }
        }
    }

    /// <summary>Temperate meadow and forest assets.</summary>
    public sealed class FieldCatalog : EnvTreeCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            Broadleafs("Field", "Oak", "bark_oak", "leaf_round", new[] { 6f, 7.5f, 9.5f }, 0.3f, 0.9f, 0.45f);
            Broadleafs("Field", "OakDark", "bark_oak", "leaf_oak_dark", new[] { 6.5f, 8.5f }, 0.32f, 0.85f, 0.42f);
            Broadleafs("Field", "Birch", "bark_birch", "leaf_birch", new[] { 7f, 8.5f, 10f }, 0.27f, 1.2f, 0.5f, 0.028f);
            Conifers("Field", "Pine", "bark_oak", "leaf_pine", new[] { 7f, 9f, 11f }, Vector3.zero);
            DeadTrees("Field", 2, 6f, true);

            Boulders("Field", "Boulder", "rock_granite", new[] { 0.6f, 0.9f, 1.3f, 1.8f }, 0.72f);
            Boulders("Field", "MossyBoulder", "rock_mossy", new[] { 0.7f, 1.1f, 1.6f }, 0.7f);
            Clusters("Field", "Mossy", "rock_mossy");
            for (int k = 0; k < 2; k++)
            {
                int i = k;
                Add("Field", "Stones", "StandingStone_0" + (i + 1), new[] { "rock_mossy" }, m => EnvRocks.Standing(m, 0, 401 + i * 5, 2.6f + i * 1.2f, 0.7f), true, true);
            }

            FlowerPatches("Field", "grass_green", "flower_yellow", "flower_white", "flower_pink", "flower_purple", "flower_red", "flower_blue");
            Add("Field", "Flowers", "Mixed_01", new[] { "grass_green", "flower_yellow", "flower_pink", "flower_white" }, m =>
            {
                var rng = new Rng(77);
                EnvShapes.Patch(m, 0, Vector3.zero, 0.9f, 12, 0.5f, 0.45f, 2, rng);
                EnvShapes.Patch(m, 1, new Vector3(-0.3f, 0f, 0f), 0.5f, 5, 0.42f, 0.5f, 2, rng);
                EnvShapes.Patch(m, 2, new Vector3(0.35f, 0f, 0.2f), 0.5f, 5, 0.42f, 0.5f, 2, rng);
                EnvShapes.Patch(m, 3, new Vector3(0f, 0f, -0.4f), 0.5f, 5, 0.42f, 0.5f, 2, rng);
            }, false);

            Grasses("Field", "Grass", "grass_green", 3, 0.55f);
            Grasses("Field", "TallGrass", "grass_green", 2, 1.1f);
            Bushes("Field", "Bush", "leaf_bush", 3, 0.9f);
            for (int k = 0; k < 2; k++)
            {
                int i = k;
                Add("Field", "Plants", "Fern_0" + (i + 1), new[] { "leaf_fern" }, m => EnvPlants.Fern(m, 0, 501 + i * 3, 8 + i * 3, 0.9f + i * 0.3f, 15f, 55f), false);
                Add("Field", "Plants", "Stump_0" + (i + 1), new[] { "bark_oak" }, m => EnvDecor.Stump(m, 0, 0.45f + i * 0.2f, 0.5f + i * 0.2f), true, true);
                Add("Field", "Plants", "Log_0" + (i + 1), new[] { "bark_oak" }, m => EnvDecor.Log(m, 0, 2.4f + i * 1.2f, 0.28f + i * 0.08f), true, true);
            }

            Add("Field", "Decor", "Fence_01", new[] { "wood_planks" }, m => EnvDecor.Fence(m, 0, 2.4f, 1.1f), true, true);
            Add("Field", "Decor", "Signpost_01", new[] { "wood_planks" }, m => EnvDecor.Signpost(m, 0), true, true);
            Add("Field", "Decor", "Haystack_01", new[] { "straw" }, m => EnvDecor.Haystack(m, 0, 1.2f), true, true);
            Add("Field", "Decor", "Haystack_02", new[] { "straw" }, m => EnvDecor.Haystack(m, 0, 1.8f), true, true);
            Add("Field", "Decor", "Well_01", new[] { "rock_mossy", "wood_planks", "dark" }, m => EnvDecor.Well(m, 0, 1, 2), true, true);
            Add("Field", "Decor", "Barrel_01", new[] { "wood_planks", "metal" }, m => EnvDecor.Barrel(m, 0, 1, 0.5f, 1.0f), true, true);
            Add("Field", "Decor", "Crate_01", new[] { "wood_planks" }, m => EnvDecor.Crate(m, 0, 0.9f, 8f, Vector3.zero), true, true);
            Add("Field", "Decor", "LampPost_01", new[] { "metal", "wood_dark" }, m => EnvDecor.LampPost(m, 0, 1), true, true);
        }
    }

    /// <summary>Beach and harbor assets.</summary>
    public sealed class CoastCatalog : EnvTreeCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            for (int k = 0; k < 4; k++)
            {
                int i = k;
                Add("Coast", "Trees", "Palm_0" + (i + 1), new[] { "bark_palm", "leaf_palm", "wood_dark" }, m => EnvTrees.Palm(m, new PalmSpec
                {
                    Seed = 3 + i * 5,
                    Height = 6f + i * 1.3f,
                    Lean = new Vector3(1.2f + i * 0.4f, 0f, 0.3f * (i - 1.5f)),
                    Fronds = 9 + i % 2 * 2,
                    Nuts = i % 2 == 0 ? 4 : 0
                }));
            }

            Add("Coast", "Trees", "PalmDry_01", new[] { "bark_palm", "leaf_palm_dry", "wood_dark" }, m => EnvTrees.Palm(m, new PalmSpec { Seed = 21, Height = 7f, Fronds = 8, PitchMax = 35f, Droop = 0.9f }));
            Add("Coast", "Trees", "PalmTall_01", new[] { "bark_palm", "leaf_palm", "wood_dark" }, m => EnvTrees.Palm(m, new PalmSpec { Seed = 29, Height = 11f, TrunkRadius = 0.22f, Lean = new Vector3(0.8f, 0f, 0.5f), Fronds = 12, FrondLength = 4f, FrondWidth = 1.9f, Nuts = 5 }));
            Conifers("Coast", "WindPine", "bark_oak", "leaf_pine_cool", new[] { 6f, 8f }, new Vector3(0.12f, 0f, 0.05f));

            Boulders("Coast", "SeaRock", "rock_sea", new[] { 0.6f, 1.0f, 1.5f, 2.1f }, 0.65f);
            Clusters("Coast", "Sea", "rock_sea");
            Add("Coast", "Stones", "SeaArch_01", new[] { "rock_sea" }, m => EnvRocks.Arch(m, 0, 61, 4.5f, 3.6f), true, true);
            Add("Coast", "Stones", "SeaSpires_01", new[] { "rock_sea" }, m => EnvRocks.Spires(m, 0, 63, 5.5f, 4), true, true);
            Add("Coast", "Stones", "SeaSpires_02", new[] { "rock_sea" }, m => EnvRocks.Spires(m, 0, 67, 3.5f, 3), true, true);

            Grasses("Coast", "BeachGrass", "grass_beach", 3, 0.7f);
            Add("Coast", "Plants", "Reeds_01", new[] { "grass_beach" }, m => EnvPlants.Reeds(m, 0, 81, 0.6f, 1.8f), false);
            Add("Coast", "Plants", "Reeds_02", new[] { "grass_beach" }, m => EnvPlants.Reeds(m, 0, 83, 0.9f, 2.4f), false);
            Bushes("Coast", "Shrub", "leaf_olive", 2, 0.9f);
            for (int k = 0; k < 2; k++)
            {
                int i = k;
                Add("Coast", "Plants", "Driftwood_0" + (i + 1), new[] { "bark_dead" }, m => EnvDecor.Log(m, 0, 2.2f + i * 1.4f, 0.16f + i * 0.05f), true, true);
            }

            FlowerPatches("Coast", "grass_beach", "flower_pink", "flower_white");

            Add("Coast", "Decor", "Pier_01", new[] { "wood_planks", "wood_dark" }, m => EnvDecor.Pier(m, 0, 1, 4f), true, true);
            Add("Coast", "Decor", "Pier_02", new[] { "wood_planks", "wood_dark" }, m => EnvDecor.Pier(m, 0, 1, 8f), true, true);
            Add("Coast", "Decor", "Rowboat_01", new[] { "wood_planks", "wood_dark" }, m => EnvDecor.Rowboat(m, 0, 1), true, true);
            Add("Coast", "Decor", "Barrel_01", new[] { "wood_dark", "metal" }, m => EnvDecor.Barrel(m, 0, 1, 0.5f, 1.0f), true, true);
            Add("Coast", "Decor", "CrateStack_01", new[] { "wood_planks" }, m =>
            {
                EnvDecor.Crate(m, 0, 0.95f, 5f, Vector3.zero);
                EnvDecor.Crate(m, 0, 0.8f, -12f, new Vector3(1.05f, 0f, 0.1f));
                EnvDecor.Crate(m, 0, 0.8f, 20f, new Vector3(0.4f, 0.95f, 0f));
            }, true, true);
            Add("Coast", "Decor", "Buoy_01", new[] { "cloth_stripe", "metal" }, m => EnvDecor.Buoy(m, 0, 1), true, true);
            Add("Coast", "Decor", "NetRack_01", new[] { "wood_dark", "cloth_cream" }, m => EnvDecor.NetRack(m, 0, 1), true, true);
            Add("Coast", "Decor", "LampPost_01", new[] { "metal", "wood_dark" }, m => EnvDecor.LampPost(m, 0, 1), true, true);
        }
    }

    /// <summary>Arid land assets.</summary>
    public sealed class DesertCatalog : EnvTreeCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            for (int k = 0; k < 3; k++)
            {
                int i = k;
                Add("Desert", "Trees", "Acacia_0" + (i + 1), new[] { "bark_dead", "leaf_acacia" }, m => EnvTrees.Acacia(m, 7 + i * 11, 5.5f + i * 1.2f, 3.2f + i * 0.5f, 0, 1));
            }

            for (int k = 0; k < 2; k++)
            {
                int i = k;
                Add("Desert", "Trees", "DatePalm_0" + (i + 1), new[] { "bark_palm", "leaf_palm_dry", "wood_dark" }, m => EnvTrees.Palm(m, new PalmSpec
                {
                    Seed = 9 + i * 4,
                    Height = 6.5f + i * 1.5f,
                    Lean = new Vector3(0.6f * (i + 1), 0f, 0.3f),
                    Fronds = 11,
                    PitchMax = 65f,
                    Droop = 0.55f,
                    Nuts = 6
                }));
            }

            Broadleafs("Desert", "Olive", "bark_dead", "leaf_olive", new[] { 4f, 5f }, 0.34f, 0.8f, 0.4f);
            DeadTrees("Desert", 3, 5.5f, false);

            Boulders("Desert", "Sandstone", "rock_sandstone", new[] { 0.7f, 1.1f, 1.7f, 2.4f }, 0.7f);
            Clusters("Desert", "Sand", "rock_sandstone");
            Add("Desert", "Stones", "Spires_01", new[] { "rock_sandstone" }, m => EnvRocks.Spires(m, 0, 91, 7f, 4), true, true);
            Add("Desert", "Stones", "Spires_02", new[] { "rock_sandstone" }, m => EnvRocks.Spires(m, 0, 93, 4.5f, 3), true, true);
            Add("Desert", "Stones", "Arch_01", new[] { "rock_sandstone" }, m => EnvRocks.Arch(m, 0, 95, 5f, 4.2f), true, true);

            for (int k = 0; k < 3; k++)
            {
                int i = k;
                Add("Desert", "Plants", "Saguaro_0" + (i + 1), new[] { "cactus" }, m => EnvPlants.Saguaro(m, 0, 11 + i * 6, 3.4f + i * 0.9f, i + 1), true, true);
            }

            for (int k = 0; k < 2; k++)
            {
                int i = k;
                Add("Desert", "Plants", "BarrelCactus_0" + (i + 1), new[] { "cactus" }, m => EnvPlants.BarrelCactus(m, 0, 0.35f + i * 0.2f, 0.6f + i * 0.4f), true, true);
                Add("Desert", "Plants", "PaddleCactus_0" + (i + 1), new[] { "cactus" }, m => EnvPlants.PaddleCactus(m, 0, 15 + i * 8, 5 + i * 3), true, false);
                Add("Desert", "Plants", "Agave_0" + (i + 1), new[] { "leaf_olive" }, m => EnvPlants.Agave(m, 0, 41 + i * 6, 9 + i * 4, 1.1f + i * 0.5f), false);
            }

            Bushes("Desert", "DryBush", "leaf_dry_bush", 3, 0.8f);
            Grasses("Desert", "DryGrass", "grass_dry", 3, 0.6f);
            FlowerPatches("Desert", "grass_dry", "flower_desert");

            Add("Desert", "Decor", "Column_01", new[] { "sandstone_blocks" }, m => EnvDecor.Column(m, 0, 3.6f, 0.45f, false), true, true);
            Add("Desert", "Decor", "Column_Broken_01", new[] { "sandstone_blocks" }, m => EnvDecor.Column(m, 0, 1.8f, 0.45f, true), true, true);
            Add("Desert", "Decor", "Obelisk_01", new[] { "sandstone_blocks" }, m => EnvDecor.Obelisk(m, 0, 5f), true, true);
            Add("Desert", "Decor", "Tent_01", new[] { "cloth_stripe", "wood_dark" }, m => EnvDecor.Tent(m, 0, 1, 2.4f), true, true);
            Add("Desert", "Decor", "Tent_02", new[] { "cloth_cream", "wood_dark" }, m => EnvDecor.Tent(m, 0, 1, 3.2f), true, true);
            for (int k = 0; k < 3; k++)
            {
                int i = k;
                Add("Desert", "Decor", "Vase_0" + (i + 1), new[] { "pottery" }, m => EnvDecor.Vase(m, 0, i), true, true);
            }

            Add("Desert", "Decor", "Crate_01", new[] { "wood_dark" }, m => EnvDecor.Crate(m, 0, 0.9f, 15f, Vector3.zero), true, true);
            Add("Desert", "Decor", "Barrel_01", new[] { "wood_dark", "metal" }, m => EnvDecor.Barrel(m, 0, 1, 0.5f, 1.0f), true, true);
        }
    }

    /// <summary>Highland and snow assets.</summary>
    public sealed class MountainCatalog : EnvTreeCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            Conifers("Mountain", "Spruce", "bark_oak", "leaf_spruce", new[] { 8f, 10f, 13f }, Vector3.zero);
            Conifers("Mountain", "SnowPine", "bark_oak", "leaf_pine_snow", new[] { 7f, 9f, 12f }, Vector3.zero);
            Conifers("Mountain", "HardyPine", "bark_dead", "leaf_pine_cool", new[] { 4.5f, 6f }, new Vector3(0.1f, 0f, 0.06f));
            DeadTrees("Mountain", 2, 6f, true);

            Boulders("Mountain", "Slate", "rock_slate", new[] { 0.8f, 1.3f, 2.0f, 2.8f }, 0.8f);
            Boulders("Mountain", "Granite", "rock_granite", new[] { 1.0f, 1.8f }, 0.85f);
            for (int k = 0; k < 3; k++)
            {
                int i = k;
                Add("Mountain", "Stones", "SnowRock_0" + (i + 1), new[] { "rock_slate", "snow" }, m =>
                {
                    float s = 1.0f + i * 0.7f;
                    EnvShapes.GroundedRock(m, 0, Vector3.zero, new Vector3(s, s * 0.85f, s * 0.9f), 120 + i * 9, 0.26f, 1.6f, 1);
                    EnvShapes.Rock(m, 1, new Vector3(0f, s * 0.75f, 0f), new Vector3(s * 0.78f, s * 0.22f, s * 0.7f), 140 + i * 9, 0.1f, 0.5f, 2f, 1, Vector3.zero);
                }, true, true);
                Add("Mountain", "Stones", "Spires_0" + (i + 1), new[] { "rock_slate" }, m => EnvRocks.Spires(m, 0, 150 + i * 7, 5f + i * 2.5f, 3 + i), true, true);
            }

            Clusters("Mountain", "Slate", "rock_slate");
            Add("Mountain", "Stones", "Arch_01", new[] { "rock_slate" }, m => EnvRocks.Arch(m, 0, 171, 5f, 4.5f), true, true);
            Add("Mountain", "Stones", "Cairn_01", new[] { "rock_slate" }, m => EnvRocks.Cairn(m, 0, 5, 5, 0.55f), true, true);
            Add("Mountain", "Stones", "Cairn_02", new[] { "rock_granite" }, m => EnvRocks.Cairn(m, 0, 9, 4, 0.7f), true, true);

            FlowerPatches("Mountain", "grass_green", "flower_alpine");
            Grasses("Mountain", "AlpineGrass", "grass_green", 2, 0.5f);
            Bushes("Mountain", "Shrub", "leaf_dry_bush", 2, 0.7f);
            Add("Mountain", "Plants", "Fern_01", new[] { "leaf_fern" }, m => EnvPlants.Fern(m, 0, 531, 9, 1f, 15f, 50f), false);

            Add("Mountain", "Decor", "IceCrystals_01", new[] { "ice" }, m => EnvDecor.IceCrystals(m, 0, 3, 5, 2.4f), true, true);
            Add("Mountain", "Decor", "IceCrystals_02", new[] { "ice" }, m => EnvDecor.IceCrystals(m, 0, 8, 7, 3.8f), true, true);
            Add("Mountain", "Decor", "SnowMound_01", new[] { "snow" }, m => EnvShapes.GroundedRock(m, 0, Vector3.zero, new Vector3(2.2f, 0.7f, 1.8f), 17, 0.08f, 2f, 1), true, false);
            Add("Mountain", "Decor", "SnowMound_02", new[] { "snow" }, m => EnvShapes.GroundedRock(m, 0, Vector3.zero, new Vector3(3.4f, 1.0f, 2.6f), 23, 0.08f, 2f, 1), true, false);
            Add("Mountain", "Decor", "Signpost_01", new[] { "wood_dark" }, m => EnvDecor.Signpost(m, 0), true, true);
            Add("Mountain", "Decor", "LampPost_01", new[] { "metal", "wood_dark" }, m => EnvDecor.LampPost(m, 0, 1), true, true);
        }
    }

    /// <summary>Cave, mine, tomb and dungeon entrances for every biome.</summary>
    public sealed class DungeonCatalog : EnvCatalogBase
    {
        /// <inheritdoc />
        protected override void Register()
        {
            string[] caveRocks = { "rock_granite", "rock_mossy", "rock_sandstone", "rock_slate" };
            string[] caveNames = { "CaveField", "CaveMossy", "CaveDesert", "CaveMountain" };
            for (int k = 0; k < caveRocks.Length; k++)
            {
                int i = k;
                Add("Dungeon", "Entrances", caveNames[i] + "_01", new[] { caveRocks[i], "dark" }, m => EnvEntrances.CaveMouth(m, 0, 1, 200 + i * 13, 3.4f + i * 0.5f, 3.2f + i * 0.4f, 6f), true, true);
            }

            Add("Dungeon", "Entrances", "CaveSmall_01", new[] { "rock_granite", "dark" }, m => EnvEntrances.CaveMouth(m, 0, 1, 311, 2.4f, 2.4f, 5f), true, true);
            Add("Dungeon", "Entrances", "CaveHuge_01", new[] { "rock_slate", "dark" }, m => EnvEntrances.CaveMouth(m, 0, 1, 313, 6f, 5.5f, 8f), true, true);
            Add("Dungeon", "Entrances", "CaveSea_01", new[] { "rock_sea", "dark" }, m => EnvEntrances.CaveMouth(m, 0, 1, 317, 4f, 3.4f, 6f), true, true);
            Add("Dungeon", "Entrances", "Mine_01", new[] { "rock_granite", "dark", "wood_dark" }, m => EnvEntrances.MineEntrance(m, 0, 1, 2, 330, 2.6f, 2.6f), true, true);
            Add("Dungeon", "Entrances", "Mine_02", new[] { "rock_slate", "dark", "wood_planks" }, m => EnvEntrances.MineEntrance(m, 0, 1, 2, 334, 3.2f, 3.0f), true, true);
            Add("Dungeon", "Entrances", "StoneGate_01", new[] { "ruin_blocks", "dark" }, m => EnvEntrances.StoneGate(m, 0, 1, 2.6f, 3.2f, false), true, true);
            Add("Dungeon", "Entrances", "StoneGateRuined_01", new[] { "ruin_blocks", "dark" }, m => EnvEntrances.StoneGate(m, 0, 1, 2.6f, 3.2f, true), true, true);
            Add("Dungeon", "Entrances", "SandTomb_01", new[] { "sandstone_blocks", "dark" }, m => EnvEntrances.StoneGate(m, 0, 1, 2.2f, 3.0f, false), true, true);
            Add("Dungeon", "Entrances", "SandTombRuined_01", new[] { "sandstone_blocks", "dark" }, m => EnvEntrances.StoneGate(m, 0, 1, 2.2f, 3.0f, true), true, true);
            Add("Dungeon", "Entrances", "SlateGate_01", new[] { "slate_blocks", "dark" }, m => EnvEntrances.StoneGate(m, 0, 1, 2.8f, 3.6f, false), true, true);
            Add("Dungeon", "Entrances", "Mausoleum_01", new[] { "ruin_blocks", "slate_blocks", "dark" }, m => EnvEntrances.Mausoleum(m, 0, 1, 2), true, true);
            Add("Dungeon", "Entrances", "Sinkhole_01", new[] { "rock_granite", "dark" }, m => EnvEntrances.Sinkhole(m, 0, 1, 401, 1.4f), true, true);
            Add("Dungeon", "Entrances", "Sinkhole_02", new[] { "rock_mossy", "dark" }, m => EnvEntrances.Sinkhole(m, 0, 1, 407, 2.0f), true, true);
            Add("Dungeon", "Entrances", "Sinkhole_03", new[] { "rock_sandstone", "dark" }, m => EnvEntrances.Sinkhole(m, 0, 1, 411, 1.7f), true, true);
            Add("Dungeon", "Entrances", "SealedDoor_01", new[] { "ruin_blocks", "metal" }, m => EnvEntrances.SealedDoor(m, 0, 1), true, true);
            Add("Dungeon", "Entrances", "SealedDoor_02", new[] { "slate_blocks", "metal" }, m => EnvEntrances.SealedDoor(m, 0, 1), true, true);
            Add("Dungeon", "Entrances", "ArchPortal_01", new[] { "ruin_blocks", "ice" }, m => EnvEntrances.ArchPortal(m, 0, 1, 2.6f, 3.4f), true, true);
            Add("Dungeon", "Entrances", "ArchPortal_02", new[] { "slate_blocks", "ice" }, m => EnvEntrances.ArchPortal(m, 0, 1, 3.4f, 4.2f), true, true);
            Add("Dungeon", "Entrances", "IceCave_01", new[] { "rock_slate", "snow", "ice", "dark" }, m => EnvEntrances.IceCave(m, 0, 1, 2, 3, 350), true, true);
        }
    }
}
