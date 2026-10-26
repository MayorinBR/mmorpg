using System.IO;
using UnityEditor;

namespace Project.EditorTools
{
    /// <summary>
    /// Creates nested asset folders on demand.
    /// </summary>
    public static class AssetFolders
    {
        /// <summary>
        /// Makes sure the given project-relative folder exists, creating missing parents.
        /// </summary>
        /// <param name="folder">Project-relative folder path such as <c>Assets/Materials/Greybox</c>.</param>
        public static void Ensure(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
