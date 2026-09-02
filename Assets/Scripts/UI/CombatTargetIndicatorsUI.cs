using UnityEngine;
using Project.Character.Combat;
using Project.Character.Movement;

namespace Project.UI
{
    /// <summary>
    /// Drives the two ground-ring visual indicators used for combat
    /// targeting: a ring under whatever enemy a pending skill's target
    /// picker is currently hovering (see <see cref="SkillTargetingController"/>),
    /// and a separate ring under the player's current combat target (see
    /// <see cref="PlayerTargetSelector"/>). Kept separate from either of
    /// those so neither has to know about rendering.
    /// </summary>
    public class CombatTargetIndicatorsUI : MonoBehaviour
    {
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private GroundRingFollower currentTargetRing;
        [SerializeField] private GroundRingFollower skillPickerRing;

        private void OnEnable()
        {
            targetSelector.TargetChanged += HandleTargetChanged;
        }

        private void OnDisable()
        {
            targetSelector.TargetChanged -= HandleTargetChanged;

            if (SkillTargetingController.Instance != null)
            {
                SkillTargetingController.Instance.HoveredEnemyChanged -= HandleHoveredEnemyChanged;
            }
        }

        private void Start()
        {
            // Subscribed here rather than OnEnable: SkillTargetingController.Instance
            // is only guaranteed to be set once every object's Awake has run,
            // which Unity guarantees has happened by the time any Start runs.
            if (SkillTargetingController.Instance != null)
            {
                SkillTargetingController.Instance.HoveredEnemyChanged += HandleHoveredEnemyChanged;
            }
        }

        private void HandleTargetChanged(Transform newTarget)
        {
            if (newTarget != null)
            {
                currentTargetRing.Show(newTarget);
            }
            else
            {
                currentTargetRing.Hide();
            }
        }

        private void HandleHoveredEnemyChanged(Transform hoveredEnemy)
        {
            if (hoveredEnemy != null)
            {
                skillPickerRing.Show(hoveredEnemy);
            }
            else
            {
                skillPickerRing.Hide();
            }
        }
    }
}
