using UnityEngine;

namespace Project.EditorTools.Dungeon
{
    /// <summary>Places the props of every room of a dungeon layout.</summary>
    public sealed class DungeonDresser
    {
        private readonly DungeonLayout layout;
        private readonly DungeonPrefabLibrary library;

        /// <summary>Initializes a new instance of the <see cref="DungeonDresser"/> class.</summary>
        /// <param name="layout">The layout whose rooms are dressed.</param>
        /// <param name="library">The prefab source.</param>
        public DungeonDresser(DungeonLayout layout, DungeonPrefabLibrary library)
        {
            this.layout = layout;
            this.library = library;
        }

        /// <summary>Instantiates every prop of every room.</summary>
        /// <param name="parent">The parent transform.</param>
        /// <returns>The number of props placed.</returns>
        public int Dress(Transform parent)
        {
            int count = 0;
            foreach (var room in layout.Rooms)
            {
                foreach (PropSpec prop in room.Value.Props)
                {
                    Vector2 position = layout.ToWorld(room.Key, prop.U, prop.V);
                    GameObject instance = library.Instantiate(prop.Name, parent, new Vector3(position.x, prop.Lift, position.y), Quaternion.Euler(0f, prop.Yaw, 0f));
                    instance.name = $"{prop.Name}_{room.Key}{count}";
                    count++;
                }
            }

            return count;
        }
    }
}
