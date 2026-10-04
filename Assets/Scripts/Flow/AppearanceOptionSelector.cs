using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Flow
{
    /// <summary>
    /// One row of the appearance panel: a previous button, a next button and an optional label that
    /// shows the current entry. The selection wraps around at both ends. A row configured with an
    /// empty list shows a dash and disables its buttons, which is how the rows for the future head
    /// and body models are kept visible but inactive.
    /// </summary>
    public sealed class AppearanceOptionSelector : MonoBehaviour
    {
        private const string EmptyLabel = "-";

        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        [Tooltip("Optional. Shows the name of the current entry.")]
        [SerializeField] private TMP_Text label;

        private IReadOnlyList<string> labels = Array.Empty<string>();
        private Action<int> onChanged;
        private bool listenersAdded;

        /// <summary>Gets the index of the current entry, or -1 when the list is empty.</summary>
        public int Index { get; private set; } = -1;

        /// <summary>
        /// Sets the entries to choose from and selects one.
        /// </summary>
        /// <param name="entries">Display names of the entries. An empty list disables the row.</param>
        /// <param name="selectedIndex">Entry to select. Values outside the list select the first entry.</param>
        /// <param name="selectionChanged">Called with the new index each time the player changes the selection.</param>
        public void Configure(IReadOnlyList<string> entries, int selectedIndex, Action<int> selectionChanged)
        {
            AddListeners();
            labels = entries ?? Array.Empty<string>();
            onChanged = selectionChanged;
            Index = labels.Count == 0 ? -1 : (selectedIndex >= 0 && selectedIndex < labels.Count ? selectedIndex : 0);

            bool usable = labels.Count > 0;
            previousButton.interactable = usable;
            nextButton.interactable = usable;
            Refresh();
        }

        private void AddListeners()
        {
            if (listenersAdded)
            {
                return;
            }

            listenersAdded = true;
            previousButton.onClick.AddListener(() => Step(-1));
            nextButton.onClick.AddListener(() => Step(1));
        }

        private void Step(int direction)
        {
            if (labels.Count == 0)
            {
                return;
            }

            Index = (Index + direction + labels.Count) % labels.Count;
            Refresh();
            onChanged?.Invoke(Index);
        }

        private void Refresh()
        {
            if (label != null)
            {
                label.text = Index >= 0 ? labels[Index] : EmptyLabel;
            }
        }
    }
}
