using System.Collections.Generic;
using UnityEngine;
using Project.Character.Hair;
using Project.Character.Stats;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Shows the 3D model of every equipped item on the player's current character model, parented
    /// to the matching bone of its Humanoid skeleton (see <see cref="EquipmentAnchorResolver"/>).
    /// Skinned pieces (see <see cref="SkinnedEquipment"/>) are bound to the character skeleton instead and hide the
    /// body regions they cover. The models are rebuilt whenever the equipment or the character model changes.
    /// </summary>
    public sealed class EquipmentVisualController : MonoBehaviour
    {
        private static readonly EquipmentSlot[] VisibleSlots =
        {
            EquipmentSlot.LeftHand,
            EquipmentSlot.RightHand,
            EquipmentSlot.HeadTop,
            EquipmentSlot.HeadFace,
            EquipmentSlot.Body,
            EquipmentSlot.Pants,
            EquipmentSlot.Boots,
            EquipmentSlot.Gloves
        };

        private static readonly Quaternion MainHandDaggerGrip = Quaternion.Euler(355.73f, 59.58f, 198.5f);
        private static readonly Quaternion OffHandDaggerGrip = Quaternion.Euler(353.99f, 299.64f, 171.38f);

        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private PlayerGenderController genderController;

        [Tooltip("Optional. When assigned, weapons are only shown while the player is in combat. Left empty, weapons are always shown.")]
        [SerializeField] private PlayerCombatState combatState;

        [Tooltip("Where the torso is cut under the neck, from the upper chest bone (0) to the neck bone (1).")]
        [SerializeField, Range(0f, 1f)] private float neckClipHeight = 0.3f;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly List<GameObject> weapons = new List<GameObject>();
        private HairCustomization observedHair;
        private bool hairHidden;
        private BodyPart coveredParts;

        /// <summary>
        /// The model of the equipped main-hand weapon, or null while the hand is empty or the models are being rebuilt.
        /// </summary>
        public Transform MainHandWeapon { get; private set; }

        /// <summary>
        /// The Animator of the character model the equipment is attached to, or null when there is no model.
        /// </summary>
        public Animator CurrentAnimator { get; private set; }

        private void OnEnable()
        {
            equipment.EquipmentChanged += Rebuild;
            genderController.GenderChanged += HandleGenderChanged;

            if (combatState != null)
            {
                combatState.Changed += HandleCombatChanged;
            }
        }

        private void OnDisable()
        {
            equipment.EquipmentChanged -= Rebuild;
            genderController.GenderChanged -= HandleGenderChanged;

            if (combatState != null)
            {
                combatState.Changed -= HandleCombatChanged;
            }

            ObserveHair(null);
        }

        private void Start()
        {
            Rebuild();
        }

        private void HandleCombatChanged(bool inCombat)
        {
            ApplyWeaponVisibility();
        }

        private void ApplyWeaponVisibility()
        {
            var visible = combatState == null || combatState.IsInCombat;

            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.SetActive(visible);
                }
            }
        }

        private void HandleGenderChanged(CharacterGender gender)
        {
            Rebuild();
        }

        private void Rebuild()
        {
            DestroySpawned();
            hairHidden = false;
            coveredParts = BodyPart.None;
            MainHandWeapon = null;
            CurrentAnimator = null;

            var geometry = genderController.CurrentGeometry;

            if (geometry == null)
            {
                ObserveHair(null);
                return;
            }

            var animator = geometry.GetComponentInChildren<Animator>();
            CurrentAnimator = animator;

            if (animator != null)
            {
                foreach (var slot in VisibleSlots)
                {
                    AttachSlot(animator, slot);
                }

                BodyVisibility.Apply(animator, coveredParts, neckClipHeight);
            }

            ObserveHair(geometry.GetComponentInChildren<HairCustomization>());
            ApplyHairVisibility();
            ApplyWeaponVisibility();
        }

        private void AttachSlot(Animator animator, EquipmentSlot slot)
        {
            foreach (var item in equipment.GetEquippedItems(slot))
            {
                if (IsSecondHalfOfTwoHandedItem(slot, item))
                {
                    continue;
                }

                var model = item.GetEquippedModel(genderController.CurrentGender);

                if (model != null && model.TryGetComponent(out SkinnedEquipment skinned))
                {
                    AttachSkinned(animator.transform, slot, item, model, skinned);
                    continue;
                }

                if (model == null || !EquipmentAnchorResolver.TryResolve(slot, item.WeaponSubtype, out var boneId))
                {
                    continue;
                }

                var bone = FindBone(animator, boneId);

                if (bone == null)
                {
                    Debug.LogWarning($"{animator.name} has no {boneId} bone, so {item.name} cannot be shown.", animator);
                    continue;
                }

                var instance = Instantiate(model, ResolveAttachPoint(bone, item), false);
                instance.name = $"Equipped_{slot}_{item.name}";
                ApplyDaggerGrip(instance.transform, slot, item);
                spawned.Add(instance);

                if (item.WeaponSubtype != WeaponSubtype.Unarmed)
                {
                    weapons.Add(instance);

                    if (slot == EquipmentSlot.LeftHand)
                    {
                        MainHandWeapon = instance.transform;
                    }
                }

                if (item.HidesHair)
                {
                    hairHidden = true;
                }
            }
        }

        private void AttachSkinned(Transform characterRoot, EquipmentSlot slot, ItemDefinition item, GameObject model, SkinnedEquipment skinned)
        {
            var bones = SkinnedEquipmentBinder.IndexBones(characterRoot);
            var instance = Instantiate(model, characterRoot, false);
            instance.name = $"Equipped_{slot}_{item.name}";

            if (!SkinnedEquipmentBinder.Bind(instance, bones))
            {
                Debug.LogWarning($"{item.name} does not match the skeleton of {characterRoot.name}, so it cannot be shown.", characterRoot);
                Destroy(instance);
                return;
            }

            spawned.Add(instance);
            coveredParts |= skinned.CoveredParts;

            if (item.HidesHair)
            {
                hairHidden = true;
            }
        }

        private static bool IsSecondHalfOfTwoHandedItem(EquipmentSlot slot, ItemDefinition item)
        {
            if (slot != EquipmentSlot.RightHand)
            {
                return false;
            }

            var usesMainHand = false;

            foreach (var required in item.RequiredSlots)
            {
                usesMainHand |= required == EquipmentSlot.LeftHand;
            }

            return usesMainHand && item.RequiredSlots.Count > 1;
        }

        /// <summary>
        /// Turns a dagger in the palm socket so that, with the arms hanging, the main-hand blade points forward and
        /// the off-hand blade points backward. Other items keep the rotation of their prefab.
        /// </summary>
        /// <param name="model">The spawned item model, already parented to its socket.</param>
        /// <param name="slot">The slot the item occupies.</param>
        /// <param name="item">The item being attached.</param>
        private static void ApplyDaggerGrip(Transform model, EquipmentSlot slot, ItemDefinition item)
        {
            if (item.WeaponSubtype != WeaponSubtype.Dagger)
            {
                return;
            }

            model.localRotation = slot == EquipmentSlot.RightHand ? OffHandDaggerGrip : MainHandDaggerGrip;
        }

        /// <summary>
        /// Returns the transform a model is parented to: the palm socket of the hand bone for daggers,
        /// so the grip sits in the palm, or the bone itself for every other item.
        /// </summary>
        /// <param name="bone">The bone resolved for the item's slot.</param>
        /// <param name="item">The item being attached.</param>
        /// <returns>The socket child of the bone when one applies, otherwise the bone.</returns>
        private static Transform ResolveAttachPoint(Transform bone, ItemDefinition item)
        {
            if (item.WeaponSubtype != WeaponSubtype.Dagger)
            {
                return bone;
            }

            foreach (Transform child in bone)
            {
                if (child.name.StartsWith("Socket_Hand"))
                {
                    return child;
                }
            }

            return bone;
        }

        private static Transform FindBone(Animator animator, HumanBodyBones bone)
        {
            var found = animator.GetBoneTransform(bone);

            if (found == null && bone == HumanBodyBones.UpperChest)
            {
                found = animator.GetBoneTransform(HumanBodyBones.Chest);
            }

            return found;
        }

        private void ObserveHair(HairCustomization hair)
        {
            if (observedHair == hair)
            {
                return;
            }

            if (observedHair != null)
            {
                observedHair.Changed -= ApplyHairVisibility;
            }

            observedHair = hair;

            if (observedHair != null)
            {
                observedHair.Changed += ApplyHairVisibility;
            }
        }

        private void ApplyHairVisibility()
        {
            if (observedHair == null)
            {
                return;
            }

            var current = observedHair.FindInstance();

            if (current == null)
            {
                return;
            }

            // HairCustomization destroys the previous model at the end of the frame, so right after
            // a rebuild both the old and the new model sit under the socket; every one is toggled.
            var socket = current.parent;

            for (int i = 0; i < socket.childCount; i++)
            {
                var child = socket.GetChild(i);

                if (child.name == current.name)
                {
                    child.gameObject.SetActive(!hairHidden);
                }
            }
        }

        private void DestroySpawned()
        {
            foreach (var instance in spawned)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }

            spawned.Clear();
            weapons.Clear();
        }
    }
}
