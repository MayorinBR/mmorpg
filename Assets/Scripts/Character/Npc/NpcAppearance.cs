using System.Collections.Generic;
using UnityEngine;
using Project.Character.Combat;
using Project.Character.Head;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Npc
{
    /// <summary>
    /// Look of an NPC built on a Male_Body or Female_Body prefab variant: a head shape and a list of
    /// clothing items. Clothes are ordinary skinned equipment items, so they reuse the item prefabs
    /// and hide the body regions they cover. The look is applied when the NPC starts, and the
    /// inspector applies it in the editor as a preview. Hair is configured on its own component.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HeadCustomization))]
    public sealed class NpcAppearance : MonoBehaviour
    {
        private const string InstancePrefix = "Outfit_";

        [Tooltip("Body of this NPC. Selects the head models and the gender specific item models.")]
        [SerializeField] private CharacterGender gender;

        [SerializeField] private HeadCatalog headCatalog;

        [SerializeField] private int headStyle;

        [Tooltip("Skinned equipment items worn by the NPC, in any slot.")]
        [SerializeField] private List<ItemDefinition> outfit = new List<ItemDefinition>();

        [Tooltip("Where a covered torso is cut under the neck, from the upper chest bone (0) to the neck bone (1).")]
        [SerializeField, Range(0f, 1f)] private float neckClipHeight = 0.3f;

        /// <summary>Gets the catalog of selectable head shapes.</summary>
        public HeadCatalog HeadCatalog => headCatalog;

        private void Start()
        {
            Rebuild();
        }

        /// <summary>
        /// Applies the head shape and the outfit to the character.
        /// </summary>
        public void Rebuild()
        {
            var animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning($"{name} has no Animator, so its appearance cannot be applied.", this);
                return;
            }

            var head = GetComponent<HeadCustomization>();
            head.Configure(headCatalog, gender == CharacterGender.Female);
            head.Apply(headStyle);

            var covered = ApplyOutfit(animator.transform);
            BodyVisibility.Apply(animator, covered, neckClipHeight);
        }

        private BodyPart ApplyOutfit(Transform root)
        {
            ClearOutfit(root);

            var bones = SkinnedEquipmentBinder.IndexBones(root);
            var covered = BodyPart.None;

            foreach (var item in outfit)
            {
                if (item == null)
                {
                    continue;
                }

                var model = item.GetEquippedModel(gender);

                if (model == null || !model.TryGetComponent(out SkinnedEquipment skinned))
                {
                    Debug.LogWarning($"{item.name} is not a skinned piece of equipment and is ignored.", this);
                    continue;
                }

                var instance = Instantiate(model, root, false);
                instance.name = InstancePrefix + item.name;

                if (!SkinnedEquipmentBinder.Bind(instance, bones))
                {
                    Debug.LogWarning($"{item.name} does not match the skeleton of {name}.", this);
                    DestroyNow(instance);
                    continue;
                }

                MarkAsPreview(instance);
                covered |= skinned.CoveredParts;
            }

            return covered;
        }

        private static void ClearOutfit(Transform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);

                if (child.name.StartsWith(InstancePrefix))
                {
                    DestroyNow(child.gameObject);
                }
            }
        }

        private static void MarkAsPreview(GameObject instance)
        {
            if (Application.isPlaying)
            {
                return;
            }

            foreach (var child in instance.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.hideFlags = HideFlags.DontSave;
            }
        }

        private static void DestroyNow(GameObject target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
