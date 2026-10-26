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
            EquipmentSlot.Ammo,
            EquipmentSlot.Body,
            EquipmentSlot.Pants,
            EquipmentSlot.Boots,
            EquipmentSlot.Gloves
        };

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

            var geometry = genderController.CurrentGeometry;

            if (geometry == null)
            {
                ObserveHair(null);
                return;
            }

            var animator = geometry.GetComponentInChildren<Animator>();

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

                var instance = Instantiate(model, bone, false);
                instance.name = $"Equipped_{slot}_{item.name}";
                spawned.Add(instance);

                if (item.WeaponSubtype != WeaponSubtype.Unarmed)
                {
                    weapons.Add(instance);
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

            foreach (var required in item.RequiredSlots)
            {
                if (required == EquipmentSlot.LeftHand)
                {
                    return true;
                }
            }

            return false;
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
