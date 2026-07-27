using System.Collections;
using UnityEngine;
using TMPro;
using Project.Character.Combat;
using Project.Character.Movement;
using Project.Combat;

namespace Project.Character
{
    /// <summary>
    /// Reacts to the player's death by disabling movement/combat/input,
    /// showing a game over screen with a respawn countdown, then reviving
    /// the player at <see cref="respawnPoint"/> with full health.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerCombatController combatController;
        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float respawnDelaySeconds = 5f;
        [SerializeField] private GameObject deathScreenRoot;
        [SerializeField] private TMP_Text countdownText;

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void HandleDeath()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            SetControlsEnabled(false);
            deathScreenRoot.SetActive(true);

            var remainingSeconds = respawnDelaySeconds;

            while (remainingSeconds > 0f)
            {
                UpdateCountdownText(remainingSeconds);
                yield return null;
                remainingSeconds -= Time.deltaTime;
            }

            movementController.WarpTo(respawnPoint.position);
            health.ResetHealth();
            mana.ResetMana();
            deathScreenRoot.SetActive(false);
            SetControlsEnabled(true);
        }

        private void SetControlsEnabled(bool isEnabled)
        {
            movementController.enabled = isEnabled;
            combatController.enabled = isEnabled;
            inputRouter.enabled = isEnabled;
        }

        private void UpdateCountdownText(float remainingSeconds)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(remainingSeconds).ToString();
            }
        }
    }
}