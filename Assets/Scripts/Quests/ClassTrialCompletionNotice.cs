using UnityEngine;
using Project.Combat;

namespace Project.Quests
{
    /// <summary>
    /// Tells the player to return to the Trial Master as soon as a class trial
    /// quest completes. It does not promote anyone: the class is only granted when
    /// the player turns the trial in at the NPC. Sits on the persistent player
    /// object so the message appears in any map.
    /// </summary>
    public class ClassTrialCompletionNotice : MonoBehaviour
    {
        [SerializeField] private ClassTrialCatalog catalog;
        [SerializeField] private QuestManager questManager;

        private void OnEnable()
        {
            if (questManager != null)
            {
                questManager.QuestCompleted += HandleQuestCompleted;
            }
        }

        private void OnDisable()
        {
            if (questManager != null)
            {
                questManager.QuestCompleted -= HandleQuestCompleted;
            }
        }

        /// <summary>
        /// Publishes the turn-in reminder when the completed quest is a class trial.
        /// </summary>
        /// <param name="quest">The quest that just completed.</param>
        private void HandleQuestCompleted(QuestDefinition quest)
        {
            if (catalog != null && catalog.TryGetTargetClass(quest, out _))
            {
                PlayerFeedbackChannel.Publish("Trial complete! Return to the Trial Master to turn it in.");
            }
        }
    }
}
