using UnityEngine;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// Passively restores HP and mana at a flat rate while a boss has no
    /// current target (see <see cref="EnemyController.PlayerTarget"/>), so
    /// it's back to full readiness for the next encounter even if a
    /// previous fight was abandoned or lost. Not wired to ordinary
    /// enemies — regular mobs have no such regeneration.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class BossPassiveRegenController : MonoBehaviour
    {
        [SerializeField] private float healthPerSecond = 100f;
        [SerializeField] private float manaPerSecond = 30f;
        [SerializeField] private HealthComponent health;
        [SerializeField] private ManaComponent mana;

        private EnemyController enemy;
        private float healthAccumulator;
        private float manaAccumulator;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
        }

        private void Update()
        {
            if (enemy.PlayerTarget != null)
            {
                healthAccumulator = 0f;
                manaAccumulator = 0f;
                return;
            }

            RegenHealth();
            RegenMana();
        }

        private void RegenHealth()
        {
            if (health == null || health.IsDead)
            {
                return;
            }

            healthAccumulator += healthPerSecond * Time.deltaTime;
            var wholeAmount = Mathf.FloorToInt(healthAccumulator);

            if (wholeAmount <= 0)
            {
                return;
            }

            healthAccumulator -= wholeAmount;
            health.Heal(wholeAmount);
        }

        private void RegenMana()
        {
            if (mana == null)
            {
                return;
            }

            manaAccumulator += manaPerSecond * Time.deltaTime;
            var wholeAmount = Mathf.FloorToInt(manaAccumulator);

            if (wholeAmount <= 0)
            {
                return;
            }

            manaAccumulator -= wholeAmount;
            mana.RestoreMana(wholeAmount);
        }
    }
}
