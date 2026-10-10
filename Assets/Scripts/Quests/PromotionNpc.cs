using UnityEngine;
using Project.Character.Stats;

namespace Project.Quests
{
    /// <summary>
    /// Marks an NPC as a class promotion master, read by
    /// <c>Project.UI.DialogueWindowUI</c> when the player picks a
    /// <see cref="Project.NPC.NpcDialogueOptionAction.ClassPromotion"/> option.
    /// Holds only data: the one-time trial quest a Novice must finish and the
    /// classes this NPC can promote to. Today a single NPC offers all six
    /// classes for testing; later each class gets its own NPC with its own
    /// quest and a shorter <see cref="OfferedClasses"/> list.
    /// </summary>
    public class PromotionNpc : MonoBehaviour
    {
        [SerializeField] private QuestDefinition promotionQuest;

        [SerializeField] private CharacterClass[] offeredClasses =
        {
            CharacterClass.Swordman,
            CharacterClass.Archer,
            CharacterClass.Merchant,
            CharacterClass.Acolyte,
            CharacterClass.Thief,
            CharacterClass.Mage
        };

        /// <summary>Gets the trial quest a Novice must complete once before promoting.</summary>
        public QuestDefinition PromotionQuest => promotionQuest;

        /// <summary>Gets the classes this NPC can promote a Novice to.</summary>
        public CharacterClass[] OfferedClasses => offeredClasses;
    }
}
