using Project.Character.Combat;
using Project.Skills;
using System.Linq;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Populates the Skill Book window with one <see cref="SkillBookEntryUI"/>
    /// row per skill available to the player's current class, refreshing
    /// whenever a skill is learned or upgraded elsewhere.
    /// </summary>
    public class SkillBookWindowUI : MonoBehaviour
    {
        [SerializeField] private SkillDefinition[] allSkills;
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private SkillBookEntryUI entryPrefab;
        [SerializeField] private Transform contentRoot;

        private void Start()
        {
            Populate();
            skillBook.SkillLeveledUp += OnSkillLeveledUp;
        }

        private void OnDestroy()
        {
            skillBook.SkillLeveledUp -= OnSkillLeveledUp;
        }

        private void Populate()
        {
            foreach (var skill in allSkills)
            {
                var restrictedToOtherClass = skill.AllowedClasses.Count > 0
                    && !skill.AllowedClasses.Contains(classController.CurrentClass);

                if (restrictedToOtherClass)
                {
                    continue;
                }

                var entry = Instantiate(entryPrefab, contentRoot);
                entry.Setup(skill, skillBook);
            }
        }

        private void OnSkillLeveledUp(SkillDefinition skill, int newLevel)
        {
            foreach (Transform child in contentRoot)
            {
                var entry = child.GetComponent<SkillBookEntryUI>();

                if (entry != null && entry.Skill == skill)
                {
                    entry.Refresh();
                }
            }
        }
    }
}