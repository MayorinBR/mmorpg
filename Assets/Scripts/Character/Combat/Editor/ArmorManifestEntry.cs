using System;
using System.Collections.Generic;
using Project.Items;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// One armor item described in the armor manifest: where its models live, which body regions
    /// it covers and the placeholder values of its item asset.
    /// </summary>
    [Serializable]
    public sealed class ArmorManifestEntry
    {
        /// <summary>Name shared by the prefab and the item asset.</summary>
        public string assetName;

        /// <summary>Name shown in the game.</summary>
        public string displayName;

        /// <summary>Description shown in the game.</summary>
        public string description;

        /// <summary>Subfolder of the prefab and item folders, for example Armor or Boots.</summary>
        public string category;

        /// <summary>Name of an <see cref="EquipmentSlot"/> value.</summary>
        public string slot;

        /// <summary>Model paths relative to the equipment models folder, without extension.</summary>
        public List<string> models = new List<string>();

        /// <summary>Names of the <see cref="BodyPart"/> values hidden while the item is worn.</summary>
        public List<string> coveredParts = new List<string>();

        /// <summary>Optional material name shared by a set. Empty uses the material name found in the model.</summary>
        public string material;

        /// <summary>Vitality bonus.</summary>
        public int vitality;

        /// <summary>Agility bonus.</summary>
        public int agility;

        /// <summary>Item weight.</summary>
        public float weight;

        /// <summary>Shop buy price.</summary>
        public int buyPrice;

        /// <summary>Shop sell price.</summary>
        public int sellPrice;

        /// <summary>Drop chance in the Poring loot table, from 0 to 1.</summary>
        public float dropChance;

        /// <summary>
        /// Reads the equipment slot.
        /// </summary>
        /// <returns>The parsed slot, or <see cref="EquipmentSlot.Body"/> when the name is not valid.</returns>
        public EquipmentSlot ParseSlot()
        {
            return Enum.TryParse(slot, out EquipmentSlot parsed) ? parsed : EquipmentSlot.Body;
        }

        /// <summary>
        /// Combines the covered part names into a mask.
        /// </summary>
        /// <returns>The mask of every valid part name; invalid names are ignored.</returns>
        public BodyPart ParseCoveredParts()
        {
            var mask = BodyPart.None;

            foreach (var name in coveredParts)
            {
                if (Enum.TryParse(name, out BodyPart part))
                {
                    mask |= part;
                }
            }

            return mask;
        }

        /// <summary>
        /// Checks that the entry has everything the build needs.
        /// </summary>
        /// <param name="error">The reason the entry is invalid, or an empty string.</param>
        /// <returns>True when the entry is usable.</returns>
        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(assetName) || string.IsNullOrWhiteSpace(category) || models.Count == 0)
            {
                error = "assetName, category and at least one model are required";
                return false;
            }

            if (!Enum.TryParse(slot, out EquipmentSlot _))
            {
                error = $"unknown slot '{slot}'";
                return false;
            }

            foreach (var name in coveredParts)
            {
                if (!Enum.TryParse(name, out BodyPart _))
                {
                    error = $"unknown covered part '{name}'";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}
