namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Low shrubs and bushes shared by every biome, saved under Common/Shrubs.</summary>
    public sealed class ShrubCatalog : EnvCatalogBase
    {
        private const string Area = "Common";
        private const string Type = "Shrubs";

        /// <inheritdoc />
        protected override void Register()
        {
            RegisterShapes("Green", "leaf_bush", 6000);
            RegisterShapes("Olive", "leaf_olive", 6100);
            RegisterShapes("Dry", "leaf_dry_bush", 6200);
            RegisterThemed("ShrubFlowering_Pink", "leaf_shrub_pink", new ShrubSpec(6300, 0.75f, 0.85f, 4, 0.8f));
            RegisterThemed("ShrubFlowering_White", "leaf_shrub_white", new ShrubSpec(6310, 0.7f, 0.8f, 4, 0.8f));
            RegisterThemed("ShrubBerry_Red", "leaf_shrub_berry", new ShrubSpec(6320, 0.7f, 0.8f, 3, 0.75f));
            RegisterThemed("ShrubAutumn_Orange", "leaf_shrub_autumn", new ShrubSpec(6330, 0.8f, 0.95f, 4, 0.8f));
            RegisterThemed("ShrubHeather_Purple", "leaf_shrub_heather", new ShrubSpec(6340, 0.9f, 0.5f, 5, 0.9f, 10));
        }

        private void RegisterShapes(string tint, string material, int seed)
        {
            RegisterThemed("ShrubRound_" + tint, material, new ShrubSpec(seed, 0.7f, 0.8f, 1, 0f, 16));
            RegisterThemed("ShrubWide_" + tint, material, new ShrubSpec(seed + 1, 0.8f, 0.55f, 4, 0.9f));
            RegisterThemed("ShrubTall_" + tint, material, new ShrubSpec(seed + 2, 0.55f, 1.3f, 3, 0.55f));
        }

        private void RegisterThemed(string name, string material, ShrubSpec spec)
        {
            Add(Area, Type, name, new[] { material }, m => EnvShrubs.Shrub(m, 0, spec));
        }
    }
}
