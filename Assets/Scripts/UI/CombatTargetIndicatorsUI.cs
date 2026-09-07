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

        /// <summary>
        /// Retargets both ring followers to this map's local instances.
        /// Needed on the persisted player (see <see cref="Project.World.PersistentPlayerAnchor"/>):
        /// the ring GameObjects live in the map scene, not on the player, so
        /// they're destroyed on every <c>SceneManager.LoadScene</c> and the
        /// serialized references from the previous map would otherwise go
        /// stale, exactly like <see cref="WorldSpaceHealthBarFollower.SetViewCamera"/>.
        /// </summary>
        /// <param name="newCurrentTargetRing">This map's ring for the player's current combat target.</param>
        /// <param name="newSkillPickerRing">This map's ring for the skill target picker's hovered enemy.</param>
        public void SetRings(GroundRingFollower newCurrentTargetRing, GroundRingFollower newSkillPickerRing)
        {
            currentTargetRing = newCurrentTargetRing;
            skillPickerRing = newSkillPickerRing;
        }

        private void HandleTargetChanged(Transform newTarget)
        {
            // currentTargetRing can be a stale reference to a ring destroyed by a
            // scene load, in the brief window before SetRings() re-points it at
            // the new map's ring (see SetRings' doc comment above).
            if (currentTargetRing == null)
            {
                return;
            }

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
            if (skillPickerRing == null)
            {
                return;
            }

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
