using UnityEngine;

namespace Project.NPC
{
    /// <summary>
    /// An NPC's opening dialogue line and its list of clickable response
    /// options. Authored as an asset so the same line/option data can be
    /// reused or tuned without touching a scene, the same way
    /// <see cref="Project.Character.Stats.CharacterStatsDefinition"/> is
    /// authored separately from the monster instance that reads it. A
    /// portrait is opt-in — <see cref="Portrait"/> being null is a normal,
    /// fully-supported case, not a missing-data error.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNpcDialogue", menuName = "Project/NPC/Dialogue")]
    public class NpcDialogueDefinition : ScriptableObject
    {
        [TextArea(2, 5)]
        [SerializeField] private string line;

        [Tooltip("Optional. Shown next to the dialogue line when set; the dialogue box works the same either way when left empty.")]
        [SerializeField] private Sprite portrait;

        [SerializeField] private NpcDialogueOption[] options;

        /// <summary>Gets the NPC's line of dialogue.</summary>
        public string Line => line;

        /// <summary>Gets the NPC's portrait, or null if this NPC has none.</summary>
        public Sprite Portrait => portrait;

        /// <summary>Gets the response options shown below the line.</summary>
        public NpcDialogueOption[] Options => options;
    }
}
