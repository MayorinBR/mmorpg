using UnityEngine;

namespace Mmo.EditorTools.EnvAssets
{
    /// <summary>Geometry builder for wooden footbridges.</summary>
    public static class EnvBridgeShapes
    {
        private const float DeckTop = 0.12f;
        private const float PlankThickness = 0.14f;
        private const float PlankPitch = 0.42f;
        private const float PlankGap = 0.03f;
        private const float StringerHeight = 0.28f;
        private const float StringerWidth = 0.22f;
        private const float PylonSize = 0.34f;
        private const float PylonBottom = -3f;
        private const float PylonSpacing = 4.5f;
        private const float PostSize = 0.14f;
        private const float PostSpacing = 2.25f;
        private const float RailHeight = 1.15f;
        private const float RailThickness = 0.09f;

        /// <summary>Builds a straight bridge along the X axis, centered on the origin, whose deck top is slightly above y = 0.</summary>
        /// <param name="m">Destination mesh.</param>
        /// <param name="deck">Light wood submesh used for planks and rails.</param>
        /// <param name="frame">Dark wood submesh used for stringers, pylons and posts.</param>
        /// <param name="length">Length of the deck.</param>
        /// <param name="width">Width of the deck.</param>
        public static void Footbridge(EnvMesh m, int deck, int frame, float length, float width)
        {
            float half = length * 0.5f;
            float plankY = DeckTop - (PlankThickness * 0.5f);
            int planks = Mathf.FloorToInt(length / PlankPitch);
            for (int i = 0; i < planks; i++)
            {
                float x = -half + (PlankPitch * (i + 0.5f));
                EnvShapes.Box(m, deck, new Vector3(x, plankY, 0f), new Vector3(PlankPitch - PlankGap, PlankThickness, width), Quaternion.identity, 1f, false);
            }

            float stringerY = DeckTop - PlankThickness - (StringerHeight * 0.5f);
            float edge = (width * 0.5f) - 0.3f;
            foreach (float side in new[] { -edge, edge })
            {
                EnvShapes.Box(m, frame, new Vector3(0f, stringerY, side), new Vector3(length, StringerHeight, StringerWidth), Quaternion.identity, 1f, false);
            }

            BuildPylons(m, frame, half, edge);
            BuildRails(m, deck, frame, half, (width * 0.5f) - 0.1f);
        }

        private static void BuildPylons(EnvMesh m, int frame, float half, float edge)
        {
            int count = Mathf.Max(2, Mathf.RoundToInt((half * 2f) / PylonSpacing) + 1);
            float top = DeckTop - PlankThickness;
            float height = top - PylonBottom;
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(-half + 0.6f, half - 0.6f, i / (float)(count - 1));
                foreach (float side in new[] { -edge, edge })
                {
                    EnvShapes.Box(m, frame, new Vector3(x, PylonBottom + (height * 0.5f), side), new Vector3(PylonSize, height, PylonSize), Quaternion.identity, 1f, false);
                }
            }
        }

        private static void BuildRails(EnvMesh m, int deck, int frame, float half, float railZ)
        {
            int posts = Mathf.Max(2, Mathf.RoundToInt((half * 2f) / PostSpacing) + 1);
            foreach (float side in new[] { -railZ, railZ })
            {
                for (int i = 0; i < posts; i++)
                {
                    float x = Mathf.Lerp(-half + 0.1f, half - 0.1f, i / (float)(posts - 1));
                    EnvShapes.Box(m, frame, new Vector3(x, DeckTop + (RailHeight * 0.5f), side), new Vector3(PostSize, RailHeight, PostSize), Quaternion.identity, 1f, false);
                }

                EnvShapes.Box(m, deck, new Vector3(0f, DeckTop + RailHeight, side), new Vector3(half * 2f, RailThickness, RailThickness * 1.4f), Quaternion.identity, 1f, false);
                EnvShapes.Box(m, deck, new Vector3(0f, DeckTop + (RailHeight * 0.55f), side), new Vector3(half * 2f, RailThickness * 0.8f, RailThickness), Quaternion.identity, 1f, false);
            }
        }
    }

    /// <summary>Wooden bridges for rivers and streams, saved under Field/Bridges.</summary>
    public sealed class BridgeCatalog : EnvCatalogBase
    {
        private const string Area = "Field";
        private const string Type = "Bridges";

        /// <inheritdoc />
        protected override void Register()
        {
            Add(Area, Type, "Bridge_01", new[] { "wood_planks", "wood_dark" }, m => EnvBridgeShapes.Footbridge(m, 0, 1, 18f, 3.2f), false, true);
            Add(Area, Type, "Bridge_02", new[] { "wood_planks", "wood_dark" }, m => EnvBridgeShapes.Footbridge(m, 0, 1, 12f, 3f), false, true);
        }
    }
}
