using System;
using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// Holds the player-chosen hairstyle and hair color and keeps the matching model attached to
    /// the head socket of the character. Attach it to the root of a character model next to
    /// <c>FaceCustomization</c>. Hair models are authored with their origin on the head socket, and
    /// each hairstyle stores a fine adjustment per body type that is applied on top of it.
    /// </summary>
    public sealed class HairCustomization : MonoBehaviour
    {
        /// <summary>
        /// Style index that means no hair.
        /// </summary>
        public const int NoHair = -1;

        private const string InstanceName = "Hair";

        [Tooltip("Head bone the hair is attached to. When empty, the first child named Socket Name is used.")]
        [SerializeField] private Transform socket;

        [SerializeField] private string socketName = "Socket_HeadTop";

        [Tooltip("Names and models of the selectable hairstyles.")]
        [SerializeField] private HairCatalog catalog;

        [SerializeField] private int styleIndex;

        [Tooltip("Body model the hair is worn on. Selects which stored placement is applied.")]
        [SerializeField] private HairBodyType bodyType = HairBodyType.Male;

        [SerializeField] private HairColor color = HairColor.Brown;

        private readonly HairTintWriter tintWriter = new HairTintWriter();

        /// <summary>
        /// Raised after the look changed and the hair model was rebuilt.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Index of the chosen hairstyle in the catalog, or <see cref="NoHair"/>.
        /// </summary>
        public int StyleIndex => styleIndex;

        /// <summary>
        /// Body model the hair is worn on.
        /// </summary>
        public HairBodyType BodyType => bodyType;

        /// <summary>
        /// Chosen hair color preset.
        /// </summary>
        public HairColor ColorChoice => color;

        /// <summary>
        /// Names and models of the selectable hairstyles, or null when no catalog is assigned.
        /// </summary>
        public HairCatalog Catalog => catalog;

        /// <summary>
        /// Sets which catalog and which body's placements this component uses, for components that
        /// are added to a model at runtime and so cannot be configured in the inspector.
        /// </summary>
        /// <param name="newCatalog">The hairstyle catalog.</param>
        /// <param name="newBodyType">The body model the hair is worn on.</param>
        public void Configure(HairCatalog newCatalog, HairBodyType newBodyType)
        {
            catalog = newCatalog;
            bodyType = newBodyType;
        }

        /// <summary>
        /// Chooses a new look and applies it. This is the entry point for a future character
        /// creation screen.
        /// </summary>
        /// <param name="style">Index of the hairstyle in the catalog, or <see cref="NoHair"/>.</param>
        /// <param name="hairColor">Hair color.</param>
        public void Apply(int style, HairColor hairColor)
        {
            styleIndex = style;
            color = hairColor;
            Rebuild();
        }

        /// <summary>
        /// Removes the current hair model and attaches the one that matches the chosen style,
        /// tinted with the chosen color. Also used by the inspector to preview changes.
        /// </summary>
        public void Rebuild()
        {
            Transform target = HairSocketLocator.Find(this, socket, socketName);
            if (target == null)
            {
                return;
            }

            RemoveExisting(target);

            if (catalog != null && catalog.TryGet(styleIndex, out HairStyleEntry entry))
            {
                GameObject instance = Instantiate(entry.Prefab, target, false);
                instance.name = InstanceName;
                entry.GetPlacement(bodyType).ApplyTo(instance.transform);
                tintWriter.Apply(instance, HairColorPalette.Resolve(color));
            }

            Changed?.Invoke();
        }

        /// <summary>
        /// Finds the hair model currently attached to the socket.
        /// </summary>
        /// <returns>The hair model, or null when there is none.</returns>
        public Transform FindInstance()
        {
            Transform target = HairSocketLocator.Find(this, socket, socketName);
            return target == null ? null : target.Find(InstanceName);
        }

        private void Start()
        {
            Rebuild();
        }

        private static void RemoveExisting(Transform target)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                Transform child = target.GetChild(i);
                if (child.name != InstanceName)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
