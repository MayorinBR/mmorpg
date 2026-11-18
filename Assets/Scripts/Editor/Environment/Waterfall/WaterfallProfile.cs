using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Shape and flow parameters of the Map 0,0 waterfall. The mouth opens in a wall that faces south (-Z) and the
    /// water falls toward the river basin in front of it.
    /// </summary>
    [CreateAssetMenu(menuName = "Environment/Waterfall Profile", fileName = "WaterfallProfile")]
    public sealed class WaterfallProfile : ScriptableObject
    {
        [SerializeField] private float centerX = -45f;
        [SerializeField] private float sillHeight = 116f;
        [SerializeField] private float topHeight = 128f;
        [SerializeField] private float mouthHalfWidth = 5.2f;
        [SerializeField] private float caveDepth = 5f;
        [SerializeField] private float caveTaper = 0.72f;
        [SerializeField] private float caveFlowSpeed = 1.3f;
        [SerializeField] private float rimMinWidth = 0.9f;
        [SerializeField] private float rimMaxWidth = 2.4f;
        [SerializeField] private float rimProtrusion = 0.9f;
        [SerializeField] private float sheetWidthAtLip = 5f;
        [SerializeField] private float sheetWidthAtBase = 8.2f;
        [SerializeField] private float landingZ = 80.5f;
        [SerializeField] private int seed = 7;

        /// <summary>World X of the mouth center and of the whole fall.</summary>
        public float CenterX => centerX;

        /// <summary>World height of the mouth floor, where the water leaves the rock.</summary>
        public float SillHeight => sillHeight;

        /// <summary>World height of the top of the mouth.</summary>
        public float TopHeight => topHeight;

        /// <summary>Half of the widest opening of the mouth, in meters.</summary>
        public float MouthHalfWidth => mouthHalfWidth;

        /// <summary>How far the cave extends into the mountain, in meters.</summary>
        public float CaveDepth => caveDepth;

        /// <summary>Scale of the cave at its back relative to the mouth, from 0 to 1.</summary>
        public float CaveTaper => caveTaper;

        /// <summary>Speed of the water inside the cave, in meters per second.</summary>
        public float CaveFlowSpeed => caveFlowSpeed;

        /// <summary>Smallest width of the rock collar around the mouth, in meters.</summary>
        public float RimMinWidth => rimMinWidth;

        /// <summary>Largest width of the rock collar around the mouth, in meters.</summary>
        public float RimMaxWidth => rimMaxWidth;

        /// <summary>How far the inner edge of the collar sticks out of the wall, in meters.</summary>
        public float RimProtrusion => rimProtrusion;

        /// <summary>Width of the water sheet where it leaves the mouth, in meters.</summary>
        public float SheetWidthAtLip => sheetWidthAtLip;

        /// <summary>Width of the water sheet where it hits the river, in meters.</summary>
        public float SheetWidthAtBase => sheetWidthAtBase;

        /// <summary>World Z where the falling water lands in the river. It sets the speed of the water leaving the lip.</summary>
        public float LandingZ => landingZ;

        /// <summary>Seed of the random irregularities of the mouth.</summary>
        public int Seed => seed;
    }
}
