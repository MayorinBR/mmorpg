using UnityEditor;
using UnityEngine;
using Project.Character.Combat;

namespace Project.EditorTools
{
    /// <summary>
    /// Play Mode debug window for granting a chosen number of base levels
    /// or job levels at once, without grinding experience. Each level is
    /// granted by feeding <see cref="PlayerExperience"/>/<see cref="PlayerJobProgress"/>
    /// exactly the experience their next level requires, one level at a
    /// time, so no partial experience is left over afterward.
    /// </summary>
    public class LevelUpDebugWindow : EditorWindow
    {
        private int levelsToAdd = 1;

        [MenuItem("Tools/Debug/Level Up Player... (Play Mode)")]
        private static void Open()
        {
            GetWindow<LevelUpDebugWindow>(true, "Level Up Player");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use this.", MessageType.Info);
                return;
            }

            levelsToAdd = Mathf.Max(1, EditorGUILayout.IntField("Levels to add", levelsToAdd));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Base Levels"))
                {
                    AddBaseLevels(levelsToAdd);
                }

                if (GUILayout.Button("Add Job Levels"))
                {
                    AddJobLevels(levelsToAdd);
                }
            }
        }

        private static void AddBaseLevels(int levels)
        {
            var experience = Object.FindFirstObjectByType<PlayerExperience>();

            if (experience == null)
            {
                Debug.LogWarning("LevelUpDebugWindow: no PlayerExperience found in the current scene.");
                return;
            }

            for (var i = 0; i < levels; i++)
            {
                experience.AddExperience(experience.RequiredExperienceForNextLevel);
            }

            Debug.Log($"LevelUpDebugWindow: added {levels} base level(s), now level {experience.CurrentLevel}.");
        }

        private static void AddJobLevels(int levels)
        {
            var jobProgress = Object.FindFirstObjectByType<PlayerJobProgress>();

            if (jobProgress == null)
            {
                Debug.LogWarning("LevelUpDebugWindow: no PlayerJobProgress found in the current scene.");
                return;
            }

            for (var i = 0; i < levels; i++)
            {
                jobProgress.AddExperience(jobProgress.RequiredExperienceForNextLevel);
            }

            Debug.Log($"LevelUpDebugWindow: added {levels} job level(s), now job level {jobProgress.JobLevel}.");
        }
    }
}
