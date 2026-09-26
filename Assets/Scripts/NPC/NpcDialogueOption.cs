using UnityEngine;

namespace Project.NPC
{
    /// <summary>
    /// What clicking a dialogue response option does. <see cref="Close"/> is
    /// what any option ends on unless it names a specific follow-up action —
    /// there is no multi-page dialogue tree today, only one line and a flat
    /// list of responses (see <see cref="NpcDialogueDefinition"/>), so an
    /// option's job is either to hand off to another system or to end the
    /// conversation. <see cref="AcceptQuest"/> hands off to whichever quest
    /// a <c>Project.Quests.QuestGiverNpc</c> on the same NPC offers — kept
    /// as a plain enum case rather than a field here so this assembly never
    /// needs to depend on the quest system.
    /// </summary>
    public enum NpcDialogueOptionAction
    {
        Close,
        OpenShop,
        AcceptQuest
    }

    /// <summary>
    /// A single clickable response in an <see cref="NpcDialogueDefinition"/>,
    /// e.g. "Buy/Sell" ⇒ <see cref="NpcDialogueOptionAction.OpenShop"/>, or
    /// "Never mind" ⇒ <see cref="NpcDialogueOptionAction.Close"/>.
    /// </summary>
    [System.Serializable]
    public struct NpcDialogueOption
    {
        [SerializeField] private string optionText;
        [SerializeField] private NpcDialogueOptionAction action;

        /// <summary>Gets the text shown on this option's button.</summary>
        public string OptionText => optionText;

        /// <summary>Gets what happens when this option is clicked.</summary>
        public NpcDialogueOptionAction Action => action;
    }
}
