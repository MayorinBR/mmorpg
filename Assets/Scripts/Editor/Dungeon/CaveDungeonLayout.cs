using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>The dungeon layout of Prototype_Cave01: entrance hall, guard hall, prison, crypt, ruins, library, cave and boss throne room.</summary>
    public static class CaveDungeonLayout
    {
        private const float North = 0f;
        private const float East = 90f;
        private const float South = 180f;
        private const float West = 270f;
        private const string Arch = "wall_arch_open";
        private const string CellRooms = "klmnop";

        private static readonly string[] Map =
        {
            "......klmnop..........",
            "SSSS..DDDDDDsLLLL.....",
            "SSSS..DDDDDD.LLLL.....",
            "SSSS..BBBBB..LLLLRRRRR",
            "AAAA..BBBBB..FFF.RRRRR",
            "AAAAccBBBBBuuFFFqRRRRR",
            "AAAA..BBBBB..FFF.RRRRR",
            "WWWW..BBBBB..GGGGRRRRR",
            "WWWW..EEEEEE.GGGG.....",
            "WWWW..EEEEEErGGGG.....",
            "......EEEEEE..........",
        };

        /// <summary>Creates the layout.</summary>
        /// <returns>The layout whose cell (0, 5) is centered on the existing entrance portal.</returns>
        public static DungeonLayout Create()
        {
            return new DungeonLayout
            {
                Grid = new DungeonGrid(Map, new Vector2(0f, 1.5f)),
                Rooms = BuildRooms(),
                Openings = BuildOpenings(),
                Priority = "RAFBLEDSWqcursklmnopG",
                FloorOverrides = new Dictionary<Vector2Int, string> { { new Vector2Int(19, 5), "floor_rune" } },
                Spawns = BuildSpawns(),
                BossSpawn = new SpawnSpec('R', 10f, 10f),
            };
        }

        private static IReadOnlyList<SpawnSpec> BuildSpawns()
        {
            return new[]
            {
                new SpawnSpec('B', 3.5f, 15.5f), new SpawnSpec('B', 16.5f, 15.5f), new SpawnSpec('B', 3.5f, 4.5f), new SpawnSpec('B', 16.5f, 4.5f),
                new SpawnSpec('D', 7f, 3.5f), new SpawnSpec('D', 15f, 4f),
                new SpawnSpec('E', 6f, 3.2f), new SpawnSpec('E', 18f, 3.5f),
                new SpawnSpec('F', 3.2f, 6f), new SpawnSpec('F', 9f, 6f),
                new SpawnSpec('L', 4f, 5.5f), new SpawnSpec('L', 11f, 3f),
                new SpawnSpec('G', 4.5f, 6.8f), new SpawnSpec('G', 8.5f, 5f), new SpawnSpec('G', 11.5f, 3f),
                new SpawnSpec('R', 4f, 5f), new SpawnSpec('R', 4f, 15f), new SpawnSpec('R', 14f, 10f),
            };
        }

        private static IReadOnlyList<Opening> BuildOpenings()
        {
            var openings = new List<Opening>
            {
                Door(3, 5, 4, 5), Door(5, 5, 6, 5), Door(1, 4, 1, 3), Door(1, 6, 1, 7),
                Door(8, 3, 8, 2), Door(8, 7, 8, 8), Door(10, 5, 11, 5), Door(12, 5, 13, 5),
                Door(11, 1, 12, 1), Door(12, 1, 13, 1), Door(14, 3, 14, 4), Door(15, 5, 16, 5),
                Door(11, 9, 12, 9), Door(12, 9, 13, 9), Door(14, 6, 14, 7),
                Door(16, 5, 17, 5), Door(16, 7, 17, 7),
                new Opening(new Vector2Int(19, 7), new Vector2Int(19, 8), "wall_arch_boss"),
            };

            for (int column = 6; column <= 11; column++)
            {
                openings.Add(new Opening(new Vector2Int(column, 0), new Vector2Int(column, 1), "wall_cell_bars"));
            }

            return openings;
        }

        private static Opening Door(int firstColumn, int firstRow, int secondColumn, int secondRow)
        {
            return new Opening(new Vector2Int(firstColumn, firstRow), new Vector2Int(secondColumn, secondRow), Arch);
        }

        private static IReadOnlyDictionary<char, RoomSpec> BuildRooms()
        {
            var rooms = new Dictionary<char, RoomSpec>
            {
                { 'A', new RoomSpec(HallStyle(), EntranceProps()) },
                { 'B', new RoomSpec(GuardStyle(), GuardProps()) },
                { 'D', new RoomSpec(PrisonStyle(), PrisonProps()) },
                { 'E', new RoomSpec(CryptStyle(), CryptProps()) },
                { 'S', new RoomSpec(StoreStyle(), StoreProps()) },
                { 'W', new RoomSpec(StoreStyle(), BarracksProps()) },
                { 'F', new RoomSpec(RuinsStyle(), RuinsProps()) },
                { 'L', new RoomSpec(LibraryStyle(), LibraryProps()) },
                { 'G', new RoomSpec(CaveStyle(), CaveProps()) },
                { 'R', new RoomSpec(BossStyle(), BossProps()) },
            };

            foreach (char corridor in "cursq")
            {
                rooms[corridor] = new RoomSpec(corridor == 'q' ? AnteStyle() : CorridorStyle(), new PropSpec[0]);
            }

            foreach (char cell in CellRooms)
            {
                rooms[cell] = new RoomSpec(CellStyle(), CellProps(cell));
            }

            return rooms;
        }

        private static RoomStyle HallStyle() => new RoomStyle
        {
            Floors = new[] { "floor_flagstone", "floor_flagstone_slabs", "floor_flagstone" },
            Walls = new[] { "wall_straight", "wall_straight", "wall_cracked", "wall_straight_buttress", "wall_banner" },
            TorchWall = "wall_torch",
            TorchEvery = 2,
            Post = "wall_post_torch",
        };

        private static RoomStyle GuardStyle() => new RoomStyle
        {
            Floors = new[] { "floor_checker", "floor_flagstone", "floor_checker", "floor_cracked" },
            Walls = new[] { "wall_straight", "wall_straight_buttress", "wall_cracked", "wall_banner" },
            TorchWall = "wall_torch",
            TorchEvery = 3,
            Post = "wall_post_round",
        };

        private static RoomStyle CorridorStyle() => new RoomStyle
        {
            Floors = new[] { "floor_brick" },
            Walls = new[] { "wall_straight", "wall_straight", "wall_cracked", "wall_moss" },
            TorchWall = "wall_torch",
            TorchEvery = 2,
            Post = "wall_post",
        };

        private static RoomStyle AnteStyle() => new RoomStyle
        {
            Floors = new[] { "floor_flagstone_slabs" },
            Walls = new[] { "wall_straight", "wall_cracked" },
            TorchWall = "wall_torch",
            TorchEvery = 2,
            Post = "wall_post_torch",
        };

        private static RoomStyle PrisonStyle() => new RoomStyle
        {
            Floors = new[] { "floor_brick", "floor_drain", "floor_cracked", "floor_brick", "floor_moss" },
            Walls = new[] { "wall_straight", "wall_window_bars", "wall_moss", "wall_cracked", "wall_straight" },
            TorchWall = "wall_torch",
            TorchEvery = 3,
            Post = "wall_post",
        };

        private static RoomStyle CellStyle() => new RoomStyle
        {
            Floors = new[] { "floor_brick" },
            Walls = new[] { "wall_straight", "wall_moss", "wall_cracked" },
            Post = "wall_post",
        };

        private static RoomStyle CryptStyle() => new RoomStyle
        {
            Floors = new[] { "floor_flagstone_slabs", "floor_cracked", "floor_moss", "floor_flagstone" },
            Walls = new[] { "wall_straight", "wall_moss", "wall_cracked", "wall_straight_buttress" },
            TorchWall = "wall_torch",
            TorchEvery = 3,
            Post = "wall_post_round",
        };

        private static RoomStyle StoreStyle() => new RoomStyle
        {
            Floors = new[] { "floor_wood", "floor_brick" },
            Walls = new[] { "wall_straight", "wall_cracked", "wall_straight" },
            TorchWall = "wall_torch",
            TorchEvery = 4,
            Post = "wall_post",
        };

        private static RoomStyle RuinsStyle() => new RoomStyle
        {
            Floors = new[] { "floor_cracked", "floor_moss", "floor_flagstone", "floor_cracked" },
            Walls = new[] { "wall_ruined_a", "wall_ruined_b", "wall_collapsed", "wall_cracked", "wall_moss" },
            TorchWall = "wall_torch",
            TorchEvery = 4,
            Post = "wall_post",
        };

        private static RoomStyle LibraryStyle() => new RoomStyle
        {
            Floors = new[] { "floor_wood", "floor_cracked" },
            Walls = new[] { "wall_straight", "wall_cracked", "wall_moss", "wall_straight" },
            TorchWall = "wall_torch",
            TorchEvery = 3,
            Post = "wall_post",
        };

        private static RoomStyle CaveStyle() => new RoomStyle
        {
            Floors = new[] { "floor_cave" },
            Walls = new[] { "cave_wall_a", "cave_wall_b", "cave_wall_c", "cave_wall_mossy", "cave_wall_crystals" },
            Post = "cave_column",
        };

        private static RoomStyle BossStyle() => new RoomStyle
        {
            Floors = new[] { "floor_checker", "floor_flagstone" },
            Walls = new[] { "wall_straight", "wall_banner", "wall_straight_buttress", "wall_straight" },
            TorchWall = "wall_torch",
            TorchEvery = 2,
            Post = "wall_post_tall_gargoyle",
        };

        private static PropSpec P(string name, float u, float v, float yaw = North, float lift = 0f) => new PropSpec(name, u, v, yaw, lift);

        private static PropSpec[] EntranceProps() => new[]
        {
            P("brazier_bowl", 2.4f, 2f), P("brazier_bowl", 2.4f, 10f),
            P("pillar_square", 8f, 3f), P("pillar_square", 8f, 9f), P("pillar_square", 12f, 3f), P("pillar_square", 12f, 9f),
            P("statue_knight", 14f, 2.2f, West), P("statue_knight", 14f, 9.8f, West),
            P("banner_standing", 9.5f, 0.9f), P("banner_standing", 9.5f, 11.1f, South),
            P("weapon_rack", 13f, 0.8f), P("armor_stand", 13f, 11.2f, South),
            P("barrel_old", 1.2f, 1.1f), P("barrel_old", 2.3f, 0.95f), P("crate_a", 1.1f, 11f, 20f),
        };

        private static PropSpec[] GuardProps() => new[]
        {
            P("pillar_round", 5.5f, 5f), P("pillar_round", 14.5f, 5f), P("pillar_round", 5.5f, 15f), P("pillar_round", 14.5f, 15f),
            P("brazier_bowl", 8f, 2.2f), P("brazier_bowl", 12f, 2.2f), P("brazier_bowl", 8f, 17.8f), P("brazier_bowl", 12f, 17.8f),
            P("statue_knight", 1.9f, 3f, East), P("statue_knight", 1.9f, 17f, East),
            P("statue_knight", 18.1f, 3f, West), P("statue_knight", 18.1f, 17f, West),
            P("weapon_rack", 4.5f, 19.1f, South), P("armor_stand", 15.5f, 19.2f, South),
            P("skeleton_lying", 16.5f, 8f, 30f), P("bones_scatter_a", 4f, 8.5f), P("bones_scatter_a", 15f, 12.5f, 40f),
            P("skull_pile", 18.2f, 1.6f), P("crate_stack", 17.5f, 18f, 15f), P("barrel_old", 1.1f, 19f),
        };

        private static PropSpec[] PrisonProps() => new[]
        {
            P("barrel_old", 1f, 7f), P("barrel_broken", 2.4f, 7f, 40f),
            P("table_wood_old", 20.5f, 5.4f, East), P("chair_broken", 18.6f, 5.2f, 100f), P("bucket_wood", 22.6f, 6.8f),
            P("skeleton_chained", 4.5f, 7.6f, South), P("skeleton_chained", 14.5f, 7.6f, South), P("shackles_wall", 17.5f, 7.6f, South),
            P("hanging_cage", 6f, 4.8f), P("candelabra_floor", 22.5f, 1.2f), P("bones_scatter_b", 12f, 3f, 70f),
        };

        private static PropSpec[] CellProps(char cell)
        {
            var props = new List<PropSpec>
            {
                P("bed_straw", 1f, 2.2f), P("bucket_wood", 3.1f, 3f), P("shackles_wall", 2.6f, 0.6f),
            };

            if (cell == 'l' || cell == 'n' || cell == 'p')
            {
                props.Add(P("skeleton_lying", 2.6f, 2f, 80f));
            }

            return props.ToArray();
        }

        private static PropSpec[] CryptProps() => new[]
        {
            P("sarcophagus_stone", 3.5f, 9.3f), P("sarcophagus_stone", 7f, 9.3f), P("sarcophagus_broken", 17f, 9.2f), P("sarcophagus_stone", 20.5f, 9.3f),
            P("altar_stone", 12f, 10f, South),
            P("gravestone_a", 4f, 2.5f, 20f), P("gravestone_a", 7f, 2.8f, 345f), P("gravestone_a", 17.5f, 2.4f, 10f),
            P("gravestone_cross", 20.5f, 2.6f), P("gravestone_broken", 13.5f, 2f),
            P("candelabra_floor", 1.5f, 10.5f), P("candelabra_floor", 22.5f, 10.5f),
            P("urn_large", 1.3f, 1.2f), P("urn_large", 22.7f, 1.2f), P("urn_small", 2.5f, 1f),
            P("coffin_open", 14.2f, 4.2f, 15f), P("skeleton_lying", 16f, 5.4f, 70f), P("bones_scatter_a", 6f, 5f),
        };

        private static PropSpec[] StoreProps() => new[]
        {
            P("crate_stack", 2.2f, 2.2f), P("crate_a", 4.3f, 1f), P("crate_broken", 1.3f, 5f, 30f),
            P("barrel_old", 14.5f, 1.2f), P("barrel_old", 13.4f, 1f), P("barrel_broken", 14.4f, 2.8f, 60f),
            P("sack_pile", 14.4f, 10.4f), P("chest_wood", 10f, 10.8f, South), P("chest_iron", 12.8f, 10.8f, South),
            P("shelf_potions", 8f, 0.7f), P("shelf_potions", 11f, 0.7f),
            P("table_wood_old", 12f, 5.5f, East), P("bucket_wood", 13.8f, 6.2f),
        };

        private static PropSpec[] BarracksProps() => new[]
        {
            P("bed_straw", 1.4f, 10.2f), P("bed_straw", 3.6f, 10.2f), P("bed_straw", 10f, 10.2f), P("bed_straw", 12.2f, 10.2f), P("bed_straw", 14.4f, 10.2f),
            P("table_wood_old", 8f, 4.5f), P("bench_wood_broken", 8f, 2.7f, East), P("chair_broken", 10.2f, 5f, 280f),
            P("weapon_rack", 14.5f, 0.8f), P("armor_stand", 12.5f, 0.9f), P("chest_wood", 1.5f, 1f), P("bucket_wood", 3f, 1.1f),
        };

        private static PropSpec[] RuinsProps() => new[]
        {
            P("well_dry", 6f, 6f),
            P("pillar_broken", 2.8f, 2.6f), P("pillar_stump", 9.5f, 2.3f), P("pillar_stump", 2.5f, 9.5f),
            P("fallen_pillar", 8.4f, 9.4f, 10f), P("rubble_a", 2.6f, 4.3f, 30f), P("rubble_b", 9.4f, 4.6f, 90f),
            P("skeleton_lying", 3.5f, 7.5f, 100f), P("bones_scatter_a", 8.5f, 7f), P("debris_planks", 9.8f, 7.6f, 20f),
        };

        private static PropSpec[] LibraryProps() => new[]
        {
            P("bookshelf_ruined", 3f, 0.9f), P("bookshelf_ruined", 6.5f, 0.9f), P("bookshelf_ruined", 10f, 0.9f), P("bookshelf_ruined", 13.5f, 0.9f),
            P("bookshelf_ruined", 0.9f, 7.2f, East), P("bookshelf_ruined", 0.9f, 9.9f, East),
            P("table_wood_old", 9f, 7f), P("chair_broken", 7.3f, 7.2f, 100f), P("chair_broken", 10.8f, 6.3f, 280f),
            P("candelabra_floor", 13.2f, 10f), P("debris_planks", 14f, 5f, 30f), P("bones_scatter_b", 5f, 4f),
            P("urn_small", 14.8f, 1.2f), P("rubble_b", 13.5f, 8.4f, 20f), P("skeleton_lying", 12.5f, 4.5f, 150f),
            P("chest_open_gold", 14.8f, 10.8f, South),
            P("torch_standing", 1.8f, 3.2f), P("torch_standing", 14.4f, 6.2f),
        };

        private static PropSpec[] CaveProps() => new[]
        {
            P("stalagmite_cluster", 2.5f, 2.5f), P("stalagmite_cluster", 13.5f, 10f, 40f), P("stalagmite_a", 11.5f, 8.5f), P("stalagmite_b", 4.5f, 11f),
            P("rock_large_a", 14f, 7.5f, 60f), P("rock_large_b", 2.5f, 4.5f, 20f), P("rock_pile", 9f, 10f), P("rock_small_a", 6f, 8.5f), P("rock_small_b", 12f, 0.9f),
            P("crystal_cluster_blue", 1.9f, 5.8f), P("crystal_cluster_purple", 7.5f, 1.5f), P("crystal_cluster_blue", 14.6f, 10.4f, 120f),
            P("mushroom_glow", 4.5f, 10.5f), P("mushroom_glow", 12.3f, 1.6f), P("mushroom_glow", 1.6f, 1.8f),
            P("bones_scatter_a", 9f, 4.5f, 30f),
            P("torch_standing", 1.5f, 1.4f), P("torch_standing", 14.6f, 11f), P("torch_standing", 10f, 1.2f), P("brazier_bowl", 8f, 8.6f),
            P("crate_a", 7.2f, 10.4f, 25f), P("mining_pickaxe_prop", 6f, 9.6f, 40f),
        };

        private static PropSpec[] BossProps() => new[]
        {
            P("platform_dais", 18f, 10f), P("throne_dark", 18.3f, 10f, West, 0.96f),
            P("pillar_carved", 5f, 4.5f), P("pillar_carved", 5f, 15.5f), P("pillar_carved", 11.5f, 4.5f), P("pillar_carved", 11.5f, 15.5f),
            P("brazier_bowl", 15f, 5.5f), P("brazier_bowl", 15f, 14.5f), P("brazier_bowl", 3f, 13.5f),
            P("statue_knight", 17.8f, 2.4f, West), P("statue_knight", 17.8f, 17.6f, West),
            P("banner_standing", 8f, 1.1f), P("banner_standing", 8f, 18.9f, South),
            P("skull_pile", 16.5f, 6.2f), P("skeleton_lying", 13f, 15.5f, 80f), P("bones_scatter_a", 7f, 12.5f),
            P("weapon_rack", 13f, 1f), P("armor_stand", 16f, 18.9f, South), P("urn_large", 1.2f, 1.4f), P("urn_large", 1.2f, 15.3f),
        };
    }
}
