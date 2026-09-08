using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// A single floating combat-text instance: shows a damage amount over
    /// a background placeholder, rises and fades out over its lifetime,
    /// then destroys itself. Built entirely from code via <see cref="Create"/>
    /// instead of a prefab, so spawning one — for the player or for any
    /// enemy — needs no scene or asset wiring beyond
    /// <see cref="DamageNumberSpawner"/> itself.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private const float CanvasScale = 0.015f;
        private const float BackgroundWidth = 80f;
        private const float BackgroundHeight = 40f;
        private const float BaseFontSize = 24f;

        // A critical hit's feedback is the same popup, just bigger: both
        // the background and the text scale up by this factor so the
        // extra emphasis reads at a glance without a different layout.
        private const float CriticalSizeMultiplier = 1.2f;

        // Placeholder colors so the feedback is readable and a critical
        // hit is visually distinct before real art exists: black for a
        // normal hit, brown for a critical one. Swap these for real
        // sprites/colors later without touching the popup's rise/fade
        // behavior.
        private static readonly Color NormalBackgroundColor = Color.black;
        private static readonly Color CriticalBackgroundColor = new Color32(101, 67, 33, 255);

        // Placeholder colors for the dodge popup: a white background with
        // black text, distinct from the damage popups above. Swap for real
        // art later without touching the popup's rise/fade behavior.
        private static readonly Color DodgeBackgroundColor = Color.white;
        private static readonly Color DodgeTextColor = Color.black;

        [SerializeField] private float lifetimeSeconds = 1f;
        [SerializeField] private float riseSpeed = 1.5f;
        [SerializeField, Range(0f, 1f)] private float fadeStartFraction = 0.5f;

        private CanvasGroup canvasGroup;
        private Camera viewCamera;
        private float elapsed;

        /// <summary>
        /// Builds a new floating damage number at the given world position
        /// and starts its rise/fade lifecycle immediately. A critical hit
        /// gets a larger, differently colored background and larger text
        /// (see <see cref="CriticalSizeMultiplier"/>) so it stands out from
        /// a normal hit.
        /// </summary>
        /// <param name="amount">The damage amount to display.</param>
        /// <param name="worldPosition">Where to spawn the popup.</param>
        /// <param name="isCritical">Whether this popup represents a critical hit.</param>
        /// <returns>The spawned popup's component.</returns>
        public static DamagePopup Create(int amount, Vector3 worldPosition, bool isCritical = false)
        {
            var sizeMultiplier = isCritical ? CriticalSizeMultiplier : 1f;
            var backgroundColor = isCritical ? CriticalBackgroundColor : NormalBackgroundColor;

            return CreatePopup(amount.ToString(), worldPosition, backgroundColor, Color.white, sizeMultiplier);
        }

        /// <summary>
        /// Builds a new floating "dodge" popup at the given world position,
        /// for an attack that missed. Same rise/fade lifecycle as a damage
        /// popup, but with its own placeholder colors (see
        /// <see cref="DodgeBackgroundColor"/> and <see cref="DodgeTextColor"/>)
        /// and no critical-style size scaling.
        /// </summary>
        /// <param name="worldPosition">Where to spawn the popup.</param>
        /// <returns>The spawned popup's component.</returns>
        public static DamagePopup CreateDodge(Vector3 worldPosition)
        {
            return CreatePopup("dodge", worldPosition, DodgeBackgroundColor, DodgeTextColor, 1f);
        }

        private static DamagePopup CreatePopup(string text, Vector3 worldPosition, Color backgroundColor, Color textColor, float sizeMultiplier)
        {
            var root = new GameObject("DamagePopup", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.position = worldPosition;
            rootRect.sizeDelta = new Vector2(BackgroundWidth, BackgroundHeight) * sizeMultiplier;
            rootRect.localScale = Vector3.one * CanvasScale;

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>();
            var canvasGroup = root.AddComponent<CanvasGroup>();

            // Placeholder background so the text stays readable against any
            // environment. Swap this color/sprite for real art later
            // without touching the popup's rise/fade behavior.
            var background = new GameObject("Background", typeof(Image));
            background.transform.SetParent(root.transform, false);
            background.GetComponent<Image>().color = backgroundColor;
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;

            var textObject = new GameObject("AmountText", typeof(TextMeshProUGUI));
            textObject.transform.SetParent(root.transform, false);
            var textComponent = textObject.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = textColor;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.fontSize = BaseFontSize * sizeMultiplier;
            textComponent.alignment = TextAlignmentOptions.Center;
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var popup = root.AddComponent<DamagePopup>();
            popup.canvasGroup = canvasGroup;
            popup.viewCamera = Camera.main;

            return popup;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

            if (viewCamera != null)
            {
                transform.forward = viewCamera.transform.forward;
            }

            var fadeStartTime = lifetimeSeconds * fadeStartFraction;

            if (elapsed > fadeStartTime)
            {
                var fadeDuration = Mathf.Max(lifetimeSeconds - fadeStartTime, 0.0001f);
                canvasGroup.alpha = Mathf.Clamp01(1f - ((elapsed - fadeStartTime) / fadeDuration));
            }

            if (elapsed >= lifetimeSeconds)
            {
                Destroy(gameObject);
            }
        }
    }
}
