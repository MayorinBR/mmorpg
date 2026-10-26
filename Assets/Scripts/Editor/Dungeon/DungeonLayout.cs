using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Which kit pieces build the floor, walls and posts of a room.</summary>
    public sealed class RoomStyle
    {
        /// <summary>Gets the floor pieces to pick from; repeating a name raises its weight.</summary>
        public IReadOnlyList<string> Floors { get; set; }

        /// <summary>Gets the wall pieces to pick from; repeating a name raises its weight.</summary>
        public IReadOnlyList<string> Walls { get; set; }

        /// <summary>Gets the wall piece that carries a torch, or null for none.</summary>
        public string TorchWall { get; set; }

        /// <summary>Gets how many wall segments pass between two torch walls.</summary>
        public int TorchEvery { get; set; } = 3;

        /// <summary>Gets the piece placed where walls meet.</summary>
        public string Post { get; set; }
    }

    /// <summary>A kit piece placed inside a room, measured from the south-west corner of the room.</summary>
    public readonly struct PropSpec
    {
        /// <summary>Initializes a new instance of the <see cref="PropSpec"/> struct.</summary>
        /// <param name="name">The kit piece name.</param>
        /// <param name="u">Meters east of the room's west edge.</param>
        /// <param name="v">Meters north of the room's south edge.</param>
        /// <param name="yaw">Rotation around the vertical axis; zero faces north.</param>
        /// <param name="lift">Height above the floor in meters.</param>
        public PropSpec(string name, float u, float v, float yaw, float lift)
        {
            Name = name;
            U = u;
            V = v;
            Yaw = yaw;
            Lift = lift;
        }

        /// <summary>Gets the kit piece name.</summary>
        public string Name { get; }

        /// <summary>Gets the distance from the west edge in meters.</summary>
        public float U { get; }

        /// <summary>Gets the distance from the south edge in meters.</summary>
        public float V { get; }

        /// <summary>Gets the rotation around the vertical axis in degrees.</summary>
        public float Yaw { get; }

        /// <summary>Gets the height above the floor in meters.</summary>
        public float Lift { get; }
    }

    /// <summary>A room: its style and the props that dress it.</summary>
    public sealed class RoomSpec
    {
        /// <summary>Initializes a new instance of the <see cref="RoomSpec"/> class.</summary>
        /// <param name="style">The room style.</param>
        /// <param name="props">The props of the room.</param>
        public RoomSpec(RoomStyle style, IReadOnlyList<PropSpec> props)
        {
            Style = style;
            Props = props;
        }

        /// <summary>Gets the room style.</summary>
        public RoomStyle Style { get; }

        /// <summary>Gets the props of the room.</summary>
        public IReadOnlyList<PropSpec> Props { get; }
    }

    /// <summary>A passage or barrier piece placed on the boundary between two cells.</summary>
    public readonly struct Opening
    {
        /// <summary>Initializes a new instance of the <see cref="Opening"/> struct.</summary>
        /// <param name="first">One of the two cells.</param>
        /// <param name="second">The other cell.</param>
        /// <param name="piece">The wall piece that fills the boundary.</param>
        public Opening(Vector2Int first, Vector2Int second, string piece)
        {
            First = first;
            Second = second;
            Piece = piece;
        }

        /// <summary>Gets one of the two cells.</summary>
        public Vector2Int First { get; }

        /// <summary>Gets the other cell.</summary>
        public Vector2Int Second { get; }

        /// <summary>Gets the wall piece that fills the boundary.</summary>
        public string Piece { get; }
    }

    /// <summary>A point where enemies spawn, measured from the south-west corner of a room.</summary>
    public readonly struct SpawnSpec
    {
        /// <summary>Initializes a new instance of the <see cref="SpawnSpec"/> struct.</summary>
        /// <param name="room">The room character.</param>
        /// <param name="u">Meters east of the room's west edge.</param>
        /// <param name="v">Meters north of the room's south edge.</param>
        public SpawnSpec(char room, float u, float v)
        {
            Room = room;
            U = u;
            V = v;
        }

        /// <summary>Gets the room character.</summary>
        public char Room { get; }

        /// <summary>Gets the distance from the west edge in meters.</summary>
        public float U { get; }

        /// <summary>Gets the distance from the south edge in meters.</summary>
        public float V { get; }
    }

    /// <summary>Everything the builders need to turn a grid of rooms into a dungeon.</summary>
    public sealed class DungeonLayout
    {
        /// <summary>Gets or sets the room grid.</summary>
        public DungeonGrid Grid { get; set; }

        /// <summary>Gets or sets the rooms by character.</summary>
        public IReadOnlyDictionary<char, RoomSpec> Rooms { get; set; }

        /// <summary>Gets or sets the doors and bars between rooms.</summary>
        public IReadOnlyList<Opening> Openings { get; set; }

        /// <summary>Gets or sets the room characters from the most to the least important; the first room owns a shared wall.</summary>
        public string Priority { get; set; }

        /// <summary>Gets or sets the floor pieces that replace the random pick for single cells.</summary>
        public IReadOnlyDictionary<Vector2Int, string> FloorOverrides { get; set; }

        /// <summary>Gets or sets the enemy spawn points.</summary>
        public IReadOnlyList<SpawnSpec> Spawns { get; set; }

        /// <summary>Gets or sets the boss spawn point.</summary>
        public SpawnSpec BossSpawn { get; set; }

        /// <summary>Gets the importance rank of a room; lower ranks own shared walls.</summary>
        /// <param name="room">The room character.</param>
        /// <returns>The rank, or a large number for an unknown room.</returns>
        public int Rank(char room)
        {
            int index = Priority.IndexOf(room);
            return index < 0 ? int.MaxValue : index;
        }

        /// <summary>Converts a room-relative point to a world XZ position.</summary>
        /// <param name="room">The room character.</param>
        /// <param name="u">Meters east of the room's west edge.</param>
        /// <param name="v">Meters north of the room's south edge.</param>
        /// <returns>The world XZ position.</returns>
        public Vector2 ToWorld(char room, float u, float v)
        {
            Rect rect = Grid.RoomRect(room);
            return new Vector2(rect.xMin + u, rect.yMin + v);
        }
    }
}
