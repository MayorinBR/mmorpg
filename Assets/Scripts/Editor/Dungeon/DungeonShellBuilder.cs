using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Builds the floors, walls, doors and posts of a dungeon layout from kit pieces.</summary>
    public sealed class DungeonShellBuilder
    {
        private const int QuarterTurns = 4;
        private const float QuarterTurnDegrees = 90f;

        private readonly DungeonLayout layout;
        private readonly DungeonPrefabLibrary library;
        private readonly System.Random random;
        private readonly Dictionary<(Vector2Int, Vector2Int), string> openings = new Dictionary<(Vector2Int, Vector2Int), string>();

        /// <summary>Initializes a new instance of the <see cref="DungeonShellBuilder"/> class.</summary>
        /// <param name="layout">The layout to build.</param>
        /// <param name="library">The prefab source.</param>
        /// <param name="seed">Seed of the random choices, so a rebuild gives the same dungeon.</param>
        public DungeonShellBuilder(DungeonLayout layout, DungeonPrefabLibrary library, int seed)
        {
            this.layout = layout;
            this.library = library;
            random = new System.Random(seed);
            foreach (Opening opening in layout.Openings)
            {
                openings[Key(opening.First, opening.Second)] = opening.Piece;
            }
        }

        /// <summary>Places one floor tile on every floor cell.</summary>
        /// <param name="parent">The parent transform.</param>
        /// <param name="groundLayer">The physics layer given to the tiles, or zero to keep the prefab layer.</param>
        public void BuildFloors(Transform parent, int groundLayer)
        {
            foreach (Vector2Int cell in layout.Grid.Cells())
            {
                char room = layout.Grid.RoomAt(cell);
                string piece = layout.FloorOverrides.TryGetValue(cell, out string forced) ? forced : Pick(layout.Rooms[room].Style.Floors);
                Vector2 center = layout.Grid.CellCenter(cell);
                var rotation = Quaternion.Euler(0f, random.Next(QuarterTurns) * QuarterTurnDegrees, 0f);
                GameObject tile = library.Instantiate(piece, parent, new Vector3(center.x, 0f, center.y), rotation);
                tile.name = $"{piece}_{cell.x}_{cell.y}";
                if (groundLayer != 0)
                {
                    SetLayer(tile, groundLayer);
                }
            }
        }

        /// <summary>Places a wall, door or bars piece on every boundary between rooms.</summary>
        /// <param name="parent">The parent transform.</param>
        public void BuildWalls(Transform parent)
        {
            foreach (GridEdge edge in layout.Grid.Edges())
            {
                Vector2Int owner = OwnerCell(edge);
                char room = layout.Grid.RoomAt(owner);
                Vector2 first = layout.Grid.CellCenter(edge.First);
                Vector2 second = layout.Grid.CellCenter(edge.Second);
                Vector2 middle = (first + second) * 0.5f;
                Vector2 inward = (layout.Grid.CellCenter(owner) - middle).normalized;
                string piece = openings.TryGetValue(Key(edge.First, edge.Second), out string opening) ? opening : PickWall(layout.Rooms[room].Style, edge);
                GameObject wall = library.Instantiate(piece, parent, new Vector3(middle.x, 0f, middle.y), Quaternion.LookRotation(new Vector3(inward.x, 0f, inward.y)));
                wall.name = $"{piece}_{edge.First.x}_{edge.First.y}_{edge.Second.x}_{edge.Second.y}";
            }
        }

        /// <summary>Places a post wherever walls end, turn or branch.</summary>
        /// <param name="parent">The parent transform.</param>
        public void BuildPosts(Transform parent)
        {
            for (int row = 0; row <= layout.Grid.Rows; row++)
            {
                for (int column = 0; column <= layout.Grid.Columns; column++)
                {
                    WallDirections walls = layout.Grid.WallsAt(column, row);
                    if (!NeedsPost(walls))
                    {
                        continue;
                    }

                    Vector2 position = layout.Grid.VertexPosition(column, row);
                    string piece = layout.Rooms[ImportantRoomAround(column, row)].Style.Post;
                    GameObject post = library.Instantiate(piece, parent, new Vector3(position.x, 0f, position.y), Quaternion.identity);
                    post.name = $"{piece}_{column}_{row}";
                }
            }
        }

        private static (Vector2Int, Vector2Int) Key(Vector2Int a, Vector2Int b)
        {
            bool swap = b.x < a.x || (b.x == a.x && b.y < a.y);
            return swap ? (b, a) : (a, b);
        }

        private static bool NeedsPost(WallDirections walls)
        {
            if (walls == WallDirections.None)
            {
                return false;
            }

            bool straight = walls == (WallDirections.North | WallDirections.South) || walls == (WallDirections.East | WallDirections.West);
            return !straight;
        }

        private static void SetLayer(GameObject root, int layer)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = layer;
            }
        }

        private char ImportantRoomAround(int column, int row)
        {
            char best = DungeonGrid.Empty;
            int bestRank = int.MaxValue;
            foreach (char room in layout.Grid.RoomsAround(column, row))
            {
                if (room != DungeonGrid.Empty && layout.Rank(room) < bestRank)
                {
                    best = room;
                    bestRank = layout.Rank(room);
                }
            }

            return best;
        }

        private Vector2Int OwnerCell(GridEdge edge)
        {
            if (edge.FirstRoom == DungeonGrid.Empty)
            {
                return edge.Second;
            }

            if (edge.SecondRoom == DungeonGrid.Empty)
            {
                return edge.First;
            }

            return layout.Rank(edge.FirstRoom) <= layout.Rank(edge.SecondRoom) ? edge.First : edge.Second;
        }

        private string PickWall(RoomStyle style, GridEdge edge)
        {
            bool torch = style.TorchWall != null && edge.LineIndex % style.TorchEvery == 0;
            return torch ? style.TorchWall : Pick(style.Walls);
        }

        private string Pick(IReadOnlyList<string> choices) => choices[random.Next(choices.Count)];
    }
}
