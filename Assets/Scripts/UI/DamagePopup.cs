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

        [SerializeField] private float lifetimeSeconds = 1f;
        [SerializeField] private float riseSpeed = 1.5f;
        [SerializeField, Range(0f, 1f)] private float fadeStartFraction = 0.5f;

        private CanvasGroup canvasGroup;
        private Camera viewCamera;
        private float elapsed;

        /// <summary>
        /// Builds a new floating damage number at the given world position
        /// and starts its rise/fade lifecycle immediately.
        /// </summary>
        /// <param name="amount">The damage amount to display.</param>
        /// <param name="worldPosition">Where to spawn the popup.</param>
        /// <returns>The spawned popup's component.</returns>
        public static DamagePopup Create(int amount, Vector3 worldPosition)
        {
            var root = new GameObject("DamagePopup", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.position = worldPosition;
            rootRect.sizeDelta = new Vector2(BackgroundWidth, BackgroundHeight);
            rootRect.localScale = Vector3.one * CanvasScale;

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>();
            var canvasGroup = root.AddComponent<CanvasGroup>();

            // Placeholder background so the white number stays readable
            // against any environment. Swap this color/sprite for real art
            // later without touching the popup's rise/fade behavior.
            var background = new GameObject("Background", typeof(Image));
            background.transform.SetParent(root.transform, false);
            background.GetComponent<Image>().color = Color.red;
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;

            var textObject = new GameObject("AmountText", typeof(TextMeshProUGUI));
            textObject.transform.SetParent(root.transform, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = amount.ToString();
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
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
