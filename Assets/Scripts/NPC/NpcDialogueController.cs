using UnityEngine;

namespace Project.NPC
{
    /// <summary>
    /// The generic NPC interaction entry point: what
    /// <see cref="Project.Character.Movement.PlayerNpcInteractionController"/>
    /// walks the player up to and opens on click, for any NPC. Every
    /// interactable NPC gets one of these; <see cref="ShopKeeper"/> is left
    /// empty for an NPC that only talks, or wired for a merchant whose
    /// dialogue includes an "open shop" option. Replaces the previous
    /// hardcoded shop-only entry point (walking up to an NPC used to open
    /// its shop directly) with a branching dialogue box instead, per
    /// <see cref="NpcDialogueDefinition"/>.
    /// </summary>
    public class NpcDialogueController : MonoBehaviour
    {
        [SerializeField] private NpcDialogueDefinition dialogue;

        [Tooltip("Optional. Wired when this NPC also sells items, so a dialogue option can open its shop. Left empty for an NPC with no shop.")]
        [SerializeField] private NpcShopKeeper shopKeeper;

        [SerializeField] private NpcAnimationController animationController;

        /// <summary>Gets this NPC's dialogue line and response options.</summary>
        public NpcDialogueDefinition Dialogue => dialogue;

        /// <summary>Gets this NPC's shop, or null if it has none.</summary>
        public NpcShopKeeper ShopKeeper => shopKeeper;

        /// <summary>Gets the controller for this NPC's Idle/Interact animations, if assigned.</summary>
        public NpcAnimationController AnimationController => animationController;
    }
}
