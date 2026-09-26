using System.Collections.Generic;
using UnityEngine;

namespace Project.Quests
{
    /// <summary>
    /// Marks an interactable — an NPC, or a static prop like a bulletin
    /// board — as posting several quests at once, read by
    /// <c>Project.UI.QuestBoardWindowUI</c> when the player picks an
    /// <see cref="Project.NPC.NpcDialogueOptionAction.OpenQuestBoard"/>
    /// option from its dialogue. Kept separate from <see cref="QuestGiverNpc"/>
    /// (which offers exactly one quest directly through a dialogue option)
    /// since a board's whole point is picking from a list rather than a
    /// single accept prompt.
    /// </summary>
    public class QuestBoardNpc : MonoBehaviour
    {
        [SerializeField] private QuestDefinition[] offeredQuests;

        /// <summary>Gets every quest posted on this board.</summary>
        public IReadOnlyList<QuestDefinition> OfferedQuests => offeredQuests;
    }
}
