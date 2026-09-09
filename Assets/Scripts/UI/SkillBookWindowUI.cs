using Project.Character.Combat;
using Project.Character.Stats;
using Project.Skills;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Populates the Skill Book window with one <see cref="SkillBookEntryUI"/>
    /// row per skill in the current class's <see cref="SkillDatabase"/>,
    /// resolved through <see cref="ClassSkillDatabaseLookup"/> so each
    /// class's skill list lives only in its own database asset. Refreshes
    /// whenever a skill is learned/upgraded or the player's class changes,
    /// so a newly authored skill on a class's database shows up here
    /// automatically and the window always reflects whichever class is
    /// currently chosen — no Inspector change on this component required
    /// for either case.
    /// </summary>
    public class SkillBookWindowUI : MonoBehaviour
    {
        [SerializeField] private ClassSkillDatabaseLookup skillDatabaseLookup;
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerExperience experience;
        [SerializeField] private SkillBookEntryUI entryPrefab;
        [SerializeField] private Transform contentRoot;

        private void Start()
        {
            Populate();
            skillBook.SkillLeveledUp += OnSkillLeveledUp;
            classController.ClassChanged += OnClassChanged;
        }

        private void OnDestroy()
        {
            skillBook.SkillLeveledUp -= OnSkillLeveledUp;
            classController.ClassChanged -= OnClassChanged;
        }

        private void Populate()
        {
            var database = skillDatabaseLookup.GetDatabase(classController.CurrentClass);

            if (database == null)
            {
                return;
            }

            foreach (var skill in database.AllSkills)
            {
                var entry = Instantiate(entryPrefab, contentRoot);
                entry.Setup(skill, skillBook);
            }
        }

        private void OnSkillLeveledUp(SkillDefinition skill, int newLevel)
        {
            RefreshAllEntries();
        }

        private void OnClassChanged(CharacterClass newClass)
        {
            ClearEntries();
            Populate();
        }

        private void ClearEntries()
        {
            for (var i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }
        }

        private void RefreshAllEntries()
        {
            foreach (Transform child in contentRoot)
            {
                var entry = child.GetComponent<SkillBookEntryUI>();

                if (entry != null)
                {
                    entry.Refresh();
                }
            }
        }
    }
}
