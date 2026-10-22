using UnityEngine;

namespace Project.Quests
{
    /// <summary>
    /// Marks an NPC as the class trial master, read by
    /// <c>Project.UI.DialogueWindowUI</c> when the player picks a
    /// <see cref="Project.NPC.NpcDialogueOptionAction.ClassPromotion"/> option.
    /// Holds only data: the catalog of class trials the NPC offers. A Novice
    /// picks one trial at a time, completing it promotes the player, and
    /// afterwards the NPC only has generic dialogue.
    /// </summary>
    public class PromotionNpc : MonoBehaviour
    {
        [SerializeField] private ClassTrialCatalog catalog;

        /// <summary>Gets the class trials this NPC offers.</summary>
        public ClassTrialCatalog Catalog => catalog;
    }
}
