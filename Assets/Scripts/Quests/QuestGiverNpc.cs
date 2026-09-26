using UnityEngine;

namespace Project.Quests
{
    /// <summary>
    /// Marks an NPC as offering a quest, read by
    /// <c>Project.UI.DialogueWindowUI</c> when the player picks an
    /// <see cref="Project.NPC.NpcDialogueOptionAction.AcceptQuest"/> option
    /// from that NPC's dialogue. Kept as a separate component — rather than
    /// a field on <see cref="Project.NPC.NpcDialogueController"/> — so the
    /// NPC dialogue system itself never needs to depend on Project.Quests.
    /// </summary>
    public class QuestGiverNpc : MonoBehaviour
    {
        [SerializeField] private QuestDefinition offeredQuest;

        /// <summary>Gets the quest this NPC offers.</summary>
        public QuestDefinition OfferedQuest => offeredQuest;
    }
}
