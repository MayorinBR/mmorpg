using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-off editor tool that groups item models and prefabs into per-type folders
/// using <see cref="AssetDatabase.MoveAsset"/>, which keeps GUIDs and references intact.
/// </summary>
public static class ItemFolderReorganizer
{
    private static readonly (string from, string to)[] Moves =
    {
        ("Assets/Models/Items/Axe", "Assets/Models/Items/Weapons/Axe"),
        ("Assets/Models/Items/Book", "Assets/Models/Items/Weapons/Book"),
        ("Assets/Models/Items/Bow", "Assets/Models/Items/Weapons/Bow"),
        ("Assets/Models/Items/Dagger", "Assets/Models/Items/Weapons/Dagger"),
        ("Assets/Models/Items/Staff", "Assets/Models/Items/Weapons/Staff"),
        ("Assets/Models/Items/Swords", "Assets/Models/Items/Weapons/Swords"),
        ("Assets/Models/Items/arrow.obj", "Assets/Models/Items/Projectiles/arrow.obj"),
        ("Assets/Models/Items/magic-bolt.obj", "Assets/Models/Items/Projectiles/magic-bolt.obj"),
        ("Assets/Prefabs/Items/AprenticeBook.prefab", "Assets/Prefabs/Items/Weapons/AprenticeBook.prefab"),
        ("Assets/Prefabs/Items/ArcaneStaff.prefab", "Assets/Prefabs/Items/Weapons/ArcaneStaff.prefab"),
        ("Assets/Prefabs/Items/IronDagger.prefab", "Assets/Prefabs/Items/Weapons/IronDagger.prefab"),
        ("Assets/Prefabs/Items/IronSword.prefab", "Assets/Prefabs/Items/Weapons/IronSword.prefab"),
        ("Assets/Prefabs/Items/NordicAxe.prefab", "Assets/Prefabs/Items/Weapons/NordicAxe.prefab"),
        ("Assets/Prefabs/Items/OakBow.prefab", "Assets/Prefabs/Items/Weapons/OakBow.prefab"),
        ("Assets/Prefabs/Items/ItemPickup.prefab", "Assets/Prefabs/Items/Pickups/ItemPickup.prefab"),
    };

    /// <summary>
    /// Moves every configured asset to its type folder, creating destination folders as needed.
    /// </summary>
    [MenuItem("Tools/Project/Reorganize Item Folders")]
    public static void Run()
    {
        var moved = 0;
        foreach (var (from, to) in Moves)
        {
            if (!AssetDatabase.IsValidFolder(from) && AssetDatabase.LoadMainAssetAtPath(from) == null)
            {
                continue;
            }

            EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
            var error = AssetDatabase.MoveAsset(from, to);
            if (string.IsNullOrEmpty(error))
            {
                moved++;
            }
            else
            {
                Debug.LogError($"Move failed: {from} -> {to}: {error}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Item folders reorganized: {moved} of {Moves.Length} moved.");
    }

    /// <summary>
    /// Creates the folder and any missing parents.
    /// </summary>
    /// <param name="path">Unity folder path starting with Assets.</param>
    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
