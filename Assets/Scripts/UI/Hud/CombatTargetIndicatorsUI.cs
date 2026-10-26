using UnityEngine;
using Project.Character.Combat;
using Project.Character.Movement;

namespace Project.UI
{
    /// <summary>
    /// Drives the ground rings of enemies: a white ring on the enemy under
    /// the cursor (see <see cref="PlayerInputRouter.HoveredEnemyChanged"/>),
    /// which also covers the skill target picker, and a red ring on the
    /// player's current combat target (see <see cref="PlayerTargetSelector"/>).
    /// The rings themselves belong to each enemy's <see cref="TargetRingMarker"/>,
    /// so nothing here depends on the current map.
    /// </summary>
    public class CombatTargetIndicatorsUI : MonoBehaviour
    {
        [SerializeField] private PlayerTargetSelector targetSelector;

        private PlayerInputRouter inputRouter;
        private TargetRingMarker hoveredMarker;
        private TargetRingMarker targetedMarker;
        private SkillTargetingController skillTargeting;
        private TargetRingMarker skillMarker;

        private void OnEnable()
        {
            SubscribeSkillTargeting();
            targetSelector.TargetChanged += HandleTargetChanged;

            if (inputRouter != null)
            {
                inputRouter.HoveredEnemyChanged += HandleHoveredEnemyChanged;
            }
        }

        private void Start()
        {
            SubscribeSkillTargeting();
            inputRouter = FindFirstObjectByType<PlayerInputRouter>();

            if (inputRouter != null)
            {
                inputRouter.HoveredEnemyChanged += HandleHoveredEnemyChanged;
            }
        }

        private void OnDisable()
        {
            UnsubscribeSkillTargeting();
            targetSelector.TargetChanged -= HandleTargetChanged;

            if (inputRouter != null)
            {
                inputRouter.HoveredEnemyChanged -= HandleHoveredEnemyChanged;
            }
        }

        private void SubscribeSkillTargeting()
        {
            UnsubscribeSkillTargeting();
            skillTargeting = SkillTargetingController.Instance;

            if (skillTargeting != null)
            {
                skillTargeting.HoveredEnemyChanged += HandleSkillHoveredChanged;
            }
        }

        private void UnsubscribeSkillTargeting()
        {
            if (skillTargeting != null)
            {
                skillTargeting.HoveredEnemyChanged -= HandleSkillHoveredChanged;
                skillTargeting = null;
            }

            if (skillMarker != null)
            {
                skillMarker.SetSkillPicked(false);
                skillMarker = null;
            }
        }

        private void HandleSkillHoveredChanged(Transform hoveredEnemy)
        {
            if (skillMarker != null)
            {
                skillMarker.SetSkillPicked(false);
            }

            skillMarker = FindMarker(hoveredEnemy);

            if (skillMarker != null)
            {
                skillMarker.SetSkillPicked(true);
            }
        }

        private void HandleTargetChanged(Transform newTarget)
        {
            if (targetedMarker != null)
            {
                targetedMarker.SetTargeted(false);
            }

            targetedMarker = FindMarker(newTarget);

            if (targetedMarker != null)
            {
                targetedMarker.SetTargeted(true);
            }
        }

        private void HandleHoveredEnemyChanged(Transform hoveredEnemy)
        {
            if (hoveredMarker != null)
            {
                hoveredMarker.SetHovered(false);
            }

            hoveredMarker = FindMarker(hoveredEnemy);

            if (hoveredMarker != null)
            {
                hoveredMarker.SetHovered(true);
            }
        }

        private static TargetRingMarker FindMarker(Transform enemy)
        {
            return enemy != null ? enemy.GetComponentInChildren<TargetRingMarker>(true) : null;
        }
    }
}
