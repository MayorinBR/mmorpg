using UnityEngine;

namespace Project.Quests
{
    /// <summary>
    /// Shows an overhead icon above an NPC while it offers a quest the player
    /// has neither accepted nor completed, mirroring Ragnarok Online's "!"
    /// quest cue. Kept in <c>Project.Quests</c>, alongside the quest state it
    /// reads, rather than <c>Project.UI</c>, so it can reference
    /// <see cref="QuestGiverNpc"/> and <see cref="QuestManager"/> directly
    /// without introducing an assembly cycle.
    /// </summary>
    public class QuestGiverIndicator : MonoBehaviour
    {
        [SerializeField] private QuestGiverNpc npc;
        [SerializeField] private QuestManager questManager;

        [Tooltip("The icon to toggle. Expected to already be positioned and billboarded - e.g. as a child of the NPC's existing world-space name tag canvas, which already follows and faces the camera.")]
        [SerializeField] private GameObject indicatorRoot;

        private void OnEnable()
        {
            questManager.QuestsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            questManager.QuestsChanged -= Refresh;
        }

        private void Refresh()
        {
            var quest = npc.OfferedQuest;
            var hasAvailableQuest = quest != null && !questManager.IsActive(quest) && !questManager.IsCompleted(quest);
            indicatorRoot.SetActive(hasAvailableQuest);
        }
    }
}
