using UnityEngine;

namespace Project.Quests
{
    /// <summary>
    /// Resolves a stable string id to a <see cref="QuestDefinition"/> asset
    /// and back, using the asset's own name. Exists so the save system
    /// (which cannot serialize a direct ScriptableObject reference through
    /// JSON) can record and later look up "which quest" without depending
    /// on any particular UI component's own list of known quests. Mirrors
    /// <c>Project.Items.ItemDatabase</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Project/Quests/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        [SerializeField] private QuestDefinition[] allQuests;

        /// <summary>Gets the stable id for a quest, currently its asset name.</summary>
        /// <param name="quest">The quest to get an id for.</param>
        /// <returns>The quest's id, or an empty string if <paramref name="quest"/> is null.</returns>
        public string GetId(QuestDefinition quest)
        {
            return quest != null ? quest.name : string.Empty;
        }

        /// <summary>Finds the quest asset with the given id.</summary>
        /// <param name="id">The id to look up, as returned by <see cref="GetId"/>.</param>
        /// <returns>The matching quest, or null if not found or <paramref name="id"/> is empty.</returns>
        public QuestDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (var quest in allQuests)
            {
                if (quest != null && quest.name == id)
                {
                    return quest;
                }
            }

            return null;
        }
    }
}
