using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Turns a <see cref="BuffPayload"/> into the readable stat lines shown in the effect tooltip.
    /// </summary>
    public static class BuffPayloadDescription
    {
        /// <summary>
        /// Describes every non-zero component of a payload, one line each.
        /// </summary>
        /// <param name="payload">The payload to describe.</param>
        /// <returns>The lines joined by new lines, or an empty string for an empty payload.</returns>
        public static string Describe(BuffPayload payload)
        {
            var lines = new List<string>();

            AddPercent(lines, "ATK", payload.AtkPercent);
            AddPercent(lines, "DEF", payload.DefPercent);
            AddFlat(lines, "MDEF", payload.MdefFlat);
            AddPercent(lines, "ASPD", payload.AspdPercent);
            AddFlat(lines, "Max HP", payload.MaxHealthFlat);
            AddFlat(lines, "STR", payload.Strength);
            AddFlat(lines, "AGI", payload.Agility);
            AddFlat(lines, "VIT", payload.Vitality);
            AddFlat(lines, "INT", payload.Intelligence);
            AddFlat(lines, "DEX", payload.Dexterity);
            AddFlat(lines, "LUK", payload.Luck);
            AddPercent(lines, "Fire damage", payload.FireDamagePercent);

            if (payload.IncomingDamageReductionPercent != 0f)
            {
                lines.Add($"Damage taken -{Mathf.RoundToInt(payload.IncomingDamageReductionPercent * 100f)}%");
            }

            if (payload.PreventsFlinch)
            {
                lines.Add("Cannot be staggered by hits");
            }

            return string.Join("\n", lines);
        }

        private static void AddPercent(List<string> lines, string label, float value)
        {
            var percent = Mathf.RoundToInt(value * 100f);

            if (percent != 0)
            {
                lines.Add($"{label} {Sign(percent)}{Mathf.Abs(percent)}%");
            }
        }

        private static void AddFlat(List<string> lines, string label, int value)
        {
            if (value != 0)
            {
                lines.Add($"{label} {Sign(value)}{Mathf.Abs(value)}");
            }
        }

        private static string Sign(int value)
        {
            return value > 0 ? "+" : "-";
        }
    }
}
