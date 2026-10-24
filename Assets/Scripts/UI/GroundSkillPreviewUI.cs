using UnityEngine;
using Project.Character.Combat;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Shows a yellow disc under the cursor while the player is choosing
    /// where to place a ground skill, sized to the skill's area radius.
    /// </summary>
    public class GroundSkillPreviewUI : MonoBehaviour
    {
        private const float HeightAboveGround = 0.06f;

        [Tooltip("Visual instantiated as the placement preview (the yellow area prefab).")]
        [SerializeField] private SkillAreaVisual areaPrefab;

        private SkillTargetingController targeting;
        private SkillAreaVisual instance;

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Hide();
        }

        private void OnDestroy()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }

        private void Subscribe()
        {
            Unsubscribe();
            targeting = SkillTargetingController.Instance;

            if (targeting != null)
            {
                targeting.HoveredGroundPointChanged += HandleGroundPointChanged;
            }
        }

        private void Unsubscribe()
        {
            if (targeting != null)
            {
                targeting.HoveredGroundPointChanged -= HandleGroundPointChanged;
                targeting = null;
            }
        }

        private void HandleGroundPointChanged(Vector3? point)
        {
            var skill = targeting != null ? targeting.PendingSkill : null;

            if (!point.HasValue || skill == null || areaPrefab == null)
            {
                Hide();
                return;
            }

            if (instance == null)
            {
                instance = Instantiate(areaPrefab);
            }

            instance.SetRadius(skill.AreaRadius);
            instance.transform.position = point.Value + Vector3.up * HeightAboveGround;
            instance.gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
            }
        }
    }
}
