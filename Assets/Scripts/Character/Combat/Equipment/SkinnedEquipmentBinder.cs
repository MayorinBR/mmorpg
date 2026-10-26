using System.Collections.Generic;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Attaches the skinned meshes of an equipment prefab instance to the skeleton of a character,
    /// matching bones by name, so the equipment deforms with the character animation.
    /// </summary>
    public static class SkinnedEquipmentBinder
    {
        private const string FallbackRootBone = "Hips";

        /// <summary>
        /// Indexes the bones of a character by name. Renderers and any transform that belongs to an
        /// equipment instance are ignored, so the result only holds the character skeleton.
        /// </summary>
        /// <param name="characterRoot">The root of the character model.</param>
        /// <returns>The bones of the character, keyed by name.</returns>
        public static Dictionary<string, Transform> IndexBones(Transform characterRoot)
        {
            var bones = new Dictionary<string, Transform>();

            foreach (var candidate in characterRoot.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.GetComponent<Renderer>() == null && candidate.GetComponentInParent<SkinnedEquipment>() == null)
                {
                    bones.TryAdd(candidate.name, candidate);
                }
            }

            return bones;
        }

        /// <summary>
        /// Rebinds every skinned renderer of an equipment instance to the character bones and removes
        /// the instance's own copy of the skeleton.
        /// </summary>
        /// <param name="instance">The spawned equipment prefab, already parented under the character.</param>
        /// <param name="bones">The character bones, from <see cref="IndexBones"/>.</param>
        /// <returns>True when every renderer was bound; false when a bone is missing, in which case nothing is changed.</returns>
        public static bool Bind(GameObject instance, IReadOnlyDictionary<string, Transform> bones)
        {
            var renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mapped = new List<(SkinnedMeshRenderer renderer, Transform[] bones, Transform root)>();

            foreach (var renderer in renderers)
            {
                if (!TryMap(renderer, bones, out var rendererBones, out var root))
                {
                    return false;
                }

                mapped.Add((renderer, rendererBones, root));
            }

            foreach (var (renderer, rendererBones, root) in mapped)
            {
                renderer.bones = rendererBones;
                renderer.rootBone = root;
                renderer.transform.SetParent(instance.transform, false);
            }

            RemoveImportedSkeleton(instance.transform);
            return true;
        }

        private static bool TryMap(SkinnedMeshRenderer renderer, IReadOnlyDictionary<string, Transform> bones, out Transform[] mapped, out Transform root)
        {
            var source = renderer.bones;
            mapped = new Transform[source.Length];
            root = null;

            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] == null || !bones.TryGetValue(source[i].name, out mapped[i]))
                {
                    return false;
                }
            }

            var rootName = renderer.rootBone != null ? renderer.rootBone.name : FallbackRootBone;
            return bones.TryGetValue(rootName, out root) || bones.TryGetValue(FallbackRootBone, out root);
        }

        private static void RemoveImportedSkeleton(Transform instanceRoot)
        {
            for (var i = instanceRoot.childCount - 1; i >= 0; i--)
            {
                var child = instanceRoot.GetChild(i);

                if (child.GetComponent<SkinnedMeshRenderer>() == null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(child.gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }
}
