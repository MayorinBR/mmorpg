using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Tints every renderer under a model root with a solid color while
    /// active, blended over each renderer's own original color — e.g. a
    /// red mask while a boss is enraged, or a green mask while a character
    /// is poisoned. Applies the tint through a <see cref="MaterialPropertyBlock"/>,
    /// so the shared material asset itself is never modified and every
    /// instance can tint independently. Renderers are looked up fresh each
    /// time the tint turns on rather than cached once, since a model root
    /// like the player's gender-specific geometry is only populated at
    /// runtime (see <c>Project.Character.Combat.PlayerGenderController</c>).
    /// If <see cref="poisonSource"/> is wired, the tint automatically
    /// tracks its <see cref="StatusEffectController.IsPoisoned"/> state;
    /// otherwise call <see cref="SetActive"/> directly (e.g. a boss driving
    /// its own Rage-skill mask from <see cref="Project.AI.BossSkillController"/>).
    /// </summary>
    public class StatusTintController : MonoBehaviour
    {
        [Tooltip("Root to search for renderers under. Left empty, searches this GameObject and its children instead.")]
        [SerializeField] private Transform modelRoot;

        [SerializeField] private Color tintColor = Color.red;

        [Tooltip("How strongly the tint replaces each renderer's original color (0 = no visible change, 1 = fully replaced).")]
        [SerializeField, Range(0f, 1f)] private float tintStrength = 0.6f;

        [Tooltip("The shader color property this tint writes to. Standard URP Lit/Simple Lit materials use \"_BaseColor\" (the default); a custom Shader Graph material may expose a differently named property instead (e.g. this project's slime materials use \"_Base_Color\").")]
        [SerializeField] private string colorPropertyName = "_BaseColor";

        [Tooltip("Optional. When wired, the tint automatically follows this character's IsPoisoned state instead of being driven manually via SetActive.")]
        [SerializeField] private StatusEffectController poisonSource;

        private MaterialPropertyBlock propertyBlock;
        private Renderer[] renderers;
        private Color[] originalColors;
        private int colorPropertyId;
        private bool isActive;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            colorPropertyId = Shader.PropertyToID(colorPropertyName);
        }

        private void Update()
        {
            if (poisonSource != null)
            {
                SetActive(poisonSource.IsPoisoned);
            }
        }

        /// <summary>Turns the tint on or off. No-ops if already in the requested state.</summary>
        /// <param name="active">True to show the tint, false to restore each renderer's original color.</param>
        public void SetActive(bool active)
        {
            if (active == isActive)
            {
                return;
            }

            isActive = active;

            if (active)
            {
                CaptureRenderers();
            }

            if (renderers == null)
            {
                return;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                var targetRenderer = renderers[i];

                if (targetRenderer == null)
                {
                    continue;
                }

                var color = active ? Color.Lerp(originalColors[i], tintColor, tintStrength) : originalColors[i];
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(colorPropertyId, color);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void CaptureRenderers()
        {
            var searchRoot = modelRoot != null ? modelRoot : transform;
            renderers = searchRoot.GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];

            for (var i = 0; i < renderers.Length; i++)
            {
                var sharedMaterial = renderers[i] != null ? renderers[i].sharedMaterial : null;
                originalColors[i] = sharedMaterial != null && sharedMaterial.HasColor(colorPropertyId)
                    ? sharedMaterial.GetColor(colorPropertyId)
                    : Color.white;
            }
        }
    }
}
