using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Flow
{
    /// <summary>
    /// A modal popup with a message and either a Yes/No pair (a question) or
    /// a single OK button (a notice). Put it on the full-screen overlay
    /// object that contains the buttons, so the overlay also blocks clicks
    /// on the screen behind it. Starts hidden; callers only deal with
    /// callbacks, never with the buttons.
    /// </summary>
    public class ConfirmationPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private Button okButton;

        private Action onYes;
        private Action onNo;
        private Action onOk;

        private void Awake()
        {
            yesButton.onClick.AddListener(() => Close(onYes));
            noButton.onClick.AddListener(() => Close(onNo));
            okButton.onClick.AddListener(() => Close(onOk));
        }

        /// <summary>
        /// Shows a question with Yes and No buttons.
        /// </summary>
        /// <param name="message">The question to display.</param>
        /// <param name="onConfirm">Invoked, after the popup closes, when the player picks Yes.</param>
        /// <param name="onCancel">Invoked, after the popup closes, when the player picks No. Optional.</param>
        public void ShowConfirm(string message, Action onConfirm, Action onCancel = null)
        {
            Open(message, showYesNo: true);
            onYes = onConfirm;
            onNo = onCancel;
            onOk = null;
        }

        /// <summary>
        /// Shows a notice with a single OK button.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="onClose">Invoked, after the popup closes, when the player picks OK. Optional.</param>
        public void ShowMessage(string message, Action onClose = null)
        {
            Open(message, showYesNo: false);
            onYes = null;
            onNo = null;
            onOk = onClose;
        }

        /// <summary>Hides the popup without invoking any callback.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Open(string message, bool showYesNo)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            yesButton.gameObject.SetActive(showYesNo);
            noButton.gameObject.SetActive(showYesNo);
            okButton.gameObject.SetActive(!showYesNo);
        }

        private void Close(Action callback)
        {
            Hide();
            callback?.Invoke();
        }
    }
}
