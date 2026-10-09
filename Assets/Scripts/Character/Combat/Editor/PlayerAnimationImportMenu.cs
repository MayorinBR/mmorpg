using UnityEditor;
using UnityEngine;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Editor menu that configures the import settings of the player animation clips
    /// dropped into the animations folder. A new Mixamo file is imported as Generic
    /// and without looping, so a Humanoid character cannot play it and an idle or run
    /// clip stops after one cycle; this menu switches each listed file to Humanoid
    /// and turns looping on where the clip needs it.
    /// </summary>
    public static class PlayerAnimationImportMenu
    {
        private const string AnimationsFolder = "Assets/Models/Player/Animations";

        private static readonly (string file, bool loops)[] Clips =
        {
            ("Idle", true),
            ("Run With Sword", true),
            ("Two Hand Sword Attack", false),
        };

        /// <summary>
        /// Sets every listed animation file to Humanoid with an avatar created from the file
        /// and applies its loop setting, then reimports it. Safe to run more than once.
        /// </summary>
        [MenuItem("Tools/Equipment/Setup Player Animation Imports")]
        public static void SetupImports()
        {
            foreach (var (file, loops) in Clips)
            {
                var path = $"{AnimationsFolder}/{file}.fbx";
                if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
                {
                    Debug.LogWarning($"Animation: missing {path}");
                    continue;
                }

                Configure(importer, loops);
                importer.SaveAndReimport();
            }

            Debug.Log("Animation: player animation imports configured.");
        }

        private static void Configure(ModelImporter importer, bool loops)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.loopTime = loops;
            }

            importer.clipAnimations = clips;
        }
    }
}
