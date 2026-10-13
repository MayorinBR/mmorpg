using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Character.Animation.EditorTools
{
    /// <summary>
    /// Keeps the vertical drop of death clips inside the pose. The player's Animator runs with
    /// root motion disabled, so any motion Unity extracts from the hips (the fall to the ground)
    /// is thrown away and the body ends up lying in the air at standing height. Baking the root
    /// transform into the pose keeps the fall in the skeleton, so the body lands on the floor.
    /// </summary>
    public class DeathAnimationImportSettings : AssetPostprocessor
    {
        private const string AnimationFolder = "Assets/Animations/Player";
        private const string MenuPath = "Tools/Animation/Reimport Death Animations";
        private static readonly string[] DeathNameKeywords = { "Dying", "Death" };

        /// <summary>
        /// Forces a reimport of every death clip so the baked root settings are applied
        /// without touching the Inspector.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void ReimportDeathAnimations()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { AnimationFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (IsDeathClip(path))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private void OnPreprocessModel()
        {
            if (!IsDeathClip(assetPath) || assetImporter is not ModelImporter importer)
            {
                return;
            }

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                return;
            }

            var clips = importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;

            importer.clipAnimations = clips.Select(BakeRootIntoPose).ToArray();
        }

        private static ModelImporterClipAnimation BakeRootIntoPose(ModelImporterClipAnimation clip)
        {
            clip.loopTime = false;
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalOrientation = false;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            clip.heightFromFeet = false;
            return clip;
        }

        private static bool IsDeathClip(string path)
        {
            if (!path.StartsWith(AnimationFolder, StringComparison.OrdinalIgnoreCase)
                || !path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return DeathNameKeywords.Any(keyword => path.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
