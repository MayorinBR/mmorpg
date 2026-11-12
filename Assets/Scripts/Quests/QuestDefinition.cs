using System.Collections.Generic;
using UnityEngine;
using Project.Items;
using Project.NPC;

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
        [SerializeField] private ItemDefinition grantItemOnAccept;
        [SerializeField] private int grantItemOnAcceptQuantity = 1;

        [Tooltip("Optional. The NPC dialogue the player must open to turn this quest in once its objectives are done. Left empty, the quest completes and pays its reward the moment the objectives are met.")]
        [SerializeField] private NpcDialogueDefinition turnInDialogue;

        [Tooltip("Name of the NPC the quest log tells the player to report to. Only used when Turn In Dialogue is set.")]
        [SerializeField] private string turnInNpcName;

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

        /// <summary>
        /// Gets the item handed to the player immediately on accepting this
        /// quest (e.g. a courier's key item), or null for none. Unlike
        /// <see cref="RewardItem"/>, this is granted the moment the quest
        /// starts rather than on completion, and only if the player's
        /// inventory has room for it — see <see cref="QuestManager.AcceptQuest"/>.
        /// </summary>
        public ItemDefinition GrantItemOnAccept => grantItemOnAccept;

        /// <summary>Gets the quantity of <see cref="GrantItemOnAccept"/> granted, when it isn't null.</summary>
        public int GrantItemOnAcceptQuantity => grantItemOnAcceptQuantity;

        /// <summary>
        /// Gets the dialogue of the NPC this quest must be turned in to, or null
        /// when the quest completes as soon as its objectives are met.
        /// </summary>
        public NpcDialogueDefinition TurnInDialogue => turnInDialogue;

        /// <summary>Gets the display name of the turn-in NPC, with a generic fallback when none is authored.</summary>
        public string TurnInNpcName => string.IsNullOrWhiteSpace(turnInNpcName) ? "the quest giver" : turnInNpcName;
    }
}
