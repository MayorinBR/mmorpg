using System.Collections.Generic;
using UnityEngine;
using Project.Items;

namespace Project.Quests
{
    /// <summary>
    /// Authored data for one quest: its display text, the list of
    /// requirements that must all be satisfied to complete it, and the
    /// reward granted on completion. A quest with more than one requirement
    /// completes only once every one of them is met, so a single asset can
    /// represent both a simple one-objective quest and a compound one.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Project/Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private QuestRequirement[] requirements;
        [SerializeField] private int rewardExperience;
        [SerializeField] private int rewardJobExperience;
        [SerializeField] private ItemDefinition rewardItem;
        [SerializeField] private int rewardItemQuantity = 1;

        /// <summary>Gets the quest's display title, shown in the quest log and dialogue.</summary>
        public string Title => title;

        /// <summary>Gets the quest's flavor/objective description.</summary>
        public string Description => description;

        /// <summary>Gets every requirement that must be satisfied to complete this quest.</summary>
        public IReadOnlyList<QuestRequirement> Requirements => requirements;

        /// <summary>Gets the Base Experience granted on completion.</summary>
        public int RewardExperience => rewardExperience;

        /// <summary>Gets the Job Experience granted on completion.</summary>
        public int RewardJobExperience => rewardJobExperience;

        /// <summary>Gets the item granted on completion, or null for no item reward.</summary>
        public ItemDefinition RewardItem => rewardItem;

        /// <summary>Gets the quantity of <see cref="RewardItem"/> granted, when it isn't null.</summary>
        public int RewardItemQuantity => rewardItemQuantity;
    }
}
