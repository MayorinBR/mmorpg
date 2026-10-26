using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>The wall directions that leave a grid vertex.</summary>
    [Flags]
    public enum WallDirections
    {
        /// <summary>No wall touches the vertex.</summary>
        None = 0,

        /// <summary>A wall runs toward the south.</summary>
        South = 1,

        /// <summary>A wall runs toward the north.</summary>
        North = 2,

        /// <summary>A wall runs toward the west.</summary>
        West = 4,

        /// <summary>A wall runs toward the east.</summary>
        East = 8,
    }

    /// <summary>The boundary between two neighbouring cells that belong to different rooms.</summary>
    public readonly struct GridEdge
    {
        /// <summary>Initializes a new instance of the <see cref="GridEdge"/> struct.</summary>
        /// <param name="first">The western or southern cell.</param>
        /// <param name="second">The eastern or northern cell.</param>
        /// <param name="firstRoom">Room of the first cell.</param>
        /// <param name="secondRoom">Room of the second cell.</param>
        public GridEdge(Vector2Int first, Vector2Int second, char firstRoom, char secondRoom)
        {
            First = first;
            Second = second;
            FirstRoom = firstRoom;
            SecondRoom = secondRoom;
        }

        /// <summary>Gets the western or southern cell.</summary>
        public Vector2Int First { get; }

        /// <summary>Gets the eastern or northern cell.</summary>
        public Vector2Int Second { get; }

        /// <summary>Gets the room of the first cell.</summary>
        public char FirstRoom { get; }

        /// <summary>Gets the room of the second cell.</summary>
        public char SecondRoom { get; }

        /// <summary>Gets a value indicating whether the edge runs along the world z axis.</summary>
        public bool RunsAlongZ => First.y == Second.y;

        /// <summary>Gets the position of the edge along its wall line, used to space repeating pieces.</summary>
        public int LineIndex => RunsAlongZ ? First.y : First.x;
    }

    /// <summary>A rectangular grid of room cells on the XZ plane; row zero is the southern-most row.</summary>
    public sealed class DungeonGrid
    {
        /// <summary>The character of a cell without floor.</summary>
        public const char Empty = '.';

        /// <summary>The side length of a cell in meters.</summary>
        public const float CellSize = 4f;

        private readonly IReadOnlyList<string> rows;
        private readonly Vector2 origin;

        /// <summary>Initializes a new instance of the <see cref="DungeonGrid"/> class.</summary>
        /// <param name="rows">One string per row, from south to north; each character names the room of a cell.</param>
        /// <param name="origin">World XZ position of the center of cell (0, 0).</param>
        public DungeonGrid(IReadOnlyList<string> rows, Vector2 origin)
        {
            Columns = rows[0].Length;
            foreach (string row in rows)
            {
                if (row.Length != Columns)
                {
                    throw new ArgumentException("All grid rows must have the same length.", nameof(rows));
                }
            }

            this.rows = rows;
            this.origin = origin;
            Rows = rows.Count;
        }

        /// <summary>Gets the number of columns.</summary>
        public int Columns { get; }

        /// <summary>Gets the number of rows.</summary>
        public int Rows { get; }

        /// <summary>Gets the room character of a cell.</summary>
        /// <param name="cell">The cell coordinates.</param>
        /// <returns>The room character, or <see cref="Empty"/> outside the grid.</returns>
        public char RoomAt(Vector2Int cell)
        {
            return cell.x < 0 || cell.y < 0 || cell.x >= Columns || cell.y >= Rows ? Empty : rows[cell.y][cell.x];
        }

        /// <summary>Gets the world XZ position of a cell center.</summary>
        /// <param name="cell">The cell coordinates.</param>
        /// <returns>The cell center.</returns>
        public Vector2 CellCenter(Vector2Int cell) => origin + (new Vector2(cell.x, cell.y) * CellSize);

        /// <summary>Gets the world XZ position of the vertex at the south-west corner of a cell.</summary>
        /// <param name="column">The column of the cell.</param>
        /// <param name="row">The row of the cell.</param>
        /// <returns>The vertex position.</returns>
        public Vector2 VertexPosition(int column, int row) => origin + (new Vector2(column - 0.5f, row - 0.5f) * CellSize);

        /// <summary>Enumerates the cells that have a floor.</summary>
        /// <returns>The floor cells.</returns>
        public IEnumerable<Vector2Int> Cells()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    var cell = new Vector2Int(column, row);
                    if (RoomAt(cell) != Empty)
                    {
                        yield return cell;
                    }
                }
            }
        }

        /// <summary>Gets the world rectangle covered by all cells of a room.</summary>
        /// <param name="room">The room character.</param>
        /// <returns>A rectangle whose x and y are the minimum world x and z.</returns>
        public Rect RoomRect(char room)
        {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;
            foreach (Vector2Int cell in Cells())
            {
                if (RoomAt(cell) != room)
                {
                    continue;
                }

                Vector2 center = CellCenter(cell);
                minX = Mathf.Min(minX, center.x);
                minZ = Mathf.Min(minZ, center.y);
                maxX = Mathf.Max(maxX, center.x);
                maxZ = Mathf.Max(maxZ, center.y);
            }

            float half = CellSize * 0.5f;
            return Rect.MinMaxRect(minX - half, minZ - half, maxX + half, maxZ + half);
        }

        /// <summary>Enumerates every boundary between cells of different rooms, including the outer boundary.</summary>
        /// <returns>The edges.</returns>
        public IEnumerable<GridEdge> Edges()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = -1; column < Columns; column++)
                {
                    var first = new Vector2Int(column, row);
                    var second = new Vector2Int(column + 1, row);
                    if (RoomAt(first) != RoomAt(second))
                    {
                        yield return new GridEdge(first, second, RoomAt(first), RoomAt(second));
                    }
                }
            }

            for (int column = 0; column < Columns; column++)
            {
                for (int row = -1; row < Rows; row++)
                {
                    var first = new Vector2Int(column, row);
                    var second = new Vector2Int(column, row + 1);
                    if (RoomAt(first) != RoomAt(second))
                    {
                        yield return new GridEdge(first, second, RoomAt(first), RoomAt(second));
                    }
                }
            }
        }

        /// <summary>Gets which walls leave a vertex.</summary>
        /// <param name="column">The vertex column; the vertex lies at the south-west corner of that cell.</param>
        /// <param name="row">The vertex row.</param>
        /// <returns>The wall directions at the vertex.</returns>
        public WallDirections WallsAt(int column, int row)
        {
            char southWest = RoomAt(new Vector2Int(column - 1, row - 1));
            char southEast = RoomAt(new Vector2Int(column, row - 1));
            char northWest = RoomAt(new Vector2Int(column - 1, row));
            char northEast = RoomAt(new Vector2Int(column, row));
            WallDirections walls = WallDirections.None;
            walls |= southWest != southEast ? WallDirections.South : WallDirections.None;
            walls |= northWest != northEast ? WallDirections.North : WallDirections.None;
            walls |= southWest != northWest ? WallDirections.West : WallDirections.None;
            walls |= southEast != northEast ? WallDirections.East : WallDirections.None;
            return walls;
        }

        /// <summary>Gets the rooms of the four cells around a vertex.</summary>
        /// <param name="column">The vertex column.</param>
        /// <param name="row">The vertex row.</param>
        /// <returns>The four room characters, empty cells included.</returns>
        public IEnumerable<char> RoomsAround(int column, int row)
        {
            yield return RoomAt(new Vector2Int(column - 1, row - 1));
            yield return RoomAt(new Vector2Int(column, row - 1));
            yield return RoomAt(new Vector2Int(column - 1, row));
            yield return RoomAt(new Vector2Int(column, row));
        }
    }
}
