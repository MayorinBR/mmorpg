using UnityEngine;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// Populates the Quest Log window with one <see cref="QuestLogEntryUI"/>
    /// row per active quest, mirroring <see cref="SkillBookWindowUI"/>'s
    /// clear-and-repopulate approach on every <see cref="QuestManager.QuestsChanged"/>
    /// — simpler than diffing, and cheap enough at the handful of quests a
    /// player tracks at once.
    /// </summary>
    public class QuestLogWindowUI : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private Transform contentRoot;

        private void Start()
        {
            Rebuild();
            questManager.QuestsChanged += Rebuild;
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.QuestsChanged -= Rebuild;
            }
        }

        private void Rebuild()
        {
            for (var i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }

            foreach (var active in questManager.ActiveQuests)
            {
                QuestLogEntryUI.Create(contentRoot, active);
            }
        }
    }
}
