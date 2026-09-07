using System.Collections;
using UnityEngine;

namespace Project.AI
{
    /// <summary>
    /// Shows a brief overhead icon whenever this enemy engages a player —
    /// on fresh detection, passive retaliation, or memory-based re-aggro —
    /// mirroring Ragnarok Online's "!" aggro cue. Purely cosmetic: it
    /// reacts to <see cref="EnemyController.PlayerEngaged"/> and has no
    /// influence on the state machine. Kept in <c>Project.AI</c> rather
    /// than <c>Project.UI</c> so it can reference <see cref="EnemyController"/>
    /// directly without introducing an assembly cycle (Project.AI already
    /// depends on Project.World, which depends on Project.UI).
    /// </summary>
    public class EnemyAggroIndicator : MonoBehaviour
    {
        [SerializeField] private EnemyController controller;

        [Tooltip("The icon to toggle. Expected to already be positioned and billboarded — e.g. as a child of the enemy's existing world-space health bar canvas, which already follows and faces the camera.")]
        [SerializeField] private GameObject indicatorRoot;

        [SerializeField] private float displaySeconds = 1.5f;

        private Coroutine hideRoutine;

        private void OnEnable()
        {
            controller.PlayerEngaged += HandlePlayerEngaged;
        }

        private void OnDisable()
        {
            controller.PlayerEngaged -= HandlePlayerEngaged;
        }

        private void HandlePlayerEngaged()
        {
            indicatorRoot.SetActive(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displaySeconds);
            indicatorRoot.SetActive(false);
            hideRoutine = null;
        }
    }
}
