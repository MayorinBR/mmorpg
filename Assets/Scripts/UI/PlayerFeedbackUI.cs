using UnityEngine;
using TMPro;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Displays short-lived feedback messages published via
    /// <see cref="PlayerFeedbackChannel"/> (e.g. "inventory full") in a
    /// fixed screen location, hiding again after
    /// <see cref="displaySeconds"/>. A newly published message replaces
    /// whatever is currently shown and restarts the timer rather than
    /// queuing behind it.
    /// </summary>
    public class PlayerFeedbackUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float displaySeconds = 2.5f;

        private float hideAtTime;

        private void Awake()
        {
            root.SetActive(false);
        }

        private void OnEnable()
        {
            PlayerFeedbackChannel.MessagePublished += ShowMessage;
        }

        private void OnDisable()
        {
            PlayerFeedbackChannel.MessagePublished -= ShowMessage;
        }

        private void Update()
        {
            if (root.activeSelf && Time.time >= hideAtTime)
            {
                root.SetActive(false);
            }
        }

        private void ShowMessage(string message)
        {
            messageText.text = message;
            root.SetActive(true);
            hideAtTime = Time.time + displaySeconds;
        }
    }
}
