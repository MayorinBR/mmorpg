using UnityEngine;
using Project.Character.Combat;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Spawns a floating <see cref="DamagePopup"/> above this character
    /// every time its <see cref="HealthComponent"/> reports damage taken or
    /// a dodged attack. Attach alongside a <see cref="HealthComponent"/> on
    /// the player and on each enemy — <see cref="health"/> resolves itself
    /// via <see cref="GetComponent{T}"/> when left unassigned, so adding
    /// this component is the only setup step needed.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private float horizontalJitter = 0.3f;

        [Tooltip("Optional. Resolved from the owning hierarchy when empty; only the player has one, and it adds a green popup for passive skill healing.")]
        [SerializeField] private PlayerHealthRegenController regenController;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            if (regenController == null)
            {
                regenController = transform.root.GetComponentInChildren<PlayerHealthRegenController>(true);
            }
        }

        private void OnEnable()
        {
            health.DamageTaken += HandleDamageTaken;
            health.Dodged += HandleDodged;

            if (regenController != null)
            {
                regenController.SkillRegenApplied += HandleSkillRegenApplied;
            }
        }

        private void OnDisable()
        {
            health.DamageTaken -= HandleDamageTaken;
            health.Dodged -= HandleDodged;

            if (regenController != null)
            {
                regenController.SkillRegenApplied -= HandleSkillRegenApplied;
            }
        }

        private void HandleSkillRegenApplied(int amount)
        {
            DamagePopup.CreateHeal(amount, GetSpawnPosition());
        }

        private void HandleDamageTaken(int amount, bool isCritical, Element element)
        {
            DamagePopup.Create(amount, GetSpawnPosition(), element, isCritical);
        }

        private void HandleDodged()
        {
            DamagePopup.CreateDodge(GetSpawnPosition());
        }

        private Vector3 GetSpawnPosition()
        {
            var jitter = new Vector3(Random.Range(-horizontalJitter, horizontalJitter), 0f, 0f);
            return transform.position + spawnOffset + jitter;
        }
    }
}
