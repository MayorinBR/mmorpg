using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Receives Animation Events fired from the player's currently active
    /// model. Unity calls an Animation Event's function on whatever
    /// GameObject owns the <see cref="Animator"/>, and since that
    /// GameObject (the gendered model clone) is destroyed and recreated on
    /// every gender swap (see <see cref="PlayerGenderController.SetGender"/>),
    /// there's no single GameObject to wire this on once in the Editor —
    /// <see cref="PlayerGenderController"/> adds one of these to the freshly
    /// instantiated Animator's GameObject instead, right after it resolves
    /// that Animator, and points it at <see cref="PlayerCombatController"/>.
    /// </summary>
    public class PlayerAnimationEventRelay : MonoBehaviour
    {
        private PlayerCombatController combatController;

        /// <summary>Wires the controller this relay forwards Animation Events to.</summary>
        /// <param name="combatController">The player's combat controller.</param>
        public void Initialize(PlayerCombatController combatController)
        {
            this.combatController = combatController;
        }

        /// <summary>
        /// Call this from an Animation Event on the Archer's AttackRanged
        /// clip or the Mage's Cast clip, at the frame a ranged basic
        /// attack's projectile should leave the hand. Forwards to
        /// <see cref="PlayerCombatController.ReleaseProjectile"/>.
        /// </summary>
        public void ReleaseProjectile()
        {
            combatController?.ReleaseProjectile();
        }
    }
}
