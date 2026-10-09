using System;
using Project.Character.Animation;
using Project.Character.Movement;
using Project.Combat;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks whether the player is in combat. The player enters combat when a target is selected,
    /// when an attack or spell cast is triggered, or when something attacks the player, and leaves it
    /// <see cref="lingerSeconds"/> after the last of those happened.
    /// </summary>
    public sealed class PlayerCombatState : MonoBehaviour
    {
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private PlayerAnimatorController animatorController;
        [SerializeField] private HealthComponent health;

        [Tooltip("Seconds the player stays in combat after the last attack, cast, hit taken or target selection.")]
        [SerializeField, Min(0f)] private float lingerSeconds = 5f;

        private float combatEndsAt = float.NegativeInfinity;

        /// <summary>Raised when the player enters (true) or leaves (false) combat.</summary>
        public event Action<bool> Changed;

        /// <summary>Gets whether the player is currently in combat.</summary>
        public bool IsInCombat { get; private set; }

        private void OnEnable()
        {
            if (animatorController != null)
            {
                animatorController.CombatActionTriggered += Refresh;
            }

            if (health != null)
            {
                health.AttackedBy += HandleAttackedBy;
            }
        }

        private void OnDisable()
        {
            if (animatorController != null)
            {
                animatorController.CombatActionTriggered -= Refresh;
            }

            if (health != null)
            {
                health.AttackedBy -= HandleAttackedBy;
            }
        }

        private void Update()
        {
            if (targetSelector != null && targetSelector.CurrentTarget != null)
            {
                combatEndsAt = Time.time + lingerSeconds;
            }

            SetInCombat(Time.time < combatEndsAt);
        }

        private void HandleAttackedBy(Transform attacker)
        {
            Refresh();
        }

        private void Refresh()
        {
            combatEndsAt = Time.time + lingerSeconds;
            SetInCombat(true);
        }

        private void SetInCombat(bool value)
        {
            if (IsInCombat == value)
            {
                return;
            }

            IsInCombat = value;
            Changed?.Invoke(value);
        }
    }
}
