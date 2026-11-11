using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Project.EditorTools.Maps.World
{
    /// <summary>A warp portal placed where a land route crosses a map border.</summary>
    [Serializable]
    public sealed class WorldPortalData
    {
        /// <summary>Scene the portal leads to.</summary>
        public string dest;

        /// <summary>Local X of the portal.</summary>
        public float x;

        /// <summary>Local Z of the portal.</summary>
        public float z;

        /// <summary>X component of the unit vector pointing into the map.</summary>
        public float inx;

        /// <summary>Z component of the unit vector pointing into the map.</summary>
        public float inz;

        /// <summary>True when both maps already existed, so the existing portals are kept.</summary>
        public bool bothExisting;
    }

    /// <summary>Walkability of the four borders sampled every 2 m, one character per sample.</summary>
    [Serializable]
    public sealed class WorldWalkData
    {
        /// <summary>North border, west to east.</summary>
        public string N;

        /// <summary>East border, north to south.</summary>
        public string E;

        /// <summary>South border, west to east.</summary>
        public string S;

        /// <summary>West border, north to south.</summary>
        public string W;
    }

    /// <summary>Manifest entry describing one map of the world.</summary>
    [Serializable]
    public sealed class WorldCellInfo
    {
        /// <summary>Scene and map id.</summary>
        public string name;

        /// <summary>Row in the 10 x 10 world grid.</summary>
        public int r;

        /// <summary>Column in the 10 x 10 world grid.</summary>
        public int c;

        /// <summary>True for maps that existed before the world was designed.</summary>
        public bool existing;

        /// <summary>Terrain kind used for naming.</summary>
        public string kind;

        /// <summary>World height of the terrain origin.</summary>
        public float lo;

        /// <summary>Terrain height range in meters.</summary>
        public float size;

        /// <summary>Respawn position (x, z) in local coordinates.</summary>
        public float[] hub;

        /// <summary>Portals to place.</summary>
        public WorldPortalData[] portals;

        /// <summary>Border walkability.</summary>
        public WorldWalkData walk;

        /// <summary>Number of props in the props file.</summary>
        public int props;

        /// <summary>True when the north border is a forest the player cannot cross.</summary>
        public bool northBarrier;
    }

    /// <summary>Top level manifest of the baked world.</summary>
    [Serializable]
    public sealed class WorldManifest
    {
        /// <summary>Terrain layer asset names in splat order.</summary>
        public string[] layers;

        /// <summary>Every map of the world.</summary>
        public WorldCellInfo[] cells;
    }

    /// <summary>One prop to instantiate.</summary>
    public readonly struct WorldProp
    {
        /// <summary>Initializes a new instance of the <see cref="WorldProp"/> struct.</summary>
        /// <param name="prefab">Prefab name.</param>
        /// <param name="position">Local XZ position.</param>
        /// <param name="yaw">Yaw in degrees.</param>
        /// <param name="scale">Uniform scale.</param>
        /// <param name="height">Optional absolute world height; the terrain surface is used when null.</param>
        public WorldProp(string prefab, Vector2 position, float yaw, float scale, float? height = null)
        {
            Prefab = prefab;
            Position = position;
            Yaw = yaw;
            Scale = scale;
            Height = height;
        }

        /// <summary>Gets the prefab name.</summary>
        public string Prefab { get; }

        /// <summary>Gets the local XZ position.</summary>
        public Vector2 Position { get; }

        /// <summary>Gets the yaw in degrees.</summary>
        public float Yaw { get; }

        /// <summary>Gets the uniform scale.</summary>
        public float Scale { get; }

        /// <summary>Gets the absolute world height, or null to follow the terrain surface.</summary>
        public float? Height { get; }
    }

    /// <summary>The baked rasters of one map, laid out south to north and west to east.</summary>
    public sealed class WorldCellData
    {
        /// <summary>Heightmap samples per side.</summary>
        public const int HeightSamples = 513;

        /// <summary>Splat texels per side.</summary>
        public const int SplatSamples = 256;

        /// <summary>Water raster samples per side.</summary>
        public const int WaterSamples = 257;

        /// <summary>Gets the manifest entry.</summary>
        public WorldCellInfo Info { get; set; }

        /// <summary>Gets the normalized heights, 513 x 513.</summary>
        public ushort[] Heights { get; set; }

        /// <summary>Gets the splat weights, 256 x 256 x layers.</summary>
        public byte[] Splat { get; set; }

        /// <summary>Gets the water level in centimeters, 65 x 65, or short.MinValue where dry.</summary>
        public short[] Water { get; set; }

        /// <summary>Gets how much of the pre-existing terrain is kept, 256 x 256.</summary>
        public byte[] Keep { get; set; }

        /// <summary>Gets the props.</summary>
        public List<WorldProp> Props { get; set; }
    }

    /// <summary>Extracts and reads the baked world data.</summary>
    public sealed class WorldBuildData
    {
        private readonly string folder;

        private WorldBuildData(string folder, WorldManifest manifest)
        {
            this.folder = folder;
            Manifest = manifest;
        }

        /// <summary>Gets the manifest.</summary>
        public WorldManifest Manifest { get; }

        /// <summary>Extracts a data archive and reads its manifest.</summary>
        /// <param name="zipPath">Path of the archive.</param>
        /// <param name="workFolder">Folder the archive is extracted to.</param>
        /// <returns>The loaded data set.</returns>
        public static WorldBuildData Open(string zipPath, string workFolder)
        {
            if (Directory.Exists(workFolder))
            {
                Directory.Delete(workFolder, true);
            }

            ZipFile.ExtractToDirectory(zipPath, workFolder);
            var manifest = JsonUtility.FromJson<WorldManifest>(File.ReadAllText(Path.Combine(workFolder, "manifest.json")));
            return new WorldBuildData(workFolder, manifest);
        }

        /// <summary>Opens an already extracted data folder.</summary>
        /// <param name="workFolder">The extracted folder.</param>
        /// <returns>The loaded data set.</returns>
        public static WorldBuildData OpenExtracted(string workFolder)
        {
            var manifest = JsonUtility.FromJson<WorldManifest>(File.ReadAllText(Path.Combine(workFolder, "manifest.json")));
            return new WorldBuildData(workFolder, manifest);
        }

        /// <summary>Loads the rasters of one map.</summary>
        /// <param name="info">The manifest entry.</param>
        /// <returns>The map data.</returns>
        public WorldCellData Load(WorldCellInfo info)
        {
            string stem = Path.Combine(folder, info.name);
            return new WorldCellData
            {
                Info = info,
                Heights = ReadAll<ushort>(stem + ".h16"),
                Splat = File.ReadAllBytes(stem + ".spl"),
                Water = ReadAll<short>(stem + ".wtr"),
                Keep = File.ReadAllBytes(stem + ".wk"),
                Props = ReadProps(stem + ".props.txt"),
            };
        }

        private static T[] ReadAll<T>(string path)
            where T : struct
        {
            byte[] bytes = File.ReadAllBytes(path);
            var values = new T[bytes.Length / System.Runtime.InteropServices.Marshal.SizeOf<T>()];
            Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
            return values;
        }

        private static List<WorldProp> ReadProps(string path)
        {
            var props = new List<WorldProp>();
            if (!File.Exists(path))
            {
                return props;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                string[] part = line.Split(' ');
                if (part.Length < 5)
                {
                    continue;
                }

                props.Add(new WorldProp(
                    part[0],
                    new Vector2(Parse(part[1]), Parse(part[2])),
                    Parse(part[3]),
                    Parse(part[4]),
                    part.Length > 5 ? Parse(part[5]) : (float?)null));
            }

            return props;
        }

        private static float Parse(string text)
        {
            return float.Parse(text, CultureInfo.InvariantCulture);
        }
    }
}
