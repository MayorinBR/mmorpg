using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    /// <summary>
    /// A single clickable response row in the dialogue box, mirroring
    /// <see cref="ShopBuyEntryUI"/>'s purely-presentational shape: it just
    /// shows text and reports back that its button was clicked.
    /// </summary>
    public class DialogueOptionButtonUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        private Action onClick;

        /// <summary>
        /// Configures this row's text and invokes the given callback when
        /// its button is clicked.
        /// </summary>
        /// <param name="optionText">The text shown on this row.</param>
        /// <param name="clickCallback">Invoked when this row's button is clicked.</param>
        public void Setup(string optionText, Action clickCallback)
        {
            onClick = clickCallback;

            if (label != null)
            {
                label.text = optionText;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
