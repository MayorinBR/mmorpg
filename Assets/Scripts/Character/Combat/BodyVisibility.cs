using System.Collections.Generic;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Shows or hides the body renderers of a character according to the regions covered by worn
    /// skinned equipment. A covered torso keeps only its neck.
    /// </summary>
    public static class BodyVisibility
    {
        /// <summary>
        /// Hides the body meshes covered by worn armor and shows the rest.
        /// </summary>
        /// <param name="animator">Animator of the character that owns the body renderers.</param>
        /// <param name="covered">The body regions covered by the worn equipment.</param>
        /// <param name="neckClipHeight">Where the torso is cut under the neck, from the upper chest bone (0) to the neck bone (1).</param>
        public static void Apply(Animator animator, BodyPart covered, float neckClipHeight)
        {
            var hidden = new HashSet<string>(BodyPartNames.Resolve(covered));

            foreach (var renderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!BodyPartNames.IsBodyMesh(renderer.name))
                {
                    continue;
                }

                var isHidden = hidden.Contains(renderer.name);
                var clip = renderer.GetComponent<TorsoNeckClip>();

                if (isHidden && renderer.name == BodyPartNames.TorsoMeshName && TryGetNeckBones(animator, out var lower, out var upper))
                {
                    if (clip == null)
                    {
                        clip = renderer.gameObject.AddComponent<TorsoNeckClip>();
                    }

                    clip.Enable(lower, upper, neckClipHeight);
                    renderer.enabled = true;
                    continue;
                }

                if (clip != null)
                {
                    clip.Disable();
                }

                renderer.enabled = !isHidden;
            }
        }

        private static bool TryGetNeckBones(Animator animator, out Transform lower, out Transform upper)
        {
            upper = animator.GetBoneTransform(HumanBodyBones.Neck);
            lower = animator.GetBoneTransform(HumanBodyBones.UpperChest);

            if (lower == null)
            {
                lower = animator.GetBoneTransform(HumanBodyBones.Chest);
            }

            return lower != null && upper != null;
        }
    }
}
